using VChatService.Orm;
namespace VChatService.Table;
[Table("user")]
class User
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; } = "";
    [Column("name")]
    public string Name { get; set; } = "";
    [Column("password")]
    public string Password { get; set; } = "";
    [Column("ip_adress")]
    public string IPAdress { get; set; } = "";
    [Column("created_at")]
    public long CreatedAt { get; set; } = 0L;
    [Column("identity")]
    public string Identity { get; set; } = "";
    [Column("token")]
    public string Token { get; set; } = "";
}