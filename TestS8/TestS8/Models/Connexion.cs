using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestS8.Models
{
    public class Connexion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConnexionID { get; set; }
        public DateTime DateConnexion { get; set; }
        public string Email { get; set; }
        public string? UtilisateurID { get; set; }
        public Utilisateur Utilisateur { get; set; }
        public ICollection<Simulation> Simulations { get; set; }
    }
}
