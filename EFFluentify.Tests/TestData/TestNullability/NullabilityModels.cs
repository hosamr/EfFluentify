using System;

namespace EFFluentify.Tests.TestData.TestNullability
{
    // A plain POCO with no annotations at all: every scalar should get a bare Property() call.
    public class Tag
    {
        public int Id { get; set; }
        public string Label { get; set; } = null!;
    }

    // All-nullable scalars: the synthetic Nullable rule emits IsRequired(false) for every
    // nullable property EXCEPT ones whose name ends in "Id" (treated as optional foreign keys).
    public class Setting
    {
        public int Id { get; set; }
        public string? Key { get; set; }
        public int? Count { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? OwnerId { get; set; }
    }
}
