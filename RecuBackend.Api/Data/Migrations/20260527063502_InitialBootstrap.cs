using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecuBackend.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialBootstrap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BootstrapPings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Message = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BootstrapPings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BootstrapPings_CreatedAtUtc",
                table: "BootstrapPings",
                column: "CreatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BootstrapPings");
        }
    }
}
