namespace Words
{
    public class SuccinctTreeData
    {
        public SuccinctTreeData(string[] encodingBytes, int encodingBits, string[] letterBytes)
        {
            EncodingBytes = encodingBytes;
            EncodingBits = encodingBits;
            LetterBytes = letterBytes;
        }

        public string[] EncodingBytes { get; }

        public int EncodingBits { get; }

        public string[] LetterBytes { get; }
    }
}