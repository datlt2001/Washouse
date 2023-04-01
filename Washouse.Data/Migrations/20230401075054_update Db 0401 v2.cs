using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Washouse.Data.Migrations
{
    public partial class updateDb0401v2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedTime",
                table: "Deliveries");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeliveryDate",
                table: "Deliveries",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AddColumn<bool>(
                name: "DeliveryType",
                table: "Deliveries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "EstimateTime",
                table: "Deliveries",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryType",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "EstimateTime",
                table: "Deliveries");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeliveryDate",
                table: "Deliveries",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EstimatedTime",
                table: "Deliveries",
                type: "time",
                nullable: true);
        }
    }
}
