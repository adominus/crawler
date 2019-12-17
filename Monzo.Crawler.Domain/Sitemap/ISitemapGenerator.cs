using System.Collections.Generic;
using System.Threading.Tasks;

namespace Monzo.Crawler.Domain.Sitemap
{
	public interface ISitemapGenerator
	{
		Task<IEnumerable<PageModel>> GenerateAsync(string website);
	}
}
