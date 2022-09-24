using System.Reflection;
using WorkerService1;
using TwitterSample.Services.Cache;
using TwitterSample.API.Worker.Services.Twitter;

IConfiguration configuration;

if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development")
{
    var configBuilder = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddUserSecrets(Assembly.GetExecutingAssembly());

    configuration = configBuilder.Build();
}
else
{
    var configBuilder = new ConfigurationBuilder()
        .AddEnvironmentVariables();

    configuration = configBuilder.Build();
}

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddSingleton<ITwitterService>(new TwitterService(configuration["TwitterAuthUrl"], configuration["TwitterApiUrl"], configuration["TwitterKey"], configuration["TwitterSecret"]));
        services.AddSingleton<ICacheService>(new CacheService(configuration["CacheConnectionString"]));
    })
    .Build();

await host.RunAsync();
