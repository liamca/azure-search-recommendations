// This application will download usage data from Azure Search Analytics
// and then upload the usage data to Azure ML Recommendations to allow
// applications to call for specific item based recommendations

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Configuration;
using System.IO;
using System.Text;
namespace AzureSearchAnalyticsExtraction
{
    class Program
    {
        static void Main(string[] args)
        {
            // Azure Storage Details
            // Set the folder where your operations are stored
            // For example it might look like this "resourceId=/SUBSCRIPTIONS/06454CD3-0DCD-4542-AEC7-237A0B4C0FAE/RESOURCEGROUPS/MYSERVICE-SEARCH/PROVIDERS/MICROSOFT.SEARCH/SEARCHSERVICES/MYSEARCHSERVICE"
            string AzureStorageOperationsFolder = [Operations Folder];
            string IndexName = "movies";
            string MonthToParse = "01";
            string DayToParse = "29";

            
            string StorageAccountName = ConfigurationManager.AppSettings["StorageAccountName"];
            string StorageApiKey = ConfigurationManager.AppSettings["StorageApiKey"];

            DownloadData(StorageApiKey, StorageAccountName, IndexName, MonthToParse, DayToParse);
        }


        public static void DownloadData(string StorageApiKey, string StorageAccountName, string IndexName, string MonthToParse, string DayToParse)
        {
            // This application will take all the Azure Search Traffic Analytics data, and extract the User / Item ID info
            // Configure cloud storage and search connections
            StorageCredentials credentials = new StorageCredentials(StorageAccountName, StorageApiKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(credentials, true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("insights-logs-operationlogs");
            CloudBlobDirectory staDir = container.GetDirectoryReference(AzureStorageOperationsFolder + "/y=2016/m=" + MonthToParse + "/d=" + DayToParse + "/");

            string line, UserID, ItemID;
            int counter = 0;
            int batchcounter = 0;
            StringBuilder sb = new StringBuilder();

            string fileDir = Path.Combine(Directory.GetCurrentDirectory(), "usage.csv");
            if (File.Exists(fileDir))
                File.Delete(fileDir);
            StreamWriter csvData = File.AppendText(fileDir);

            // Iterate through all the operation logs
            var staItems = staDir.ListBlobs(useFlatBlobListing: true);
            foreach (var staItem in staItems)
            {
                if (staItem.Parent.Uri.MakeRelativeUri(staItem.Uri).ToString() == "PT1H.json")
                {
                    Console.WriteLine("Processing {0}", staItem.Parent.Prefix + "PT1H.json");
                    CloudBlockBlob staBlob = container.GetBlockBlobReference(staItem.Parent.Prefix + "PT1H.json");
                    using (var stream = staBlob.OpenRead())     // Reading line by line rather than using a JSON parser since this file can be really big
                    {
                        using (StreamReader file = new StreamReader(stream))
                        {
                            while ((line = file.ReadLine()) != null)
                            {
                                line = line.Trim();
                                // Look for lines like this "properties": { "Description" : "GET /indexes('movies')/docs('1641')" , "Query" : "?userid=13222&api-version=2015-02-28" , "Documents" : 0, "IndexName" : "movies" }
                                if ((line.IndexOf("properties") == 1)
                                    && (line.IndexOf("/indexes('" + IndexName + "')/docs(") > -1)
                                    && (line.IndexOf("userid=") > -1))
                                {

                                    try
                                    {
                                        ItemID = line.Substring(line.IndexOf("docs") + 6);
                                        ItemID = ItemID.Substring(0, FindFirstNonIntChar(ItemID));
                                        UserID = line.Substring(line.IndexOf("userid=") + 7);
                                        UserID = UserID.Substring(0, FindFirstNonIntChar(UserID));

                                        counter++;
                                        batchcounter++;
                                        sb.AppendLine(UserID + "," + ItemID);
                                        // Every batch results to write 10K rows to Blob storage
                                        if (batchcounter == 10000)
                                        {
                                            Console.WriteLine("Writing {0} rows...", batchcounter);
                                            csvData.Write(sb.ToString());
                                            sb = new StringBuilder();
                                            batchcounter = 0;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Error parsing line: {0}", ex.Message.ToString());
                                    }
                                }
                            }
                            // Write anything left
                            if (batchcounter > 0)
                            {
                                Console.WriteLine("Writing {0} rows...", batchcounter);
                                csvData.Write(sb.ToString());
                                Console.WriteLine("Total rows written {0}", counter);
                            }

                        }

                    }
                }

            }

            csvData.Close();

            Console.WriteLine("Complete.  Press any key to continue.");
            Console.ReadLine();
        }

        private static int FindFirstNonIntChar(string ParseString)
        {
            // Find the first non int char
            int n;
            for (int i = 0; i < ParseString.Length; i++)
            {
                if (int.TryParse(ParseString[i].ToString(), out n) == false)
                    return i;

            }
            return ParseString.Length;
        }

    }

}
