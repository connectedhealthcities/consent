using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CHC.Consent.EFCore.Migrations
{
    public partial class ConsentHeader : Migration
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
                        onDelete: ReferentialAction.NoAction);
                });

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
                name: "Consent");
        }
    }
}
