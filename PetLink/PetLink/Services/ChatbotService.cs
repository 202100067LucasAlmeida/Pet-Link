using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using PetLink.Models.Enums;

namespace PetLink.Services
{
    /// <summary>
    /// Serviço responsável pelo chatbot inteligente da plataforma PetLink.
    /// Interpreta mensagens do utilizador e devolve respostas contextuais
    /// baseadas em dados reais da base de dados (adoção, petsitters, eventos, etc.).
    /// </summary>
    public class ChatbotService : IChatbotService
    {
        private readonly ApplicationDbContext _context;

        public ChatbotService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetBotResponseAsync(string userMessage, int? currentUserId)
        {
            var msg = userMessage.Trim().ToLowerInvariant();

            User? user = null;
            if (currentUserId.HasValue)
                user = await _context.Users.FindAsync(currentUserId.Value);

            if (ContainsAny(msg, "hi", "hello", "hey", "good morning", "good afternoon", "good evening"))
                return Greeting(user);

            if (ContainsAny(msg, "bye", "goodbye", "see you"))
                return "Goodbye! Come back anytime you need help. Wishing you and your pets all the best! 🐶🐱";

            if (ContainsAny(msg, "thank", "thanks", "appreciate"))
                return "You're welcome! I'm happy to help. If you ever need anything else, just type 'hello' to get started. 🐾";

            if (ContainsAny(msg, "how it works", "about", "what is petlink"))
                return PlatformExplanation();

            if (ContainsAny(msg, "cost", "price", "fee", "free"))
                return CostExplanation();

            if (ContainsAny(msg, "adopt", "adoption"))
                return await AdoptionHelpAsync(user);

            if (ContainsAny(msg, "puppy", "baby", "young dog"))
                return await AnimalAgeSearchAsync(Species.Dog, Age.Puppy);

            if (ContainsAny(msg, "kitten", "baby cat"))
                return await AnimalAgeSearchAsync(Species.Cat, Age.Puppy);

            if (ContainsAny(msg, "senior", "older"))
                return await AnimalSeniorSearchAsync();

            if (ContainsAny(msg, "dog", "dogs"))
                return await AnimalSearchAsync(Species.Dog);

            if (ContainsAny(msg, "cat", "cats"))
                return await AnimalSearchAsync(Species.Cat);

            if (ContainsAny(msg, "bird", "birds"))
                return await AnimalSearchAsync(Species.Bird);

            if (ContainsAny(msg, "rodent", "rodents", "hamster"))
                return await AnimalSearchAsync(Species.Rodent);

            if (ContainsAny(msg, "health", "vaccine", "deworm", "vet", "healthy"))
                return HealthExplanation();

            if (ContainsAny(msg, "sitter", "petsitter", "petsitting", "pet sitter"))
                return await PetsitterHelpAsync();

            if (ContainsAny(msg, "walk", "walking", "dog walker"))
                return await PetsitterServiceSearchAsync(ServiceType.Walking);

            if (ContainsAny(msg, "board", "boarding", "hotel", "stay"))
                return await PetsitterServiceSearchAsync(ServiceType.Boarding);

            if (ContainsAny(msg, "event", "events", "workshop", "meetup"))
                return await EventsHelpAsync();

            if (ContainsAny(msg, "location", "local", "shelter", "city", "where"))
                return await LocationHelpAsync();

            if (ContainsAny(msg, "contact", "support", "help", "admin"))
                return SupportInfo();

            if (ContainsAny(msg, "booking", "book", "schedule", "reservation"))
                return BookingHelp();

            if (ContainsAny(msg, "review", "rate", "rating", "stars"))
                return ReviewHelp();

            if (ContainsAny(msg, "profile", "account", "login", "password"))
                return ProfileHelp(user);

            return DefaultResponse();
        }

        private static bool ContainsAny(string message, params string[] keywords)
        {
            return keywords.Any(kw => message.Contains(kw));
        }

        private string Greeting(User? user)
        {
            var name = user?.Name?.Split(' ')[0] ?? "there";
            return $"Hey {name}! 👋 I'm **PetLink Buddy**, your virtual assistant. I can help you find pets to adopt, connect with petsitters, discover events, and more! Try asking me about:\n\n" +
                   $"🐶 **Dogs** - See available dogs for adoption\n" +
                   $"🐱 **Cats** - See available cats\n" +
                   $"📋 **Adopt** - Learn about the adoption process\n" +
                   $"🏠 **Petsitter** - Find a sitter\n" +
                   $"📅 **Events** - Upcoming PetLink events\n" +
                   $"📍 **Location** - Find shelters near you";
        }

