using System.Collections.Generic;

namespace Monzo.Crawler.Domain.Sitemap
{
	public interface ISitemapWriter
	{
		void Write(IEnumerable<PageModel> pages);
	}
}
