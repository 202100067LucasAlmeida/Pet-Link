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
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PetLink.Controllers
{
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
            // Procura o utilizador na base de dados
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == password);

            if (user == null)
            {
                // Mostra erro na página se não encontrar
                ViewBag.Error = "Email ou password inválidos.";
                return View();
            }

            // Cria o "Cartão de Cidadão" virtual (Claims) do utilizador
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()), // Define se é Admin, PetSitter, etc.
                new Claim("UserId", user.Id.ToString()),
                new Claim("IsVerified", user.IsVerified.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = rememberMe };

            // Efetua o login (Cria o Cookie no navegador)
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("Index", "Home");
        }

        // 3. Mostra a página de Registo 
        [HttpGet]
        public IActionResult SignUpForm()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: Recebe os dados do formulário de registo e cria a conta
        [HttpPost]
        public async Task<IActionResult> SignUpForm(string fullName, string email, string phone, string password, string confirmPassword, string userType)
        {
            var errors = new List<string>();

            if (!IsValidName(fullName))
            {
                errors.Add("O nome não deve ter números nem símbolos.");
            }

            if (!IsValidEmail(email))
            {
                errors.Add("O email deve conter um @ e ser válido.");
            }

            if (password != confirmPassword)
            {
                errors.Add("As passwords não coincidem.");
            }

            var passwordErrors = ValidatePassword(password);
            errors.AddRange(passwordErrors);

            if (errors.Any())
            {
                ViewBag.Error = string.Join(" ", errors);
                // Preserve form data
                ViewBag.FullName = fullName;
                ViewBag.Email = email;
                ViewBag.Phone = phone;
                ViewBag.UserType = userType;
                return View();
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                ViewBag.Error = "Este email já está registado.";
                ViewBag.FullName = fullName;
                ViewBag.Email = email;
                ViewBag.Phone = phone;
                ViewBag.UserType = userType;
                return View();
            }

            UserRole role = UserRole.User;
            if (userType == "PetSitter") role = UserRole.PetSitter;
            if (userType == "Shelter") role = UserRole.Shelter;
            if (userType == "User") role = UserRole.User;

            var newUser = new PetLink.Models.User
            {
                Name = fullName,
                Email = email,
                PasswordHash = password,
                Role = role,
                IsVerified = false // Requer verificação do admin para Associações e PetSitters
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Conta criada com sucesso! Podes fazer login.";
            return RedirectToAction("LoginForm");
        }

        // 4. Faz o Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }



        //sprint 2 -----------

        //verificações pedidas do sign up
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
                errors["fullName"] = "O nome não deve ter números nem símbolos.";
            }

            // Validate Email
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                errors["email"] = "O email é obrigatório.";
            }
            else if (!IsValidEmail(model.Email))
            {
                errors["email"] = "O email deve conter um @ e ser válido.";
            }
            else
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    errors["email"] = "Este email já está registado.";
                }
            }

            // Validate Phone
            if (string.IsNullOrWhiteSpace(model.Phone))
            {
                errors["phone"] = "O telefone é obrigatório.";
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
                errors["confirmPassword"] = "As passwords não coincidem.";
            }

            if (errors.Any())
            {
                return Json(new { success = false, errors });
            }

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


        //métodos extra
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
                errors.Add("A password é obrigatória.");
                return errors;
            }

            if (password.Length < 6)
                errors.Add("A password deve ter pelo menos 6 caracteres.");

            if (!password.Any(char.IsLower))
                errors.Add("A password deve ter pelo menos uma letra minúscula.");

            if (!password.Any(char.IsUpper))
                errors.Add("A password deve ter pelo menos uma letra maiúscula.");

            if (!password.Any(char.IsDigit))
                errors.Add("A password deve ter pelo menos um número.");

            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                errors.Add("A password deve ter pelo menos um símbolo.");

            return errors;
        }

        #region Favorites
        // GET: Profile/MyFavorites
        [Authorize]
        public async Task<IActionResult> MyFavorites()
        {
            // Get current user ID from claims
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Challenge();
            }

            var favoriteListings = await _context.FavoritePets
                .Where(f => f.UserId == userId)
                .Include(f => f.AnimalListing)
                .Select(f => f.AnimalListing)
                .Where(a => a.Status == ListingStatus.Published)
                .ToListAsync();

            return View(favoriteListings);
        }

        // POST: Profile/ToggleFavorite
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int animalListingId)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Verificar se o animal listing existe e está publicado
                var animalListing = await _context.AnimalListings
                    .FirstOrDefaultAsync(a => a.Id == animalListingId && a.Status == ListingStatus.Published);

                if (animalListing == null)
                {
                    return Json(new { success = false, message = "Animal listing not available" });
                }

                var existingFavorite = await _context.FavoritePets
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.AnimalListingId == animalListingId);

                if (existingFavorite != null)
                {
                    _context.FavoritePets.Remove(existingFavorite);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, isFavorited = false, message = "Removed from favorites" });
                }
                else
                {
                    var favorite = new FavoritePet
                    {
                        UserId = userId,
                        AnimalListingId = animalListingId,
                        CreatedAt = DateTime.Now
                    };
                    _context.FavoritePets.Add(favorite);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, isFavorited = true, message = "Added to favorites" });
                }
            }
            catch
            {
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        // GET: Profile/IsFavorited
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> IsFavorited(int animalListingId)
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Json(false);
            }

            var isFavorited = await _context.FavoritePets
                .AnyAsync(f => f.UserId == userId && f.AnimalListingId == animalListingId);

            return Json(isFavorited);
        }

        #endregion
    

    }
}