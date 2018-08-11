namespace Words
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;

    /// <summary>
    /// Ternary search tree for string matching.
    /// </summary>
    /// <code>
    /// typedef struct tnode *Tptr;
    /// typedef struct tnode {
    ///     char splitchar;
    ///     Tptr lokid, eqkid, hikid;
    /// } Tnode;
    /// </code>
    /// <code>
    /// int rsearch(Tptr p, char *s)
    /// {
    ///     if (!p) return 0;
    ///     if (*s &lt; p-&gt;splitchar)
    ///         return rsearch(p-&gt;lokid, s);
    ///     else if (*s &gt; p->splitchar)
    ///         return rsearch(p-&gt;hikid, s);
    ///     else {
    ///         if (*s == 0) return 1;
    ///         return rsearch(p-&gt;eqkid, ++s);
    ///     }
    /// }
    /// </code>
    /// <code>
    /// int search(char *s)
    /// {
    ///     Tptr p;
    ///     p = root;
    ///     while (p) {
    ///         if (*s &lt; p-&gt;splitchar)
    ///             p = p-&gt;lokid;
    ///         else if (*s == p-&gt;splitchar) {
    ///             if (*s++ == 0)
    ///                 return 1;
    ///             p = p-&gt;eqkid;
    ///         } else
    ///             p = p-&gt;hikid;
    ///     }
    ///     return 0;
    /// }
    /// </code>
    public class TernaryTree
    {
        private readonly Language language;
        private Node root;

        /// <summary>
        /// Initializes a new instance of the TernaryTree class.
        /// </summary>
        /// <param name="language"></param>
        public TernaryTree(Language language)
        {
            this.language = language ?? throw new ArgumentNullException(nameof(language));
        }

        public int Nodes { get; private set; }

        /// <summary>
        /// Add word to tree.
        /// </summary>
        /// <code>
        /// Tptr insert(Tptr p, char *s)
        /// {
        ///     if (p == 0) {
        ///         p = (Tptr) malloc(sizeof(Tnode));
        ///         p-&gt;splitchar = *s;
        ///         p-&gt;lokid = p-&gt;eqkid = p-&gt;hikid = 0;
        ///     }
        ///
        ///     if (*s &lt; p-&gt;splitchar)
        ///         p-&gt;lokid = insert(p-&gt;lokid, s);
        ///     else if (*s == p-&gt;splitchar) {
        ///         if (*s != 0)
        ///             p-&gt;eqkid = insert(p-&gt;eqkid, ++s);
        ///     }
        ///     else
        ///         p-&gt;hikid = insert(p-&gt;hikid, s);
        ///     return p;
        /// }
        /// </code>
        /// <param name="s"></param>
        public void Add(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException("s");

            Add(s, 0, ref root);
        }

        /// <summary>
        /// Add words to tree.
        /// </summary>
        /// <param name="s"></param>
        public void Add(params string[] s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            foreach (var item in s)
                Add(item);
        }

        public bool Contains(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException();

            int pos = 0;
            Node node = root;
            while (node != null)
            {
                if (s[pos] < node.Char)
                    node = node.Left;
                else if (s[pos] > node.Char)
                    node = node.Right;
                else
                {
                    if (++pos == s.Length)
                        return node.WordEnd;
                    node = node.Center;
                }
            }

            return false;
        }

        public List<string> Matches(string s, int limit = 100)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException();

            var matches = new List<string>();

            const int pos = 0;
            Node node = root;
            Nodes = 0;
            Matches(s, string.Empty, pos, node, matches, limit);

            return matches;
        }

        /// <summary>
        /// Traverse tree in sorted order.
        /// </summary>
        /// <code>
        /// void traverse(Tptr p)
        /// {
        ///     if (!p) return;
        ///     traverse(p-&gt;lokid);
        ///     if (p-&gt;splitchar)
        ///         traverse(p-&gt;eqkid);
        ///     else
        ///         printf("%s/n", (char *) p-&gt;eqkid);
        ///     traverse(p-&gt;hikid);
        /// }
        /// </code>
        public void Traverse(Action<string> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            Traverse(root, action, string.Empty);
        }

        private void Traverse(Node node, Action<string> action, string s)
        {
            if (node == null)
                return;

            Traverse(node.Left, action, s);
            if (node.WordEnd)
                action.Invoke(s);
            else
                Traverse(node.Center, action, s + node.Char);
            Traverse(node.Right, action, s);
        }

        public List<string> NearSearch(string s, int d = 1, int limit = 100)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException();

            var matches = new List<string>();

            const int pos = 0;
            Node node = root;
            Nodes = 0;
            NearSearch(s, string.Empty, pos, node, matches, limit, d);

            return matches;
        }

        private static Node Add(string s, int pos, ref Node node)
        {
            char c = pos == s.Length ? default(char) : s[pos];
            if (node == null)
                node = new Node { Char = c, WordEnd = false };

            if (c < node.Char)
                node.Left = Add(s, pos, ref node.Left);
            else if (c == node.Char)
            {
                if (pos != s.Length)
                    node.Center = Add(s, pos + 1, ref node.Center);
                else
                    node.WordEnd = true;
            }
            else
                node.Right = Add(s, pos, ref node.Right);

            return node;
        }

        /// <summary>
        /// Performs a matching search.
        /// </summary>
        /// <code>
        /// void pmsearch(Tptr p, char *s)
        /// {
        ///     if (!p) return;
        ///     nodecnt++;
        ///     if (*s == '.' || *s &lt; p-&gt;splitchar)
        ///         pmsearch(p-&gt;lokid, s);
        ///     if (*s == '.' || *s == p-&gt;splitchar)
        ///         if (p-&gt;splitchar && *s)
        ///             pmsearch(p-&gt;eqkid, s+1);
        ///     if (*s == 0 && p-&gt;splitchar == 0)
        ///         srcharr[srchtop++] = (char *) p-&gt;eqkid;
        ///     if (*s == '.' || *s &gt; p-&gt;splitchar)
        ///         pmsearch(p-&gt;hikid, s);
        /// }
        /// </code>
        /// <param name="s"></param>
        /// <param name="substr"></param>
        /// <param name="pos"></param>
        /// <param name="node"></param>
        /// <param name="matches"></param>
        /// <param name="limit"></param>
        private void Matches(string s, string substr, int pos, Node node, List<string> matches, int limit)
        {
            if (node == null || matches.Count >= limit)
                return;
            Nodes++;

            char c = pos == s.Length ? default(char) : s[pos];
            if (WildcardMatchLeft(c, node.Char) || c < node.Char)
                Matches(s, substr, pos, node.Left, matches, limit);

            if (WildcardMatch(c, node.Char) || c == node.Char)
                Matches(s, substr + node.Char, pos + 1, node.Center, matches, limit);

            if (c == default(char) && node.WordEnd)
                matches.Add(substr);

            if (WildcardMatchRight(c, node.Char) || c > node.Char)
                Matches(s, substr, pos, node.Right, matches, limit);
        }

        /// <summary>
        /// Determines if search should traverse the center node.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool WildcardMatch(char c, char node)
        {
            if (c == '?')
                return true;
            if (c == '#')
                return language.Consonants.Contains(node);
            if (c == '@')
                return language.Vowels.Contains(node);
            return false;
        }

        /// <summary>
        /// Determines if search should traverse the left node.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool WildcardMatchLeft(char c, char node)
        {
            if (c == '?')
                return true;
            if (c == '#')
                return language.Consonants.Min <= node;
            if (c == '@')
                return language.Vowels.Min <= node;
            return false;
        }

        /// <summary>
        /// Determines if search should traverse the right node.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool WildcardMatchRight(char c, char node)
        {
            if (c == '?')
                return true;
            if (c == '#')
                return node <= language.Consonants.Max;
            if (c == '@')
                return node <= language.Vowels.Max;
            return false;
        }

        /// <summary>
        /// Finds matches using a nearest search.
        /// </summary>
        /// <code>
        /// void nearsearch(Tptr p, char *s, int d)
        /// {
        ///     if (!p || d &lt; 0) return;
        ///     nodecnt++;
        ///     if (d &gt; 0 || *s &lt; p-&gt;splitchar)
        ///         nearsearch(p->lokid, s, d);
        ///     if (p-&gt;splitchar == 0) {
        ///         if ((int) strlen(s) &lt;= d)
        ///         srcharr[srchtop++] = (char *) p-&gt;eqkid;
        ///     }
        ///     else
        ///         nearsearch(p->eqkid,
        ///                    *s ? s+1:s,
        ///                    (*s==p->splitchar) ? d:d-1);
        ///     if (d &gt; 0 || *s &gt; p->splitchar)
        ///         nearsearch(p->hikid, s, d);
        /// }
        /// </code>
        /// <param name="s"></param>
        /// <param name="substr"></param>
        /// <param name="pos"></param>
        /// <param name="node"></param>
        /// <param name="matches"></param>
        /// <param name="limit"></param>
        /// <param name="depth"></param>
        private void NearSearch(string s, string substr, int pos, Node node, List<string> matches, int limit, int depth)
        {
            if (node == null || matches.Count >= limit || depth < 0)
                return;

            Nodes++;

            char c = default(char);
            if (pos < s.Length)
                c = s[pos];

            if (depth > 0 || c < node.Char)
                NearSearch(s, substr, pos, node.Left, matches, limit, depth);

            if (node.WordEnd)
            {
                if (s.Length - pos <= depth)
                    matches.Add(substr);
            }
            else
            {
                int newDepth = c == node.Char ? depth : depth - 1;
                if (c != default(char))
                    NearSearch(s, substr + node.Char, pos + 1, node.Center, matches, limit, newDepth);
                else
                    NearSearch(s, substr + node.Char, pos, node.Center, matches, limit, newDepth);
            }

            if (depth > 0 || c > node.Char)
                NearSearch(s, substr, pos, node.Right, matches, limit, depth);
        }

        public SuccinctTree EncodeSuccinct()
        {
            var encodingBuilder = new StringBuilder("10");
            var nodes = new List<SuccinctNode>();
            var stack = new Stack<Node>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var children = 0;
                var node = stack.Pop();
                if (node.WordEnd) continue;

                if (node.Center != null)
                {
                    stack.Push(node.Center);
                    children++;
                }

                if (node.Left != null)
                {
                    stack.Push(node.Left);
                    children++;
                }

                if (node.Right != null)
                {
                    stack.Push(node.Right);
                    children++;
                }

                encodingBuilder.Append(new string('1', children));
                encodingBuilder.Append("0");
                nodes.Add(new SuccinctNode(node, children));
            }

            var encoding = encodingBuilder.ToString();
            return new SuccinctTree(encoding, nodes.ToArray());
        }
    }
}
