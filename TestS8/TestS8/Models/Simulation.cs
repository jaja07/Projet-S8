using System.ComponentModel.DataAnnotations;

namespace TestS8.Models
{
    public class Simulation
    {
        [Key]
        public int IdSimul { get; set; }
        public DateTime Date { get; set; }
        public DateTime Duree_simul { get; set; }
        public int UtilisateurId { get; set; } // Clé étrangère
        public Utilisateur Utilisateur { get; set; } // Association avec Utilisateur

        public ICollection<Modele> Modeles { get; set; }

        public Simulation()
        {
            Modeles = new List<Modele>();
        }

    }
}
