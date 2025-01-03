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

			return await ExecuteNonQueryAsync(query, item);
		}

		public async Task<IFrameResult?> InsertBase64(IFrameResult item)
		{
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

			return await ExecuteNonQueryAsync(query, item);
		}

		#endregion

		#region Private Methods

		private async Task<IFrameResult?> ExecuteNonQueryAsync(string query, IFrameResult item)
		{
			try
			{
				using (SqlConnection connection = new(_connectionString))
				{
					await connection.OpenAsync();

					using SqlCommand command = new(query, connection);
					command.Parameters.AddWithValue("@Id", item.Id);
					command.Parameters.AddWithValue("@Frame", item.Frame);
					command.Parameters.AddWithValue("@RunId", item.RunId);
					command.Parameters.AddWithValue("@RunDateTime", item.RunDateTime);

					if (query.Contains("FrameResults"))
					{
						command.Parameters.AddWithValue("@Summary", item.Summary);
						command.Parameters.AddWithValue("@ChildYesNo", item.ChildYesNo);
						command.Parameters.AddWithValue("@MD5Hash", item.MD5Hash);
						command.Parameters.AddWithValue("@Hate", item.Hate);
						command.Parameters.AddWithValue("@SelfHarm", item.SelfHarm);
						command.Parameters.AddWithValue("@Violence", item.Violence);
						command.Parameters.AddWithValue("@Sexual", item.Sexual);
					}
					else if (query.Contains("FrameBase64"))
					{
						command.Parameters.AddWithValue("@ImageBase64", item.ImageBase64);
					}

					await command.ExecuteNonQueryAsync();
				}
				return item;
			}
			catch (Exception ex)
			{
				_logHelper.LogException(ex.Message, nameof(AzureSQLHelper), nameof(ExecuteNonQueryAsync), ex);
				return null;
			}
		}

		#endregion
	}
}
