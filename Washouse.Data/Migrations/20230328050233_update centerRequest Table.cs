using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Washouse.Data.Migrations
{
    public partial class updatecenterRequestTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloseTime",
                table: "CenterRequests");

            migrationBuilder.DropColumn(
                name: "OpenTime",
                table: "CenterRequests");

            migrationBuilder.DropColumn(
                name: "WeekOff",
                table: "CenterRequests");

            migrationBuilder.AddColumn<bool>(
                name: "HasDelivery",
                table: "CenterRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasDelivery",
                table: "CenterRequests");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "CloseTime",
                table: "CenterRequests",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "OpenTime",
                table: "CenterRequests",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WeekOff",
                table: "CenterRequests",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);
        }
    }
}
