using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Washouse.Data.Migrations
{
    public partial class AddtableOperatingHour : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloseTime",
                table: "Centers");

            migrationBuilder.DropColumn(
                name: "OpenTime",
                table: "Centers");

            migrationBuilder.CreateTable(
                name: "DaysOfWeek",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DayName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DaysOfWeek", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperatingHour",
                columns: table => new
                {
                    CenterId = table.Column<int>(type: "int", nullable: false),
                    DaysOfWeekId = table.Column<int>(type: "int", nullable: false),
                    OpenTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    CloseTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatingHour", x => new { x.CenterId, x.DaysOfWeekId });
                    table.ForeignKey(
                        name: "FK_OperatingHours_Centers",
                        column: x => x.CenterId,
                        principalTable: "Centers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OperatingHours_DaysOfWeeks",
                        column: x => x.DaysOfWeekId,
                        principalTable: "DaysOfWeek",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperatingHour_DaysOfWeekId",
                table: "OperatingHour",
                column: "DaysOfWeekId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperatingHour");

            migrationBuilder.DropTable(
                name: "DaysOfWeek");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "CloseTime",
                table: "Centers",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "OpenTime",
                table: "Centers",
                type: "time",
                nullable: true);
        }
    }
}
