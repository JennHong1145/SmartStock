namespace SmartStock.Server.Entities
{
	public class SalesOrder
	{
		public int Id { get; set; }

		public int CustomerId { get; set; }

		public Customer? Customer { get; set; }

		public DateTime OrderDate { get; set; }

		public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;

		public decimal TotalAmount { get; set; }

		public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();

		public Invoice? Invoice { get; set; }
		public byte[] RowVersion { get; set; } = Array.Empty<byte>();
	}
}