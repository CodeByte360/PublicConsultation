using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PublicConsultation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AutoTrainModelAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatbotKnowledgeIndex",
                columns: table => new
                {
                    Oid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Keyword = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Intent = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceEntity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Weight = table.Column<double>(type: "float", nullable: false),
                    HitCount = table.Column<int>(type: "int", nullable: false),
                    LastTrainedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatbotKnowledgeIndex", x => x.Oid);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatbotKnowledgeIndex");
        }
    }
}
