using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurfSessions_API.Migrations
{
    /// <inheritdoc />
    public partial class addOwnerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Spots",
                type: "varchar(42)",
                maxLength: 42,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Sessions",
                type: "varchar(42)",
                maxLength: 42,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Spots");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Sessions");
        }
    }
}
