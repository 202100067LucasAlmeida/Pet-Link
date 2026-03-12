using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using PetLink.Data;
using Microsoft.EntityFrameworkCore;
using PetLink.Models.Enums;

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
            //confirma o nome
            if (!IsValidName(fullName))
            {
                ViewBag.Error = "O nome não deve ter números nem simbolos.";
                return View();
            }


            // Valida se as passwords coincidem
            if (password != confirmPassword)
            {
                ViewBag.Error = "As passwords não coincidem.";
                return View();
            }

            // Verifica se o email já existe na base de dados
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                ViewBag.Error = "Este email já está registado.";
                return View();
            }

            // Mapea o tipo de utilizador (userType do HTML para o nosso Enum UserRole)

            UserRole role = UserRole.User;

            if (userType == "PetSitter") role = UserRole.PetSitter;
            if (userType == "Associacao") role = UserRole.Shelter;
            if(userType == "User") role = UserRole.User;
            
            // Cria o novo objeto User
            var newUser = new PetLink.Models.User
            {
                Name = fullName,
                Email = email,
                PasswordHash = password,
                Role = role,
                IsVerified = false // Requer verificação do admin para Associações e PetSitters
            };

            //Guarda na base de dados o user
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Redireciona para o Login com uma mensagem de sucesso
            TempData["SuccessMessage"] = "Conta criada com sucesso! Podes fazer login.";
            return RedirectToAction("LoginForm");
        }

        // Verifica se o nome contém apenas letras, espaços ou caracteres válidos
        private bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[\p{L}\s\-']+$");
        }

        // 4. Faz o Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}