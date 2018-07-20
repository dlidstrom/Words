namespace Words
{
    using System.Diagnostics;

    [DebuggerDisplay("Char = {Char}, WordEnd = {WordEnd}, Left = {Left?.Char}, Center = {Center?.Char}, Right = {Right?.Char}")]
    public class Node
    {
        public char Char { get; set; }
        public bool WordEnd { get; set; }
        public Node Left;
        public Node Center;
        public Node Right;

        public override string ToString()
        {
            return $"Char = {Char}, WordEnd = {WordEnd}, Left = {Left.Char}, Center = {Center.Char}, Right = {Right.Char}";
        }
    }
}