using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IsArama.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Sources",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Sources",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Sources",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Sources",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Sources",
                keyColumn: "Id",
                keyValue: 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Sources",
                columns: new[] { "Id", "BaseUrl", "IsActive", "Name" },
                values: new object[,]
                {
                    { 5, "https://www.iskur.gov.tr", true, "İŞKUR" },
                    { 6, "https://www.sahibinden.com", true, "Sahibinden.com" },
                    { 8, "https://tr.indeed.com", true, "Indeed TR" },
                    { 9, "https://www.adzuna.com.tr", true, "Adzuna TR" },
                    { 10, "https://jooble.org/tr", true, "Jooble TR" }
                });
        }
    }
}
