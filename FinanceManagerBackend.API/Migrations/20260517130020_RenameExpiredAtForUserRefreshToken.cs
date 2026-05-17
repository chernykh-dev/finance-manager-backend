using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagerBackend.API.Migrations
{
    /// <inheritdoc />
    public partial class RenameExpiredAtForUserRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "expired_at",
                table: "users",
                newName: "refresh_token_expired_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "refresh_token_expired_at",
                table: "users",
                newName: "expired_at");
        }
    }
}
