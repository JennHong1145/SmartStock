using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStock.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddReportingView : Migration
    {
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"
CREATE VIEW vw_SalesSummaryByProduct AS
SELECT
	p.Id AS ProductId,
	p.Name AS ProductName,
	SUM(i.Quantity) AS TotalQuantitySold,
	SUM(i.LineTotal) AS TotalRevenue
FROM SalesOrderItems AS i
INNER JOIN SalesOrders AS o ON o.Id = i.SalesOrderId
INNER JOIN Products AS p ON p.Id = i.ProductId
WHERE o.Status IN ('Confirmed', 'Shipped')
GROUP BY p.Id, p.Name;");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("DROP VIEW vw_SalesSummaryByProduct;");
		}
	}
}
