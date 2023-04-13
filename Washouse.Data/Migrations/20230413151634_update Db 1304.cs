using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Washouse.Data.Migrations
{
    public partial class updateDb1304 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_OrderDetails",
                table: "Feedbacks");

            migrationBuilder.RenameColumn(
                name: "OrderDetailId",
                table: "Feedbacks",
                newName: "ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedbacks_OrderDetailId",
                table: "Feedbacks",
                newName: "IX_Feedbacks_ServiceId");

            migrationBuilder.AddColumn<string>(
                name: "CancelReasonByCustomer",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelReasonByStaff",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderId",
                table: "Feedbacks",
                type: "varchar(20)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_OrderId",
                table: "Feedbacks",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Orders",
                table: "Feedbacks",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Services",
                table: "Feedbacks",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Orders",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Services",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_OrderId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "CancelReasonByCustomer",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CancelReasonByStaff",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Feedbacks");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "Feedbacks",
                newName: "OrderDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedbacks_ServiceId",
                table: "Feedbacks",
                newName: "IX_Feedbacks_OrderDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_OrderDetails",
                table: "Feedbacks",
                column: "OrderDetailId",
                principalTable: "OrderDetails",
                principalColumn: "Id");
        }
    }
}
