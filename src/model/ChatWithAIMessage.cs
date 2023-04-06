using System.Text.Json.Serialization;
using VChatService.Net;

namespace VChatService.Model;

[JsonType("chat_with_ai_request_message")]
class ChatWithAIMessage : RequestMessage
{
    [JsonPropertyName("message")]
    public List<Message>? Messages { get; set; }
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 1.0f;
    public override ResponseMessage Process()
    {
        var response = new ChatWithAIResponseMessage();
        ChatRequestBody chatRequestBody = new ChatRequestBody();
        ChatResponseBody? chatResponseBody = null;

        chatRequestBody.Messages = Messages!;
        chatRequestBody.Temperature = Temperature;
        
        VChat.bot.ChatCompletionAsync(chatRequestBody).ContinueWith((task) =>
        {
            chatResponseBody = task.Result;
        }).Wait();
        
        if (chatResponseBody == null)
        {
            response.Text = "ChatBot is not available.";
            return response;
        }
        response.Text = chatResponseBody?.Choices[0]?.Message?.Content;
        return response;
    }
}

[JsonType("chat_with_ai_response_message")]
class ChatWithAIResponseMessage : ResponseMessage
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}