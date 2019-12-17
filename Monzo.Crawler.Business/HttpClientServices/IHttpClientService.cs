using System;
using System.Threading.Tasks;

namespace Monzo.Crawler.Business.HttpClientServices
{
	public interface IHttpClientService
	{
		Task<string> GetHtmlBody(Uri uri);
	}
}
