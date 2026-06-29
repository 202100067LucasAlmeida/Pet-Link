using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using PetLink.Data;
using PetLink.Hubs;
using PetLink.Models;
using PetLink.Models.Enums;
using PetLink.Models.ViewModels;
using PetLink.Services;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace PetLink.Controllers
{
    /// <summary>
    /// Modelo utilizado para receber pedidos de verificação de email via JSON.
    /// </summary>
    public class EmailCheckRequest
    {
        public string Email { get; set; }
    }

    /// <summary>
    /// Controlador responsável pela gestão de perfis e autenticação de utilizadores.
    /// Cobre o registo, login, logout, autenticação via Google,
    /// recuperação de password, definições de conta e painel de perfil.
    /// </summary>
    public class ProfileController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        /// <summary>
        /// Inicializa uma nova instância do controlador de perfil.
        /// </summary>
        /// <param name="context">Contexto da base de dados.</param>
        /// <param name="webHostEnvironment">Ambiente de execução da aplicação web.</param>
        /// <param name="notificationService">Serviço responsável pelo envio de notificações.</param>
        /// <param name="emailService">Serviço responsável pelo envio de emails.</param>
        public ProfileController(ApplicationDbContext context, 
                                 IWebHostEnvironment webHostEnvironment, 
                                 INotificationService notificationService, 
                                 IEmailService emailService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        /// <summary>
        /// Apresenta o formulário de início de sessão.
        /// Utilizadores já autenticados são redirecionados para a página inicial.
        /// </summary>
        /// <returns>Vista do formulário de login ou redirecionamento.</returns>
        [HttpGet]
        public IActionResult LoginForm()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        /// <summary>
        /// Processa o início de sessão com email e password.
        /// Em caso de sucesso, cria o cookie de autenticação.
        /// Devolve o resultado em formato JSON.
        /// </summary>
        /// <param name="email">Email do utilizador.</param>
        /// <param name="password">Password do utilizador.</param>
        /// <param name="rememberMe">Indica se a sessão deve ser persistente.</param>
        /// <returns>Resposta JSON com o resultado da autenticação.</returns>
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

        /// <summary>
        /// Apresenta o formulário de registo de nova conta.
        /// Utilizadores já autenticados são redirecionados para a página inicial.
        /// </summary>
        /// <returns>Vista do formulário de registo ou redirecionamento.</returns>
        [HttpGet]
        public IActionResult SignUpForm()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        /// <summary>
        /// Apresenta o formulário de recuperação de password.
        /// </summary>
        /// <returns>Vista do formulário de recuperação de password.</returns>  
        [HttpGet]
        public IActionResult ForgotPasswordForm()
        {
            return View();
        }

        /// <summary>
        /// Termina a sessão do utilizador autenticado e redireciona para a página inicial.
        /// </summary>
        /// <returns>Redirecionamento para a página inicial.</returns>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }


        /// <summary>
        /// Verifica se um endereço de email já se encontra registado na plataforma.
        /// Devolve o resultado em formato JSON.
        /// </summary>
        /// <param name="request">Objeto com o email a verificar.</param>
        /// <returns>Resposta JSON com a propriedade <c>isAvailable</c> a indicar a disponibilidade do email.</returns>
        [HttpPost]
        public async Task<IActionResult> ValidateEmail([FromBody] EmailCheckRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return Json(new { isAvailable = false });

            bool emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());
            return Json(new { isAvailable = !emailExists });
        }

        /// <summary>
        /// Valida os dados do formulário de registo e cria uma nova conta de utilizador.
        /// Notifica os administradores após a criação bem-sucedida.
        /// Devolve o resultado em formato JSON.
        /// </summary>
        /// <param name="model">Dados submetidos no formulário de registo.</param>
        /// <returns>Resposta JSON com o resultado da validação e criação de conta.</returns>
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
                IsVerified = false,
                Phone = model.Phone,
                CreatedAt = DateTime.Now,
                ProfilePicture = "/images/default-avatar.jpg"
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            await _notificationService.CreateNewUserNotificationForAdminsAsync(
                newUser.Id,
                newUser.Name,
                newUser.Email,
                newUser.Role
            );

            return Json(new { success = true });
        }

        /// <summary>
        /// Valida os requisitos de complexidade de uma password.
        /// Devolve um objeto JSON com o estado de cada requisito.
        /// </summary>
        /// <param name="model">Modelo com a password a validar.</param>
        /// <returns>Resposta JSON com os requisitos cumpridos e por cumprir.</returns>
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


        /// <summary>
        /// Apresenta as definições da conta do utilizador autenticado.
        /// </summary>
        /// <returns>Vista das definições de conta com os dados do utilizador.</returns>
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

        /// <summary>
        /// Apresenta o painel de perfil do utilizador autenticado.
        /// Inclui animais guardados, candidaturas ativas, conversas recentes,
        /// notificações, avaliações e, para administradores,
        /// dados de gestão pendentes como anúncios, utilizadores e eventos.
        /// </summary>
        /// <returns>Vista do perfil com o ViewModel preenchido.</returns>
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

            List<AnimalListing> pendingListingsForAdmin = null;
            List<User> unverifiedUsersForAdmin = null;
            List<Event> pendingEventsForAdmin = null; 
            if (User.IsInRole("Admin"))
            {
                pendingListingsForAdmin = await _context.AnimalListings
                    .Include(a => a.Tutor)
                    .Where(a => a.Status == ListingStatus.Pending)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                unverifiedUsersForAdmin = await _context.Users
                    //.Where(u => !u.IsVerified && u.Role == UserRole.Shelter)
                    .Where(u => !u.IsVerified)
                    .OrderByDescending(u => u.CreatedAt)
                    .ToListAsync();

                 pendingEventsForAdmin = await _context.Events
                    .Include(e => e.Organizer)
                    .Where(e => e.Status == EventStatus.Pending)
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();
            }

            var favoritePetsitters = await _context.FavoritePetsitters
                .Where(f => f.UserId == userId)
                .Include(f => f.Petsitter)
                    .ThenInclude(p => p.User)
                .Select(f => f.Petsitter)
                .ToListAsync();

            var viewModel = new ProfileViewModel
            {
                User = user,
                SavedPets = savedPets,
                ActiveApplications = activeApplications,
                RecentConversations = recentConversations,
                TotalApplications = totalApplications,
                UnreadMessages = unreadMessages,
                DaysSinceJoined = daysSinceJoined, 
                RecentNotifications = await _notificationService.GetUserRecentNotificationsAsync(userId, 5),
                PendingListingsForAdmin = pendingListingsForAdmin,
                UnverifiedUsersForAdmin = unverifiedUsersForAdmin,
                FavoritePetsitters = favoritePetsitters,
                PendingEventsForAdmin = pendingEventsForAdmin,
            };

            // Buscar reviews apenas se o user pode receber avaliações (User ou PetSitter)
            var averageRating = 0.0;
            var totalReviews = 0;
            var canReceiveReviews = (user.Role == UserRole.User || user.Role == UserRole.PetSitter);

            if (canReceiveReviews)
            {
                var reviews = await _context.Reviews
                    .Where(r => r.ReviewedId == userId && r.IsApproved)
                    .ToListAsync();
                
                totalReviews = reviews.Count;
                averageRating = totalReviews > 0 ? Math.Round(reviews.Average(r => r.Rating), 1) : 0;
            }

            

            ViewBag.AverageRating = averageRating;
            ViewBag.TotalReviews = totalReviews;
            ViewBag.CanReceiveReviews = canReceiveReviews;

            return View(viewModel);
        }

        /// <summary>
        /// Marca uma notificação específica como lida.
        /// </summary>
        /// <param name="notificationId">Identificador da notificação.</param>
        /// <returns>Redireciona para o painel de perfil.</returns>
        [HttpPost]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            await _notificationService.MarkAsReadAsync(notificationId);
            return RedirectToAction(nameof(MyProfile));
        }

        /// <summary>
        /// Marca todas as notificações de um utilizador como lidas.
        /// </summary>
        /// <param name="userId">Identificador do utilizador.</param>
        /// <returns>Redireciona para o painel de perfil.</returns>

        [HttpPost]
        public async Task<IActionResult> MarkAllNotificationAsRead(int userId)
        {
            await _notificationService.MarkAllAsReadAsync(userId);
            return RedirectToAction(nameof(MyProfile));
        }

        /// <summary>
        /// Marca todas as mensagens não lidas do utilizador autenticado como lidas.
        /// Acessível apenas a utilizadores com os papéis User ou PetSitter.
        /// Devolve o resultado em formato JSON.
        /// </summary>
        /// <returns>Resposta JSON com o resultado da operação.</returns>// POST: Profile/MarkMessagesAsRead
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

        /// <summary>
        /// Atualiza os dados da conta do utilizador autenticado,
        /// incluindo nome, telefone, cidade, biografia e foto de perfil.
        /// Caso solicitado, remove a foto de perfil atual e repõe a imagem por defeito.
        /// </summary>
        /// <param name="updatedUser">Dados atualizados do utilizador.</param>
        /// <param name="profilePicture">Nova foto de perfil (opcional).</param>
        /// <param name="removePhoto">Indica se a foto de perfil deve ser removida.</param>
        /// <returns>Redireciona para as definições de conta após guardar.</returns>
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
            userInDb.City = updatedUser.City;
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

        /// <summary>
        /// Remove a foto de perfil do utilizador autenticado,
        /// apaga o ficheiro físico do servidor e repõe a imagem por defeito.
        /// </summary>
        /// <returns>Redireciona para as definições de conta após a remoção.</returns>
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

        /// <summary>
        /// Guarda um ficheiro de imagem de perfil no servidor
        /// e devolve o respetivo caminho relativo.
        /// </summary>
        /// <param name="file">Ficheiro de imagem enviado pelo utilizador.</param>
        /// <returns>Caminho relativo da imagem guardada.</returns>
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

        /// <summary>
        /// Inicia o fluxo de autenticação via Google,
        /// redirecionando o utilizador para a página de login da Google.
        /// </summary>
        /// <returns>Desafio de autenticação com o esquema Google.</returns>
        public IActionResult LoginWithGoogle()
        {
            var redirectUrl = Url.Action("GoogleResponse");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme); 
        }

        /// <summary>
        /// Processa a resposta de autenticação da Google.
        /// Caso o utilizador não exista na plataforma, é criada uma nova conta automaticamente.
        /// Em caso de sucesso, cria o cookie de autenticação da aplicação.
        /// </summary>
        /// <returns>Redireciona para a página inicial em caso de sucesso, ou para o login em caso de falha.</returns>
        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                return RedirectToAction("LoginForm");
            }

            var claims = result.Principal.Identities
                .FirstOrDefault()?.Claims;

            var email = claims?
                .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            var name = claims?
                .FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("LoginForm");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                user = new User
                {
                    Name = name ?? "Google User",
                    Email = email,
                    Phone = null,
                    PasswordHash = null,
                    Role = UserRole.User,
                    IsVerified = true,
                    IsExternalLogin = true,
                    CreatedAt = DateTime.Now,
                    ProfilePicture = "/images/default-avatar.jpg"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            var appClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim("UserId", user.Id.ToString())
                };

            var identity = new ClaimsIdentity(
                appClaims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Processa o pedido de recuperação de password.
        /// Gera um token de reset, guarda-o na base de dados e envia um email com o link de redefinição.
        /// Contas com login externo via Google não suportam este fluxo.
        /// Devolve o resultado em formato JSON.
        /// </summary>
        /// <param name="model">Modelo com o email para recuperação de password.</param>
        /// <returns>Resposta JSON com o resultado da operação.</returns>
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                return Json(new
                {
                    success = true
                });
            }

            if (user.IsExternalLogin)
            {
                return Json(new
                {
                    success = false,
                    message = "This account uses Google login. Please reset your password via Google."
                });
            }

            user.PasswordResetToken = Guid.NewGuid().ToString();

            user.PasswordResetTokenExpiry =
                DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            var resetLink =
                Url.Action(
                    "ResetPassword",
                    "Profile",
                    new { token = user.PasswordResetToken },
                    Request.Scheme);

            Console.WriteLine("RESET LINK: " + resetLink);

            await _emailService.SendEmailAsync(
                user.Email,
                "Reset your password",
                $@"
                <h2>Password Recovery</h2>
                <p>Click the link below:</p>
                <a href='{resetLink}'>Reset Password</a>
                ");

            return Json(new
            {
                success = true
            });
        }

        /// <summary>
        /// Processa o pedido de recuperação de password.
        /// Gera um token de reset, guarda-o na base de dados e envia um email com o link de redefinição.
        /// Contas com login externo via Google não suportam este fluxo.
        /// Devolve o resultado em formato JSON.
        /// </summary>
        /// <param name="model">Modelo com o email para recuperação de password.</param>
        /// <returns>Resposta JSON com o resultado da operação.</returns>
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("LoginForm");

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.PasswordResetToken == token &&
                    u.PasswordResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
                return BadRequest("Invalid or expired token");

            return View(new ResetPasswordViewModel
            {
                Token = token
            });
        }

        /// <summary>
        /// Processa a redefinição de password com base no token fornecido.
        /// Valida a correspondência das passwords e a validade do token
        /// antes de atualizar a password na base de dados.
        /// Devolve o resultado em formato JSON.
        /// </summary>
        /// <param name="model">Modelo com o token, nova password e confirmação.</param>
        /// <returns>Resposta JSON com o resultado da operação.</returns>
        [HttpPost]
        public async Task<IActionResult> ResetPassword(
    ResetPasswordViewModel model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                return Json(new
                {
                    success = false,
                    message = "Passwords do not match"
                });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.PasswordResetToken == model.Token);

            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid token"
                });
            }

            if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                return Json(new
                {
                    success = false,
                    message = "Token expired"
                });
            }

            user.PasswordHash =
                UserHashHelpers.HashPassword(model.Password);

            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true
            });
        }

        /// <summary>
        /// Apresenta a página de centro de ajuda da plataforma.
        /// </summary>
        /// <returns>Vista do centro de ajuda.</returns>
        public IActionResult HelpCenter()
        {
            return View();
        }
    }
}
