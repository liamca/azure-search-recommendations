using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimulateAzureSearchLookupRequests
{
    class Program
    {
        public static string SearchServiceName = ConfigurationManager.AppSettings["SearchServiceName"];
        public static string SearchApiKey = ConfigurationManager.AppSettings["SearchApiKey"];

        private static Uri _serviceUri;
        private static HttpClient _httpClient;
        public static string errorMessage;

        private const int TaskCount = 10;

        static void Main(string[] args)
        {
            //Load in the user simulation file line by line to make search requests
            int counter = 0;
            string line;

            _serviceUri = new Uri("https://" + SearchServiceName + ".search.windows.net");
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("api-key", SearchApiKey);


            var tasks = new Task[TaskCount];

            int index = 0;
            System.IO.StreamReader file = new System.IO.StreamReader("userrequests.txt");
            while ((line = file.ReadLine()) != null)
            {
                counter++;
                List<int> UserRequest = line.Split(',').Select(int.Parse).ToList();

                tasks[index] = Task.Factory.StartNew(() => ExecLookup(UserRequest));
                index++;

                if (index == 10)
                {
                    Task.WaitAll(tasks);
                    Console.WriteLine("Completed Requests");
                    index = 0;
                    tasks = new Task[TaskCount];

                }

                //ExecLookup(UserRequest);
                Console.WriteLine("Request: {0}", counter);


            }

            file.Close();

            // Suspend the screen.
            Console.ReadLine();
        }

        
        static void ExecLookup(List<int> UserRequest)
        {
            try
            {
                // Pass the specified suggestion text and return the fields
                Uri uri = new Uri(_serviceUri, "/indexes/movies/docs('" + UserRequest[1].ToString() + "')?userid=" + UserRequest[0].ToString());

                HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(_httpClient, HttpMethod.Get, uri);
                AzureSearchHelper.EnsureSuccessfulSearchResponse(response);
                var option = AzureSearchHelper.DeserializeJson<dynamic>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message.ToString());
            }
        }

    }

}
