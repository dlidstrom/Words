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
            ITree tree = ternaryTree.EncodeSuccinct();

            // Act
            var ternaryTreeMatches = ((ITree)ternaryTree).Matches(m);
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
    }
}
