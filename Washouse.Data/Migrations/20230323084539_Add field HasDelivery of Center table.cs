using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Washouse.Data.Migrations
{
    public partial class AddfieldHasDeliveryofCentertable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Staffs_Centers_CenterId",
                table: "Staffs");

            migrationBuilder.AlterColumn<int>(
                name: "CenterId",
                table: "Staffs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "HasDelivery",
                table: "Centers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Staffs_Centers_CenterId",
                table: "Staffs",
                column: "CenterId",
                principalTable: "Centers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Staffs_Centers_CenterId",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "HasDelivery",
                table: "Centers");

            migrationBuilder.AlterColumn<int>(
                name: "CenterId",
                table: "Staffs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Staffs_Centers_CenterId",
                table: "Staffs",
                column: "CenterId",
                principalTable: "Centers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
