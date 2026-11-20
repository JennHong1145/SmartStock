using System.ComponentModel.DataAnnotations;

namespace SmartStock.Server.Entities
{
	public class Category
	{
		public int Id { get; set; }

		[MaxLength(100)]
		public required string Name { get; set; }

		public string? Description { get; set; }

		public int? ParentCategoryId { get; set; }

		public Category? ParentCategory { get; set; }
	}
}
