namespace Words
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Diagnostics;
    using System.Globalization;

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
            var wordFinder = new WordFinder(@"C:\Users\danlid\Dropbox\Programming\TernarySearchTree\swedish-word-list-bigger.txt", Encoding.UTF8, Language.Swedish);
            //var wordFinder = new WordFinder(@"C:\Users\danlid\Dropbox\Programming\TernarySearchTree\swedish-word-list", Encoding.UTF8);
            //var wordFinder = new WordFinder(@"C:\Users\danlid\Dropbox\Programming\TernarySearchTree\strange.txt", Encoding.UTF8);
            stopwatch.Stop();
            Console.WriteLine("{0} ms", stopwatch.ElapsedMilliseconds);

            Console.WriteLine("Enter word to search for. A single 'q' exits.");
            Console.Write(": ");
            string input = Console.ReadLine();
            while (input != "q")
            {
                if (string.IsNullOrWhiteSpace(input))
                    continue;

                stopwatch.Restart();
                //var matches = wordFinder.Matches(input);
                var matches = wordFinder.Near(input);
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

                Console.WriteLine("Search completed in {0} ms", stopwatch.ElapsedMilliseconds);
                Console.Write(": ");
                input = Console.ReadLine();
            }
        }
    }
}
