using EFfluentify.Domain.Models;

namespace EFfluentify.Domain.Rules.Interfaces
{
    public interface IPropertyFluentRule
    {
        bool CanApply(AttributeEntry attribute, Property property);
        IEnumerable<string> GetFluentLines(AttributeEntry attribute, Property property);
        public IEnumerable<string> GetAnnotationPropertyNames();

    }

}
