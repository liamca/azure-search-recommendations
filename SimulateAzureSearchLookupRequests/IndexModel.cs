using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulateAzureSearchLookupRequests
{
    public class Movies
    {
        public String id {get; set;}
        public String title {get; set;}
        public Int32 imdbID {get; set;}
        public String spanishTitle {get; set;}
        public String imdbPictureURL {get; set;}
        public Int32 year {get; set;}
        public String rtID {get; set;}
        public Double rtAllCriticsRating {get; set;}
        public String[] recommendations {get; set;}

    }
}
