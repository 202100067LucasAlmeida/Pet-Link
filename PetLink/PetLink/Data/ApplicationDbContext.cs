using Microsoft.EntityFrameworkCore;
using PetLink.Models;

namespace PetLink.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }

        // Estas propriedades DbSet vão transformar-se nas tabelas do SQL Server
        public DbSet<User> Users { get; set; }
        public DbSet<AnimalListing> AnimalListings { get; set; }
        // public DbSet<Message> Messages { get; set; }
        // public DbSet<Favorite> Favorites { get; set; }

        // Opcional: Usar o OnModelCreating para regras de base de dados mais complexas
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Exemplo: Um User tem muitos AnimalListings
            modelBuilder.Entity<AnimalListing>()
                .HasOne(a => a.Tutor)
                .WithMany(u => u.Listings)
                .HasForeignKey(a => a.TutorId)
                .OnDelete(DeleteBehavior.Cascade); // Se o user for apagado, os anúncios dele também são
        }
    }
}