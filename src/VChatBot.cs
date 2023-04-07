using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace VChatService;

public class VChatBotConfig
{
    [JsonPropertyName("openai_key")]
    public string OpenAIKey { get; set; } = "";
    [JsonPropertyName("proxy")]
    public string Proxy { get; set; } = "";
}

public class Message
{
    [JsonPropertyName("role")]
    public string? Role { set; get; }
    [JsonPropertyName("content")]
    public string? Content { set; get; }
}

public class Choice
{
    [JsonPropertyName("index")]
    public int Index { set; get; }
    [JsonPropertyName("message")]
    public Message? Message { set; get; }
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { set; get; }
}

public class ChatRequestBody
{
    [JsonPropertyName("model")]
    public string Model { set; get; } = "gpt-3.5-turbo";
    [JsonPropertyName("temperature")]
    public double Temperature { set; get; } = 1.0;
    [JsonPropertyName("max_tokens")]
    public int MaxTokens { set; get; } = 512;
    [JsonPropertyName("messages")]
    public List<Message> Messages { set; get; } = new List<Message>();
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, VConfig.JsonSerializerOptions);
    }
}

public class ChatResponseBody
{
    [JsonPropertyName("id")]
    public string? Id { set; get; }
    [JsonPropertyName("object")]
    public string? Object { set; get; }
    [JsonPropertyName("created")]
    public int? Created { set; get; }
    [JsonPropertyName("model")]
    public string? Model { set; get; }
    [JsonPropertyName("choices")]
    public List<Choice>? Choices { set; get; }
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, VConfig.JsonSerializerOptions);
    }
}

public class VChatBot
{
    HttpClient client;
    VChatBotConfig config;
    public VChatBot(VChatBotConfig config)
    {
        this.config = config;
        if (config.Proxy != String.Empty)
        {
            WebRequest.DefaultWebProxy = new WebProxy(config.Proxy);
            VChat.logger.Info(GetType(), "Using proxy: " + config.Proxy);
        }
        client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + config.OpenAIKey);
    }
    public async Task<ChatResponseBody> ChatCompletionAsync(ChatRequestBody chatRequestBody)
    {
        var content = new StringContent(chatRequestBody.ToString(), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
        var json = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(await response.Content.ReadAsStringAsync()));
        VChat.logger.Info(GetType(), $"response: {json}");
        ChatResponseBody? charResponseBody = JsonSerializer.Deserialize<ChatResponseBody>(json);
        if (charResponseBody == null)
        {
            throw new Exception("ChatBot is not available.");
        }
        VChat.logger.Info(GetType(), $"response: {charResponseBody}");
        return charResponseBody;
    }
}