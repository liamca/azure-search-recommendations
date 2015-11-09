## Loading Data into Azure SQL / SQL Server database

To load the data into the SQL database:
- Create a database 
- Create a Movies table using the file: data\sql_table_create.sql
- Import the data from the Tab delimited file (sql_movie_data.txt) using BCP from command line with a command similar to the following:
	bcp Movies in data\sql_movie_data.txt -S [AZURESQLSERVER].database.windows.net -d [AZURESQLDATABASENAME] -U [SQLUSERNAME] -P [SQLPASSWORD] -t "\t" -c

## Code to add to the Bootstrap Template

The completed JavaScript code to allow you to Query the Azure Search index can be found: 
\JavaScipt.src\bootstrap-3.3.5\docs\examples\starter-template - Complete

If you would like to walk through the demo from scratch, the original CSS can be found here:
\JavaScipt.src\bootstrap-3.3.5\docs\examples\starter-template

If you do not wish to create your own Azure Search service, you can use the one included in the "starter-template - Complete" directory that provides a Query API key, allowing you to execute queries but not make changes to the Azure Search index.

## Command for executing Mahout Recommendations

- Upload the file data\movie_usage.txt to Blob Storage 
- Create an HDInsight instance (enabling Remote Desktop) and connect to the machine through Remote Desktop (available from the Azure Portal)
- From the HDInsight machine, open the "Hadoop Command Line"
- Change to the Mahout bin directory under c:\apps\dist.  Mine looks like this, but you may get a more recent version of Mahout
	C:\apps\dist\mahout-1.0.0.2.3.3.0-2992\bin
- Execute the following command line where you replace the [CONTAINER] & [STORAGEACT] with your Azure Storage details (where you placed the movie_usage.txt file):

mahout itemsimilarity -s SIMILARITY_COSINE --input "wasb://[CONTAINER]@[STORAGEACT].blob.core.windows.net/movie_usage.txt" --output "wasb://[CONTAINER]@[STORAGEACT].blob.core.windows.net/output/" --tempDir "wasb://[CONTAINER]@[STORAGEACT].blob.core.windows.net/temp" -m 5

This should take quite a few minutes to complete, but when it does, your storage container should contain the following file which will include your movie recommendataions:
/movies/output/part-r-00000

This file has 3 columns: [Item ID of Movie], [Item ID of Recommendations related to this Movie], [Similarity Percentage]

## Importing Data from 


