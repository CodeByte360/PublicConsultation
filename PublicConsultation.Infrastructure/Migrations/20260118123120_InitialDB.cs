using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PublicConsultation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiAnalysisResults",
                columns: table => new
                {
                    Oid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnalysisType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResultText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiAnalysisResults", x => x.Oid);
                });

            migrationBuilder.CreateTable(
                name: "Ministries",
                columns: table => new
                {
                    Oid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ministries", x => x.Oid);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Oid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Oid);
                });

            migrationBuilder.CreateTable(
                name: "DraftDocuments",
                columns: table => new
                {
                    Oid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinistryOrDepartment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinistryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConsultationStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConsultationEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftDocuments", x => x.Oid);
                    table.ForeignKey(
                        name: "FK_DraftDocuments_Ministries_MinistryId",
                        column: x => x.MinistryId,
                        principalTable: "Ministries",
                        principalColumn: "Oid");
                });

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                columns: table => new
                {
                    Oid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullNameEnglish = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullNameBangla = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NIDNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Designation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Division = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.Oid);
                    table.ForeignKey(
                        name: "FK_UserAccounts_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Oid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Oid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreviousHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelatedDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Oid);
                    table.ForeignKey(
                        name: "FK_AuditLogs_DraftDocuments_RelatedDocumentId",
                        column: x => x.RelatedDocumentId,
                        principalTable: "DraftDocuments",
                        principalColumn: "Oid");
                });

            migrationBuilder.CreateTable(
                name: "Rules",
                columns: table => new
                {
                    Oid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DraftDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SectionTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExistingProvision = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProposedProvision = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rules", x => x.Oid);
                    table.ForeignKey(
                        name: "FK_Rules_DraftDocuments_DraftDocumentId",
                        column: x => x.DraftDocumentId,
                        principalTable: "DraftDocuments",
                        principalColumn: "Oid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Opinions",
                columns: table => new
                {
                    Oid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpinionText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Suggestion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sentiment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opinions", x => x.Oid);
                    table.ForeignKey(
                        name: "FK_Opinions_Rules_RuleId",
                        column: x => x.RuleId,
                        principalTable: "Rules",
                        principalColumn: "Oid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Opinions_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Oid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_RelatedDocumentId",
                table: "AuditLogs",
                column: "RelatedDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftDocuments_MinistryId",
                table: "DraftDocuments",
                column: "MinistryId");

            migrationBuilder.CreateIndex(
                name: "IX_Opinions_RuleId",
                table: "Opinions",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Opinions_UserId",
                table: "Opinions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rules_DraftDocumentId",
                table: "Rules",
                column: "DraftDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_RoleId",
                table: "UserAccounts",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiAnalysisResults");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Opinions");

            migrationBuilder.DropTable(
                name: "Rules");

            migrationBuilder.DropTable(
                name: "UserAccounts");

            migrationBuilder.DropTable(
                name: "DraftDocuments");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Ministries");
        }
    }
}
