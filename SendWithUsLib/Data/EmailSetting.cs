using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;

namespace SendWithUsLib
{
    public partial class EmailSetting
    {
        [JsonProperty("EmailConfig")]
        public EmailConfig EmailConfig { get; set; }
    }

    public partial class EmailConfig
    {
        [JsonProperty("ApiKey")]
        public string ApiKey { get; set; }

        [JsonProperty("TimeoutInMilliseconds")]
        public int TimeoutInMilliseconds { get; set; }

        [JsonProperty("RetryCount")]
        public int RetryCount { get; set; }

        [JsonProperty("RetryIntervalMilliseconds")]
        public int RetryIntervalMilliseconds { get; set; }

        [JsonProperty("MaxBatchRequest")]
        public int MaxBatchRequest { get; set; }
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal } }
        };
    }
}
