using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smurfs.Persistence.EFCore.AppData.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddedCreatedTimestampToSmurf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "Smurfs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "Smurfs");
        }
    }
}
