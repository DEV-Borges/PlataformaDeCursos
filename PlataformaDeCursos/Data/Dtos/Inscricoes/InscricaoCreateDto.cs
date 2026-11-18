using System.ComponentModel.DataAnnotations;

namespace PlataformaDeCursos.Data.Dtos.Inscricoes;

public class InscricaoCreateDto
{
    [Required(ErrorMessage = "O ID do curso é obrigatório.")]
    public int CursoId { get; set; }

    // Opcional no DTO: obrigatório apenas quando a requisição for feita por um Admin.
    // Estudantes comuns são identificados exclusivamente pelo token JWT.
    public int? EstudanteId { get; set; }
}