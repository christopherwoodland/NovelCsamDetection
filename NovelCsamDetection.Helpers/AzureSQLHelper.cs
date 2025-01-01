namespace NovelCsamDetection.Helpers
{
	public class AzureSQLHelper : IAzureSQLHelper
	{
		#region Private Fields
		private readonly string _connectionString;
		private readonly ILogHelper _logHelper;

		#endregion

		#region Constructor

		public AzureSQLHelper(ILogHelper logHelper)
		{
			_logHelper = logHelper;
			_connectionString = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTION_STRING") ?? "";
		}

		#endregion

		#region Public Methods

		public async Task<IFrameResult?> CreateFrameResult(IFrameResult item)
		{

			try
			{
				using (SqlConnection connection = new(_connectionString))
				{
					await connection.OpenAsync();

					string query = @"
                        INSERT INTO FrameResults (
                            Id,
                            Summary,
                            ChildYesNo,
                            MD5Hash,
                            Frame,
                            RunId,
                            Hate,
                            SelfHarm,
                            Violence,
                            Sexual,
                            RunDateTime
                        ) VALUES (
                            @Id,
                            @Summary,
                            @ChildYesNo,
                            @MD5Hash,
                            @Frame,
                            @RunId,
                            @Hate,
                            @SelfHarm,
                            @Violence,
                            @Sexual,
                            @RunDateTime
                        )";

					using SqlCommand command = new(query, connection);
					command.Parameters.AddWithValue("@Id", item.Id);
					command.Parameters.AddWithValue("@Summary", item.Summary);
					command.Parameters.AddWithValue("@ChildYesNo", item.ChildYesNo);
					command.Parameters.AddWithValue("@MD5Hash", item.MD5Hash);
					command.Parameters.AddWithValue("@Frame", item.Frame);
					command.Parameters.AddWithValue("@RunId", item.RunId);
					command.Parameters.AddWithValue("@Hate", item.Hate);
					command.Parameters.AddWithValue("@SelfHarm", item.SelfHarm);
					command.Parameters.AddWithValue("@Violence", item.Violence);
					command.Parameters.AddWithValue("@Sexual", item.Sexual);
					//command.Parameters.AddWithValue("@ImageBase64", item.ImageBase64);
					command.Parameters.AddWithValue("@RunDateTime", item.RunDateTime);

					await command.ExecuteNonQueryAsync();
				}
				return item;
			}
			catch (Exception ex)
			{
				_logHelper.LogException(ex.Message, nameof(AzureSQLHelper), nameof(CreateFrameResult), ex);
				return null;
			}
		}

		public async Task<IFrameResult?> InsertBase64(IFrameResult item)
		{

			try
			{
				using (SqlConnection connection = new(_connectionString))
				{
					await connection.OpenAsync();

					string query = @"
                        INSERT INTO FrameBase64 (
                            Id,
                            Frame,
                            ImageBase64,
							RunDateTime,
							RunId
                        ) VALUES (
                            @Id,
                            @Frame,
                            @ImageBase64,
							@RunDateTime,
							@RunId
                        )";

					using SqlCommand command = new(query, connection);
					command.Parameters.AddWithValue("@Id", item.Id);
					command.Parameters.AddWithValue("@Frame", item.Frame);
					command.Parameters.AddWithValue("@RunId", item.RunId);
					command.Parameters.AddWithValue("@ImageBase64", item.ImageBase64);
					command.Parameters.AddWithValue("@RunDateTime", item.RunDateTime);

					await command.ExecuteNonQueryAsync();
				}
				return item;
			}
			catch (Exception ex)
			{
				_logHelper.LogException(ex.Message, nameof(AzureSQLHelper), nameof(CreateFrameResult), ex);
				return null;
			}
		}

		#endregion

	}
}
