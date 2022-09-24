using System.Reflection;
using TwitterSample.Services.Cache;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

IConfiguration configuration;

if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
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

// Add Cache Service to IoC container
builder.Services.AddSingleton<ICacheService>(new CacheService(configuration["CacheConnectionString"]));

var app = builder.Build();

// Would normally only enable these for Development, but will be useful for a working demonstration
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
