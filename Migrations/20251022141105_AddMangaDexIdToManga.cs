using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN_MANGA_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class AddMangaDexIdToManga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MangaDexId",
                table: "Mangas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MangaDexId",
                table: "Mangas");
        }
    }
}
