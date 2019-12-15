using Monzo.Crawler.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Monzo.Crawler.Business
{
	public class SitemapGenerator : ISitemapGenerator
	{
		private readonly ILinkCrawler _linkCrawler;

		private ConcurrentDictionary<string, Page> _sitemap;

		public SitemapGenerator(ILinkCrawler linkCrawler)
		{
			_linkCrawler = linkCrawler;
		}

		public async Task<IEnumerable<Page>> GenerateAsync(string website)
		{
			if (!Uri.TryCreate(website, UriKind.Absolute, out Uri websiteUri))
			{
				throw new ArgumentException(nameof(website));
			}

			_sitemap = new ConcurrentDictionary<string, Page>();

			await TryCrawl(websiteUri);

			return _sitemap.Select(x => x.Value);
		}

		private Task TryCrawl(Uri uri)
		{
			if (_sitemap.TryAdd(uri.AbsoluteUri, null))
			{
				return Crawl(uri);
			}

			return Task.FromResult(1);
		}

		private async Task Crawl(Uri uri)
		{
			var links = await _linkCrawler.FindLinksOnSameDomain(uri);

			var page = new Page
			{
				Address = uri,
				LinkedAddresses = links
			};

			_sitemap.TryUpdate(
				key: uri.AbsoluteUri,
				newValue: page,
				comparisonValue: null);

			await Task.WhenAll(links?.Select(link => TryCrawl(link)) ?? new Task[0]);
		}
	}
}
