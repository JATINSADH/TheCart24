using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheCart24.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutKeyToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckoutKey",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckoutKey",
                table: "Orders");
        }
    }
}
