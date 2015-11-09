// Copyright (c) 2015 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is /furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in /all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

// Data was provided by GrouLens - http://grouplens.org/datasets/hetrec-2011/
// Please refer to this page for details on the licensing of this data
// http://files.grouplens.org/datasets/hetrec2011/hetrec2011-movielens-readme.txt
 
var inSearch = false;
var apiKey = [Azure Search Service API Key];
var azureSearchService = [Azure Search Service - Do not add .search.windows.net];

function execSuggest()
{
	// Use the autosuggest to lookup viable movies
	var q = encodeURIComponent($("#q").val());
	var searchAPI = "https://" + azureSearchService + ".search.windows.net/indexes/movies/docs?$top=12&$select=title,spanishTitle,recommendations,imdbPictureURL&api-version=2015-02-28&search=" + q;
	inSearch= true;
    $.ajax({
        url: searchAPI,
        beforeSend: function (request) {
            request.setRequestHeader("api-key", apiKey);
            request.setRequestHeader("Content-Type", "application/json");
            request.setRequestHeader("Accept", "application/json; odata.metadata=none");
        },
        type: "GET",
        success: function (data) {
			$( "#mediaContainer" ).html('');
			for (var item in data.value)
			{
				var title = data.value[item].title;
				var imageURL = data.value[item].imdbPictureURL;
				var recommendations = data.value[item].recommendations;
				$( "#mediaContainer" ).append( '<div class="col-md-4" style="text-align:center"><a href="javascript:void(0);" onclick="openMovieDetails(\'' + title + '\',\'' + recommendations.toString() + '\',\'' + item.toString() + '\');"><img src=' + imageURL + ' height=200><br><div style="height:100px"><b>' + title + '</b></a></div></div>' );
			}
			inSearch= false;
        }
    });
}

function openMovieDetails(title, recs, id)
{
	// Open the dialog with the recommendations
	$("#modal-title").html(title);
	$("#recDiv").html('Loading recommendations...');

	var filter = '';
	var recArray = recs.split(',');
	if (recArray[0] != ""){
		for (var i=0; i< recArray.length; i++)
		{
			if (i==0)
				filter+= '&$filter=id eq \'' + recArray[i] + '\'';
			else
				filter+= ' or id eq \'' + recArray[i] + '\'';
		}
		var searchAPI = "https://" + azureSearchService + ".search.windows.net/indexes/movies/docs?api-version=2015-02-28&search=*&$select=id,title" + filter;
		
		$.ajax({
			url: searchAPI,
			beforeSend: function (request) {
				request.setRequestHeader("api-key", apiKey);
				request.setRequestHeader("Content-Type", "application/json");
				request.setRequestHeader("Accept", "application/json; odata.metadata=none");
			},
			type: "GET",
			success: function (data) {
				$( "#recDiv" ).html('');
				for (var item in data.value)
					$( "#recDiv" ).append( '<p>' + data.value[item].title + '</p>' );
			}
		});
	} else 
		$("#recDiv").html("No recommendations available");

	$('#myModal').modal('show');
}
