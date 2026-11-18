namespace PlataformaDeCursos.Data.Dtos.Estudantes;

public class EstudanteReadDto
{
    public int Id { get; set; }
    public string UsuarioIdentityId { get; set; }
    public string NomeCompleto { get; set; }
    public string Email { get; set; }
    public DateTime DataCadastro { get; set; }
}