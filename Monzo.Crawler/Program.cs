using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            await Task.FromResult(1);
            Console.WriteLine("Hello World!");

            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.1


            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                {
                    services.AddTransient<ITestService, TestService>();

                    services.AddTransient<LoggingDelegatingHandler>();

                    services.AddHttpClient<ITestService, TestService>(client =>
                    {
                        client.BaseAddress = new Uri("https://www.bbc.co.uk/");
                    })
                    .AddHttpMessageHandler<LoggingDelegatingHandler>();
                })
                .UseConsoleLifetime();

            var host = builder.Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;

                try
                {
                    var myService = services.GetRequiredService<ITestService>();
                    var result = await myService.TestMethod();

                    //Console.WriteLine(result);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();

                    logger.LogError(ex, "An error occurred.");
                }
            }
        }
    }

    public interface IPrinter
    {
        void PrintLine(string s);
    }

    public class ConsolePrinter : IPrinter
    {
        public void PrintLine(string s)
        {
            Console.WriteLine(s);
        }
    }

    public class StringPrinter : IPrinter
    {
        private StringBuilder _stringBuilder;

        public StringPrinter()
        {
            _stringBuilder = new StringBuilder();
        }

        public void PrintLine(string s)
        {
            _stringBuilder.Append(s);
        }
    }

    public class LoggingDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger<LoggingDelegatingHandler> logger;

        public LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger)
            : base()
        {
            this.logger = logger;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            logger.LogError("Inside the logging delegating handler!");

            return base.SendAsync(request, cancellationToken);
        }
    }

    public class TestService : ITestService
    {
        private readonly HttpClient httpClient;

        public TestService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public Task<string> TestMethod()
        {
            return httpClient.GetStringAsync("");
        }
    }

    public interface ITestService
    {
        Task<string> TestMethod();
    }
}
