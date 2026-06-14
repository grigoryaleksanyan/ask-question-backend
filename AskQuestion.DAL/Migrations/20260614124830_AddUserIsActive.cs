using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AskQuestion.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql(
                @"UPDATE ""Users"" SET ""IsActive"" = false WHERE ""Id"" IN (SELECT ""UserId"" FROM ""UserDetails"" WHERE ""IsDeleted"" = true)");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserDetails",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                @"UPDATE ""UserDetails"" SET ""IsDeleted"" = true WHERE ""UserId"" IN (SELECT ""Id"" FROM ""Users"" WHERE ""IsActive"" = false)");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");
        }
    }
}
