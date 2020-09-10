using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Caliburn.Micro;
using Ccr.MaterialDesign.MVVM;
using Ccr.Std.Core.Extensions;
using FFmpeg.NET.Events;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Converter;
using YoutubeExplode.Playlists;
using YouTubeRipper.Media;
using YouTubeRipper.Media.Metadata;
using YouTubeRipper.Models;

namespace YouTubeRipper.ViewModels
{
	public enum UrlSourceLinkType
	{
		Playlist,
		Channel,
		Video,
		Invalid
	}

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
		private bool _downloadAudioOnly = true;
		private bool _isDownloadProcessRunning;
		private double _downloadProcessPercentage;
		private double _conversionProcessPercentage;
		private int _completedVideos;


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

		public string SourceUrl
		{
			get => _sourceUrl;
			set
			{
				_sourceUrl = value;
				NotifyOfPropertyChange(() => SourceUrl);
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
				AddVideosPopupViewState = AddVideosPopupViewStates.AddVideosPopupViewContractedStates;
			});

		public ICommand AddVideosFromChannelOrPlaylist => new Command(
			t =>
			{
				var sourceLinkType = GetSourceLinkType(SourceUrl);

				switch (sourceLinkType)
				{
					case UrlSourceLinkType.Playlist:
						loadPlaylistVideos(SourceUrl);
						break;
					case UrlSourceLinkType.Channel:
						loadChannelVideos(SourceUrl);
						break;
					case UrlSourceLinkType.Video:
					case UrlSourceLinkType.Invalid:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				SourceUrl = "";

				AddVideosPopupViewState = AddVideosPopupViewStates.AddVideosPopupViewContractedStates;
			});


		public ICommand StopBatchDownloadCommand => new Command(
			t =>
			{
				beginBatchDownloadProcess();
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

			VideosQueued.AddRange(
				playlistVideos.Select(t => new VideoInfo(t)));
		}

		private async void loadChannelVideos(string url)
		{
			var channelId = new ChannelId(url);
			var client = new YoutubeClient();

			var channelVideos = await client.Channels.GetUploadsAsync(channelId);

			VideosQueued.AddRange(
				channelVideos.Select(t => new VideoInfo(t)));
		}

		private async void beginBatchDownloadProcess()
		{
			CompletedVideos = 0;

			var youtubeClient = new YoutubeClient();
			var converter = new YoutubeConverter(youtubeClient);

			var targetDirectory = _downloadRootDirectory
				.CreateSubdirectory($"Batch Download - {DateTime.Now:yyyy-MM-dd HH-mm-ss}");

			targetDirectory.Create();

			foreach (var videoInfo in VideosQueued)
			{
				CurrentProcessingVideoInfo = videoInfo;

				var modifiedVideoTitle = FormatOpieAndAnthonyTitle(videoInfo.VideoTitle);

				Console.WriteLine($"Processing video {videoInfo.VideoTitle}");

				DateTime? dateTime = null;


				if (DateTimeRecognizer.TryExtractDate(
					modifiedVideoTitle,
					out var extractedText,
					out dateTime))
				{
					modifiedVideoTitle = extractedText.Replace("()", "");

					if (dateTime.HasValue)
						Console.WriteLine($"Extracted DateTime {dateTime.Value:yyyy-MM-dd}");
				}

				var legalFileNameTitle = ReplaceInvalidChars(modifiedVideoTitle);

				var compressedTitle = RemoveSpacesFromTitle(legalFileNameTitle);

				if (dateTime.HasValue)
					compressedTitle = $"{dateTime.Value:yyyy-MM-dd}-{compressedTitle}";
				
				var targetFileName = $@"{targetDirectory.FullName}\{compressedTitle}.mp4";

				Console.WriteLine($"Performing MP4 download...");
				
				try
				{
					var downloadProgress = new Progress<double>();
					downloadProgress.ProgressChanged += onDownloadProcessProgressChanged;

					await converter.DownloadVideoAsync(
						videoInfo.Video.Id,
						targetFileName,
						downloadProgress);

					Console.WriteLine($"Finished download.");

					var mp4FileInfo = new FileInfo(targetFileName);

					var mp3File = await MediaConverter
						.ConvertMP4ToMp3Async(
							mp4FileInfo,
							onConversionProcessProgressUpdated);

					Console.WriteLine($"Completed Conversion to MP3. Deleting MP4.");

					if (!mp3File.Exists)
						Console.WriteLine($"Cannot convert video {mp4FileInfo.FullName} from mp4 to mp3!");

					//else
					//	mp4FileInfo.Delete();

					var audioShellObject = new AudioShellObject(mp3File);

					if (!audioShellObject.TrySetEncodedBy(videoInfo.Uploader))
						Console.WriteLine(
							$"Failed to set metadata: EncodedBy = {videoInfo.Uploader.Quote()}.");

					if (!audioShellObject.TrySetDateEncoded(DateTime.UtcNow))
						Console.WriteLine(
							$"Failed to set metadata: DateEncoded = \"{DateTime.UtcNow:s}\".");

					if (!audioShellObject.TrySetTitle(videoInfo.VideoTitle))
						Console.WriteLine(
							$"Failed to set metadata: Title = {videoInfo.VideoTitle.Quote()}.");

					if (!audioShellObject.TrySetSubtitle(videoInfo.Description))
						Console.WriteLine(
							$"Failed to set metadata: Subtitle = {videoInfo.Description.Quote()}.");

					if (dateTime != null)
					{
						if (!audioShellObject.TrySetDateReleased(dateTime.Value))
							Console.WriteLine(
								$"Failed to set metadata: DateReleased = {dateTime.Value.ToString("yyyy-MM-dd").SQuote()}.");

						if (!audioShellObject.TrySetDate(dateTime.Value))
							Console.WriteLine(
								$"Failed to set metadata: Date = {dateTime.Value.ToString("yyyy-MM-dd").SQuote()}.");
					}

					Console.WriteLine($"Completed setting metadata.");

					videoInfo.IsDownloadComplete = true;
				}
				catch (Exception ex)
				{
					var targetErrorTxtFileName = $@"{targetDirectory.FullName}\{compressedTitle}.txt";
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

		public string ReplaceInvalidChars(
			string filename)
		{
			return string.Join(" ", filename.Split(Path.GetInvalidFileNameChars()));
		}

		public string FormatOpieAndAnthonyTitle(
			string filename)
		{
			return filename
				.Replace("O&A", "")
				.Replace("Opie & Anthony", "")
				.Replace("Opie and Anthony", "")
				.Replace("O and A", "")
				.Replace("o&a", "")
				.Replace("opie & anthony", "")
				.Replace("opie and anthony", "")
				.Replace("o and a", "")
				.Trim(' ', '-', ':');
		}

		public string RemoveSpacesFromTitle(
			string title)
		{
			var titleCase = Regex.Replace(title, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
			var compressedTitleCase = titleCase.Replace(" ", "");
			return compressedTitleCase;
		}
	}
}