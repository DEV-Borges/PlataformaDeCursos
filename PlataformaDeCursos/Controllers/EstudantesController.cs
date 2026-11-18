using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaDeCursos.Data;
using PlataformaDeCursos.Data.Dtos.Estudantes;
using PlataformaDeCursos.Models;

namespace PlataformaDeCursos.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Exige usuário autenticado por padrão em todo o Controller
public class EstudantesController : ControllerBase
{
    private readonly CursoContext _context;
    private readonly IMapper _mapper;
    private readonly UserManager<IdentityUser> _userManager;

    public EstudantesController(
        CursoContext context,
        IMapper mapper,
        UserManager<IdentityUser> userManager)
    {
        _context = context;
        _mapper = mapper;
        _userManager = userManager;
    }

    // POST api/estudantes (Apenas Admin)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateEstudante([FromBody] EstudanteCreateDto dto)
    {
        // 1. Verifica se o UserId realmente existe na tabela do Identity
        var usuarioIdentity = await _userManager.FindByIdAsync(dto.UsuarioIdentityId);
        if (usuarioIdentity == null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Usuário Identity inválido",
                Detail = "O UsuarioIdentityId informado não existe no sistema de autenticação.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        // 2. Valida conflito de e-mail ou de vínculo Identity já existente (409 Conflict)
        var conflitoExistente = await _context.Estudantes
            .AnyAsync(e => e.Email == dto.Email || e.UsuarioIdentityId == dto.UsuarioIdentityId);

        if (conflitoExistente)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Conflito de dados",
                Detail = "Já existe um estudante cadastrado com este e-mail ou vinculado a este usuário.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var estudante = _mapper.Map<Estudante>(dto);

        _context.Estudantes.Add(estudante);
        await _context.SaveChangesAsync();

        var estudanteRead = _mapper.Map<EstudanteReadDto>(estudante);
        return CreatedAtAction(nameof(GetEstudanteById), new { id = estudante.Id }, estudanteRead);
    }

    // GET api/estudantes (Somente Admin - protege dados sensíveis)
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetEstudantes()
    {
        var estudantes = await _context.Estudantes
            .Where(e => !e.Deletado)
            .OrderBy(e => e.NomeCompleto)
            .ToListAsync();

        var estudantesDto = _mapper.Map<List<EstudanteReadDto>>(estudantes);
        return Ok(estudantesDto);
    }

    // GET api/estudantes/me (Retorna o perfil do próprio estudante autenticado)
    [HttpGet("me")]
    public async Task<IActionResult> GetMeuPerfil()
    {
        var usuarioLogadoId = ObterUsuarioLogadoId();
        if (string.IsNullOrEmpty(usuarioLogadoId))
            return Unauthorized();

        var estudante = await _context.Estudantes
            .FirstOrDefaultAsync(e => e.UsuarioIdentityId == usuarioLogadoId && !e.Deletado);

        if (estudante == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Perfil não encontrado",
                Detail = "Nenhum perfil de estudante ativo está vinculado à sua conta.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(_mapper.Map<EstudanteReadDto>(estudante));
    }

    // GET api/estudantes/{id} (Admin ou o próprio estudante)
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEstudanteById(int id)
    {
        var estudante = await _context.Estudantes
            .FirstOrDefaultAsync(e => e.Id == id && !e.Deletado);

        if (estudante == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Estudante não encontrado",
                Status = StatusCodes.Status404NotFound
            });
        }

        if (!EhProprioEstudanteOuAdmin(estudante))
            return Forbid(); // 403 Forbidden

        return Ok(_mapper.Map<EstudanteReadDto>(estudante));
    }

    // PUT api/estudantes/{id} (Admin ou o próprio estudante)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEstudante(int id, [FromBody] EstudanteUpdateDto dto)
    {
        var estudante = await _context.Estudantes
            .FirstOrDefaultAsync(e => e.Id == id && !e.Deletado);

        if (estudante == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Estudante não encontrado",
                Status = StatusCodes.Status404NotFound
            });
        }

        if (!EhProprioEstudanteOuAdmin(estudante))
            return Forbid(); // 403 Forbidden

        // Verifica se o novo e-mail já está em uso por outro estudante (409 Conflict)
        var emailEmUso = await _context.Estudantes
            .AnyAsync(e => e.Email == dto.Email && e.Id != id);

        if (emailEmUso)
        {
            return Conflict(new ProblemDetails
            {
                Title = "E-mail já cadastrado",
                Detail = "O e-mail informado já está sendo utilizado por outro estudante.",
                Status = StatusCodes.Status409Conflict
            });
        }

        _mapper.Map(dto, estudante);
        await _context.SaveChangesAsync();

        return NoContent(); // 204
    }

    // DELETE api/estudantes/{id} (Somente Admin - aplica Soft Delete)
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEstudante(int id)
    {
        var estudante = await _context.Estudantes
            .FirstOrDefaultAsync(e => e.Id == id && !e.Deletado);

        if (estudante == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Estudante não encontrado",
                Status = StatusCodes.Status404NotFound
            });
        }

        estudante.Deletado = true;
        await _context.SaveChangesAsync();

        return NoContent(); // 204
    }

    // --- Métodos Auxiliares Centralizados ---

    private string? ObterUsuarioLogadoId()
    {
        // Lê a claim 'sub' ou 'NameIdentifier' presente no token JWT
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