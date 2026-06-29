using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PetLink.Data;
using PetLink.Models;
using PetLink.Models.Enums;

namespace PetLink.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão e visualização de perfis de pet sitters.
    /// Permite listar, pesquisar e consultar os detalhes de cada pet sitter,
    /// incluindo o histórico de mensagens com o utilizador autenticado.
    /// </summary>
    public class PetsitterController : BaseController
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Inicializa uma nova instância do controlador de pet sitters.
        /// </summary>
        /// <param name="context">Contexto da base de dados.</param>
        public PetsitterController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Apresenta a listagem de todos os pet sitters disponíveis,
        /// ordenados por avaliação por defeito.
        /// </summary>
        /// <returns>Vista com a lista de pet sitters.</returns>
        public async Task<IActionResult> Index()
        {
            return await Search(null, null, null, null);
        }
        
        /// <summary>
        /// Apresenta os detalhes do perfil de um pet sitter.
        /// Caso o utilizador autenticado não seja o próprio pet sitter,
        /// é também carregado o histórico de mensagens entre ambos.
        /// </summary>
        /// <param name="id">Identificador do pet sitter.</param>
        /// <returns>Vista com os detalhes do pet sitter.</returns>
        public async Task<IActionResult> Details(int id)
        {
            var sitter = await _context.Petsitters
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sitter == null) return NotFound();

            // 1. Verificar se o utilizador logado está a ver o seu próprio perfil
            bool isSelf = false;
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (!string.IsNullOrEmpty(userIdClaim))
            {
                int currentUserId = int.Parse(userIdClaim);
                isSelf = (sitter.UserId == currentUserId);

                // 2. Só carrega o histórico de mensagens se NÃO for o próprio
                if (!isSelf)
                {
                    ViewBag.ChatHistory = await _context.Messages
                        .Where(m => (m.SenderId == currentUserId && m.ReceiverId == sitter.UserId) ||
                                    (m.SenderId == sitter.UserId && m.ReceiverId == currentUserId))
                        .OrderBy(m => m.Timestamp)
                        .ToListAsync();
                }
            }

            ViewBag.IsSelf = isSelf;

            // Carregar outros sitters para a secção de baixo (excluindo o atual)
            ViewBag.OtherSitters = await _context.Petsitters
                .Include(p => p.User)
                .Where(p => p.Id != id)
                .Take(4)
                .ToListAsync();

            return View(sitter);
        }

        /// <summary>
        /// Pesquisa pet sitters aplicando filtros opcionais por tipo de serviço,
        /// taxa horária máxima, preferências de animal e critério de ordenação.
        /// </summary>
        /// <param name="serviceType">Tipo de serviço oferecido (opcional).</param>
        /// <param name="maxRate">Taxa horária máxima (opcional).</param>
        /// <param name="petPreferences">Preferência de tipo de animal (opcional).</param>
        /// <param name="sort">Critério de ordenação: "price_asc", "price_desc" ou avaliação por defeito (opcional).</param>
        /// <returns>Vista com os resultados filtrados e ordenados.</returns>
        public async Task<IActionResult> Search(ServiceType? serviceType,
                                                decimal? maxRate,
                                                PetPreferences? petPreferences,
                                                string? sort)
        {

            var query = _context.Petsitters
                        .Include(p => p.User)
                        .AsQueryable();

            if (serviceType.HasValue)
            {
                query = query.Where(p => p.serviceType == serviceType.Value);
            }

            if (maxRate.HasValue)
            {
                query = query.Where(p => p.HourlyRate <= maxRate.Value);
            }

            if (petPreferences.HasValue)
            {
                query = query.Where(p => p.petPreferences == petPreferences.Value);
            }

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.HourlyRate),
                "price_desc" => query.OrderByDescending(p => p.HourlyRate),
                //"rating" => query.OrderByDescending(p => p.Rating),
                _ => query.OrderByDescending(p => p.Rating)
            };

            var results = await query.ToListAsync();


            // Na action Search do PetsitterController
            var activeFilters = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(serviceType.ToString()))
                activeFilters["ServiceType"] = serviceType;

            if (maxRate < 30)
                activeFilters["MaxRate"] = maxRate;

            if (!string.IsNullOrEmpty(petPreferences.ToString()))
                activeFilters["PetPreferences"] = petPreferences;

            ViewBag.ActiveFilters = activeFilters;
            ViewBag.CurrentServiceType = serviceType;
            ViewBag.CurrentMaxRate = maxRate;
            ViewBag.CurrentPetPreferences = petPreferences;

            return View("Index", results);
        }
    }
}