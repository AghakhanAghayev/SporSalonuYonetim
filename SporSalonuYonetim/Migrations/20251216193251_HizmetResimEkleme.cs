using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SporSalonuYonetim.Migrations
{
    /// <inheritdoc />
    public partial class HizmetResimEkleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResimUrl",
                table: "Hizmetler",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResimUrl",
                table: "Hizmetler");
        }
    }
}
