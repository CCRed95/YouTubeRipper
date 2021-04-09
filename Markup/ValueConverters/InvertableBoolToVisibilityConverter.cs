using System.Windows;
using Ccr.Xaml.Markup.Converters.Infrastructure;

namespace YouTubeRipper.Markup.ValueConverters
{
	public class InvertableBoolToVisibilityConverter
		: XamlConverter<bool, ConverterParam<bool>, Visibility>
	{
		public override Visibility Convert(
			bool value,
			ConverterParam<bool> param)
		{
			if (param.Value)
				return value ? Visibility.Hidden : Visibility.Visible;
			
			return value ? Visibility.Visible : Visibility.Hidden;
		}
	}
}