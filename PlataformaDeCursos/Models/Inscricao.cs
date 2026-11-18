namespace PlataformaDeCursos.Models;

public enum StatusInscricao
{
    Ativa = 1,
    Cancelada = 2
}

public class Inscricao
{
    public int Id { get; set; }
    public int EstudanteId { get; set; }
    public Estudante Estudante { get; set; }
    public int CursoId { get; set; }
    public Cursos Curso { get; set; }
    public StatusInscricao Status { get; set; } = StatusInscricao.Ativa;
    public DateTime DataMatricula { get; set; } = DateTime.UtcNow;
    public bool Deletado { get; set; } = false;
}