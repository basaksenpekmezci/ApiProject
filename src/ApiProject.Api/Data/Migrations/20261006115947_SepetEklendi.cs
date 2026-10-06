using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiProject.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class SepetEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SepetKalemi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirmaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KullaniciId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UrunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Adet = table.Column<int>(type: "int", nullable: false),
                    EklenmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SepetKalemi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SepetKalemi_Firma_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firma",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SepetKalemi_Kullanici_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SepetKalemi_Urun_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SepetKalemi_FirmaId_KullaniciId_UrunId",
                table: "SepetKalemi",
                columns: new[] { "FirmaId", "KullaniciId", "UrunId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SepetKalemi_KullaniciId",
                table: "SepetKalemi",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_SepetKalemi_UrunId",
                table: "SepetKalemi",
                column: "UrunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SepetKalemi");
        }
    }
}
