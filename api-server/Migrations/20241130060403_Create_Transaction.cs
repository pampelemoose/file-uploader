using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_server.Migrations
{
    /// <inheritdoc />
    public partial class Create_Transaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TransactionId = table.Column<string>(type: "VARCHAR", maxLength: 50, nullable: false),
                    AccountNo = table.Column<string>(type: "VARCHAR", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    CurrencyCode = table.Column<string>(type: "TEXT", nullable: false),
                    TransactedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");
        }
    }
}
