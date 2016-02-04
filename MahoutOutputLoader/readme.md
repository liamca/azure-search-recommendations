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

## Command to Create Recommendations using Mahout

- Upload the file data\movie_usage.txt to Azure Blob Storage 
- I used Azure HDInsight to create a Hadoop version 2.7.0  (HDI 3.3) cluster on Windows which you might want to also created because Mahout can act very differently from version to version.
- Create an HDInsight instance (enabling Remote Desktop) and connect to the machine through Remote Desktop (available from the Azure Portal).  
- From the HDInsight machine, open the "Hadoop Command Line"
- Change to the Mahout bin directory under c:\apps\dist.  Mine looks like this, but you may get a more recent version of Mahout
	C:\apps\dist\mahout-1.0.0.2.3.3.0-2992\bin
- Execute the following command line where you replace the [CONTAINER] & [STORAGEACT] with your Azure Storage details (where you placed the movie_usage.txt file):

<pre><code>
mahout itemsimilarity 
   -s SIMILARITY_COSINE 
   --input "wasb://[CONTAINER]@[STORAGEACT].blob.core.windows.net/movie_usage.txt" 
   --output "wasb://[CONTAINER]@[STORAGEACT].blob.core.windows.net/output/" 
   --tempDir "wasb://[CONTAINER]@[STORAGEACT].blob.core.windows.net/temp" 
   -m 5
</code></pre>

This should take quite a few minutes to complete, but when it does, your storage container should contain the following file which will include your movie recommendations:
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

## Operationalizing the Process
Keeping an HDInsight cluster running all the time can be fairly expensive, so it is better to simply launch a new instance whenever you want to update the recommendations in your Azure Search service.  There are numerous ways to do this, but one option is to use Azure Data Factory, where you can create a scheduled process to run this.  You can learn more about how to do this, [here](https://azure.microsoft.com/en-in/documentation/articles/data-factory-map-reduce/).
