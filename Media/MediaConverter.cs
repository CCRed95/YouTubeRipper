using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ccr.Std.Core.Extensions;
using FFmpeg.NET;
using FFmpeg.NET.Events;
using JetBrains.Annotations;
using YouTubeRipper.Extensions;

namespace YouTubeRipper.Media
{
	public class MediaConverter
	{
		public static async Task<FileInfo> ConvertMP4ToMp3Async(
			[NotNull] FileInfo sourceVideoMp4File,
			EventHandler<ConversionProgressEventArgs> progressUpdated = null)
		{
			sourceVideoMp4File.IsNotNull(nameof(sourceVideoMp4File));

			var targetAudioName = sourceVideoMp4File.GetFileNameWithoutExtension();

			if (sourceVideoMp4File.Directory == null)
				throw new DirectoryNotFoundException(
					$"Source video file directory is null.");

			var targetAudioFile = new FileInfo(
				$@"{sourceVideoMp4File.Directory.FullName}\{targetAudioName}.mp3");

			var ffmpegPath = AppDomain.CurrentDomain.BaseDirectory + "ffmpeg.exe";

			var inputFile = new MediaFile(sourceVideoMp4File.FullName);
			var outputFile = new MediaFile(targetAudioFile.FullName);

			var ffmpeg = new Engine(ffmpegPath);

			var cancellationToken = new CancellationToken();


			void onFFMpegProgress(object sender, ConversionProgressEventArgs args)
			{
				progressUpdated?.Invoke(sender, args);
			}

			void onFFMpegError(object sender, ConversionErrorEventArgs args)
			{
				var ffmpegEngine = sender.As<Engine>();
				ffmpegEngine.Error -= onFFMpegError;

				Console.WriteLine($"FFMpeg Error: {args.Exception}");
			}

			void onFFMpegComplete(object sender, ConversionCompleteEventArgs args)
			{
				ffmpeg.Error -= onFFMpegError;
				ffmpeg.Progress -= onFFMpegProgress;
				ffmpeg.Complete -= onFFMpegComplete;
			}

			ffmpeg.Error += onFFMpegError;
			ffmpeg.Progress += onFFMpegProgress;
			ffmpeg.Complete += onFFMpegComplete;

			var result = await ffmpeg.ConvertAsync(
				inputFile,
				outputFile,
				cancellationToken);

			return result.FileInfo;
		}
	}
}


/*

old
		public static FileInfo ConvertMP4ToMp3(
			[NotNull] FileInfo sourceVideoMp4File)
		{
			sourceVideoMp4File.IsNotNull(nameof(sourceVideoMp4File));

			var targetAudioName = sourceVideoMp4File.GetFileNameWithoutExtension();

			if (sourceVideoMp4File.Directory == null)
				throw new DirectoryNotFoundException(
					$"Source video file directory is null.");

			var targetAudioFile = new FileInfo(
				$@"{sourceVideoMp4File.Directory.FullName}\{targetAudioName}.mp3");

			var ffmpegPath = AppDomain.CurrentDomain.BaseDirectory + "ffmpeg.exe";

			var command =
				$"-i {sourceVideoMp4File.FullName.Quote()} " +
				$"-q:a " +
				$"0 " +
				$"-map " +
				$"a {targetAudioFile.FullName.Quote()}";


			using var conversionProcess = new Process
			{
				StartInfo =
				{
					UseShellExecute = false,
					RedirectStandardOutput = false,
					FileName = ffmpegPath,
					Arguments = command,
					CreateNoWindow = true,
				}
			};

			conversionProcess.Start();
			conversionProcess.WaitForExit();

			return targetAudioFile;
		}



using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Ccr.Std.Core.Extensions;
using JetBrains.Annotations;
using YouTubeRipper.Extensions;

namespace YouTubeRipper.Media
{
  public class MediaConverter
  {
    public static async Task<FileInfo> ConvertMP4ToMp3(
      [NotNull] FileInfo sourceVideoMp4File)
    {
      sourceVideoMp4File.IsNotNull(nameof(sourceVideoMp4File));

      var backgroundWorker = new BackgroundWorker();

      var targetAudioName = sourceVideoMp4File.GetFileNameWithoutExtension();

      if (sourceVideoMp4File.Directory == null)
        throw new DirectoryNotFoundException(
          $"Source video file directory is null.");

      var targetAudioFile = new FileInfo(
        $@"{sourceVideoMp4File.Directory.FullName}\{targetAudioName}.mp3");

      var ffmpegPath = AppDomain.CurrentDomain.BaseDirectory + "ffmpeg.exe";

      var command =
        $"-i {sourceVideoMp4File.FullName.Quote()} " +
        $"-q:a " +
        $"0 " +
        $"-map " +
        $"a {targetAudioFile.FullName.Quote()}";


      using var conversionProcess = new Process
      {
        StartInfo =
        {
          UseShellExecute = false,
          RedirectStandardOutput = false,
          FileName = ffmpegPath,
          Arguments = command,
          CreateNoWindow = true,
        }
      };

      conversionProcess.Start();

      var fileSizeBytes = sourceVideoMp4File.Length;
      
      var progress = 0;
      var processedBytes = 0l;

      using (var inputFile = new FileStream(
        targetAudioFile.FullName, 
        FileMode.Open, 
        FileAccess.Read,
        FileShare.None))
      {
        using (var outputFile = new FileStream(
          targetAudioFile.FullName, 
          FileMode.Create,
          FileAccess.Write,
          FileShare.None))
        {
          int byteSize;
          var byteBuffer = new byte[fileSizeBytes];

          while ((byteSize = inputFile.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
          {                       
            double currentProcessedBytes = processedBytes;
            double byteBufferTotal = byteBuffer.Length;
            var progressPercentage = currentProcessedBytes / byteBufferTotal;

            outputFile.Write(byteBuffer, 0, byteSize);
            processedBytes += byteSize;
            progress = Convert.ToUInt16(progressPercentage);

            // Update the progress bar.
            backgroundWorker.ReportProgress(
              (int)(100 * processedBytes / fileSizeBytes));
          }
          outputFile.Close();
        }
        inputFile.Close();
      }

      while (conversionProcess.HasExited == false)
      {
      }

      conversionProcess.WaitForExit();

      return targetAudioFile;
    }
  }
}
*/
