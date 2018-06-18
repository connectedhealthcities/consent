using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CHC.Consent.EFCore.Migrations
{
    public partial class AccessAttempt1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AccessControlListId",
                table: "Study",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "AccessControlList",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessControlList", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permission",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Access = table.Column<string>(maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityPrinicipal",
                columns: table => new
                {
                    ConsentRoleId = table.Column<string>(nullable: true),
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Discriminator = table.Column<string>(nullable: false),
                    ConsentUserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityPrinicipal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityPrinicipal_AspNetRoles_ConsentRoleId",
                        column: x => x.ConsentRoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SecurityPrinicipal_AspNetUsers_ConsentUserId",
                        column: x => x.ConsentUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessControl",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AccessControlListId = table.Column<long>(nullable: false),
                    PermissionId = table.Column<long>(nullable: false),
                    SecurityPrincipalId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessControl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessControl_AccessControlList_AccessControlListId",
                        column: x => x.AccessControlListId,
                        principalTable: "AccessControlList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessControl_Permission_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessControl_SecurityPrinicipal_SecurityPrincipalId",
                        column: x => x.SecurityPrincipalId,
                        principalTable: "SecurityPrinicipal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            
            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.Sql(
                    @"
DECLARE @tmp TABLE (AclId bigint, StudyId bigint);

MERGE INTO AccessControlList
USING (SELECT
         'Study ' + CAST(Study.Id as NVARCHAR(255)) AS Name,
         Id
       FROM Study) as S
ON 1 = 0
WHEN NOT MATCHED BY TARGET THEN INSERT (Description) VALUES (S.Name)
OUTPUT inserted.Id as AclId, S.Id as StudyId into @tmp;

UPDATE Study SET AccessControlListId = M.AclId
FROM Study
  INNER JOIN @tmp as M on M.StudyId = Study.Id;


");
            }

            migrationBuilder.CreateIndex(
                name: "IX_Study_AccessControlListId",
                table: "Study",
                column: "AccessControlListId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccessControl_AccessControlListId",
                table: "AccessControl",
                column: "AccessControlListId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessControl_PermissionId",
                table: "AccessControl",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessControl_SecurityPrincipalId",
                table: "AccessControl",
                column: "SecurityPrincipalId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityPrinicipal_ConsentRoleId",
                table: "SecurityPrinicipal",
                column: "ConsentRoleId",
                unique: true,
                filter: "[ConsentRoleId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityPrinicipal_ConsentUserId",
                table: "SecurityPrinicipal",
                column: "ConsentUserId",
                unique: true,
                filter: "[ConsentUserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Study_AccessControlList_AccessControlListId",
                table: "Study",
                column: "AccessControlListId",
                principalTable: "AccessControlList",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Study_AccessControlList_AccessControlListId",
                table: "Study");

            migrationBuilder.DropTable(
                name: "AccessControl");

            migrationBuilder.DropTable(
                name: "AccessControlList");

            migrationBuilder.DropTable(
                name: "Permission");

            migrationBuilder.DropTable(
                name: "SecurityPrinicipal");

            migrationBuilder.DropIndex(
                name: "IX_Study_AccessControlListId",
                table: "Study");

            migrationBuilder.DropColumn(
                name: "AccessControlListId",
                table: "Study");
        }
    }
}
