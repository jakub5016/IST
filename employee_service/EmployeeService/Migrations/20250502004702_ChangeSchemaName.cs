using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeService.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSchemaName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "employee");

            migrationBuilder.RenameTable(
                name: "employee",
                schema: "doctor",
                newName: "employee",
                newSchema: "employee");

            migrationBuilder.RenameTable(
                name: "doctor",
                schema: "doctor",
                newName: "doctor",
                newSchema: "employee");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "doctor");

            migrationBuilder.RenameTable(
                name: "employee",
                schema: "employee",
                newName: "employee",
                newSchema: "doctor");

            migrationBuilder.RenameTable(
                name: "doctor",
                schema: "employee",
                newName: "doctor",
                newSchema: "doctor");
        }
    }
}
