using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PublicConsultation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateBiomatric : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Biometrics",
                columns: table => new
                {
                    Oid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeftThumb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LeftIndex = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RightThumb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RightIndex = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Biometrics", x => x.Oid);
                    table.ForeignKey(
                        name: "FK_Biometrics_UserAccounts_UserAccountId",
                        column: x => x.UserAccountId,
                        principalTable: "UserAccounts",
                        principalColumn: "Oid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Biometrics_UserAccountId",
                table: "Biometrics",
                column: "UserAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Biometrics");
        }
    }
}
