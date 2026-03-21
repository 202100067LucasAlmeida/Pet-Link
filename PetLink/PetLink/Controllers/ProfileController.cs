using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using PetLink.Models.Enums;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PetLink.Controllers
{
    // Classe necessária para o JS verificar o email em tempo real
    public class EmailCheckRequest
    {
        public string Email { get; set; }
    }

    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Mostra a página de Login (GET)
        [HttpGet]
        public IActionResult LoginForm()
        {
            // Se já tiver login feito, manda para a Home
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        // 2. Recebe os dados do formulário quando clicas "Log In" (POST)
        [HttpPost]
        public async Task<IActionResult> LoginForm(string email, string password, bool rememberMe)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return Json(new { success = false, message = "Please fill in all fields." });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == password);

            if (user == null)
            {
                return Json(new { success = false, message = "Invalid email or password." });
            }

            // Login (Claims)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("UserId", user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = rememberMe });

            // Retorna sucesso para o JS fazer o redirect
            return Json(new { success = true });
        }

        // 3. Mostra a página de Registo (GET)
        [HttpGet]
        public IActionResult SignUpForm()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        // 4. Faz o Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }


        // sprint 2 -----------

        // Verificação do Email para o JavaScript (Real-time)
        [HttpPost]
        public async Task<IActionResult> ValidateEmail([FromBody] EmailCheckRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return Json(new { isAvailable = false });

            bool emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());
            return Json(new { isAvailable = !emailExists });
        }

        // Verificações pedidas do sign up e Criação de Conta
        [HttpPost]
        public async Task<IActionResult> ValidateSignUp([FromBody] SignUpValidationModel model)
        {
            var errors = new Dictionary<string, string>();

            // Validate Name
            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                errors["fullName"] = "O nome é obrigatório.";
            }
            else if (!IsValidName(model.FullName))
            {
                errors["fullName"] = "Name must not contain numbers or symbols.";
            }

            // Validate Email
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                errors["email"] = "O email é obrigatório.";
            }
            else if (!IsValidEmail(model.Email))
            {
                errors["email"] = "Please provide a valid email address.";
            }
            else
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    errors["email"] = "This email is already registered.";
                }
            }

            // Validate Phone
            if (string.IsNullOrWhiteSpace(model.Phone))
            {
                errors["phone"] = "Phone number is required.";
            }

            // Validate Password
            var passwordErrors = ValidatePassword(model.Password);
            if (passwordErrors.Any())
            {
                errors["password"] = string.Join(" ", passwordErrors);
            }

            // Validate Confirm Password
            if (model.Password != model.ConfirmPassword)
            {
                errors["confirmPassword"] = "Passwords do not match.";
            }

            if (errors.Any())
            {
                return Json(new { success = false, errors });
            }

            UserRole role = UserRole.User;
            if (model.UserType == "PetSitter") role = UserRole.PetSitter;
            if (model.UserType == "Shelter") role = UserRole.Shelter;

            var newUser = new PetLink.Models.User
            {
                Name = model.FullName,
                Email = model.Email,
                PasswordHash = model.Password,
                Role = role,
                IsVerified = false // Requer verificação do admin para Associações e PetSitters
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        [HttpPost]
        public IActionResult ValidatePassword([FromBody] PasswordValidationModel model)
        {
            var requirements = new Dictionary<string, bool>
            {
                { "length", model.Password?.Length >= 6 },
                { "lowercase", model.Password?.Any(char.IsLower) ?? false },
                { "uppercase", model.Password?.Any(char.IsUpper) ?? false },
                { "number", model.Password?.Any(char.IsDigit) ?? false },
                { "symbol", model.Password?.Any(c => !char.IsLetterOrDigit(c)) ?? false }
            };

            return Json(requirements);
        }


        // métodos extra
        private bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            return Regex.IsMatch(name, @"^[\p{L}\s\-']+$");
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private List<string> ValidatePassword(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(password))
            {
                errors.Add("Password is required.");
                return errors;
            }

            if (password.Length < 6)
                errors.Add("Minimum 6 characters.");

            if (!password.Any(char.IsLower))
                errors.Add("One lowercase letter required.");

            if (!password.Any(char.IsUpper))
                errors.Add("One uppercase letter required.");

            if (!password.Any(char.IsDigit))
                errors.Add("One number required.");

            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                errors.Add("One symbol required.");

            return errors;
        }


        //account settings
        public IActionResult AccountSettings()
        {
            return View();
        }

        // ==================== MY PROFILE PAGE ====================
        
        // GET: Profile/MyProfile
        [Authorize(Roles = "User,PetSitter")]
        public async Task<IActionResult> MyProfile()
        {
            // Obter o ID do utilizador logado
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Challenge();
            }

            // Buscar utilizador
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            // Buscar saved pets (favoritos)
            var savedPets = await _context.FavoritePets
                .Where(f => f.UserId == userId)
                .Include(f => f.AnimalListing)
                .Select(f => f.AnimalListing)
                .Where(a => a.Status == ListingStatus.Published)
                .Take(3)
                .ToListAsync();

            // Buscar active applications (pending ou approved)
            var activeApplications = await _context.Applications
                .Where(a => a.UserId == userId && 
                       (a.Status == ApplicationStatus.Pending || a.Status == ApplicationStatus.Approved))
                .Include(a => a.AnimalListing)
                .ThenInclude(a => a.Tutor)
                .OrderByDescending(a => a.SubmittedAt)
                .Take(3)
                .ToListAsync();

            // Buscar mensagens recentes
            var recentMessages = await _context.Messages
                .Where(m => m.ReceiverId == userId)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.CreatedAt)
                .Take(3)
                .ToListAsync();

            // Calcular estatísticas
            var totalApplications = await _context.Applications
                .CountAsync(a => a.UserId == userId);
                
            var unreadMessages = await _context.Messages
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);

            // Passar dados para a view via ViewBag
            ViewBag.SavedPets = savedPets;
            ViewBag.ActiveApplications = activeApplications;
            ViewBag.RecentMessages = recentMessages;
            ViewBag.TotalApplications = totalApplications;
            ViewBag.UnreadMessages = unreadMessages;

            return View(user);
        }

        // POST: Profile/MarkMessagesAsRead
        [HttpPost]
        [Authorize(Roles = "User,PetSitter")]
        public async Task<IActionResult> MarkMessagesAsRead()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Json(new { success = false });
            }

            var unreadMessages = await _context.Messages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .ToListAsync();

            foreach (var msg in unreadMessages)
            {
                msg.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}