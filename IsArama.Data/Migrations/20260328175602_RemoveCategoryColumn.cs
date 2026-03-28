using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsArama.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCategoryColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Jobs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Jobs",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }
    }
}
