using System.ComponentModel.DataAnnotations;

namespace PlataformaDeCursos.Data.Dtos.Auth;

public class RegisterDto
{
    /// <example>Bruno Silva</example>
    [Required(ErrorMessage = "O nome completo é obrigatório.")]
    public string NomeCompleto { get; set; }

    /// <example>aluno@plataforma.com</example>
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
    public string Email { get; set; }

    /// <example>Aluno@123!</example>
    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")]
    public string Senha { get; set; }
}

public class LoginDto
{
    /// <example>admin@plataforma.com</example>
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress]
    public string Email { get; set; }

    /// <example>Admin@123!</example>
    [Required(ErrorMessage = "A senha é obrigatória.")]
    public string Senha { get; set; }
}

public class TokenResponseDto
{
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    public string AccessToken { get; set; }

    public DateTime ExpiraEm { get; set; }

    /// <example>admin@plataforma.com</example>
    public string Email { get; set; }

    public IList<string> Roles { get; set; }
}