namespace SmartStock.Server.Entities
{
	public class PurchaseOrder
	{
		public int Id { get; set; }

		public int SupplierId { get; set; }

		public Supplier? Supplier { get; set; }

		public DateTime OrderDate { get; set; }

		public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

		public decimal TotalAmount { get; set; }

		public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
	}
}