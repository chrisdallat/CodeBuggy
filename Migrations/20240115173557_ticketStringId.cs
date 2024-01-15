using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeBuggy.Migrations
{
    /// <inheritdoc />
    public partial class ticketStringId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StringId",
                table: "Tickets",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StringId",
                table: "Tickets");
        }
    }
}
