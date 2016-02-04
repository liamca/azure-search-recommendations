//Copyright (c) 2015 
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is /furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in /all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.

// Data was provided by GrouLens - http://grouplens.org/datasets/hetrec-2011/
// Please refer to this page for details on the licensing of this data
// http://files.grouplens.org/datasets/hetrec2011/hetrec2011-movielens-readme.txt

using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace ImportAzureSearchIndexData
{
    class Program
    {
        public static string SearchServiceName = ConfigurationManager.AppSettings["SearchServiceName"];
        public static string SearchApiKey = ConfigurationManager.AppSettings["SearchApiKey"];

        public static string IndexName = "movies";
        public static SearchServiceClient SearchClient;
        public static SearchIndexClient IndexClient;

        static void Main(string[] args)
        {
            // This sample will Create an Azure Search Index and import data from a set of JSON files
            SearchClient = new SearchServiceClient(SearchServiceName, new SearchCredentials(SearchApiKey));
            IndexClient = SearchClient.Indexes.GetClient(IndexName);
            CreateIndex();
            SearchDocuments(IndexClient, "star wars");

            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();
        }

        public static void CreateIndex()
        {
            SearchClient = new SearchServiceClient(SearchServiceName, new SearchCredentials(SearchApiKey));
            IndexClient = SearchClient.Indexes.GetClient(IndexName);

            try
            {
                if (DeleteIndex(SearchClient, IndexName))
                {
                    // Create index schema for this dataset
                    Console.WriteLine("{0}", "Creating index...\n");
                    var definition = new Index()
                    {
                        Name = IndexName,
                        Fields = new[] 
                    { 
                        new Field("id",             DataType.String)            { IsKey = true,  IsSearchable = false, IsFilterable = true,  IsSortable = false, IsFacetable = false, IsRetrievable = true  },
                        new Field("title",          DataType.String)            { IsKey = false, IsSearchable = true,  IsFilterable = false, IsSortable = false, IsFacetable = false, IsRetrievable = true  },
                        new Field("imdbID",         DataType.Int32)             { IsKey = false, IsSearchable = false, IsFilterable = false, IsSortable = false, IsFacetable = false, IsRetrievable = true  },
                        new Field("spanishTitle",   DataType.String)            { IsKey = false, IsSearchable = true,  IsFilterable = false, IsSortable = false, IsFacetable = false, IsRetrievable = true  },
                        new Field("imdbPictureURL", DataType.String)            { IsKey = false, IsSearchable = false, IsFilterable = false, IsSortable = false, IsFacetable = false, IsRetrievable = true  },
                        new Field("year",           DataType.Int32)             { IsKey = false, IsSearchable = false, IsFilterable = true,  IsSortable = true,  IsFacetable = true,  IsRetrievable = true  },
                        new Field("rtID",           DataType.String)            { IsKey = false, IsSearchable = false, IsFilterable = false, IsSortable = false, IsFacetable = false, IsRetrievable = true  },
                        new Field("rtAllCriticsRating",DataType.Double)         { IsKey = false, IsSearchable = false, IsFilterable = true,  IsSortable = true,  IsFacetable = true,  IsRetrievable = true  },
                        new Field("recommendations",DataType.Collection(DataType.String)) { IsSearchable = true, IsFilterable = true, IsFacetable = true }
                    },
                        Suggesters = new[]
                        {
                            new Suggester("sg", SuggesterSearchMode.AnalyzingInfixMatching, new string[] { "title" })
                        },
                        CorsOptions = new CorsOptions(new string[] { "*" })     // This * option should only be enabled for demo purposes or when you fully trust your users
                    };

                    SearchClient.Indexes.Create(definition);

                    // first apply the changes and if we succeed then record the new version that 
                    // we'll use as starting point next time
                    Console.WriteLine("{0}", "Uploading Content...\n");
                    ApplyData(@"data\movies.dat");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}:\r\n", ex.Message.ToString());
            }
        }

        public static void ApplyData(string fileName)
        {
            List<IndexAction> indexOperations = new List<IndexAction>();
            string line;
            int rowCounter = 0;
            int totalCounter = 0;
            int n;
            double d;

            // Get unique rated movies
            IEnumerable<int> ratedMovies = ImportRatedIDs();

            // Read the file and display it line by line.
            using (StreamReader file = new StreamReader(fileName))
            {
                file.ReadLine();    // Skip header
                while ((line = file.ReadLine()) != null)
                {

                    char[] delimiters = new char[] { '\t' };
                    string[] parts = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    // If it is a rated movie, add it
                    if (ratedMovies.Contains(Convert.ToInt32(parts[0])))
                    {
                        Document doc = new Document();
                        doc.Add("id", Convert.ToString(parts[0]));
                        doc.Add("title", Convert.ToString(parts[1]));
                        doc.Add("imdbID", Convert.ToInt32(parts[2]));
                        doc.Add("spanishTitle", Convert.ToString(parts[3]));
                        doc.Add("imdbPictureURL", Convert.ToString(parts[4]));

                        if (int.TryParse(parts[5], out n))
                            doc.Add("year", Convert.ToInt32(parts[5]));
                        doc.Add("rtID", Convert.ToString(parts[6]));

                        if (double.TryParse(parts[7], out d))
                            doc.Add("rtAllCriticsRating", Convert.ToDouble(parts[7]));

                        indexOperations.Add(new IndexAction(IndexActionType.Upload, doc));
                        rowCounter++;
                        totalCounter++;
                        if (rowCounter == 1000)
                        {
                            IndexClient.Documents.Index(new IndexBatch(indexOperations));
                            indexOperations.Clear();
                            Console.WriteLine("Uploaded {0} docs...", totalCounter);
                            rowCounter = 0;
                        }
                    }
                }
                if (rowCounter > 0)
                {
                    Console.WriteLine("Uploading {0} docs...", rowCounter);
                    IndexClient.Documents.Index(new IndexBatch(indexOperations));
                }

                file.Close();
            }
        }

        public static IEnumerable<int> ImportRatedIDs()
        {
            List<int> ratedMovies = new List<int>();
            string line;

            Console.WriteLine("Getting rated movies...");

            using (StreamReader file = new StreamReader("data\\user_ratedmovies.dat"))
            {
                file.ReadLine();    // Skip header
                while ((line = file.ReadLine()) != null)
                {
                    char[] delimiters = new char[] { '\t' };
                    string[] parts = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    ratedMovies.Add(Convert.ToInt32(parts[1]));
                }
            }

            return (from d in ratedMovies select d).Distinct();

        }


        public static bool DeleteIndex(SearchServiceClient searchClient, string dataset)
        {
            // Delete the index if it exists
            try
            {
                Console.WriteLine("{0}", "Deleting index...\n");
                searchClient.Indexes.Delete(dataset);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting index: {0}\r\n", ex.Message.ToString());
                Console.WriteLine("Did you remember to add your SearchServiceName and SearchServiceApiKey to the app.config?\r\n");
                return false;
            }

            return true;
        }

        private static void SearchDocuments(SearchIndexClient indexClient, string searchText, string filter = null)
        {
            // Execute search based on search text and optional filter
            var sp = new SearchParameters();

            if (!String.IsNullOrEmpty(filter))
            {
                sp.Filter = filter;
            }

            DocumentSearchResponse<Movies> response = indexClient.Documents.Search<Movies>(searchText, sp);
            foreach (SearchResult<Movies> result in response)
            {
                Console.WriteLine("ID: {0} - {1}", result.Document.Id, result.Document.Title);
            }
        }
    }
}
