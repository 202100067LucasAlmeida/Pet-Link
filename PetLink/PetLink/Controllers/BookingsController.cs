using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using PetLink.Models.Enums;

namespace PetLink.Controllers
{
    [Authorize]
    public class BookingsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

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

            return View(bookings);
        }

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

            TempData["Success"] = "Booking marked as completed.";
            return RedirectToAction(nameof(Manage));
        }
    }
}
