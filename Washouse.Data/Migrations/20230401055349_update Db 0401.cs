using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Washouse.Data.Migrations
{
    public partial class updateDb0401 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WalletId",
                table: "Centers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WalletId",
                table: "CenterRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WalletId",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Wallets",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WalletTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    FromWalletId = table.Column<int>(type: "int", nullable: false),
                    ToWalletId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdateTimeStamp = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Payments",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Wallets_From",
                        column: x => x.FromWalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Wallets_To",
                        column: x => x.ToWalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Centers_WalletId",
                table: "Centers",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_WalletId",
                table: "Accounts",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_WalletId",
                table: "Transactions",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_FromWalletId",
                table: "WalletTransactions",
                column: "FromWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_PaymentId",
                table: "WalletTransactions",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_ToWalletId",
                table: "WalletTransactions",
                column: "ToWalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Wallets",
                table: "Accounts",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Centers_Wallets",
                table: "Centers",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Wallets",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Centers_Wallets",
                table: "Centers");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "WalletTransactions");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Centers_WalletId",
                table: "Centers");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_WalletId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "WalletId",
                table: "Centers");

            migrationBuilder.DropColumn(
                name: "WalletId",
                table: "CenterRequests");

            migrationBuilder.DropColumn(
                name: "WalletId",
                table: "Accounts");
        }
    }
}
