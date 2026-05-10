using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITPRO_CRM.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRoleEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MauChinh",
                table: "CauHinh");

            migrationBuilder.DropColumn(
                name: "MauPhu",
                table: "CauHinh");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MauChinh",
                table: "CauHinh",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MauPhu",
                table: "CauHinh",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }
    }
}
