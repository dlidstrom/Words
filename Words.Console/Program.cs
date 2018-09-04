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
                else
                {
                    Run();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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
