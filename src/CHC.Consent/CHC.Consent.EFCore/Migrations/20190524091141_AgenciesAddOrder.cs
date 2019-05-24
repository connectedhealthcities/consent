using Microsoft.EntityFrameworkCore.Migrations;

namespace CHC.Consent.EFCore.Migrations
{
    public partial class AgenciesAddOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AgencyField_AgencyId",
                table: "AgencyField");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "AgencyField",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AgencyField_AgencyId_Order",
                table: "AgencyField",
                columns: new[] { "AgencyId", "Order" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AgencyField_AgencyId_Order",
                table: "AgencyField");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "AgencyField");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyField_AgencyId",
                table: "AgencyField",
                column: "AgencyId");
        }
    }
}
