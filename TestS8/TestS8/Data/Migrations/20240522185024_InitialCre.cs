using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestS8.Data.Migrations
{
    public partial class InitialCre : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Utilisateur",
                columns: table => new
                {
                    UtilisateurID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Mail = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateur", x => x.UtilisateurID);
                });

            migrationBuilder.CreateTable(
                name: "Connexion",
                columns: table => new
                {
                    ConnexionID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateConnexion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UtilisateurID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connexion", x => x.ConnexionID);
                    table.ForeignKey(
                        name: "FK_Connexion_Utilisateur_UtilisateurID",
                        column: x => x.UtilisateurID,
                        principalTable: "Utilisateur",
                        principalColumn: "UtilisateurID");
                });

            migrationBuilder.CreateTable(
                name: "Simulation",
                columns: table => new
                {
                    SimulationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConnexionID = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Simulation", x => x.SimulationID);
                    table.ForeignKey(
                        name: "FK_Simulation_Connexion_ConnexionID",
                        column: x => x.ConnexionID,
                        principalTable: "Connexion",
                        principalColumn: "ConnexionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Modele",
                columns: table => new
                {
                    ModeleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Accuracy = table.Column<float>(type: "real", nullable: false),
                    Accuracy_cross = table.Column<float>(type: "real", nullable: false),
                    Hyperparametre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Duree_simul = table.Column<float>(type: "real", nullable: false),
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
                name: "Parametres",
                columns: table => new
                {
                    ParametresID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Valeur = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModeleID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parametres", x => x.ParametresID);
                    table.ForeignKey(
                        name: "FK_Parametres_Modele_ModeleID",
                        column: x => x.ModeleID,
                        principalTable: "Modele",
                        principalColumn: "ModeleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Connexion_UtilisateurID",
                table: "Connexion",
                column: "UtilisateurID");

            migrationBuilder.CreateIndex(
                name: "IX_Modele_SimulationID",
                table: "Modele",
                column: "SimulationID");

            migrationBuilder.CreateIndex(
                name: "IX_Parametres_ModeleID",
                table: "Parametres",
                column: "ModeleID");

            migrationBuilder.CreateIndex(
                name: "IX_Simulation_ConnexionID",
                table: "Simulation",
                column: "ConnexionID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Parametres");

            migrationBuilder.DropTable(
                name: "Modele");

            migrationBuilder.DropTable(
                name: "Simulation");

            migrationBuilder.DropTable(
                name: "Connexion");

            migrationBuilder.DropTable(
                name: "Utilisateur");
        }
    }
}
