using FinanceManagerBackend.API.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagerBackend.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDeleteBehaviors : Migration
    {
        private static readonly string[] currencyCodeValues = new []{ "RUB", "USD", "EUR" };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_currencies_name",
                table: "currencies");

            migrationBuilder.DropColumn(
                name: "name",
                table: "currencies");

            migrationBuilder.AlterColumn<string>(
                name: "comment",
                table: "transactions",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "currencies",
                type: "character(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "currencies",
                keyColumn: "id",
                keyValues: new [] { DbInitializer.RubCurrencyId, DbInitializer.UsdCurrencyId, DbInitializer.EurCurrencyId },
                column: "code",
                values: currencyCodeValues);

            migrationBuilder.CreateIndex(
                name: "ix_transactions_account_id",
                table: "transactions",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_category_id",
                table: "transactions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_currencies_code",
                table: "currencies",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_accounts_currency_id",
                table: "accounts",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "ix_accounts_user_id",
                table: "accounts",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_accounts_currencies_currency_id",
                table: "accounts",
                column: "currency_id",
                principalTable: "currencies",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_accounts_users_user_id",
                table: "accounts",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_transactions_accounts_account_id",
                table: "transactions",
                column: "account_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_transactions_categories_category_id",
                table: "transactions",
                column: "category_id",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_accounts_currencies_currency_id",
                table: "accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_accounts_users_user_id",
                table: "accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_transactions_accounts_account_id",
                table: "transactions");

            migrationBuilder.DropForeignKey(
                name: "fk_transactions_categories_category_id",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "ix_transactions_account_id",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "ix_transactions_category_id",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "ix_currencies_code",
                table: "currencies");

            migrationBuilder.DropIndex(
                name: "ix_accounts_currency_id",
                table: "accounts");

            migrationBuilder.DropIndex(
                name: "ix_accounts_user_id",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "code",
                table: "currencies");

            migrationBuilder.AlterColumn<string>(
                name: "comment",
                table: "transactions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "currencies",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_currencies_name",
                table: "currencies",
                column: "name",
                unique: true);
        }
    }
}
