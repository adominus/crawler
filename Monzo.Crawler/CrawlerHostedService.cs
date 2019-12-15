using Microsoft.Extensions.Hosting;
using Monzo.Crawler.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Monzo.Crawler
{
	internal class CrawlerHostedService : IHostedService
	{
		private readonly ISitemapGenerator _sitemapGenerator;

		public CrawlerHostedService(ISitemapGenerator sitemapGenerator)
		{
			_sitemapGenerator = sitemapGenerator;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			// TODO: How do we want to get the domain into here? 
			await _sitemapGenerator.Generate("https://www.bbc.co.uk/");
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
