using System.ComponentModel.DataAnnotations;

namespace TestS8.Models
{
    public class Simulation
    {
        [Key]
        public int SimulationID { get; set; }
        public DateTime Date { get; set; }
        public string ConnexionID { get; set; }

        //Propriétés de navigation
        public Connexion Connexion { get; set; } 
        public ICollection<Modele> Modeles { get; set; }
    }
}
