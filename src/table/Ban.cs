using VChatService.Orm;
namespace VChatService.Table;

[Table("ban")]
class Ban
{
    [Column("user_id")]
    public string UserId { get; set; } = "";
    [Column("created_at")]
    public long CreatedAt { get; set; } = 0L;
    [Column("ip_adress")]
    public string IPAdress { get; set; } = "";
}