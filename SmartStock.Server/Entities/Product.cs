namespace SmartStock.Server.Entities
{
	public class Product : IAuditable, ISoftDelete
	{
		public int Id { get; set; }

		public required string SKU { get; set; }

		public required string Name { get; set; }

		public string? Description { get; set; }

		public decimal Price { get; set; }

		public bool IsActive { get; set; } = true;

		public int CategoryId { get; set; }

		public Category? Category { get; set; }

		// Soft delete
		public bool IsDeleted { get; set; }

		// Auditing
		public DateTime CreatedAt { get; set; }

		public DateTime? UpdatedAt { get; set; }

		public string? CreatedBy { get; set; }

		public string? UpdatedBy { get; set; }

		// Concurrency
		public byte[] RowVersion { get; set; } = Array.Empty<byte>();
	}
}