using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeService.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "doctor");

            migrationBuilder.CreateTable(
                name: "doctor",
                schema: "doctor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    specialization = table.Column<string>(type: "text", nullable: false),
                    room_number = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_doctor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "employee",
                schema: "doctor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    shift_start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    shift_end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employee", x => x.id);
                    table.ForeignKey(
                        name: "fk_employee_doctor_id",
                        column: x => x.id,
                        principalSchema: "doctor",
                        principalTable: "doctor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employee",
                schema: "doctor");

            migrationBuilder.DropTable(
                name: "doctor",
                schema: "doctor");
        }
    }
}
