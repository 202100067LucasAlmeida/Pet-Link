using Microsoft.AspNetCore.Mvc;

namespace PetLink.Controllers
{
    /// <summary>
    /// Controlador responsável pelo tratamento e apresentação de páginas de erro.
    /// Gere erros de servidor, acesso negado e códigos de estado HTTP.
    /// </summary>
    public class ErrorsController : Controller
    {
        /// <summary>
        /// Apresenta a página de erro interno do servidor.
        /// </summary>
        /// <returns>Vista de erro de servidor.</returns>
        [Route("Errors/ServerFault")]
        public IActionResult ServerFault()
        {
            return View();
        }

        /// <summary>
        /// Apresenta a página de acesso negado.
        /// Exibida quando o utilizador tenta aceder a um recurso sem as permissões necessárias.
        /// </summary>
        /// <returns>Vista de acesso negado.</returns>
        [Route("Errors/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Apresenta a página de erro correspondente ao código de estado HTTP recebido.
        /// O código 404 redireciona para a vista de recurso não encontrado;
        /// qualquer outro código apresenta a vista de erro de servidor.
        /// </summary>
        /// <param name="code">Código de estado HTTP.</param>
        /// <returns>Vista de erro adequada ao código recebido.</returns>
        [Route("Errors/Status")]
        public IActionResult Status(int code)
        {
            if (code == 404)
            {
                return View("FileMissing");
            }
            return View("ServerFault");
        }
    }
}