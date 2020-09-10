using System;
using System.Windows.Media;
using Ccr.Std.Core.Extensions;
using Ccr.Xaml.Markup.Converters.Infrastructure;

namespace YouTubeRipper.Markup.ValueConverters
{
	public class UrlToImageSourceConverter
		: XamlConverter<string, NullParam, ImageSource>
	{
		private static readonly ImageSourceConverter _converter = new ImageSourceConverter();


		public override ImageSource Convert(
			string url, 
			NullParam param)
		{
			try
			{
				var imageSource = _converter.ConvertFrom(url);
				return imageSource.As<ImageSource>();
			}
			catch (Exception ex)
			{
				return null;
			}
		}
	}
}
