using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CHC.Consent.EFCore.Migrations
{
    public partial class Baseline : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    ParentRoleId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoles_AspNetRoles_ParentRoleId",
                        column: x => x.ParentRoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
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
                name: "Person",
                columns: table => new
                {
                    AccessControlListId = table.Column<long>(nullable: false),
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Person_AccessControlList_AccessControlListId",
                        column: x => x.AccessControlListId,
                        principalTable: "AccessControlList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Study",
                columns: table => new
                {
                    AccessControlListId = table.Column<long>(nullable: false),
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Study", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Study_AccessControlList_AccessControlListId",
                        column: x => x.AccessControlListId,
                        principalTable: "AccessControlList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<long>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<long>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false),
                    RoleId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecurityPrinicipal",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Discriminator = table.Column<string>(nullable: false),
                    ConsentRoleId = table.Column<long>(nullable: true),
                    ConsentUserId = table.Column<long>(nullable: true)
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
                name: "PersonIdentifier",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PersonId = table.Column<long>(nullable: false),
                    TypeName = table.Column<string>(nullable: false),
                    Value = table.Column<string>(maxLength: 2147483647, nullable: true),
                    ValueType = table.Column<string>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Deleted = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonIdentifier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonIdentifier_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudySubject",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PersonId = table.Column<long>(nullable: false),
                    StudyId = table.Column<long>(nullable: false),
                    SubjectIdentifier = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudySubject", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudySubject_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudySubject_Study_StudyId",
                        column: x => x.StudyId,
                        principalTable: "Study",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubjectIdentifiers",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CurrentValue = table.Column<long>(nullable: false),
                    StudyId = table.Column<long>(nullable: false),
                    AccessControlListId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectIdentifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubjectIdentifiers_AccessControlList_AccessControlListId",
                        column: x => x.AccessControlListId,
                        principalTable: "AccessControlList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubjectIdentifiers_Study_StudyId",
                        column: x => x.StudyId,
                        principalTable: "Study",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccessControl",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AccessControlListId = table.Column<long>(nullable: false),
                    SecurityPrincipalId = table.Column<long>(nullable: false),
                    PermissionId = table.Column<long>(nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Consent",
                columns: table => new
                {
                    AccessControlListId = table.Column<long>(nullable: false),
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GivenByPersonId = table.Column<long>(nullable: false),
                    StudySubjectId = table.Column<long>(nullable: false),
                    DateProvided = table.Column<DateTime>(nullable: false),
                    DateWithdrawn = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consent_AccessControlList_AccessControlListId",
                        column: x => x.AccessControlListId,
                        principalTable: "AccessControlList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Consent_Person_GivenByPersonId",
                        column: x => x.GivenByPersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    Value = table.Column<string>(maxLength: 2147483647, nullable: false),
                    Type = table.Column<string>(nullable: false),
                    ConsentId = table.Column<long>(nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Evidence",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Value = table.Column<string>(maxLength: 2147483647, nullable: false),
                    Type = table.Column<string>(nullable: false),
                    ConsentId = table.Column<long>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Evidence_Consent_ConsentId",
                        column: x => x.ConsentId,
                        principalTable: "Consent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Evidence_Consent_ConsentId1",
                        column: x => x.ConsentId,
                        principalTable: "Consent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_ParentRoleId",
                table: "AspNetRoles",
                column: "ParentRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CaseIdentifier_ConsentId",
                table: "CaseIdentifier",
                column: "ConsentId");

            migrationBuilder.CreateIndex(
                name: "IX_Consent_AccessControlListId",
                table: "Consent",
                column: "AccessControlListId",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Evidence_ConsentId",
                table: "Evidence",
                column: "ConsentId");

            migrationBuilder.CreateIndex(
                name: "IX_Evidence_ConsentId1",
                table: "Evidence",
                column: "ConsentId");

            migrationBuilder.CreateIndex(
                name: "IX_Person_AccessControlListId",
                table: "Person",
                column: "AccessControlListId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonIdentifier_PersonId",
                table: "PersonIdentifier",
                column: "PersonId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Study_AccessControlListId",
                table: "Study",
                column: "AccessControlListId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudySubject_PersonId",
                table: "StudySubject",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_StudySubject_StudyId_PersonId",
                table: "StudySubject",
                columns: new[] { "StudyId", "PersonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudySubject_StudyId_SubjectIdentifier",
                table: "StudySubject",
                columns: new[] { "StudyId", "SubjectIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubjectIdentifiers_AccessControlListId",
                table: "SubjectIdentifiers",
                column: "AccessControlListId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubjectIdentifiers_StudyId",
                table: "SubjectIdentifiers",
                column: "StudyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessControl");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CaseIdentifier");

            migrationBuilder.DropTable(
                name: "Evidence");

            migrationBuilder.DropTable(
                name: "PersonIdentifier");

            migrationBuilder.DropTable(
                name: "SubjectIdentifiers");

            migrationBuilder.DropTable(
                name: "Permission");

            migrationBuilder.DropTable(
                name: "SecurityPrinicipal");

            migrationBuilder.DropTable(
                name: "Consent");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "StudySubject");

            migrationBuilder.DropTable(
                name: "Person");

            migrationBuilder.DropTable(
                name: "Study");

            migrationBuilder.DropTable(
                name: "AccessControlList");
        }
    }
}
