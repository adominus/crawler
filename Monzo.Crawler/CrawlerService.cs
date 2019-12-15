using Microsoft.Extensions.Options;
using Monzo.Crawler.Domain;
using System;
using System.Threading.Tasks;

namespace Monzo.Crawler
{
	internal class CrawlerService
	{
		private readonly ISitemapGenerator _sitemapGenerator;
		private readonly CrawlerOptions _crawlerOptions;

		public CrawlerService(
			ISitemapGenerator sitemapGenerator,
			IOptions<CrawlerOptions> crawlerOptions)
		{
			_sitemapGenerator = sitemapGenerator;
			_crawlerOptions = crawlerOptions?.Value ?? throw new ArgumentNullException(nameof(crawlerOptions));
		}

		public async Task ExecuteAsync()
		{
			await _sitemapGenerator.GenerateAsync(_crawlerOptions.Website);
		}
	}
}
