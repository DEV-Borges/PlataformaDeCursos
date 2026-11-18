using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaDeCursos.Data;
using PlataformaDeCursos.Data.Dtos.Cursos;
using PlataformaDeCursos.Models;

namespace PlataformaDeCursos.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CursosController : ControllerBase
{
    private readonly CursoContext _context;
    private readonly IMapper _mapper;

    public CursosController(CursoContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // POST api/cursos (Apenas Admin e Instrutor)
    [HttpPost]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> CreateCurso([FromBody] CursoCreateDto cursoDto)
    {
        var curso = _mapper.Map<Cursos>(cursoDto);

        _context.Cursos.Add(curso);
        await _context.SaveChangesAsync();

        var cursoRead = _mapper.Map<CursoReadDto>(curso);

        return CreatedAtAction(nameof(GetCursoById), new { id = curso.Id }, cursoRead);
    }

    // GET api/cursos (Público, com Paginação, Filtros, Busca e Cache de 60s)
    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(Duration = 60)]
    public async Task<IActionResult> GetCursos([FromQuery] CursoQueryParameters parametros)
    {
        // Apenas cursos não deletados (Soft Delete)
        var query = _context.Cursos.Where(c => !c.Deletado).AsQueryable();

        // 1. Filtro Exato por Categoria
        if (!string.IsNullOrWhiteSpace(parametros.Categoria))
        {
            query = query.Where(c => c.Categoria == parametros.Categoria);
        }

        // 2. Busca por termo (no título ou descrição)
        if (!string.IsNullOrWhiteSpace(parametros.Busca))
        {
            query = query.Where(c => c.Titulo.Contains(parametros.Busca) || c.Descricao.Contains(parametros.Busca));
        }

        // 3. Ordenação
        query = parametros.OrdenarPor?.ToLower() switch
        {
            "titulo" => query.OrderBy(c => c.Titulo),
            "titulo_desc" => query.OrderByDescending(c => c.Titulo),
            "data" => query.OrderBy(c => c.DataCriacao),
            "data_desc" => query.OrderByDescending(c => c.DataCriacao),
            _ => query.OrderByDescending(c => c.DataCriacao) // Padrão
        };

        // 4. Paginação
        var cursos = await query
            .Skip((parametros.Pagina - 1) * parametros.TamanhoPagina)
            .Take(parametros.TamanhoPagina)
            .ToListAsync();

        var totalItens = await query.CountAsync();
        var cursosDto = _mapper.Map<List<CursoReadDto>>(cursos);

        // Retornar meta-dados no cabeçalho ou envolto num objeto anônimo
        return Ok(new
        {
            TotalItens = totalItens,
            PaginaAtual = parametros.Pagina,
            TamanhoPagina = parametros.TamanhoPagina,
            Cursos = cursosDto
        });
    }

    // GET api/cursos/{id} (Público, com Cache)
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ResponseCache(Duration = 60)]
    public async Task<IActionResult> GetCursoById(int id)
    {
        var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id && !c.Deletado);

        if (curso == null)
            return NotFound(new ProblemDetails { Title = "Curso não encontrado", Status = 404 });

        var cursoDto = _mapper.Map<CursoReadDto>(curso);
        return Ok(cursoDto);
    }

    // PUT api/cursos/{id} (Apenas Admin e Instrutor)
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> UpdateCurso(int id, [FromBody] CursoUpdateDto cursoDto)
    {
        var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id && !c.Deletado);

        if (curso == null)
            return NotFound(new ProblemDetails { Title = "Curso não encontrado", Status = 404 });

        _mapper.Map(cursoDto, curso); // Sobrescreve as propriedades

        await _context.SaveChangesAsync();
        return NoContent(); // 204
    }

    // DELETE api/cursos/{id} (Apenas Admin) - Usando Soft Delete
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCurso(int id)
    {
        var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id && !c.Deletado);

        if (curso == null)
            return NotFound(new ProblemDetails { Title = "Curso não encontrado", Status = 404 });

        // Soft delete
        curso.Deletado = true;
        await _context.SaveChangesAsync();

        return NoContent(); // 204
    }
}