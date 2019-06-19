using Microsoft.EntityFrameworkCore.Migrations;

namespace CHC.Consent.EFCore.Migrations
{
    public partial class MakeConsentGivenByOptional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "GivenByPersonId",
                table: "Consent",
                nullable: true,
                oldClrType: typeof(long));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "GivenByPersonId",
                table: "Consent",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);
        }
    }
}
