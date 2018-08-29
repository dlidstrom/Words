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
            var ternaryTree = new TernaryTree(Language.Swedish)
            {
                Log = Console.WriteLine
            };
            ternaryTree.Add(s);
            var tree = ternaryTree.EncodeSuccinct();
            tree.Log = Console.WriteLine;

            // Act
            Console.WriteLine("Ternary:");
            var ternaryTreeMatches = ((ITree)ternaryTree).Matches(m);
            Console.WriteLine("Succinct:");
            var treeMatches = ((ITree)tree).Matches(m);

            // Assert
            Assert.That(treeMatches, Is.EqualTo(ternaryTreeMatches));
        }

        [Test]
        public void ConsonantsSearch()
        {
            // Arrange
            var ternaryTree = new TernaryTree(Language.Swedish);
            ternaryTree.Add("abeg", "abhg", "abfg", "abug", "abtg");
            ITree tree = ternaryTree.EncodeSuccinct();

            // Act
            var ternaryTreeMatches = ((ITree)ternaryTree).Matches("ab#g");
            var matches = tree.Matches("ab#g");

            // Assert
            Assert.That(matches, Is.EqualTo(ternaryTreeMatches));
        }

        [Test]
        public void WildCardSearch()
        {
            // Arrange
            var ternaryTree = new TernaryTree(Language.Swedish);
            ternaryTree.Add("abcd", "aecd");
            ITree tree = ternaryTree.EncodeSuccinct();

            // Act
            var ternaryTreeMatches = ((ITree)ternaryTree).Matches("a?cd");
            var matches = tree.Matches("a?cd");

            // Assert
            Assert.That(matches, Is.EqualTo(ternaryTreeMatches));
        }

        [Test]
        public void VowelsSearch()
        {
            // Arrange
            var ternaryTree = new TernaryTree(Language.Swedish);
            ternaryTree.Add("abcd", "ebcd", "tbcd", "ubcd");
            ITree tree = ternaryTree.EncodeSuccinct();

            // Act
            var ternaryTreeMatches = ((ITree)ternaryTree).Matches("@bcd");
            var matches = tree.Matches("@bcd");

            // Assert
            Assert.That(matches, Is.EqualTo(ternaryTreeMatches));
        }

        [Test]
        public void NearSearch()
        {
            // Arrange
            var ternaryTree = new TernaryTree(Language.Swedish);
            ternaryTree.Add("abcde", "abce", "abcf");
            ITree tree = ternaryTree.EncodeSuccinct();

            // Act
            var ternaryTreeMatches = ((ITree)ternaryTree).NearSearch("abc");
            var matches = tree.NearSearch("abc");

            // Assert
            Assert.That(matches, Is.EqualTo(ternaryTreeMatches));
        }

        [TestCaseSource(nameof(VerifySource))]
        public void Verify(int wordsToAdd, int i, string addedWords)
        {
            var ternary = new TernaryTree(Language.Swedish);
            foreach (var addedWord in addedWords.Split(','))
            {
                ternary.Add(addedWord);
            }

            var succinct = ternary.EncodeSuccinct();

            // Act
            foreach (var addedWord in addedWords.Split(','))
            {
                var ternaryMatches = ternary.Matches(addedWord, 100);
                var succinctMatches = succinct.Matches(addedWord, 100);

                // Assert
                Assert.That(
                    succinctMatches,
                    Is.EqualTo(ternaryMatches),
                    $"Failed to find equal matches for {addedWord}. Added words: {string.Join(", ", addedWords)}.");
            }
        }

        private static IEnumerable<TestCaseData> VerifySource
        {
            get
            {
                const string filename = @"C:\Programming\Words\Words.Web\App_Data\words.txt";
                var lines = File.ReadAllLines(filename, Encoding.UTF8);
                var random = new Random();

                // try adding more and more words
                for (var wordsToAdd = 2; wordsToAdd < 50; wordsToAdd++)
                {
                    // try a few random selections
                    for (var i = 0; i < 30; i++)
                    {
                        var addedWords = new List<string>();
                        for (var j = 0; j < wordsToAdd; j++)
                        {
                            var wordIndex = random.Next(lines.Length);
                            var word = lines[wordIndex];
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
                var words = new[] { "di", "likströmsgeneratorerna", "indrivbar", "innergängsfräsning", "sprutmålning", "klassexemplar", "illfundighet", "helikopterräddning", "småspiken", "dingdång", "obeprövad" };
                foreach (var word in words)
                {
                    yield return new TestCaseData(words, word);
                }
            }
        }
    }
}
