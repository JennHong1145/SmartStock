using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartStock.Server.Entities
{
	[Index(nameof(SKU), IsUnique = true)]
	public class Product
    {
		public int Id { get; set; }

		[MaxLength(100)]
		public required string SKU { get; set; }

		[MaxLength(100)]
		public required string Name { get; set; }

		public string? Description { get; set; }

		public decimal Price { get; set; }

		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; }

		public int CategoryId { get; set; }

		public Category? Category { get; set; }
	}
}
