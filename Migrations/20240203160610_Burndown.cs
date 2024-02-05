using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CodeBuggy.Migrations
{
    /// <inheritdoc />
    public partial class Burndown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BurndownData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BurndownData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyTicketCounts",
                columns: table => new
                {
                    BurndownDataId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TodoCount = table.Column<int>(type: "integer", nullable: false),
                    InProgressCount = table.Column<int>(type: "integer", nullable: false),
                    ReviewCount = table.Column<int>(type: "integer", nullable: false),
                    LowPriorityCount = table.Column<int>(type: "integer", nullable: false),
                    MediumPriorityCount = table.Column<int>(type: "integer", nullable: false),
                    HighPriorityCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyTicketCounts", x => new { x.BurndownDataId, x.Id });
                    table.ForeignKey(
                        name: "FK_DailyTicketCounts_BurndownData_BurndownDataId",
                        column: x => x.BurndownDataId,
                        principalTable: "BurndownData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyTicketCounts");

            migrationBuilder.DropTable(
                name: "BurndownData");
        }
    }
}
