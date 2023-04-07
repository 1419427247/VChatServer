using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace VChatService;

class VConfigAttribute : Attribute
{
    public string Name { get; set; }
    public VConfigAttribute(string name)
    {
        Name = name;
    }
}

class VConfig
{
    public static JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        PropertyNameCaseInsensitive = true,
    };
    [JsonPropertyName("sqlite")]
    public VSqliteConfig Sqlite { get; set; } = new VSqliteConfig();
    [JsonPropertyName("http_server")]
    public VHttpServerConfig HttpServer { get; set; } = new VHttpServerConfig();
    [JsonPropertyName("logger")]
    public VLoggerConfig Logger { get; set; } = new VLoggerConfig();
    [JsonPropertyName("chat_bot")]
    public VChatBotConfig ChatBot { get; set; } = new VChatBotConfig();

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