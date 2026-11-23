using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartStock.Server.Data;
using SmartStock.Server.Entities;
using SmartStock.Server.Repositories;
using SmartStock.Server.Services;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("DefaultConnection"),
		sql => sql.EnableRetryOnFailure());

	// Log SQL only while developing, so production doesn't print every query
	if (builder.Environment.IsDevelopment())
	{
		options.LogTo(message => System.Diagnostics.Debug.WriteLine(message),
			new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);
	}
});

builder.Services.AddIdentityCore<ApplicationUser>()
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReportService, ReportService>();
// Let the Blazor WASM dev server call this API (it runs on a different port)
builder.Services.AddCors(options =>
{
	options.AddPolicy("DevClient", policy =>
		policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	await RoleSeeder.SeedRolesAsync(scope.ServiceProvider);

	// Realistic sample data only on a developer machine
	if (app.Environment.IsDevelopment())
	{
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		await DataSeeder.SeedAsync(db);
	}
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
	app.UseCors("DevClient");
}

app.UseAuthorization();

app.MapControllers();

app.Run();
