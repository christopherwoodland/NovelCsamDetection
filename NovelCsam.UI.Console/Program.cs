internal class Program
{
	private const int FilesPerFolder = 5;
	private static string GenerateFolderName(int folderIndex)
	{
		return $"Folder_{folderIndex}";
	}
	private static void SelectAndUploadImages(IVideoHelper app, string CNVIDEOS, string CNINPUT)
	{
		using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		folderBrowserDialog.Description = "Select a folder containing images";
		folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;

		// Show the dialog and get the result
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
		{
			// Get the selected folder path
			string selectedFolderPath = folderBrowserDialog.SelectedPath;
			Console.WriteLine($"----------------------------------------------------------------------------\n");
			Console.WriteLine($"Selected folder: {selectedFolderPath}");

			// Get all image files in the selected folder
			var imageFiles = Directory.GetFiles(selectedFolderPath, "*.*")
									  .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
													 file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
													 file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
													 file.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
													 file.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
													 file.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase))
									  .ToList();

			if (imageFiles.Count > 0)
			{
				Console.WriteLine("Enter a custom folder name please...");
				var folderName = Console.ReadLine();
				int folderIndex = 1;
				string currentFolderName = GenerateFolderName(folderIndex);
				string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");


				for (int i = 0; i < imageFiles.Count; i++)
				{
					if (i > 0 && i % FilesPerFolder == 0)
					{
						folderIndex++;
						currentFolderName = GenerateFolderName(folderIndex);
					}

					string imageFile = imageFiles[i];
					Console.WriteLine($"Selected file: {imageFile}");
					var ret = app.UploadVideoToBlobAsync(CNVIDEOS, CNINPUT, imageFile, currentFolderName, true, timestamp, folderName);
					Console.WriteLine($"Selected file Upload Path: {ret.Result}");
				}
			}
			else
			{
				Console.WriteLine("No image files found in the selected folder.");
			}

			Console.WriteLine($"----------------------------------------------------------------------------\r\n");
		}
		else
		{
			Console.WriteLine("No folder selected.");
		}
	}

	private static string PrintMenu()
	{
		string choice = "";
		do
		{
			Console.WriteLine("Novel CSAM Detection Menu");
			Console.WriteLine("#####..1.) Upload Video to Azure..........#####");
			Console.WriteLine("#####..2.) Upload Images to Azure.........#####");
			Console.WriteLine("#####..3.) Segment Video..................#####");
			Console.WriteLine("#####..4.) Extract Frames.................#####");
			Console.WriteLine("#####..5.) Run Safety Analysis............#####");
			Console.WriteLine("#####..6.) Upload and Run Safety Analysis.#####");
			Console.WriteLine("#####..X.) Exit...........................#####");

			Console.WriteLine("Please enter a valid choice 1 - 6, or X to exit");
			choice = Console.ReadLine()?.ToLower(System.Globalization.CultureInfo.CurrentCulture) ?? "";
		} while (choice != "1" && choice != "2"
				&& choice != "3" && choice != "4" && choice != "5" && choice != "6" && choice != "x");

		return choice;
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

	private static void SetEnvVariables()
	{
			}

	[STAThread]
	public static void Main(string[] args)
	{

		try
		{
			const string CNVIDEOS = "videos";
			const string CNINPUT = "input";
			const string CNEXTRACTED = "extracted";
			const string CNRESULTS = "results";
			SetEnvVariables();

			// Ensure the application runs in STA mode
			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			var serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection);

			// Resolve and run the main application
			var serviceProvider = serviceCollection.BuildServiceProvider();
			var app = serviceProvider.GetService<IVideoHelper>();
			var sth = serviceProvider.GetService<IStorageHelper>();

			if (app != null && sth != null)
			{
				string choice = "";
				choice = PrintMenu();
				while (choice != "x")
				{
					switch (choice)
					{
						case "1": //Upload Video to Blob
							using (OpenFileDialog openFileDialog = new())
							{
								openFileDialog.InitialDirectory = "C:\\";
								openFileDialog.Filter = "All Video Files|*.mp4;*.avi;*.mov;*.wmv;*.flv;*.mkv;*.webm;*.mpeg;*.mpg|MP4 Files (*.mp4)|*.mp4|AVI Files (*.avi)|*.avi|MOV Files (*.mov)|*.mov|WMV Files (*.wmv)|*.wmv|FLV Files (*.flv)|*.flv|MKV Files (*.mkv)|*.mkv|WebM Files (*.webm)|*.webm|MPEG Files (*.mpeg;*.mpg)|*.mpeg;*.mpg|All files (*.*)|*.*";
								openFileDialog.FilterIndex = 1;
								openFileDialog.RestoreDirectory = true;
								if (openFileDialog.ShowDialog() == DialogResult.OK)
								{
									// Get the selected file path
									string selectedFilePath = openFileDialog.FileName;
									Console.WriteLine($"----------------------------------------------------------------------------\n");
									Console.WriteLine($"Selected file: {selectedFilePath}");
									var ret = app.UploadVideoToBlobAsync(CNVIDEOS, CNINPUT, selectedFilePath);
									Console.WriteLine($"Selected file Upload Path: {ret.Result}");
									Console.WriteLine($"----------------------------------------------------------------------------\r\n");
								}
								else
								{
									Console.WriteLine("No file selected.");
								}
							}
							break;
						case "2": //Upload Images to Blob
							SelectAndUploadImages(app, CNVIDEOS, CNEXTRACTED);
							break;
						case "3": break;
						case "4": break;
						case "5": //Analyse of extracted images/frames
							var dirList = sth?.ListDirectoriesInFolderAsync(CNVIDEOS, CNEXTRACTED, 2);
							if (dirList?.Result.Count > 0)
							{
								int chosenDirKey = -1;
								do
								{
									Console.WriteLine($"-----------------------------------");
									foreach (var dir in dirList.Result)
									{
										Console.WriteLine($"({dir.Key}): {dir.Value}");
									}
									Console.WriteLine($"-----------------------------------");
									Console.WriteLine("Choose which directory...e.g. 1");
									var userInput = Console.ReadLine();
									bool isInteger = int.TryParse(userInput, out int result);
									if (!isInteger)
									{
										chosenDirKey = -1;
									}
									else
									{
										chosenDirKey = result;
									}
								} while (!dirList.Result.ContainsKey(chosenDirKey));
								string? chosenDirValue = "";
								dirList.Result.TryGetValue(chosenDirKey, out chosenDirValue);
								if (!string.IsNullOrEmpty(chosenDirValue))
								{

									app.UploadFrameResultsAsync(CNVIDEOS, chosenDirValue, CNRESULTS, true);
								}
							}
							else
							{
								Console.WriteLine("There are no directories containing images for processing. \r\n" +
									"Try extracting some frames or uploading some images.");
							}
							break;
						case "6": break;
						default: break;

					}
					choice = PrintMenu();
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
}