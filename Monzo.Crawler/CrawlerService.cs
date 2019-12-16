using Microsoft.Extensions.Options;
using Monzo.Crawler.Domain;
using System;
using System.Threading.Tasks;

namespace Monzo.Crawler
{
	internal class CrawlerService
	{
		private readonly ISitemapGenerator _sitemapGenerator;
		private readonly ISitemapWriter _sitemapWriter;
		private readonly CrawlerOptions _crawlerOptions;

		public CrawlerService(
			ISitemapGenerator sitemapGenerator,
			ISitemapWriter sitemapWriter,
			IOptions<CrawlerOptions> crawlerOptions)
		{
			_sitemapGenerator = sitemapGenerator;
			_sitemapWriter = sitemapWriter;
			_crawlerOptions = crawlerOptions?.Value ?? throw new ArgumentNullException(nameof(crawlerOptions));
		}

		public async Task ExecuteAsync()
		{
			var result = await _sitemapGenerator.GenerateAsync(_crawlerOptions.Website);

			_sitemapWriter.Write(result);
		}
	}
}
