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
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly INotificationService _notificationService;

        public EventsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, INotificationService notificationService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _notificationService = notificationService;
        }

        // GET: Events/Index - Listagem pública de eventos aprovados
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

        // GET: Events/Details/5
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

        // GET: Events/Create
        [Authorize(Roles = "Shelter")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Shelter")]
        public async Task<IActionResult> Create(Event model, IFormFile? imageFile)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Challenge();

            if (ModelState.IsValid)
            {
                // Upload da imagem
                if (imageFile != null && imageFile.Length > 0)
                {
                    model.ImageUrl = await UploadImage(imageFile);
                }
                else
                {
                    model.ImageUrl = "/images/default-event.jpg";
                }

                model.OrganizerId = int.Parse(userIdClaim);
                model.CreatedAt = DateTime.Now;
                model.Status = EventStatus.Pending;

                _context.Events.Add(model);
                await _context.SaveChangesAsync();

                // Notificar admins sobre novo evento pendente
                await _notificationService.CreateNewEventNotificationForAdminsAsync(
                    model.Id,
                    model.Name,
                    model.OrganizerId
                );

                TempData["Success"] = "Event created successfully! It will be visible after admin approval.";
                return RedirectToAction(nameof(MyEvents));
            }

            return View(model);
        }

        // GET: Events/Edit/5
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

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Shelter,Admin")]
        public async Task<IActionResult> Edit(int id, Event model, IFormFile? imageFile)
        {
            if (id != model.Id) return NotFound();

            var existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null) return NotFound();

            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Challenge();

            int userId = int.Parse(userIdClaim);
            bool isAdmin = User.IsInRole("Admin");

            if (existingEvent.OrganizerId != userId && !isAdmin) return Forbid();

            if (ModelState.IsValid)
            {
                existingEvent.Name = model.Name;
                existingEvent.Description = model.Description;
                existingEvent.StartDate = model.StartDate;
                existingEvent.EndDate = model.EndDate;
                existingEvent.Location = model.Location;
                existingEvent.Type = model.Type;
                existingEvent.UpdatedAt = DateTime.Now;

                if (imageFile != null && imageFile.Length > 0)
                {
                    existingEvent.ImageUrl = await UploadImage(imageFile);
                }

                // Se for admin, pode alterar o status
                if (isAdmin && existingEvent.Status != model.Status)
                {
                    existingEvent.Status = model.Status;
                    existingEvent.ApprovedAt = model.Status == EventStatus.Approved ? DateTime.Now : null;
                    existingEvent.ApprovedBy = model.Status == EventStatus.Approved ? userId : null;
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Event updated successfully!";
                return RedirectToAction(isAdmin ? nameof(Manage) : nameof(MyEvents));
            }

            return View(model);
        }

        // GET: Events/MyEvents - Eventos criados pela shelter
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

        // GET: Events/Manage - Admin apenas
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

        // POST: Events/Approve/5
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

        // POST: Events/Reject/5
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

        // GET: Events/Search - API para pesquisa rápida
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
    }
}