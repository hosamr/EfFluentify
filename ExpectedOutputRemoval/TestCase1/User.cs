using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFFluentify.Tests.TestData
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Email { get; set; }
		
        public string TemporaryToken { get; set; }
		
        public string TemporaryToken2 { get; set; }
		public string FirstName { get; set; }
		public string RowGuid { get; set; }
		        public List<Order> Orders { get; set; } = new();
        public Profile Profile { get; set; } = null!;
		public int? ManagerId { get; set; }
        public User Manager { get; set; }
        public ICollection<User> Subordinates { get; set; }


    }
}