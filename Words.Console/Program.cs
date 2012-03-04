﻿namespace Words.Console
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using Mono.Terminal;
    using System.Linq;

    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                new Program().Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void Run()
        {
            Stopwatch stopwatch = new Stopwatch();
            Console.Write("Constructing search tree...");
            stopwatch.Start();
            var wordFinder = new WordFinder(@"C:\Users\Daniel\Dropbox\Programming\TernarySearchTree\swedish-word-list-bigger.txt", Encoding.UTF8, Language.Swedish);
            //var wordFinder = new WordFinder(@"C:\Users\danlid\Dropbox\Programming\TernarySearchTree\english-word-list.txt", Encoding.UTF8, Language.English);
            //var wordFinder = new WordFinder(@"C:\Users\danlid\Dropbox\Programming\TernarySearchTree\swedish-english.txt", Encoding.UTF8, Language.Swedish);
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

                    stopwatch.Restart();
                    var matches = wordFinder.Matches(input, 2, 100);
                    stopwatch.Stop();
                    if (matches.Count > 0)
                    {
                        Console.WriteLine("Found {0} words matching '{1}':", matches.Count, input);
                        matches.ForEach(m => Console.WriteLine("{0,-7}: {1}", m.Type, m.Value));
                    }
                    else
                    {
                        Console.WriteLine("Did not find any words matching '{0}'", input);
                    }

                    Console.WriteLine("Search completed in {0:F2} ms. Visited {1} nodes.", 1000.0 * stopwatch.ElapsedTicks / Stopwatch.Frequency, wordFinder.Nodes);
                }
                while (false);
                input = lineEditor.Edit(": ", string.Empty);
            }
        }
    }
}
