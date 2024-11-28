using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YAZLAB2.Migrations
{
    /// <inheritdoc />
    public partial class ef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IlgiAlanları_AspNetUsers_KullanıcıId1",
                table: "IlgiAlanları");

            migrationBuilder.DropForeignKey(
                name: "FK_IlgiAlanları_Kategoris_KategoriId",
                table: "IlgiAlanları");

            migrationBuilder.DropIndex(
                name: "IX_IlgiAlanları_KategoriId",
                table: "IlgiAlanları");

            migrationBuilder.DropIndex(
                name: "IX_IlgiAlanları_KullanıcıId1",
                table: "IlgiAlanları");

            migrationBuilder.DropColumn(
                name: "KullanıcıId1",
                table: "IlgiAlanları");

            migrationBuilder.DropColumn(
                name: "IlgiAlanlari",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "EtkinlikResmi",
                table: "Etkinlikler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EtkinlikResmi",
                table: "Etkinlikler");

            migrationBuilder.AddColumn<string>(
                name: "KullanıcıId1",
                table: "IlgiAlanları",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IlgiAlanlari",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_IlgiAlanları_KategoriId",
                table: "IlgiAlanları",
                column: "KategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_IlgiAlanları_KullanıcıId1",
                table: "IlgiAlanları",
                column: "KullanıcıId1");

            migrationBuilder.AddForeignKey(
                name: "FK_IlgiAlanları_AspNetUsers_KullanıcıId1",
                table: "IlgiAlanları",
                column: "KullanıcıId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IlgiAlanları_Kategoris_KategoriId",
                table: "IlgiAlanları",
                column: "KategoriId",
                principalTable: "Kategoris",
                principalColumn: "KategoriId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
