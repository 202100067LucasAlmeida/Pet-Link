using Microsoft.EntityFrameworkCore;
using PetLink.Models;
using PetLink.Models.Enums;
using System;
using System.Linq;

namespace PetLink.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Verifica se a base de dados já tem algum Utilizador.
                // Se sim, assume que o Seed já foi feito e sai do método.
                if (context.Users.Any())
                {
                    return;
                }

                // 1. Cria utilizadores (Admin, Shelter, Particular)
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

                // 2. Cria utilizadores base para os Pet Sitters
                var sitter1 = new User { Name = "Sarah Jenkins", Email = "sarah@petlink.com", PasswordHash = "...", Role = UserRole.PetSitter, IsVerified = true };
                var sitter2 = new User { Name = "Marcus Chen", Email = "marcus@petlink.com", PasswordHash = "...", Role = UserRole.PetSitter, IsVerified = true };
                var sitter3 = new User { Name = "Elena Smith", Email = "elena@petlink.com", PasswordHash = "...", Role = UserRole.PetSitter, IsVerified = true };

                // Adiciona e guarda todos os utilizadores para que a BD gere os IDs (necessários para as chaves estrangeiras)
                context.Users.AddRange(admin, shelter, particular, sitter1, sitter2, sitter3);
                context.SaveChanges();

                // 3. Cria os detalhes/perfis dos Pet Sitters ligados aos IDs gerados
                var petSitters = new Petsitter[]
                {
                    new Petsitter
                    {
                        UserId = sitter1.Id,
                        Age = 31,
                        HourlyRate = 3.00m,
                        Rating = 4.9,
                        LocationZone = "Downtown",
                        DistanceKm = 2.4,
                        Bio = "Professional dog walker with 5+ years of experience. I specialize in...",
                        SpecialtyTags = "SMALL DOGS,PUPPIES"
                    },
                    new Petsitter
                    {
                        UserId = sitter2.Id,
                        Age = 42,
                        HourlyRate = 7.00m,
                        Rating = 5.0,
                        LocationZone = "Westside",
                        DistanceKm = 0.8,
                        Bio = "Cat behavior specialist. I offer luxury boarding and house sitting...",
                        SpecialtyTags = "CATS ONLY,MEDICAL CARE"
                    },
                    new Petsitter
                    {
                        UserId = sitter3.Id,
                        Age = 21,
                        HourlyRate = 5.00m,
                        Rating = 4.8,
                        LocationZone = "North Park",
                        DistanceKm = 3.2,
                        Bio = "Reliable student available for the weekends. Specialised in exotic...",
                        SpecialtyTags = "BIRDS,EXOTIC"
                    }
                };

                // Adiciona os Pet Sitters (usando o AddRange genérico do Contexto)
                context.AddRange(petSitters);

                // 4. Cria anúncios de animais associados aos Shelters/Particulares
                var listings = new AnimalListing[]
                {
                    new AnimalListing
                    {
                        Name = "Cooper", Species = Species.Dog, Location = "Lisbon, Portugal", AgeMonths = 4,
                        Description = "Cooper is the personification of joy. Found as a stray, he hasn't let his past dampen his spirit. He loves tennis balls, belly rubs, and is excellent with children. He's looking for a family that can match his energy and love.",
                        IsVaccinated = true, IsDewormed = true, IsSterilized = true, Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-1), TutorId = shelter.Id
                    },
                    new AnimalListing
                    {
                        Name = "Luna", Species = Species.Cat, Location = "Setúbal, Portugal", AgeMonths = 24,
                        Description = "Sweet American Shorthair cat. Very calm and great for apartments. She loves to nap in sunny spots.",
                        IsVaccinated = true, IsDewormed = true, IsSterilized = true, Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-2), TutorId = shelter.Id
                    },
                    new AnimalListing
                    {
                        Name = "Rio", Species = Species.Bird, Location = "Leiria, Portugal", AgeMonths = 12,
                        Description = "Colorful Macaw parrot. Very talkative, smart, and loves interaction. Needs an experienced bird owner.",
                        IsVaccinated = false, IsDewormed = false, IsSterilized = false, Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-3), TutorId = particular.Id
                    },
                    new AnimalListing
                    {
                        Name = "Shadow", Species = Species.Dog, Location = "Algarve, Portugal", AgeMonths = 60,
                        Description = "Faithful mutt looking for a quiet home. Shadow is a very loyal companion who enjoys long, peaceful walks.",
                        IsVaccinated = true, IsDewormed = true, IsSterilized = true, Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-4), TutorId = shelter.Id
                    },
                    new AnimalListing
                    {
                        Name = "Rex", Species = Species.Dog, Location = "Porto, Portugal", AgeMonths = 36,
                        Description = "Friendly French Bulldog. Perfect for apartment living and very sociable with other dogs.",
                        IsVaccinated = true, IsDewormed = true, IsSterilized = true, Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-5), TutorId = shelter.Id
                    },
                    new AnimalListing
                    {
                        Name = "Max", Species = Species.Dog, Location = "Coimbra, Portugal", AgeMonths = 96,
                        Description = "Senior German Shepherd. Very well trained, protective, and calm. Looking for a comfortable retirement home.",
                        IsVaccinated = true, IsDewormed = true, IsSterilized = true, Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-6), TutorId = particular.Id
                    },
                    new AnimalListing
                    {
                        Name = "Daisy", Species = Species.Dog, Location = "Braga, Portugal", AgeMonths = 48,
                        Description = "Active Beagle who loves the outdoors. Has a great nose and needs plenty of exercise.",
                        IsVaccinated = true, IsDewormed = true, IsSterilized = false, Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-7), TutorId = shelter.Id
                    },
                    new AnimalListing
                    {
                        Name = "Misty", Species = Species.Cat, Location = "Aveiro, Portugal", AgeMonths = 60,
                        Description = "Calm Domestic Longhair. Very independent but enjoys evening cuddles on the sofa.",
                        IsVaccinated = true, IsDewormed = true, IsSterilized = true, Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-8), TutorId = particular.Id
                    }
                };

                context.AnimalListings.AddRange(listings);

                // Guarda as alterações finais das Listings e dos PetSitters
                context.SaveChanges();
            }
        }
    }
}