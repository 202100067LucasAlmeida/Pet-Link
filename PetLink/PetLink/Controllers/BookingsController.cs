using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using PetLink.Models.Enums;

namespace PetLink.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão de reservas de serviços de pet sitting.
    /// Permite criar, confirmar, rejeitar, cancelar e concluir reservas,
    /// bem como consultar as reservas do utilizador e gerir as reservas recebidas.
    /// </summary>
    [Authorize]
    public class BookingsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Inicializa uma nova instância do controlador de reservas.
        /// </summary>
        /// <param name="context">Contexto da base de dados.</param>
        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Cria uma nova reserva de serviço de pet sitting.
        /// Valida as datas, calcula o preço total e envia uma mensagem
        /// automática ao pet sitter com os detalhes da reserva.
        /// </summary>
        /// <param name="petsitterId">Identificador do pet sitter.</param>
        /// <param name="serviceType">Tipo de serviço pretendido.</param>
        /// <param name="startDate">Data e hora de início do serviço.</param>
        /// <param name="endDate">Data e hora de fim do serviço.</param>
        /// <param name="petName">Nome do animal (opcional).</param>
        /// <param name="petSpecies">Espécie do animal (opcional).</param>
        /// <param name="message">Mensagem adicional para o pet sitter (opcional).</param>
        /// <returns>Redireciona para os detalhes do pet sitter após a criação da reserva.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int petsitterId, ServiceType serviceType, DateTime startDate, DateTime endDate, string? petName, string? petSpecies, string? message)
        {
            if (!GetCurrentUserId(out int userId))
                return Challenge();

            var petsitter = await _context.Petsitters
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == petsitterId);

            if (petsitter == null)
                return NotFound();

            if (startDate < DateTime.Today)
            {
                TempData["Error"] = "Start date cannot be in the past.";
                return RedirectToAction("Details", "Petsitter", new { id = petsitterId });
            }

            if (endDate <= startDate)
            {
                TempData["Error"] = "End date must be after start date.";
                return RedirectToAction("Details", "Petsitter", new { id = petsitterId });
            }

            var hours = (decimal)(endDate - startDate).TotalHours;
            var totalPrice = hours * petsitter.HourlyRate;

            var booking = new Booking
            {
                UserId = userId,
                PetsitterId = petsitterId,
                ServiceType = serviceType,
                StartDate = startDate,
                EndDate = endDate,
                PetName = petName,
                PetSpecies = petSpecies,
                Message = message,
                TotalPrice = totalPrice,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now
            };

            _context.Bookings.Add(booking);

            var chatMessage = new Message
            {
                SenderId = userId,
                ReceiverId = petsitter.UserId,
                Content = $"[Booking Request] Hello! I'd like to book your services.\n\nService: {serviceType}\nDates: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}\nPet: {petName ?? "Not specified"} ({petSpecies ?? "Not specified"})\nMessage: {message ?? "No message"}",
                Timestamp = DateTime.Now,
                IsRead = false
            };
            _context.Messages.Add(chatMessage);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking request sent! The sitter will review it shortly.";
            return RedirectToAction("Details", "Petsitter", new { id = petsitterId });
        }

        /// <summary>
        /// Lista todas as reservas efetuadas pelo utilizador autenticado.
        /// </summary>
        /// <returns>Vista com as reservas do utilizador.</returns>
        public async Task<IActionResult> MyBookings()
        {
            if (!GetCurrentUserId(out int userId))
                return Challenge();

            var bookings = await _context.Bookings
                .Include(b => b.Petsitter)
                .ThenInclude(p => p.User)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bookings);
        }

        /// <summary>
        /// Lista todas as reservas recebidas pelo pet sitter autenticado.
        /// Requer que o utilizador possua um perfil de pet sitter.
        /// </summary>
        /// <returns>Vista com as reservas recebidas para gestão.</returns>
        public async Task<IActionResult> Manage()
        {
            if (!GetCurrentUserId(out int userId))
                return Challenge();

            var petsitterProfile = await _context.Petsitters
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (petsitterProfile == null)
            {
                TempData["Error"] = "You need a petsitter profile to manage bookings.";
                return RedirectToAction("Index", "Petsitter");
            }

            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Petsitter)
                .ThenInclude(p => p.User)
                .Where(b => b.PetsitterId == petsitterProfile.Id)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View("ManageBookings", bookings);
        }

        /// <summary>
        /// Confirma uma reserva pendente.
        /// Apenas o pet sitter associado à reserva pode efetuar esta ação.
        /// </summary>
        /// <param name="id">Identificador da reserva.</param>
        /// <returns>Redireciona para a gestão de reservas.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Petsitter)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return NotFound();

            if (!GetCurrentUserId(out int userId))
                return Challenge();

            if (booking.Petsitter.UserId != userId)
                return Forbid();

            booking.Status = BookingStatus.Confirmed;
            booking.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking confirmed!";
            return RedirectToAction(nameof(Manage));
        }

        /// <summary>
        /// Rejeita uma reserva pendente.
        /// Pode ser efetuado pelo pet sitter ou pelo utilizador que criou a reserva.
        /// </summary>
        /// <param name="id">Identificador da reserva.</param>
        /// <returns>Redireciona para a gestão de reservas.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Petsitter)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return NotFound();

            if (!GetCurrentUserId(out int userId))
                return Challenge();

            if (booking.Petsitter.UserId != userId && booking.UserId != userId)
                return Forbid();

            booking.Status = BookingStatus.Rejected;
            booking.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking rejected.";
            return RedirectToAction(nameof(Manage));
        }

        /// <summary>
        /// Cancela uma reserva existente.
        /// Apenas o utilizador que criou a reserva pode efetuar esta ação.
        /// </summary>
        /// <param name="id">Identificador da reserva.</param>
        /// <returns>Redireciona para as reservas do utilizador.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            if (!GetCurrentUserId(out int userId))
                return Challenge();

            if (booking.UserId != userId)
                return Forbid();

            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking cancelled.";
            return RedirectToAction(nameof(MyBookings));
        }

        /// <summary>
        /// Marca uma reserva como concluída e redireciona para a criação de uma avaliação.
        /// Apenas o pet sitter associado à reserva pode efetuar esta ação.
        /// </summary>
        /// <param name="id">Identificador da reserva.</param>
        /// <returns>Redireciona para o formulário de avaliação do pet sitter.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Petsitter)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return NotFound();

            if (!GetCurrentUserId(out int userId))
                return Challenge();

            if (booking.Petsitter.UserId != userId)
                return Forbid();

            booking.Status = BookingStatus.Completed;
            booking.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction("Create", "Review", new { 
            reviewedId = booking.PetsitterId,
            reviewType = "Petsitter"  });
        }
    }
}
