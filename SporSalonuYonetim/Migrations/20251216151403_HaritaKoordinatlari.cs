using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SporSalonuYonetim.Migrations
{
    /// <inheritdoc />
    public partial class HaritaKoordinatlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Boylam",
                table: "SporSalonlari",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Enlem",
                table: "SporSalonlari",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Boylam",
                table: "SporSalonlari");

            migrationBuilder.DropColumn(
                name: "Enlem",
                table: "SporSalonlari");
        }
    }
}
