namespace PlataformaDeCursos.Data.Dtos.Inscricoes;

public class InscricaoReadDto
{
    public int Id { get; set; }
    public int EstudanteId { get; set; }
    public string NomeEstudante { get; set; }
    public int CursoId { get; set; }
    public string TituloCurso { get; set; }
    public string CategoriaCurso { get; set; }
    public int CargaHorariaCurso { get; set; }
    public string Status { get; set; }
    public DateTime DataMatricula { get; set; }
}