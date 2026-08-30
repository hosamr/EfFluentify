using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EFFluentify.Tests.TestData.TestFkCases
{
    // 1. Basic Foreign Key on Navigation Property
    public class Blog
    {
        public int Id { get; set; }
        public ICollection<Post> Posts { get; set; } = null!;
    }

    public class Post
    {
        public int Id { get; set; }
        public int BlogId { get; set; }

        [ForeignKey("BlogId")]
        public Blog Blog { get; set; } = null!;
    }

    // 2. Foreign Key on Foreign Key Property
    public class Author
    {
        public int Id { get; set; }
        public ICollection<Book> Books { get; set; } = null!;
    }

    public class Book
    {
        public int Id { get; set; }

        [ForeignKey("AuthorNav")]
        public int AuthorId { get; set; }
        public Author AuthorNav { get; set; } = null!;
    }

    // 3. Composite Foreign Keys
    public class Team
    {
        [Key]
        public int TeamId { get; set; }
        [Key]
        public int LeagueId { get; set; }

        public ICollection<Player> Players { get; set; } = null!;
    }

    public class Player
    {
        public int Id { get; set; }

        public int MyTeamId { get; set; }
        public int MyLeagueId { get; set; }

        [ForeignKey("MyTeamId, MyLeagueId")]
        public Team Team { get; set; } = null!;
    }

    // 4. Self-Referencing Foreign Key
    public class Employee
    {
        public int Id { get; set; }
        public int? ManagerId { get; set; }

        [ForeignKey("ManagerId")]
        public Employee? Manager { get; set; }
        public ICollection<Employee> Subordinates { get; set; } = null!;
    }

    // 5. One-to-One Relationship
    public class Car
    {
        public int Id { get; set; }
        public Engine Engine { get; set; } = null!;
    }

    public class Engine
    {
        public int Id { get; set; }
        public int CarId { get; set; }

        [ForeignKey("CarId")]
        public Car Car { get; set; } = null!;
    }

    // 6. One-to-One with Shared Primary Key
    public class User
    {
        public int Id { get; set; }
        public UserProfile Profile { get; set; } = null!;
    }

    public class UserProfile
    {
        [Key, ForeignKey("User")]
        public int Id { get; set; }
        public User User { get; set; } = null!;
    }

    // 7. Required vs Optional Relationships
    public class Course
    {
        public int Id { get; set; }

        public int? TeacherId { get; set; } // Optional
        [ForeignKey("TeacherId")]
        public Teacher? Teacher { get; set; }

        public int DepartmentId { get; set; } // Required
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; } = null!;
    }

    public class Teacher { public int Id { get; set; } }
    public class Department { public int Id { get; set; } }


    // 8. Inverse Navigation Property
    public class Parent
    {
        public int Id { get; set; }
        [InverseProperty("Mother")]
        public ICollection<Child> ChildrenByMother { get; set; } = null!;

        [InverseProperty("Father")]
        public ICollection<Child> ChildrenByFather { get; set; } = null!;
    }

    public class Child
    {
        public int Id { get; set; }

        public int MotherId { get; set; }
        [ForeignKey("MotherId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public Parent Mother { get; set; } = null!;

        public int FatherId { get; set; }
        [ForeignKey("FatherId")]
		[DeleteBehavior(DeleteBehavior.NoAction)]
        public Parent Father { get; set; } = null!;
    }

    // 9. Multiple Foreign Keys to Same Table
    public class Flight
    {
        public int Id { get; set; }

        public int DepartureAirportId { get; set; }
        public int ArrivalAirportId { get; set; }

        [ForeignKey("DepartureAirportId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Airport DepartureAirport { get; set; } = null!;

        [ForeignKey("ArrivalAirportId")]
        public Airport ArrivalAirport { get; set; } = null!;
    }

    public class Airport
    {
        public int Id { get; set; }
        [InverseProperty("DepartureAirport")]
        public ICollection<Flight> DepartingFlights { get; set; } = null!;

        [InverseProperty("ArrivalAirport")]
        public ICollection<Flight> ArrivingFlights { get; set; } = null!;
    }

    // 10. Foreign Key with Different Name than Convention
    public class Vendor
    {
        public int Id { get; set; }
        public ICollection<Product> Products { get; set; } = null!;
    }

    public class Product
    {
        public int Id { get; set; }
        public int VendorIdentifier { get; set; } // Not VendorId

        [ForeignKey("VendorIdentifier")]
        public Vendor Vendor { get; set; } = null!;
    }
}
