namespace PetLink.Models
{
    /// <summary>
    /// Modelo utilizado para validação do processo de registo de utilizador.
    /// Contém todos os dados necessários para criar uma nova conta na plataforma.
    /// </summary>
    public class SignUpValidationModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string UserType { get; set; }
    }
}
