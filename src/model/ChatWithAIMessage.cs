using System.Text.Json.Serialization;
using VChatService.Net;
using VChatService.Table;

namespace VChatService.Model;

[JsonBody("chat_with_ai_request_message")]
class ChatWithAIMessage : RequestBody
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }
    [JsonPropertyName("messages")]
    public List<Message>? Messages { get; set; }
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 1.0f;
    public override async Task<ResponseBody> Process()
    {
        ChatRequestBody chatRequestBody = new ChatRequestBody();
        ChatResponseBody? chatResponseBody = null;

        if (Token == null)
        {
            return new ErrorResponseMessage("Token不能为空");
        }
        if (Messages == null || Messages.Count == 0)
        {
            return new ErrorResponseMessage("Messages不能为空");
        }

        SecretKey? secretKey = await VChat.sqlite.SelectASync<SecretKey>(new SecretKey() { Token = Token });
        if (secretKey == null)
        {
            return new ErrorResponseMessage("Token无效");
        }
        if (secretKey.ExpireAt < VChat.GetNowSeconds())
        {
            await VChat.sqlite.DeleteASync(secretKey);
            return new ErrorResponseMessage("Token已过期");
        }
        if(secretKey.Count <= 0)
        {
            await VChat.sqlite.DeleteASync(secretKey);
            return new ErrorResponseMessage("Token已用完");
        }

        chatRequestBody.Messages = Messages!;
        chatRequestBody.Temperature = Temperature;

        VChat.logger.Info($"ChatBot request: {chatRequestBody}");
        chatResponseBody = await VChat.bot.ChatCompletionAsync(chatRequestBody);

        VChat.logger.Info($"ChatBot response: {chatResponseBody}");

        var chatWithAIResponseMessage = new ChatWithAIResponseMessage();
        if (chatResponseBody == null || chatResponseBody?.Choices == null || chatResponseBody?.Choices.Count == 0)
        {
            chatWithAIResponseMessage.Text = "ChatBot is not available.";
            return chatWithAIResponseMessage;
        }
        secretKey.Count -= 1;
        await VChat.sqlite.UpdateASync(secretKey);
        chatWithAIResponseMessage.Text = chatResponseBody?.Choices[0]?.Message?.Content;
        return chatWithAIResponseMessage;
    }
}

[JsonBody("chat_with_ai_response_message")]
class ChatWithAIResponseMessage : ResponseBody
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}