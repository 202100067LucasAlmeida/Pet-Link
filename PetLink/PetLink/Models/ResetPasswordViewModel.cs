namespace PetLink.Models
{
    /// <summary>
    /// Modelo utilizado no processo de redefinição de palavra-passe.
    /// Contém o token de validação e os dados necessários para definir uma nova password.
    /// </summary>
    public class ResetPasswordViewModel
    {
        /// <summary>
        /// Token único gerado no pedido de recuperação de password.
        /// É usado para validar que o pedido é legítimo e não expirou.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Nova palavra-passe definida pelo utilizador.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Confirmação da nova palavra-passe para validação de consistência.
        /// </summary>
        public string ConfirmPassword { get; set; }
    }
}