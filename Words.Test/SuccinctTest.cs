namespace Words.Test
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    [TestFixture]
    public class SuccinctTest
    {
        [TestCaseSource(nameof(RegularSearchSource))]
        public void RegularSearch(string[] s, string m)
        {
            // Arrange
            TernaryTree ternaryTree = new TernaryTree(Language.Swedish)
            {
                Log = Console.WriteLine
            };
            ternaryTree.Add(s);
            SuccinctTree tree = ternaryTree.EncodeSuccinct();
            tree.Log = Console.WriteLine;

            // Act
            Console.WriteLine("Ternary:");
            List<string> ternaryTreeMatches = ((ITree)ternaryTree).Matches(m);
            Console.WriteLine("Succinct:");
            List<string> treeMatches = ((ITree)tree).Matches(m);

            // Assert
            Assert.That(treeMatches, Is.EqualTo(ternaryTreeMatches));
        }

        [Test]
        public void ConsonantsSearch()
        {
            // Arrange
            TernaryTree ternaryTree = new TernaryTree(Language.Swedish);
            ternaryTree.Add("abeg", "abhg", "abfg", "abug", "abtg");
            ITree tree = ternaryTree.EncodeSuccinct();

            // Act
            List<string> ternaryTreeMatches = ((ITree)ternaryTree).Matches("ab#g");
            List<string> matches = tree.Matches("ab#g");

            // Assert
            Assert.That(matches, Is.EqualTo(ternaryTreeMatches));
        }

        [Test]
        public void WildCardSearch()
        {
            // Arrange
            TernaryTree ternaryTree = new TernaryTree(Language.Swedish);
            ternaryTree.Add("abcd", "aecd");
            ITree tree = ternaryTree.EncodeSuccinct();

            // Act
            List<string> ternaryTreeMatches = ((ITree)ternaryTree).Matches("a?cd");
            List<string> matches = tree.Matches("a?cd");

            // Assert
            Assert.That(matches, Is.EqualTo(ternaryTreeMatches));
        }

        [Test]
        public void VowelsSearch()
        {
            // Arrange
            TernaryTree ternaryTree = new TernaryTree(Language.Swedish);
            ternaryTree.Add("abcd", "ebcd", "tbcd", "ubcd");
            ITree tree = ternaryTree.EncodeSuccinct();

            // Act
            List<string> ternaryTreeMatches = ((ITree)ternaryTree).Matches("@bcd");
            List<string> matches = tree.Matches("@bcd");

            // Assert
            Assert.That(matches, Is.EqualTo(ternaryTreeMatches));
        }

        [Test]
        public void NearSearch()
        {
            // Arrange
            TernaryTree ternaryTree = new TernaryTree(Language.Swedish);
            ternaryTree.Add("abcde", "abce", "abcf");
            ITree tree = ternaryTree.EncodeSuccinct();

            // Act
            List<string> ternaryTreeMatches = ((ITree)ternaryTree).NearSearch("abc");
            List<string> matches = tree.NearSearch("abc");

            // Assert
            Assert.That(matches, Is.EqualTo(ternaryTreeMatches));
        }

        [TestCaseSource(nameof(VerifySource))]
        public void VerifyMatches(int wordsToAdd, int i, string addedWords)
        {
            TernaryTree ternary = new TernaryTree(Language.Swedish);
            foreach (string addedWord in addedWords.Split(','))
            {
                ternary.Add(addedWord);
            }

            SuccinctTree succinct = ternary.EncodeSuccinct();

            // Act
            foreach (string addedWord in addedWords.Split(','))
            {
                List<string> ternaryMatches = ternary.Matches(addedWord, 100);
                List<string> succinctMatches = succinct.Matches(addedWord, 100);

                // Assert
                Assert.That(
                    succinctMatches,
                    Is.EqualTo(ternaryMatches),
                    $"Failed to find equal matches for {addedWord}. Added words: {string.Join(", ", addedWords)}.");
            }
        }

        [TestCaseSource(nameof(VerifySource))]
        public void VerifyNear(int wordsToAdd, int i, string addedWords)
        {
            TernaryTree ternary = new TernaryTree(Language.Swedish);
            foreach (string addedWord in addedWords.Split(','))
            {
                ternary.Add(addedWord);
            }

            SuccinctTree succinct = ternary.EncodeSuccinct();

            // Act
            foreach (string addedWord in addedWords.Split(','))
            {
                List<string> ternaryMatches = ((ITree)ternary).NearSearch(addedWord);
                List<string> succinctMatches = ((ITree)succinct).NearSearch(addedWord);

                // Assert
                Assert.That(
                    succinctMatches,
                    Is.EqualTo(ternaryMatches),
                    $"Failed to find near matches for {addedWord}. Added words: {string.Join(", ", addedWords)}.");
            }
        }

        private static IEnumerable<TestCaseData> VerifySource
        {
            get
            {
                const string filename = @"C:\Programming\Words\Words.Web\App_Data\words.txt";
                string[] lines = File.ReadAllLines(filename, Encoding.UTF8);
                Random random = new Random();

                // try adding more and more words
                for (int wordsToAdd = 2; wordsToAdd < 25; wordsToAdd++)
                {
                    // try a few random selections
                    for (int i = 0; i < 30; i++)
                    {
                        List<string> addedWords = new List<string>();
                        for (int j = 0; j < wordsToAdd; j++)
                        {
                            int wordIndex = random.Next(lines.Length);
                            string word = lines[wordIndex];
                            addedWords.Add(word);
                        }

                        yield return new TestCaseData(wordsToAdd, i, string.Join(",", addedWords));
                    }
                }
            }
        }

        private static IEnumerable<TestCaseData> RegularSearchSource
        {
            get
            {
                yield return new TestCaseData(new[] { "ab", "ac" }, "ac");
                yield return new TestCaseData(new[] { "ab", "ac", "ad" }, "ab");
                yield return new TestCaseData(new[] { "ab", "ac", "ad" }, "ac");
                yield return new TestCaseData(new[] { "ab", "ac", "ad" }, "ad");
                yield return new TestCaseData(new[] { "ab", "ac", "ad", "ae" }, "ae");
                yield return new TestCaseData(new[] { "abc", "acb", "adc" }, "abc");
                yield return new TestCaseData(new[] { "abc", "acb", "adc" }, "acb");
                yield return new TestCaseData(new[] { "abc", "acb", "adc" }, "adc");
                yield return new TestCaseData(new[] { "abc", "acb" }, "acb");
                yield return new TestCaseData(new[] { "nedkämpades", "edisongängors" }, "edisongängors");
                string[] words = new[] { "di", "likströmsgeneratorerna", "indrivbar", "innergängsfräsning", "sprutmålning", "klassexemplar", "illfundighet", "helikopterräddning", "småspiken", "dingdång", "obeprövad" };
                foreach (string word in words)
                {
                    yield return new TestCaseData(words, word);
                }
            }
        }
    }
}
