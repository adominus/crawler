using Monzo.Crawler.Domain;
using System;

namespace Monzo.Crawler
{
	public class ConsoleWriter : ITextWriter
    {
        public void WriteLine(string s)
        {
            Console.WriteLine(s);
        }
    }

    //public class PoliteDelegatingHandler : DelegatingHandler
    //{
    //    protected override async Task<HttpResponseMessage> SendAsync(
    //        HttpRequestMessage request,
    //        CancellationToken cancellationToken)
    //    {
    //        var result =  await base.SendAsync(request, cancellationToken);

    //        await Task.Delay(1000);

    //        return result;
    //    }
    //}
}
