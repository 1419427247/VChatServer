namespace VChatService.Net
{
    internal class JsonBodyAttribute : Attribute
    {
        public string Name { get; set; }
        public JsonBodyAttribute(string name)
        {
            Name = name;
        }
    }
}