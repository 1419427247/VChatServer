namespace VChatService.Model;
public abstract class RequestMessage
{
    public abstract Task<ResponseMessage> Process();
}