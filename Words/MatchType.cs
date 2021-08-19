#nullable enable

namespace Words
{
    public enum MatchType
    {
        /// <summary>
        /// Match found by using word search.
        /// </summary>
        Word,

        /// <summary>
        /// Match found by using anagram search.
        /// </summary>
        Anagram,

        Near
    }
}