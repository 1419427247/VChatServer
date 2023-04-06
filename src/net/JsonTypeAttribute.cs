namespace VChatService.Net
{
    internal class JsonType : Attribute
    {
        public string Name { get; set; }
        public JsonType(string name)
        {
            Name = name;
        }
    }
}