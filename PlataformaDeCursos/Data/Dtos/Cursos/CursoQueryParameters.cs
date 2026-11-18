namespace PlataformaDeCursos.Data.Dtos.Cursos;

public class CursoQueryParameters
{
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 10;
    public string? Categoria { get; set; }
    public string? Busca { get; set; }

    public string? OrdenarPor { get; set; }
}