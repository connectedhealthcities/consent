using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CHC.Consent.EFCore.Migrations
{
    public partial class Stage1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BirthOrder = table.Column<int>(nullable: true),
                    DateOfBirth = table.Column<DateTime>(nullable: true),
                    NhsNumber = table.Column<string>(nullable: true),
                    Sex = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BradfordHosptialNumber",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    HospitalNumber = table.Column<string>(nullable: true),
                    PersonId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BradfordHosptialNumber", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BradfordHosptialNumber_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonName",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EffectiveFrom = table.Column<DateTime>(nullable: false),
                    EffectiveTo = table.Column<DateTime>(nullable: true),
                    Family = table.Column<string>(nullable: true),
                    Given = table.Column<string>(nullable: true),
                    PersonId = table.Column<long>(nullable: false),
                    Prefix = table.Column<string>(nullable: true),
                    Suffix = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonName", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonName_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BradfordHosptialNumber_PersonId",
                table: "BradfordHosptialNumber",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonName_PersonId",
                table: "PersonName",
                column: "PersonId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BradfordHosptialNumber");

            migrationBuilder.DropTable(
                name: "PersonName");

            migrationBuilder.DropTable(
                name: "People");
        }
    }
}
