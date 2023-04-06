using VChatService.Orm;
namespace VChatService.Table;
[Table("plugin")]
class Plugin
{
    [PrimaryKey,Column("id")]
    public string Id { get; set; }
    [Column("name")]
    public string Name { get; set; }
    [Column("description")]
    public string Description { get; set; }
    [Column("user_id")]
    public string UserId { get; set; }
    [Column("script")]
    public string Script { get; set; }
    [Column("created_at")]
    public long CreatedAt { get; set; }
    [Column("like_count")]
    public long LikeCount { get; set; }
}