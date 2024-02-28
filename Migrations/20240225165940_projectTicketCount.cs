using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeBuggy.Migrations
{
    /// <inheritdoc />
    public partial class projectTicketCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TicketsCount",
                table: "Projects",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TicketsCount",
                table: "Projects");
        }
    }
}
