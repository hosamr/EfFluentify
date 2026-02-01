using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Tests.TestData
{
    public class Order
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal price { get; set; }

        [Url]
        public string ProductUrl { get; set; }

        [EmailAddress]
        public string email { get; set; } = string.Empty;
		
        public string TemporaryToken { get; set; }
        public string CustomColumn { get; set; }
        public string CustomColumnNameOnly { get; set; }
		public byte[] RowVersion { get; set; }
		
		public int UserId { get; set; }

        public User User { get; set; } = null!;

    }
}