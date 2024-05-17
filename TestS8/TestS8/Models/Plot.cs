using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestS8.Models
{
    public class Plot
    {
        [Key]
        public int PlotID { get; set; }
        public string Chemin { get; set; }
        public string Nom {  get; set; }
        public int ModeleID { get; set; }
        public Modele Modele { get; set; } // Association avec Modele
    }
}
