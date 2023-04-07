using System.Text.Json.Serialization;
using VChatService.Net;
using VChatService.Table;

namespace VChatService.Model;
[JsonBody("get_token_request_message")]
class GetTokenRequestMessage : RequestBody
{
    [JsonPropertyName("count")]
    public long Count { get; set; } = 0L;

    public override async Task<ResponseBody> Process()
    {
        SecretKey secretKey = new SecretKey()
        {
            Token = VChat.GetRandomString(64),
            ExpireAt = VChat.GetNowSeconds() + 3600,
            Count = 100
        };
        await VChat.sqlite.InsertASync(secretKey);
        return new GetTokenResponseMessage()
        {
            Token = secretKey.Token
        };
    }
}

[JsonBody("get_token_response_message")]
class GetTokenResponseMessage : ResponseBody
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }
}