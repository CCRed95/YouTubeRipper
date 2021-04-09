using System;
using System.Windows;
using Ccr.MaterialDesign.MVVM;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace YouTubeRipper.Models
{
	public class VideoInfo
		: ViewModelBase
	{
		private VideoId _videoId;
		private object _video;
		private string _videoTitle;
		private string _uploader;
		private string _description;
		private Duration _videoDuration;
		private string _thumbnailImageSource;
		private bool _shouldDownload = true;
		private bool _isDownloadComplete;
		private DateTimeOffset _uploadDate;


		public VideoId VideoId
		{
			get => _videoId;
			set
			{
				_videoId = value;
				NotifyOfPropertyChange(() => VideoId);
			}
		}

		public object Video
		{
			get => _video;
			set
			{
				_video = value;
				NotifyOfPropertyChange(() => Video);
			}
		}

		public string VideoTitle
		{
			get => _videoTitle;
			set
			{
				_videoTitle = value;
				NotifyOfPropertyChange(() => VideoTitle);
			}
		}

		public string Uploader
		{
			get => _uploader;
			set
			{
				_uploader = value;
				NotifyOfPropertyChange(() => Uploader);
			}
		}

		public string Description
		{
			get => _description;
			set
			{
				_description = value;
				NotifyOfPropertyChange(() => Description);
			}
		}

		public Duration VideoDuration
		{
			get => _videoDuration;
			set
			{
				_videoDuration = value;
				NotifyOfPropertyChange(() => VideoDuration);
			}
		}

		public string ThumbnailImageSource
		{
			get => _thumbnailImageSource;
			set
			{
				_thumbnailImageSource = value;
				NotifyOfPropertyChange(() => ThumbnailImageSource);
			}
		}

		public bool ShouldDownload
		{
			get => _shouldDownload;
			set
			{
				_shouldDownload = value;
				NotifyOfPropertyChange(() => ShouldDownload);
			}
		}

		public bool IsDownloadComplete
		{
			get => _isDownloadComplete;
			set
			{
				_isDownloadComplete = value;
				NotifyOfPropertyChange(() => IsDownloadComplete);
			}
		}

		public DateTimeOffset UploadDate
		{
			get => _uploadDate;
			private set
			{
				_uploadDate = value;
				NotifyOfPropertyChange(() => UploadDate);
			}
		}


		public VideoInfo(
			PlaylistVideo video,
			DateTime? publishedAt = null)
		{
			Video = video;
			VideoId = video.Id;
			Description = video.Description;
			Uploader = video.Author;
			VideoDuration = video.Duration;
			VideoTitle = video.Title;
			ThumbnailImageSource = video.Thumbnails.MediumResUrl;

			UploadDate = publishedAt ?? new DateTimeOffset(1971, 1, 1, 0, 0, 0, TimeSpan.Zero);

			ShouldDownload = true;
		}

		public VideoInfo(
			Video video)
		{
			Video = video;
			VideoId = video.Id;
			Description = video.Description;
			Uploader = video.Author;
			VideoDuration = video.Duration;
			VideoTitle = video.Title;
			ThumbnailImageSource = video.Thumbnails.MediumResUrl;
			
			UploadDate = video.UploadDate;
			
			ShouldDownload = true;
		}
	}
}
