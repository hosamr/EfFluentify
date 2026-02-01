namespace EFfluentify.Domain.Models
{
    public sealed class PropertyModel
    {
        public string Name { get; set; } = "";
        public string TypeName { get; set; } = "";
        public bool IsNullable { get; set; }
        public List<AttributeModel> Attributes { get; set; } = new();
    }

}
