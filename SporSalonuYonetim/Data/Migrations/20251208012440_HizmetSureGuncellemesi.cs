using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SporSalonuYonetim.Data.Migrations
{
    /// <inheritdoc />
    public partial class HizmetSureGuncellemesi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Sure",
                table: "Hizmetler",
                newName: "SureDakika");

            migrationBuilder.AlterColumn<string>(
                name: "Aciklama",
                table: "Hizmetler",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SureDakika",
                table: "Hizmetler",
                newName: "Sure");

            migrationBuilder.AlterColumn<string>(
                name: "Aciklama",
                table: "Hizmetler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
