CREATE TABLE [dbo].[movies](
	[id] [int] NOT NULL,
	[title] [varchar](1024) NULL,
	[imdbID] [varchar](32) NULL,
	[spanishTitle] [varchar](1024) NULL,
	[imdbPictureURL] [varchar](max) NULL,
	[year] [int] NULL,
	[rtID] [varchar](256) NULL,
	[rtAllCriticsRating] [numeric](5, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)
)

GO
