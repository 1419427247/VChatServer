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
    public override async Task<ResponseMessage> Process()
    {
        var chatWithAIResponseMessage = new ChatWithAIResponseMessage();
        ChatRequestBody chatRequestBody = new ChatRequestBody();
        ChatResponseBody? chatResponseBody = null;

        chatRequestBody.Messages = Messages!;
        chatRequestBody.Temperature = Temperature;

        VChat.logger.Info($"ChatBot request: {chatRequestBody}");
        chatResponseBody = await VChat.bot.ChatCompletionAsync(chatRequestBody);
        
        VChat.logger.Info($"ChatBot response: {chatResponseBody}");
        if (chatResponseBody == null || chatResponseBody?.Choices == null || chatResponseBody?.Choices.Count == 0)
        {
            chatWithAIResponseMessage.Text = "ChatBot is not available.";
            return chatWithAIResponseMessage;
        }
        chatWithAIResponseMessage.Text = chatResponseBody?.Choices[0]?.Message?.Content;
        return chatWithAIResponseMessage;
    }
}

[JsonType("chat_with_ai_response_message")]
class ChatWithAIResponseMessage : ResponseMessage
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}