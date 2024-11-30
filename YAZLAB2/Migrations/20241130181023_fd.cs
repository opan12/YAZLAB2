using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YAZLAB2.Migrations
{
    /// <inheritdoc />
    public partial class fd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Puanlar",
                table: "Puanlar");

            migrationBuilder.AlterColumn<string>(
                name: "KullaniciID",
                table: "Puanlar",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "PuanId",
                table: "Puanlar",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Puanlar",
                table: "Puanlar",
                column: "PuanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Puanlar",
                table: "Puanlar");

            migrationBuilder.DropColumn(
                name: "PuanId",
                table: "Puanlar");

            migrationBuilder.AlterColumn<string>(
                name: "KullaniciID",
                table: "Puanlar",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Puanlar",
                table: "Puanlar",
                column: "KullaniciID");
        }
    }
}
