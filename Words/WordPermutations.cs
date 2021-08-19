#nullable enable

namespace Words
{
    public class WordPermutations
    {
        public WordPermutations()
        {
        }

        public WordPermutations(string normalizedSorted, string[] words)
        {
            NormalizedSorted = normalizedSorted;
            Words = words;
        }

        public string NormalizedSorted { get; private set; }

        public string[] Words { get; private set; }
    }
}