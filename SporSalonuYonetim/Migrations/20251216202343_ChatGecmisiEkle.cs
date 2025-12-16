using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SporSalonuYonetim.Migrations
{
    /// <inheritdoc />
    public partial class ChatGecmisiEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatMesajlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Mesaj = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kimden = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UyeId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMesajlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMesajlari_AspNetUsers_UyeId",
                        column: x => x.UyeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMesajlari_UyeId",
                table: "ChatMesajlari",
                column: "UyeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMesajlari");
        }
    }
}
