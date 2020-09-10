using System;
using System.Text.RegularExpressions;

namespace YouTubeRipper.Media
{
	public class DateTimeRecognizer
	{
		private static readonly Regex _numericDateRegex = new Regex(
			@"([\d]{1,2})[ ]?[-|\/][ ]?([\d]{1,2})[ ]?[-|\/][ ]?([\d]{2,4})");

		private static readonly Regex _longformDateRegex = new Regex(
			@"(Saturday|Sunday|Monday|Tuesday|Wednesday|Friday|Sat|Sun|Mon|Tues|Tue|Wed|Thur|Thurs|Fri)?[.,]?[ ]?(January|February|March|April|May|June|July|August|September|October|November|December|Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sept|Sep|Oct|Nov|Dec)[.,]?[ ]?([\d]{1,2})[.,]?[ ]?([\d]{2,4})");


		public static bool TryExtractDate(
			string inputText,
			out string extractedText,
			out DateTime? dateTime)
		{
			if (_numericDateRegex.IsMatch(inputText))
			{
				var match = _numericDateRegex.Match(inputText);
				
				var matchStartIndex = match.Index;
				var matchLength = match.Length;

				var dateString = inputText.Substring(
					matchStartIndex, 
					matchLength);

				var formattedSring = inputText.Replace(dateString, "");

				if (!DateTime.TryParse(dateString, out var date))
				{
					extractedText = null;
					dateTime = null;

					return false;
				}

				extractedText = formattedSring;
				dateTime = date;

				return true;
			}
			if (_longformDateRegex.IsMatch(inputText))
			{
				var match = _longformDateRegex.Match(inputText);

				var matchStartIndex = match.Index;
				var matchLength = match.Length;

				var dateString = inputText.Substring(
					matchStartIndex,
					matchLength);

				var formattedSring = inputText.Replace(dateString, "");

				if (!DateTime.TryParse(dateString, out var date))
				{
					extractedText = null;
					dateTime = null;

					return false;
				}

				extractedText = formattedSring;
				dateTime = date;

				return true;
			}

			extractedText = null;
			dateTime = null;

			return false;
		}
	}
}
