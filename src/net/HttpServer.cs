using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VChatService.Model;

namespace VChatService.Net;
public class HttpServer
{
    private HttpListener listener;
    private Dictionary<string, Type> RequestTypes = new();
    private Dictionary<string, int> IPMinuteCount = new();
    public HttpServer(string prefix)
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        listener = new HttpListener();
        listener.Prefixes.Add(prefix);
        VChat.logger.Info("Already added prefix: " + prefix);
    }

    public void Start()
    {
        Type[] types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (Type type in types)
        {
            JsonType? attribute = type.GetCustomAttribute<JsonType>();
            if (attribute != null)
            {
                RequestTypes.Add(attribute.Name, type);
            }
        }
        listener.Start();
        listener.BeginGetContext(OnListenerCallback, listener);
        VChat.logger.Info("HttpServer started.");

        Task.Run(() =>
        {
            while (listener.IsListening)
            {
                IPMinuteCount.Clear();
                Task.Delay(60000).Wait();
                System.Console.WriteLine("Clear");
            }
        });
    }

    public void Stop()
    {
        listener.Stop();
    }
    private void OnListenerCallback(IAsyncResult async_result)
    {
        HttpListener httpListener = (HttpListener)async_result.AsyncState!;
        HttpListenerContext httpListenerContext = httpListener.EndGetContext(async_result);
        httpListener.BeginGetContext(OnListenerCallback, httpListener);

        using var reader = new StreamReader(httpListenerContext.Request.InputStream);
        using var writer = new StreamWriter(httpListenerContext.Response.OutputStream);

        string data = reader.ReadToEnd();
        VChat.logger.Info($"Received data from {httpListenerContext.Request.RemoteEndPoint}: {data}");
        RequestMessage? requestMessage = null;
        ResponseMessage? responseMessage = null;
        try
        {
            if (IPMinuteCount.ContainsKey(httpListenerContext.Request.RemoteEndPoint.Address.ToString()) == false)
            {
                IPMinuteCount.Add(httpListenerContext.Request.RemoteEndPoint.Address.ToString(), 0);
            }
            IPMinuteCount[httpListenerContext.Request.RemoteEndPoint.Address.ToString()]++;
            if (IPMinuteCount[httpListenerContext.Request.RemoteEndPoint.Address.ToString()] > VChat.config.MaxRequestPerMinute)
            {
                responseMessage = new ErrorResponseMessage()
                {
                    Text = "一分钟内请求次最多为" + VChat.config.MaxRequestPerMinute + "次，请稍后再试"
                };
            }
            else
            {
                JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(data);
                if (jsonElement.TryGetProperty("type", out JsonElement typeJsonElement) && jsonElement.TryGetProperty("content", out JsonElement contextJsonElement))
                {
                    string type = typeJsonElement.GetString()!;
                    if (RequestTypes.TryGetValue(type, out Type? contentType))
                    {
                        requestMessage = JsonSerializer.Deserialize(contextJsonElement.GetRawText(), contentType) as RequestMessage;
                    }
                }
                if (requestMessage != null)
                {
                    responseMessage = requestMessage.Process();
                    if (responseMessage == null)
                    {
                        responseMessage = new ErrorResponseMessage()
                        {
                            Text = "响应消息为空. 请求消息为: " + requestMessage.GetType().Name
                        };
                    }
                }
                else
                {
                    responseMessage = new ErrorResponseMessage()
                    {
                        Text = "请求消息格式错误"
                    };
                }
            }
        }
        catch (Exception e)
        {
            responseMessage = new ErrorResponseMessage()
            {
                Text = e.Message
            };
        }
        string responseJson =
        "{\"type\": \"" + responseMessage!.GetType().GetCustomAttribute<JsonType>()?.Name + "\"," +
        "\"content\": " + responseMessage.ToString() + "}";
        VChat.logger.Info($"Response to {httpListenerContext.Request.RemoteEndPoint}: {responseJson}");
        byte[] buffer = Encoding.UTF8.GetBytes(responseJson);
        httpListenerContext.Response.ContentLength64 = buffer.Length;
        httpListenerContext.Response.ContentType = "application/json";
        using (Stream output = httpListenerContext.Response.OutputStream)
        {
            output.Write(buffer, 0, buffer.Length);
        }
    }
}

[JsonType("error_response_message")]
class ErrorResponseMessage : ResponseMessage
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}