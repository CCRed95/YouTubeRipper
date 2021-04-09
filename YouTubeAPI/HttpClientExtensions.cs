using System.Net;

namespace YouTubeRipper.YouTubeAPI
{
	internal static class HttpClientExtensions
	{
		internal static CookieContainer WithCookie(
			this CookieContainer @this,
			string name,
			string value,
			string path = "/",
			string domain = ".youtube.com")
		{
			@this.Add(new Cookie(name, value, path, domain));
			return @this;
		}
	}
}