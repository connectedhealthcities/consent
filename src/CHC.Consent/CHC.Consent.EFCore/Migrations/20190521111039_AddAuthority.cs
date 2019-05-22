using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CHC.Consent.EFCore.Migrations
{
    public partial class AddAuthority : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AuthorityId",
                table: "PersonIdentifier",
                nullable: true,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Authority",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false),
                    SystemName = table.Column<string>(nullable: false),
                    Priority = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authority", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonIdentifier_AuthorityId",
                table: "PersonIdentifier",
                column: "AuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_Authority_SystemName",
                table: "Authority",
                column: "SystemName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonIdentifier_Authority_AuthorityId",
                table: "PersonIdentifier",
                column: "AuthorityId",
                principalTable: "Authority",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonIdentifier_Authority_AuthorityId",
                table: "PersonIdentifier");

            migrationBuilder.DropTable(
                name: "Authority");

            migrationBuilder.DropIndex(
                name: "IX_PersonIdentifier_AuthorityId",
                table: "PersonIdentifier");

            migrationBuilder.DropColumn(
                name: "AuthorityId",
                table: "PersonIdentifier");
        }
    }
}
