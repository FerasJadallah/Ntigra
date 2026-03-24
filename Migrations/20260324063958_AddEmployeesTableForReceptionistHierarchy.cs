using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ntigra.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeesTableForReceptionistHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
INSERT INTO [Employees] ([Id], [EmployeeId], [FirstName], [LastName], [Department], [HireDate])
SELECT [Id], [EmployeeId], [FirstName], [LastName], [Department], [HireDate]
FROM [Receptionists];
");

            migrationBuilder.DropForeignKey(
                name: "FK_Receptionists_Users_Id",
                table: "Receptionists");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Receptionists");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "Receptionists");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Receptionists");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "Receptionists");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Receptionists");

            migrationBuilder.AddForeignKey(
                name: "FK_Receptionists_Employees_Id",
                table: "Receptionists",
                column: "Id",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receptionists_Employees_Id",
                table: "Receptionists");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Receptionists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeId",
                table: "Receptionists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Receptionists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "HireDate",
                table: "Receptionists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Receptionists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
UPDATE r
SET r.[EmployeeId] = e.[EmployeeId],
    r.[FirstName] = e.[FirstName],
    r.[LastName] = e.[LastName],
    r.[Department] = e.[Department],
    r.[HireDate] = e.[HireDate]
FROM [Receptionists] r
INNER JOIN [Employees] e ON e.[Id] = r.[Id];
");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.AddForeignKey(
                name: "FK_Receptionists_Users_Id",
                table: "Receptionists",
                column: "Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
