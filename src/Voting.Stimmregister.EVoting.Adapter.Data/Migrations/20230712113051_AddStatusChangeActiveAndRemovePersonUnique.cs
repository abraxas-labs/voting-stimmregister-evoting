using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmregister.EVoting.Adapter.Data.Migrations;

public partial class AddStatusChangeActiveAndRemovePersonUnique : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Persons_Ahvn13",
            table: "Persons");

        migrationBuilder.AddColumn<bool>(
            name: "Active",
            table: "EVotingStatusChanges",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Active",
            table: "EVotingStatusChanges");

        migrationBuilder.CreateIndex(
            name: "IX_Persons_Ahvn13",
            table: "Persons",
            column: "Ahvn13",
            unique: true);
    }
}
