using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Washouse.Data.Migrations
{
    public partial class updatemeasurementandrate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "OrderDetails",
                newName: "Measurement");

            migrationBuilder.AddColumn<decimal>(
                name: "Rate",
                table: "Services",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Rate",
                table: "ServiceRequests",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rate",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Rate",
                table: "ServiceRequests");

            migrationBuilder.RenameColumn(
                name: "Measurement",
                table: "OrderDetails",
                newName: "Quantity");
        }
    }
}
