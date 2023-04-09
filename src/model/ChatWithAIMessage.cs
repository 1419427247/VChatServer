using System.Net;
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
    public List<ChatRequestMessageBody>? Messages { get; set; }
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 1.0f;
    [JsonPropertyName("frequency_penalty")]
    public double FrequencyPenalty { set; get; } = 0.0;
    [JsonPropertyName("presence_penalty")]
    public double PresencePenalty { set; get; } = 0.0;
    public override async Task<ResponseBody> Process()
    {
        // if (Token == null)
        // {
        //     return new ErrorResponseMessage("令牌不能为空");
        // }
        // if (Messages == null || Messages.Count == 0)
        // {
        //     return new ErrorResponseMessage("消息不能为空");
        // }

        // SecretKey? secretKey = await VChat.sqlite.SelectASync<SecretKey>(new SecretKey() { Token = Token });
        // if (secretKey == null)
        // {
        //     return new ErrorResponseMessage("令牌无效");
        // }
        // if (secretKey.ExpireAt < VChat.GetNowSeconds())
        // {
        //     await VChat.sqlite.DeleteASync(secretKey);
        //     return new ErrorResponseMessage("令牌已过期");
        // }
        // if (secretKey.Count <= 0)
        // {
        //     await VChat.sqlite.DeleteASync(secretKey);
        //     return new ErrorResponseMessage("令牌已用完");
        // }
        var key = VChat.bot.ChatCompletion(new ChatRequestBody()
        {
            Messages = Messages!,
            Temperature = Temperature,
            FrequencyPenalty = FrequencyPenalty,
            PresencePenalty = PresencePenalty,
        });
        var chatWithAIResponseMessage = new ChatWithAIResponseMessage()
        {
            Key = key
        };
        // secretKey.Count -= 1;
        // await VChat.sqlite.UpdateASync(secretKey);

        return chatWithAIResponseMessage;
    }
}

[JsonBody("chat_with_ai_response_message")]
class ChatWithAIResponseMessage : ResponseBody
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }
}