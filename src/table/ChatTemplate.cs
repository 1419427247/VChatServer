using VChatService.Orm;

namespace VChatService.Table;
[Table("chat_template")]
class ChatTemplate
{
    [PrimaryKey]
    [Column("id")]
    public string Id { get; set; }
    [Column("title")]
    public string Title { get; set; }
    [Column("description")]
    public string Description { get; set; }
    [Column("chat")]
    public string Chat { get; set; }
    [Column("user_id")]
    public string UserId { get; set; }
    [Column("created_at")]
    public long CreatedAt { get; set; }
    [Column("like_count")]
    public long LikeCount { get; set; }
}