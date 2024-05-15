using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestS8.Models
{
    public class Modele
    {
        [Key]
        public int IdModele { get; set; }
        public int Accuracy { get; set; }
        public string Hyperparametre { get; set; }
        public string Nom { get; set; }
        public int Accuracy_cross { get; set; }
        [ForeignKey("Simulation")]
        public int SimulationId { get; set; } // Clé étrangère
        public Simulation Simulation { get; set; } // Association avec Simulation
        public ICollection<Plot> Plots { get; set; }

        public Modele()
        {
            Plots = new List<Plot>();
        }

    }
}
