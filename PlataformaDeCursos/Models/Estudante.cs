namespace PlataformaDeCursos.Models;

public class Estudante
{
    public int Id { get; set; }
    public string UsuarioIdentityId { get; set; }
    public string NomeCompleto { get; set; }
    public string Email { get; set; }
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public bool Deletado { get; set; } = false;
    public ICollection<Inscricao> Inscricoes { get; set; }
}