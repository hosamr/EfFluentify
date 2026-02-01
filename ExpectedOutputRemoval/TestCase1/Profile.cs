using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Tests.TestData
{
    public class Profile
    {
        public string Name { get; set; }
		public string Email { get; set; }
		public List<string> Tags { get; set; } = new();
		public int UserId { get; set; }

        public User User { get; set; } = null!;

    }
}
