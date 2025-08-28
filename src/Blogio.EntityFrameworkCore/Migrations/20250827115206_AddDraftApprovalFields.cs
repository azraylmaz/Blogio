using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blogio.Migrations
{
    /// <inheritdoc />
    public partial class AddDraftApprovalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewerNote",
                table: "BlogPostDrafts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "BlogPostDrafts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewerNote",
                table: "BlogPostDrafts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "BlogPostDrafts");
        }
    }
}
