using EFFluentify.Domain.Models;
using EFFluentify.Domain.Rules.Interfaces;
using EFFluentify.Domain.Rules.PropertyRules;
using EFFluentify.Tests.Helpers;
using Xunit;

namespace EFFluentify.Tests.Unit.Domain
{
    [Trait("Category", "Unit")]
    public class PropertyRulesTests
    {
        private static List<string> Apply(IPropertyFluentRule rule, AttributeEntry attr, Property? prop = null)
        {
            prop ??= ModelFactory.Prop("P", "int");
            return rule.CanApply(attr, prop)
                ? rule.GetFluentLines(attr, prop).ToList()
                : new List<string>();
        }

        // ---- Required ----
        [Fact]
        public void Required_Emits_IsRequired()
        {
            Assert.Equal(new[] { ".IsRequired()" }, Apply(new RequiredToIsRequiredRule(), ModelFactory.Attr("Required")));
        }

        [Fact]
        public void Required_DoesNotApply_ToOtherAttributes()
        {
            Assert.False(new RequiredToIsRequiredRule().CanApply(ModelFactory.Attr("MaxLength"), ModelFactory.Prop("P")));
        }

        // ---- MaxLength / StringLength ----
        [Theory]
        [InlineData("MaxLength")]
        [InlineData("StringLength")]
        public void MaxLength_Positional_Emits_HasMaxLength(string attrName)
        {
            Assert.Equal(new[] { ".HasMaxLength(50)" }, Apply(new MaxLengthToHasMaxLengthRule(), ModelFactory.Attr(attrName, "50")));
        }

        [Fact]
        public void MaxLength_NamedMaximumLength_Emits_HasMaxLength()
        {
            var attr = ModelFactory.Attr("StringLength").Named("MaximumLength", "100");
            Assert.Equal(new[] { ".HasMaxLength(100)" }, Apply(new MaxLengthToHasMaxLengthRule(), attr));
        }

        [Fact]
        public void MaxLength_NoArgs_EmitsNothing()
        {
            Assert.Empty(Apply(new MaxLengthToHasMaxLengthRule(), ModelFactory.Attr("MaxLength")));
        }

        // ---- Column ----
        [Fact]
        public void Column_Name_Emits_HasColumnName()
        {
            Assert.Equal(new[] { ".HasColumnName(\"user_name\")" },
                Apply(new ColumnToHasColumnRule(), ModelFactory.Attr("Column", "\"user_name\"")));
        }

        [Fact]
        public void Column_NameAndTypeName_EmitsBoth()
        {
            var attr = ModelFactory.Attr("Column", "\"c\"").Named("TypeName", "\"varchar(200)\"");
            Assert.Equal(new[] { ".HasColumnName(\"c\")", ".HasColumnType(\"varchar(200)\")" },
                Apply(new ColumnToHasColumnRule(), attr));
        }

        [Fact]
        public void Column_TypeNameOnly_EmitsType()
        {
            var attr = ModelFactory.Attr("Column").Named("TypeName", "\"varchar(10)\"");
            Assert.Equal(new[] { ".HasColumnType(\"varchar(10)\")" }, Apply(new ColumnToHasColumnRule(), attr));
        }

        // ---- DatabaseGenerated ----
        [Theory]
        [InlineData("DatabaseGeneratedOption.Identity", ".ValueGeneratedOnAdd()")]
        [InlineData("DatabaseGeneratedOption.Computed", ".ValueGeneratedOnAddOrUpdate()")]
        [InlineData("DatabaseGeneratedOption.None", ".ValueGeneratedNever()")]
        public void DatabaseGenerated_MapsOptionToCall(string option, string expected)
        {
            Assert.Equal(new[] { expected }, Apply(new DatabaseGeneratedToValueGeneratedRule(), ModelFactory.Attr("DatabaseGenerated", option)));
        }

        [Theory]
        [InlineData("DatabaseGeneratedOption.Unknown")]
        [InlineData("")]
        public void DatabaseGenerated_UnknownOrEmpty_EmitsNothing(string option)
        {
            Assert.Empty(Apply(new DatabaseGeneratedToValueGeneratedRule(), ModelFactory.Attr("DatabaseGenerated", option)));
        }

