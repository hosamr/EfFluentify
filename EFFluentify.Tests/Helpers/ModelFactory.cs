using EFFluentify.Domain.Models;

namespace EFFluentify.Tests.Helpers
{
    internal static class ModelFactory
    {
        public static AttributeEntry Attr(string name, params string[] positional)
        {
            var a = new AttributeEntry { Name = name };
            a.PositionalArgs.AddRange(positional);
            return a;
        }

        public static AttributeEntry Named(this AttributeEntry a, string key, string value)
        {
            a.NamedArgs[key] = value;
            return a;
        }

        public static Property Prop(string name, string type = "int", bool? nullable = null, params AttributeEntry[] attrs)
        {
            var p = new Property
            {
                Name = name,
                TypeName = type,
                IsNullable = nullable ?? type.TrimEnd().EndsWith("?")
            };
            p.Attributes.AddRange(attrs);
            return p;
        }

        public static EntityModel Entity(string name, Property[]? props = null, AttributeEntry[]? attrs = null, string ns = "Test.Models")
        {
            var e = new EntityModel { Name = name, Namespace = ns };
            if (props != null) e.Properties.AddRange(props);
            if (attrs != null) e.Attributes.AddRange(attrs);
            return e;
        }
    }
}
