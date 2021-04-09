using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;
using Ccr.MaterialDesign.MVVM;
using Ccr.Std.Core.Extensions;
using FFmpeg.NET.Events;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Converter;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos.Streams;
using YouTubeRipper.Media;
using YouTubeRipper.Media.Metadata;
using YouTubeRipper.Models;
using YouTubeRipper.YouTubeAPI;

namespace YouTubeRipper.ViewModels
{
	public class RootViewModel
		: ViewModelBase
	{
		public enum AddVideosPopupViewStates
		{
			AddVideosPopupViewContractedStates,
			AddVideosPopupViewExpandedStates
		}


		private AddVideosPopupViewStates _addVideosPopupViewState
			= AddVideosPopupViewStates.AddVideosPopupViewContractedStates;
		private BindableCollection<VideoInfo> _videosQueued;
		private AddVideosPopupViewModel _addVideosPopupView;
		private DirectoryInfo _downloadRootDirectory;
		private VideoInfo _currentProcessingVideoInfo;
		private string _sourceUrl;
		private bool _convertToAudio = true;
		private bool _downloadAudioOnly = false;
		private bool _isDownloadProcessRunning;
		private double _downloadProcessPercentage;
		private double _conversionProcessPercentage;
		private int _completedVideos;
		private bool _isUrlInvalid;

		//private static DateTime? _currentFileStartDateTime;
		//private static Dictionary<TimeSpan, FileSize> _downloadRateLog
		//	= new Dictionary<TimeSpan, FileSize>();


		public bool IsUrlInvalid
		{
			get => _isUrlInvalid;
			set
			{
				_isUrlInvalid = value;
				NotifyOfPropertyChange(() => IsUrlInvalid);
			}
		}

		public AddVideosPopupViewStates AddVideosPopupViewState
		{
			get => _addVideosPopupViewState;
			set
			{
				_addVideosPopupViewState = value;
				NotifyOfPropertyChange(() => AddVideosPopupViewState);
			}
		}

		public BindableCollection<VideoInfo> VideosQueued
		{
			get => _videosQueued;
			set
			{
				_videosQueued = value;
				NotifyOfPropertyChange(() => VideosQueued);
			}
		}

		public AddVideosPopupViewModel AddVideosPopupView
		{
			get => _addVideosPopupView;
			set
			{
				_addVideosPopupView = value;
				NotifyOfPropertyChange(() => AddVideosPopupView);
			}
		}

		public DirectoryInfo DownloadRootDirectory
		{
			get => _downloadRootDirectory;
			set
			{
				_downloadRootDirectory = value;
				NotifyOfPropertyChange(() => DownloadRootDirectory);
			}
		}

		public VideoInfo CurrentProcessingVideoInfo
		{
			get => _currentProcessingVideoInfo;
			set
			{
				_currentProcessingVideoInfo = value;
				NotifyOfPropertyChange(() => CurrentProcessingVideoInfo);
			}
		}

		//private FileSize _totalEstimatedFileSize;

		//public FileSize TotalEstimatedFileSize
		//{
		//	get => _totalEstimatedFileSize;
		//	set
		//	{
		//		_totalEstimatedFileSize = value;
		//		NotifyOfPropertyChange(() => TotalEstimatedFileSize);
		//	}
		//}

		//private FileSize _currentDownloadedFileSize;

		//public FileSize CurrentDownloadedFileSize
		//{
		//	get => _currentDownloadedFileSize;
		//	set
		//	{
		//		_currentDownloadedFileSize = value;
		//		NotifyOfPropertyChange(() => CurrentDownloadedFileSize);
		//	}
		//}

		public string SourceUrl
		{
			get => _sourceUrl;
			set
			{
				_sourceUrl = value;
				NotifyOfPropertyChange(() => SourceUrl);
			}
		}

		public bool ConvertToAudio
		{
			get => _convertToAudio;
			set
			{
				_convertToAudio = value;
				NotifyOfPropertyChange(() => ConvertToAudio);
			}
		}

		public bool DownloadAudioOnly
		{
			get => _downloadAudioOnly;
			set
			{
				_downloadAudioOnly = value;
				NotifyOfPropertyChange(() => DownloadAudioOnly);
			}
		}

		public bool IsDownloadProcessRunning
		{
			get => _isDownloadProcessRunning;
			set
			{
				_isDownloadProcessRunning = value;
				NotifyOfPropertyChange(() => IsDownloadProcessRunning);
			}
		}

		public double DownloadProcessPercentage
		{
			get => _downloadProcessPercentage;
			set
			{
				_downloadProcessPercentage = value;
				NotifyOfPropertyChange(() => DownloadProcessPercentage);
			}
		}

		public double ConversionProcessPercentage
		{
			get => _conversionProcessPercentage;
			set
			{
				_conversionProcessPercentage = value;
				NotifyOfPropertyChange(() => ConversionProcessPercentage);
			}
		}

		public int CompletedVideos
		{
			get => _completedVideos;
			set
			{
				_completedVideos = value;
				NotifyOfPropertyChange(() => CompletedVideos);
			}
		}

		//private TimeSpan _estimatedTimeRemaining;

		//public TimeSpan EstimatedTimeRemaining
		//{
		//	get => _estimatedTimeRemaining;
		//	set
		//	{
		//		_estimatedTimeRemaining = value;
		//		NotifyOfPropertyChange(() => EstimatedTimeRemaining);
		//	}
		//}


		public RootViewModel()
		{
			VideosQueued = new BindableCollection<VideoInfo>();

			var documentsDirectory = new DirectoryInfo(
				Environment.GetFolderPath(
					Environment.SpecialFolder.MyDocuments));

			var downloadDirectory = new DirectoryInfo(
				$@"{documentsDirectory.FullName}\YouTube Ripper\");

			if (downloadDirectory.Exists)
				downloadDirectory.Create();

			DownloadRootDirectory = downloadDirectory;
		}



		public ICommand OpenAddNewVideosDialogCommand => new Command(
			t =>
			{
				AddVideosPopupViewState = AddVideosPopupViewStates.AddVideosPopupViewExpandedStates;
			});

		public ICommand CloseAddNewVideosDialogCommand => new Command(
			t =>
			{
				SourceUrl = "";
				IsUrlInvalid = false;

				AddVideosPopupViewState = AddVideosPopupViewStates.AddVideosPopupViewContractedStates;
			});

		public ICommand AddVideosFromChannelOrPlaylist => new Command(
			t =>
			{
				var sourceLinkType = GetSourceLinkType(SourceUrl);

				switch (sourceLinkType)
				{
					case UrlSourceLinkType.Playlist:
						IsUrlInvalid = false;
						loadPlaylistVideos(SourceUrl);
						break;
					case UrlSourceLinkType.Channel:
						IsUrlInvalid = false;
						loadChannelVideos(SourceUrl);
						break;
					case UrlSourceLinkType.Video:
					case UrlSourceLinkType.Invalid:
						IsUrlInvalid = true;
						return;
					default:
						throw new ArgumentOutOfRangeException();
				}

				SourceUrl = "";

				AddVideosPopupViewState = AddVideosPopupViewStates.AddVideosPopupViewContractedStates;
			});

		public ICommand SaveConfigCommand => new Command(
			t =>
			{

			});

		public ICommand StopBatchDownloadCommand => new Command(
			t =>
			{
				// TODO implement stopping batch download/cancellationToke
			},
			t => IsDownloadProcessRunning);

		
		public ICommand BeginBatchDownloadCommand => new Command(
			t =>
			{
				beginBatchDownloadProcess();
			},
			t => !IsDownloadProcessRunning && VideosQueued.Any());



		private UrlSourceLinkType GetSourceLinkType(string url)
		{
			var playlistId = PlaylistId.TryParse(url);

			if (playlistId != null)
				return UrlSourceLinkType.Playlist;

			var channelId = ChannelId.TryParse(url);

			if (channelId != null)
				return UrlSourceLinkType.Channel;

			return UrlSourceLinkType.Invalid;
		}

		private async void loadPlaylistVideos(string url)
		{
			var playlistId = new PlaylistId(url);
			var client = new YoutubeClient();

			var playlistVideos = await client.Playlists.GetVideosAsync(playlistId);


			//foreach (var playlistVideo in playlistVideos)
			//{
			//	VideosQueued.Add(
			//		await VideoInfo.CreateAsync(playlistVideo));
			//}
			//foreach (var playlistVideo in playlistVideos)
			//{
			//	try
			//	{
			//		VideosQueued.Add(
			//			await VideoInfo.CreateAsync(playlistVideo));

			//		updateEstimatedFileSize();
			//	}
			//	catch (Exception ex)
			//	{

			//	}
			//}


			var playlistVideosResponse = YouTubeAPIHelper
				.GetVideosInPlaylistAsync(playlistId);

			var playlistVideosPublishedDates = playlistVideosResponse
				.Result
				.Select(
					t => (t.ContentDetails.VideoId, t.Snippet.PublishedAt))
				.ToArray();

			VideosQueued.AddRange(
				playlistVideos.Select(
					t => new VideoInfo(t,
						playlistVideosPublishedDates
							.FirstOrDefault(
								p => p.VideoId == t.Id.Value).PublishedAt)));

			VideosQueued.AddRange(
				playlistVideos.Select(t => new VideoInfo(t)));

			//updateEstimatedFileSize();
		}

		private async void loadChannelVideos(string url)
		{
			var channelId = new ChannelId(url);
			var client = new YoutubeClient();

			var channelVideos = await client.Channels.GetUploadsAsync(channelId);

			var playlistVideosResponse = YouTubeAPIHelper
				.GetVideosFromChannelAsync(channelId.Value);

			var playlistVideos = playlistVideosResponse
				.Result
				.Select(
					t => (t.Id.VideoId, t.Snippet.PublishedAt))
				.ToArray();

			//foreach (var playlistVideo in playlistVideos)
			//{
			//	playlistVideo.Snippet.PublishedAt
			//}

			//foreach (var channelVideo in channelVideos)
			//{
			//	try
			//	{
			//		VideosQueued.Add(
			//			await VideoInfo.CreateAsync(channelVideo));

			//		updateEstimatedFileSize();
			//	}
			//	catch (Exception ex)
			//	{

			//	}
			//}


			VideosQueued.AddRange(
				channelVideos.Select(
					t => new VideoInfo(t, 
						playlistVideos
							.FirstOrDefault(
								p => p.VideoId == t.Id.Value).PublishedAt)));

			//updateEstimatedFileSize();
		}

		//private void updateEstimatedFileSize()
		//{
		//	//var fileSizeTotalBytes = VideosQueued
		//	//	.Where(t => t.ShouldDownload)
		//	//	.Where(t => t.StreamFileSize.IsCompleted)
		//	//	.Sum(t => t.StreamFileSize.Result.TotalBytes);
		//	var fileSizeTotalBytes = VideosQueued
		//		.Where(t => t.ShouldDownload)
		//		.Where(t => t.StreamFileSize.HasValue)
		//		.Sum(t => t.StreamFileSize.Value.TotalBytes);

		//	TotalEstimatedFileSize = new FileSize(fileSizeTotalBytes);
		//}

		//private bool _hasHaltedDownloadRateUpdater = false;

		//private void updateTimeRemaining()
		//{
		//	if (!IsDownloadProcessRunning ||! _currentFileStartDateTime.HasValue)
		//	{
		//		if (_hasHaltedDownloadRateUpdater)
		//			return;

		//		_downloadRateLog.Clear();

		//		_hasHaltedDownloadRateUpdater = true;
		//		return;
		//	}

		//	_hasHaltedDownloadRateUpdater = false;

		//	//var areAnyIncomplete = VideosQueued
		//	//	.Where(t => t.ShouldDownload)
		//	//	.Any(t => !t.StreamFileSize.IsCompleted);

		//	//var errorFileSizeFetches = VideosQueued
		//	//	.Where(t => t.ShouldDownload)
		//	//	.Where(t => t.StreamFileSize.IsFaulted)
		//	//	.ToArray();

		//	var areAnyIncomplete = VideosQueued
		//		.Where(t => t.ShouldDownload)
		//		.Any(t => !t.StreamFileSize.HasValue);

		//	var errorFileSizeFetches = VideosQueued
		//		.Where(t => t.ShouldDownload)
		//		.Where(t => t.HasStreamFileSizeExceededRetryAttempts)
		//		.ToArray();

		//	if (areAnyIncomplete)
		//	{
		//	}

		//	if (errorFileSizeFetches.Any())
		//	{
		//	}

		//	if (!CurrentProcessingVideoInfo.StreamFileSize.HasValue)
		//		return;

		//	// current file size * percentage complete (estimated value)
		//	//var estimatedCompleteFileSizeBytesInFile =
		//	//	CurrentProcessingVideoInfo.StreamFileSize.Result.TotalBytes * (DownloadProcessPercentage / 100d);
		//	var estimatedCompleteFileSizeBytesInFile =
		//		CurrentProcessingVideoInfo.StreamFileSize.Value.TotalBytes * (DownloadProcessPercentage / 100d);

		//	var estimatedCompletesInFileSizeBytes =
		//		(long)Math.Round(estimatedCompleteFileSizeBytesInFile);

		//	//var previouslyCompletedFileSize = TotalEstimatedFileSize

		//	var estimatedCompletesInFileSize = new FileSize(estimatedCompletesInFileSizeBytes);
		////	TotalEstimatedFileSize

		//	Console.WriteLine($"[completed file size: {estimatedCompletesInFileSize}");
			
		//	var timeSinceDownloadBegan = DateTime.Now - _currentFileStartDateTime.Value;

		//	Console.WriteLine($"[time since download began: {timeSinceDownloadBegan}");

		//	_downloadRateLog.Add(timeSinceDownloadBegan, estimatedCompletesInFileSize);

		//	if (_downloadRateLog.Count >= 5)
		//	{
		//		var firstEntry = _downloadRateLog.First();
		//		_downloadRateLog.Remove(firstEntry.Key);

		//		var bytesPerSecondRates = new List<double>();

		//		var lastLogEntry = firstEntry;
		//		foreach (var currentLogEntry in _downloadRateLog.Skip(1))
		//		{
		//			var timeBetweenRequests = currentLogEntry.Key - lastLogEntry.Key;

		//			Console.WriteLine($"    - [time between requests: {timeBetweenRequests}");

		//			var fileSizeDownloadedBetweenRequestsBytes 
		//				= currentLogEntry.Value.TotalBytes - lastLogEntry.Value.TotalBytes;

		//			var fileSizeDownloadedBetweenRequests
		//				= new FileSize(fileSizeDownloadedBetweenRequestsBytes);
					
		//			Console.WriteLine($"    - [file Size Downloaded Between Requests: {fileSizeDownloadedBetweenRequests}");

		//			var scaleFactor = 1d / timeBetweenRequests.TotalSeconds;

		//			Console.WriteLine($"    - [scale factor: {scaleFactor}");

		//			var bytesPerSecondRate = fileSizeDownloadedBetweenRequests.TotalBytes * scaleFactor;
		//			bytesPerSecondRates.Add(bytesPerSecondRate);

		//			lastLogEntry = currentLogEntry;
		//		}

		//		var averageBytesPerSecondRate = bytesPerSecondRates.Average();
		//		var averageBytesPerSecondRateFileSize = new FileSize((long)Math.Round(averageBytesPerSecondRate));
				
		//		Console.WriteLine($"|avg bytes / second: {averageBytesPerSecondRateFileSize}");
				
		//		var totalBytesRemaining =
		//			TotalEstimatedFileSize.TotalBytes
		//			- CurrentDownloadedFileSize.TotalBytes
		//			- estimatedCompleteFileSizeBytesInFile;

		//		var totalBytesRemainingFileSize = new FileSize((long)totalBytesRemaining);

		//		Console.WriteLine($"[total size remaining: {totalBytesRemainingFileSize}");

		//		var estimatedRemainingSeconds = totalBytesRemaining / averageBytesPerSecondRate;
		//		var estimatedTimeRemaining = TimeSpan.FromSeconds(estimatedRemainingSeconds);

		//		Console.WriteLine($"|estimated time remaining: {estimatedTimeRemaining}");

		//		EstimatedTimeRemaining = estimatedTimeRemaining;
		//	}
		//}


		private async void beginBatchDownloadProcess()
		{
			IsDownloadProcessRunning = true;

			CompletedVideos = 0;

			var youtubeClient = new YoutubeClient();
			//var converter = new YoutubeConverter(youtubeClient);

			var targetDirectory = _downloadRootDirectory
				.CreateSubdirectory($"Batch Download - {DateTime.Now:yyyy-MM-dd HH-mm-ss}");

			targetDirectory.Create();

			foreach (var videoInfo in VideosQueued)
			{
				if (!videoInfo.ShouldDownload)
				{
					CompletedVideos += 1;
					continue;
				}

				//_currentFileStartDateTime = DateTime.Now;
				CurrentProcessingVideoInfo = videoInfo;

				var modifiedVideoTitle = videoInfo.VideoTitle;

				//TODO do this better, have options / different format options
				//var modifiedVideoTitle = FormatOpieAndAnthonyTitle(videoInfo.VideoTitle);
				//modifiedVideoTitle = modifiedVideoTitle.Replace("Joe Rogan Experience", "JRE");

				Console.WriteLine($"Processing video {videoInfo.VideoTitle}");

				var dateTimeOffset = videoInfo.UploadDate;
				DateTime dateTime = dateTimeOffset.DateTime;

				if (DateTimeRecognizer.TryExtractDate(
					modifiedVideoTitle,
					out var extractedText,
					out var extractedDateTime))
				{
					modifiedVideoTitle = extractedText.Replace("()", "");

					if (extractedDateTime.HasValue)
					{
						Console.WriteLine(
							$"Extracted DateTime from video title: \"{extractedDateTime.Value:yyyy-MM-dd}\".");

						dateTime = extractedDateTime.Value;
					}
				}

				var legalFileNameTitle = ReplaceInvalidChars(modifiedVideoTitle);

				var compressedTitle = legalFileNameTitle;

				 //var compressedTitle = RemoveSpacesFromTitle(legalFileNameTitle);

				//var compressedTitle = $"JRE-{dateTime:yyyy-MM-dd}";//-{compressedTitle}";

				var targetFileName = $@"{targetDirectory.FullName}\{compressedTitle}.mp4";

				Console.WriteLine($"Performing MP4 download...");

				try
				{
					var downloadProgress = new Progress<double>();
					downloadProgress.ProgressChanged += onDownloadProcessProgressChanged;

					//var streamManifest = await youtubeClient
					//	.Videos
					//	.Streams
					//	.GetManifestAsync(videoInfo.Video.Id);
					//	.ConfigureAwait(false);

					//var streamInfos = GetBestMediaStreamInfos(streamManifest, request.Format).ToArray();

					await youtubeClient.Videos.DownloadAsync(
						videoInfo.VideoId,
						targetFileName,
						//t => t.SetFormat("mp4"),  //.SetPreset(ConversionPreset.UltraFast),
						downloadProgress);

					Console.WriteLine($"Finished download.");

					var mp4FileInfo = new FileInfo(targetFileName);

					if (ConvertToAudio)
					{
						var mp3File = await MediaConverter
							.ConvertMP4ToMp3Async(
								mp4FileInfo,
								onConversionProcessProgressUpdated);

						if (!mp3File.Exists)
						{
							Console.WriteLine(
								$"Video conversion from .mp4 to .mp3 failed! The file " +
								$"{mp4FileInfo.FullName.SQuote()} does not exist.");

							//TODO should break/continue loop for setting mp4 metadata?
						}
						else
							Console.WriteLine(
								$"Completed Conversion to .mp3.");

						if (DownloadAudioOnly)
						{
							Console.WriteLine($"Deleting MP4.");
							mp4FileInfo.Delete();
						}
						//else
						//	

						var audioShellObject = new AudioShellObject(mp3File);

						if (!audioShellObject.TrySetEncodedBy(videoInfo.Uploader))
							Console.WriteLine(
								$"Failed to set metadata: EncodedBy = {videoInfo.Uploader.Quote()}.");

						if (!audioShellObject.TrySetDateEncoded(DateTime.UtcNow))
							Console.WriteLine(
								$"Failed to set metadata: DateEncoded = \"{DateTime.UtcNow:s}\".");

						if (!audioShellObject.TrySetTitle(modifiedVideoTitle))
							Console.WriteLine(
								$"Failed to set metadata: Title = {modifiedVideoTitle.Quote()}.");

						if (!audioShellObject.TrySetSubtitle(videoInfo.Description))
							Console.WriteLine(
								$"Failed to set metadata: Subtitle = {videoInfo.Description.Quote()}.");

						if (!audioShellObject.TrySetDateReleased(dateTime))
							Console.WriteLine(
								$"Failed to set metadata: DateReleased = {dateTime.ToString("yyyy-MM-dd").SQuote()}.");

						if (!audioShellObject.TrySetDate(dateTime))
							Console.WriteLine(
								$"Failed to set metadata: Date = {dateTime.ToString("yyyy-MM-dd").SQuote()}.");

						Console.WriteLine(
							$"Completed setting metadata.");
					}

					videoInfo.IsDownloadComplete = true;

					//if (CurrentProcessingVideoInfo.StreamFileSize.HasValue)
					//{
					//	CurrentDownloadedFileSize = new FileSize(
					//		CurrentDownloadedFileSize.TotalBytes +
					//		CurrentProcessingVideoInfo.StreamFileSize.Value.TotalBytes);
					//}
					//else
					//{
					//	Console.WriteLine(
					//		$"ERROR: Could not add streamFileSize for {CurrentProcessingVideoInfo.VideoTitle.Quote()} because " +
					//		$"StreamFileSize is 'null' or failed to fetch.");
					//}

					//CurrentDownloadedFileSize = new FileSize(
					//	CurrentDownloadedFileSize.TotalBytes +
					//	CurrentProcessingVideoInfo.StreamFileSize.Result.TotalBytes);
				}
				catch (Exception ex)
				{
					var targetErrorTxtFileName = $@"{targetDirectory.FullName}\{compressedTitle}.ErrorLog.txt";
					var targetErrorTxtFileInfo = new FileInfo(targetErrorTxtFileName);

					using var writer = targetErrorTxtFileInfo.CreateText();
					await writer.WriteLineAsync(ex.ToString());

					writer.Close();
				}

				CompletedVideos += 1;
				DownloadProcessPercentage = 0;
				ConversionProcessPercentage = 0;
			}

			VideosQueued.Clear();

			IsDownloadProcessRunning = false;

			Process.Start("explorer.exe", targetDirectory.FullName);
		}

		private void onDownloadProcessProgressChanged(
			object sender,
			double percentage)
		{
			DownloadProcessPercentage = percentage * 100;
		}

		private void onConversionProcessProgressUpdated(
			object sender,
			ConversionProgressEventArgs args)
		{
			var percentageComplete =
				(double)args.ProcessedDuration.Ticks
				/ args.TotalDuration.Ticks;

			ConversionProcessPercentage = percentageComplete * 100;
		}

		private string ReplaceInvalidChars(
			string filename)
		{
			return string.Join(" ", filename.Split(Path.GetInvalidFileNameChars()));
		}

		//private string FormatOpieAndAnthonyTitle(
		//	string filename)
		//{
		//	return filename
		//		.Replace("O&A", "")
		//		.Replace("Opie & Anthony", "")
		//		.Replace("Opie and Anthony", "")
		//		.Replace("O and A", "")
		//		.Replace("o&a", "")
		//		.Replace("opie & anthony", "")
		//		.Replace("opie and anthony", "")
		//		.Replace("o and a", "")
		//		.Trim(' ', '-', ':');
		//}

		private string RemoveSpacesFromTitle(
			string title)
		{
			var titleCase = Regex.Replace(title, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
			var compressedTitleCase = titleCase.Replace(" ", "");
			return compressedTitleCase;
		}
	}
}
