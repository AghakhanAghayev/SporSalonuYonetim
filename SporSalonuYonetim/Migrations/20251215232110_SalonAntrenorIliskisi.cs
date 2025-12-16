using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SporSalonuYonetim.Migrations
{
    /// <inheritdoc />
    public partial class SalonAntrenorIliskisi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AcilisSaati",
                table: "SporSalonlari",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KapanisSaati",
                table: "SporSalonlari",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SporSalonuId",
                table: "Antrenorler",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Antrenorler_SporSalonuId",
                table: "Antrenorler",
                column: "SporSalonuId");

            migrationBuilder.AddForeignKey(
                name: "FK_Antrenorler_SporSalonlari_SporSalonuId",
                table: "Antrenorler",
                column: "SporSalonuId",
                principalTable: "SporSalonlari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Antrenorler_SporSalonlari_SporSalonuId",
                table: "Antrenorler");

            migrationBuilder.DropIndex(
                name: "IX_Antrenorler_SporSalonuId",
                table: "Antrenorler");

            migrationBuilder.DropColumn(
                name: "AcilisSaati",
                table: "SporSalonlari");

            migrationBuilder.DropColumn(
                name: "KapanisSaati",
                table: "SporSalonlari");

            migrationBuilder.DropColumn(
                name: "SporSalonuId",
                table: "Antrenorler");
        }
    }
}
