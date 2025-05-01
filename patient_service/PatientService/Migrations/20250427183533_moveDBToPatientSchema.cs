using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientService.Migrations
{
    /// <inheritdoc />
    public partial class moveDBToPatientSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "patient");

            migrationBuilder.RenameTable(
                name: "Patients",
                newName: "Patients",
                newSchema: "patient");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_PESEL",
                schema: "patient",
                table: "Patients",
                column: "PESEL",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patients_PESEL",
                schema: "patient",
                table: "Patients");

            migrationBuilder.RenameTable(
                name: "Patients",
                schema: "patient",
                newName: "Patients");
        }
    }
}
