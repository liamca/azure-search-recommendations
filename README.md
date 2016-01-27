---
services: search
platforms: dotnet
author: liamca
---

# Azure Search Recommendations Sample

This sample shows how to easily add recommendations to an Azure Search index.  

![Demo Screen Shot](https://raw.githubusercontent.com/liamca/azure-search-recommendations/master/product_recommendations.png)

## What is a Recommendation?

Recommendations is a technique for surfacing more items from a catalog based on existing search User activity (such as web logs) used to recommend items and improve conversion.  
Recommendation engines often trained using past customer activity or by collecting data directly from digital store
Common recommendations include: 
- Frequently Bought Together: a customer who bought this also bought that
- Customer to Item recommendations: a customer like you also bought that

## Creating the Azure Search Index

- Open the solution AzureSearchMovieRecommendations.sln and set ImportAzureSearchIndexData as the Default Project.  
- Open Program.cs within ImportAzureSearchIndexData and alter SearchServiceName and SearchApiKey to point to your Azure Search service
- Download hetrec2011-movielens-2k.zip from http://grouplens.org/datasets/hetrec-2011/ and copy the files Movies.dat & user_ratedmovies.dat to the \ImportAzureSearchIndexData\data
- Run the project to create an index and load Movie data 
- At the end, the application will execute a test search

## Create a simple HTML application to Search Movies

A completed JavaScript web application to allow you to Query the Azure Search index can be found: 
\WebSite\starter-template-complete

If you would like to walk through the demo from scratch, the original CSS can be found here:
\WebSite\starter-template

Open the search.js file within \WebSite\starter-template-complete and update the apiKey and azureSearchService with your Azure Search service details.

You should be able to open this file in a browser such as Chrome to now view movies by typing into the search box.

## Command for executing Creating Recommendations using Mahout

- Upload the file data\movie_usage.txt to Azure Blob Storage 
- I used Azyre HDInsight to create a Hadoop version 2.7.0  (HDI 3.3) cluster on Windows which you might want to also created because Mahout can act very differntly from version to version.
- Create an HDInsight instance (enabling Remote Desktop) and connect to the machine through Remote Desktop (available from the Azure Portal).  
- From the HDInsight machine, open the "Hadoop Command Line"
- Change to the Mahout bin directory under c:\apps\dist.  Mine looks like this, but you may get a more recent version of Mahout
	C:\apps\dist\mahout-1.0.0.2.3.3.0-2992\bin
- Execute the following command line where you replace the [CONTAINER] & [STORAGEACT] with your Azure Storage details (where you placed the movie_usage.txt file):

mahout itemsimilarity -s SIMILARITY_COSINE --input "wasb://[CONTAINER]@[STORAGEACT].blob.core.windows.net/movie_usage.txt" --output "wasb://[CONTAINER]@[STORAGEACT].blob.core.windows.net/output/" --tempDir "wasb://[CONTAINER]@[STORAGEACT].blob.core.windows.net/temp" -m 5

This should take quite a few minutes to complete, but when it does, your storage container should contain the following file which will include your movie recommendataions:
/movies/output/part-r-00000

This file has 3 columns: [Item ID of Movie], [Item ID of Recommendations related to this Movie], [Similarity Percentage]

## Importing Data from Mahout to Azure Search

The application that created the Azure Search index, had also created a field called Recommendations which is of type Collection (which is like a comma separated set of strings).  We will merge the data created in the previous step with this Azure Search index.  

- From the Visual Studio solution AzureSearchMovieRecommendations.sln, open Program.cs within MahoutOutputLoader.
- Update SearchServiceName and SearchApiKey with your Azure Search service details
- Update StorageApiKey and StorageAccountName with your Azure Storage account details for which you stored your Mahout product recommendations file
- Run the application to merge the data
 
## Visualizing the Recommendations
At this point you should be able to go back to the web application and click on any of the movies to see recommendations.

If you want to see how the recommendations were returned when you clicked on this image, open Search.js and look at the openMovieDetails() function.

## Credit

Data was provided by GrouLens (http://grouplens.org/datasets/hetrec-2011/)

Please refer to this page for details on the licensing of this data: http://files.grouplens.org/datasets/hetrec2011/hetrec2011-movielens-readme.txt


