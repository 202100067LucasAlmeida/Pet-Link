using Microsoft.EntityFrameworkCore;
using PetLink.Models;
using PetLink.Models.Enums;

namespace PetLink.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Verifica se a base de dados já tem algum Utilizador
                if (context.Users.Any())
                {
                    return;   // A base de dados já foi preenchida ("seeded")
                }

                // 1. Criar Utilizadores
                var admin = new User
                {
                    Name = "Administrador PetLink",
                    Email = "admin@petlink.pt",
                    PasswordHash = "admin123", // Nota: Num projeto real, isto deve ser encriptado!
                    Role = UserRole.Admin,
                    IsVerified = true
                };

                var associacao = new User
                {
                    Name = "Abrigo Patinhas Felizes",
                    Email = "abrigo@patinhas.pt",
                    PasswordHash = "abrigo123",
                    Role = UserRole.Shelter,
                    IsVerified = true
                };

                var particular = new User
                {
                    Name = "João Silva",
                    Email = "joao.silva@email.com",
                    PasswordHash = "joao123",
                    Role = UserRole.User,
                    IsVerified = false
                };

                // Adicionar os utilizadores ao contexto e gravar para gerar os IDs
                context.Users.AddRange(admin, associacao, particular);
                context.SaveChanges();

                // 2. Criar Anúncios de Animais (atribuindo o TutorId)
                var listings = new AnimalListing[]
                {
                    new AnimalListing
                    {
                        Name = "Bobi",
                        Species = Species.Dog,
                        Location = "Setúbal",
                        AgeMonths = 24,
                        Description = "Cão muito amigável e energético. Adora correr no parque.",
                        IsVaccinated = true,
                        IsDewormed = true,
                        IsSterilized = true,
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-10),
                        TutorId = associacao.Id // Associado à Associação
                    },
                    new AnimalListing
                    {
                        Name = "Luna",
                        Species = Species.Cat,
                        Location = "Lisboa",
                        AgeMonths = 6,
                        Description = "Gatinha muito meiga, ideal para apartamento. Muito calma.",
                        IsVaccinated = true,
                        IsDewormed = true,
                        IsSterilized = false, // Ainda é muito nova
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-2),
                        TutorId = associacao.Id // Associado à Associação
                    },
                    new AnimalListing
                    {
                        Name = "Rex",
                        Species = Species.Dog,
                        Location = "Porto",
                        AgeMonths = 48,
                        Description = "Cão de guarda porte grande. Precisa de espaço exterior.",
                        IsVaccinated = true,
                        IsDewormed = false,
                        IsSterilized = false,
                        Status = ListingStatus.Pendent, // Aguarda aprovação do Admin
                        CreatedAt = DateTime.Now,
                        TutorId = particular.Id // Associado ao utilizador particular
                    }
                };

                context.AnimalListings.AddRange(listings);
                context.SaveChanges();
            }
        }
    }
}