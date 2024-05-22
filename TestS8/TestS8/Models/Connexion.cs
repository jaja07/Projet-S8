namespace TestS8.Models
{
    public class Connexion
    {
        public string ConnexionID { get; set; }
        public DateTime DateConnexion { get; set; }
        public string Email { get; set; }
        public string? UtilisateurID { get; set; }
        public Utilisateur Utilisateur { get; set; }
        public ICollection<Simulation> Simulations { get; set; }
    }
}
