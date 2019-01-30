using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CHC.Consent.EFCore.Migrations
{
    public partial class DefinitionEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EvidenceDefinition",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Definition = table.Column<string>(maxLength: 2147483647, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidenceDefinition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdentifierDefinition",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Definition = table.Column<string>(maxLength: 2147483647, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentifierDefinition", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceDefinition_Name",
                table: "EvidenceDefinition",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentifierDefinition_Name",
                table: "IdentifierDefinition",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvidenceDefinition");

            migrationBuilder.DropTable(
                name: "IdentifierDefinition");
        }
    }
}
