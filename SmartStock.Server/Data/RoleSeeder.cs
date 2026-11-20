using Microsoft.AspNetCore.Identity;

namespace SmartStock.Server.Data
{
	public static class RoleSeeder
	{
		private static readonly string[] Roles = { "Admin", "Staff" };

		public static async Task SeedRolesAsync(IServiceProvider services)
		{
			var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

			foreach (var roleName in Roles)
			{
				if (!await roleManager.RoleExistsAsync(roleName))
				{
					await roleManager.CreateAsync(new IdentityRole(roleName));
				}
			}
		}
	}
}