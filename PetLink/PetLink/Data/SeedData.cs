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
                // 1. Base data (users, petsitters, animals, etc.)
                if (!context.Users.Any())
                {
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

                    var shelter = new User
                    {
                        Name = "Sunny Paws Shelter",
                        Email = "hello@sunnypaws.com",
                        PasswordHash = UserHashHelpers.HashPassword("shelter123"),
                        Role = UserRole.Shelter,
                        IsVerified = true,
                        Phone = "+351 234 567 890",
                        City = "Braga",
                        Bio = "Animal shelter specializing in rescue dogs and cats",
                        ProfilePicture = "/images/avatars/sunnypaws.jpg",
                        CreatedAt = DateTime.Now.AddDays(-180)
                    };

                    var particular = new User
                    {
                        Name = "João Silva",
                        Email = "joao.silva@email.com",
                        PasswordHash = UserHashHelpers.HashPassword("joao123"),
                        Role = UserRole.User,
                        IsVerified = false,
                        Phone = "+351 912 345 679",
                        City = "Setúbal",
                        Bio = "Pet lover looking for adoption",
                        ProfilePicture = "/images/avatars/joaosilva.jpg",
                        CreatedAt = DateTime.Now.AddDays(-30)
                    };

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

                    context.Users.AddRange(admin, shelter, particular, sitter1, sitter2, sitter3);
                    context.SaveChanges();

                    // 2. Petsitter profiles
                    var sarahPs = new Petsitter
                    {
                        UserId = sitter1.Id,
                        Age = 31,
                        serviceType = ServiceType.Walking,
                        petPreferences = PetPreferences.SmallDogs,
                        HourlyRate = 3.00m,
                        Rating = 4.9,
                        LocationZone = "Downtown",
                        DistanceKm = 2.4,
                        Bio = "Professional dog walker with 5+ years experience. Specialized in small dogs and puppies.",
                        SpecialtyTags = "SMALL DOGS,PUPPIES,DAILY WALKS"
                    };

                    var marcusPs = new Petsitter
                    {
                        UserId = sitter2.Id,
                        Age = 42,
                        serviceType = ServiceType.Boarding,
                        petPreferences = PetPreferences.Cats,
                        HourlyRate = 7.00m,
                        Rating = 5.0,
                        LocationZone = "Westside",
                        DistanceKm = 0.8,
                        Bio = "Cat behavior specialist with veterinary background. Luxury boarding available.",
                        SpecialtyTags = "CATS ONLY,MEDICAL CARE,BOARDING"
                    };

                    var elenaPs = new Petsitter
                    {
                        UserId = sitter3.Id,
                        Age = 21,
                        serviceType = ServiceType.HouseSitting,
                        petPreferences = PetPreferences.Exotic,
                        HourlyRate = 5.00m,
                        Rating = 4.8,
                        LocationZone = "North Park",
                        DistanceKm = 3.2,
                        Bio = "University student experienced with exotic pets and birds. Weekend availability.",
                        SpecialtyTags = "BIRDS,EXOTIC,REPTILES"
                    };

                    context.Petsitters.AddRange(sarahPs, marcusPs, elenaPs);

                    // 3. Animal listings (stored as variables so we can reference their IDs)
                    var cooper = new AnimalListing
                    {
                        Name = "Cooper",
                        Species = Species.Dog,
                        Age = Age.Puppy,
                        AgeMonths = 4,
                        Location = "Lisbon",
                        Description = "Energetic puppy loves playing!",
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-1),
                        TutorId = shelter.Id,
                        ImageUrl = "/images/animals/cooper.jpg",
                        HealthDocuments = new List<HealthDocument>
                        {
                            new HealthDocument { Name = "Boletim de Vacinas", Type = HealthDocumentType.Vaccine, FilePath = "/images/placeholders/proof_vacination.png" },
                            new HealthDocument { Name = "Desparasitação", Type = HealthDocumentType.Deworming, FilePath = "/images/placeholders/proof_vacination.png" },
                            new HealthDocument { Name = "Certificado Esterilização", Type = HealthDocumentType.Sterilization, FilePath = "/images/placeholders/proof_vacination.png" }
                        }
                    };

                    var luna = new AnimalListing
                    {
                        Name = "Luna",
                        Species = Species.Cat,
                        Age = Age.Adult,
                        AgeMonths = 24,
                        Location = "Setúbal",
                        Description = "Calm apartment cat",
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-2),
                        TutorId = shelter.Id,
                        ImageUrl = "/images/animals/luna.jpg",
                        HealthDocuments = new List<HealthDocument>
                        {
                            new HealthDocument { Name = "Boletim de Vacinas", Type = HealthDocumentType.Vaccine, FilePath = "/images/placeholders/proof_vacination.png" },
                            new HealthDocument { Name = "Desparasitação", Type = HealthDocumentType.Deworming, FilePath = "/images/placeholders/proof_vacination.png" },
                            new HealthDocument { Name = "Certificado Esterilização", Type = HealthDocumentType.Sterilization, FilePath = "/images/placeholders/proof_vacination.png" }
                        }
                    };

                    var rio = new AnimalListing
                    {
                        Name = "Rio",
                        Species = Species.Bird,
                        Age = Age.Puppy,
                        AgeMonths = 12,
                        Location = "Leiria",
                        Description = "Talkative macaw parrot",
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-3),
                        TutorId = particular.Id,
                        ImageUrl = "/images/animals/rio.jpg"
                    };

                    var shadow = new AnimalListing
                    {
                        Name = "Shadow",
                        Species = Species.Dog,
                        Age = Age.Adult,
                        AgeMonths = 60,
                        Location = "Algarve",
                        Description = "Loyal companion for quiet home",
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-4),
                        TutorId = shelter.Id,
                        ImageUrl = "/images/animals/shadow.jpg",
                        HealthDocuments = new List<HealthDocument>
                        {
                            new HealthDocument { Name = "Boletim de Vacinas", Type = HealthDocumentType.Vaccine, FilePath = "/images/placeholders/proof_vacination.png" },
                            new HealthDocument { Name = "Desparasitação", Type = HealthDocumentType.Deworming, FilePath = "/images/placeholders/proof_vacination.png" },
                            new HealthDocument { Name = "Certificado Esterilização", Type = HealthDocumentType.Sterilization, FilePath = "/images/placeholders/proof_vacination.png" }
                        }
                    };

                    var rex = new AnimalListing
                    {
                        Name = "Rex",
                        Species = Species.Dog,
                        Age = Age.Adult,
                        AgeMonths = 36,
                        Location = "Porto",
                        Description = "Friendly French Bulldog",
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-5),
                        TutorId = shelter.Id,
                        ImageUrl = "/images/animals/rex.jpg",
                        HealthDocuments = new List<HealthDocument>
                        {
                            new HealthDocument { Name = "Boletim de Vacinas", Type = HealthDocumentType.Vaccine, FilePath = "/images/placeholders/proof_vacination.png" },
                            new HealthDocument { Name = "Desparasitação", Type = HealthDocumentType.Deworming, FilePath = "/images/placeholders/proof_vacination.png" },
                            new HealthDocument { Name = "Certificado Esterilização", Type = HealthDocumentType.Sterilization, FilePath = "/images/placeholders/proof_vacination.png" }
                        }
                    };

                    var max = new AnimalListing
                    {
                        Name = "Max",
                        Species = Species.Dog,
                        Age = Age.Senior,
                        AgeMonths = 96,
                        Location = "Coimbra",
                        Description = "Trained senior German Shepherd",
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-6),
                        TutorId = particular.Id,
                        ImageUrl = "/images/animals/max.jpg",
                        HealthDocuments = new List<HealthDocument>
                        {
                            new HealthDocument { Name = "Boletim de Vacinas", Type = HealthDocumentType.Vaccine, FilePath = "/images/placeholders/proof_vacination.png" },
                            new HealthDocument { Name = "Desparasitação", Type = HealthDocumentType.Deworming, FilePath = "/images/placeholders/proof_vacination.png" },
                            new HealthDocument { Name = "Certificado Esterilização", Type = HealthDocumentType.Sterilization, FilePath = "/images/placeholders/proof_vacination.png" }
                        }
                    };

                    var daisy = new AnimalListing
                    {
                        Name = "Daisy",
                        Species = Species.Dog,
                        Age = Age.Adult,
                        AgeMonths = 48,
                        Location = "Braga",
                        Description = "Active Beagle needs exercise",
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-7),
                        TutorId = shelter.Id,
                        ImageUrl = "/images/animals/daisy.jpg",
                        HealthDocuments = new List<HealthDocument>
                        {
                            new HealthDocument { Name = "Boletim de Vacinas", Type = HealthDocumentType.Vaccine, FilePath = "/images/placeholders/proof_vacination.png" },
                            new HealthDocument { Name = "Desparasitação", Type = HealthDocumentType.Deworming, FilePath = "/images/placeholders/proof_vacination.png" }
                        }
                    };

                    var misty = new AnimalListing
                    {
                        Name = "Misty",
                        Species = Species.Cat,
                        Age = Age.Adult,
                        AgeMonths = 60,
                        Location = "Aveiro",
                        Description = "Independent longhair cat",
                        Status = ListingStatus.Published,
                        CreatedAt = DateTime.Now.AddDays(-8),
                        TutorId = particular.Id,
                        ImageUrl = "/images/animals/misty.jpg",
                        HealthDocuments = new List<HealthDocument>
                        {
                            new HealthDocument { Name = "Boletim de Vacinas", Type = HealthDocumentType.Vaccine, FilePath = "/images/placeholders/proof_vacination.png" },
                            new HealthDocument { Name = "Desparasitação", Type = HealthDocumentType.Deworming, FilePath = "/images/placeholders/proof_vacination.png" },
                            new HealthDocument { Name = "Certificado Esterilização", Type = HealthDocumentType.Sterilization, FilePath = "/images/placeholders/proof_vacination.png" }
                        }
                    };

                    context.AnimalListings.AddRange(cooper, luna, rio, shadow, rex, max, daisy, misty);
                    context.SaveChanges();

                    // 4. Favorite Pets 
                    context.FavoritePets.AddRange(
                        new FavoritePet
                        {
                            UserId = particular.Id,
                            AnimalListingId = cooper.Id,
                            CreatedAt = DateTime.Now.AddDays(-2)
                        },
                        new FavoritePet
                        {
                            UserId = particular.Id,
                            AnimalListingId = shadow.Id,
                            CreatedAt = DateTime.Now.AddDays(-1)
                        }
                    );

                    // 5. Sample Applications 
                    context.Applications.AddRange(
                        new Application
                        {
                            UserId = particular.Id,
                            AnimalListingId = cooper.Id,
                            Message = "Interested in adopting Cooper!",
                            Status = ApplicationStatus.Approved,
                            SubmittedAt = DateTime.Now.AddDays(-1)
                        },
                        new Application
                        {
                            UserId = particular.Id,
                            AnimalListingId = luna.Id,
                            Message = "Love Luna, is she good with kids?",
                            Status = ApplicationStatus.Pending,
                            SubmittedAt = DateTime.Now
                        }
                    );

                    // 6. Sample Messages
                    if (!context.Messages.Any())
                    {
                        context.Messages.AddRange(
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
                            new Message
                            {
                                SenderId = particular.Id,
                                ReceiverId = shelter.Id,
                                Content = "Olá! O Cooper dá-se bem com outros cães?",
                                Timestamp = DateTime.Now.AddDays(-1),
                                IsRead = false
                            },
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

                    context.SaveChanges();
                }

                // 7. Bookings
                if (!context.Bookings.Any())
                {
                    var particular = context.Users.FirstOrDefault(u => u.Email == "joao.silva@email.com");
                    var sitter1 = context.Users.FirstOrDefault(u => u.Email == "sarah@petlink.com");
                    var sitter2 = context.Users.FirstOrDefault(u => u.Email == "marcus@petlink.com");
                    var sitter3 = context.Users.FirstOrDefault(u => u.Email == "elena@petlink.com");

                    var sarahPs = context.Petsitters.FirstOrDefault(p => p.UserId == sitter1.Id);
                    var marcusPs = context.Petsitters.FirstOrDefault(p => p.UserId == sitter2.Id);
                    var elenaPs = context.Petsitters.FirstOrDefault(p => p.UserId == sitter3.Id);

                    if (particular != null && sarahPs != null && marcusPs != null && elenaPs != null)
                    {
                        var bookings = new List<Booking>
                        {
                            new Booking
                            {
                                UserId = particular.Id,
                                PetsitterId = sarahPs.Id,
                                ServiceType = ServiceType.Walking,
                                StartDate = DateTime.Today.AddDays(3),
                                EndDate = DateTime.Today.AddDays(3).AddHours(2),
                                PetName = "Rex",
                                PetSpecies = "Dog",
                                Message = "Preciso de alguém para passear o meu cão Rex por 2 horas no centro.",
                                TotalPrice = sarahPs.HourlyRate * 2,
                                Status = BookingStatus.Confirmed,
                                CreatedAt = DateTime.Now.AddDays(-2),
                                UpdatedAt = DateTime.Now.AddDays(-1)
                            },
                            new Booking
                            {
                                UserId = particular.Id,
                                PetsitterId = marcusPs.Id,
                                ServiceType = ServiceType.Boarding,
                                StartDate = DateTime.Today.AddDays(10),
                                EndDate = DateTime.Today.AddDays(12),
                                PetName = "Misty",
                                PetSpecies = "Cat",
                                Message = "Preciso de deixar a minha gata Misty durante um fim de semana prolongado.",
                                TotalPrice = marcusPs.HourlyRate * 48,
                                Status = BookingStatus.Pending,
                                CreatedAt = DateTime.Now.AddDays(-1)
                            },
                            new Booking
                            {
                                UserId = particular.Id,
                                PetsitterId = elenaPs.Id,
                                ServiceType = ServiceType.HouseSitting,
                                StartDate = DateTime.Today.AddDays(5),
                                EndDate = DateTime.Today.AddDays(7),
                                PetName = "Rio",
                                PetSpecies = "Bird",
                                Message = "Vou viajar e preciso de alguém para cuidar do meu papagaio Rio em casa.",
                                TotalPrice = elenaPs.HourlyRate * 48,
                                Status = BookingStatus.Pending,
                                CreatedAt = DateTime.Now.AddHours(-12)
                            }
                        };

                        context.Bookings.AddRange(bookings);
                        context.SaveChanges();
                    }
                }

                // 8. Reviews
                if (!context.Reviews.Any())
                {
                    var particular = context.Users.FirstOrDefault(u => u.Email == "joao.silva@email.com");
                    var shelter = context.Users.FirstOrDefault(u => u.Email == "hello@sunnypaws.com");
                    var sitter1 = context.Users.FirstOrDefault(u => u.Email == "sarah@petlink.com");
                    var admin = context.Users.FirstOrDefault(u => u.Email == "admin@petlink.pt");
                    var cooper = context.AnimalListings.FirstOrDefault(a => a.Name == "Cooper" && a.TutorId == shelter.Id);
                    var luna = context.AnimalListings.FirstOrDefault(a => a.Name == "Luna" && a.TutorId == shelter.Id);

                    if (particular != null && shelter != null && cooper != null)
                    {
                        context.Reviews.AddRange(
                            new Review
                            {
                                ReviewerId = particular.Id,
                                ReviewedId = shelter.Id,
                                AnimalListingId = cooper.Id,
                                Rating = 5,
                                CreatedAt = DateTime.Now.AddDays(-1),
                                IsApproved = true
                            },
                            new Review
                            {
                                ReviewerId = shelter.Id,
                                ReviewedId = particular.Id,
                                AnimalListingId = cooper.Id,
                                Rating = 5,
                                CreatedAt = DateTime.Now.AddDays(-1),
                                IsApproved = true
                            }
                        );

                        if (sitter1 != null && luna != null)
                        {
                            context.Reviews.Add(
                                new Review
                                {
                                    ReviewerId = particular.Id,
                                    ReviewedId = sitter1.Id,
                                    AnimalListingId = luna.Id,
                                    Rating = 4,
                                    CreatedAt = DateTime.Now.AddDays(-5),
                                    IsApproved = true
                                }
                            );
                        }

                        if (admin != null)
                        {
                            context.Reviews.Add(
                                new Review
                                {
                                    ReviewerId = admin.Id,
                                    ReviewedId = shelter.Id,
                                    AnimalListingId = cooper.Id,
                                    Rating = 5,
                                    CreatedAt = DateTime.Now.AddDays(-7),
                                    IsApproved = true
                                }
                            );
                        }

                        context.SaveChanges();
                    }
                }

                // 9. Events
                if (!context.Events.Any())
                {
                    var shelter = context.Users.FirstOrDefault(u => u.Email == "hello@sunnypaws.com");
                    var admin = context.Users.FirstOrDefault(u => u.Email == "admin@petlink.pt");

                    if (shelter != null && admin != null)
                    {
                        context.Events.AddRange(
                            new Event
                            {
                                Name = "Feira de Adoção de Verão",
                                Description = "Venha conhecer os nossos animais resgatados e encontre o seu novo melhor amigo. Haverá atividades, rastreios veterinários gratuitos e muita diversão para toda a família.",
                                StartDate = DateTime.Today.AddDays(-15),
                                EndDate = DateTime.Today.AddDays(-15).AddHours(8),
                                Location = "Parque da Cidade, Braga",
                                Type = EventType.Adoption,
                                Status = EventStatus.Completed,
                                ImageUrl = "/images/events/feira-adocao.png",
                                OrganizerId = shelter.Id,
                                CreatedAt = DateTime.Now.AddDays(-45),
                                UpdatedAt = DateTime.Now.AddDays(-14),
                                ApprovedAt = DateTime.Now.AddDays(-40),
                                ApprovedBy = admin.Id,
                                AcceptsDonations = true,
                                AcceptsVolunteers = true
                            },
                            new Event
                            {
                                Name = "Workshop de Cuidados Caninos",
                                Description = "Workshop prático sobre nutrição, higiene e cuidados básicos para cães. Ministrado por veterinários convidados.",
                                StartDate = DateTime.Today.AddDays(20),
                                EndDate = DateTime.Today.AddDays(20).AddHours(4),
                                Location = "Auditório da Câmara Municipal, Braga",
                                Type = EventType.Education,
                                Status = EventStatus.Approved,
                                ImageUrl = "/images/events/workshop-caninos.png",
                                OrganizerId = shelter.Id,
                                CreatedAt = DateTime.Now.AddDays(-20),
                                UpdatedAt = DateTime.Now.AddDays(-10),
                                ApprovedAt = DateTime.Now.AddDays(-15),
                                ApprovedBy = admin.Id,
                                AcceptsDonations = false,
                                AcceptsVolunteers = false
                            },
                            new Event
                            {
                                Name = "Sessão de Yoga Canino",
                                Description = "Uma manhã relaxante de yoga com os nossos cães resgatados. Contribua para o bem-estar dos animais enquanto pratica exercício. Todos os lucros revertem para o abrigo.",
                                StartDate = DateTime.Today.AddDays(45),
                                EndDate = DateTime.Today.AddDays(45).AddHours(3),
                                Location = "Parque da Cidade, Braga",
                                Type = EventType.Fundraising,
                                Status = EventStatus.Approved,
                                ImageUrl = "/images/events/yoga-canino.png",
                                OrganizerId = shelter.Id,
                                CreatedAt = DateTime.Now.AddDays(-20),
                                UpdatedAt = DateTime.Now.AddDays(-10),
                                ApprovedAt = DateTime.Now.AddDays(-15),
                                ApprovedBy = admin.Id,
                                AcceptsDonations = true,
                                AcceptsVolunteers = false
                            },
                            new Event
                            {
                                Name = "Dia de Voluntariado no Abrigo",
                                Description = "Passa um dia connosco a ajudar no cuidado dos animais: limpeza, alimentação, passeios e socialização. Traga a sua energia e amor pelos animais!",
                                StartDate = DateTime.Today.AddDays(60),
                                EndDate = DateTime.Today.AddDays(60).AddHours(6),
                                Location = "Abrigo Sunny Paws, Braga",
                                Type = EventType.Volunteer,
                                Status = EventStatus.Approved,
                                ImageUrl = "/images/events/voluntariado.png",
                                OrganizerId = shelter.Id,
                                CreatedAt = DateTime.Now.AddDays(-30),
                                UpdatedAt = DateTime.Now.AddDays(-25),
                                ApprovedAt = DateTime.Now.AddDays(-28),
                                ApprovedBy = admin.Id,
                                AcceptsDonations = false,
                                AcceptsVolunteers = true
                            }
                        );

                        context.SaveChanges();
                    }
                }

                // 10. Event Interests
                if (!context.EventInterests.Any())
                {
                    var particular = context.Users.FirstOrDefault(u => u.Email == "joao.silva@email.com");
                    var feira = context.Events.FirstOrDefault(e => e.Name == "Feira de Adoção de Verão");
                    var voluntariado = context.Events.FirstOrDefault(e => e.Name == "Dia de Voluntariado no Abrigo");
                    var workshop = context.Events.FirstOrDefault(e => e.Name == "Workshop de Cuidados Caninos");

                    if (particular != null)
                    {
                        var interests = new List<EventInterest>();

                        if (feira != null)
                        {
                            interests.Add(new EventInterest
                            {
                                EventId = feira.Id,
                                UserId = particular.Id,
                                RegisteredAt = DateTime.Now.AddDays(-20),
                                IsConfirmed = true
                            });
                        }

                        if (voluntariado != null)
                        {
                            interests.Add(new EventInterest
                            {
                                EventId = voluntariado.Id,
                                UserId = particular.Id,
                                RegisteredAt = DateTime.Now.AddDays(-10),
                                IsConfirmed = false
                            });
                        }

                        if (workshop != null)
                        {
                            var admin = context.Users.FirstOrDefault(u => u.Email == "admin@petlink.pt");
                            if (admin != null)
                            {
                                interests.Add(new EventInterest
                                {
                                    EventId = workshop.Id,
                                    UserId = admin.Id,
                                    RegisteredAt = DateTime.Now.AddDays(-15),
                                    IsConfirmed = true
                                });
                            }
                        }

                        if (interests.Any())
                        {
                            context.EventInterests.AddRange(interests);
                            context.SaveChanges();
                        }
                    }
                }

                // 11. Resources
                if (!context.Resources.Any())
                {
                    context.Resources.AddRange(
                        new Resource
                        {
                            Title = "Guia Completo de Vacinação Canina",
                            Content = "Manter as vacinas do seu cão em dia é essencial para a saúde dele. Este guia aborda o calendário de vacinação, as vacinas obrigatórias e recomendadas, e a frequência de reforços. Vacinas como a antirrábica, esgana, parvovirose e leptospirose são fundamentais para prevenir doenças graves.",
                            Type = ResourceType.Article,
                            Species = Species.Dog,
                            Category = ResourceCategory.Health,
                            MediaUrl = null,
                            CreatedAt = DateTime.Now.AddDays(-60)
                        },
                        new Resource
                        {
                            Title = "Como Alimentar o Seu Gato Adulto",
                            Content = "A alimentação felina requer cuidados especiais. Os gatos são carnívoros estritos e necessitam de uma dieta rica em proteína animal. Saiba quais os alimentos recomendados, a frequência das refeições e como ler os rótulos das rações para garantir uma nutrição equilibrada ao seu felino.",
                            Type = ResourceType.Article,
                            Species = Species.Cat,
                            Category = ResourceCategory.Nutrition,
                            MediaUrl = null,
                            CreatedAt = DateTime.Now.AddDays(-50)
                        },
                        new Resource
                        {
                            Title = "Técnicas de Adestramento Positivo",
                            Content = "O adestramento positivo baseia-se no reforço de comportamentos desejados através de recompensas, nunca de punições. Neste artigo explicamos técnicas como o clicker training, o targeting e como ensinar comandos básicos como senta, fica e vem ao seu cão.",
                            Type = ResourceType.Article,
                            Species = Species.Dog,
                            Category = ResourceCategory.Training,
                            MediaUrl = null,
                            CreatedAt = DateTime.Now.AddDays(-40)
                        },
                        new Resource
                        {
                            Title = "Cuidados Básicos para Aves Domésticas",
                            Content = "Ter uma ave como animal de estimação requer conhecimentos específicos. Abordamos a alimentação adequada, a importância da socialização, cuidados com a gaiola, banhos e sinais de doença a que deve estar atento.",
                            Type = ResourceType.Article,
                            Species = Species.Bird,
                            Category = ResourceCategory.Health,
                            MediaUrl = null,
                            CreatedAt = DateTime.Now.AddDays(-30)
                        },
                        new Resource
                        {
                            Title = "Como Preparar a Sua Casa para a Chegada de um Animal",
                            Content = "A chegada de um novo animal a casa é um momento emocionante. Prepare o ambiente com os acessórios essenciais, crie um espaço seguro, estabeleça rotinas e saiba como fazer uma apresentação gradual ao resto da família.",
                            Type = ResourceType.Article,
                            Species = Species.Dog,
                            Category = ResourceCategory.General,
                            MediaUrl = null,
                            CreatedAt = DateTime.Now.AddDays(-20)
                        },
                        new Resource
                        {
                            Title = "O Essencial sobre Roedores Domésticos",
                            Content = "Hamsters, porquinhos-da-índia e gerbos são animais de estimação populares que requerem cuidados específicos. Neste guia abordamos a gaiola ideal, alimentação adequada, enriquecimento ambiental e os sinais de doença mais comuns nestes pequenos companheiros.",
                            Type = ResourceType.Article,
                            Species = Species.Rodent,
                            Category = ResourceCategory.General,
                            MediaUrl = null,
                            CreatedAt = DateTime.Now.AddDays(-15)
                        },
                        new Resource
                        {
                            Title = "Guia de Alimentação para Répteis",
                            Content = "Répteis têm necessidades nutricionais muito específicas que variam conforme a espécie. Desde a suplementação de cálcio em lagartos até à alimentação de serpentes, este artigo crie os fundamentos para manter o seu réptil saudável.",
                            Type = ResourceType.Article,
                            Species = Species.Reptile,
                            Category = ResourceCategory.Nutrition,
                            MediaUrl = null,
                            CreatedAt = DateTime.Now.AddDays(-10)
                        },
                        new Resource
                        {
                            Title = "Vídeo: Como Dar Banho ao Seu Cão",
                            Content = "Neste vídeo tutorial da Go Groomer, aprenda a maneira correta de dar banho ao seu cão em casa. Abrange desde a escolha do champô e condicionador adequados até à secagem e limpeza dos ouvidos. Um guia completo para donos de todos os níveis.",
                            Type = ResourceType.Video,
                            Species = Species.Dog,
                            Category = ResourceCategory.Health,
                            MediaUrl = "https://www.youtube.com/watch?v=C7UyYQp4OJ8",
                            CreatedAt = DateTime.Now.AddDays(-25)
                        },
                        new Resource
                        {
                            Title = "Vídeo: Enriquecimento Ambiental para Gatos",
                            Content = "Jackson Galaxy, especialista em comportamento felino, explica como melhorar a vida do seu gato através da 'catification' — o processo de adaptar a sua casa às necessidades naturais do felino. Desde estantes e prateleiras até brinquedos DIY, aprenda a criar um ambiente rico e estimulante.",
                            Type = ResourceType.Video,
                            Species = Species.Cat,
                            Category = ResourceCategory.General,
                            MediaUrl = "https://www.youtube.com/watch?v=dn1r68vSGGU",
                            CreatedAt = DateTime.Now.AddDays(-18)
                        },
                        new Resource
                        {
                            Title = "Vídeo: Guia Completo para Iniciantes em Aquários",
                            Content = "O Aquascaping Cube apresenta um tutorial passo a passo para montar e manter o seu primeiro aquário de água doce. Aborda equipamento necessário, ciclagem do tanque, escolha de plantas e peixes, e rotinas de manutenção para um aquário saudável.",
                            Type = ResourceType.Video,
                            Species = Species.Fish,
                            Category = ResourceCategory.General,
                            MediaUrl = "https://www.youtube.com/watch?v=l2_YSehHpbA",
                            CreatedAt = DateTime.Now.AddDays(-7)
                        }
                    );

                    context.SaveChanges();
                }

                // 12. Listings Notifications
                if (!context.ListingsNotifications.Any())
                {
                    var particular = context.Users.FirstOrDefault(u => u.Email == "joao.silva@email.com");
                    var shelter = context.Users.FirstOrDefault(u => u.Email == "hello@sunnypaws.com");
                    var cooper = context.AnimalListings.FirstOrDefault(a => a.Name == "Cooper");
                    var luna = context.AnimalListings.FirstOrDefault(a => a.Name == "Luna");

                    var notifications = new List<ListingsNotification>();

                    if (particular != null && cooper != null)
                    {
                        notifications.Add(new ListingsNotification
                        {
                            UserId = particular.Id,
                            Title = "Candidatura Aprovada",
                            Message = "A sua candidatura para adotar o Cooper foi aprovada! Entre em contacto com o abrigo para agendar uma visita.",
                            AnimalListingId = cooper.Id,
                            IsRead = false,
                            CreatedAt = DateTime.Now.AddDays(-1)
                        });
                    }

                    if (shelter != null && cooper != null)
                    {
                        notifications.Add(new ListingsNotification
                        {
                            UserId = shelter.Id,
                            Title = "Nova Candidatura de Adoção",
                            Message = "O utilizador João Silva está interessado em adotar o Cooper. Reveja a candidatura na secção de candidaturas.",
                            AnimalListingId = cooper.Id,
                            IsRead = true,
                            CreatedAt = DateTime.Now.AddDays(-2)
                        });
                    }

                    if (shelter != null && luna != null)
                    {
                        notifications.Add(new ListingsNotification
                        {
                            UserId = shelter.Id,
                            Title = "Nova Candidatura de Adoção",
                            Message = "O utilizador João Silva está interessado em adotar a Luna. Reveja a candidatura na secção de candidaturas.",
                            AnimalListingId = luna.Id,
                            IsRead = false,
                            CreatedAt = DateTime.Now.AddHours(-6)
                        });
                    }

                    if (particular != null)
                    {
                        notifications.Add(new ListingsNotification
                        {
                            UserId = particular.Id,
                            Title = "Novo Animal Disponível",
                            Message = "O Sunny Paws Shelter adicionou um novo animal: Daisy, uma Beagle ativa que precisa de exercício. Dê uma olhadela!",
                            IsRead = false,
                            CreatedAt = DateTime.Now.AddDays(-7)
                        });
                    }

                    if (notifications.Any())
                    {
                        context.ListingsNotifications.AddRange(notifications);
                        context.SaveChanges();
                    }
                }

                // 13. Favorite Petsitters
                if (!context.FavoritePetsitters.Any())
                {
                    var particular = context.Users.FirstOrDefault(u => u.Email == "joao.silva@email.com");
                    var sitter1User = context.Users.FirstOrDefault(u => u.Email == "sarah@petlink.com");

                    if (particular != null && sitter1User != null)
                    {
                        var sarahPs = context.Petsitters.FirstOrDefault(p => p.UserId == sitter1User.Id);

                        if (sarahPs != null)
                        {
                            context.FavoritePetsitters.Add(new FavoritePetsitter
                            {
                                UserId = particular.Id,
                                PetsitterId = sarahPs.Id,
                                CreatedAt = DateTime.Now.AddDays(-10)
                            });

                            context.SaveChanges();
                        }
                    }
                }

                // 14. Animal Photos (no additional photos available yet)
                // Photos can be added via the application's upload functionality.
            }
        }
    }
}
