using Monzo.Crawler.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Monzo.Crawler.Business
{
	public class SitemapWriter : ISitemapWriter
	{
		private readonly ITextWriter _textWriter;

		public SitemapWriter(ITextWriter textWriter)
		{
			_textWriter = textWriter;
		}

		public void Write(IEnumerable<Page> pages)
		{
			foreach (var page in pages
				.Where(x => x.Address != null)
				.OrderBy(x => x.Address.AbsoluteUri))
			{
				_textWriter.WriteLine(page.Address.AbsoluteUri);

				int linkedAddressCount = page.LinkedAddresses?.Count() ?? 0;

				if (linkedAddressCount == 0)
				{
					_textWriter.WriteLine("- No Links");
				}
				else
				{
					_textWriter.WriteLine($"- {linkedAddressCount} Link{(linkedAddressCount == 1 ? "" : "s")}: ");

					foreach (var linkedAddress in page.LinkedAddresses.OrderBy(x => x.AbsoluteUri))
					{
						_textWriter.WriteLine($"	- {linkedAddress.AbsoluteUri}");
					}

				}
			}
		}
	}
}
