namespace NovelCsam.Models
{
    public record AnalyzeParameters
    {
        [JsonProperty("FileNameParam")]
        public string FileNameParam { get; set; }

        [JsonProperty("KeyVaultParam")]
        public string KeyVaultParam { get; set; }

        [JsonProperty("SourceContainerNameParam")]
        public string SourceContainerNameParam { get; set; }

        [JsonProperty("SourceDirPathParam")]
        public string SourceDirPathParam { get; set; }

        [JsonProperty("DestDirPathExtParam")]
        public string DestDirPathExtParam { get; set; }

        [JsonProperty("DestDirPathParam")]
        public string DestDirPathParam { get; set; }

        [JsonProperty("StorageAccountNameParam")]
        public string StorageAccountNameParam { get; set; }

        [JsonProperty("DebugModeParam")]
        public string DebugModeParam { get; set; }

        [JsonProperty("SubscriptionParam")]
        public string SubscriptionParam { get; set; }

        [JsonProperty("ResourceGroupParam")]
        public string ResourceGroupParam { get; set; }

        [JsonProperty("JobNameParam")]
        public string JobNameParam { get; set; }

        [JsonProperty("WriteToJsonParam")]
        public string WriteToJsonParam { get; set; }

        [JsonProperty("WriteToCosmosParam")]
        public string WriteToCosmosParam { get; set; }

        [JsonProperty("CosmosDbNameParam")]
        public string CosmosDbNameParam { get; set; }

        [JsonProperty("CosmosDbContainerNameParam")]
        public string CosmosDbContainerNameParam { get; set; }
    }
}