using System.Collections.Generic;

namespace Monzo.Crawler.Business.HtmlUtilties
{
	public interface IHtmlParser
	{
		IEnumerable<AnchorModel> GetAnchors(string html);
	}
}
