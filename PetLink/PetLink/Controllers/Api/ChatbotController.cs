using Microsoft.AspNetCore.Mvc;
using PetLink.Services;
using System.Security.Claims;

namespace PetLink.Controllers.Api
{
    /// <summary>
    /// Controlador de API responsável pela interação com o chatbot da plataforma.
    /// Recebe mensagens do utilizador e devolve a resposta gerada pelo serviço de chatbot.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        /// <summary>
        /// Inicializa uma nova instância do controlador do chatbot.
        /// </summary>
        /// <param name="chatbotService">Serviço responsável por gerar as respostas do chatbot.</param>
        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        /// <summary>
        /// Envia uma mensagem do utilizador ao chatbot e devolve a respetiva resposta.
        /// Caso o utilizador esteja autenticado, o seu identificador é associado ao pedido
        /// para permitir respostas contextualizadas.
        /// </summary>
        /// <param name="request">Objeto contendo a mensagem a enviar ao chatbot.</param>
        /// <returns>Resposta JSON com a réplica do chatbot e a hora correspondente.</returns>
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

    /// <summary>
    /// Modelo utilizado para receber a mensagem enviada pelo utilizador ao chatbot.
    /// </summary>
    public class ChatbotRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
