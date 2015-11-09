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

namespace ImportAzureSearchIndexData
{
    class Program
    {
        public static string SearchServiceName = [Azure Search Service - Exclude .search.windows.net];
        public static string SearchApiKey = [Azure Search API Key];

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
                        new Field("actorTags",DataType.Collection(DataType.String)) { IsSearchable = true, IsFilterable = true, IsFacetable = true },
                        new Field("genreTags",DataType.Collection(DataType.String)) { IsSearchable = true, IsFilterable = true, IsFacetable = true },
                        new Field("movieTags",DataType.Collection(DataType.String)) { IsSearchable = true, IsFilterable = true, IsFacetable = true },
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
                    ApplyData(@"data\movies1.json");
                    ApplyData(@"data\movies2.json");
                    ApplyData(@"data\movies3.json");
                    ApplyData(@"data\movies4.json");
                    ApplyData(@"data\movies5.json");
                    ApplyData(@"data\movies6.json");
                    ApplyData(@"data\movies7.json");
                    ApplyData(@"data\movies8.json");
                    ApplyData(@"data\movies9.json");
                    ApplyData(@"data\movies10.json");
                    ApplyData(@"data\movies11.json");
                    ApplyData(@"data\movies12.json");
                    ApplyData(@"data\movies13.json");
                    ApplyData(@"data\movies14.json");
                    ApplyData(@"data\movies15.json");
                    ApplyData(@"data\movies16.json");
                    ApplyData(@"data\movies17.json");
                    ApplyData(@"data\movies18.json");
                    ApplyData(@"data\movies19.json");
                    ApplyData(@"data\movies20.json");
                    ApplyData(@"data\movies21.json");

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
            IndexAction ia = new IndexAction();
            object session = Newtonsoft.Json.JsonConvert.DeserializeObject(System.IO.File.ReadAllText(fileName));
            JArray docArray = (JArray)(session);
            foreach (var document in docArray)
            {
                Document doc = new Document();
                doc.Add("id", Convert.ToString(document["id"]));
                doc.Add("title", Convert.ToString(document["title"]));
                doc.Add("imdbID", Convert.ToInt32(document["imdbID"]));
                doc.Add("spanishTitle", Convert.ToString(document["spanishTitle"]));
                doc.Add("imdbPictureURL", Convert.ToString(document["imdbPictureURL"]));
                doc.Add("year", Convert.ToInt32(document["year"]));
                doc.Add("rtID", Convert.ToString(document["rtID"]));
                doc.Add("rtAllCriticsRating", Convert.ToDouble(document["rtAllCriticsRating"]));
                doc.Add("actorTags", (JArray)document["actorTags"]);
                doc.Add("genreTags", (JArray)document["genreTags"]);
                doc.Add("movieTags", (JArray)document["movieTags"]);

                indexOperations.Add(new IndexAction(IndexActionType.Upload, doc));
            }
            Console.WriteLine("Uploading {0}...\n", fileName);

            IndexClient.Documents.Index(new IndexBatch(indexOperations));
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
