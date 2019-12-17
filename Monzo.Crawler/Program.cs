using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Monzo.Crawler.Business.HtmlUtilties;
using Monzo.Crawler.Business.HttpClientServices;
using Monzo.Crawler.Business.Sitemap;
using Monzo.Crawler.Configuration;
using Monzo.Crawler.DelegatingHandlers;
using Monzo.Crawler.Domain;
using Monzo.Crawler.Domain.Sitemap;
using Polly;
using System.Net.Http;
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
                    maxParallelization: 10,
                    maxQueuingActions: int.MaxValue));
        }
    }
}
