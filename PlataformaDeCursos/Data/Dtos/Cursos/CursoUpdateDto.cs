using System.ComponentModel.DataAnnotations;

namespace PlataformaDeCursos.Data.Dtos.Cursos;

public class CursoUpdateDto
{
    [Required]
    [StringLength(200)]
    public string Titulo { get; set; }

    [Required]
    public string Descricao { get; set; }

    [Required]
    public string Categoria { get; set; }

    [Range(1, 1000)]
    public int CargaHoraria { get; set; }
}