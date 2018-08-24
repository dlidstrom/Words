namespace Words.Console
{
    using Mono.Terminal;
    using Newtonsoft.Json;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0 && args[0] == "encode")
                {
                    const string filename = @"C:\Programming\Words\Words.Web\App_Data\words.txt";
                    var lines = File.ReadAllLines(filename, Encoding.UTF8);
                    var wordFinder = WordFinder.CreateTernary(lines, Language.Swedish);
                    var tree = wordFinder.EncodeSuccinct();
                    var json = JsonConvert.SerializeObject(tree.GetData(), Formatting.Indented);
                    File.WriteAllText(
                        @"C:\Programming\words.json",
                        json);
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
            var stopwatch = new Stopwatch();
            Console.Write("Constructing search trees...");
            stopwatch.Start();
            var (ternary, succinct) = CreateWordFinders();

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

                    foreach (var wordFinder in new[] { ternary, succinct })
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

        private static (WordFinder ternary, WordFinder succinct) CreateWordFinders()
        {
            //var wordFinder = new WordFinder(@"C:\Users\danlid\Dropbox\Programming\TernarySearchTree\english-word-list.txt", Encoding.UTF8, Language.English);
            //var wordFinder = new WordFinder(@"C:\Users\danlid\Dropbox\Programming\TernarySearchTree\swedish-english.txt", Encoding.UTF8, Language.Swedish);

            const string filename = @"C:\Programming\Words\Words.Web\App_Data\words.txt";
            var lines = File.ReadAllLines(filename, Encoding.UTF8);
            var wordFinderTernary = WordFinder.CreateTernary(lines, Language.Swedish);

            var succinctTreeDataJson = File.ReadAllText(@"C:\Programming\words.json");
            var succinctTreeData = JsonConvert.DeserializeObject<SuccinctTreeData>(succinctTreeDataJson);
            var wordFinderSuccinct = WordFinder.CreateSuccinct(lines, succinctTreeData, Language.Swedish);
            return (wordFinderTernary, wordFinderSuccinct);
        }
    }
}
