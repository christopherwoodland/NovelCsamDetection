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
				Environment.SetEnvironmentVariable("AZURE_SQL_CONNECTION_STRING", "Server=tcp:df-sql-server-eastus-001.database.windows.net,1433;Initial Catalog=df-sql-eastus-001;Persist Security Info=False;User ID=cwoodland;Password=django20221!!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
				Environment.SetEnvironmentVariable("CONTENT_SAFETY_CONNECTION_STRING", "https://cwcontentsafetyeastus2001.cognitiveservices.azure.com/");
				Environment.SetEnvironmentVariable("CONTENT_SAFETY_CONNECTION_KEY", "2b69c5c74f0642c9bc5cf713111e5bf9");
				Environment.SetEnvironmentVariable("COSMOS_DB_CONNECTION_STRING", "AccountEndpoint=https://igital-forensics-eastus-001.documents.azure.com:443/;AccountKey=QGcPlwvi2dq5gFSJheXIF9rupVC1vh6bQqqucgkuU4cGh7O5bQCsV7FdZZS2wFJWyNetwhouv27xACDbTrvj2A==;");
				Environment.SetEnvironmentVariable("COSMOS_DB_DATABASE_NAME", "Forensics");
				Environment.SetEnvironmentVariable("COSMOS_DB_CONTAINER_NAME", "Images");
				Environment.SetEnvironmentVariable("STORAGE_ACCOUNT_NAME", "digitalforensicsstg");
				Environment.SetEnvironmentVariable("STORAGE_ACCOUNT_KEY", "IGnMXV0C6jZrPX3RapNgHDtfY0OI8M20Y9K+qOG6Zm0mvvRdzUJOHHIkae0VjTKoIWRugb82cFi7+AStRuP/MQ==");
				Environment.SetEnvironmentVariable("STORAGE_ACCOUNT_URL", "https://digitalforensicsstg.dfs.core.windows.net");
				Environment.SetEnvironmentVariable("OPEN_AI_DEPLOYMENT_NAME", "gpt-4o");
				Environment.SetEnvironmentVariable("OPEN_AI_KEY", "89a35462495b4448b433e57d092397e3");
				Environment.SetEnvironmentVariable("OPEN_AI_ENDPOINT", "https://openai-sesame-eastus-001.openai.azure.com/");
				Environment.SetEnvironmentVariable("OPEN_AI_MODEL", "gpt-4o");
				Environment.SetEnvironmentVariable("APP_INSIGHTS_CONNECTION_STRING", "e6f138fa-f1d7-43d3-94e4-a7373d488218;IngestionEndpoint=https://eastus2-3.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus2.livediagnostics.monitor.azure.com/;ApplicationId=11096425-5af5-4abe-b1b6-d2cf72715c1a");


				if (args.Length != 0 && args.Length == 10)
				{
					int frameInterval = Convert.ToInt16(args[0]);
					var fileName = args[1];
					var containerNameVideos = args[2];
					var containerFolderPathInput = args[3];
					var containerFolderPathExtracted = args[4];
					var containerFolderPathResults = args[5];
					var containerFolderPathSegmented = args[6];
					int splitInterval = Convert.ToInt16(args[7]);
					int jobType = Convert.ToInt16(args[8]);
					string sourceFileNameOrPath = args[9];


					// Resolve and run the main application
					var serviceProvider = serviceCollection.BuildServiceProvider();
					var app = serviceProvider.GetService<IVideoHelper>();

					if (app != null)
					{
						switch (jobType)
						{
							case 1:
								await app.UploadSegmentVideoToBlobAsync(splitInterval, fileName, containerNameVideos, containerFolderPathInput, containerFolderPathSegmented);
								break;
							case 2:
								await app.UploadExtractedFramesToBlobAsync(frameInterval, fileName, containerNameVideos, containerFolderPathInput, containerFolderPathExtracted, sourceFileNameOrPath);
								break;
							case 3:
								await app.UploadFrameResultsToBlobAsync(containerNameVideos, containerFolderPathInput, containerFolderPathResults, true);
								break;
							case 4:
								await app.UploadFrameResultsToBlobAsync(containerNameVideos, containerFolderPathInput, containerFolderPathResults, true);
								break;
							case 5:
								await app.UploadVideoToBlobAsync(containerNameVideos, containerFolderPathInput, sourceFileNameOrPath);
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