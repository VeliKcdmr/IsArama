using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IsArama.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCategoryAddStringCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Categories_CategoryId",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_CategoryId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Jobs");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Jobs",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [Sources] ON;
                IF NOT EXISTS (SELECT 1 FROM [Sources] WHERE [Id] = 11)
                    INSERT INTO [Sources] ([Id], [BaseUrl], [IsActive], [Name]) VALUES (11, 'https://www.isbul.net', 1, N'İşbul.net');
                IF NOT EXISTS (SELECT 1 FROM [Sources] WHERE [Id] = 12)
                    INSERT INTO [Sources] ([Id], [BaseUrl], [IsActive], [Name]) VALUES (12, 'https://www.linkedin.com', 1, N'LinkedIn TR');
                SET IDENTITY_INSERT [Sources] OFF;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Sources",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Sources",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Jobs");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CategoryId",
                table: "Jobs",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Categories_CategoryId",
                table: "Jobs",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
