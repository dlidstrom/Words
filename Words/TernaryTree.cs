#nullable enable

namespace Words
{
    using System;
    using System.Collections.Generic;

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
    public class TernaryTree : ITree
    {
        private readonly Language language;
        private readonly List<Node> nodes = new();
        private Node? root;

        /// <summary>
        /// Initializes a new instance of the TernaryTree class.
        /// </summary>
        /// <param name="language"></param>
        public TernaryTree(Language language)
        {
            this.language = language ?? throw new ArgumentNullException(nameof(language));
        }

        public int Nodes { get; private set; }

        public Action<string> Log { get; set; } = s => { };

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
            {
                throw new ArgumentException("requires non-empty", nameof(s));
            }

            _ = Add(s, 0, ref root);
        }

        /// <summary>
        /// Add words to tree.
        /// </summary>
        /// <param name="s"></param>
        public void Add(params string[] s)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            foreach (string item in s)
            {
                Add(item);
            }
        }

        public bool Contains(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentException("requires non-empty", nameof(s));
            }

            int pos = 0;
            Node? node = root;
            while (node != null)
            {
                if (s[pos] < node.Character)
                {
                    node = node.Left;
                }
                else if (s[pos] > node.Character)
                {
                    node = node.Right;
                }
                else
                {
                    if (++pos == s.Length)
                    {
                        return node.WordEnd;
                    }

                    node = node.Center;
                }
            }

            return false;
        }

        public List<string> Matches(string s, int limit)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentException("requires non-empty", nameof(s));
            }

            List<string> matches = new();

            const int pos = 0;
            Node? node = root;
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
            {
                throw new ArgumentNullException(nameof(action));
            }

            Traverse(root, action, string.Empty);
        }

        private void Traverse(Node? node, Action<string> action, string s)
        {
            if (node == null)
            {
                return;
            }

            Traverse(node.Left, action, s);
            if (node.WordEnd)
            {
                action.Invoke(s);
            }
            else
            {
                Traverse(node.Center, action, s + node.Character);
            }

            Traverse(node.Right, action, s);
        }

        public List<string> NearSearch(string s, int d, int limit)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentException("requires non-empty", nameof(s));
            }

            List<string> matches = new();

            const int pos = 0;
            Node? node = root;
            Nodes = 0;
            NearSearch(s, string.Empty, pos, node, matches, limit, d);

            return matches;
        }

        private Node Add(string s, int pos, ref Node? node)
        {
            char c = pos == s.Length ? default : s[pos];
            if (node == null)
            {
                node = new Node { Character = c, WordEnd = false };
                nodes.Add(node);
            }

            if (c < node.Character)
            {
                node.Left = Add(s, pos, ref node.Left);
            }
            else if (c == node.Character)
            {
                if (pos != s.Length)
                {
                    node.Center = Add(s, pos + 1, ref node.Center);
                }
                else
                {
                    node.WordEnd = true;
                }
            }
            else
            {
                node.Right = Add(s, pos, ref node.Right);
            }

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
        private void Matches(string s, string substr, int pos, Node? node, List<string> matches, int limit)
        {
            if (node == null || matches.Count >= limit)
            {
                return;
            }

            Log($"Visiting {node}");
            Nodes++;

            char c = pos == s.Length ? default : s[pos];
            if (WildcardMatchLeft(c, node.Character) || c < node.Character)
            {
                Matches(s, substr, pos, node.Left, matches, limit);
            }

            if (WildcardMatch(c, node.Character) || c == node.Character)
            {
                Matches(s, substr + node.Character, pos + 1, node.Center, matches, limit);
            }

            if (c == default(char) && node.WordEnd)
            {
                matches.Add(substr);
            }

            if (WildcardMatchRight(c, node.Character) || c > node.Character)
            {
                Matches(s, substr, pos, node.Right, matches, limit);
            }
        }

        /// <summary>
        /// Determines if search should traverse the center node.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool WildcardMatch(char c, char node)
        {
            return c switch
            {
                '?' => true,
                '#' => language.Consonants.Contains(node),
                '@' => language.Vowels.Contains(node),
                _ => false
            };
        }

        /// <summary>
        /// Determines if search should traverse the left node.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool WildcardMatchLeft(char c, char node)
        {
            return c switch
            {
                '?' => true,
                '#' => language.Consonants.Min <= node,
                '@' => language.Vowels.Min <= node,
                _ => false
            };
        }

        /// <summary>
        /// Determines if search should traverse the right node.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool WildcardMatchRight(char c, char node)
        {
            return c switch
            {
                '?' => true,
                '#' => node <= language.Consonants.Max,
                '@' => node <= language.Vowels.Max,
                _ => false
            };
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
        private void NearSearch(string s, string substr, int pos, Node? node, List<string> matches, int limit, int depth)
        {
            if (node == null || matches.Count >= limit || depth < 0)
            {
                return;
            }

            Nodes++;

            char c = default;
            if (pos < s.Length)
            {
                c = s[pos];
            }

            if (depth > 0 || c < node.Character)
            {
                NearSearch(s, substr, pos, node.Left, matches, limit, depth);
            }

            if (node.WordEnd)
            {
                if (s.Length - pos <= depth)
                {
                    matches.Add(substr);
                }
            }
            else
            {
                int newDepth = c == node.Character ? depth : depth - 1;
                if (c != default(char))
                {
                    NearSearch(s, substr + node.Character, pos + 1, node.Center, matches, limit, newDepth);
                }
                else
                {
                    NearSearch(s, substr + node.Character, pos, node.Center, matches, limit, newDepth);
                }
            }

            if (depth > 0 || c > node.Character)
            {
                NearSearch(s, substr, pos, node.Right, matches, limit, depth);
            }
        }

        public SuccinctTree EncodeSuccinct()
        {
            if (root is null)
            {
                throw new ApplicationException("Empty tree");
            }

            BitWriter encodingWriter = new();
            BitWriter letterWriter = new();
            encodingWriter.Write(0x02, 2);
            Queue<Node> queue = new();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                int children = 0;
                Node node = queue.Dequeue();

                if (node.Center != null)
                {
                    queue.Enqueue(node.Center);
                    children++;
                }

                if (node.Left != null)
                {
                    queue.Enqueue(node.Left);
                    children++;
                }

                if (node.Right != null)
                {
                    queue.Enqueue(node.Right);
                    children++;
                }

                for (int i = 0; i < children; i++)
                {
                    encodingWriter.Write(1, 1);
                }

                encodingWriter.Write(0, 1);
                letterWriter.Write(node);
            }

            (string data, int totalBits) encoding = encodingWriter.GetData();
            int expectedBits = (2 * nodes.Count) + 1;
            if (encoding.totalBits != expectedBits)
            {
                string message = string.Format(
                    "Unexpected number of bits. Expected 2 * nodes.Count + 1 = {0} but got {1}.",
                    expectedBits,
                    encoding.totalBits);
                throw new ApplicationException(message);
            }

            (string data, int totalBits) letterData = letterWriter.GetData();
            SuccinctTree tree = new(
                encoding,
                letterData,
                language);
            return tree;
        }
    }
}
