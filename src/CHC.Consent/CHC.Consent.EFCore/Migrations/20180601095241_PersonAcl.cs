using Microsoft.EntityFrameworkCore.Migrations;

namespace CHC.Consent.EFCore.Migrations
{
    public partial class PersonAcl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AccessControlListId",
                table: "Person",
                nullable: true);
            
            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.Sql(
                    @"
DECLARE @tmp TABLE (AclId bigint, PersonId bigint);

MERGE INTO AccessControlList
USING (SELECT
         'Person ' + CAST(Person.Id as NVARCHAR(255)) AS Name,
         Id
       FROM Person) as S
ON 1 = 0
WHEN NOT MATCHED BY TARGET THEN INSERT (Description) VALUES (S.Name)
OUTPUT inserted.Id as AclId, S.Id as PersonId into @tmp;

UPDATE Person SET AccessControlListId = M.AclId
FROM Person
  INNER JOIN @tmp as M on M.PersonId = Person.Id;
");
            }

            migrationBuilder.AlterColumn<long>(
                name: "AccessControlListId",
                table: "Person",
                nullable: false
            );

            migrationBuilder.CreateIndex(
                name: "IX_Person_AccessControlListId",
                table: "Person",
                column: "AccessControlListId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Person_AccessControlList_AccessControlListId",
                table: "Person",
                column: "AccessControlListId",
                principalTable: "AccessControlList",
                principalColumn: "Id"
                );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Person_AccessControlList_AccessControlListId",
                table: "Person");

            migrationBuilder.DropIndex(
                name: "IX_Person_AccessControlListId",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "AccessControlListId",
                table: "Person");
        }
    }
}
