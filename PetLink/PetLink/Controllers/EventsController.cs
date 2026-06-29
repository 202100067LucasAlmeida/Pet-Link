using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using PetLink.Hubs; 
using PetLink.Models.Enums;
using PetLink.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PetLink.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão de eventos criados por abrigos.
    /// Permite criar, editar, aprovar, rejeitar e eliminar eventos,
    /// bem como gerir o interesse dos utilizadores e pesquisar eventos aprovados.
    /// </summary>
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Inicializa uma nova instância do controlador de eventos.
        /// </summary>
        /// <param name="context">Contexto da base de dados.</param>
        /// <param name="webHostEnvironment">Ambiente de execução da aplicação web.</param>
        /// <param name="notificationService">Serviço responsável pelo envio de notificações.</param>
        public EventsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, INotificationService notificationService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Apresenta a listagem pública de eventos aprovados,
        /// permitindo filtrar por localização, datas e tipo de evento.
        /// </summary>
        /// <param name="location">Localização para filtrar os eventos (opcional).</param>
        /// <param name="startDate">Data de início mínima para filtrar os eventos (opcional).</param>
        /// <param name="endDate">Data de fim máxima para filtrar os eventos (opcional).</param>
        /// <param name="type">Tipo de evento para filtrar (opcional).</param>
        /// <returns>Vista com a lista de eventos que correspondem aos filtros aplicados.</returns>
        public async Task<IActionResult> Index(string? location, DateTime? startDate, DateTime? endDate, EventType? type)
        {
            var query = _context.Events
                .Include(e => e.Organizer)
                .Where(e => e.Status == EventStatus.Approved);

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(e => e.Location.Contains(location));
                ViewBag.CurrentLocation = location;
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.StartDate >= startDate.Value);
                ViewBag.CurrentStartDate = startDate.Value.ToString("yyyy-MM-dd");
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.EndDate <= endDate.Value);
                ViewBag.CurrentEndDate = endDate.Value.ToString("yyyy-MM-dd");
            }

            if (type.HasValue)
            {
                query = query.Where(e => e.Type == type.Value);
                ViewBag.CurrentType = type.Value;
            }

            var events = await query
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            ViewBag.EventTypes = Enum.GetValues(typeof(EventType));
            return View(events);
        }

        /// <summary>
        /// Apresenta os detalhes de um evento.
        /// Eventos não aprovados só são visíveis pelo organizador ou por administradores.
        /// </summary>
        /// <param name="id">Identificador do evento.</param>
        /// <returns>Vista com os detalhes do evento.</returns>
        public async Task<IActionResult> Details(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null) return NotFound();

            // Se não estiver aprovado, só o organizador e admin podem ver
            if (eventItem.Status != EventStatus.Approved)
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                bool isAuthorized = false;
                
                if (userIdClaim != null)
                {
                    int userId = int.Parse(userIdClaim);
                    isAuthorized = eventItem.OrganizerId == userId || User.IsInRole("Admin");
                }
                
                if (!isAuthorized) return Forbid();
            }

            return View(eventItem);
        }

        /// <summary>
        /// Apresenta o formulário de criação de um novo evento.
        /// Apenas acessível a utilizadores com o papel de Shelter.
        /// </summary>
        /// <returns>Vista de criação de evento.</returns>
        [Authorize(Roles = "Shelter")]
        public IActionResult Create()
        {
            Console.WriteLine("=== CREATE GET ===");
 
            return View();
        }

        /// <summary>
        /// Cria um novo evento com estado pendente de aprovação.
        /// Efetua o upload da imagem, guarda o evento e notifica os administradores.
        /// Apenas acessível a utilizadores com o papel de Shelter.
        /// </summary>
        /// <param name="model">Dados do evento a criar.</param>
        /// <param name="imageFile">Imagem do evento (opcional).</param>
        /// <returns>Redireciona para os eventos do organizador caso a criação seja bem-sucedida.</returns>
        [HttpPost]
        [Authorize(Roles = "Shelter")]
        public async Task<IActionResult> Create(Event model, IFormFile? imageFile)
        {
            Console.WriteLine("=== CREATE EVENT POST ===");
            
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                Console.WriteLine("UserId not found!");
                return Challenge();
            }
            
            Console.WriteLine($"UserId: {userIdClaim}");
            Console.WriteLine($"Name: {model.Name}");
            Console.WriteLine($"Description: {model.Description}");
            Console.WriteLine($"StartDate: {model.StartDate}");
            Console.WriteLine($"Location: {model.Location}");
            Console.WriteLine($"Type: {model.Type}");
            Console.WriteLine($"ImageFile: {imageFile?.FileName}");
            
            // Remover validação de campos que não precisamos
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Organizer");
            ModelState.Remove("ApprovedBy");
            ModelState.Remove("ApprovedAt");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");
            
            // Verificar erros de validação
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }
                return View(model);
            }
            
            try
            {
                // Upload da imagem
                if (imageFile != null && imageFile.Length > 0)
                {
                    model.ImageUrl = await UploadImage(imageFile);
                    Console.WriteLine($"Image uploaded: {model.ImageUrl}");
                }
                else
                {
                    model.ImageUrl = "/images/default-event.jpg";
                    Console.WriteLine("Using default image");
                }

                model.OrganizerId = int.Parse(userIdClaim);
                model.CreatedAt = DateTime.Now;
                model.Status = EventStatus.Pending;

                _context.Events.Add(model);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"Event created with ID: {model.Id}");

                // Notificar admins (com try-catch para não falhar)
                try
                {
                    await _notificationService.CreateNewEventNotificationForAdminsAsync(
                        model.Id,
                        model.Name,
                        model.OrganizerId
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Notification error: {ex.Message}");
                }

                TempData["Success"] = "Event created successfully! It will be visible after admin approval.";
                return RedirectToAction(nameof(MyEvents));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating event: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["Error"] = $"Error: {ex.Message}";
                return View(model);
            }
        }

        /// <summary>
        /// Apresenta o formulário de edição de um evento existente.
        /// Apenas o organizador do evento ou um administrador podem aceder.
        /// </summary>
        /// <param name="id">Identificador do evento.</param>
        /// <returns>Vista de edição do evento.</returns>
        [Authorize(Roles = "Shelter,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null) return NotFound();

            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Challenge();

            int userId = int.Parse(userIdClaim);
            bool isAdmin = User.IsInRole("Admin");

            if (eventItem.OrganizerId != userId && !isAdmin) return Forbid();

            return View(eventItem);
        }

        /// <summary>
        /// Atualiza as informações de um evento existente.
        /// Quando editado pelo organizador, o evento regressa ao estado pendente de aprovação.
        /// Quando editado por um administrador, o estado pode ser alterado manualmente.
        /// </summary>
        /// <param name="id">Identificador do evento.</param>
        /// <param name="model">Dados atualizados do evento.</param>
        /// <param name="imageFile">Nova imagem do evento (opcional).</param>
        /// <returns>Redireciona para a lista correspondente após guardar.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Shelter,Admin")]
        public async Task<IActionResult> Edit(int id, Event model, IFormFile? imageFile)
        {
            Console.WriteLine("=== EDIT POST - CHAMADO ===");
    
            if (id != model.Id) return NotFound();

            var existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null) return NotFound();

            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Challenge();

            int userId = int.Parse(userIdClaim);
            bool isAdmin = User.IsInRole("Admin");

            if (existingEvent.OrganizerId != userId && !isAdmin) return Forbid();

            ModelState.Remove("ImageUrl");
            ModelState.Remove("Organizer");
            ModelState.Remove("ApprovedBy");
            ModelState.Remove("ApprovedAt");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");

            if (ModelState.IsValid)
            {
                try
                {
                    existingEvent.Name = model.Name;
                    existingEvent.Description = model.Description;
                    existingEvent.StartDate = model.StartDate;
                    existingEvent.EndDate = model.EndDate;
                    existingEvent.Location = model.Location;
                    existingEvent.Type = model.Type;
                    existingEvent.UpdatedAt = DateTime.Now;
                    existingEvent.AcceptsDonations = model.AcceptsDonations;
                    existingEvent.AcceptsVolunteers = model.AcceptsVolunteers;

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        existingEvent.ImageUrl = await UploadImage(imageFile);
                    }

                    // ========== REGRA IMPORTANTE ==========
                    // Se não for admin (ou seja, é o shelter a editar), o evento volta a Pending
                    // Se for admin a editar, mantém o status atual ou pode alterar manualmente
                    if (!isAdmin)
                    {
                        // Shelter a editar: evento volta para aprovação
                        existingEvent.Status = EventStatus.Pending;
                        existingEvent.ApprovedAt = null;
                        existingEvent.ApprovedBy = null;
                
                    }
                    else
                    {
                        // Admin a editar: pode alterar o status manualmente
                        if (existingEvent.Status != model.Status)
                        {
                            existingEvent.Status = model.Status;
                            existingEvent.ApprovedAt = model.Status == EventStatus.Approved ? DateTime.Now : null;
                            existingEvent.ApprovedBy = model.Status == EventStatus.Approved ? userId : null;
                        }
                    }

                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Event updated. New status: {existingEvent.Status}");

                    TempData["Success"] = !isAdmin 
                        ? "Event updated! It will be visible again after admin approval." 
                        : "Event updated successfully!";

                    return RedirectToAction(isAdmin ? nameof(Manage) : nameof(MyEvents));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    TempData["Error"] = $"Error: {ex.Message}";
                }
            }

            return View(model);
        }

        /// <summary>
        /// Lista todos os eventos criados pelo abrigo autenticado.
        /// Apenas acessível a utilizadores com o papel de Shelter.
        /// </summary>
        /// <returns>Vista com os eventos do organizador.</returns>
        [Authorize(Roles = "Shelter")]
        public async Task<IActionResult> MyEvents()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Challenge();

            int userId = int.Parse(userIdClaim);

            var events = await _context.Events
                .Where(e => e.OrganizerId == userId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return View(events);
        }

        /// <summary>
        /// Apresenta a área de gestão de eventos para administradores,
        /// separando os eventos pendentes de aprovação dos já aprovados.
        /// Apenas acessível a administradores.
        /// </summary>
        /// <returns>Vista de gestão com eventos pendentes e aprovados.</returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            var pendingEvents = await _context.Events
                .Include(e => e.Organizer)
                .Where(e => e.Status == EventStatus.Pending)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();

            var approvedEvents = await _context.Events
                .Include(e => e.Organizer)
                .Where(e => e.Status == EventStatus.Approved)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();

            ViewBag.PendingEvents = pendingEvents;
            ViewBag.ApprovedEvents = approvedEvents;

            return View();
        }

        /// <summary>
        /// Aprova um evento pendente e notifica o organizador.
        /// Apenas acessível a administradores.
        /// </summary>
        /// <param name="id">Identificador do evento.</param>
        /// <returns>Redireciona para a gestão de eventos.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null) return NotFound();

            eventItem.Status = EventStatus.Approved;
            eventItem.ApprovedAt = DateTime.Now;
            
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim != null)
            {
                eventItem.ApprovedBy = int.Parse(userIdClaim);
            }

            await _context.SaveChangesAsync();

            // Notificar organizador
            await _notificationService.CreateEventApprovalNotificationAsync(
                eventItem.OrganizerId,
                eventItem.Name,
                eventItem.Id,
                true
            );

            TempData["Success"] = $"Event '{eventItem.Name}' has been approved!";
            return RedirectToAction(nameof(Manage));
        }

        /// <summary>
        /// Rejeita um evento pendente e notifica o organizador,
        /// podendo incluir um motivo de rejeição.
        /// Apenas acessível a administradores.
        /// </summary>
        /// <param name="id">Identificador do evento.</param>
        /// <param name="rejectionReason">Motivo da rejeição (opcional).</param>
        /// <returns>Redireciona para a gestão de eventos.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id, string? rejectionReason)
        {
            var eventItem = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null) return NotFound();

            eventItem.Status = EventStatus.Rejected;
            await _context.SaveChangesAsync();

            // Notificar organizador
            await _notificationService.CreateEventApprovalNotificationAsync(
                eventItem.OrganizerId,
                eventItem.Name,
                eventItem.Id,
                false,
                rejectionReason
            );

            TempData["Success"] = $"Event '{eventItem.Name}' has been rejected.";
            return RedirectToAction(nameof(Manage));
        }

        /// <summary>
        /// Pesquisa eventos aprovados por nome, descrição ou localização.
        /// Devolve os resultados em formato JSON para utilização em pesquisas rápidas.
        /// </summary>
        /// <param name="query">Texto a pesquisar.</param>
        /// <returns>Lista JSON com os eventos correspondentes (máximo de 10 resultados).</returns>
        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            var events = await _context.Events
                .Where(e => e.Status == EventStatus.Approved)
                .Where(e => e.Name.Contains(query) || 
                           e.Description.Contains(query) || 
                           e.Location.Contains(query))
                .Take(10)
                .Select(e => new { e.Id, e.Name, e.Location, e.StartDate })
                .ToListAsync();

            return Json(events);
        }

        /// <summary>
        /// Remove definitivamente um evento.
        /// Apenas acessível a administradores.
        /// </summary>
        /// <param name="id">Identificador do evento.</param>
        /// <returns>Redireciona para a gestão de eventos.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            try
            {
                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Event '{eventItem.Name}' has been deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting event: {ex.Message}";
            }

            return RedirectToAction(nameof(Manage));
        }

        /// <summary>
        /// Guarda uma imagem no servidor e devolve o respetivo caminho relativo.
        /// </summary>
        /// <param name="file">Imagem enviada pelo utilizador.</param>
        /// <returns>Caminho relativo da imagem guardada.</returns>
        private async Task<string> UploadImage(IFormFile file)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "events");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            string filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/events/{fileName}";
        }

        /// <summary>
        /// Regista ou remove o interesse de um utilizador num evento aprovado.
        /// Caso o utilizador já esteja registado, o interesse é removido (comportamento de toggle).
        /// Notifica o organizador sempre que um novo interesse é registado.
        /// </summary>
        /// <param name="id">Identificador do evento.</param>
        /// <returns>Resposta JSON indicando o resultado da operação.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RegisterInterest(int id)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            int userId = int.Parse(userIdClaim);

            // Verificar se o evento existe e está aprovado
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id && e.Status == EventStatus.Approved);

            if (eventItem == null)
            {
                return Json(new { success = false, message = "Event not found or not available" });
            }

            // Verificar se já está registado
            var existingInterest = await _context.EventInterests
                .FirstOrDefaultAsync(ei => ei.EventId == id && ei.UserId == userId);

            if (existingInterest != null)
            {
                // Se já estiver registado, remover (toggle)
                _context.EventInterests.Remove(existingInterest);
                await _context.SaveChangesAsync();
                return Json(new { success = true, registered = false, message = "Interest removed" });
            }

            // Registrar interesse
            var interest = new EventInterest
            {
                EventId = id,
                UserId = userId,
                RegisteredAt = DateTime.Now,
                IsConfirmed = false
            };

            _context.EventInterests.Add(interest);
            await _context.SaveChangesAsync();

            // Notificar o organizador
            await _notificationService.CreateNewEventInterestNotificationAsync(
                eventItem.OrganizerId,
                eventItem.Name,
                id,
                userId,
                User.Identity.Name
            );

            return Json(new { success = true, registered = true, message = "You are now registered for this event!" });
        }

        /// <summary>
        /// Lista os utilizadores que registaram interesse num evento.
        /// Apenas acessível pelo organizador do evento ou por administradores.
        /// </summary>
        /// <param name="id">Identificador do evento.</param>
        /// <returns>Vista com a lista de utilizadores interessados.</returns>
        [HttpGet]
        public async Task<IActionResult> InterestedUsers(int id)
        {
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null) return NotFound();

            // Verificar se o utilizador atual é o organizador ou admin
            var userIdClaim = User.FindFirst("UserId")?.Value;
            bool isOrganizer = userIdClaim != null && eventItem.OrganizerId.ToString() == userIdClaim;
            bool isAdmin = User.IsInRole("Admin");

            if (!isOrganizer && !isAdmin)
            {
                return Forbid();
            }

            var interestedUsers = await _context.EventInterests
                .Where(ei => ei.EventId == id)
                .Include(ei => ei.User)
                .OrderByDescending(ei => ei.RegisteredAt)
                .ToListAsync();

            ViewBag.EventName = eventItem.Name;
            return View(interestedUsers);
        }

        /// <summary>
        /// Verifica se o utilizador autenticado tem interesse registado num evento.
        /// Devolve o resultado em formato JSON.
        /// </summary>
        /// <param name="id">Identificador do evento.</param>
        /// <returns>Resposta JSON com a propriedade <c>registered</c> a indicar o estado do interesse.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CheckInterest(int id)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Json(new { registered = false });
            }

            int userId = int.Parse(userIdClaim);

            var registered = await _context.EventInterests
                .AnyAsync(ei => ei.EventId == id && ei.UserId == userId);

            return Json(new { registered = registered });
        }
    }
}