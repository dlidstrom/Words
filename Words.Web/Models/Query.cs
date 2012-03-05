namespace Words.Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public class Query
    {
        public string Text { get; set; }
        public int Nodes { get; set; }
        public double ElapsedMilliseconds { get; set; }
    }
}