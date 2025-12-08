using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmregister.EVoting.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEmailChange",
                table: "EmailVerifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEmailChange",
                table: "EmailVerifications");
        }
    }
}
