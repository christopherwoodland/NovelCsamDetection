namespace VideoHelper.Helpers.Auth
{

    public static class AccountTokenProvider
    {
        public static async Task<string> GetAccountAccessTokenAsync(ILogger logger, ArmAccessTokenPermission permission = ArmAccessTokenPermission.Contributor, ArmAccessTokenScope scope = ArmAccessTokenScope.Account, CancellationToken ct = default)
        {
            var armAccessToken = await GetArmAccessTokenAsync(ct);
            //var accountAccessToken = await GetAccountAccessTokenAsync(logger, armAccessToken, permission, scope, ct);
            return armAccessToken;
        }

        public static async Task<string> GetArmAccessTokenAsync(CancellationToken ct = default)
        {
            var azureResourceManager = "https://management.azure.com";
            var tokenRequestContext = new TokenRequestContext(new[] { $"{azureResourceManager}/.default" });
            var tokenRequestResult = await new DefaultAzureCredential().GetTokenAsync(tokenRequestContext, ct);
            return tokenRequestResult.Token;
        }

        //public static async Task<string> GetAccountAccessTokenAsync(ILogger logger, string armAccessToken, ArmAccessTokenPermission permission = ArmAccessTokenPermission.Contributor, ArmAccessTokenScope scope = ArmAccessTokenScope.Account, CancellationToken ct = default)
        //{
        //    var accessTokenRequest = new AccessTokenRequest
        //    {
        //        PermissionType = permission,
        //        Scope = scope
        //    };

        //    try
        //    {
        //        var jsonRequestBody = System.Text.Json.JsonSerializer.Serialize(accessTokenRequest);
        //        logger.LogInformation("Getting Account access token: {0}", jsonRequestBody);
        //        var httpContent = new StringContent(jsonRequestBody, System.Text.Encoding.UTF8, "application/json");

        //        // Set request uri
        //        var requestUri = $"{AzureResourceManager}/subscriptions/{SubscriptionId}/resourcegroups/{ResourceGroup}/providers/Microsoft.VideoIndexer/accounts/{ViAccountName}/generateAccessToken?api-version={ApiVersion}";
        //        var client = new HttpClient(new HttpClientHandler());
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", armAccessToken);

        //        var result = await client.PostAsync(requestUri, httpContent, ct);
        //        result.EnsureSuccessStatusCode();
        //        var jsonResponseBody = await result.Content.ReadAsStringAsync(ct);
        //        logger.LogInformation("Got Account access token: {0},{1}", scope, permission);
        //        return System.Text.Json.JsonSerializer.Deserialize<GenerateAccessTokenResponse>(jsonResponseBody)?.AccessToken!;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, "Could not get GetAccountAccessTokenAsync");
        //        throw;
        //    }
        //}

    }
}
