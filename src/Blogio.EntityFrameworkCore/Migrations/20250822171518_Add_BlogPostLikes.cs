using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blogio.Migrations
{
    /// <inheritdoc />
    public partial class Add_BlogPostLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BlogPostLike",
                table: "BlogPostLike");

            migrationBuilder.RenameTable(
                name: "BlogPostLike",
                newName: "BlogPostLikes");

            migrationBuilder.RenameIndex(
                name: "IX_BlogPostLike_BlogPostId_UserId",
                table: "BlogPostLikes",
                newName: "IX_BlogPostLikes_BlogPostId_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlogPostLikes",
                table: "BlogPostLikes",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BlogPostLikes",
                table: "BlogPostLikes");

            migrationBuilder.RenameTable(
                name: "BlogPostLikes",
                newName: "BlogPostLike");

            migrationBuilder.RenameIndex(
                name: "IX_BlogPostLikes_BlogPostId_UserId",
                table: "BlogPostLike",
                newName: "IX_BlogPostLike_BlogPostId_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlogPostLike",
                table: "BlogPostLike",
                column: "Id");
        }
    }
}
