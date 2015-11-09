using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportAzureSearchIndexData
{
    [SerializePropertyNamesAsCamelCase]
    public class Movies
    {
        public string Id { get; set; }

        public string Title { get; set; }

    }
}
