using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Washouse.Data.Migrations
{
    public partial class updateservicpriceemaxValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxWeight",
                table: "ServicePrices");

            migrationBuilder.DropColumn(
                name: "MinWeight",
                table: "ServicePrices");

            migrationBuilder.AddColumn<decimal>(
                name: "MaxValue",
                table: "ServicePrices",
                type: "decimal(8,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Measurement",
                table: "OrderDetails",
                type: "decimal(8,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxValue",
                table: "ServicePrices");

            migrationBuilder.AddColumn<decimal>(
                name: "MaxWeight",
                table: "ServicePrices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MinWeight",
                table: "ServicePrices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Measurement",
                table: "OrderDetails",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,3)");
        }
    }
}
