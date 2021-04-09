using System;
using Ccr.Xaml.Markup.Converters.Infrastructure;

namespace YouTubeRipper.Markup.ValueConverters
{
	public class TimeSpanSimpleConverter
		: XamlConverter<TimeSpan, NullParam, string>
	{
		public override string Convert(
			TimeSpan value,
			NullParam param)
		{
			return // $"{value.Days:##}" +
				($"{value.Hours:#0}" +
					$":{value.Minutes:00}" +
					$":{value.Seconds:00}")
				.Trim(' ', ',', '-', '_');
		}
	}
}