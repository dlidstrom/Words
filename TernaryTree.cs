namespace Words
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 
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
    ///     if (*s < p->splitchar)
    ///         return rsearch(p->lokid, s);
    ///     else if (*s > p->splitchar)
    ///         return rsearch(p->hikid, s);
    ///     else {
    ///         if (*s == 0) return 1;
    ///         return rsearch(p->eqkid, ++s);
    ///     }
    /// }
    /// </code>
    /// <code>
    /// int search(char *s)
    /// {
    ///     Tptr p;
    ///     p = root;
    ///     while (p) {
    ///         if (*s < p->splitchar)
    ///             p = p->lokid;
    ///         else if (*s == p->splitchar) {
    ///             if (*s++ == 0)
    ///                 return 1;
    ///             p = p->eqkid;
    ///         } else
    ///             p = p->hikid;
    ///     }
    ///     return 0;
    /// }
    /// </code>
    public class TernaryTree
    {
        private readonly Language language;
        private Node m_root = null;

        public TernaryTree(Language language)
        {
            if (language == null)
                throw new ArgumentNullException("language");
            this.language = language;
        }

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
                throw new ArgumentException();

            Add(s, 0, ref m_root);
        }

        private Node Add(string s, int pos, ref Node node)
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

        public bool Contains(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException();

            int pos = 0;
            Node node = m_root;
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

            int pos = 0;
            Node node = m_root;
            Matches(s, string.Empty, pos, node, matches, limit);

            return matches;
        }

        /// <summary>
        /// </summary>
        /// <code>
        /// void pmsearch(Tptr p, char *s)
        /// {
        ///     if (!p) return;
        ///     nodecnt++;
        ///     if (*s == '.' || *s < p->splitchar)
        ///         pmsearch(p->lokid, s);
        ///     if (*s == '.' || *s == p->splitchar)
        ///         if (p->splitchar && *s)
        ///             pmsearch(p->eqkid, s+1);
        ///     if (*s == 0 && p->splitchar == 0)
        ///         srcharr[srchtop++] = (char *) p->eqkid;
        ///     if (*s == '.' || *s > p->splitchar)
        ///         pmsearch(p->hikid, s);
        /// }
        /// </code>
        /// <param name="s"></param>
        /// <param name="substr"></param>
        /// <param name="pos"></param>
        /// <param name="node"></param>
        /// <param name="matches"></param>
        private void Matches(string s, string substr, int pos, Node node, List<string> matches, int limit)
        {
            if (node == null || matches.Count >= limit)
                return;

            char c = pos == s.Length ? default(char) : s[pos];
            if (c < node.Char || WildcardMatch(c, node.Char))
                Matches(s, substr, pos, node.Left, matches, limit);

            if (c == node.Char || WildcardMatch(c, node.Char))
                Matches(s, substr + node.Char, pos + 1, node.Center, matches, limit);

            if (c == default(char) && node.Char == default(char))
                matches.Add(substr);

            if (c > node.Char || WildcardMatch(c, node.Char))
                Matches(s, substr, pos, node.Right, matches, limit);
        }

        private bool WildcardMatch(char c, char node)
        {
            return c == '?'
                || (c == '#' && language.Consonants.Contains(node))
                || (c == '@' && language.Vowels.Contains(node));
        }

        /// <summary>
        /// void traverse(Tptr p)
        /// {
        ///     if (!p) return;
        ///     traverse(p->lokid);
        ///     if (p->splitchar)
        ///         traverse(p->eqkid);
        ///     else
        ///         printf("%s/n", (char *) p->eqkid);
        ///     traverse(p->hikid);
        /// }
        /// </summary>
        public void Traverse()
        {
        }

        public List<string> NearSearch(string s, int d = 1, int limit = 100)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException();

            var matches = new List<string>();

            int pos = 0;
            Node node = m_root;
            NearSearch(s, string.Empty, pos, node, matches, limit, d);

            return matches;
        }

        /// <summary>
        /// 
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
        private void NearSearch(string s, string substr, int pos, Node node, List<string> matches, int limit, int depth)
        {
            if (node == null || matches.Count >= limit || depth < 0)
                return;

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
    }
}
