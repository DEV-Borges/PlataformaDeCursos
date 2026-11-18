using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PlataformaDeCursos.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using PlataformaDeCursos.Middlewares;
using System.Reflection;

using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CursoConnection");
builder.Services.AddDbContext<CursoContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddIdentityCore<IdentityUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<CursoContext>()
.AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "SuaSuperChaveSecretaMuitoSeguraParaOProjeto2024!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "PlataformaDeCursosAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "PlataformaDeCursosApp";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddControllers();

// Ativa validação automática de entrada (400 Bad Request em ProblemDetails)
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Padroniza respostas de erro HTTP (400, 401, 403, 404) no formato RFC 7807 ProblemDetails
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Plataforma de Cursos API",
        Version = "v1",
        Description = """
            API REST para gerenciamento de Cursos, Estudantes e Matrículas.

            ### Regras de Paginação e Filtros
            * **Paginação padrão:** `pagina = 1`, `tamanhoPagina = 10` (limite recomendado: 50 itens por página).
            * **Filtros em Cursos:** `categoria` (filtro exato), `busca` (termo no título ou descrição) e `ordenarPor` (`titulo`, `titulo_desc`, `data`, `data_desc`).
            * **Filtros em Matrículas:** `status` (`1` = Ativa, `2` = Cancelada).

            ### Papéis (Roles)
            * **Admin:** Acesso total ao sistema.
            * **Instructor:** Criação e atualização de cursos.
            * **Student:** Consulta e realização da própria matrícula e visualização/edição do próprio perfil.
            """
    });

    // Configuração do Security Scheme HTTP Bearer (JWT)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira apenas o token JWT gerado no endpoint /api/auth/login (não é necessário digitar 'Bearer ' antes)."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Integra comentários XML (resumos, exemplos e respostas) ao Swagger
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

var app = builder.Build();

app.UseMiddleware<TratamentoExcecaoMiddleware>();
app.UseStatusCodePages(); // Garante corpo ProblemDetails em 401 e 403

// Ativa o Swagger em Desenvolvimento e Homologação (Staging / Homologacao)
if (app.Environment.IsDevelopment() ||
    app.Environment.IsStaging() ||
    app.Environment.IsEnvironment("Homologacao"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Plataforma de Cursos API v1");
        c.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    await PlataformaDeCursos.Data.Seeders.DbSeeder.SeedRolesAndAdminAsync(scope.ServiceProvider);
}
app.Run();