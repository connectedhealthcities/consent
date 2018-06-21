using Microsoft.EntityFrameworkCore.Migrations;

namespace CHC.Consent.EFCore.Migrations
{
    public partial class ConsentAcl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AccessControlListId",
                table: "Consent",
                nullable: true);

          
            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.Sql(
                    @"
DECLARE @tmp TABLE (AclId bigint, ConsentId bigint);

MERGE INTO AccessControlList
USING (SELECT
         'Consent' + CAST(Consent.Id as NVARCHAR(255)) AS Name,
         Id
       FROM Consent) as S
ON 1 = 0
WHEN NOT MATCHED BY TARGET THEN INSERT (Description) VALUES (S.Name)
OUTPUT inserted.Id as AclId, S.Id as ConsentId into @tmp;

UPDATE Consent SET AccessControlListId = M.AclId
FROM Consent
  INNER JOIN @tmp as M on M.ConsentId = Consent.Id;
");
            }
            
            migrationBuilder.AlterColumn<long>(
                name: "AccessControlListId",
                table: "Consent",
                nullable: false);
            
            migrationBuilder.CreateIndex(
                name: "IX_Consent_AccessControlListId",
                table: "Consent",
                column: "AccessControlListId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Consent_AccessControlList_AccessControlListId",
                table: "Consent",
                column: "AccessControlListId",
                principalTable: "AccessControlList",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consent_AccessControlList_AccessControlListId",
                table: "Consent");

            migrationBuilder.DropIndex(
                name: "IX_Consent_AccessControlListId",
                table: "Consent");

            migrationBuilder.DropColumn(
                name: "AccessControlListId",
                table: "Consent");
        }
    }
}
