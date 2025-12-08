using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmregister.EVoting.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCantonBfsToPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "CantonBfs",
                table: "Persons",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.Sql(@"UPDATE ""Persons"" SET ""CantonBfs"" = 17");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CantonBfs",
                table: "Persons");
        }
    }
}
