//Copyright 2014 Microsoft
//Licensed under the MIT License

//Based on sample code provided at https://github.com/twitter/typeahead.js
//Data and Images provided by Wikipedia - http://en.wikipedia.org/wiki/List_of_vegetables 

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;
using System.Text;

namespace SimulateAzureSearchLookupRequests
{
    public class AzureSearchHelper
    {
        // This sample uses the prototype API.  Please review the documenation for details on this API version - http://msdn.microsoft.com/en-us/library/azure/dn864560.aspx
        public const string ApiVersionString = "api-version=2015-02-28";

        private static readonly JsonSerializerSettings _jsonSettings;

        static AzureSearchHelper()
        {
            _jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented, // for readability, change to None for compactness
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

            _jsonSettings.Converters.Add(new StringEnumConverter());
        }

        public static string SerializeJson(object value)
        {
            return JsonConvert.SerializeObject(value, _jsonSettings);
        }

        public static T DeserializeJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _jsonSettings);
        }

        public static HttpResponseMessage SendSearchRequest(HttpClient client, HttpMethod method, Uri uri, string json = null)
        {
            UriBuilder builder = new UriBuilder(uri);
            string separator = string.IsNullOrWhiteSpace(builder.Query) ? string.Empty : "&";
            builder.Query = builder.Query.TrimStart('?') + separator + ApiVersionString;

            var request = new HttpRequestMessage(method, builder.Uri);

            if (json != null)
            {
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return client.SendAsync(request).Result;
        }

        public static void EnsureSuccessfulSearchResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string error = response.Content == null ? null : response.Content.ReadAsStringAsync().Result;
                throw new Exception("Search request failed: " + error);
            }
        }
    }
}