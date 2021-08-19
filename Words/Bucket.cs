#nullable enable

namespace Words
{
    public record Bucket(int Number, SuccinctTreeData Data)
    {
        public static int ToBucket(int length)
        {
            return length switch
            {
                int i when i is >= 1 and <= 7 => 1,
                int i when i is 8 => 2,
                int i when i is 9 => 3,
                int i when i is 10 => 4,
                int i when i is 11 => 5,
                int i when i is 12 => 6,
                int i when i is 13 => 7,
                int i when i is 14 => 8,
                int i when i is >= 15 and <= 16 => 9,
                _ => 10
            };
        }
    }
}