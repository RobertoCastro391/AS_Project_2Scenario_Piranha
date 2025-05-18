using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Editorial.Workflows.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBindingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentBindings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CurrentStageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", nullable: false),
                    ContentId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentBindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentBindings_WorkflowStages_CurrentStageId",
                        column: x => x.CurrentStageId,
                        principalTable: "WorkflowStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentBindings_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentBindings_CurrentStageId",
                table: "ContentBindings",
                column: "CurrentStageId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentBindings_WorkflowId",
                table: "ContentBindings",
                column: "WorkflowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentBindings");
        }
    }
}
