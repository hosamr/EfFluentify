using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestFkModels
{
    public class User
    {
        public int Id { get; set; }
        public ICollection<Post> Posts { get; set; }
        public ICollection<Post> EditedPosts { get; set; }
    }

    public class Post
    {
        public int Id { get; set; }

        // 1. Basic Navigation FK
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User Author { get; set; }

        public int EditorId { get; set; }
        [ForeignKey("EditorId")]
        public User Editor { get; set; } // 9. Multiple FKs to same table
    }

    public class Role
    {
        public int Id { get; set; }
    }

    public class Admin
    {
        public int Id { get; set; }
        
        // 2. FK on FK Property
        [ForeignKey("Role")]
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }

    public class Composite
    {
        public int Key1 { get; set; }
        public int Key2 { get; set; }
    }

    public class CompositeChild
    {
        public int Id { get; set; }
        public int CKey1 { get; set; }
        public int CKey2 { get; set; }

        // 3. Composite FK
        [ForeignKey("CKey1, CKey2")]
        public Composite Composite { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        
        // 4. Self-Referencing
        [ForeignKey("ParentId")]
        public Category Parent { get; set; }
        
        // Inverse for self-ref would be implied or explicit?
        public ICollection<Category> Children { get; set; }
    }

    public class Blog
    {
        public int Id { get; set; }
        public BlogDetail Detail { get; set; }
    }

    public class BlogDetail
    {
        // 6. Shared PK (One-to-One)
        [Key]
        [ForeignKey("Blog")]
        public int BlogId { get; set; }
        
        public Blog Blog { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public int? OptionalCustomerId { get; set; }

        // 12. Nullable FK
        [ForeignKey("OptionalCustomerId")]
        public Customer? OptionalCustomer { get; set; }

        public int RequiredCustomerId { get; set; }
        
        // 7. Required Relationship
        [ForeignKey("RequiredCustomerId")]
        public Customer RequiredCustomer { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        // 8. Inverse Navigation & 10. InverseProperty
        [InverseProperty("OptionalCustomer")]
        public ICollection<Order> OptionalOrders { get; set; }

        [InverseProperty("RequiredCustomer")]
        public ICollection<Order> RequiredOrders { get; set; }
    }
}
