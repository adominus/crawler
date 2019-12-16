using System.Collections.Generic;

namespace Monzo.Crawler.Domain
{
	public interface ISitemapWriter
	{
		void Write(IEnumerable<Page> pages);
	}
}
