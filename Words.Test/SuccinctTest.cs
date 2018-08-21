namespace Words.Test
{
    using System.Collections.Generic;
    using NUnit.Framework;

    [TestFixture]
    public class SuccinctTest
    {
        [TestCaseSource(nameof(RegularSearchSource))]
        public void RegularSearch(string[] s, string m)
        {
            // Arrange
            var ternaryTree = new TernaryTree(Language.Swedish);
            ternaryTree.Add(s);
            var tree = ternaryTree.EncodeSuccinct();

            // Act
            var ternaryTreeMatches = ternaryTree.Matches(m);
            var treeMatches = tree.Matches(m);

            // Assert
            Assert.That(treeMatches, Is.EqualTo(ternaryTreeMatches));
        }

        [Test]
        public void ConsonantsSearch()
        {
            // Arrange
            var ternaryTree = new TernaryTree(Language.Swedish);
            ternaryTree.Add("abeg", "abhg", "abfg", "abug", "abtg");
            var tree = ternaryTree.EncodeSuccinct();

            // Act
            var ternaryTreeMatches = ternaryTree.Matches("ab#g");
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
            var tree = ternaryTree.EncodeSuccinct();

            // Act
            var ternaryTreeMatches = ternaryTree.Matches("a?cd");
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
            var tree = ternaryTree.EncodeSuccinct();

            // Act
            var ternaryTreeMatches = ternaryTree.Matches("@bcd");
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
            var tree = ternaryTree.EncodeSuccinct();

            // Act
            var ternaryTreeMatches = ternaryTree.NearSearch("abc");
            var matches = tree.Matches("abc");

            // Assert
            Assert.That(matches, Is.EqualTo(ternaryTreeMatches));
        }

        [TestCaseSource(nameof(SelectSource))]
        public void Select(int selectIndex, int selectValue)
        {
            // Arrange
            var writer = new BitWriter();
            writer.Write(0x03, 2);
            writer.Write(0, 5);
            writer.Write(1, 1);
            var tree = new SuccinctTree(writer.GetData().data, new SuccinctNode[5], null);

            // Act
            var actual = tree.Select(1, selectIndex);

            // Assert
            Assert.That(actual, Is.EqualTo(selectValue));
        }

        [TestCaseSource(nameof(RankSource))]
        public void Rank(int rankIndex, int rankValue)
        {
            // Arrange
            var writer = new BitWriter();
            writer.Write(0x03, 2);
            writer.Write(0, 5);
            writer.Write(1, 1);
            var tree = new SuccinctTree(writer.GetData().data, new SuccinctNode[5], null);

            // Act
            var actual = tree.Rank(1, rankIndex);

            // Assert
            Assert.That(actual, Is.EqualTo(rankValue));
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
            }
        }

        private static IEnumerable<TestCaseData> SelectSource
        {
            get
            {
                yield return new TestCaseData(1, 0);
                yield return new TestCaseData(2, 1);
                yield return new TestCaseData(3, 7);
            }
        }

        private static IEnumerable<TestCaseData> RankSource
        {
            get
            {
                yield return new TestCaseData(0, 1);
                yield return new TestCaseData(1, 2);
                yield return new TestCaseData(2, 2);
                yield return new TestCaseData(3, 2);
                yield return new TestCaseData(4, 2);
                yield return new TestCaseData(5, 2);
                yield return new TestCaseData(6, 2);
                yield return new TestCaseData(7, 3);
            }
        }
    }
}
