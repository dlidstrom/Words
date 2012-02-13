namespace Words
{
    using System;
    using System.Diagnostics;
    using System.Text;

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
            stopwatch.Stop();
            Console.WriteLine("{0} ms", stopwatch.ElapsedMilliseconds);

            Console.WriteLine("Enter word to search for. A single 'q' exits.");
            Console.Write(": ");
            string input = Console.ReadLine();
            while (input != "q")
            {
                do
                {
                    if (string.IsNullOrWhiteSpace(input))
                        break;

                    stopwatch.Restart();
                    var matches = wordFinder.Matches(input);
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

                    Console.WriteLine("Search completed in {0:F2} ms", 1000.0 * stopwatch.ElapsedTicks / Stopwatch.Frequency);
                }
                while (false);
                Console.Write(": ");
                input = Console.ReadLine();
            }
        }
    }
}
