using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Washouse.Data.Migrations
{
    public partial class AddServiceRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CenterRequests_Centers_CenterRequestId",
                table: "CenterRequests");

            migrationBuilder.DropIndex(
                name: "IX_CenterRequests_CenterRequestId",
                table: "CenterRequests");

            migrationBuilder.DropColumn(
                name: "CenterRequestId",
                table: "CenterRequests");

            migrationBuilder.CreateTable(
                name: "ServiceRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceRequesting = table.Column<int>(type: "int", nullable: false),
                    RequestStatus = table.Column<bool>(type: "bit", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Alias = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PriceType = table.Column<bool>(type: "bit", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TimeEstimate = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    HomeFlag = table.Column<bool>(type: "bit", nullable: true),
                    HotFlag = table.Column<bool>(type: "bit", nullable: true),
                    Rating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Services_ServiceRequesting",
                        column: x => x.ServiceRequesting,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CenterRequests_CenterRequesting",
                table: "CenterRequests",
                column: "CenterRequesting");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ServiceRequesting",
                table: "ServiceRequests",
                column: "ServiceRequesting");

            migrationBuilder.AddForeignKey(
                name: "FK_CenterRequests_Centers_CenterRequesting",
                table: "CenterRequests",
                column: "CenterRequesting",
                principalTable: "Centers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CenterRequests_Centers_CenterRequesting",
                table: "CenterRequests");

            migrationBuilder.DropTable(
                name: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_CenterRequests_CenterRequesting",
                table: "CenterRequests");

            migrationBuilder.AddColumn<int>(
                name: "CenterRequestId",
                table: "CenterRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CenterRequests_CenterRequestId",
                table: "CenterRequests",
                column: "CenterRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_CenterRequests_Centers_CenterRequestId",
                table: "CenterRequests",
                column: "CenterRequestId",
                principalTable: "Centers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
