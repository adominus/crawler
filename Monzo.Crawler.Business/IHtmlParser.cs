using System.Collections.Generic;

namespace Monzo.Crawler.Business
{
	public interface IHtmlParser
	{
		IEnumerable<Anchor> GetAnchors(string html);
	}
}
