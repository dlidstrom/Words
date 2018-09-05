namespace Words
{
    using System.Collections.Generic;

    public interface ITree
    {
        List<string> Matches(string s, int limit = 100);

        List<string> NearSearch(string s, int d = 1, int limit = 100);
    }
}