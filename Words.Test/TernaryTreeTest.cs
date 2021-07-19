namespace Words.Test
{
    using System.Collections.Generic;
    using NUnit.Framework;

    [TestFixture]
    public class TernaryTreeTest
    {
        [Test]
        public void RegularSearch()
        {
            // Arrange
            TernaryTree tree = new TernaryTree(Language.Swedish);
            tree.Add("a", "b", "c");

            // Act
            List<string> matches = ((ITree)tree).Matches("b");

            // Assert
            Assert.AreEqual(1, matches.Count);
        }

        [Test]
        public void WildCardSearch()
        {
            // Arrange
            TernaryTree tree = new TernaryTree(Language.Swedish);
            tree.Add("abcd", "aecd");

            // Act
            List<string> matches = ((ITree)tree).Matches("a?cd");

            // Assert
            Assert.AreEqual(2, matches.Count);
        }

        [Test]
        public void VowelsSearch()
        {
            // Arrange
            TernaryTree tree = new TernaryTree(Language.Swedish);
            tree.Add("abcd", "ebcd", "tbcd", "ubcd");

            // Act
            List<string> matches = ((ITree)tree).Matches("@bcd");

            // Assert
            Assert.AreEqual(3, matches.Count);
        }

        [Test]
        public void ConsonantsSearch()
        {
            // Arrange
            TernaryTree tree = new TernaryTree(Language.Swedish);
            tree.Add("abeg", "abhg", "abfg", "abug", "abtg");

            // Act
            List<string> matches = ((ITree)tree).Matches("ab#g");

            // Assert
            Assert.AreEqual(3, matches.Count);
        }

        [Test]
        public void NearSearch()
        {
            // Arrange
            TernaryTree tree = new TernaryTree(Language.Swedish);
            tree.Add("abcde", "abce", "abcf");

            // Act
            List<string> matches = ((ITree)tree).NearSearch("abc");

            // Assert
            Assert.AreEqual(2, matches.Count);
        }

        [Test]
        public void Traverse()
        {
            // Arrange
            TernaryTree tree = new TernaryTree(Language.Swedish);
            tree.Add("abe", "abc", "abd");
            List<string> visited = new List<string>();

            // Act
            tree.Traverse(visited.Add);

            // Assert
            List<string> expected = new List<string> { "abc", "abd", "abe" };
            Assert.AreEqual(expected.Count, visited.Count);
            Assert.AreEqual(expected[0], visited[0]);
            Assert.AreEqual(expected[1], visited[1]);
            Assert.AreEqual(expected[2], visited[2]);
        }
    }
}
