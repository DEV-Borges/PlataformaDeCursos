using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PlataformaDeCursos.Data;
using PlataformaDeCursos.Data.Dtos.Auth;
using PlataformaDeCursos.Models;

namespace PlataformaDeCursos.Controllers;

[ApiController]
[Route("api/auth")]
[Tags("Auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly CursoContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<IdentityUser> userManager,
        CursoContext context,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Registra um novo estudante na plataforma.
    /// </summary>
    /// <remarks>
    /// Acesso: **Público (Anônimo)**. Cria a conta no Identity com o papel `Student` e gera automaticamente o perfil na tabela `Estudantes`.
    /// </remarks>
    /// <response code="201">Estudante registrado com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos ou senha fora do padrão.</response>
    /// <response code="409">E-mail já cadastrado na plataforma.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var usuarioExistente = await _userManager.FindByEmailAsync(dto.Email);
        if (usuarioExistente != null)
        {
            return Conflict(new ProblemDetails
            {
                Title = "E-mail já cadastrado",
                Detail = "Já existe uma conta registrada com este endereço de e-mail.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var novoUsuario = new IdentityUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true
        };

        var resultado = await _userManager.CreateAsync(novoUsuario, dto.Senha);
        if (!resultado.Succeeded)
        {
            return BadRequest(new ValidationProblemDetails(
                resultado.Errors.ToDictionary(e => e.Code, e => new[] { e.Description }))
            {
                Title = "Falha ao criar usuário",
                Status = StatusCodes.Status400BadRequest
            });
        }

        await _userManager.AddToRoleAsync(novoUsuario, "Student");

        var estudante = new Estudante
        {
            UsuarioIdentityId = novoUsuario.Id,
            NomeCompleto = dto.NomeCompleto,
            Email = dto.Email,
            DataCadastro = DateTime.UtcNow,
            Deletado = false
        };

        _context.Estudantes.Add(estudante);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new
        {
            Mensagem = "Conta criada com sucesso.",
            EstudanteId = estudante.Id,
            UsuarioIdentityId = novoUsuario.Id
        });
    }

    /// <summary>
    /// Autentica um usuário e retorna o token JWT Bearer.
    /// </summary>
    /// <remarks>
    /// Acesso: **Público (Anônimo)**.
    ///
    /// Exemplo de credencial padrão criada pelo Seeder (Role **Admin**):
    /// ```json
    /// {
    ///   "email": "admin@plataforma.com",
    ///   "senha": "Admin@123!"
    /// }
    /// ```
    /// </remarks>
    /// <response code="200">Retorna o token JWT e sua data de expiração.</response>
    /// <response code="401">Credenciais inválidas ou conta bloqueada.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Senha))
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Credenciais inválidas",
                Detail = "E-mail ou senha incorretos.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var expiraEm = DateTime.UtcNow.AddHours(2);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var jwtKey = _configuration["Jwt:Key"] ?? "SuaSuperChaveSecretaMuitoSeguraParaOProjeto2024!";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "PlataformaDeCursosAPI";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "PlataformaDeCursosApp";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiraEm,
            signingCredentials: creds);

        return Ok(new TokenResponseDto
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiraEm = expiraEm,
            Email = user.Email!,
            Roles = roles
        });
    }
}