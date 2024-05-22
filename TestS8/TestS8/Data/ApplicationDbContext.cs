using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TestS8.Models;

namespace TestS8.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<TestS8.Models.Utilisateur>? Utilisateur { get; set; }
        public DbSet<TestS8.Models.Connexion>? Connexion { get; set; }
        public DbSet<TestS8.Models.Modele>? Modele { get; set; }
        public DbSet<TestS8.Models.Parametres>? Parametres { get; set; }
        public DbSet<TestS8.Models.Simulation>? Simulation { get; set; }
      
    }
}
