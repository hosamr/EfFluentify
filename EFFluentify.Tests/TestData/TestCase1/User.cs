using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFFluentify.Tests.TestData
{
    [Table("Users", Schema = "dbo")]
    [Index(nameof(Email), IsUnique = true, Name = "IX_User_Email")]
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public string? Email { get; set; }
        [NotMapped]
        public string TemporaryToken { get; set; }
        [NotMapped]
        public string TemporaryToken2 { get; set; }
        [StringLength(50)]
        public string FirstName { get; set; }
        [ConcurrencyCheck]
        public string RowGuid { get; set; }
        public List<Order> Orders { get; set; } = new();
        public Profile Profile { get; set; } = null!;

        public int? ManagerId { get; set; }
        [ForeignKey(nameof(ManagerId))]
        public User Manager { get; set; }

        public ICollection<User> Subordinates { get; set; }

    }
}
