#nullable enable

namespace Words
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SuccinctTree : ITree
    {
        private readonly RankDirectory directory;
        private readonly Language language;
        private readonly BitString encoding;
        private readonly int encodingBits;
        private readonly BitString letterData;

        public SuccinctTree(
            (string data, int totalBits) encoding,
            (string data, int totalBits) letters,
            Language language)
        {
            this.encoding = new BitString(encoding.data);
            encodingBits = encoding.totalBits;
            letterData = new BitString(letters.data);
            directory = RankDirectory.Create(
                encoding,
                32 * 32,
                32);
            this.language = language;
        }

        public SuccinctTree(SuccinctTreeData data, Language language)
        {
            string encodingJoined = string.Join(string.Empty, data.EncodingBytes);
            encoding = new BitString(encodingJoined);
            encodingBits = data.EncodingBits;
            letterData = new BitString(string.Join(string.Empty, data.LetterBytes));
            directory = RankDirectory.Create(
                (encodingJoined, encodingBits),
                32 * 32,
                32);
            this.language = language;
        }

        public Action<string>? Log { get; set; }

        public SuccinctTreeData GetData()
        {
            SuccinctTreeData data = new(
                encoding.Bytes.ChunkSplit(80).ToArray(),
                encodingBits,
                letterData.Bytes.ChunkSplit(80).ToArray());
            return data;
        }

        public List<string> Matches(string s, int limit)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentException("must be non-empty", nameof(s));
            }

            List<string> matches = new();

            const int pos = 0;
            SuccinctNode node = letterData.GetNode(0);
            Matches(s, string.Empty, pos, node, matches, limit);

            return matches;
        }

        public List<string> NearSearch(string s, int d, int limit)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentException("must be non-empty", nameof(s));
            }

            List<string> matches = new();

            const int pos = 0;
            SuccinctNode node = letterData.GetNode(0);
            NearSearch(s, string.Empty, pos, node, matches, limit, d);

            return matches;
        }

        private void Matches(
            string s,
            string substr,
            int pos,
            SuccinctNode? node,
            List<string> matches,
            int limit)
        {
            if (node == null || (limit > 0 && matches.Count >= limit))
            {
                return;
            }

            (SuccinctNode? center, SuccinctNode? left, SuccinctNode? right) = LoadChildren(node);
            LogImpl(() => $"Visiting {node}, Left = {left?.Character}, Center = {center?.Character}, Right = {right?.Character}");

            char c = pos == s.Length ? default : s[pos];
            if (c < node.Character || WildcardMatchLeft(c, node.Character))
            {
                Matches(s, substr, pos, left, matches, limit);
            }

            if (c == node.Character || WildcardMatch(c, node.Character))
            {
                Matches(s, substr + node.Character, pos + 1, center, matches, limit);
            }

            if (c == default(char) && node.WordEnd)
            {
                matches.Add(substr);
            }

            if (c > node.Character || WildcardMatchRight(c, node.Character))
            {
                Matches(s, substr, pos, right, matches, limit);
            }
        }

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

        private void NearSearch(string s, string substr, int pos, SuccinctNode? node, List<string> matches, int limit, int depth)
        {
            if (node == null || matches.Count >= limit || depth < 0)
            {
                return;
            }

            (SuccinctNode? center, SuccinctNode? left, SuccinctNode? right) = LoadChildren(node);
            char c = default;
            if (pos < s.Length)
            {
                c = s[pos];
            }

            if (depth > 0 || c < node.Character)
            {
                NearSearch(s, substr, pos, left, matches, limit, depth);
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
                    NearSearch(s, substr + node.Character, pos + 1, center, matches, limit, newDepth);
                }
                else
                {
                    NearSearch(s, substr + node.Character, pos, center, matches, limit, newDepth);
                }
            }

            if (depth > 0 || c > node.Character)
            {
                NearSearch(s, substr, pos, right, matches, limit, depth);
            }
        }

        private (SuccinctNode? center, SuccinctNode? left, SuccinctNode? right) LoadChildren(
            SuccinctNode node)
        {
            int sel = directory.Select(node.NodeIndex + 1);
            int firstChild = sel - node.NodeIndex;
            int childOfNextNode = directory.Select(node.NodeIndex + 2, sel, sel + 8) - node.NodeIndex - 1;
            int numberOfChildren = childOfNextNode - firstChild;
            switch (numberOfChildren)
            {
                case 3:
                    {
                        SuccinctNode center = letterData.GetNode(firstChild);
                        SuccinctNode left = letterData.GetNode(firstChild + 1);
                        SuccinctNode right = letterData.GetNode(firstChild + 2);
                        return (center, left, right);
                    }

                case 2:
                    {
                        SuccinctNode center = letterData.GetNode(firstChild);
                        SuccinctNode leftOrRight = letterData.GetNode(firstChild + 1);
                        if (leftOrRight.Character < node.Character)
                        {
                            SuccinctNode left = leftOrRight;
                            return (center, left, null);
                        }

                        SuccinctNode right = leftOrRight;
                        return (center, null, right);
                    }

                case 1:
                    {
                        SuccinctNode child = letterData.GetNode(firstChild);
                        if (node.WordEnd)
                        {
                            if (child.Character < node.Character)
                            {
                                SuccinctNode left = child;
                                return (null, left, null);
                            }

                            if (child.Character > node.Character)
                            {
                                SuccinctNode right = child;
                                return (null, null, right);
                            }
                        }

                        SuccinctNode center = child;
                        return (center, null, null);
                    }

                default:
                    {
                        return (null, null, null);
                    }
            }
        }

        private void LogImpl(Func<string> func)
        {
            Log?.Invoke(func.Invoke());
        }
    }
}