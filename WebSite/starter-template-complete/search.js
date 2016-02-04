var azureSearchQueryApiKey = "";
var azureMLModelId = ""
var azureMLBuildId = ""
var inSearch = false;

function execSuggest()
{
	// Execute a search to lookup viable movies
	var q = encodeURIComponent($("#q").val());
	var searchAPI = "https://azs-playground.search.windows.net/indexes/movies/docs?$top=12&$select=id,title,imdbPictureURL&api-version=2015-02-28&search=" + q;
	inSearch= true;
    $.ajax({
        url: searchAPI,
        beforeSend: function (request) {
            request.setRequestHeader("api-key", azureSearchQueryApiKey);
            request.setRequestHeader("Content-Type", "application/json");
            request.setRequestHeader("Accept", "application/json; odata.metadata=none");
        },
        type: "GET",
        success: function (data) {
			$( "#mediaContainer" ).html('');
			for (var item in data.value)
			{
				var id = data.value[item].id;
				var title = data.value[item].title;
				var imageURL = data.value[item].imdbPictureURL;
				$( "#mediaContainer" ).append( '<div class="col-md-4" style="text-align:center"><a href="javascript:void(0);" onclick="openMovieDetails(\'' + title + '\',\'' + id + '\');"><img src=' + imageURL + ' height=200><br><div style="height:100px"><b>' + title + '</b></a></div></div>' );
			}
			inSearch= false;
        }
    });
}

function openMovieDetails(title, id)
{
	// Open the dialog with the recommendations
	$("#modal-title").html(title);
	$("#recDiv").html('Loading recommendations...');

	var recommendatationAPI = "https://api.datamarket.azure.com/data.ashx/amla/recommendations/v2/ItemRecommend?$format=json&modelId='" + azureMLModelId + "'&numberOfResults=5&buildId=" + azureMLBuildId + "&includeMetadata=false&apiVersion='1.0'&itemIds='" + id + "'";

	$.ajax({
		url: recommendatationAPI,
		beforeSend: function (request) {
			request.setRequestHeader("Authorization", "Basic QWNjb3VudEtleTpIK0ZDVldETWZZbnpja2ZUa3pxNDlzT01aR2dFVDlVNFdqL2xCSHhZeStzPQ==");
		},
		type: "GET",
		success: function (data) {
			$("#recDiv").html('');
			for (var item in data.d.results)
				$( "#recDiv" ).append( '<p>' + data.d.results[item].Name + '</p>' );
		}
	});

	$('#myModal').modal('show');
}