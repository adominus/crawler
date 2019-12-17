using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Monzo.Crawler.DelegatingHandlers
{
    public class PoliteDelegatingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(300);

            var result = await base.SendAsync(request, cancellationToken);

            return result;
        }
    }
}
