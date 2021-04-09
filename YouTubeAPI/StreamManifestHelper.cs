using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

namespace YouTubeRipper.YouTubeAPI
{
	public static class StreamManifestHelper
	{
		private static bool IsTranscodingRequired(
			Container container,
			ConversionFormat format)
		{
			return !string.Equals(
				container.Name,
				format.Name,
				StringComparison.OrdinalIgnoreCase);
		}


		public static IEnumerable<IStreamInfo> GetBestMediaStreamInfos(
			StreamManifest streamManifest,
			ConversionFormat format)
		{
			// Fail if there are no available streams
			if (!streamManifest.Streams.Any())
				throw new ArgumentException(
					"There are no streams available.",
					nameof(streamManifest));

			// Use single muxed stream if adaptive streams are not available
			if (!streamManifest.GetAudioOnly().Any() || !streamManifest.GetVideoOnly().Any())
			{
				// Priority: video quality -> transcoding
				yield return streamManifest
					.GetMuxed()
					.OrderByDescending(s => s.VideoQuality)
					.ThenByDescending(s => !IsTranscodingRequired(s.Container, format))
					.First();

				yield break;
			}

			// Include audio stream
			// Priority: transcoding -> bitrate
			yield return streamManifest
				.GetAudioOnly()
				.OrderByDescending(s => !IsTranscodingRequired(s.Container, format))
				.ThenByDescending(s => s.Bitrate)
				.First();

			// Include video stream
			if (!format.IsAudioOnly)
			{
				// Priority: video quality -> framerate -> transcoding
				yield return streamManifest
					.GetVideoOnly()
					.OrderByDescending(s => s.VideoQuality)
					.ThenByDescending(s => s.Framerate)
					.ThenByDescending(s => !IsTranscodingRequired(s.Container, format))
					.First();
			}
		}
	}
}