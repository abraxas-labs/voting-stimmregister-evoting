using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmregister.EVoting.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Persons",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmailVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ContextId = table.Column<string>(type: "text", nullable: false),
                    VerificationCode = table.Column<string>(type: "text", nullable: false),
                    Ahvn13 = table.Column<long>(type: "bigint", nullable: false),
                    BfsCanton = table.Column<short>(type: "smallint", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerifications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerifications_Ahvn13",
                table: "EmailVerifications",
                column: "Ahvn13",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerifications_ContextId",
                table: "EmailVerifications",
                column: "ContextId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailVerifications");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Persons");
        }
    }
}
