using System.ComponentModel.DataAnnotations;

namespace TestS8.Models
{
    public class Simulation
    {
        [Key]
        public int SimulationID { get; set; }
        public DateTime Date { get; set; }
        public string UtilisateurID { get; set; }
        public Utilisateur Utilisateur { get; set; } // Association avec Utilisateur
        public ICollection<Modele> Modeles { get; set; }
    }
}
