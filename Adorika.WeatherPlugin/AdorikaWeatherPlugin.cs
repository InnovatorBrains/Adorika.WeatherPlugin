using SharedKernel.Abstractions;
using Microsoft.Extensions.DependencyInjection;
namespace SamplePlugin;

/// <summary>
/// Sample plugin that demonstrates a simple API endpoint
/// </summary>
public class AdorikaWeatherPlugin : IPlugin
{
    private readonly List<WeatherForecast> _forecasts = new();

    public string Id => "adorika-weather-plugin";
    public string Name => "Weather Plugin";
    public string Version => "1.0.0";
    public string Description => "A sample plugin that provides weather forecast endpoints";

    public async Task InitializeAsync(IPluginHost host)
    {
        host.LogInformation($"Initializing {Name} v{Version}");
        
        // Initialize some sample data
        _forecasts.AddRange(GenerateSampleForecasts());
        
        host.LogInformation($"{Name} initialized successfully");
        await Task.CompletedTask;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Register plugin-specific services
        services.AddSingleton<IWeatherService, WeatherService>();
        services.AddSingleton(this); // Register the plugin instance itself
    }

    public void ConfigureEndpoints(IPluginEndpointBuilder endpointBuilder)
    {
        // Plugin's API endpoints using the abstracted interface
        endpointBuilder.MapGet("/api/weather/forecast", async () => 
        {
            return await GetWeatherForecastAsync();
        });

        endpointBuilder.MapGet("/api/weather/current", async () => 
        {
            return await GetCurrentWeatherAsync();
        });

        endpointBuilder.MapPost("/api/weather/forecast", async (object request) => 
        {
            // For now, just return a success message
            return new { message = "Weather forecast updated", timestamp = DateTime.UtcNow };
        });

        endpointBuilder.MapGet("/api/weather/info", async () => 
        {
            return await GetPluginInfoAsync();
        });
    }

    public async Task DisposeAsync()
    {
        // Cleanup resources
        _forecasts.Clear();
        await Task.CompletedTask;
    }

    private async Task<object> GetWeatherForecastAsync()
    {
        var forecasts = _forecasts.Any() ? _forecasts : GenerateSampleForecasts();
        return await Task.FromResult(forecasts);
    }

    private async Task<object> GetCurrentWeatherAsync()
    {
        var current = new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = "Current Weather"
        };
        return await Task.FromResult(current);
    }

    private async Task<object> GetPluginInfoAsync()
    {
        return await Task.FromResult(new
        {
            Id,
            Name,
            Version,
            Description,
            Author = "Sample Plugin Developer",
            Endpoints = new[]
            {
                "/api/weather/forecast",
                "/api/weather/current",
                "/api/weather/info"
            }
        });
    }

    private static List<WeatherForecast> GenerateSampleForecasts()
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = summaries[Random.Shared.Next(summaries.Length)]
        }).ToList();
    }
}

/// <summary>
/// Weather forecast data model
/// </summary>
public class WeatherForecast
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string? Summary { get; set; }
}

/// <summary>
/// Weather service interface
/// </summary>
public interface IWeatherService
{
    Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days = 5);
    Task<WeatherForecast> AddForecastAsync(WeatherForecast forecast);
}

/// <summary>
/// Weather service implementation
/// </summary>
public class WeatherService : IWeatherService
{
    private readonly List<WeatherForecast> _forecasts = new();

    public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days = 5)
    {
        // In a real implementation, this would fetch from a database or external API
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        var forecasts = Enumerable.Range(1, Math.Min(days, 30)).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = summaries[Random.Shared.Next(summaries.Length)]
        }).ToList();

        return await Task.FromResult(forecasts);
    }

    public async Task<WeatherForecast> AddForecastAsync(WeatherForecast forecast)
    {
        // In a real implementation, this would save to a database
        _forecasts.Add(forecast);
        return await Task.FromResult(forecast);
    }
}
