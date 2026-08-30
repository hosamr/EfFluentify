using EFFluentify.Domain.Models;

namespace EFFluentify.Domain.Rules.Interfaces
{
    public interface IPropertyFluentRule
    {
        bool CanApply(AttributeEntry attribute, Property property);
        IEnumerable<string> GetFluentLines(AttributeEntry attribute, Property property);
        public IEnumerable<string> GetAnnotationPropertyNames();

    }

}
