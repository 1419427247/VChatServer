using System.Text.Json.Serialization;
using VChatService.Net;

namespace VChatService.Model
{
    [JsonType("login_request_message")]
    class LoginRequestMessage : RequestMessage
    {
        [JsonPropertyName("username")]
        public string? Username { get; set; }
        [JsonPropertyName("password")]
        public string? Password { get; set; }

        public override ResponseMessage Process()
        {
            var response = new LoginResponseMessage();

            response.Token = "1234567890";
            System.Console.WriteLine("Login request received from " + Username + " with password " + Password);
            return response;
        }
    }

    [JsonType("login_response_message")]
    class LoginResponseMessage : ResponseMessage
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }
    }
}