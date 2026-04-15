namespace EFfluentify.Domain.Models
{
    public sealed class Property
    {
        public string Name { get; set; } = "";
        public string TypeName { get; set; } = "";
        public bool IsNullable { get; set; }
        public List<AttributeEntry> Attributes { get; set; } = new();
    }

}
