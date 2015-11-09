using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahoutOutput_Loader
{
    public class Recommendation
    {
        public int itemID { get; set; }
        public int recItemID { get; set; }
        public double percentSimilar { get; set; }
    }

    public class SearchIndexSchema
    {
        public string id { get; set; }

        public string[] recommendations { get; set; }
    }
}
