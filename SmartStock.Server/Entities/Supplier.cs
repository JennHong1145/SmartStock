namespace SmartStock.Server.Entities
{
	public class Supplier : IAuditable, ISoftDelete
	{
		public int Id { get; set; }

		public required string Name { get; set; }

		public string? ContactEmail { get; set; }

		public string? Phone { get; set; }

		public string? Address { get; set; }
		public bool IsDeleted { get; set; }

		// Auditing
		public DateTime CreatedAt { get; set; }

		public DateTime? UpdatedAt { get; set; }

		public string? CreatedBy { get; set; }

		public string? UpdatedBy { get; set; }
	}
}