using EFFluentify.Domain.Models;
using EFFluentify.Domain.Rules.EntityRules;
using EFFluentify.Domain.Rules.Helpers;
using EFFluentify.Tests.Helpers;
using Xunit;
using static EFFluentify.Tests.Helpers.ModelFactory;

namespace EFFluentify.Tests.Unit.Domain
{
    [Trait("Category", "Unit")]
    public class ForeignKeyFluentRuleTests
    {
        private static List<string> Lines(EntityModel dependent, params EntityModel[] allEntities)
        {
            var rule = new ForeignKeyFluentRule(new ModelContext(allEntities));
            return rule.CanApply(dependent) ? rule.GetFluentLines(dependent).ToList() : new List<string>();
        }

        [Fact]
        public void Basic_ForeignKeyOnNavigation_ManyToOne()
        {
            var blog = Entity("Blog", new[] { Prop("Id"), Prop("Posts", "ICollection<Post>") });
            var post = Entity("Post", new[]
            {
                Prop("Id"),
                Prop("BlogId", "int"),
                Prop("Blog", "Blog", attrs: new[] { Attr("ForeignKey", "\"BlogId\"") })
            });

            Assert.Equal(
                new[] { "builder.HasOne(x => x.Blog).WithMany(x => x.Posts).HasForeignKey(x => x.BlogId).IsRequired();" },
                Lines(post, blog, post));
        }

        [Fact]
        public void ForeignKeyOnScalarProperty_PointsToNavigation()
        {
            var author = Entity("Author", new[] { Prop("Id"), Prop("Books", "ICollection<Book>") });
            var book = Entity("Book", new[]
            {
                Prop("Id"),
                Prop("AuthorId", "int", attrs: new[] { Attr("ForeignKey", "\"AuthorNav\"") }),
                Prop("AuthorNav", "Author")
            });

            Assert.Equal(
                new[] { "builder.HasOne(x => x.AuthorNav).WithMany(x => x.Books).HasForeignKey(x => x.AuthorId).IsRequired();" },
                Lines(book, author, book));
        }

        [Fact]
        public void CompositeForeignKey_EmitsAnonymousObject()
        {
            var team = Entity("Team", new[]
            {
                Prop("TeamId", "int", attrs: new[] { Attr("Key") }),
                Prop("LeagueId", "int", attrs: new[] { Attr("Key") }),
                Prop("Players", "ICollection<Player>")
            });
            var player = Entity("Player", new[]
            {
                Prop("Id"),
                Prop("MyTeamId", "int"),
                Prop("MyLeagueId", "int"),
                Prop("Team", "Team", attrs: new[] { Attr("ForeignKey", "\"MyTeamId, MyLeagueId\"") })
            });

            Assert.Equal(
                new[] { "builder.HasOne(x => x.Team).WithMany(x => x.Players).HasForeignKey(x => new { x.MyTeamId, x.MyLeagueId }).IsRequired();" },
                Lines(player, team, player));
        }

        [Fact]
        public void SelfReferencing_OptionalForeignKey_OmitsIsRequired()
        {
            var employee = Entity("Employee", new[]
            {
                Prop("Id"),
                Prop("ManagerId", "int?", nullable: true),
                Prop("Manager", "Employee?", attrs: new[] { Attr("ForeignKey", "\"ManagerId\"") }),
                Prop("Subordinates", "ICollection<Employee>")
            });

            Assert.Equal(
                new[] { "builder.HasOne(x => x.Manager).WithMany(x => x.Subordinates).HasForeignKey(x => x.ManagerId);" },
                Lines(employee, employee));
        }

        [Fact]
        public void OneToOne_ReferenceInverse_UsesGenericHasForeignKey()
        {
            var car = Entity("Car", new[] { Prop("Id"), Prop("Engine", "Engine") });
            var engine = Entity("Engine", new[]
            {
                Prop("Id"),
                Prop("CarId", "int"),
                Prop("Car", "Car", attrs: new[] { Attr("ForeignKey", "\"CarId\"") })
            });

            Assert.Equal(
                new[] { "builder.HasOne(x => x.Car).WithOne(x => x.Engine).HasForeignKey<Engine>(x => x.CarId).IsRequired();" },
                Lines(engine, car, engine));
        }

