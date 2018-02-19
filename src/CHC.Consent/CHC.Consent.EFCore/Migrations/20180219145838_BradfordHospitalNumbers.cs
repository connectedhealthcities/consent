using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CHC.Consent.EFCore.Migrations
{
    public partial class BradfordHospitalNumbers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BradfordHospitalNumberEntity",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    HospitalNumber = table.Column<string>(nullable: true),
                    PersonEntityId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BradfordHospitalNumberEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BradfordHospitalNumberEntity_People_PersonEntityId",
                        column: x => x.PersonEntityId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BradfordHospitalNumberEntity_PersonEntityId",
                table: "BradfordHospitalNumberEntity",
                column: "PersonEntityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BradfordHospitalNumberEntity");
        }
    }
}
