namespace VChatService.Orm;
class ColumnAttribute : Attribute
{
    public string Name { get; set; }
    public ColumnAttribute(string name)
    {
        Name = name;
    }
}