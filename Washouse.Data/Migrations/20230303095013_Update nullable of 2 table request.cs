using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Washouse.Data.Migrations
{
    public partial class Updatenullableof2tablerequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CenterRequests_Centers_CenterRequesting",
                table: "CenterRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Services_ServiceRequesting",
                table: "ServiceRequests");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceRequesting",
                table: "ServiceRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CenterRequesting",
                table: "CenterRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_CenterRequests_Centers_CenterRequesting",
                table: "CenterRequests",
                column: "CenterRequesting",
                principalTable: "Centers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Services_ServiceRequesting",
                table: "ServiceRequests",
                column: "ServiceRequesting",
                principalTable: "Services",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CenterRequests_Centers_CenterRequesting",
                table: "CenterRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Services_ServiceRequesting",
                table: "ServiceRequests");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceRequesting",
                table: "ServiceRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CenterRequesting",
                table: "CenterRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CenterRequests_Centers_CenterRequesting",
                table: "CenterRequests",
                column: "CenterRequesting",
                principalTable: "Centers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Services_ServiceRequesting",
                table: "ServiceRequests",
                column: "ServiceRequesting",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