        // ---- ConcurrencyCheck ----
        [Fact]
        public void ConcurrencyCheck_Emits_IsConcurrencyToken()
        {
            Assert.Equal(new[] { ".IsConcurrencyToken()" },
                Apply(new ConcurrencyCheckToIsConcurrencyTokenRule(), ModelFactory.Attr("ConcurrencyCheck")));
        }

        // ---- Timestamp ----
        [Fact]
        public void Timestamp_Emits_RowVersion_Concurrency_And_ValueGenerated()
        {
            Assert.Equal(
                new[] { ".IsRowVersion()", ".IsConcurrencyToken()", ".ValueGeneratedOnAddOrUpdate()" },
                Apply(new TimestampToRowVersionRule(), ModelFactory.Attr("Timestamp")));
        }

        // ---- Precision ----
        [Fact]
        public void Precision_PositionalPrecisionAndScale()
        {
            Assert.Equal(new[] { ".HasPrecision(18, 2)" }, Apply(new PrecisionFluentRule(), ModelFactory.Attr("Precision", "18", "2")));
        }

        [Fact]
        public void Precision_PrecisionOnly()
        {
            Assert.Equal(new[] { ".HasPrecision(10)" }, Apply(new PrecisionFluentRule(), ModelFactory.Attr("Precision", "10")));
        }

        [Fact]
        public void Precision_NamedArgs_Override()
        {
            var attr = ModelFactory.Attr("Precision").Named("precision", "9").Named("scale", "3");
            Assert.Equal(new[] { ".HasPrecision(9, 3)" }, Apply(new PrecisionFluentRule(), attr));
        }

        [Fact]
        public void Precision_NoArgs_EmitsNothing()
        {
            Assert.Empty(Apply(new PrecisionFluentRule(), ModelFactory.Attr("Precision")));
        }

        // ---- Unicode ----
        [Theory]
        [InlineData("true", ".IsUnicode(true)")]
        [InlineData("false", ".IsUnicode(false)")]
        public void Unicode_WithBool_EmitsExplicit(string arg, string expected)
        {
            Assert.Equal(new[] { expected }, Apply(new UnicodeFluentRule(), ModelFactory.Attr("Unicode", arg)));
        }

        [Fact]
        public void Unicode_NoArg_EmitsBare()
        {
            Assert.Equal(new[] { ".IsUnicode()" }, Apply(new UnicodeFluentRule(), ModelFactory.Attr("Unicode")));
        }

        // ---- DefaultValue ----
        [Theory]
        [InlineData("0", ".HasDefaultValue(0)")]
        [InlineData("\"N/A\"", ".HasDefaultValue(\"N/A\")")]
        public void DefaultValue_Emits_HasDefaultValue(string value, string expected)
        {
            Assert.Equal(new[] { expected }, Apply(new DefaultValueToHasDefaultValueRule(), ModelFactory.Attr("DefaultValue", value)));
        }

        [Fact]
        public void DefaultValue_NoArg_EmitsNothing()
        {
            Assert.Empty(Apply(new DefaultValueToHasDefaultValueRule(), ModelFactory.Attr("DefaultValue")));
        }

        // ---- Comment (property) ----
        [Fact]
        public void Comment_Emits_HasComment()
        {
            Assert.Equal(new[] { ".HasComment(\"note\")" }, Apply(new CommentToHasCommentRule(), ModelFactory.Attr("Comment", "\"note\"")));
        }

        // ---- Nullable ----
        [Fact]
        public void Nullable_OnNullableNonKey_Emits_IsRequiredFalse()
        {
            var prop = ModelFactory.Prop("Email", "string?", nullable: true);
            Assert.Equal(new[] { ".IsRequired(false)" }, Apply(new NullablePropertyFluentRule(), ModelFactory.Attr("Nullable"), prop));
        }

        [Fact]
        public void Nullable_OnIdSuffixProperty_DoesNotApply()
        {
            var prop = ModelFactory.Prop("ManagerId", "int?", nullable: true);
            Assert.False(new NullablePropertyFluentRule().CanApply(ModelFactory.Attr("Nullable"), prop));
        }

        [Fact]
        public void Nullable_OnNonNullableProperty_DoesNotApply()
        {
            var prop = ModelFactory.Prop("Name", "string", nullable: false);
            Assert.False(new NullablePropertyFluentRule().CanApply(ModelFactory.Attr("Nullable"), prop));
        }
    }
}
