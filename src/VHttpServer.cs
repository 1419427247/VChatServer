using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VChatService.Model;
using VChatService.Net;
using VChatService.Table;

namespace VChatService;

public class VHttpServerConfig
{
    [JsonPropertyName("host")]
    public string Host { get; set; } = "http://localhost:8080/";
    [JsonPropertyName("max_request_count")]
    public int MaxRequestCount { get; set; } = 30;
    [JsonPropertyName("refresh_second")]
    public int RefreshSecond { get; set; } = 60;
}

public class VHttpServer
{
    private HttpListener listener;
    private Dictionary<string, Type> requestTypes = new();
    private Dictionary<string, int> ipRequestCount = new();
    private VHttpServerConfig config;
    public VHttpServer(VHttpServerConfig config)
    {
        this.config = config;
        listener = new HttpListener();
        listener.Prefixes.Add(config.Host);
    }
    public void Start()
    {
        Type[] types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (Type type in types)
        {
            JsonBodyAttribute? attribute = type.GetCustomAttribute<JsonBodyAttribute>();
            if (attribute != null)
            {
                requestTypes.Add(attribute.Name, type);
                VChat.logger.Debug(GetType(), "注册JsonBody: " + attribute.Name + " -> " + type.Name);
            }
        }
        listener.Start();
        listener.BeginGetContext(OnListenerCallback, listener);
        Task.Run(() =>
        {
            while (listener.IsListening)
            {
                ipRequestCount.Clear();
                Task.Delay(config.RefreshSecond * 1000).Wait();
            }
        });
    }

    public void Stop()
    {
        listener.Stop();
    }
    private async void OnListenerCallback(IAsyncResult async_result)
    {
        HttpListener httpListener = (HttpListener)async_result.AsyncState!;
        HttpListenerContext httpListenerContext = httpListener.EndGetContext(async_result);
        httpListener.BeginGetContext(OnListenerCallback, httpListener);

        RequestBody? requestMessage = null;
        ResponseBody? responseMessage = null;

        string adress = httpListenerContext.Request.RemoteEndPoint.Address.ToString();

        if (httpListenerContext.Request.ContentLength64 > 1024 * 10)
        {
            responseMessage = new ErrorResponseMessage("请求内容过长");
        }
        else
        {
            using var reader = new BinaryReader(httpListenerContext.Request.InputStream);
            try
            {
                String json = await Decompress(reader.ReadBytes((int)httpListenerContext.Request.ContentLength64));
                if (ipRequestCount.ContainsKey(adress) == false)
                {
                    ipRequestCount.Add(adress, 0);
                }
                ipRequestCount[adress]++;
                if (ipRequestCount[adress] > config.MaxRequestCount)
                {
                    responseMessage = new ErrorResponseMessage("请求过于频繁");
                }
                else
                {
                    JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
                    if (jsonElement.TryGetProperty("type", out JsonElement typeJsonElement) && jsonElement.TryGetProperty("content", out JsonElement contextJsonElement))
                    {
                        string type = typeJsonElement.GetString()!;
                        if (requestTypes.TryGetValue(type, out Type? contentType))
                        {
                            requestMessage = JsonSerializer.Deserialize(contextJsonElement.GetRawText(), contentType) as RequestBody;
                            if (requestMessage != null)
                            {
                                VChat.logger.Info($"Request from {adress}: {json}");
                                responseMessage = await requestMessage.Process();

                                if (responseMessage == null)
                                {
                                    responseMessage = new ErrorResponseMessage("响应消息为空：" + requestMessage.GetType().Name);
                                }
                            }
                            else
                            {
                                responseMessage = new ErrorResponseMessage("请求消息格式错误");
                            }
                        }
                        else
                        {
                            responseMessage = new ErrorResponseMessage("请求类型错误：" + type);
                        }
                    }
                    else
                    {
                        responseMessage = new ErrorResponseMessage("请求格式错误");
                    }
                }
            }
            catch (Exception e)
            {
                responseMessage = new ErrorResponseMessage(e.Message);
            }
        }

        string responseJson =
        "{\"type\": \"" + responseMessage!.GetType().GetCustomAttribute<JsonBodyAttribute>()?.Name + "\"," +
        "\"content\": " + responseMessage.ToString() + "}";

        VChat.logger.Info($"Response to {httpListenerContext.Request.RemoteEndPoint}: {responseJson}");
        byte[] buffer = await Compress(responseJson);

        httpListenerContext.Response.ContentLength64 = buffer.Length;
        httpListenerContext.Response.ContentType = "application/json";
        httpListenerContext.Response.Headers.Add("Content-Encoding", "gzip");
        using var writer = new BinaryWriter(httpListenerContext.Response.OutputStream);
        writer.Write(buffer);
    }

    public static async Task<byte[]> Compress(String data)
    {
        using var compressedStream = new MemoryStream();
        using var gZipStream = new GZipStream(compressedStream, CompressionMode.Compress);
        await gZipStream.WriteAsync(Encoding.UTF8.GetBytes(data));
        await gZipStream.FlushAsync();
        return compressedStream.ToArray();
    }

    public static async Task<string> Decompress(byte[] data)
    {
        using var compressedStream = new MemoryStream(data);
        using var gZipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
        using var decompressedStream = new MemoryStream();
        await gZipStream.CopyToAsync(decompressedStream);
        return Encoding.UTF8.GetString(decompressedStream.ToArray());
    }
}

[JsonBody("error_response_message")]
class ErrorResponseMessage : ResponseBody
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    public ErrorResponseMessage(string text)
    {
        Text = text;
    }
}