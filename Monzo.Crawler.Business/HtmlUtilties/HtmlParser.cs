using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace Monzo.Crawler.Business.HtmlUtilties
{
	public class HtmlParser : IHtmlParser
	{
		public IEnumerable<AnchorModel> GetAnchors(string html)
		{
			if (string.IsNullOrWhiteSpace(html))
			{
				return null;
			}

			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(html);

			var anchorTags = htmlDocument?.DocumentNode?.SelectNodes("//a");

			if (anchorTags == null)
			{
				return Enumerable.Empty<AnchorModel>();
			}

			return anchorTags
				.Select(x => x.GetAttributeValue("href", null))
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Select(x => new AnchorModel
				{
					Href = x
				});
		}
	}
}
