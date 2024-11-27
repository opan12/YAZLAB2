using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YAZLAB2.Migrations
{
    /// <inheritdoc />
    public partial class sdbk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KullanıcıId1",
                table: "IlgiAlanları",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DogumTarihi",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "IlgiAlanlari",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<DateTime>(
                name: "DogumTarihi",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
