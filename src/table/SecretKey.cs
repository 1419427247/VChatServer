using VChatService.Orm;
namespace VChatService.Table;

[Table("secret_key")]
class SecretKey 
{
    [PrimaryKey, Column("token")]
    public string Token { get; set; } = "";
    [Column("count")]
    public long Count { get; set; } = 0L;
    [Column("expire_at")]
    public long ExpireAt { get; set; } = 0L;
}