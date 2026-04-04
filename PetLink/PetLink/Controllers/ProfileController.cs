using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Hubs;
using PetLink.Models;
using PetLink.Models.Enums;
using PetLink.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PetLink.Controllers
{
    public class EmailCheckRequest
    {
        public string Email { get; set; }
    }

    public class ProfileController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly INotificationService _notificationService;

        // 2. Adiciona ao construtor
        public ProfileController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, INotificationService notificationService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _notificationService = notificationService;
        }

        /// <summary>
        /// GET: Profile/LoginForm - Displays login form
        /// Redirects authenticated users to Home/Index
        /// </summary>
        [HttpGet]
        public IActionResult LoginForm()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        // Recebe os dados do formulário 
        [HttpPost]
        public async Task<IActionResult> LoginForm(string email, string password, bool rememberMe)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return Json(new { success = false, message = "Please fill in all fields." });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !UserHashHelpers.VerifyPassword(password, user.PasswordHash))
            {
                return Json(new { success = false, message = "Invalid email or password." });
            }

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

            return Json(new { success = true });
        }

        // Mostra a página de Registo 
        [HttpGet]
        public IActionResult SignUpForm()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        public IActionResult ForgotPasswordForm()
        {
            return View();
        }

        // Faz o Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }


        // sprint 2 -----------

        // Verificação do Email
        [HttpPost]
        public async Task<IActionResult> ValidateEmail([FromBody] EmailCheckRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return Json(new { isAvailable = false });

            bool emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());
            return Json(new { isAvailable = !emailExists });
        }

        // Verificaçõesdo sign up e criação de conta
        [HttpPost]
        public async Task<IActionResult> ValidateSignUp([FromBody] SignUpValidationModel model)
        {
            var errors = new Dictionary<string, string>();

            // valida nome
            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                errors["fullName"] = "O nome é obrigatório.";
            }
            else if (!IsValidName(model.FullName))
            {
                errors["fullName"] = "Name must not contain numbers or symbols.";
            }

            // valida email
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

            // valida telemóvel
            if (string.IsNullOrWhiteSpace(model.Phone))
            {
                errors["phone"] = "Phone number is required.";
            }

            // valida password
            var passwordErrors = ValidatePassword(model.Password);
            if (passwordErrors.Any())
            {
                errors["password"] = string.Join(" ", passwordErrors);
            }

            // valida confirmação de password
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
                PasswordHash = UserHashHelpers.HashPassword(model.Password),
                Role = role,
                IsVerified = false, // Requer verificação do admin para Associações e PetSitters
                Phone = model.Phone,
                CreatedAt = DateTime.Now,
                ProfilePicture = "/images/default-avatar.jpg"
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
        [HttpGet]
        public async Task<IActionResult> AccountSettings()
        {
            // 1. Descobrir quem é o utilizador logado
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = int.Parse(userIdClaim);

            // 2. Ir buscar TODOS os dados dele à Base de Dados
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // 3. Enviar o utilizador carregado para a View!
            return View(user);
        }

        // My profile

        // GET: Profile/MyProfile
        [Authorize]
        public async Task<IActionResult> MyProfile()
        {
            // Obtem o ID do utilizador logado
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Challenge();
            }

            // Vai buscar o utilizador
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            // Vai buscar saved pets
            var savedPets = await _context.FavoritePets
                .Where(f => f.UserId == userId)
                .Include(f => f.AnimalListing)
                .Select(f => f.AnimalListing)
                .Where(a => a.Status == ListingStatus.Published)
                .ToListAsync();

            // vai buscar active applications 
            var activeApplications = await _context.Applications
                .Where(a => a.UserId == userId &&
                       (a.Status == ApplicationStatus.Pending || a.Status == ApplicationStatus.Approved))
                .Include(a => a.AnimalListing)
                .ThenInclude(a => a.Tutor)
                .OrderByDescending(a => a.SubmittedAt)
                .Take(3)
                .ToListAsync();

            // mensagens recentes
            var recentConversations = new List<Message>();

            var allMessages = await _context.Messages
            .Where(m => m.ReceiverId == userId || m.SenderId == userId)
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

            // Agrupar por conversa 
            var conversations = allMessages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g => g.OrderByDescending(m => m.CreatedAt).FirstOrDefault())
            .OrderByDescending(m => m.CreatedAt)
            .Take(3)
            .ToList();

            recentConversations = conversations;

            // Calcular estatísticas
            var totalApplications = await _context.Applications
                .CountAsync(a => a.UserId == userId);

            var unreadMessages = await _context.Messages
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);

            var daysSinceJoined = (int)(DateTime.Now - user.CreatedAt).TotalDays;

            var viewModel = new ProfileViewModel
            {
                User = user,
                SavedPets = savedPets,
                ActiveApplications = activeApplications,
                RecentConversations = recentConversations,
                TotalApplications = totalApplications,
                UnreadMessages = unreadMessages,
                DaysSinceJoined = daysSinceJoined,
                RecentNotifications = await _notificationService.GetUserRecentNotificationsAsync(userId, 5)
            };

            return View(viewModel);
        }

        // POST: Profile/MarkNotificationAsRead
        [HttpPost]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            await _notificationService.MarkAsReadAsync(notificationId);
            return RedirectToAction(nameof(MyProfile));  // This reloads the page
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllNotificationAsRead(int userId)
        {
            await _notificationService.MarkAllAsReadAsync(userId);
            return RedirectToAction(nameof(MyProfile));  // This reloads the page
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccount(User updatedUser, IFormFile? profilePicture, bool removePhoto)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Challenge();
            int currentUserId = int.Parse(userIdClaim);

            // 1. Buscar o utilizador original para garantir que editamos o nosso próprio perfil
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (userInDb == null) return NotFound();

            // 2. Atualizar apenas os campos permitidos
            userInDb.Name = updatedUser.Name;
            userInDb.Phone = updatedUser.Phone;
            userInDb.Location = updatedUser.Location;
            userInDb.Bio = updatedUser.Bio;

            // 3. Lógica de Foto
            if (removePhoto)
            {
                if (!string.IsNullOrEmpty(userInDb.ProfilePicture) && !userInDb.ProfilePicture.Contains("default-avatar.jpg"))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, userInDb.ProfilePicture.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }
                userInDb.ProfilePicture = "/images/default-avatar.jpg";
            }
            else if (profilePicture != null && profilePicture.Length > 0)
            {
                // Se já tinha foto, apaga a antiga antes de subir a nova para não acumular lixo
                if (!string.IsNullOrEmpty(userInDb.ProfilePicture) && !userInDb.ProfilePicture.Contains("default-avatar.jpg"))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, userInDb.ProfilePicture.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }
                userInDb.ProfilePicture = await SaveProfileFile(profilePicture);
            }

            _context.Users.Update(userInDb);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(AccountSettings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProfilePicture()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Challenge();

            int userId = int.Parse(userIdClaim);
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound();

            // 1. Se a foto atual não for a default, vamos apagar o ficheiro físico
            if (!string.IsNullOrEmpty(user.ProfilePicture) && !user.ProfilePicture.Contains("default-avatar.jpg"))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicture.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // 2. Voltamos o caminho para a foto default ou null
            user.ProfilePicture = "/images/default-avatar.jpg";

            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile picture removed successfully!";
            return RedirectToAction(nameof(AccountSettings));
        }

        private async Task<string> SaveProfileFile(IFormFile file)
        {
            // 1. Criar um nome único (Ex: 550e8400-e29b-41d4.jpg)
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            // 2. Definir o caminho da pasta: wwwroot/images/avatars
            string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images", "avatars");

            // 3. Garantir que a pasta existe
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            string filePath = Path.Combine(uploadDir, fileName);

            // 4. Guardar o ficheiro no disco
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 5. Retornar o caminho relativo para a Base de Dados
            return $"/images/avatars/{fileName}";
        }

        public IActionResult HelpCenter()
        {
            return View();
        }
    }
}