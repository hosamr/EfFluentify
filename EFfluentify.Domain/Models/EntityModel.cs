namespace EFfluentify.Domain.Models
{
    public sealed class EntityModel
    {
        public string Namespace { get; set; } = "";
        public string Name { get; set; } = "";
        public List<AttributeModel> Attributes { get; set; } = new();
        public List<PropertyModel> Properties { get; set; } = new();
        public string SourcePath { get; set; } = "";
    }

}
