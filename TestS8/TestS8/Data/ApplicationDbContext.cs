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
        public DbSet<TestS8.Models.Modele>? Modele { get; set; }
        public DbSet<TestS8.Models.Plot>? Plot { get; set; }
        public DbSet<TestS8.Models.Simulation>? Simulation { get; set; }
    }
}
