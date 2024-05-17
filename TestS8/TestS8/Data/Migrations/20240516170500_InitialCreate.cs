using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestS8.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Utilisateur",
                columns: table => new
                {
                    UtilisateurID = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateur", x => x.UtilisateurID);
                });

            migrationBuilder.CreateTable(
                name: "Simulation",
                columns: table => new
                {
                    SimulationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Duree_simul = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UtilisateurID = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Simulation", x => x.SimulationID);
                    table.ForeignKey(
                        name: "FK_Simulation_Utilisateur_UtilisateurID",
                        column: x => x.UtilisateurID,
                        principalTable: "Utilisateur",
                        principalColumn: "UtilisateurID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Modele",
                columns: table => new
                {
                    ModeleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Accuracy = table.Column<int>(type: "int", nullable: false),
                    Hyperparametre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Accuracy_cross = table.Column<int>(type: "int", nullable: false),
                    SimulationID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modele", x => x.ModeleID);
                    table.ForeignKey(
                        name: "FK_Modele_Simulation_SimulationID",
                        column: x => x.SimulationID,
                        principalTable: "Simulation",
                        principalColumn: "SimulationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Plot",
                columns: table => new
                {
                    PlotID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Chemin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModeleID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plot", x => x.PlotID);
                    table.ForeignKey(
                        name: "FK_Plot_Modele_ModeleID",
                        column: x => x.ModeleID,
                        principalTable: "Modele",
                        principalColumn: "ModeleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Modele_SimulationID",
                table: "Modele",
                column: "SimulationID");

            migrationBuilder.CreateIndex(
                name: "IX_Plot_ModeleID",
                table: "Plot",
                column: "ModeleID");

            migrationBuilder.CreateIndex(
                name: "IX_Simulation_UtilisateurID",
                table: "Simulation",
                column: "UtilisateurID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Plot");

            migrationBuilder.DropTable(
                name: "Modele");

            migrationBuilder.DropTable(
                name: "Simulation");

            migrationBuilder.DropTable(
                name: "Utilisateur");
        }
    }
}
