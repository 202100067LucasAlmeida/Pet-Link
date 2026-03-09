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

                // 1. Criar Utilizadores (Atualizado com o nome do abrigo do Mockup)
                var admin = new User
                {
                    Name = "Administrador PetLink",
                    Email = "admin@petlink.pt",
                    PasswordHash = "admin123",
                    Role = UserRole.Admin,
                    IsVerified = true
                };

                var shelter = new User
                {
                    Name = "Sunny Paws Shelter",
                    Email = "hello@sunnypaws.com",
                    PasswordHash = "shelter123",
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

                context.Users.AddRange(admin, shelter, particular);
                context.SaveChanges();

                // 2. Criar Anúncios de Animais (Baseado nos Mockups)
                var listings = new AnimalListing[]
                {
                    new AnimalListing
                    {
                        Name = "Cooper",
                        Species = Species.Dog,
                        Location = "Lisbon, Portugal",
                        AgeMonths = 4,
                        Description = "Cooper is the personification of joy. Found as a stray, he hasn't let his past dampen his spirit. He loves tennis balls, belly rubs, and is excellent with children. He's looking for a family that can match his energy and love.",
                        IsVaccinated = true,
                        IsDewormed = true,
                        IsSterilized = true,
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-1),
                        TutorId = shelter.Id
                    },
                    new AnimalListing
                    {
                        Name = "Luna",
                        Species = Species.Cat,
                        Location = "Setúbal, Portugal",
                        AgeMonths = 24, // 2 years
                        Description = "Sweet American Shorthair cat. Very calm and great for apartments. She loves to nap in sunny spots.",
                        IsVaccinated = true,
                        IsDewormed = true,
                        IsSterilized = true,
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-2),
                        TutorId = shelter.Id
                    },
                    new AnimalListing
                    {
                        Name = "Rio",
                        Species = Species.Bird,
                        Location = "Leiria, Portugal",
                        AgeMonths = 12, // 1 year
                        Description = "Colorful Macaw parrot. Very talkative, smart, and loves interaction. Needs an experienced bird owner.",
                        IsVaccinated = false,
                        IsDewormed = false,
                        IsSterilized = false,
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-3),
                        TutorId = particular.Id
                    },
                    new AnimalListing
                    {
                        Name = "Shadow",
                        Species = Species.Dog,
                        Location = "Algarve, Portugal",
                        AgeMonths = 60, // 5 years
                        Description = "Faithful mutt looking for a quiet home. Shadow is a very loyal companion who enjoys long, peaceful walks.",
                        IsVaccinated = true,
                        IsDewormed = true,
                        IsSterilized = true,
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-4),
                        TutorId = shelter.Id
                    },
                    new AnimalListing
                    {
                        Name = "Rex",
                        Species = Species.Dog,
                        Location = "Porto, Portugal",
                        AgeMonths = 36, // 3 years
                        Description = "Friendly French Bulldog. Perfect for apartment living and very sociable with other dogs.",
                        IsVaccinated = true,
                        IsDewormed = true,
                        IsSterilized = true,
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-5),
                        TutorId = shelter.Id
                    },
                    new AnimalListing
                    {
                        Name = "Max",
                        Species = Species.Dog,
                        Location = "Coimbra, Portugal",
                        AgeMonths = 96, // 8 years (Senior)
                        Description = "Senior German Shepherd. Very well trained, protective, and calm. Looking for a comfortable retirement home.",
                        IsVaccinated = true,
                        IsDewormed = true,
                        IsSterilized = true,
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-6),
                        TutorId = particular.Id
                    },
                    new AnimalListing
                    {
                        Name = "Daisy",
                        Species = Species.Dog,
                        Location = "Braga, Portugal",
                        AgeMonths = 48, // 4 years
                        Description = "Active Beagle who loves the outdoors. Has a great nose and needs plenty of exercise.",
                        IsVaccinated = true,
                        IsDewormed = true,
                        IsSterilized = false,
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-7),
                        TutorId = shelter.Id
                    },
                    new AnimalListing
                    {
                        Name = "Misty",
                        Species = Species.Cat,
                        Location = "Aveiro, Portugal",
                        AgeMonths = 60, // 5 years
                        Description = "Calm Domestic Longhair. Very independent but enjoys evening cuddles on the sofa.",
                        IsVaccinated = true,
                        IsDewormed = true,
                        IsSterilized = true,
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-8),
                        TutorId = particular.Id
                    }
                };

                context.AnimalListings.AddRange(listings);
                context.SaveChanges();
            }
        }
    }
}