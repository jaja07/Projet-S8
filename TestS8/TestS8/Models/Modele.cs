using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestS8.Models
{
    public class Modele
    {
        [Key]
        public int ModeleID { get; set; }
        public string Nom { get; set; }
        public float Accuracy { get; set; }
        public float Accuracy_cross { get; set; }
        public string Hyperparametre { get; set; }
        
        public float Duree_simul { get; set; }
        
        public int SimulationID { get; set; }
        public Simulation Simulation { get; set; } // Association avec Simulation
        public ICollection<Plot> Plots { get; set; }


    }
}
