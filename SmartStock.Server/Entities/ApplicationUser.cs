using Microsoft.AspNetCore.Identity;

namespace SmartStock.Server.Entities
{
	public class ApplicationUser : IdentityUser
	{
		public string? FullName { get; set; }
	}
}