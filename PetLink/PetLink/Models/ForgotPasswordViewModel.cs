namespace PetLink.Models
{
    /// <summary>
    /// ViewModel utilizado para receber o pedido de recuperação de password,
    /// contendo o email da conta a recuperar.
    /// </summary>
    public class ForgotPasswordViewModel
    {
        /// <summary>
        /// Email associado à conta para a qual se pretende recuperar a password.
        /// </summary>
        public string Email { get; set; }
    }
}