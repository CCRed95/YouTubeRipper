using Ccr.Xaml.Markup.Converters.Infrastructure;

namespace YouTubeRipper.Markup.ValueConverters
{
	public class DoubleToPercentageConverter
		: XamlConverter<double, NullParam, string>
	{
		public override string Convert(
			double value,
			NullParam param)
		{
			return $"{value:###.###} %";
		}
	}
}