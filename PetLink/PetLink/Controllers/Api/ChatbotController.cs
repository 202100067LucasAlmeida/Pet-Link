using Microsoft.AspNetCore.Mvc;
using PetLink.Services;
using System.Security.Claims;

namespace PetLink.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] ChatbotRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { reply = "Please type a message." });

            var userIdClaim = User.FindFirst("UserId")?.Value;
            int? userId = null;
            if (int.TryParse(userIdClaim, out var id))
                userId = id;

            var reply = await _chatbotService.GetBotResponseAsync(request.Message, userId);

            return Ok(new
            {
                reply,
                timestamp = DateTime.UtcNow
            });
        }
    }

    public class ChatbotRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
