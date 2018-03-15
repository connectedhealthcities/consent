using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CHC.Consent.EFCore.Migrations
{
    public partial class ConsentPartOne : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Consent",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DateProvided = table.Column<DateTime>(nullable: false),
                    DateWithdrawn = table.Column<DateTime>(nullable: true),
                    GivenByPersonId = table.Column<long>(nullable: false),
                    StudySubjectId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consent_Person_GivenByPersonId",
                        column: x => x.GivenByPersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Consent_StudySubject_StudySubjectId",
                        column: x => x.StudySubjectId,
                        principalTable: "StudySubject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseIdentifier",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ConsentId = table.Column<long>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    Value = table.Column<string>(maxLength: 2147483647, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseIdentifier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseIdentifier_Consent_ConsentId",
                        column: x => x.ConsentId,
                        principalTable: "Consent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseIdentifier_ConsentId",
                table: "CaseIdentifier",
                column: "ConsentId");

            migrationBuilder.CreateIndex(
                name: "IX_Consent_GivenByPersonId",
                table: "Consent",
                column: "GivenByPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Consent_StudySubjectId_DateProvided_DateWithdrawn",
                table: "Consent",
                columns: new[] { "StudySubjectId", "DateProvided", "DateWithdrawn" },
                unique: true,
                filter: "[DateWithdrawn] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseIdentifier");

            migrationBuilder.DropTable(
                name: "Consent");
        }
    }
}
