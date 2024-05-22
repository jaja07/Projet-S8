namespace TestS8.Models
{
    public class Parametres
    {
        public int ParametresID { get; set; }
        public string? Nom { get; set; }
        public string? Valeur { get; set; }
        public int ModeleID { get; set; }
        public Modele Modeles { get; set; } // Association avec Modele
    }
}
