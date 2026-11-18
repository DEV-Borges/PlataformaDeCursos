namespace PlataformaDeCursos.Exceptions;

// Mapeada para 422 Unprocessable Entity
public class RegraDeNegocioException : Exception
{
    public RegraDeNegocioException(string mensagem) : base(mensagem)
    {
    }
}

// Mapeada para 409 Conflict (ex: e-mail ou matrícula duplicada)
public class ConflitoDeDadosException : Exception
{
    public ConflitoDeDadosException(string mensagem) : base(mensagem)
    {
    }
}

// Mapeada para 404 Not Found
public class RecursoNaoEncontradoException : Exception
{
    public RecursoNaoEncontradoException(string mensagem) : base(mensagem)
    {
    }
}