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
            var bitWriter = new BitWriter();

            // Act
            foreach (var bit in bits)
            {
                bitWriter.Write(bit, 1);
            }

            // Assert
            var actual = bitWriter.GetData();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetNode()
        {
            // Arrange
            var bitWriter = new BitWriter();
            bitWriter.Write(new Node { Char = 'A', WordEnd = true });
            //bitWriter.Write(1, 1);
            var str = bitWriter.GetData().data;
            var bitString = new BitString(str);

            // Act
            var c = bitString.GetNode(0);

            // Assert
            Assert.That(c.Char, Is.EqualTo('A'));
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
