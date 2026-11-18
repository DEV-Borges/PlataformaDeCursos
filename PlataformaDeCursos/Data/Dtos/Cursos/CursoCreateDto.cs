using System.ComponentModel.DataAnnotations;

namespace PlataformaDeCursos.Data.Dtos.Cursos;

public class CursoCreateDto
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(200, ErrorMessage = "O título não pode exceder 200 caracteres.")]
    public string Titulo { get; set; }

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    public string Descricao { get; set; }

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public string Categoria { get; set; }

    [Range(1, 1000, ErrorMessage = "A carga horária deve ser entre 1 e 1000 horas.")]
    public int CargaHoraria { get; set; }
}