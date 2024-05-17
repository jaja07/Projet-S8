namespace TestS8.Models
{
    public class Utilisateur 
    {
        public string UtilisateurID { get; set; }
        public ICollection<Simulation> Simulation { get; set; } 
    }
}
