using System.Net;
using System.Net.Http;

namespace YouTubeRipper.YouTubeAPI
{
	public class YouTubeAuthorizedHttpClient 
		: HttpClient
	{
		private readonly HttpClientHandler _httpClientHandler;


		public YouTubeAuthorizedHttpClient()
		{
			var cookieContainer = new CookieContainer();

			cookieContainer
				.WithCookie("APISID", "")
				.WithCookie("HSID", "")
				.WithCookie("PREF", "")
				.WithCookie("SAPISID", "")
				.WithCookie("SID", "")
				.WithCookie("SIDCC", "")
				.WithCookie("VISITOR_INFO1_LIVE", "")
				.WithCookie("YSC", "")
				.WithCookie("__Secure-3PAPISID", "")
				.WithCookie("__Secure-3PSID", "")
				.WithCookie("__Secure-3PSIDCC", "");
			
			_httpClientHandler = new HttpClientHandler
			{
				CookieContainer = cookieContainer
			};
		}


		protected override void Dispose(bool disposing)
		{
			_httpClientHandler.Dispose();
			
			base.Dispose(disposing);
		}
	}
}