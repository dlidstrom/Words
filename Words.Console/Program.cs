namespace Words.Console
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Dapper;
    using LiteDB;
    using Mono.Terminal;
    using Newtonsoft.Json;
    using Npgsql;
    using Raven.Client.Documents;
    using Raven.Client.Documents.Session;

    public static class Program
    {
        private const string NormalizedToOriginalsFilename = @"C:\Programming\normalized_to_originals.csv";
        private const string WordPermutationsFilename = @"C:\Programming\word_permutations.csv";
        private const string EncodingFilename = @"C:\Programming\Words\Words.Web\App_Data\words.json";
        private const string WordsFilename = @"C:\Programming\words.txt";

        public static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0 && args[0] == "encode")
                {
                    Console.WriteLine($"Reading {WordsFilename}");
                    string[] lines = File.ReadAllLines(WordsFilename, Encoding.UTF8);
                    Console.WriteLine("Creating ternary tree");
                    WordFinder wordFinder = WordFinder.CreateTernary(lines, Language.Swedish);
                    Console.WriteLine("Encoding succinct");
                    SuccinctTree tree = wordFinder.EncodeSuccinct();
                    string json = JsonConvert.SerializeObject(tree.GetData(), Formatting.Indented);
                    File.WriteAllText(EncodingFilename, json);
                    Console.WriteLine($"Created {EncodingFilename}");

                    IEnumerable<string> orderedKeys =
                        from item in wordFinder.NormalizedToOriginal
                        where item.Key != item.Value
                        orderby item.Key
                        select $"{item.Key},{item.Value}";
                    File.WriteAllLines(
                        NormalizedToOriginalsFilename,
                        new[] { "normalized,original" }.Concat(orderedKeys),
                        Encoding.UTF8);
                    Console.WriteLine($"Created {NormalizedToOriginalsFilename}");

                    IEnumerable<string> permutations =
                        from item in wordFinder.Permutations
                        where item.Value.Count > 1
                        orderby item.Key
                        from val in item.Value
                        orderby val
                        select $"{item.Key},{val}";
                    File.WriteAllLines(
                        WordPermutationsFilename,
                        new[] { "normalized,permutation" }.Concat(permutations),
                        Encoding.UTF8);
                    Console.WriteLine($"Created {WordPermutationsFilename}");
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
                            using (IDbConnection connection = new NpgsqlConnection($"Host=localhost;Database=words;Username=prisma;Password={password};Include Error Detail=true"))
                            {
                                connection.Open();
                                IDbTransaction tran = connection.BeginTransaction();

                                Query[] queries = session.Advanced.LoadStartingWith<Query>(
                                    "queries/",
                                    start: start);
                                if (queries.Length == 0)
                                {
                                    break;
                                }

                                foreach (Query query in queries)
                                {
                                    IMetadataDictionary metadata = session.Advanced.GetMetadataFor(query);

                                    string id = session.Advanced.GetDocumentId(query);
                                    if (int.TryParse(
                                        id.Replace("queries/", string.Empty).Replace("-A", string.Empty),
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
                                    _ = connection.Execute(
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
                else if (args.Length == 2 && args[0] == "test-run")
                {
                    Run(args[1]);
                }
                else
                {
                    Console.WriteLine("Usage: encode|migrate|test-run <connstr>");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(AsyncFriendlyStackTrace.ExceptionExtensions.ToAsyncString(ex));
            }
        }

        private static string[] GetPermutations(IDbConnection connection, string normalized)
        {
            IEnumerable<string> query =
                connection.Query<string>(
                    "select permutation from permutation where normalized = @normalized",
                    new { normalized });
            return query.ToArray();
        }

        private static string GetOriginal(IDbConnection connection, string normalized)
        {
            string query =
                connection.QuerySingleOrDefault<string>(
                    "select original from normalized where normalized = @normalized",
                    new { normalized });
            return query;
        }

        private static void Run(string connectionString)
        {
            Console.Write("Constructing search trees...");
            Stopwatch stopwatch = Stopwatch.StartNew();
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                (WordFinder ternary, WordFinder succinct) = CreateWordFinders(
                    x => GetPermutations(connection, x),
                    x => GetOriginal(connection, x));

                stopwatch.Stop();
                Console.WriteLine("{0} ms", stopwatch.ElapsedMilliseconds);

                Console.WriteLine("Enter word to search for. A single 'q' exits.");
                LineEditor lineEditor = new LineEditor("input");
                string input = lineEditor.Edit(": ", string.Empty);
                while (input != "q")
                {
                    do
                    {
                        if (string.IsNullOrWhiteSpace(input))
                        {
                            break;
                        }

                        foreach (WordFinder wordFinder in new[] { ternary, succinct }.Where(x => x != null))
                        {
                            stopwatch.Restart();
                            List<Match> matches = wordFinder.Matches(input, 2);
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

            string[] lines = File.ReadAllLines(WordsFilename, Encoding.UTF8);
            WordFinder wordFinderTernary = WordFinder.CreateTernary(lines, Language.Swedish);

            string succinctTreeDataJson = File.ReadAllText(EncodingFilename);
            SuccinctTreeData succinctTreeData = JsonConvert.DeserializeObject<SuccinctTreeData>(succinctTreeDataJson);
            WordFinder wordFinderSuccinct = WordFinder.CreateSuccinct(
                succinctTreeData,
                Language.Swedish,
                getPermutations,
                getOriginal);
            return (wordFinderTernary, wordFinderSuccinct);
        }
    }
}
