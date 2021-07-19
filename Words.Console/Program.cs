namespace Words.Console
{
    using Mono.Terminal;
    using Newtonsoft.Json;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using LiteDB;
    using Raven.Client.Documents;
    using Raven.Client.Documents.Session;
    using System.Data;
    using Dapper;

    public static class Program
    {
        private const string EncodingFilename = @"C:\Programming\words.json";
        private const string DbFilename = @"C:\Programming\words.db";
        private const string WordsFilename = @"C:\Programming\Words\Words.Web\App_Data\words.txt";

        public static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0 && args[0] == "encode")
                {
                    var lines = File.ReadAllLines(WordsFilename, Encoding.UTF8);
                    var wordFinder = WordFinder.CreateTernary(lines, Language.Swedish);
                    var tree = wordFinder.EncodeSuccinct();
                    var json = JsonConvert.SerializeObject(tree.GetData(), Formatting.Indented);
                    File.WriteAllText(EncodingFilename, json);
                    if (File.Exists(DbFilename))
                    {
                        File.Delete(DbFilename);
                    }

                    using (var db = new DatabaseWrapper(DbFilename))
                    {
                        var orderedKeys = wordFinder.NormalizedToOriginal
                            .Where(x => x.Key != x.Value)
                            .Select(x => x.Key)
                            .OrderBy(x => x)
                            .ToArray();
                        db.NormalizedToOriginals.InsertBulk(
                            orderedKeys
                            .Select(x => new NormalizedToOriginal(x, wordFinder.NormalizedToOriginal[x])));

                        var permutations = wordFinder.Permutations
                            .Where(x => x.Value.Count > 1)
                            .ToArray();
                        db.WordPermutations.InsertBulk(
                            permutations
                                .Select(x => new WordPermutations(x.Key, x.Value.ToArray())));
                    }
                }
                else if (args.Length > 1 && args[0] == "migrate")
                {
                    // move all words into postgres
                    string password = args[1];
                    using (IDocumentStore documentStore = new DocumentStore
                    {
                        Urls = new[] { "http://localhost:8080" },
                        Database = "Krysshjalpen"
                    }.Initialize())
                    {
                        int start = 0;
                        while (true)
                        {
                            using (IDocumentSession session = documentStore.OpenSession())
                            using (IDbConnection connection = new Npgsql.NpgsqlConnection($"Host=localhost;Database=words;Username=prisma;Password={password};Include Error Detail=true"))
                            {
                                connection.Open();
                                IDbTransaction tran = connection.BeginTransaction();

                                Query[] queries = session.Advanced.LoadStartingWith<Query>(
                                    "queries/",
                                    start: start);
                                if (queries.Length == 0) break;
                                foreach (var query in queries)
                                {
                                    IMetadataDictionary metadata = session.Advanced.GetMetadataFor(query);

                                    string id = session.Advanced.GetDocumentId(query);
                                    if (int.TryParse(
                                        id.Replace("queries/", string.Empty),
                                        out int ravendbId) == false)
                                    {
                                        throw new Exception($"Failed to parse {id}");
                                    }

                                    var values = new
                                    {
                                        Type = query.Type ?? string.Empty,
                                        Text = query.Text.Substring(0, Math.Min(255, query.Text.Length)),
                                        ElapsedMilliseconds = (int)Math.Round(query.ElapsedMilliseconds),
                                        CreatedDate = session.Advanced.GetLastModifiedFor(query),
                                        RavenDbId = ravendbId
                                    };
                                    Console.WriteLine(values);
                                    connection.Execute(
                                        @"insert into query(type, text, elapsed_milliseconds, ravendb_id, created_date)
                                          values (@type, @text, @elapsedmilliseconds, @ravendbid, @createddate)
                                          on conflict(ravendb_id) do nothing",
                                        values);
                                }

                                tran.Commit();
                                start += queries.Length;
                            }
                        }
                    }
                }
                else if (args.Length > 1 && args[0] == "test-run")
                {
                    Run();
                }
                else
                {
                    Console.WriteLine("Usage: encode|migrate|test-run");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(AsyncFriendlyStackTrace.ExceptionExtensions.ToAsyncString(ex));
            }
        }

        private static void Run()
        {
            Console.Write("Constructing search trees...");
            var stopwatch = Stopwatch.StartNew();
            using (var db = new DatabaseWrapper(DbFilename))
            {
                var wrapper = db;
                var (ternary, succinct) = CreateWordFinders(
                    x => wrapper.WordPermutations.FindById(new BsonValue(x))?.Words ?? new string[0],
                    x => wrapper.NormalizedToOriginals.FindById(new BsonValue(x))?.Original);

                stopwatch.Stop();
                Console.WriteLine("{0} ms", stopwatch.ElapsedMilliseconds);

                Console.WriteLine("Enter word to search for. A single 'q' exits.");
                var lineEditor = new LineEditor("input");
                string input = lineEditor.Edit(": ", string.Empty);
                while (input != "q")
                {
                    do
                    {
                        if (string.IsNullOrWhiteSpace(input))
                            break;

                        foreach (var wordFinder in new[] { ternary, succinct }.Where(x => x != null))
                        {
                            stopwatch.Restart();
                            var matches = wordFinder.Matches(input, 2);
                            stopwatch.Stop();
                            if (matches.Count > 0)
                            {
                                Console.WriteLine(
                                    "{0} Found {1} words matching '{2}':",
                                    wordFinder.TreeType,
                                    matches.Count,
                                    input);
                                matches.ForEach(m => Console.WriteLine("{0,-7}: {1}", m.Type, m.Value));
                            }
                            else
                            {
                                Console.WriteLine("Did not find any words matching '{0}'", input);
                            }

                            Console.WriteLine("Search completed in {0:F2} ms", 1000.0 * stopwatch.ElapsedTicks / Stopwatch.Frequency);
                        }
                    }
                    while (false);

                    input = lineEditor.Edit(": ", string.Empty);
                }
            }
        }

        private static (WordFinder ternary, WordFinder succinct) CreateWordFinders(
            Func<string, string[]> getPermutations,
            Func<string, string> getOriginal)
        {
            //var wordFinder = new WordFinder(@"C:\Users\danlid\Dropbox\Programming\TernarySearchTree\english-word-list.txt", Encoding.UTF8, Language.English);
            //var wordFinder = new WordFinder(@"C:\Users\danlid\Dropbox\Programming\TernarySearchTree\swedish-english.txt", Encoding.UTF8, Language.Swedish);

            var lines = File.ReadAllLines(WordsFilename, Encoding.UTF8);
            var wordFinderTernary = WordFinder.CreateTernary(lines, Language.Swedish);

            var succinctTreeDataJson = File.ReadAllText(EncodingFilename);
            var succinctTreeData = JsonConvert.DeserializeObject<SuccinctTreeData>(succinctTreeDataJson);
            var wordFinderSuccinct = WordFinder.CreateSuccinct(
                succinctTreeData,
                Language.Swedish,
                getPermutations,
                getOriginal);
            return (wordFinderTernary, wordFinderSuccinct);
        }
    }
}
