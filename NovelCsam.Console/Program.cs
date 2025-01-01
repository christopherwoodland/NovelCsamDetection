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
				

				if (args.Length != 0 && args.Length == 10)
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
					string sourceFileName = args[9];


					// Resolve and run the main application
					var serviceProvider = serviceCollection.BuildServiceProvider();
					var app = serviceProvider.GetService<IVideoHelper>();

					if (app != null)
					{
						switch (jobType)
						{
							case 1:
								await app.UploadSegmentVideoToBlobAsync(splitInterval, fileName, containerName, containerFolderPath, containerFolderPathSegmented);
								break;
							case 2:
								await app.UploadExtractedFramesToBlobAsync(frameInterval, fileName, containerName, containerFolderPath, containerFolderPathExtracted, sourceFileName);
								break;
							case 3:
								await app.UploadFrameResultsToBlobAsync(containerName, containerFolderPath, containerFolderPathResults, sourceFileName,true);
								break;
							case 4:
								await app.UploadImageOnlyFrameResultsToBlobAsync(containerName, containerFolderPath, containerFolderPathResults, sourceFileName, true);
								break;

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
				logHelper?.LogException($"An error occurred during run in main: {ex.Message}", nameof(Program), nameof(Main), ex);
			}
		}

		private static void ConfigureServices(IServiceCollection services)
		{
			// Register all classes from NovelCsamDetection.Helper
			// Add Application Insights telemetry with the connection string
			services.AddApplicationInsightsTelemetryWorkerService(options =>
			{
				options.ConnectionString = Environment.GetEnvironmentVariable("APP_INSIGHTS_CONNECTION_STRING");
			});

			services.AddTransient<IAzureSQLHelper, AzureSQLHelper>();
			services.AddScoped<ILogHelper, LogHelper>();
			services.AddScoped<IContentSafteyHelper, ContentSafteyHelper>();
			services.AddScoped<IStorageHelper, StorageHelper>();
			services.AddTransient<IVideoHelper, VideoHelper>();
			//services.AddTransient<ICosmosDBHelper, CosmosDBHelper>();
		}
	}
}