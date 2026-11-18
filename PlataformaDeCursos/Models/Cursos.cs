namespace PlataformaDeCursos.Models;

public class Cursos
{
    public int Id { get; set; }
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public string Categoria { get; set; }
    public int CargaHoraria { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public bool Deletado { get; set; } = false;
    public ICollection<Inscricao> Inscricoes { get; set; }
}