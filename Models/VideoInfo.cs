using System.Windows;
using Ccr.MaterialDesign.MVVM;
using YoutubeExplode.Videos;

namespace YouTubeRipper.Models
{
	public class VideoInfo
		: ViewModelBase
	{
		private Video _video;
		private string _videoTitle;
		private string _uploader;
		private string _description;
		private Duration _videoDuration;
		private string _thumbnailImageSource;
		private bool _shouldDownload = true;
		private bool _isDownloadComplete;


		public Video Video
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


		public VideoInfo(
			Video video)
		{
			Video = video;
			Description = video.Description;
			Uploader = video.Author;
			VideoDuration = video.Duration;
			VideoTitle = video.Title;
			ThumbnailImageSource = video.Thumbnails.MediumResUrl;
		}
	}
}