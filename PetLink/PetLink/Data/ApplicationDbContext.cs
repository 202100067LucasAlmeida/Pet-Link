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

        public DbSet<User> Users { get; set; }
        public DbSet<AnimalListing> AnimalListings { get; set; }
        public DbSet<AnimalPhoto> AnimalPhotos { get; set; }
        public DbSet<FavoritePet> FavoritePets { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Petsitter> Petsitters { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Review> Reviews { get; set; }

        public DbSet<ListingsNotification> ListingsNotifications { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ListingsNotification>().ToTable("ListingsNotifications");

            modelBuilder.Entity<AnimalListing>()
                .HasOne(a => a.Tutor)
                .WithMany(u => u.Listings)
                .HasForeignKey(a => a.TutorId)
                .OnDelete(DeleteBehavior.Cascade); // Se o user for apagado, os anúncios dele também são

            modelBuilder.Entity<ListingsNotification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ListingsNotification>()
                .HasOne(n => n.AnimalListing)
                .WithMany()
                .HasForeignKey(n => n.AnimalListingId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FavoritePet>()
                .HasOne(f => f.User)
                .WithMany(u => u.FavoritePets)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict para não apagar favoritos se o user for apagado

            modelBuilder.Entity<FavoritePet>()
                .HasOne(f => f.AnimalListing)
                .WithMany(a => a.Favorites)
                .HasForeignKey(f => f.AnimalListingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice único para evitar favoritos duplicados
            modelBuilder.Entity<FavoritePet>()
                .HasIndex(f => new { f.UserId, f.AnimalListingId })
                .IsUnique();

            // Configuração para as Mensagens
            modelBuilder.Entity<Message>(entity =>
            {
                // Define quem envia
                entity.HasOne(m => m.Sender)
                      .WithMany(u => u.SentMessages)
                      .HasForeignKey(m => m.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Define quem recebe
                entity.HasOne(m => m.Receiver)
                      .WithMany(u => u.ReceivedMessages)
                      .HasForeignKey(m => m.ReceiverId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Application>()
                .HasOne(a => a.User)
                .WithMany(u => u.Applications)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Application>()
                .HasOne(a => a.AnimalListing)
                .WithMany()
                .HasForeignKey(a => a.AnimalListingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
           .HasOne(r => r.Reviewer)
           .WithMany(u => u.ReviewsGiven)
           .HasForeignKey(r => r.ReviewerId)
           .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
            .HasOne(r => r.Reviewed)
            .WithMany(u => u.ReviewsReceived)
            .HasForeignKey(r => r.ReviewedId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
            .HasOne(r => r.AnimalListing)
            .WithMany()
            .HasForeignKey(r => r.AnimalListingId)
            .OnDelete(DeleteBehavior.Restrict);

            // Índice para evitar múltiplas avaliações para o mesmo animal
            modelBuilder.Entity<Review>()
            .HasIndex(r => new { r.ReviewerId, r.AnimalListingId })
            .IsUnique();



        }
    }
}