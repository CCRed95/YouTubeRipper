using System;
using Ccr.Xaml.Markup.Converters.Infrastructure;

namespace YouTubeRipper.Markup.ValueConverters
{
	public class DateTimeToSimpleDateConverter
		: XamlConverter<DateTimeOffset, NullParam, string>
	{
		public override string Convert(
			DateTimeOffset value,
			NullParam param)
		{
			return $"{value.Date:yyyy-MM-dd}";
		}
	}
}