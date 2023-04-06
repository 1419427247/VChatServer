using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace VChatService.Model;
public class ResponseMessage
{
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, GetType(), VConfig.JsonSerializerOptions);
    }
}