        private async Task<string> AdoptionHelpAsync(User? user)
        {
            var availableCount = await _context.AnimalListings
                .CountAsync(a => a.Status == ListingStatus.Published);

            var speciesBreakdown = await _context.AnimalListings
                .Where(a => a.Status == ListingStatus.Published)
                .GroupBy(a => a.Species)
                .Select(g => new { Species = g.Key, Count = g.Count() })
                .ToListAsync();

            var breakdown = speciesBreakdown
                .Select(s => $"  - {s.Species}: **{s.Count}**")
                .ToList();

            var intro = "Here's how adoption works on **PetLink**:\n\n" +
                        "1. **Browse** available pets on the [Adopt page](/AnimalListings)\n" +
                        "2. **Click** a pet you're interested in to see details\n" +
                        "3. **Apply** by sending a message to the pet's tutor\n" +
                        "4. **Connect** with the tutor to arrange a meeting\n\n";

            var stats = $"📊 Right now there are **{availableCount}** pets available for adoption:\n" +
                        string.Join("\n", breakdown) + "\n\n";

            var action = user is not null
                ? "➡️ Ready to start? Head over to the [Adopt page](/AnimalListings) and find your new best friend!"
                : "🔑 You'll need to [Log in](/Profile/LoginForm) or [Sign up](/Profile/SignUpForm) before you can apply for adoption.";

            return intro + stats + action;
        }

        private async Task<string> AnimalSearchAsync(Species species)
        {
            var animals = await _context.AnimalListings
                .Where(a => a.Species == species && a.Status == ListingStatus.Published)
                .Include(a => a.Tutor)
                .Take(5)
                .ToListAsync();

            if (animals.Count == 0)
                return $"It looks like there are no **{species}** available for adoption right now. 😔 Check back soon or try searching for another type of pet!";

            var lines = animals.Select((a, i) =>
                $"  {i + 1}. **{a.Name}** - {a.AgeMonths / 12} yr(s) old, {a.Location}"
            );

            var total = await _context.AnimalListings
                .CountAsync(a => a.Species == species && a.Status == ListingStatus.Published);

            return $"🐾 I found **{total}** {species}(s) available! Here are some:\n\n" +
                   string.Join("\n", lines) + "\n\n" +
                   $"➡️ View all on the [Adopt page](/AnimalListings)";
        }

        private async Task<string> PetsitterHelpAsync()
        {
            var sitters = await _context.Petsitters
                .Include(p => p.User)
                .Take(3)
                .ToListAsync();

            var count = await _context.Petsitters.CountAsync();

            if (sitters.Count == 0)
                return "There are no petsitters registered yet. Check back soon!";

            var lines = sitters.Select((s, i) =>
                $"  {i + 1}. **{s.User?.Name ?? "Unknown"}** - {s.LocationZone ?? "Location not set"}"
            );

            return $"🏠 We have **{count}** petsitter(s) ready to help!\n\n" +
                   string.Join("\n", lines) + "\n\n" +
                   "➡️ Visit the [Petsitting page](/Petsitter) for more details and to book a sitter.";
        }

        private async Task<string> EventsHelpAsync()
        {
            var upcoming = await _context.Events
                .Where(e => e.StartDate >= DateTime.UtcNow)
                .OrderBy(e => e.StartDate)
                .Take(5)
                .ToListAsync();

            if (upcoming.Count == 0)
                return "There are no upcoming events scheduled. Stay tuned for future PetLink events!";

            var lines = upcoming.Select((e, i) =>
                $"  {i + 1}. **{e.Name}** - {e.StartDate:MMM dd, yyyy} at {e.Location}"
            );

            return $"📅 Here are the upcoming events:\n\n" +
                   string.Join("\n", lines) + "\n\n" +
                   "➡️ Check the [Events page](/Events) for full details.";
        }

        private async Task<string> LocationHelpAsync()
        {
            var cities = await _context.AnimalListings
                .Where(a => a.Status == ListingStatus.Published)
                .Select(a => a.Location)
                .Distinct()
                .Take(10)
                .ToListAsync();

            if (cities.Count == 0)
                return "There are no locations with available pets right now.";

            var cityList = string.Join(", ", cities);

            return $"📍 Pets are currently available in these locations:\n\n**{cityList}**\n\n" +
                   "➡️ Use the [Map view](/AnimalListings/Map) to see all pets on an interactive map!";
        }

        private static string SupportInfo()
        {
            return "Need help? Here's how to reach us:\n\n" +
                   "  📧 **Email**: support@petlink.com\n" +
                   "  💬 **Messages**: Use the in-platform messaging\n" +
                   "  ❓ **FAQ**: Check our Resources page\n\n" +
                   "Or just ask me - I'm here 24/7! 😊";
        }

        private static string BookingHelp()
        {
            return "To book a petsitter:\n\n" +
                   "1. Go to the [Petsitting page](/Petsitter)\n" +
                   "2. Choose a sitter and view their profile\n" +
                   "3. Click 'Book' and select your desired service\n" +
                   "4. Wait for the sitter to confirm your booking\n\n" +
                   "Need more help? Just ask!";
        }

