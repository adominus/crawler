using System;
using System.Collections.Generic;

namespace Monzo.Crawler.Domain
{
	public class Page
	{
		public Uri Address { get; set; }

		public IEnumerable<Uri> LinkedAddresses { get; set; }
	}
}
