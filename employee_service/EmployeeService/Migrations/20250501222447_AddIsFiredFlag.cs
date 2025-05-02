using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeService.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFiredFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_fired",
                schema: "doctor",
                table: "employee",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_fired",
                schema: "doctor",
                table: "employee");
        }
    }
}
