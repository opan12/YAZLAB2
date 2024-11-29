using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YAZLAB2.Migrations
{
    /// <inheritdoc />
    public partial class ilm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bildirimler_Etkinlikler_EtkinlikId",
                table: "Bildirimler");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IlgiAlanları",
                table: "IlgiAlanları");

            migrationBuilder.DropIndex(
                name: "IX_Bildirimler_EtkinlikId",
                table: "Bildirimler");

            migrationBuilder.AlterColumn<string>(
                name: "KullanıcıId",
                table: "IlgiAlanları",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "IlgiAlanları",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdminNotification",
                table: "Bildirimler",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Mesaj",
                table: "Bildirimler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IlgiAlanları",
                table: "IlgiAlanları",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IlgiAlanları",
                table: "IlgiAlanları");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "IlgiAlanları");

            migrationBuilder.DropColumn(
                name: "IsAdminNotification",
                table: "Bildirimler");

            migrationBuilder.DropColumn(
                name: "Mesaj",
                table: "Bildirimler");

            migrationBuilder.AlterColumn<string>(
                name: "KullanıcıId",
                table: "IlgiAlanları",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IlgiAlanları",
                table: "IlgiAlanları",
                column: "KullanıcıId");

            migrationBuilder.CreateIndex(
                name: "IX_Bildirimler_EtkinlikId",
                table: "Bildirimler",
                column: "EtkinlikId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bildirimler_Etkinlikler_EtkinlikId",
                table: "Bildirimler",
                column: "EtkinlikId",
                principalTable: "Etkinlikler",
                principalColumn: "EtkinlikId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
