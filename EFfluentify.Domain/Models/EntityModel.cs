namespace EFfluentify.Domain.Models
{
    public sealed class EntityModel
    {
        public string Namespace { get; set; } = "";
        public string Name { get; set; } = "";
        public List<AttributeEntry> Attributes { get; set; } = new();
        public List<Property> Properties { get; set; } = new();
        public string SourcePath { get; set; } = "";
    }

}
