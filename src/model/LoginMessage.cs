using System.Text.Json.Serialization;
using VChatService.Net;

namespace VChatService.Model
{
    [JsonBody("login_request_message")]
    class LoginRequestMessage : RequestBody
    {
        [JsonPropertyName("username")]
        public string? Username { get; set; }
        [JsonPropertyName("password")]
        public string? Password { get; set; }

        public override async Task<ResponseBody> Process()
        {
            await new Task(() => { });
            var response = new LoginResponseMessage();
            response.Token = "1234567890";
            return response;
        }
    }

    [JsonBody("login_response_message")]
    class LoginResponseMessage : ResponseBody
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }
    }
}