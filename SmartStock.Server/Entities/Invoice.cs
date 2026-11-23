namespace SmartStock.Server.Entities
{
	public class Invoice
	{
		public int Id { get; set; }

		public int SalesOrderId { get; set; }

		public SalesOrder? SalesOrder { get; set; }

		public required string InvoiceNumber { get; set; }

		public DateTime IssueDate { get; set; }

		public DateTime DueDate { get; set; }

		public bool IsPaid { get; set; }
	}
}