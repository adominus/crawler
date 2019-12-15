﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Monzo.Crawler.Business
{
	public interface ILinkCrawler
	{
		Task<IEnumerable<Uri>> FindLinksOnSameDomain(Uri address);
	}
}