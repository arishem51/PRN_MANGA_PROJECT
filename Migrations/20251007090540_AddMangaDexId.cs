using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN_MANGA_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class AddMangaDexId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MangaDexId",
                table: "Mangas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
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
