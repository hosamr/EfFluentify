using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace EFFluentify.Tests.TestData.TestIndexesAndColumns
{
    public enum Priority
    {
        Low,
        Medium,
        High
    }

    // Two index attributes on one entity (one unique+named, one composite non-unique),
    // explicit unicode on/off, and an enum-typed scalar property.
    [Index(nameof(Email), IsUnique = true, Name = "IX_Contact_Email")]
    [Index(nameof(LastName), nameof(FirstName))]
    public class Contact
    {
        public int Id { get; set; }

        [StringLength(320)]
        public string Email { get; set; } = null!;

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        [Unicode(false)]
        public string CountryCode { get; set; } = null!;

        [Unicode]
        public string Bio { get; set; } = null!;

        public Priority Priority { get; set; }
    }
}
