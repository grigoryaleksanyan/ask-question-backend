using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AskQuestion.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenameCyrillicCreatedColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Сreated",
                table: "Users",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "Сreated",
                table: "UserDetails",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "Сreated",
                table: "Questions",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "Сreated",
                table: "Feedback",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "Сreated",
                table: "FaqEntries",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "Сreated",
                table: "FaqCategories",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "Сreated",
                table: "Areas",
                newName: "Created");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Created",
                table: "Users",
                newName: "Сreated");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "UserDetails",
                newName: "Сreated");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "Questions",
                newName: "Сreated");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "Feedback",
                newName: "Сreated");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "FaqEntries",
                newName: "Сreated");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "FaqCategories",
                newName: "Сreated");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "Areas",
                newName: "Сreated");
        }
    }
}
