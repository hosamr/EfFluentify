using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EFFluentify.Tests.TestData.TestScalarAnnotations
{
    // Exercises column-type mapping, property-level comments, default values,
    // and the full set of DatabaseGenerated options (Identity/Computed/None).
    [Table("Invoices")]
    public class Invoice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(TypeName = "char(10)")]
        [Comment("Human-readable invoice number")]
        public string Number { get; set; } = null!;

        [Precision(12, 2)]
        [DefaultValue(0)]
        public decimal Amount { get; set; }

        [DefaultValue(false)]
        public bool IsPaid { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedUtc { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid PublicId { get; set; }

        public string? Notes { get; set; }
    }

    // Standalone composite primary key (no relationships) plus a composite unique index.
    [Index(nameof(Currency), nameof(Year), IsUnique = true, Name = "IX_ExchangeRate_Currency_Year")]
    public class ExchangeRate
    {
        [Key]
        public string Currency { get; set; } = null!;

        [Key]
        public int Year { get; set; }

        [Precision(18, 6)]
        public decimal Rate { get; set; }
    }
}
