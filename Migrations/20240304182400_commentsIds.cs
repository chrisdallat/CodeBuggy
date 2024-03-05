using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeBuggy.Migrations
{
    /// <inheritdoc />
    public partial class commentsIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommentsCount",
                table: "Tickets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<List<int>>(
                name: "CommentsIds",
                table: "Tickets",
                type: "integer[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentsCount",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CommentsIds",
                table: "Tickets");
        }
    }
}
