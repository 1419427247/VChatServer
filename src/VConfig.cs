using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace VChatService;

class VConfig
{
    public static JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        PropertyNameCaseInsensitive = true,
    };
    [JsonPropertyName("host")]
    public string Host { get; set; } = "http://*:8080/";
    [JsonPropertyName("openai_key")]
    public string OpenAIKey { get; set; } = "";
    [JsonPropertyName("proxy")]
    public string Proxy { get; set; } = "";
    [JsonPropertyName("sqlite")]
    public string Sqlite { get; set; } = "Data Source=vchat.db;Version=3;";
    [JsonPropertyName("log_file_path")]
    public string LogFilePath { get; set; } = "log.txt";
    [JsonPropertyName("minimum_log_level")]
    public string MinimumLogLevel { get; set; } = "Debug";
    [JsonPropertyName("log_output_mode")]
    public string LogOutputMode { get; set; } = "Both";

    [JsonPropertyName("max_request_per_minute")]
    public int MaxRequestPerMinute { get; set; } = 5;

    public static VConfig LoadConfig()
    {
        VConfig? result = null;
        if (!File.Exists("config.json"))
        {
            result = new VConfig();
            File.WriteAllText("config.json", JsonSerializer.Serialize(result));
            return result;
        }
        else
        {
            result = JsonSerializer.Deserialize<VConfig>(File.ReadAllText("config.json"));
            if (result == null)
            {
                throw new Exception("Failed to load config.json");
            }
            return result;
        }
    }
}