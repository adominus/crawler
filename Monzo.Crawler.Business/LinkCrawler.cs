using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Monzo.Crawler.Business
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
			await Task.FromResult(1);

			var body = await _httpClientService.GetHtmlBody(address);

			if (body != null)
			{
				return
					GetLinkedAddress(address, body)
						.Select(RemoveQueryStringAndFragment)
						// TODO: Does this work? 
						.Distinct();
			}

			return new Uri[0];
		}

		// TODO: Does this work? 
		private Uri RemoveQueryStringAndFragment(Uri uri)
			=> new Uri(uri, uri.LocalPath);

		private IEnumerable<Uri> GetLinkedAddress(Uri baseUri, string html)
		{
			// TODO: What if the html is not valid? 
			foreach (var href in _htmlParser.GetAnchors(html).Select(x => x.Href))
			{
				if (IsUriRelative(href))
				{
					yield return new Uri(baseUri, href);
				}
				else if (Uri.TryCreate(href, UriKind.Absolute, out Uri linkedUri))
				{
					if (linkedUri.Host == baseUri.Host)
					{
						yield return linkedUri;
					}
				}
			}
		}

		private bool IsUriRelative(string uri)
			=> Uri.IsWellFormedUriString(uri, UriKind.Relative);
	}
}
