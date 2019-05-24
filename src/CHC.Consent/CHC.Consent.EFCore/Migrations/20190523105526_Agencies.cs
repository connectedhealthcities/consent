using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CHC.Consent.EFCore.Migrations
{
    public partial class Agencies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agency",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false),
                    SystemName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agency", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgencyField",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AgencyId = table.Column<long>(nullable: false),
                    IdentifierDefinitionId = table.Column<long>(nullable: false),
                    Subfields = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyField", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgencyField_Agency_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agency",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgencyField_IdentifierDefinition_IdentifierDefinitionId",
                        column: x => x.IdentifierDefinitionId,
                        principalTable: "IdentifierDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agency_SystemName",
                table: "Agency",
                column: "SystemName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgencyField_AgencyId",
                table: "AgencyField",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyField_IdentifierDefinitionId",
                table: "AgencyField",
                column: "IdentifierDefinitionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgencyField");

            migrationBuilder.DropTable(
                name: "Agency");
        }
    }
}
