using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smurfs.Persistence.EFCore.AppData.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class GenderAndInfected : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Birthday",
                table: "Smurfs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Smurfs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Infected",
                table: "Smurfs",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Birthday",
                table: "Smurfs");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Smurfs");

            migrationBuilder.DropColumn(
                name: "Infected",
                table: "Smurfs");
        }
    }
}
