using PlataformaDeCursos.Models;

namespace PlataformaDeCursos.Data.Dtos.Inscricoes;

public class InscricaoQueryParameters
{
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 10;
    public StatusInscricao? Status { get; set; }
}