        [Fact]
        public void OneToOne_SharedPrimaryKey_UsesDependentTypeInHasForeignKey()
        {
            var user = Entity("User", new[] { Prop("Id"), Prop("Profile", "UserProfile") });
            var profile = Entity("UserProfile", new[]
            {
                Prop("Id", "int", attrs: new[] { Attr("Key"), Attr("ForeignKey", "\"User\"") }),
                Prop("User", "User")
            });

            Assert.Equal(
                new[] { "builder.HasOne(x => x.User).WithOne(x => x.Profile).HasForeignKey<UserProfile>(x => x.Id).IsRequired();" },
                Lines(profile, user, profile));
        }

        [Fact]
        public void InverseProperty_And_DeleteBehavior_AreHonored()
        {
            var parent = Entity("Parent", new[]
            {
                Prop("Id"),
                Prop("ChildrenByMother", "ICollection<Child>", attrs: new[] { Attr("InverseProperty", "\"Mother\"") }),
                Prop("ChildrenByFather", "ICollection<Child>", attrs: new[] { Attr("InverseProperty", "\"Father\"") })
            });
            var child = Entity("Child", new[]
            {
                Prop("Id"),
                Prop("MotherId", "int"),
                Prop("Mother", "Parent", attrs: new[] { Attr("ForeignKey", "\"MotherId\""), Attr("DeleteBehavior", "DeleteBehavior.NoAction") }),
                Prop("FatherId", "int"),
                Prop("Father", "Parent", attrs: new[] { Attr("ForeignKey", "\"FatherId\""), Attr("DeleteBehavior", "DeleteBehavior.NoAction") })
            });

            Assert.Equal(new[]
            {
                "builder.HasOne(x => x.Mother).WithMany(x => x.ChildrenByMother).HasForeignKey(x => x.MotherId).IsRequired().OnDelete(DeleteBehavior.NoAction);",
                "builder.HasOne(x => x.Father).WithMany(x => x.ChildrenByFather).HasForeignKey(x => x.FatherId).IsRequired().OnDelete(DeleteBehavior.NoAction);"
            }, Lines(child, parent, child));
        }

        [Fact]
        public void NoInverseNavigationOnPrincipal_FallsBackToParameterlessWithMany()
        {
            var teacher = Entity("Teacher", new[] { Prop("Id") });
            var department = Entity("Department", new[] { Prop("Id") });
            var course = Entity("Course", new[]
            {
                Prop("Id"),
                Prop("TeacherId", "int?", nullable: true),
                Prop("Teacher", "Teacher?", attrs: new[] { Attr("ForeignKey", "\"TeacherId\"") }),
                Prop("DepartmentId", "int"),
                Prop("Department", "Department", attrs: new[] { Attr("ForeignKey", "\"DepartmentId\"") })
            });

            Assert.Equal(new[]
            {
                "builder.HasOne(x => x.Teacher).WithMany().HasForeignKey(x => x.TeacherId);",
                "builder.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).IsRequired();"
            }, Lines(course, teacher, department, course));
        }

        [Fact]
        public void PrincipalNotInContext_FallsBackToDefaultWithMany()
        {
            var post = Entity("Post", new[]
            {
                Prop("Id"),
                Prop("BlogId", "int"),
                Prop("Blog", "Blog", attrs: new[] { Attr("ForeignKey", "\"BlogId\"") })
            });

            // Only Post is in the context; the principal "Blog" is unknown.
            Assert.Equal(
                new[] { "builder.HasOne(x => x.Blog).WithMany().HasForeignKey(x => x.BlogId).IsRequired();" },
                Lines(post, post));
        }

        [Fact]
        public void CanApply_False_WhenNoForeignKeyAttributes()
        {
            var blog = Entity("Blog", new[] { Prop("Id"), Prop("Posts", "ICollection<Post>") });
            Assert.False(new ForeignKeyFluentRule(new ModelContext(new[] { blog })).CanApply(blog));
        }

        [Fact]
        public void EmptyOrMissingForeignKeyArgument_ProducesNoLine()
        {
            var entity = Entity("Item", new[]
            {
                Prop("MissingArg", "Category", attrs: new[] { Attr("ForeignKey") }),        // no argument at all
                Prop("BlankNav", "Category", attrs: new[] { Attr("ForeignKey", "\"\"") }),   // navigation, blank name
                Prop("BlankScalarId", "int", attrs: new[] { Attr("ForeignKey", "\"\"") })    // scalar, blank name
            });

            Assert.Empty(Lines(entity, entity, Entity("Category")));
        }

