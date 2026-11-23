namespace SmartStock.Server.Services
{
	// A rule was broken (not a bug). The API turns this into a 400.
	public class BusinessRuleException : Exception
	{
		public BusinessRuleException(string message) : base(message) { }
	}

	public class InsufficientStockException : BusinessRuleException
	{
		public InsufficientStockException(string sku, int requested, int available)
			: base($"Not enough stock for {sku}: requested {requested}, available {available}.") { }
	}
}