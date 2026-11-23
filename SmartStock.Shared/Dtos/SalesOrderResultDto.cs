namespace SmartStock.Shared.Dtos
{
	public class SalesOrderResultDto
	{
		public int OrderId { get; set; }
		public string InvoiceNumber { get; set; } = string.Empty;
		public decimal TotalAmount { get; set; }
	}
}