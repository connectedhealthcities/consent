using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CHC.Consent.EFCore.Migrations
{
    public partial class SingleIdentifierTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NhsNumber",
                table: "People");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "People");

            migrationBuilder.CreateTable(
                name: "IdentifierEntity",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Created = table.Column<DateTime>(nullable: false),
                    Deleted = table.Column<DateTime>(nullable: true),
                    PersonId = table.Column<long>(nullable: false),
                    TypeName = table.Column<string>(nullable: false),
                    Value = table.Column<string>(maxLength: 2147483647, nullable: true),
                    ValueType = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentifierEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentifierEntity_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IdentifierEntity_PersonId",
                table: "IdentifierEntity",
                column: "PersonId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdentifierEntity");

            migrationBuilder.AddColumn<string>(
                name: "NhsNumber",
                table: "People",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Sex",
                table: "People",
                nullable: true);
        }
    }
}
