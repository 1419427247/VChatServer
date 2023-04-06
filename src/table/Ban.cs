using VChatService.Orm;
namespace VChatService.Table;

[Table("Ban")]
class Ban
{
    [Column("user_id")]
    public string UserId { get; set; }
    [Column("created_at")]
    public long CreatedAt { get; set; }
    [Column("ip_adress")]
    public string IPAdress { get; set; }
}