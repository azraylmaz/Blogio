using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blogio.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlogPostLike",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BlogPostId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPostLike", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogPostLike_BlogPostId_UserId",
                table: "BlogPostLike",
                columns: new[] { "BlogPostId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogPostLike");
        }
    }
}
