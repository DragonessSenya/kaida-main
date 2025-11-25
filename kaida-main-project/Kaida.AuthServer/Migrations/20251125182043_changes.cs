using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kaida.AuthServer.Migrations
{
    /// <inheritdoc />
    public partial class changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AppAccess",
                table: "AppAccess");

            migrationBuilder.DropIndex(
                name: "IX_AppAccess_UserId",
                table: "AppAccess");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AppAccess");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppAccess",
                table: "AppAccess",
                columns: new[] { "UserId", "AppId" });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Token = table.Column<string>(type: "TEXT", nullable: false),
                    Expiration = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRevoked = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppAccess",
                table: "AppAccess");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "AppAccess",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppAccess",
                table: "AppAccess",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AppAccess_UserId",
                table: "AppAccess",
                column: "UserId");
        }
    }
}
