using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN_MANGA_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class AddMangaDexChapterIdToChapter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MangaDexChapterId",
                table: "Chapters",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MangaDexChapterId",
                table: "Chapters");
        }
    }
}
