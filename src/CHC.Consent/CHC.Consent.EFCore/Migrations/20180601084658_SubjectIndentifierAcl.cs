using Microsoft.EntityFrameworkCore.Migrations;

namespace CHC.Consent.EFCore.Migrations
{
    public partial class SubjectIndentifierAcl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SecurityPrinicipal_AspNetRoles_ConsentRoleId",
                table: "SecurityPrinicipal");

            migrationBuilder.DropForeignKey(
                name: "FK_SecurityPrinicipal_AspNetUsers_ConsentUserId",
                table: "SecurityPrinicipal");

            migrationBuilder.AddColumn<long>(
                name: "AccessControlListId",
                table: "SubjectIdentifiers",
                nullable: true);
            
            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.Sql(
                    @"
DECLARE @tmp TABLE (AclId bigint, SubjectIdentifierId bigint);

MERGE INTO AccessControlList
USING (SELECT
         'Subject Identifier ' + CAST(SubjectIdentifiers.Id as NVARCHAR(255)) AS Name,
         Id
       FROM SubjectIdentifiers) as S
ON 1 = 0
WHEN NOT MATCHED BY TARGET THEN INSERT (Description) VALUES (S.Name)
OUTPUT inserted.Id as AclId, S.Id as SubjectIdentifierId into @tmp;

UPDATE SubjectIdentifiers SET AccessControlListId = M.AclId
FROM SubjectIdentifiers
  INNER JOIN @tmp as M on M.SubjectIdentifierId = SubjectIdentifiers.Id;


");
            }
            
            migrationBuilder.AlterColumn<long>(
                name: "AccessControlListId",
                table: "SubjectIdentifiers",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_SubjectIdentifiers_AccessControlListId",
                table: "SubjectIdentifiers",
                column: "AccessControlListId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityPrinicipal_AspNetRoles_ConsentRoleId",
                table: "SecurityPrinicipal",
                column: "ConsentRoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityPrinicipal_AspNetUsers_ConsentUserId",
                table: "SecurityPrinicipal",
                column: "ConsentUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectIdentifiers_AccessControlList_AccessControlListId",
                table: "SubjectIdentifiers",
                column: "AccessControlListId",
                principalTable: "AccessControlList",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SecurityPrinicipal_AspNetRoles_ConsentRoleId",
                table: "SecurityPrinicipal");

            migrationBuilder.DropForeignKey(
                name: "FK_SecurityPrinicipal_AspNetUsers_ConsentUserId",
                table: "SecurityPrinicipal");

            migrationBuilder.DropForeignKey(
                name: "FK_SubjectIdentifiers_AccessControlList_AccessControlListId",
                table: "SubjectIdentifiers");

            migrationBuilder.DropIndex(
                name: "IX_SubjectIdentifiers_AccessControlListId",
                table: "SubjectIdentifiers");

            migrationBuilder.DropColumn(
                name: "AccessControlListId",
                table: "SubjectIdentifiers");

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityPrinicipal_AspNetRoles_ConsentRoleId",
                table: "SecurityPrinicipal",
                column: "ConsentRoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityPrinicipal_AspNetUsers_ConsentUserId",
                table: "SecurityPrinicipal",
                column: "ConsentUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
