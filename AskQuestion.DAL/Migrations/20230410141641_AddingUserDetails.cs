using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AskQuestion.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddingUserDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("f2dca6c4-ea50-447a-ad25-e27062f79d4b"));

            migrationBuilder.CreateTable(
                name: "UserDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    AdditionalInfo = table.Column<string>(type: "text", nullable: true),
                    Сreated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDetails_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Login", "Password", "Updated", "UserRoleId", "Сreated" },
                values: new object[] { new Guid("64f2b7ef-5a3d-4b9f-a5f3-2b38776ecaf4"), "Admin", "$2a$11$MFy029zl31FZKB0uHT6jx.Uc7Q3TEIZjLKPi9utOY7QF4g3McQF7G", null, 1, new DateTimeOffset(new DateTime(2023, 4, 10, 14, 16, 41, 146, DateTimeKind.Unspecified).AddTicks(269), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.CreateIndex(
                name: "IX_UserDetails_UserId",
                table: "UserDetails",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDetails");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64f2b7ef-5a3d-4b9f-a5f3-2b38776ecaf4"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Login", "Password", "Updated", "UserRoleId", "Сreated" },
                values: new object[] { new Guid("f2dca6c4-ea50-447a-ad25-e27062f79d4b"), "Admin", "$2a$11$6HFoxC3xJUEzE.GT6DcCwuq4mAVMyAdf6kbYcm/bJT/a47/14oD5e", null, 1, new DateTimeOffset(new DateTime(2023, 3, 25, 11, 11, 36, 851, DateTimeKind.Unspecified).AddTicks(3603), new TimeSpan(0, 0, 0, 0, 0)) });
        }
    }
}
