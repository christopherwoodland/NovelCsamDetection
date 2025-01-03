internal class Program
{
	private const int FilesPerFolder = 100;

	private static string GenerateFolderName(int folderIndex) => $"Folder_{folderIndex}";

	private static async Task<bool> SelectAndUploadImagesAsync(IVideoHelper videoHelper, string containerName, string inputFolder)
	{
		using var folderBrowserDialog = new FolderBrowserDialog
		{
			Description = "Select a folder containing images",
			RootFolder = Environment.SpecialFolder.MyComputer
		};

		if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
		{
			Console.WriteLine("No folder selected.");
			return false;
		}

		string selectedFolderPath = folderBrowserDialog.SelectedPath;
		Console.WriteLine($"----------------------------------------------------------------------------\n");
		Console.WriteLine($"Selected folder: {selectedFolderPath}");

		var imageFiles = Directory.GetFiles(selectedFolderPath, "*.*")
								  .Where(file => new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff" }
								  .Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
								  .ToList();

		if (imageFiles.Count == 0)
		{
			Console.WriteLine("No image files found in the selected folder.");
			return false;
		}

		Console.WriteLine("Enter a custom folder name please...");
		var customFolderName = Console.ReadLine();
		int folderIndex = 1;
		string currentFolderName = GenerateFolderName(folderIndex);
		string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

		var uploadTasks = imageFiles.Select((imageFile, index) =>
		{
			if (index > 0 && index % FilesPerFolder == 0)
			{
				folderIndex++;
				currentFolderName = GenerateFolderName(folderIndex);
			}

			Console.WriteLine($"Selected file: {imageFile}");

			return Task.Run(async () =>
			{
				var uploadPath = await videoHelper.UploadFileToBlobAsync(containerName, inputFolder, imageFile, currentFolderName, true, timestamp, customFolderName);
				Console.WriteLine($"Selected file Upload Path: {uploadPath}");
			});
		}).ToList();

		await Task.WhenAll(uploadTasks);
		return true;
	}

	private static string PrintMenu()
	{
		string choice;
		do
		{
			Console.WriteLine("Novel CSAM Detection Menu");
			Console.WriteLine("###############################################");
			Console.WriteLine("#####..1.) Upload Video to Azure..........#####");
			Console.WriteLine("#####..2.) Upload Images to Azure.........#####");
			Console.WriteLine("#####..3.) Extract Frames.................#####");
			Console.WriteLine("#####..4.) Run Safety Analysis............#####");
			Console.WriteLine("#####..X.) Exit...........................#####");
			Console.WriteLine("###############################################");

			Console.WriteLine("Please enter a valid choice 1 - 4, or X to exit");
			choice = Console.ReadLine()?.ToLower(System.Globalization.CultureInfo.CurrentCulture) ?? "";
		} while (!new[] { "1", "2", "3", "4", "x" }.Contains(choice));

		return choice;
	}

	private static void ConfigureServices(IServiceCollection services)
	{
		services.AddApplicationInsightsTelemetryWorkerService(options =>
		{
			options.ConnectionString = Environment.GetEnvironmentVariable("APP_INSIGHTS_CONNECTION_STRING");
		});

		services.AddTransient<IAzureSQLHelper, AzureSQLHelper>();
		services.AddScoped<ILogHelper, LogHelper>();
		services.AddScoped<IContentSafetyHelper, ContentSafetyHelper>();
		services.AddScoped<IStorageHelper, StorageHelper>();
		services.AddTransient<IVideoHelper, VideoHelper>();
	}

	private static void SetEnvVariables()
	{
		
	}

	[STAThread]
	public static void Main(string[] args)
	{
		try
		{
			const string ContainerVideos = "videos";
			const string ContainerInput = "input";
			const string ContainerExtracted = "extracted";
			const string ContainerResults = "results";
			SetEnvVariables();

			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			var serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			var videoHelper = serviceProvider.GetService<IVideoHelper>();
			var storageHelper = serviceProvider.GetService<IStorageHelper>();

			if (videoHelper == null || storageHelper == null) return;

			string choice = PrintMenu();
			while (choice != "x")
			{
				switch (choice)
				{
					case "1":
						UploadVideo(videoHelper, ContainerVideos, ContainerInput);
						break;
					case "2":
						var uploadImagesResult = SelectAndUploadImagesAsync(videoHelper, ContainerVideos, ContainerExtracted);
						if (uploadImagesResult.Result)
						{
							Console.WriteLine("****************************************************");
							Console.WriteLine($"Image files uploaded!");
							Console.WriteLine("****************************************************");
						}
						break;
					case "3":
						ExtractFrames(videoHelper, storageHelper, ContainerVideos, ContainerInput, ContainerExtracted);
						break;
					case "4":
						RunSafetyAnalysis(videoHelper, storageHelper, ContainerVideos, ContainerExtracted, ContainerResults);
						break;
				}
				choice = PrintMenu();
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

	private static void UploadVideo(IVideoHelper videoHelper, string containerName, string inputFolder)
	{
		using var openFileDialog = new OpenFileDialog
		{
			InitialDirectory = "C:\\",
			Filter = "All Video Files|*.mp4;*.avi;*.mov;*.wmv;*.flv;*.mkv;*.webm;*.mpeg;*.mpg|MP4 Files (*.mp4)|*.mp4|AVI Files (*.avi)|*.avi|MOV Files (*.mov)|*.mov|WMV Files (*.wmv)|*.wmv|FLV Files (*.flv)|*.flv|MKV Files (*.mkv)|*.mkv|WebM Files (*.webm)|*.webm|MPEG Files (*.mpeg;*.mpg)|*.mpeg;*.mpg|All files (*.*)|*.*",
			FilterIndex = 1,
			RestoreDirectory = true
		};

		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			Console.WriteLine("No file selected.");
			return;
		}

		string selectedFilePath = openFileDialog.FileName;
		Console.WriteLine($"----------------------------------------------------------------------------\n");
		Console.WriteLine($"Selected file: {selectedFilePath}");
		var uploadResult = videoHelper.UploadFileToBlobAsync(containerName, inputFolder, selectedFilePath);
		Console.WriteLine($"Selected file Upload Path: {uploadResult.Result}");
		Console.WriteLine($"----------------------------------------------------------------------------\r\n");
	}

	private static void ExtractFrames(IVideoHelper videoHelper, IStorageHelper storageHelper, string containerName, string inputFolder, string extractedFolder)
	{
		var blobList = storageHelper?.ListBlobsInFolderWithResizeAsync(containerName, inputFolder, 3, false);
		if (blobList?.Result.Count > 0)
		{
			var menuItems = blobList.Result.Select((item, index) => new { Key = index + 1, Value = item.Key }).ToDictionary(x => x.Key, x => x.Value);
			int chosenDirKey;
			do
			{
				Console.WriteLine($"----------------------------------------------------------------------");
				foreach (var item in menuItems)
				{
					Console.WriteLine($"({item.Key}): {item.Value}");
				}
				Console.WriteLine($"----------------------------------------------------------------------");
				Console.WriteLine("Choose which file to extract frames from...e.g. 1");
				var userInput = Console.ReadLine();
				bool isInteger = int.TryParse(userInput, out int result);
				chosenDirKey = isInteger ? result : -1;
			} while (!menuItems.ContainsKey(chosenDirKey));
			string chosenDirValue = menuItems[chosenDirKey];

			if (!string.IsNullOrEmpty(chosenDirValue))
			{
				var fileName = Path.GetFileName(chosenDirValue);
				var folderPath = Path.GetDirectoryName(chosenDirValue).Replace("\\", "/");
				var extractedFramesDone = videoHelper.UploadExtractedFramesToBlobAsync(1, fileName, containerName, folderPath, extractedFolder, fileName);

				if (extractedFramesDone.Result)
				{
					Console.WriteLine("************************************************************");
					Console.WriteLine($"{chosenDirValue} is done extracting!");
					Console.WriteLine("************************************************************");
				}
			}
		}
	}

	private static void RunSafetyAnalysis(IVideoHelper videoHelper, IStorageHelper storageHelper, string containerName, string extractedFolder, string resultsFolder)
	{
		var dirList = storageHelper?.ListDirectoriesInFolderAsync(containerName, extractedFolder, 2);
		if (dirList?.Result.Count > 0)
		{
			int chosenDirKey;
			do
			{
				Console.WriteLine($"----------------------------------------------------------------------");
				foreach (var dir in dirList.Result)
				{
					Console.WriteLine($"({dir.Key}): {dir.Value}");
				}
				Console.WriteLine($"----------------------------------------------------------------------");
				Console.WriteLine("Choose which directory...e.g. 1");
				var userInput = Console.ReadLine();
				bool isInteger = int.TryParse(userInput, out int result);
				chosenDirKey = isInteger ? result : -1;
			} while (!dirList.Result.ContainsKey(chosenDirKey));
			string chosenDirValue = dirList.Result[chosenDirKey];

			if (!string.IsNullOrEmpty(chosenDirValue))
			{
				var runId = videoHelper.UploadFrameResultsAsync(containerName, chosenDirValue, resultsFolder, true);
				if (!string.IsNullOrEmpty(runId.Result))
				{
					Console.WriteLine("****************************************************");
					Console.WriteLine($"{chosenDirValue} is done running! RunId: {runId.Result}");
					Console.WriteLine("****************************************************");
				}
			}
		}
		else
		{
			Console.WriteLine("There are no directories containing images for processing. \r\n" +
				"Try extracting some frames or uploading some images.");
		}
	}
}
