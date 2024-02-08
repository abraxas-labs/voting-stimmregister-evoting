using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmregister.EVoting.Adapter.Data.Migrations;

public partial class AddRateLimit : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "RateLimits",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Ahvn13 = table.Column<long>(type: "bigint", nullable: false),
                Date = table.Column<DateOnly>(type: "date", nullable: false),
                ActionCount = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_RateLimits", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_RateLimits_Ahvn13_Date",
            table: "RateLimits",
            columns: new[] { "Ahvn13", "Date" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RateLimits");
    }
}