        private static string ReviewHelp()
        {
            return "You can leave a review after adopting a pet or using a petsitter's services. " +
                   "Go to your **Profile** and find the listing or booking you want to review. " +
                   "Ratings help the community make better choices! ⭐";
        }

        private static string ProfileHelp(User? user)
        {
            if (user is null)
                return "You're not logged in yet. 🔑 [Log in](/Profile/LoginForm) or [Sign up](/Profile/SignUpForm) to access your profile and start adopting!";

            return $"You're logged in as **{user.Name}**. You can manage your profile, view your adoption applications, " +
                   $"check your favorite pets, and more from your [Profile page](/Profile/MyProfile).";
        }

        private static string DefaultResponse()
        {
            return "I'm not sure I understand that one yet! 😅 Try asking about:\n\n" +
                   "  👋 **Hello** - Start a conversation\n" +
                   "  🐶 **Dogs** - Find dogs for adoption\n" +
                   "  🐱 **Cats** - Find cats for adoption\n" +
                   "  📋 **Adopt** - Learn the adoption process\n" +
                   "  🏠 **Petsitter** - Find a sitter\n" +
                   "  📅 **Events** - Upcoming events\n" +
                   "  📍 **Location** - Pets near you";
        }

        private string PlatformExplanation()
        {
            return "💡 **PetLink** is a community-driven platform connecting animal lovers! \n\n" +
                   "We have two main goals:\n" +
                   "1. **Adoption:** Connecting shelters and individuals with loving families to find homes for pets.\n" +
                   "2. **Petsitting:** Helping pet owners find trusted, reviewed local sitters and dog walkers.\n\n" +
                   "Everything is managed safely within our platform. Try typing **'Adopt'** or **'Sitter'** to see how it works!";
        }

        private string CostExplanation()
        {
            return "💰 **Is PetLink free?**\n\n" +
                   "**Adopting:** PetLink does not charge adoption fees. However, some shelters may have a standard adoption fee to cover vaccines, microchipping, and sterilization. You discuss this directly with them.\n\n" +
                   "**Petsitting:** PetSitters set their own hourly or daily rates. You can see their prices upfront on their profiles before booking!";
        }

        private string HealthExplanation()
        {
            return "🏥 **Pet Health & Transparency**\n\n" +
                   "On PetLink, we take pet health seriously! When you browse pets, look for the health badges on their cards:\n" +
                   "🛡️ **Vaccinated**\n" +
                   "💊 **Dewormed**\n" +
                   "❤️ **Sterilized**\n\n" +
                   "If an animal has a 'Needs Vet' badge, it means their medical history is currently unknown or incomplete.";
        }

        private async Task<string> AnimalAgeSearchAsync(Species species, Age targetAge)
        {
            var animals = await _context.AnimalListings
                .Where(a => a.Species == species && a.Age == targetAge && a.Status == ListingStatus.Published)
                .Take(3)
                .ToListAsync();

            string ageStr = targetAge == Age.Puppy ? "babies/puppies" : targetAge.ToString().ToLower();

            if (animals.Count == 0)
                return $"I couldn't find any {ageStr} {species}s right now. They get adopted really fast! 🏃‍♂️💨";

            var lines = animals.Select(a => $"  - **{a.Name}** ({a.Location})");

            return $"🍼 We have some adorable {ageStr} {species}s looking for a home!\n\n" +
                   string.Join("\n", lines) + "\n\n" +
                   "➡️ Filter by Age on the [Adopt page](/AnimalListings) to see them all!";
        }

        private async Task<string> AnimalSeniorSearchAsync()
        {
            var count = await _context.AnimalListings
                .CountAsync(a => a.Age == Age.Senior && a.Status == ListingStatus.Published);

            if (count == 0)
                return "We don't have any senior pets listed right now.";

            return $"❤️ Senior pets are amazing! They are calmer, often already trained, and have so much love to give.\n\n" +
                   $"We currently have **{count} senior pets** waiting for a quiet, loving home.\n\n" +
                   "➡️ Go to the [Adopt page](/AnimalListings) and select the 'Senior (7+ yrs)' filter to meet them.";
        }

        private async Task<string> PetsitterServiceSearchAsync(ServiceType service)
        {
            var sitters = await _context.Petsitters
                .Include(p => p.User)
                .Where(p => p.serviceType == service)
                .Take(3)
                .ToListAsync();

            string serviceName = service == ServiceType.Walking ? "Dog Walking" : service.ToString();

            if (sitters.Count == 0)
                return $"We don't have anyone specializing exclusively in **{serviceName}** right now, but many sitters are flexible!";

            var lines = sitters.Select(s => $"  - **{s.User?.Name}** (⭐ {s.Rating}/5)");

            return $"🐕 Looking for **{serviceName}**? Here are some top-rated locals:\n\n" +
                   string.Join("\n", lines) + "\n\n" +
                   "➡️ View their full profiles on the [Petsitting page](/Petsitter)!";
        }
    }
}