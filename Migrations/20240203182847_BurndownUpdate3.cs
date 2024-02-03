using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeBuggy.Migrations
{
    /// <inheritdoc />
    public partial class BurndownUpdate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TodoCount",
                table: "DailyTicketCounts",
                newName: "ToDoCount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ToDoCount",
                table: "DailyTicketCounts",
                newName: "TodoCount");
        }
    }
}
