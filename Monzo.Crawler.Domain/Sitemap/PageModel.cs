using System;
using System.Collections.Generic;

namespace Monzo.Crawler.Domain.Sitemap
{
	public class PageModel
	{
		public Uri Address { get; set; }

		public IEnumerable<Uri> LinkedAddresses { get; set; }
	}
}
