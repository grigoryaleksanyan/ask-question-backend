using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AskQuestion.DAL.Migrations
{
    /// <inheritdoc />
    public partial class QuestionFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Zone",
                table: "Questions",
                newName: "Area");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Area",
                table: "Questions",
                newName: "Zone");
        }
    }
}
