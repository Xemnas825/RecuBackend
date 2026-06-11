using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecuBackend.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCloudinaryAttachmentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "FileAttachments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResourceType",
                table: "FileAttachments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "FileAttachments");

            migrationBuilder.DropColumn(
                name: "ResourceType",
                table: "FileAttachments");
        }
    }
}
