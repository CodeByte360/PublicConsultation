using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PublicConsultation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateUserAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "UserAccounts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "UserAccounts");
        }
    }
}
