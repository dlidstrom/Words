#nullable enable

namespace Words
{
    using System;

    [Flags]
    public enum SearchType
    {
        None = 0,
        Word = 1,
        Anagram = 2,
        Near = 4,
        All = Word | Anagram | Near
    }
}
