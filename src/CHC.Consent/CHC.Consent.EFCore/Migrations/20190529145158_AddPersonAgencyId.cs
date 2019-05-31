using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CHC.Consent.EFCore.Migrations
{
    public partial class AddPersonAgencyId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonAgencyId",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PersonId = table.Column<long>(nullable: false),
                    AgencyId = table.Column<long>(nullable: false),
                    SpecificId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonAgencyId", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonAgencyId_Agency_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agency",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonAgencyId_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonAgencyId_PersonId",
                table: "PersonAgencyId",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonAgencyId_AgencyId_SpecificId",
                table: "PersonAgencyId",
                columns: new[] { "AgencyId", "SpecificId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonAgencyId");
        }
    }
}
