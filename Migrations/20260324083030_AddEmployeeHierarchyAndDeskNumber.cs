using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ntigra.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeHierarchyAndDeskNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeskNumber",
                table: "Receptionists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeskNumber",
                table: "Receptionists");
        }
    }
}
