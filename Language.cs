﻿namespace Words
{
    using System.Collections.Generic;
    using System.Globalization;

    public class Language
    {
        public HashSet<char> Vowels { get; private set; }
        public HashSet<char> Consonants { get; private set; }
        public CultureInfo CultureInfo { get; private set; }

        public static Language Swedish
        {
            get
            {
                return new Language
                {
                    Vowels = new HashSet<char> { 'a', 'o', 'u', 'å', 'e', 'i', 'y', 'ä', 'ö' },
                    Consonants = new HashSet<char> { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'z' },
                    CultureInfo = new CultureInfo("sv-SE")
                };
            }
        }
    }
}
