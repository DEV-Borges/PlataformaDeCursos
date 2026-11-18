namespace PlataformaDeCursos.Data.Dtos.Cursos;

public class CursoReadDto
{
    public int Id { get; set; }
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public string Categoria { get; set; }
    public int CargaHoraria { get; set; }
    public DateTime DataCriacao { get; set; }
}