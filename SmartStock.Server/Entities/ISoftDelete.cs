namespace SmartStock.Server.Entities
{
	public interface ISoftDelete
	{
		bool IsDeleted { get; set; }
	}
}