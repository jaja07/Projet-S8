using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestS8.Models
{
    public class Plot
    {
        [Key]
        public int IdPlot { get; set; }
        public string Chemin { get; set; }
        [ForeignKey("Modele")]
        public int ModeleId { get; set; } // Clé étrangère
        public Modele Modele { get; set; } // Association avec Modele
    }
}
