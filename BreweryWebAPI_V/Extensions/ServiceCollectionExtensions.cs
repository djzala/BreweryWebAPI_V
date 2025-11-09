using BreweryWebAPI_V.Clients;
using BreweryWebAPI_V.Configuration;

namespace BreweryWebAPI_V.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureBreweryOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BreweryOptions>(configuration.GetSection("BreweryOptions"));
            return services;
        }

        public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetSection("BreweryOptions").Get<BreweryOptions>() ?? new BreweryOptions();

            services.AddHttpClient<IOpenBreweryClient, OpenBreweryClient>(client =>
            {
                client.BaseAddress = new Uri(options.OpenBreweryBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.HttpClientTimeoutSeconds);
                client.DefaultRequestHeaders.Add("User-Agent", "BrewApi/1.0");
            })
            .AddHttpMessageHandler(() => new SimpleRetryHandler());

            return services;
        }

        // Very small delegating handler for 1 retry; replace with Polly in production
        private class SimpleRetryHandler : DelegatingHandler
        {
            public SimpleRetryHandler()
            {
                InnerHandler = new HttpClientHandler();
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                int attempts = 0;
                while (true)
                {
                    attempts++;
                    try
                    {
                        var resp = await base.SendAsync(request, cancellationToken);
                        if ((int)resp.StatusCode >= 500 && attempts < 2)
                        {
                            await Task.Delay(200, cancellationToken);
                            continue;
                        }
                        return resp;
                    }
                    catch when (attempts < 2)
                    {
                        await Task.Delay(200, cancellationToken);
                        continue;
                    }
                }
            }
        }
    }
}
