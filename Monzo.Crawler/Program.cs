using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monzo.Crawler.Business;
using Monzo.Crawler.Domain;
using System;
using System.Net.Http;
using System.Text;
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

            services.AddScoped<ILinkCrawler, LinkCrawler>();

            //services.AddTransient<ITestService, TestService>();

            //services.AddTransient<LoggingDelegatingHandler>();

            //services.AddHttpClient<ITestService, TestService>(client =>
            //{
            //    client.BaseAddress = new Uri("https://www.bbc.co.uk/");
            //})
            //.AddHttpMessageHandler<LoggingDelegatingHandler>();
        }
    }

    //public interface IPrinter
    //{
    //    void PrintLine(string s);
    //}

    //public class ConsolePrinter : IPrinter
    //{
    //    public void PrintLine(string s)
    //    {
    //        Console.WriteLine(s);
    //    }
    //}

    //public class StringPrinter : IPrinter
    //{
    //    private StringBuilder _stringBuilder;

    //    public StringPrinter()
    //    {
    //        _stringBuilder = new StringBuilder();
    //    }

    //    public void PrintLine(string s)
    //    {
    //        _stringBuilder.Append(s);
    //    }
    //}

    //public class LoggingDelegatingHandler : DelegatingHandler
    //{
    //    private readonly ILogger<LoggingDelegatingHandler> logger;

    //    public LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger)
    //        : base()
    //    {
    //        this.logger = logger;
    //    }

    //    protected override Task<HttpResponseMessage> SendAsync(
    //        HttpRequestMessage request,
    //        CancellationToken cancellationToken)
    //    {
    //        logger.LogError("Inside the logging delegating handler!");

    //        return base.SendAsync(request, cancellationToken);
    //    }
    //}

    //public class TestService : ITestService
    //{
    //    private readonly HttpClient httpClient;

    //    public TestService(HttpClient httpClient, IOptions<CrawlerOptions> options)
    //    {
    //        this.httpClient = httpClient;
    //    }

    //    public Task<string> TestMethod()
    //    {
    //        return httpClient.GetStringAsync("");
    //    }
    //}

    //public interface ITestService
    //{
    //    Task<string> TestMethod();
    //}
}
