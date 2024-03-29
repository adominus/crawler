﻿using Monzo.Crawler.Business.HtmlUtilties;
using Monzo.Crawler.Business.HttpClientServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Monzo.Crawler.Business.Sitemap
{
	public class LinkCrawler : ILinkCrawler
	{
		private readonly IHttpClientService _httpClientService;
		private readonly IHtmlParser _htmlParser;

		public LinkCrawler(
			IHttpClientService httpClientService,
			IHtmlParser htmlParser)
		{
			_httpClientService = httpClientService;
			_htmlParser = htmlParser;
		}

		public async Task<IEnumerable<Uri>> FindLinksOnSameDomain(Uri address)
		{
			var body = await _httpClientService.GetHtmlBody(address);

			if (body != null)
			{
				return
					GetLinkedAddresses(address, body)
						.Select(SanitizeAddress)
						.Distinct()
						.ToList();
			}

			return Enumerable.Empty<Uri>();
		}

		private IEnumerable<Uri> GetLinkedAddresses(Uri baseUri, string html)
		{
			foreach (var href in _htmlParser.GetAnchors(html)?.Select(x => x.Href) ?? Enumerable.Empty<string>())
			{
				if (Uri.TryCreate(baseUri, href, out Uri linkedUri))
				{
					if (linkedUri.Host == baseUri.Host)
					{
						yield return linkedUri;
					}
				}
			}
		}

		private Uri SanitizeAddress(Uri uri)
			=> new Uri(uri, uri.LocalPath.TrimEnd('/'));
	}
}
