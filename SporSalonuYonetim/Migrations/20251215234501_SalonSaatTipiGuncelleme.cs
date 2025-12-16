using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SporSalonuYonetim.Migrations
{
    /// <inheritdoc />
    public partial class SalonSaatTipiGuncelleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "KapanisSaati",
                table: "SporSalonlari",
                type: "time",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "AcilisSaati",
                table: "SporSalonlari",
                type: "time",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "KapanisSaati",
                table: "SporSalonlari",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AlterColumn<string>(
                name: "AcilisSaati",
                table: "SporSalonlari",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");
        }
    }
}
