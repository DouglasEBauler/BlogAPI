using System.ComponentModel.DataAnnotations;

namespace BlogAPI.ViewModels.Accounts;
public class LoginViewModel
{
    [Required(ErrorMessage = "O e-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "O e-mail é inválido")]
    public string Email { get; set; }
    public string Password { get; set; }
}
