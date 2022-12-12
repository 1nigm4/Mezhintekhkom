using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mezhintekhkom.Site.Data.Migrations
{
    public partial class AddPassport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PassportId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Passports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Patronymic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Passports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PassportId",
                table: "AspNetUsers",
                column: "PassportId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Passports_PassportId",
                table: "AspNetUsers",
                column: "PassportId",
                principalTable: "Passports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Passports_PassportId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Passports");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PassportId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PassportId",
                table: "AspNetUsers");
        }
    }
}
