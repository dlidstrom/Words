namespace Words.Test
{
    using System.Collections.Generic;
    using NUnit.Framework;

    [TestFixture]
    public class BitWriterTest
    {
        [TestCaseSource(nameof(TestDataSource))]
        public void VerifyData(int[] bits, string expected)
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

        private static IEnumerable<TestCaseData> TestDataSource
        {
            get
            {
                yield return new TestCaseData(new[] { 0u }, "A");
                yield return new TestCaseData(new[] { 1u }, "g");
                yield return new TestCaseData(new[] { 1u, 1u }, "w");
            }
        }
    }
}
