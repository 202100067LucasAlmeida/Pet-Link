namespace PetLink.Models
{
    /// <summary>
    /// Modelo utilizado pela vista de erro da aplicação,
    /// permitindo apresentar o identificador do pedido associado a uma falha.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Identificador único do pedido que originou o erro.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Indica se o identificador do pedido deve ser apresentado na vista.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}