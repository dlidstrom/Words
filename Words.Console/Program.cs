#nullable enable

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
    using Newtonsoft.Json;
    using Npgsql;

    public static class Program
    {
        private const string NormalizedToOriginalsFilename = "normalized_to_originals.csv";
        private const string WordPermutationsFilename = "word_permutations.csv";
        private const string EncodingFilename = "words.json";

        public static void Main(string[] args)
        {
            try
            {
                if (args.Length == 2 && args[0] == "encode")
                {
                    string wordsFilename = args[1];
                    Console.WriteLine($"Reading {wordsFilename}");
                    string[] lines = File.ReadAllLines(wordsFilename, Encoding.UTF8);

                    // several trees
                    List<Bucket> trees = new();
                    IEnumerable<IGrouping<int, string>> buckets =
                        from line in lines
                        group line by Bucket.ToBucket(line.Length)
                        into grouping
                        select grouping;
                    foreach (IGrouping<int, string> bucket in buckets.OrderBy(x => x.Key))
                    {
                        string[] bucketLines = bucket.ToArray();
                        Console.WriteLine($"Creating ternary tree bucket={bucket.Key} size={bucketLines.Length}");
                        WordFinder wordFinder = WordFinder.CreateTernary(bucketLines, Language.Swedish);
                        Console.WriteLine("Encoding succinct");
                        SuccinctTree tree = wordFinder.EncodeSuccinct();
                        trees.Add(new Bucket(bucket.Key, tree.GetData()));
                    }

                    string json = JsonConvert.SerializeObject(trees, Formatting.Indented);
                    File.WriteAllText(EncodingFilename, json);
                    Console.WriteLine($"Created {EncodingFilename}");

                    WordFinder allWords = WordFinder.CreateTernary(lines, Language.Swedish);
                    IEnumerable<string> orderedKeys =
                        from item in allWords.NormalizedToOriginal
                        orderby item.Key
                        select $"{item.Key};{item.Value}";
                    File.WriteAllLines(
                        NormalizedToOriginalsFilename,
                        new[] { "normalized;original" }.Concat(orderedKeys),
                        Encoding.UTF8);
                    Console.WriteLine($"Created {NormalizedToOriginalsFilename}");

                    IEnumerable<string> permutations =
                        from item in allWords.Permutations
                        orderby item.Key
                        from val in item.Value
                        orderby val
                        select $"{item.Key};{val}";
                    File.WriteAllLines(
                        WordPermutationsFilename,
                        new[] { "normalized;permutation" }.Concat(permutations),
                        Encoding.UTF8);
                    Console.WriteLine($"Created {WordPermutationsFilename}");
                }
                else if (args.Length == 3 && args[0] == "test-run")
                {
                    Run(args[1], args[2]);
                }
                else
                {
                    Console.WriteLine("Usage: encode <words.txt> | test-run <connstr> <words.txt>");
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

        private static string[] GetOriginal(IDbConnection connection, string[] normalized)
        {
            IEnumerable<string> query =
                connection.Query<string>(
                    "select original from normalized where normalized in(@normalized)",
                    new { normalized });
            return query.ToArray();
        }

        private static void Run(string connectionString, string wordsFilename)
        {
            Console.Write("Constructing search trees...");
            Stopwatch stopwatch = Stopwatch.StartNew();
            using IDbConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
            (WordFinder ternary, WordFinder succinct) = CreateWordFinders(
                wordsFilename,
                x => GetPermutations(connection, x),
                x => GetOriginal(connection, x));

            stopwatch.Stop();
            Console.WriteLine("{0} ms", stopwatch.ElapsedMilliseconds);

            Console.WriteLine("Enter word to search for. A single 'q' exits.");
            string? input = Console.ReadLine();
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
                        List<Match> matches = wordFinder.Matches(
                            input,
                            2,
                            x => x,
                            x => new[] { x });
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

                input = Console.ReadLine();
            }
        }

        private static (WordFinder ternary, WordFinder succinct) CreateWordFinders(
            string wordsFilename,
            Func<string, string[]> getPermutations,
            Func<string[], string[]> getOriginal)
        {
            //var wordFinder = new WordFinder(@"C:\Users\danlid\Dropbox\Programming\TernarySearchTree\english-word-list.txt", Encoding.UTF8, Language.English);
            //var wordFinder = new WordFinder(@"C:\Users\danlid\Dropbox\Programming\TernarySearchTree\swedish-english.txt", Encoding.UTF8, Language.Swedish);

            string[] lines = File.ReadAllLines(wordsFilename, Encoding.UTF8);
            WordFinder wordFinderTernary = WordFinder.CreateTernary(lines, Language.Swedish);

            string succinctTreeDataJson = File.ReadAllText(EncodingFilename);
            SuccinctTreeData? succinctTreeData = JsonConvert.DeserializeObject<SuccinctTreeData>(succinctTreeDataJson);
            if (succinctTreeData is null)
            {
                throw new InvalidOperationException("deserialization failed");
            }

            WordFinder wordFinderSuccinct = WordFinder.CreateSuccinct(
                succinctTreeData,
                Language.Swedish,
                getPermutations,
                getOriginal);
            return (wordFinderTernary, wordFinderSuccinct);
        }
    }
}
