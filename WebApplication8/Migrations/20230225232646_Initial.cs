using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication8.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
