using System.ComponentModel.DataAnnotations;

namespace PlataformaDeCursos.Data.Dtos.Estudantes;

public class EstudanteUpdateDto
{
    [Required(ErrorMessage = "O nome completo é obrigatório.")]
    [StringLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
    public string NomeCompleto { get; set; }

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
    public string Email { get; set; }
}