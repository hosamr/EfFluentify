using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Tests.TestData
{
    [Table("Profiles", Schema = "dbo")]
    [Keyless]
    [Comment("Main Profile table")]
    [Index(nameof(Email))]

    public class Profile
    {
        public string Name { get; set; }
        public string Email { get; set; }
        [NotMapped]
        public List<string> Tags { get; set; } = new();
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}
