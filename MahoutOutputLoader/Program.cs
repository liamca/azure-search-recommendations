using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahoutOutput_Loader
{
    class Program
    {
        public static string SearchServiceName = [Azure Search Service - Exclude .search.windows.net];
        public static string SearchApiKey = [Azure Search API Key];
        public static string StorageApiKey = [Azure Storage API Key];
        public static string StorageAccountName = [Azure Storage Account Name];

        public static string IndexName = "movies";
        public static string StorageContainer = "movies";

        public static SearchServiceClient serviceClient;
        public static SearchIndexClient indexClient;

        static void Main(string[] args)
        {
            // Ingest data from Mahout outout and merge into Azure Search
            string line;
            int currentID = -1;
            int docCounter = 0;
            List<Recommendation> recList = new List<Recommendation>();
            List<SearchIndexSchema> sisList = new List<SearchIndexSchema>();
            List<string> recs = new List<string>();

            // Configure cloud storage and search connections
            StorageCredentials credentials = new StorageCredentials(StorageAccountName, StorageApiKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(credentials, true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(StorageContainer);
            CloudBlockBlob blob = container.GetBlockBlobReference("output\\part-r-00000");  // In large implementations there may be multiple part files
            serviceClient = new SearchServiceClient(SearchServiceName, new SearchCredentials(SearchApiKey));
            indexClient = serviceClient.Indexes.GetClient(IndexName);

            // Open and parse mahout output file 
            using (var stream = blob.OpenRead())
            {
                using (StreamReader file = new StreamReader(stream))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        Recommendation rec = new Recommendation();
                        char[] delimiters = new char[] { '\t' };
                        string[] parts = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                        rec.itemID = Convert.ToInt32(parts[0]);
                        rec.recItemID = Convert.ToInt32(parts[1]);
                        rec.percentSimilar = Convert.ToDouble(parts[2]);
                        if (currentID != rec.itemID)
                        {
                            docCounter++;
                            if (recList.Count > 0)
                            {
                                recList = recList.OrderByDescending(w => w.percentSimilar).Take(5).ToList();    // Take the 5 most similar items
                                foreach (var item in recList)
                                    recs.Add(item.recItemID.ToString());
                                recList.Clear();
                                sisList.Add(new SearchIndexSchema { id = currentID.ToString(), recommendations = recs.ToArray() });
                                recs.Clear();
                                if (sisList.Count == 500)
                                {
                                    MergeDocument(sisList);
                                    sisList.Clear();
                                    Console.WriteLine("{0} Docs written to Index...", docCounter - 1);
                                }
                            }
                            currentID = rec.itemID;
                        }
                        recList.Add(rec);
                    }
                    file.Close();
                    if (sisList.Count > 0)
                    {
                        MergeDocument(sisList);
                        sisList.Clear();
                        Console.WriteLine("{0} Docs written to Index...", docCounter - 1);
                    }
                }
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();
        }

        private static void MergeDocument(List<SearchIndexSchema> recList)
        {
            // Merge recommendations into Azure Search with existing documents
            try
            {
                indexClient.Documents.Index(IndexBatch.Create((recList.Select(doc => IndexAction.Create(IndexActionType.MergeOrUpload, doc)))));
            }
            catch (IndexBatchException e)
            {
                Console.WriteLine("Failed to index some of the documents: {0}",
                    String.Join(", ", e.IndexResponse.Results.Where(r => !r.Succeeded).Select(r => r.Key)));
            }
        }

    }
}
