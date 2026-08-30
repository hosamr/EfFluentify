namespace EFFluentify.Domain.Models
{
    public sealed class AttributeEntry
    {
        public string Name { get; set; } = "";
        public List<string> PositionalArgs { get; set; } = new();
        public Dictionary<string, string> NamedArgs { get; set; } = new();
    }
}
