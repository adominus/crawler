using System;
using System.Threading.Tasks;

namespace Monzo.Crawler.Business
{
	public interface IHttpClientService
	{
		Task<string> GetHtmlBody(Uri uri);
	}
}
