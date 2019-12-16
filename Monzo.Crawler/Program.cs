using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Monzo.Crawler.Business;
using Monzo.Crawler.Domain;
using Polly;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Monzo.Crawler
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices)
                .UseConsoleLifetime()
                .Build();

            using var serviceScope = host.Services.CreateScope();

            var services = serviceScope.ServiceProvider;

            var crawlerService = services.GetRequiredService<CrawlerService>();

            await crawlerService.ExecuteAsync();
        }

        private static void ConfigureServices(HostBuilderContext hostBuilder, IServiceCollection services)
        {
            services.Configure<CrawlerOptions>(hostBuilder.Configuration);
            services.AddTransient<CrawlerService>();

            services.AddScoped<ISitemapGenerator, SitemapGenerator>();
            services.AddScoped<ISitemapWriter, SitemapWriter>();
            services.AddSingleton<ITextWriter, ConsoleWriter>();

            services.AddScoped<ILinkCrawler, LinkCrawler>();
            services.AddScoped<IHtmlParser, HtmlParser>();

            services.AddTransient<PoliteDelegatingHandler>();
            services.AddTransient<IHttpClientService, HttpClientService>();
            services.AddHttpClient<IHttpClientService, HttpClientService>()
                .AddHttpMessageHandler<PoliteDelegatingHandler>()
                .AddPolicyHandler(Policy.BulkheadAsync<HttpResponseMessage>(
                    maxParallelization: 100,
                    maxQueuingActions: int.MaxValue));
        }
    }

    public class PoliteDelegatingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var result = await base.SendAsync(request, cancellationToken);

            await Task.Delay(1000);

            return result;
        }
    }
}
