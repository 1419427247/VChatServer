namespace VChatService.Model;
public abstract class RequestBody
{
    public abstract Task<ResponseBody> Process();
}