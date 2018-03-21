using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CHC.Consent.Api.Infrastructure.Migrations.Identity
{
    public partial class Roles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParentRoleId",
                schema: "Authentication",
                table: "AspNetRoles",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_ParentRoleId",
                schema: "Authentication",
                table: "AspNetRoles",
                column: "ParentRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoles_AspNetRoles_ParentRoleId",
                schema: "Authentication",
                table: "AspNetRoles",
                column: "ParentRoleId",
                principalSchema: "Authentication",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoles_AspNetRoles_ParentRoleId",
                schema: "Authentication",
                table: "AspNetRoles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetRoles_ParentRoleId",
                schema: "Authentication",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "ParentRoleId",
                schema: "Authentication",
                table: "AspNetRoles");
        }
    }
}
