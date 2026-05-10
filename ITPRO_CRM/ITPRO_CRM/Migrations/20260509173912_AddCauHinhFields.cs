using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITPRO_CRM.Migrations
{
    /// <inheritdoc />
    public partial class AddCauHinhFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TenTrungTam",
                table: "CauHinh",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SoDienThoai",
                table: "CauHinh",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DiaChi",
                table: "CauHinh",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DinhDangNgay",
                table: "CauHinh",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DonViTienTe",
                table: "CauHinh",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Facebook",
                table: "CauHinh",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HotlineDuPhong",
                table: "CauHinh",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "CauHinh",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LuuTruSaoLuu",
                table: "CauHinh",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.AddColumn<string>(
                name: "MuiGio",
                table: "CauHinh",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NgonNgu",
                table: "CauHinh",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoPhienToiDa",
                table: "CauHinh",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TanSuatSaoLuu",
                table: "CauHinh",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ThoiGianHetPhien",
                table: "CauHinh",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TikTok",
                table: "CauHinh",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "CauHinh",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Youtube",
                table: "CauHinh",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DinhDangNgay",
                table: "CauHinh");

            migrationBuilder.DropColumn(
                name: "DonViTienTe",
                table: "CauHinh");

            migrationBuilder.DropColumn(
                name: "Facebook",
                table: "CauHinh");

            migrationBuilder.DropColumn(
                name: "HotlineDuPhong",
                table: "CauHinh");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "CauHinh");

            migrationBuilder.DropColumn(
                name: "LuuTruSaoLuu",
                table: "CauHinh");

            migrationBuilder.DropColumn(
                name: "MauChinh",
                table: "CauHinh");

            migrationBuilder.DropColumn(
                name: "MauPhu",
                table: "CauHinh");

            migrationBuilder.DropColumn(
                name: "MuiGio",
                table: "CauHinh");

            migrationBuilder.DropColumn(
                name: "NgonNgu",
                table: "CauHinh");

            migrationBuilder.DropColumn(
                name: "SoPhienToiDa",
                table: "CauHinh");

            migrationBuilder.DropColumn(
                name: "TanSuatSaoLuu",
                table: "CauHinh");

            migrationBuilder.DropColumn(
                name: "ThoiGianHetPhien",
                table: "CauHinh");

            migrationBuilder.DropColumn(
                name: "TikTok",
                table: "CauHinh");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "CauHinh");

            migrationBuilder.DropColumn(
                name: "Youtube",
                table: "CauHinh");

            migrationBuilder.AlterColumn<string>(
                name: "TenTrungTam",
                table: "CauHinh",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SoDienThoai",
                table: "CauHinh",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DiaChi",
                table: "CauHinh",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
