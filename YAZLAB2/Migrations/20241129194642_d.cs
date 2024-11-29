using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YAZLAB2.Migrations
{
    /// <inheritdoc />
    public partial class d : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Lat",
                table: "Etkinlikler",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Lng",
                table: "Etkinlikler",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Lat",
                table: "AspNetUsers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Lng",
                table: "AspNetUsers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lat",
                table: "Etkinlikler");

            migrationBuilder.DropColumn(
                name: "Lng",
                table: "Etkinlikler");

            migrationBuilder.DropColumn(
                name: "Lat",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Lng",
                table: "AspNetUsers");
        }
    }
}
