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
            name: "Pet",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Pet", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Smurfs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false),
                Age = table.Column<int>(type: "INTEGER", nullable: false),
                PetId = table.Column<Guid>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Smurfs", x => x.Id);
                table.ForeignKey(
                    name: "FK_Smurfs_Pet_PetId",
                    column: x => x.PetId,
                    principalTable: "Pet",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_Smurfs_PetId",
            table: "Smurfs",
            column: "PetId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Smurfs");

        migrationBuilder.DropTable(
            name: "Pet");
    }
}