        [Fact]
        public void ExplicitInverse_NotFoundOnPrincipal_FallsBackToWithMany()
        {
            var category = Entity("Category", new[] { Prop("Id"), Prop("Items", "ICollection<Item>") });
            var item = Entity("Item", new[]
            {
                Prop("Id"),
                Prop("CategoryId", "int"),
                Prop("Category", "Category", attrs: new[]
                {
                    Attr("ForeignKey", "\"CategoryId\""),
                    Attr("InverseProperty", "\"Ghost\"")
                })
            });

            Assert.Equal(
                new[] { "builder.HasOne(x => x.Category).WithMany(x => x.Ghost).HasForeignKey(x => x.CategoryId).IsRequired();" },
                Lines(item, category, item));
        }

        [Fact]
        public void ExplicitInverse_NotFoundOnPrincipal_WithSharedPrimaryKey_FallsBackToWithOne()
        {
            var user = Entity("User", new[] { Prop("Id"), Prop("Profile", "UserProfile") });
            var profile = Entity("UserProfile", new[]
            {
                Prop("Id", "int", attrs: new[] { Attr("Key"), Attr("ForeignKey", "\"User\"") }),
                Prop("User", "User", attrs: new[] { Attr("InverseProperty", "\"Ghost\"") })
            });

            Assert.Equal(
                new[] { "builder.HasOne(x => x.User).WithOne(x => x.Ghost).HasForeignKey<UserProfile>(x => x.Id).IsRequired();" },
                Lines(profile, user, profile));
        }

        [Fact]
        public void DeleteBehavior_OnScalarForeignKeyProperty_IsHonored()
        {
            var blog = Entity("Blog", new[] { Prop("Id"), Prop("Posts", "ICollection<Post>") });
            var post = Entity("Post", new[]
            {
                Prop("Id"),
                Prop("BlogId", "int", attrs: new[] { Attr("DeleteBehavior", "DeleteBehavior.SetNull") }),
                Prop("Blog", "Blog", attrs: new[] { Attr("ForeignKey", "\"BlogId\"") })
            });

            Assert.Equal(
                new[] { "builder.HasOne(x => x.Blog).WithMany(x => x.Posts).HasForeignKey(x => x.BlogId).IsRequired().OnDelete(DeleteBehavior.SetNull);" },
                Lines(post, blog, post));
        }

        [Fact]
        public void DuplicateNavigationName_IsEmittedOnce()
        {
            var foo = Entity("Foo", new[] { Prop("Id"), Prop("Bars", "ICollection<Bar>") });
            var bar = Entity("Bar", new[]
            {
                Prop("Id"),
                Prop("FooId", "int", attrs: new[] { Attr("ForeignKey", "\"Foo\"") }),  // scalar -> nav "Foo"
                Prop("Foo", "Foo", attrs: new[] { Attr("ForeignKey", "\"FooId\"") })   // navigation "Foo"
            });

            Assert.Single(Lines(bar, foo, bar));
        }

        [Fact]
        public void AutoDiscovery_WithCollectionAndReference_PrefersCollection()
        {
            var foo = Entity("Foo", new[]
            {
                Prop("Id"),
                Prop("Bars", "ICollection<Bar>"),
                Prop("PrimaryBar", "Bar")
            });
            var bar = Entity("Bar", new[]
            {
                Prop("Id"),
                Prop("FooId", "int"),
                Prop("Foo", "Foo", attrs: new[] { Attr("ForeignKey", "\"FooId\"") })
            });

            Assert.Equal(
                new[] { "builder.HasOne(x => x.Foo).WithMany(x => x.Bars).HasForeignKey(x => x.FooId).IsRequired();" },
                Lines(bar, foo, bar));
        }

        [Fact]
        public void ScalarForeignKey_ToMissingNavigation_UsesDefaultInverse()
        {
            var entity = Entity("Order", new[]
            {
                Prop("Id"),
                Prop("CustomerId", "int", attrs: new[] { Attr("ForeignKey", "\"Customer\"") }) // no Customer nav
            });

            Assert.Equal(
                new[] { "builder.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).IsRequired();" },
                Lines(entity, entity));
        }
    }
}
