using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN_MANGA_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalUrlToChapter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalUrl",
                table: "Chapters",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalUrl",
                table: "Chapters");
        }
    }
}
