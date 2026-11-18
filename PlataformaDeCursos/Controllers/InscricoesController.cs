using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaDeCursos.Data;
using PlataformaDeCursos.Data.Dtos.Inscricoes;
using PlataformaDeCursos.Models;

namespace PlataformaDeCursos.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InscricoesController : ControllerBase
{
    private readonly CursoContext _context;
    private readonly IMapper _mapper;
    private const int PrazoMaximoCancelamentoDias = 7;

    public InscricoesController(CursoContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // POST api/inscricoes
    [HttpPost]
    public async Task<IActionResult> Matricular([FromBody] InscricaoCreateDto dto)
    {
        var isAdmin = User.IsInRole("Admin");
        Estudante? estudante;

        if (isAdmin)
        {
            // Se for Admin, o EstudanteId deve obrigatoriamente vir no payload
            if (!dto.EstudanteId.HasValue)
            {
                return UnprocessableEntity(new ProblemDetails
                {
                    Title = "EstudanteId obrigatório para Admin",
                    Detail = "Ao matricular um aluno como Administrador, informe o campo 'estudanteId' no payload.",
                    Status = StatusCodes.Status422UnprocessableEntity
                });
            }

            estudante = await _context.Estudantes
                .FirstOrDefaultAsync(e => e.Id == dto.EstudanteId.Value && !e.Deletado);
        } else
        {
            // Se não for Admin, proíbe o envio de EstudanteId arbitrário no payload
            if (dto.EstudanteId.HasValue)
            {
                return UnprocessableEntity(new ProblemDetails
                {
                    Title = "Operação não permitida",
                    Detail = "Estudantes não podem informar 'estudanteId' no payload. A identificação é feita automaticamente pelo token.",
                    Status = StatusCodes.Status422UnprocessableEntity
                });
            }

            var usuarioLogadoId = ObterUsuarioLogadoId();
            if (string.IsNullOrEmpty(usuarioLogadoId))
                return Unauthorized();

            estudante = await _context.Estudantes
                .FirstOrDefaultAsync(e => e.UsuarioIdentityId == usuarioLogadoId && !e.Deletado);
        }

        // 1. Valida se o estudante existe e está ativo
        if (estudante == null)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Estudante inválido ou inativo",
                Detail = "O perfil de estudante não foi encontrado ou está desativado.",
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }

        // 2. Valida se o curso existe e está ativo
        var curso = await _context.Cursos
            .FirstOrDefaultAsync(c => c.Id == dto.CursoId && !c.Deletado);

        if (curso == null)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Curso inválido ou inativo",
                Detail = "O curso informado não existe ou não está mais disponível para matrículas.",
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }

        // 3. Inicia transação para garantir consistência e evitar condição de corrida
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Verifica se já existe matrícula prévia neste curso
            var matriculaExistente = await _context.Inscricoes
                .AnyAsync(i => i.EstudanteId == estudante.Id && i.CursoId == curso.Id);

