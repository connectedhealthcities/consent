using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CHC.Consent.EFCore.Migrations
{
    public partial class SubjectIdentifier : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubjectIdentifiers",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CurrentValue = table.Column<long>(nullable: false),
                    StudyId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectIdentifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubjectIdentifiers_Study_StudyId",
                        column: x => x.StudyId,
                        principalTable: "Study",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubjectIdentifiers_StudyId",
                table: "SubjectIdentifiers",
                column: "StudyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubjectIdentifiers");
        }
    }
}
