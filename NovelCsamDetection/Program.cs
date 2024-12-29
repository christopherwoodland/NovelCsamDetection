using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NovelCsamDetection.Helpers;

var builder = FunctionsApplication.CreateBuilder(args);

//builder.Configuration.AddAzureAppConfiguration(options =>
//{
//    options.Connect(Environment.GetEnvironmentVariable("APP_CONFIG"))
//           .Select(KeyFilter.Any, LabelFilter.Null)
//           .ConfigureRefresh(refreshOptions =>
//           {
//               refreshOptions.Register("NOVEL:*", refreshAll: true);
//           });
//});


builder.Services.AddScoped<IAzureContainerAppJobHelper, AzureContainerAppJobHelper>();
builder.Services.AddScoped<ILogHelper, LogHelper>();
builder.Services.AddSingleton<HttpClient>();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

var app = builder.Build();
app.Run();