            if (matriculaExistente)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Matrícula duplicada",
                    Detail = "Este estudante já possui um registro de matrícula para este curso.",
                    Status = StatusCodes.Status409Conflict
                });
            }

            var novaInscricao = new Inscricao
            {
                EstudanteId = estudante.Id,
                CursoId = curso.Id,
                Status = StatusInscricao.Ativa,
                DataMatricula = DateTime.UtcNow,
                Deletado = false
            };

            _context.Inscricoes.Add(novaInscricao);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Associa as referências já carregadas para montar o DTO de retorno
            novaInscricao.Estudante = estudante;
            novaInscricao.Curso = curso;

            var inscricaoRead = _mapper.Map<InscricaoReadDto>(novaInscricao);

            return CreatedAtAction(
                nameof(ListarInscricoesPorEstudante),
                new { estudanteId = estudante.Id },
                inscricaoRead);
        } catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            // Caso duas requisições simultâneas passem pelo AnyAsync, o índice único do banco bloqueia
            return Conflict(new ProblemDetails
            {
                Title = "Conflito de concorrência na matrícula",
                Detail = "Não foi possível concluir a matrícula pois já existe um vínculo para este estudante e curso.",
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    // GET api/estudantes/{estudanteId}/inscricoes (Admin ou o próprio estudante)
    [HttpGet("~/api/estudantes/{estudanteId:int}/inscricoes")]
    public async Task<IActionResult> ListarInscricoesPorEstudante(
        int estudanteId,
        [FromQuery] InscricaoQueryParameters parametros)
    {
        var estudante = await _context.Estudantes
            .FirstOrDefaultAsync(e => e.Id == estudanteId && !e.Deletado);

        if (estudante == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Estudante não encontrado",
                Status = StatusCodes.Status404NotFound
            });
        }

        // Apenas Admin ou o próprio dono do perfil podem consultar as matrículas
        if (!EhProprioEstudanteOuAdmin(estudante))
            return Forbid();

        var query = _context.Inscricoes
            .Include(i => i.Curso)
            .Include(i => i.Estudante)
            .Where(i => i.EstudanteId == estudanteId && !i.Deletado && !i.Curso.Deletado)
            .AsQueryable();

        // Filtro opcional por Status (1 = Ativa, 2 = Cancelada)
        if (parametros.Status.HasValue)
        {
            query = query.Where(i => i.Status == parametros.Status.Value);
        }

        var totalItens = await query.CountAsync();

        var inscricoes = await query
            .OrderByDescending(i => i.DataMatricula)
            .Skip((parametros.Pagina - 1) * parametros.TamanhoPagina)
            .Take(parametros.TamanhoPagina)
            .ToListAsync();

        var inscricoesDto = _mapper.Map<List<InscricaoReadDto>>(inscricoes);

        return Ok(new
        {
            TotalItens = totalItens,
            PaginaAtual = parametros.Pagina,
            TamanhoPagina = parametros.TamanhoPagina,
            Inscricoes = inscricoesDto
        });
    }

    // DELETE api/inscricoes/{id} (Cancelar matrícula mantendo histórico)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> CancelarMatricula(int id)
    {
        var inscricao = await _context.Inscricoes
            .Include(i => i.Estudante)
            .FirstOrDefaultAsync(i => i.Id == id && !i.Deletado);

        if (inscricao == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Matrícula não encontrada",
                Status = StatusCodes.Status404NotFound
            });
        }

        if (!EhProprioEstudanteOuAdmin(inscricao.Estudante))
            return Forbid();

        // Regra 1: Não cancelar o que já está cancelado
        if (inscricao.Status == StatusInscricao.Cancelada)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Matrícula já cancelada",
                Detail = "Esta matrícula já se encontra com o status Cancelada.",
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }

        // Regra 2: Prazo de 7 dias para cancelamento pelo estudante (Admin não tem limite de prazo)
        var diasDesdeMatricula = (DateTime.UtcNow - inscricao.DataMatricula).TotalDays;
        if (!User.IsInRole("Admin") && diasDesdeMatricula > PrazoMaximoCancelamentoDias)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Prazo de cancelamento expirado",
                Detail = $"O cancelamento pelo estudante só é permitido em até {PrazoMaximoCancelamentoDias} dias após a matrícula.",
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }

        // Preserva o histórico mudando apenas o status
        inscricao.Status = StatusInscricao.Cancelada;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // --- Métodos Auxiliares ---

    private string? ObterUsuarioLogadoId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
    }

    private bool EhProprioEstudanteOuAdmin(Estudante estudante)
    {
        if (User.IsInRole("Admin"))
            return true;

        var usuarioLogadoId = ObterUsuarioLogadoId();
        return !string.IsNullOrEmpty(usuarioLogadoId) &&
               estudante.UsuarioIdentityId == usuarioLogadoId;
    }
}