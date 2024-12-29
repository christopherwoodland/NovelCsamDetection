namespace NovelCsam.Models
{
    public record ExtractParameters
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
    }
}