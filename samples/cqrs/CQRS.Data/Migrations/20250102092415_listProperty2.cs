using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CQRS.Data.Migrations
{
    /// <inheritdoc />
    public partial class listProperty2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tests",
                table: "Todo",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tests",
                table: "Todo");
        }
    }
}
