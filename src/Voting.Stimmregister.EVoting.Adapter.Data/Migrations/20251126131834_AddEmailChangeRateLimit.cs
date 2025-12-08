using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmregister.EVoting.Adapter.Data.Migrations;

/// <inheritdoc />
public partial class AddEmailChangeRateLimit : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "EmailChangeCount",
            table: "RateLimits",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "EmailChangeCount",
            table: "RateLimits");
    }
}
