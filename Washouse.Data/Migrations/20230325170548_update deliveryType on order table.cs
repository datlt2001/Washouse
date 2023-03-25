using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Washouse.Data.Migrations
{
    public partial class updatedeliveryTypeonordertable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.AddColumn<int>(
                name: "DeliveryType",
                table: "Orders",
                type: "int",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: 0);*/
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {/*
            migrationBuilder.DropColumn(
                name: "DeliveryType",
                table: "Orders");*/
        }
    }
}
