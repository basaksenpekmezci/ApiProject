using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApiProject.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class IlkKurulum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Firma",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirmaKodu = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    FirmaAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Firma", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FirmaAyar",
                columns: table => new
                {
                    FirmaId = table.Column<int>(type: "int", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TemaRengi = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Dil = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false, defaultValue: "tr-TR"),
                    ZamanDilimi = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "Europe/Istanbul"),
                    EkAyarlarJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirmaAyar", x => x.FirmaId);
                    table.CheckConstraint("CK_FirmaAyar_EkAyarlarJson", "[EkAyarlarJson] IS NULL OR ISJSON([EkAyarlarJson]) = 1");
                    table.ForeignKey(
                        name: "FK_FirmaAyar_Firma_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firma",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FirmaDomain",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirmaId = table.Column<int>(type: "int", nullable: false),
                    Domain = table.Column<string>(type: "varchar(253)", unicode: false, maxLength: 253, nullable: false),
                    VarsayilanMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirmaDomain", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FirmaDomain_Firma_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firma",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Kullanici",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirmaId = table.Column<int>(type: "int", nullable: false),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizeKullaniciAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SifreHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AdSoyad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    HataliGirisSayisi = table.Column<int>(type: "int", nullable: false),
                    KilitBitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SonGirisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanici", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kullanici_Firma_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firma",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Firma",
                columns: new[] { "Id", "AktifMi", "FirmaAdi", "FirmaKodu", "OlusturmaTarihi" },
                values: new object[,]
                {
                    { 1, true, "ABC Teknoloji", "ABC", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, true, "XYZ Lojistik", "XYZ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "FirmaAyar",
                columns: new[] { "FirmaId", "Dil", "EkAyarlarJson", "GuncellemeTarihi", "LogoUrl", "TemaRengi", "ZamanDilimi" },
                values: new object[,]
                {
                    { 1, "tr-TR", "{\"destekTelefonu\":\"0212 000 00 00\",\"modulStok\":true}", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://abc.ornek.com/logo.png", "#1E88E5", "Europe/Istanbul" },
                    { 2, "en-US", "{\"destekTelefonu\":\"+44 20 0000 0000\",\"modulStok\":false}", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://xyz.ornek.com/logo.png", "#43A047", "Europe/London" }
                });

            migrationBuilder.InsertData(
                table: "FirmaDomain",
                columns: new[] { "Id", "Domain", "FirmaId", "VarsayilanMi" },
                values: new object[,]
                {
                    { 1, "abc.localhost", 1, true },
                    { 2, "abc.ornek.com", 1, false },
                    { 3, "xyz.localhost", 2, true },
                    { 4, "xyz.ornek.com", 2, false }
                });

            migrationBuilder.InsertData(
                table: "Kullanici",
                columns: new[] { "Id", "AdSoyad", "AktifMi", "Email", "FirmaId", "HataliGirisSayisi", "KilitBitisTarihi", "KullaniciAdi", "NormalizeKullaniciAdi", "OlusturmaTarihi", "SifreHash", "SonGirisTarihi" },
                values: new object[,]
                {
                    { 1, "ABC Yönetici", true, "admin@abc.ornek.com", 1, 0, null, "admin", "ADMIN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "AQAAAAIAAYagAAAAEK7v3g7bOp36PP3Pt7XkS2ojtYQEV+xmJhHfv4IlNTvFfB/NvtBp/7YvrsT7oR7ZmA==", null },
                    { 2, "XYZ Yönetici", true, "admin@xyz.ornek.com", 2, 0, null, "admin", "ADMIN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "AQAAAAIAAYagAAAAEHmK/9vv7Kzqx3rpwssF1OXu0reqQ5Q7zWSuv1u86VH45HQ5xgEQh6RrkJKJ9gjVzg==", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Firma_FirmaKodu",
                table: "Firma",
                column: "FirmaKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FirmaDomain_Domain",
                table: "FirmaDomain",
                column: "Domain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FirmaDomain_FirmaId",
                table: "FirmaDomain",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_FirmaId_NormalizeKullaniciAdi",
                table: "Kullanici",
                columns: new[] { "FirmaId", "NormalizeKullaniciAdi" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FirmaAyar");

            migrationBuilder.DropTable(
                name: "FirmaDomain");

            migrationBuilder.DropTable(
                name: "Kullanici");

            migrationBuilder.DropTable(
                name: "Firma");
        }
    }
}
