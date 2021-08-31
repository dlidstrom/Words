#nullable enable

namespace Words.Test
{
    using NUnit.Framework;
    using System.Collections.Generic;

    [TestFixture]
    public class BitWriterTest
    {
        [TestCaseSource(nameof(TestDataSource))]
        public void VerifyData(int[] bits, (string data, int totalBits) expected)
        {
            // Arrange
            BitWriter bitWriter = new();

            // Act
            foreach (int bit in bits)
            {
                bitWriter.Write(bit, 1);
            }

            // Assert
            (string data, int totalBits) actual = bitWriter.GetData();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetNode()
        {
            // Arrange
            BitWriter bitWriter = new();
            bitWriter.Write(new Node { Character = (char)0, WordEnd = true });
            string str = bitWriter.GetData().data;
            BitString bitString = new(str);

            // Act
            SuccinctNode c = bitString.GetNode(0);

            // Assert
            Assert.That(c.Character, Is.EqualTo('\u0000'));
            Assert.That(c.WordEnd, Is.True);
        }

        private static IEnumerable<TestCaseData> TestDataSource
        {
            get
            {
                yield return new TestCaseData(new[] { 0u }, ("A", 1));
                yield return new TestCaseData(new[] { 1u }, ("g", 1));
                yield return new TestCaseData(new[] { 1u, 1u }, ("w", 2));
            }
        }
    }
}
