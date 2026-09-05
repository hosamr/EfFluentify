using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EFFluentify.Tests.TestData.TestExplicitRelationships
{
    public class Category
    {
        public int Id { get; set; }
        public ICollection<Item> Items { get; set; } = null!;
    }

    // The dependent's FK navigation carries an explicit [InverseProperty] pointing back to the
    // principal's collection, and a [DeleteBehavior] supplied via a NAMED argument.
    public class Item
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        [InverseProperty("Items")]
        [DeleteBehavior(behavior: DeleteBehavior.Cascade)]
        public Category Category { get; set; } = null!;
    }
}
