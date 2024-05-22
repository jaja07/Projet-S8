namespace TestS8.Models
{
    public class Utilisateur 
    {
        public string? UtilisateurID { get; set; }
        public string? Mail {  get; set; }
        public ICollection<Connexion> Connexions { get; set; } 
    }
}
