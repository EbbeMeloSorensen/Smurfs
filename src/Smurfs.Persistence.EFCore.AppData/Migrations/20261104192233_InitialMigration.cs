using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smurfs.Persistence.EFCore.AppData.Migrations;

/// <inheritdoc />
public partial class InitialMigration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Pets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Pets", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Villages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Villages", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Smurfs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false),
                Age = table.Column<int>(type: "INTEGER", nullable: false),
                PetId = table.Column<Guid>(type: "TEXT", nullable: true),
                VillageId = table.Column<Guid>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Smurfs", x => x.Id);
                table.ForeignKey(
                    name: "FK_Smurfs_Pets_PetId",
                    column: x => x.PetId,
                    principalTable: "Pets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Smurfs_Villages_VillageId",
                    column: x => x.VillageId,
                    principalTable: "Villages",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_Smurfs_PetId",
            table: "Smurfs",
            column: "PetId");

        migrationBuilder.CreateIndex(
            name: "IX_Smurfs_VillageId",
            table: "Smurfs",
            column: "VillageId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Smurfs");

        migrationBuilder.DropTable(
            name: "Pets");

        migrationBuilder.DropTable(
            name: "Villages");
    }
}
