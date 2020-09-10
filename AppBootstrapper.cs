using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using YouTubeRipper.ViewModels;

namespace YouTubeRipper
{
	public class AppBootstrapper
		: BootstrapperBase
	{
		public AppBootstrapper()
		{
			Initialize();
		}


		protected override void OnStartup(object sender, StartupEventArgs args)
		{
			var settings = new Dictionary<string, object>
			{
				{ "SizeToContent", SizeToContent.Manual },
				{ "WindowState" , WindowState.Maximized },
				{ "Height", 1280 },
				{ "Width", 720 }
			};

			DisplayRootViewFor<RootViewModel>(settings);
		}
	}
}