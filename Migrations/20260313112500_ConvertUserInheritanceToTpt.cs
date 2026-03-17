using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ntigra.Migrations
{
    /// <inheritdoc />
    public partial class ConvertUserInheritanceToTpt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSuperAdmin = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admins_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Patients_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Receptionists",
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
                    table.PrimaryKey("PK_Receptionists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Receptionists_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
INSERT INTO [Admins] ([Id], [FirstName], [LastName], [Department], [IsSuperAdmin])
SELECT [Id],
       COALESCE([Admin_FirstName], N''),
       COALESCE([Admin_LastName], N''),
       [Admin_Department],
       COALESCE([IsSuperAdmin], 0)
FROM [Users]
WHERE [Discriminator] = N'Admin';
");

            migrationBuilder.Sql(@"
INSERT INTO [Patients] ([Id], [FirstName], [LastName], [DateOfBirth], [Phone], [Address])
SELECT [Id],
       COALESCE([Patient_FirstName], [FirstName], N''),
       COALESCE([Patient_LastName], [LastName], N''),
       COALESCE([DateOfBirth], CONVERT(datetime2, '1900-01-01T00:00:00')),
       COALESCE([Phone], N''),
       COALESCE([Address], N'')
FROM [Users]
WHERE [Discriminator] = N'Patient';
");

            migrationBuilder.Sql(@"
INSERT INTO [Receptionists] ([Id], [EmployeeId], [FirstName], [LastName], [Department], [HireDate])
SELECT [Id],
       COALESCE([EmployeeId], N''),
       COALESCE([FirstName], N''),
       COALESCE([LastName], N''),
       [Department],
       [HireDate]
FROM [Users]
WHERE [Discriminator] = N'Receptionist';
");

            migrationBuilder.DropColumn(name: "Address", table: "Users");

            migrationBuilder.DropColumn(name: "Admin_Department", table: "Users");

            migrationBuilder.DropColumn(name: "Admin_FirstName", table: "Users");

            migrationBuilder.DropColumn(name: "Admin_LastName", table: "Users");

            migrationBuilder.DropColumn(name: "DateOfBirth", table: "Users");

            migrationBuilder.DropColumn(name: "Department", table: "Users");

            migrationBuilder.DropColumn(name: "Discriminator", table: "Users");

            migrationBuilder.DropColumn(name: "EmployeeId", table: "Users");

            migrationBuilder.DropColumn(name: "FirstName", table: "Users");

            migrationBuilder.DropColumn(name: "HireDate", table: "Users");

            migrationBuilder.DropColumn(name: "IsSuperAdmin", table: "Users");

            migrationBuilder.DropColumn(name: "LastName", table: "Users");

            migrationBuilder.DropColumn(name: "Patient_FirstName", table: "Users");

            migrationBuilder.DropColumn(name: "Patient_LastName", table: "Users");

            migrationBuilder.DropColumn(name: "Phone", table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Admins");

            migrationBuilder.DropTable(name: "Patients");

            migrationBuilder.DropTable(name: "Receptionists");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Admin_Department",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Admin_FirstName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Admin_LastName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Users",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmployeeId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HireDate",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuperAdmin",
                table: "Users",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Patient_FirstName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Patient_LastName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE [Users]
SET [Discriminator] = N'Admin',
    [Admin_Department] = a.[Department],
    [Admin_FirstName] = a.[FirstName],
    [Admin_LastName] = a.[LastName],
    [IsSuperAdmin] = a.[IsSuperAdmin]
FROM [Users] u
INNER JOIN [Admins] a ON a.[Id] = u.[Id];
");

            migrationBuilder.Sql(@"
UPDATE [Users]
SET [Discriminator] = N'Patient',
    [Patient_FirstName] = p.[FirstName],
    [Patient_LastName] = p.[LastName],
    [DateOfBirth] = p.[DateOfBirth],
    [Phone] = p.[Phone],
    [Address] = p.[Address]
FROM [Users] u
INNER JOIN [Patients] p ON p.[Id] = u.[Id];
");

            migrationBuilder.Sql(@"
UPDATE [Users]
SET [Discriminator] = N'Receptionist',
    [EmployeeId] = r.[EmployeeId],
    [FirstName] = r.[FirstName],
    [LastName] = r.[LastName],
    [Department] = r.[Department],
    [HireDate] = r.[HireDate]
FROM [Users] u
INNER JOIN [Receptionists] r ON r.[Id] = u.[Id];
");

            migrationBuilder.Sql(@"
UPDATE [Users]
SET [Discriminator] = N'User'
WHERE [Discriminator] = N'';
");
        }
    }
}
