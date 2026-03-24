using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IsArama.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BaseUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JobType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OriginalUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Hash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScrapedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Jobs_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Jobs_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Yazılım Geliştirme" },
                    { 2, "Veri Bilimi & Yapay Zeka" },
                    { 3, "Siber Güvenlik" },
                    { 4, "Sistem & Ağ Yönetimi" },
                    { 5, "UI/UX & Tasarım" },
                    { 6, "DevOps & Cloud" },
                    { 7, "Oyun Geliştirme" },
                    { 8, "Muhasebe" },
                    { 9, "Finans & Bankacılık" },
                    { 10, "Denetim & Vergi" },
                    { 11, "Sigortacılık" },
                    { 12, "Satış & Satış Temsilcisi" },
                    { 13, "Dijital Pazarlama" },
                    { 14, "Marka & İletişim" },
                    { 15, "E-ticaret" },
                    { 16, "Sosyal Medya Yönetimi" },
                    { 17, "Makine Mühendisliği" },
                    { 18, "Elektrik & Elektronik Mühendisliği" },
                    { 19, "İnşaat & İnşaat Mühendisliği" },
                    { 20, "Endüstri Mühendisliği" },
                    { 21, "Kimya Mühendisliği" },
                    { 22, "Biyomedikal Mühendisliği" },
                    { 23, "İnsan Kaynakları" },
                    { 24, "İdari İşler & Sekreterlik" },
                    { 25, "Genel Müdürlük & Üst Yönetim" },
                    { 26, "Doktor & Uzman Hekim" },
                    { 27, "Hemşire & Sağlık Personeli" },
                    { 28, "Eczacılık" },
                    { 29, "Diş Hekimliği" },
                    { 30, "Psikoloji & Danışmanlık" },
                    { 31, "Öğretmenlik" },
                    { 32, "Akademik & Araştırma" },
                    { 33, "Özel Ders & Eğitmenlik" },
                    { 34, "Çocuk Gelişimi & Bakım" },
                    { 35, "Lojistik & Tedarik Zinciri" },
                    { 36, "Depo & Ambar" },
                    { 37, "Şoför & Kurye" },
                    { 38, "Üretim & İmalat" },
                    { 39, "Kalite Kontrol" },
                    { 40, "Müşteri Hizmetleri" },
                    { 41, "Çağrı Merkezi" },
                    { 42, "Teknik Destek" },
                    { 43, "Hukuk & Avukatlık" },
                    { 44, "Kamu & Devlet" },
                    { 45, "Askerlik & Güvenlik" },
                    { 46, "Gazetecilik & İçerik" },
                    { 47, "Grafik Tasarım" },
                    { 48, "Fotoğrafçılık & Video" },
                    { 49, "Mimarlık & İç Mimarlık" },
                    { 50, "Turizm & Otelcilik" },
                    { 51, "Aşçılık & Mutfak" },
                    { 52, "Garsonluk & Servis" },
                    { 53, "Perakende & Mağazacılık" },
                    { 54, "Moda & Tekstil" },
                    { 55, "Part-time & Yarı Zamanlı" },
                    { 56, "Staj & Yeni Mezun" },
                    { 57, "Uzaktan Çalışma" },
                    { 58, "Freelance" },
                    { 59, "Diğer" }
                });

            migrationBuilder.InsertData(
                table: "Sources",
                columns: new[] { "Id", "BaseUrl", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "https://www.kariyer.net", true, "Kariyer.net" },
                    { 2, "https://www.eleman.net", true, "Eleman.net" },
                    { 3, "https://www.yenibiris.com", true, "Yenibiris.com" },
                    { 4, "https://www.secretcv.com", true, "Secretcv.com" },
                    { 5, "https://www.iskur.gov.tr", true, "İŞKUR" },
                    { 6, "https://www.sahibinden.com", true, "Sahibinden.com" },
                    { 7, "https://ilan.memurlar.net", true, "Memurlar.net" },
                    { 8, "https://tr.indeed.com", true, "Indeed TR" },
                    { 9, "https://www.adzuna.com.tr", true, "Adzuna TR" },
                    { 10, "https://jooble.org/tr", true, "Jooble TR" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Name",
                table: "Companies",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CategoryId",
                table: "Jobs",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CompanyId",
                table: "Jobs",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Hash",
                table: "Jobs",
                column: "Hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_SourceId",
                table: "Jobs",
                column: "SourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Sources");
        }
    }
}
