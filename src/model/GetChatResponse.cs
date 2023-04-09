using System.Text.Json.Serialization;
using VChatService.Net;

namespace VChatService.Model;

[JsonBody("get_chat_request_message")]
class GetChatRequestMessage : RequestBody
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }
    public override async Task<ResponseBody> Process()
    {
        if (Key == null)
        {
            return new ErrorResponseMessage("Key不能为空");
        }
        List<ChatResponseBody>? chatResponseBody = VChat.bot.GetChatResponseList(Key);
        if (chatResponseBody == null)
        {
            return new ErrorResponseMessage("Key无效");
        }
        GetChatResponseMessage getChatResponseMessage = new GetChatResponseMessage()
        {
            Key = Key,
            Messages = chatResponseBody
        };
        return getChatResponseMessage;
    }
}

[JsonBody("get_chat_response_message")]
class GetChatResponseMessage : ResponseBody
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }
    [JsonPropertyName("messages")]
    public List<ChatResponseBody>? Messages { get; set; }
}