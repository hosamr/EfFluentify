using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Tests.TestData
{
    [Table("Orders", Schema = "dbo")]
    public class Order
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [Unicode(true)]
        [MinLength(10)]
        public string Description { get; set; }
        [Precision(precision: 18, scale: 2)]
        public decimal price { get; set; }
        [Url]
        public string ProductUrl { get; set; }
        [EmailAddress]
        public string email { get; set; } = string.Empty;
        [NotMapped]
        public string TemporaryToken { get; set; }
        [Column("MyColumn", TypeName = "varchar(50)")]
        public string CustomColumn { get; set; }
        [Column("CustomColumnNameOnly")]
        public string CustomColumnNameOnly { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

    }
}
