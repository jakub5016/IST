using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientService.Migrations
{
    /// <inheritdoc />
    public partial class changeNamingToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Patients",
                schema: "patient",
                table: "Patients");

            migrationBuilder.RenameTable(
                name: "Patients",
                schema: "patient",
                newName: "patients",
                newSchema: "patient");

            migrationBuilder.RenameColumn(
                name: "PESEL",
                schema: "patient",
                table: "patients",
                newName: "pesel");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "patient",
                table: "patients",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                schema: "patient",
                table: "patients",
                newName: "phone_number");

            migrationBuilder.RenameColumn(
                name: "LastName",
                schema: "patient",
                table: "patients",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "IsPhoneConfirmed",
                schema: "patient",
                table: "patients",
                newName: "is_phone_confirmed");

            migrationBuilder.RenameColumn(
                name: "IsIdentityConfirmed",
                schema: "patient",
                table: "patients",
                newName: "is_identity_confirmed");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                schema: "patient",
                table: "patients",
                newName: "first_name");

            migrationBuilder.RenameIndex(
                name: "IX_Patients_PESEL",
                schema: "patient",
                table: "patients",
                newName: "ix_patients_pesel");

            migrationBuilder.AddPrimaryKey(
                name: "pk_patients",
                schema: "patient",
                table: "patients",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_patients",
                schema: "patient",
                table: "patients");

            migrationBuilder.RenameTable(
                name: "patients",
                schema: "patient",
                newName: "Patients",
                newSchema: "patient");

            migrationBuilder.RenameColumn(
                name: "pesel",
                schema: "patient",
                table: "Patients",
                newName: "PESEL");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "patient",
                table: "Patients",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                schema: "patient",
                table: "Patients",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "last_name",
                schema: "patient",
                table: "Patients",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "is_phone_confirmed",
                schema: "patient",
                table: "Patients",
                newName: "IsPhoneConfirmed");

            migrationBuilder.RenameColumn(
                name: "is_identity_confirmed",
                schema: "patient",
                table: "Patients",
                newName: "IsIdentityConfirmed");

            migrationBuilder.RenameColumn(
                name: "first_name",
                schema: "patient",
                table: "Patients",
                newName: "FirstName");

            migrationBuilder.RenameIndex(
                name: "ix_patients_pesel",
                schema: "patient",
                table: "Patients",
                newName: "IX_Patients_PESEL");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Patients",
                schema: "patient",
                table: "Patients",
                column: "Id");
        }
    }
}
