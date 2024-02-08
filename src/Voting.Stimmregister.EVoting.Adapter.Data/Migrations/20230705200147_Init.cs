using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmregister.EVoting.Adapter.Data.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EVotingStatusChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EVotingRegistered = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ContextId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVotingStatusChanges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Document = table.Column<byte[]>(type: "bytea", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    WorkerName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StatusChangeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_EVotingStatusChanges_StatusChangeId",
                        column: x => x.StatusChangeId,
                        principalTable: "EVotingStatusChanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ahvn13 = table.Column<long>(type: "bigint", nullable: false),
                    AllowedToVote = table.Column<bool>(type: "boolean", nullable: false),
                    MunicipalityBfs = table.Column<short>(type: "smallint", nullable: false),
                    Nationality = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Sex = table.Column<int>(type: "integer", nullable: false),
                    OfficialName = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    Address_Street = table.Column<string>(type: "text", nullable: false),
                    Address_HouseNumber = table.Column<string>(type: "text", nullable: false),
                    Address_HouseNumberAddition = table.Column<string>(type: "text", nullable: false),
                    Address_Town = table.Column<string>(type: "text", nullable: false),
                    Address_ZipCode = table.Column<string>(type: "text", nullable: false),
                    Address_Canton = table.Column<string>(type: "text", nullable: false),
                    StatusChangeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Persons_EVotingStatusChanges_StatusChangeId",
                        column: x => x.StatusChangeId,
                        principalTable: "EVotingStatusChanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_StatusChangeId",
                table: "Documents",
                column: "StatusChangeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EVotingStatusChanges_ContextId",
                table: "EVotingStatusChanges",
                column: "ContextId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Ahvn13",
                table: "Persons",
                column: "Ahvn13",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_StatusChangeId",
                table: "Persons",
                column: "StatusChangeId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropTable(
                name: "EVotingStatusChanges");
        }
    }
}
