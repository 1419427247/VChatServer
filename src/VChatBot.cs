using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
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

public class ChatRequestMessageBody
{
    [JsonPropertyName("role")]
    public string? Role { set; get; }
    [JsonPropertyName("content")]
    public string? Content { set; get; }
}

public class ChatRequestChoiceBody
{
    [JsonPropertyName("index")]
    public int Index { set; get; }
    [JsonPropertyName("message")]
    public ChatRequestMessageBody? Message { set; get; }
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { set; get; }
}

public class ChatRequestBody
{
    [JsonPropertyName("model")]
    public string Model { set; get; } = "gpt-3.5-turbo";
    [JsonPropertyName("temperature")]
    public double Temperature { set; get; } = 1.0;
    [JsonPropertyName("frequency_penalty")]
    public double FrequencyPenalty { set; get; } = 0.0;
    [JsonPropertyName("presence_penalty")]
    public double PresencePenalty { set; get; } = 0.0;
    [JsonPropertyName("max_tokens")]
    public int MaxTokens { set; get; } = 512;
    [JsonPropertyName("stream")]
    public bool Stream { set; get; } = true;
    [JsonPropertyName("messages")]
    public List<ChatRequestMessageBody> Messages { set; get; } = new List<ChatRequestMessageBody>();
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, VConfig.JsonSerializerOptions);
    }
}


public class ChatResponseDeltaBody
{
    [JsonPropertyName("role")]
    public string? Role { set; get; }
    [JsonPropertyName("content")]
    public string? Content { set; get; }
}

public class ChatResponseChoiceBody
{
    [JsonPropertyName("delta")]
    public ChatResponseDeltaBody? Delta { set; get; }
    [JsonPropertyName("index")]
    public int? Index { set; get; }
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { set; get; }
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
    public List<ChatResponseChoiceBody>? Choices { set; get; }
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, VConfig.JsonSerializerOptions);
    }
}
class CharResponseBodyCache
{
    public long Created { set; get; } = 0;
    public ConcurrentQueue<ChatResponseBody> Cache { set; get; } = new ConcurrentQueue<ChatResponseBody>();
}

public class VChatBot
{
    HttpClient client;
    VChatBotConfig config;
    Dictionary<string, CharResponseBodyCache> cache = new Dictionary<string, CharResponseBodyCache>();
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
        client.Timeout = TimeSpan.FromMinutes(2);
    }
    public string ChatCompletion(ChatRequestBody chatRequestBody)
    {
        string key = VChat.GetRandomString(16);
        CharResponseBodyCache charResponseBodyCache = new CharResponseBodyCache();
        cache.Add(key, charResponseBodyCache);

        Thread thread = new Thread(async () =>
        {
            try
            {
                CancellationToken cancellationToken = default;
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
                request.Content = new StringContent(chatRequestBody.ToString(), Encoding.UTF8, "application/json");
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken)
                        .ConfigureAwait(false);
                    throw new Exception($"Failed to send request: {responseContent}");
                }
                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                var reader = new StreamReader(stream);
                while (await new ValueTask<string?>(reader.ReadLineAsync()).ConfigureAwait(false) is { } line)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (line.StartsWith("data: "))
                    {
                        line = line["data: ".Length..];
                    }
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    if (line != "[DONE]")
                    {
                        var charResponseBody = JsonSerializer.Deserialize<ChatResponseBody>(line);
                        if (charResponseBody == null)
                        {
                            throw new Exception("ChatBot is not available.");
                        }
                        charResponseBodyCache.Cache.Enqueue(charResponseBody);
                    }
                }
            
                Thread.Sleep(1000 * 60 * 5);
                cache.Remove(key);
            }
            catch (Exception e)
            {
                cache.Remove(key);
                VChat.logger.Error(GetType(),e.ToString());
            }
        });
        thread.Start();
        return key;
    }

    public List<ChatResponseBody>? GetChatResponseList(string key)
    {
        if (cache.ContainsKey(key) == false)
        {
            return null;
        }
        else
        {
            List<ChatResponseBody> chatResponseBodies = new List<ChatResponseBody>();
            CharResponseBodyCache charResponseBodyCache = cache[key];
            if (charResponseBodyCache.Cache.Count > 0)
            {
                while (charResponseBodyCache.Cache.TryDequeue(out ChatResponseBody? chatResponseBody))
                {
                    chatResponseBodies.Add(chatResponseBody);
                }
            }
            return chatResponseBodies;
        }
    }
}
// var content = new StringContent(chatRequestBody.ToString(), Encoding.UTF8, "application/json");
// HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
// using (var body = await response.Content.ReadAsStreamAsync())
// using (var reader = new StreamReader(body))
// {
//     while (!reader.EndOfStream)
//     {
//         var line = await reader.ReadLineAsync();
//         if (line != "[DONE]")
//         {
//             Console.WriteLine(line);
//             await Task.Delay(10);
//         }
//     }
// }
// await foreach (var response in StartStreaming().WithCancellation(cancellationToken))
// {
//     var content = response.Choices[0].Delta?.Content;
//     if (content is not null)
//         yield return content;
// }

// IAsyncEnumerable<> StartStreaming()
// {
//     return _httpClient.StreamUsingServerSentEvents<
//         ChatCompletionRequest, ChatCompletionResponse>
//     (
//         ChatCompletionsEndpoint,
//         request,
//         _nullIgnoreSerializerOptions,
//         cancellationToken
//     );
// }
// await response.Content.CopyToAsync(Console.OpenStandardOutput());
// }