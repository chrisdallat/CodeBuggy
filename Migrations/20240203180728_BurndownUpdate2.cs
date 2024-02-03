using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeBuggy.Migrations
{
    /// <inheritdoc />
    public partial class BurndownUpdate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DoneCount",
                table: "DailyTicketCounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NonePriorityCount",
                table: "DailyTicketCounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UrgentPriorityCount",
                table: "DailyTicketCounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoneCount",
                table: "DailyTicketCounts");

            migrationBuilder.DropColumn(
                name: "NonePriorityCount",
                table: "DailyTicketCounts");

            migrationBuilder.DropColumn(
                name: "UrgentPriorityCount",
                table: "DailyTicketCounts");
        }
    }
}
