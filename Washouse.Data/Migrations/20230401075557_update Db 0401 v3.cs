using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Washouse.Data.Migrations
{
    public partial class updateDb0401v3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EstimateTime",
                table: "Deliveries",
                newName: "EstimatedTime");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EstimatedTime",
                table: "Deliveries",
                newName: "EstimateTime");
        }
    }
}
