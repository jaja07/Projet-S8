using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestS8.Data.Migrations
{
    public partial class UpdateSimulation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duree_simul",
                table: "Simulation");

            migrationBuilder.AddColumn<float>(
                name: "Duree_simul",
                table: "Modele",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duree_simul",
                table: "Modele");

            migrationBuilder.AddColumn<DateTime>(
                name: "Duree_simul",
                table: "Simulation",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
