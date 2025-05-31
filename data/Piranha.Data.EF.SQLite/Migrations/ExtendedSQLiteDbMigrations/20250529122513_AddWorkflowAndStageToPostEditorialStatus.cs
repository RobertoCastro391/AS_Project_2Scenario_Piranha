using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Piranha.Data.EF.SQLite.Migrations.ExtendedSQLiteDbMigrations
{
    /// <inheritdoc />
    public partial class AddWorkflowAndStageToPostEditorialStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CurrentStageId",
                table: "PostEditorialStatuses",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "WorkflowId",
                table: "PostEditorialStatuses",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentStageId",
                table: "PostEditorialStatuses");

            migrationBuilder.DropColumn(
                name: "WorkflowId",
                table: "PostEditorialStatuses");
        }
    }
}
