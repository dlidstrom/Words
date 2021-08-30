#nullable enable

namespace Words
{
    using System.Diagnostics;

    [DebuggerDisplay("Char = {Character}, WordEnd = {WordEnd}, Left = {Left?.Character}, Center = {Center?.Character}, Right = {Right?.Character}")]
    public class Node
    {
        public char Character { get; set; }
        public bool WordEnd { get; set; }
        public Node? Left;
        public Node? Center;
        public Node? Right;

        public override string ToString()
        {
            return $"Char = {Character}, WordEnd = {WordEnd}, Left = {Left?.Character}, Center = {Center?.Character}, Right = {Right?.Character}";
        }
    }
}
