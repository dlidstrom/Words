namespace Words
{
    public class Node
    {
        public char Char { get; set; }
        public bool WordEnd { get; set; }
        public Node Left;
        public Node Center;
        public Node Right;

        public override string ToString()
        {
            return string.Format("Char = {0}, WordEnd = {1}, Left = {2}, Center = {3}, Right = {4}", Char, WordEnd, Left.Char, Center.Char, Right.Char);
        }
    }
}