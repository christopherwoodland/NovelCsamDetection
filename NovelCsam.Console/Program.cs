using Microsoft.Extensions.DependencyInjection;
using NovelCsamDetection.Helpers;
using NovelCsamDetection.Helpers.Interfaces;
using System;

namespace NovelCsam.Console
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			try
			{
				// Setup Dependency Injection
				var serviceCollection = new ServiceCollection();
				ConfigureServices(serviceCollection);
				var serviceProvider = serviceCollection.BuildServiceProvider();

				if (args.Length != 0 && args.Length == 7)
				{
					int frameInterval = Convert.ToInt16(args[0]);
					var fileName = args[1];
					var containerName = args[2];
					var containerFolderPath = args[3];
					var containerFolderPathExtracted = args[4];
					var containerFolderPathResults = args[5];
					var containerFolderPathSegmented = args[6];
					int splitInterval = Convert.ToInt16(args[7]);
					int jobType = Convert.ToInt16(args[8]);
					// Resolve and run the main application
					var app = serviceProvider.GetService<IVideoHelper>();

					if (app != null)
					{
						switch (jobType)
						{
							case 1:
								await app.UploadSegmentVideoToBlobAsync(splitInterval, fileName, containerName, containerFolderPath, containerFolderPathSegmented);
								break;
							case 2:
								await app.UploadExtractedFramesToBlobAsync(frameInterval, fileName, containerName, containerFolderPath, containerFolderPathExtracted);
								break;
							case 3: break;
							default: break;
						}

					}
				}
			}
			catch (Exception ex)
			{
				var serviceCollection = new ServiceCollection();
				ConfigureServices(serviceCollection);
				var serviceProvider = serviceCollection.BuildServiceProvider();
				var logHelper = serviceProvider.GetService<ILogHelper>();
				if (logHelper != null)
				{
					logHelper.LogException($"An error occurred during run in main: {ex.Message}", nameof(Program), nameof(Main), ex);
				}
			}
		}

		private static void ConfigureServices(IServiceCollection services)
		{
			// Register all classes from NovelCsamDetection.Helper
			// Add Application Insights telemetry with the connection string
			services.AddApplicationInsightsTelemetryWorkerService(options =>
			{
				options.ConnectionString = "e6f138fa-f1d7-43d3-94e4-a7373d488218;IngestionEndpoint=https://eastus2-3.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus2.livediagnostics.monitor.azure.com/;ApplicationId=11096425-5af5-4abe-b1b6-d2cf72715c1a";
			});

			services.AddScoped<ILogHelper, LogHelper>();
			services.AddScoped<IStorageHelper, StorageHelper>();
			services.AddTransient<IVideoHelper, VideoHelper>();

		}
	}
}