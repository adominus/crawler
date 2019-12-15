using System.Collections.Generic;
using System.Threading.Tasks;

namespace Monzo.Crawler.Domain
{
	public interface ISitemapGenerator
	{
		Task<IEnumerable<Page>> GenerateAsync(string website);
	}
}
