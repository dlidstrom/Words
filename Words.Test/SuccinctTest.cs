namespace Words.Test
{
    using System.Collections.Generic;
    using NUnit.Framework;

    [TestFixture]
    public class SuccinctTest
    {
        [Test]
        public void EncodesTree()
        {
            // Arrange
            var ternaryTree = new TernaryTree(Language.Swedish);
            ternaryTree.Add("ab", "ac", "ad");

            // Act
            var tree = ternaryTree.EncodeSuccinct();

            // Assert
            ternaryTree.Contains("ab");
            Assert.That(tree.Contains("ab"));
        }

        [Test]
        public void RegularSearch()
        {
            // Arrange
            var ternaryTree = new TernaryTree(Language.Swedish);
            ternaryTree.Add("a", "b");
            var tree = ternaryTree.EncodeSuccinct();

            // Act
            var matches = tree.Matches("b");

            // Assert
            Assert.AreEqual(1, matches.Count);
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
