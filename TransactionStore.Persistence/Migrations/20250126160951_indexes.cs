using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionStore.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class indexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transaction_AccountId",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_AccountId_Date",
                table: "Transactions",
                columns: new[] { "AccountId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Date_TransactionType",
                table: "Transactions",
                columns: new[] { "Date", "TransactionType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transaction_AccountId_Date",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_Date_TransactionType",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_AccountId",
                table: "Transactions",
                column: "AccountId");
        }
    }
}
