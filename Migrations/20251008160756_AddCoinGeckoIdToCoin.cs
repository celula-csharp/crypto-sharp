using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeguimientoCriptomonedas.Migrations
{
    /// <inheritdoc />
    public partial class AddCoinGeckoIdToCoin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoinGeckoId",
                table: "Coins",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoinGeckoId",
                table: "Coins");
        }
    }
}
