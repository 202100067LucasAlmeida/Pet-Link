using Microsoft.EntityFrameworkCore;
using PetLink.Models;
using PetLink.Models.Enums;
using System;
using System.Linq;
using System.Collections.Generic;

namespace PetLink.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Verifica se a base de dados já tem dados seed. Se sim, sai do método.
                if (context.Users.Any())
                {
                    return;
                }

                // 1. Cria utilizadores completos
                var admin = new User
                {
                    Name = "Administrador PetLink",
                    Email = "admin@petlink.pt",
                    PasswordHash = UserHashHelpers.HashPassword("admin123"),
                    Role = UserRole.Admin,
                    IsVerified = true,
                    Phone = "+351 912 345 678",
                    City = null,
                    Bio = "Admin system user",
                    ProfilePicture = "/images/logo.png",
                    CreatedAt = DateTime.Now.AddDays(-365)
                };

                admin.UpdateCoordinates(null, null);

                var shelter = new User
                {
                    Name = "Sunny Paws Shelter",
                    Email = "hello@sunnypaws.com",
                    PasswordHash = UserHashHelpers.HashPassword("shelter123"),
                    Role = UserRole.Shelter,
                    IsVerified = true,
                    Phone = "+351 234 567 890",
                    City = null,
                    Bio = "Animal shelter specializing in rescue dogs and cats",
                    ProfilePicture = "/images/avatars/sunnypaws.jpg",
                    CreatedAt = DateTime.Now.AddDays(-180)
                };

                shelter.UpdateCoordinates(null, null);

                var particular = new User
                {
                    Name = "João Silva",
                    Email = "joao.silva@email.com",
                    PasswordHash = UserHashHelpers.HashPassword("joao123"),
                    Role = UserRole.User,
                    IsVerified = false,
                    Phone = "+351 912 345 679",
                    City = "Setubal",
                    Bio = "Pet lover looking for adoption",
                    ProfilePicture = "/images/avatars/joaosilva.jpg",
                    CreatedAt = DateTime.Now.AddDays(-30)
                };

                particular.UpdateCoordinates("38.548168", "-8.901205");

                // Pet Sitters completos
                var sitter1 = new User
                {
                    Name = "Sarah Jenkins",
                    Email = "sarah@petlink.com",
                    PasswordHash = UserHashHelpers.HashPassword("sarah123"),
                    Role = UserRole.PetSitter,
                    IsVerified = true,
                    Phone = "+351 923 456 789",
                    City = "Setúbal",
                    Bio = "Experienced dog walker",
                    ProfilePicture = "/images/avatars/sarahjenkins.jpg",
                    CreatedAt = DateTime.Now.AddDays(-90)
                };

                sitter1.UpdateCoordinates("38.548168", "-8.901205");

                var sitter2 = new User
                {
                    Name = "Marcus Chen",
                    Email = "marcus@petlink.com",
                    PasswordHash = UserHashHelpers.HashPassword("marcus123"),
                    Role = UserRole.PetSitter,
                    IsVerified = true,
                    Phone = "+351 934 567 890",
                    City = null,
                    Bio = "Cat specialist",
                    ProfilePicture = "/images/avatars/marcuschen.jpg",
                    CreatedAt = DateTime.Now.AddDays(-120)
                };

                sitter2.UpdateCoordinates(null, null);

                var sitter3 = new User
                {
                    Name = "Elena Smith",
                    Email = "elena@petlink.com",
                    PasswordHash = UserHashHelpers.HashPassword("elena123"),
                    Role = UserRole.PetSitter,
                    IsVerified = true,
                    Phone = "+351 945 678 901",
                    City = null,
                    Bio = "Exotic pets expert",
                    ProfilePicture = "/images/avatars/elenasmith.jpg",
                    CreatedAt = DateTime.Now.AddDays(-60)
                };

                sitter3.UpdateCoordinates(null, null);

                // Salva users primeiro para gerar IDs
                context.Users.AddRange(admin, shelter, particular, sitter1, sitter2, sitter3);
                context.SaveChanges();

                // 2. PetSitters completos
                context.Petsitters.AddRange(
                    new Petsitter
                    {
                        UserId = sitter1.Id,
                        Age = 31,
                        HourlyRate = 3.00m,
                        Rating = 4.9,
                        LocationZone = "Downtown",
                        DistanceKm = 2.4,
                        Bio = "Professional dog walker with 5+ years experience. Specialized in small dogs and puppies.",
                        SpecialtyTags = "SMALL DOGS,PUPPIES,DAILY WALKS"
                    },
                    new Petsitter
                    {
                        UserId = sitter2.Id,
                        Age = 42,
                        HourlyRate = 7.00m,
                        Rating = 5.0,
                        LocationZone = "Westside",
                        DistanceKm = 0.8,
                        Bio = "Cat behavior specialist with veterinary background. Luxury boarding available.",
                        SpecialtyTags = "CATS ONLY,MEDICAL CARE,BOARDING"
                    },
                    new Petsitter
                    {
                        UserId = sitter3.Id,
                        Age = 21,
                        HourlyRate = 5.00m,
                        Rating = 4.8,
                        LocationZone = "North Park",
                        DistanceKm = 3.2,
                        Bio = "University student experienced with exotic pets and birds. Weekend availability.",
                        SpecialtyTags = "BIRDS,EXOTIC,REPTILES"
                    }
                );

                // 3. Animal Listings completos (8 animais)
                context.AnimalListings.AddRange(
                    new AnimalListing
                    {
                        Name = "Cooper",
                        Species = Species.Dog,
                        Age = Age.Puppy,
                        AgeMonths = 4,
                        Location = "Lisbon",
                        Description = "Energetic puppy loves playing!",
                        IsVaccinated = true,
                        IsDewormed = true,
                        IsSterilized = true,
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-1),
                        TutorId = shelter.Id,
                        ImageUrl = "/images/animals/cooper.jpg"
                    },
                    new AnimalListing
                    {
                        Name = "Luna",
                        Species = Species.Cat,
                        Age = Age.Adult,
                        AgeMonths = 24,
                        Location = "Setúbal",
                        Description = "Calm apartment cat",
                        IsVaccinated = true,
                        IsDewormed = true,
                        IsSterilized = true,
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-2),
                        TutorId = shelter.Id,
                        ImageUrl = "/images/animals/luna.jpg"
                    },
                    new AnimalListing
                    {
                        Name = "Rio",
                        Species = Species.Bird,
                        Age = Age.Puppy,
                        AgeMonths = 12,
                        Location = "Leiria",
                        Description = "Talkative macaw parrot",
                        IsVaccinated = false,
                        IsDewormed = false,
                        IsSterilized = false,
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-3),
                        TutorId = particular.Id,
                        ImageUrl = "/images/animals/rio.jpg"
                    },
                    new AnimalListing
                    {
                        Name = "Shadow",
                        Species = Species.Dog,
                        Age = Age.Adult,
                        AgeMonths = 60,
                        Location = "Algarve",
                        Description = "Loyal companion for quiet home",
                        IsVaccinated = true,
                        IsDewormed = true,
                        IsSterilized = true,
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-4),
                        TutorId = shelter.Id,
                        ImageUrl = "/images/animals/shadow.jpg"
                    },
                    new AnimalListing
                    {
                        Name = "Rex",
                        Species = Species.Dog,
                        Age = Age.Adult,
                        AgeMonths = 36,
                        Location = "Porto",
                        Description = "Friendly French Bulldog",
                        IsVaccinated = true,
                        IsDewormed = true,
                        IsSterilized = true,
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-5),
                        TutorId = shelter.Id,
                        ImageUrl = "/images/animals/rex.jpg"
                    },
                    new AnimalListing
                    {
                        Name = "Max",
                        Species = Species.Dog,
                        Age = Age.Senior,
                        AgeMonths = 96,
                        Location = "Coimbra",
                        Description = "Trained senior German Shepherd",
                        IsVaccinated = true,
                        IsDewormed = true,
                        IsSterilized = true,
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-6),
                        TutorId = particular.Id,
                        ImageUrl = "/images/animals/max.jpg"
                    },
                    new AnimalListing
                    {
                        Name = "Daisy",
                        Species = Species.Dog,
                        Age = Age.Adult,
                        AgeMonths = 48,
                        Location = "Braga",
                        Description = "Active Beagle needs exercise",
                        IsVaccinated = true,
                        IsDewormed = true,
                        IsSterilized = false,
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-7),
                        TutorId = shelter.Id,
                        ImageUrl = "/images/animals/daisy.jpg"
                    },
                    new AnimalListing
                    {
                        Name = "Misty",
                        Species = Species.Cat,
                        Age = Age.Adult,
                        AgeMonths = 60,
                        Location = "Aveiro",
                        Description = "Independent longhair cat",
                        IsVaccinated = true,
                        IsDewormed = true,
                        IsSterilized = true,
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-8),
                        TutorId = particular.Id,
                        ImageUrl = "/images/animals/misty.jpg"
                    }
                );

                // 4. Sample FavoritePets
                context.FavoritePets.AddRange(
                    new FavoritePet
                    {
                        UserId = particular.Id,
                        AnimalListingId = 1,
                        CreatedAt = DateTime.Now.AddDays(-2)
                    }, // João favorited Cooper
                    new FavoritePet
                    {
                        UserId = particular.Id,
                        AnimalListingId = 4,
                        CreatedAt = DateTime.Now.AddDays(-1)
                    }  // João favorited Shadow
                );

                // 5. Sample Applications
                context.Applications.AddRange(
                    new Application
                    {
                        UserId = particular.Id,
                        AnimalListingId = 1,
                        Message = "Interested in adopting Cooper!",
                        Status = ApplicationStatus.Approved,
                        SubmittedAt = DateTime.Now.AddDays(-1)
                    },
                    new Application
                    {
                        UserId = particular.Id,
                        AnimalListingId = 2,
                        Message = "Love Luna, is she good with kids?",
                        Status = ApplicationStatus.Pending,
                        SubmittedAt = DateTime.Now
                    }
                );

                // 6. Messages de teste (3 conversas)
                if (!context.Messages.Any())
                {
                    context.Messages.AddRange(
                        // João <-> Sarah (PetSitter)
                        new Message
                        {
                            SenderId = particular.Id,
                            ReceiverId = sitter1.Id,
                            Content = "Olá Sarah! Preciso de alguém para passear o meu cão na próxima semana.",
                            Timestamp = DateTime.Now.AddDays(-3),
                            IsRead = true
                        },
                        new Message
                        {
                            SenderId = sitter1.Id,
                            ReceiverId = particular.Id,
                            Content = "Olá João! Tenho disponibilidade. Qual a raça e tamanho?",
                            Timestamp = DateTime.Now.AddDays(-3).AddHours(2),
                            IsRead = false
                        },

                        // João <-> Shelter (Cooper)
                        new Message
                        {
                            SenderId = particular.Id,
                            ReceiverId = shelter.Id,
                            Content = "Olá! O Cooper dá-se bem com outros cães?",
                            Timestamp = DateTime.Now.AddDays(-1),
                            IsRead = false
                        },

                        // Shelter -> João (resposta)
                        new Message
                        {
                            SenderId = shelter.Id,
                            ReceiverId = particular.Id,
                            Content = "Sim, o Cooper é muito sociável!",
                            Timestamp = DateTime.Now.AddHours(-4),
                            IsRead = true
                        }
                    );
                }

                // Salva tudo
                context.SaveChanges();
            }
        }
    }
}

