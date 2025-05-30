using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Piranha.Data.EF.SQLite.Migrations.ExtendedSQLiteDbMigrations
{
    /// <inheritdoc />
    public partial class RenameToPageEditorialStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostEditorialStatuses");

            migrationBuilder.CreateTable(
                name: "PageEditorialStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CurrentStageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageEditorialStatus", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PageEditorialStatus");

            migrationBuilder.CreateTable(
                name: "PostEditorialStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CurrentStageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PostId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostEditorialStatuses", x => x.Id);
                });
        }
    }
}
