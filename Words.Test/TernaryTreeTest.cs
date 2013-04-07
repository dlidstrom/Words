namespace Words.Test
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TernaryTreeTest
    {
        [TestMethod]
        public void RegularSearch()
        {
            // Arrange
            var tree = new TernaryTree(Language.Swedish);
            tree.Add("abcd", "aecd");

            // Act
            var matches = tree.Matches("aecd");

            // Assert
            Assert.AreEqual(1, matches.Count);
        }

        [TestMethod]
        public void WildCardSearch()
        {
            // Arrange
            var tree = new TernaryTree(Language.Swedish);
            tree.Add("abcd", "aecd");

            // Act
            var matches = tree.Matches("a?cd");

            // Assert
            Assert.AreEqual(2, matches.Count);
        }

        [TestMethod]
        public void VowelsSearch()
        {
            // Arrange
            var tree = new TernaryTree(Language.Swedish);
            tree.Add("abcd", "ebcd", "tbcd", "ubcd");

            // Act
            var matches = tree.Matches("@bcd");

            // Assert
            Assert.AreEqual(3, matches.Count);
        }

        [TestMethod]
        public void ConsonantsSearch()
        {
            // Arrange
            var tree = new TernaryTree(Language.Swedish);
            tree.Add("abeg", "abhg", "abfg", "abug", "abtg");

            // Act
            var matches = tree.Matches("ab#g");

            // Assert
            Assert.AreEqual(3, matches.Count);
        }

        [TestMethod]
        public void NearSearch()
        {
            // Arrange
            var tree = new TernaryTree(Language.Swedish);
            tree.Add("abcde", "abce", "abcf");

            // Act
            var matches = tree.NearSearch("abc");

            // Assert
            Assert.AreEqual(2, matches.Count);
        }

        [TestMethod]
        public void Traverse()
        {
            // Arrange
            var tree = new TernaryTree(Language.Swedish);
            tree.Add("abe", "abc", "abd");
            var visited = new List<string>();

            // Act
            tree.Traverse(visited.Add);

            // Assert
            var expected = new List<string> { "abc", "abd", "abe" };
            Assert.AreEqual(expected.Count, visited.Count);
            Assert.AreEqual(expected[0], visited[0]);
            Assert.AreEqual(expected[1], visited[1]);
            Assert.AreEqual(expected[2], visited[2]);
        }
    }
}
