using System.ComponentModel.DataAnnotations;

namespace SmartStock.Shared.Dtos
{
	public class CreateCategoryRequest
	{
		[Required, StringLength(100)]
		public string Name { get; set; } = string.Empty;

		public string? Description { get; set; }

		public int? ParentCategoryId { get; set; }
	}
}