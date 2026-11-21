using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smurfs.Persistence.EFCore.AppData.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class ChangedAgeToHealth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Age",
                table: "Smurfs",
                newName: "Health");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Health",
                table: "Smurfs",
                newName: "Age");
        }
    }
}
