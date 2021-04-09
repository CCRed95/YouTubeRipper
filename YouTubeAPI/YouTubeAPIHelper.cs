using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YoutubeExplode.Playlists;

namespace YouTubeRipper.YouTubeAPI
{
	public static class YouTubeAPIHelper
	{
		private const string apiKey = "AIzaSyCuYUUROyqxyVOYg7oxnUgzvwhedmL444U";
		private const string applicationName = "YouTube Ripper";


		public static Task<List<SearchResult>> GetVideosFromChannelAsync(
			string ytChannelId)
		{
			return Task.Run(() =>
			{
				var results = new List<SearchResult>();

				using var youtubeService = CreateYouTubeService();

				var nextPageToken = " ";

				while (nextPageToken != null)
				{
					var searchListRequest = youtubeService.Search.List("snippet");
					searchListRequest.MaxResults = 50;
					searchListRequest.ChannelId = ytChannelId;
					searchListRequest.PageToken = nextPageToken;

					var searchListResponse = searchListRequest.Execute();

					results.AddRange(searchListResponse.Items);

					nextPageToken = searchListResponse.NextPageToken;
				}
				return results;
			});
		}

		public static Task<List<PlaylistItem>> GetVideosInPlaylistAsync(
			PlaylistId playlistId)
		{
			return Task.Run(() =>
			{
				var results = new List<PlaylistItem>();

				using var youtubeService = CreateYouTubeService();

				var nextPageToken = " ";

				while (nextPageToken != null)
				{
					var playlistListRequest = youtubeService.PlaylistItems.List("snippet");
					playlistListRequest.MaxResults = 50;
					playlistListRequest.PlaylistId = playlistId.Value;
					playlistListRequest.PageToken = nextPageToken;

					var playlistListResponse = playlistListRequest.Execute();

					results.AddRange(playlistListResponse.Items);

					nextPageToken = playlistListResponse.NextPageToken;
				}
				return results;
			});
		}

		private static YouTubeService CreateYouTubeService()
		{
			var youtubeService = new YouTubeService(
				new BaseClientService.Initializer
				{
					ApiKey = apiKey,
					ApplicationName = applicationName
				});

			return youtubeService;
		}


		//public static Task<FileSize?> GetVideoFileSizeAsync(
		//	string videoId)
		//{
		//	return Task.Run(() =>
		//	{
		//		var results = new List<Video>();

		//		using var youtubeService = CreateYouTubeService();

		//		var videoListRequest = youtubeService.Videos.List("FileDetails");
		//		videoListRequest.MaxResults = 50;
		//		videoListRequest.Id = videoId; 
		//		var videoListResponse = videoListRequest.Execute();

		//		results.AddRange(videoListResponse.Items);

		//		if (results.Count != 1)
		//			return null;

		//		var fileDetails= results.First().;
		//		var fileSizeBytes = fileDetails?.FileSize;

		//		if (!fileSizeBytes.HasValue)
		//			return null;

		//		return new FileSize?(
		//			new FileSize((long)fileSizeBytes.Value));
		//	});
		//}
	}
}
