using Azure.Identity;
using Microsoft.FeatureManagement;

namespace FeatureFlagDemoWithAppConfig;

public class Program
{
    enum Features
    {
        BetaFeature
    }
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        string appConfigEndpoint = builder.Configuration.GetConnectionString("AppConfig")!;

        builder.Configuration.AddAzureAppConfiguration(options =>
        {
            var credential = new DefaultAzureCredential();

            options.Connect(new Uri(appConfigEndpoint), credential)
                .UseFeatureFlags(flagOptions =>
                {
                    flagOptions.CacheExpirationInterval = TimeSpan.FromSeconds(3);
                });
        });

        // Register the Feature Management and Application Configuration librarys services
        builder.Services.AddFeatureManagement();
        builder.Services.AddAzureAppConfiguration();

        // Add services to the container.
        builder.Services.AddAuthorization();

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseHttpsRedirection();

        app.UseAuthorization();
        app.UseAzureAppConfiguration();

        // Feature flag-controlled endpoint
        app.MapGet("/api/feature", async (IFeatureManager featureManager) =>
        {
            if (await featureManager.IsEnabledAsync(nameof(Features.BetaFeature)))
            {
                return Results.Ok("Beta feature is Online");
            }
            else
            {
                return Results.Ok("Beta feature is Offline.");
            }
        });
        app.Run();
    }
}
