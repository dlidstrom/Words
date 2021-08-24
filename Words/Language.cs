#nullable enable

namespace Words
{
    using System.Collections.Generic;
    using System.Globalization;

    public record Language(CultureInfo CultureInfo, SortedSet<char> Vowels, SortedSet<char> Consonants)
    {
        public static Language Swedish => new(
            new CultureInfo("sv-SE"),
            Vowels: new SortedSet<char> { 'a', 'o', 'u', 'å', 'e', 'i', 'y', 'ä', 'ö' },
            Consonants: new SortedSet<char> { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'z' });

        public static Language English => new(
            new CultureInfo("en"),
            Vowels: new SortedSet<char> { 'a', 'o', 'u', 'e', 'i', 'y' },
            Consonants: new SortedSet<char> { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'z' });

        public string ToLower(string word)
        {
            return word.ToLower(CultureInfo);
        }
    }
}
