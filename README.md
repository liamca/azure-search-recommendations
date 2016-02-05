---
services: search
platforms: dotnet
author: liamca
---

# Azure Search Recommendations Sample

This sample shows how to add recommendations to an [Azure Search](https://azure.microsoft.com/en-us/services/search/) index.  

![Demo Screen Shot](https://raw.githubusercontent.com/liamca/azure-search-recommendations/master/product_recommendations.png)

# Give it a Try

Give the [following demo](https://rawgit.com/liamca/azure-search-recommendations/master/WebSite/demo/index.html) a try to see what we will be building by searching for a movie and then click on the movie to see recommendations.

## What is a Recommendation?

Recommendations is a technique for surfacing more items from a catalog based on existing search User activity (such as web logs) used to recommend items and improve conversion.  Recommendation engines are often trained using past customer activity or by collecting data directly from digital store
Common recommendations include: 
- <b>Frequently Bought Together</b>: a customer who bought this also bought that
- <b>Customer to Item Recommendations</b>: a customer like you also bought that

## Creating the Azure Search Index

- Open the solution AzureSearchMovieRecommendations.sln and set ImportAzureSearchIndexData as the Default Project.  
- Open App.config within ImportAzureSearchIndexData and update the SearchServiceName and SearchApiKey to point to your Azure Search service
- Download hetrec2011-movielens-2k.zip from http://grouplens.org/datasets/hetrec-2011/ and copy the files Movies.dat & user_ratedmovies.dat to the \ImportAzureSearchIndexData\data
- Buid &Run the project to create an index and load Movie data 
- At the end, the application will execute a test search

## Create a simple HTML application to Search Movies

Open the search.js file within \WebSite\starter-template-complete and update the apiKey and azureSearchService with your Azure Search service details.

You should be able to open this file in a browser such as Chrome to now view movies by typing into the search box.

## Getting Usage Data using Azure Search Traffic Analytics
Azure Search Traffic Analytics is a feature of Azure Search that tracks all activity against your search service.  This includes statistic as well as details of individual search operations.  You can learn more about this feature in my [video](https://channel9.msdn.com/Shows/Data-Exposed/Custom-Analyzers-Search-Analytics--Portal-Querying-in-Azure-Search) as well as the following [blog](https://azure.microsoft.com/en-us/documentation/articles/search-traffic-analytics/).

For this demo, we will be extracting all of the individual document lookups.  This would happen when a user see's search results and then clicks on an individual result to get details.  An example of a lookup of a document with ID 30 might look like the following.
<pre><code>
https://azs-playground.search.windows.net/indexes/movies/docs('32')?api-version=2015-02-28&userid=75
</code></pre>
Notice how this example also appends a parameter userid.  This has no impact on the search results, but it is important for our demo because it allows us to uniquely identify which users looked at which items, and will be later used in our recommendations analysis.

In the Search Traffic Analytics, this operation will be logged and will look like this:
<pre><code>
{
	"records": 
	[
		{
			 "time": "2016-01-25T23:00:00.0113871Z",
			 "resourceId": "/SUBSCRIPTIONS/[REDACTED]/RESOURCEGROUPS/[REDACTED]/PROVIDERS/MICROSOFT.SEARCH/SEARCHSERVICES/[REDACTED]",
			 "operationName": "Query.Lookup",
			 "operationVersion": "2015-02-28",
			 "category": "OperationLogs",
			 "resultType": "Success",
			 "resultSignature": 200,
			 "durationMS": 2,
			 "properties": { "Description" : "GET /indexes('movies')/docs('30')" , "Query" : "?userid=75&api-version=2015-02-28" , "Documents" : 0, "IndexName" : "movies" }
		}
	]
}
</code></pre>

Ultimately, our goal will be to extract each UserID / ItemID values from all of this lookup data.  But first, let's simulate a ton of user lookups.

### Enable Search Traffic Analytics

At this point you will need to turn on Search Traffic Analytics for your Azure Search service.  If you are not familiar with how to do this, please refer to my [video](https://channel9.msdn.com/Shows/Data-Exposed/Custom-Analyzers-Search-Analytics--Portal-Querying-in-Azure-Search) or the following [blog](https://azure.microsoft.com/en-us/documentation/articles/search-traffic-analytics/).

### Simulating User Lookups

To simulate these user requests you can run the SimulateAzureSearchLookupRequests project from within the AzureSearchMovieRecommendations Solution.  Before running, open App.config within this project and update the SearchServiceName and SearchApiKey to point to your Azure Search service.  

This process will take some time since it is simulating over 600,000 users lookups.  

### Choosing a Recommendation Option

There are numerous ways that item recommendations can be created using this usage data.  The two options I prefer are [Azure ML Recommendations API](http://datamarket.azure.com/dataset/amla/recommendations) and [Apache Mahout](http://mahout.apache.org/).  Both options I feel have strengths and weaknesses which often direct you to use one over the other such as:

<b>Azure ML Recommendations API</b>
- Very simple to use either through the .NET API or through their [UI interface](http://recommendations.azurewebsites.net)
- Provides really good recommendations without the need for deep Machine Learning experience
- Currently limited to 100,000 items (meaning in our example, no more than 100,000 movies)
- Pricing can get expensive if you send large numbers of recommendation requests 

<b>Apache Mahout</b>
- Able to support extremely large numbers of items 
- Provides good recomendations out of the box, but can be complex to improve without Machine Learning experience
- Can be a good option for Java developers
- Can be difficult to operationalize

My personal preference is to use the Azure ML Recommendations API if my content size fits their limits because it is much simpler (IMO) to use and operationalize.  I will focus the following sections on how to do this, but if you would prefer to do this using Machout, please take a [look here](https://github.com/liamca/azure-search-recommendations/tree/master/MahoutOutputLoader).

### Extracting User Data for Recommendations

Now that we have simulated a huge number of user search requests, we will want to export this data into a csv file (which we will call usage.csv) that is in format userId, productId indicating that the user of id userId looked at an item with id of productId. For example:

<pre><code>
78,1391
78,1193
78,1653
127,2013
127,2719
127,5523
</code></pre>

To do this, within the AzureSearchAnalyticsExtraction project, configure the Storage Account information in the app.config as well as the folder where the container.GetDirectoryReference can find your search operations logs.

## Uploading Usage Data to Azure ML Recommendations API

In the previous step, we created a usage.csv file from Azure Search Traffic Analytics.  To do this, we will use the AzureMLRecommendations project.  Before running this project, you will need to do the following:

- Register for an Azure ML Recommendations API account [here](http://datamarket.azure.com/dataset/amla/recommendations)
- Get the Azure ML Recommendations Email & Account Key and update the AzureMLRecommendations project with these values
- Run this project and make note of the Model ID and Build ID that are created as you will use this in the Web Site to call recommendations

## Visualizing the Recommendations

At this point you should be able to go back to the web application and update it with the Model ID and Build ID from the previous step in the Search.js file.

<pre><code>
var azureMLModelId = ""
var azureMLBuildId = ""
</code></pre>

If you want to see how the recommendations were returned when you clicked on this image, open Search.js and look at the openMovieDetails() function.

At this point you should be able to launch the web page, search for movies and return recommendations.

## Operationalizing the Process
To this point we have been running console applications to extract the usage data and then update the Azure ML Recommendations engine.  A good way to operationalize this is by publishing the code as an [Azure WebJob](https://azure.microsoft.com/en-us/documentation/articles/web-sites-create-web-jobs/) to allow yourself to schedule this execution.

## Credit

Data was provided by GrouLens (http://grouplens.org/datasets/hetrec-2011/)

Please refer to this page for details on the licensing of this data: http://files.grouplens.org/datasets/hetrec2011/hetrec2011-movielens-readme.txt


