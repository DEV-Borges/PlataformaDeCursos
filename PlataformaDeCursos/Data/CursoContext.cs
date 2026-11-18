using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlataformaDeCursos.Models;

namespace PlataformaDeCursos.Data;

public class CursoContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public CursoContext(DbContextOptions<CursoContext> options) : base(options)
    {
    }

    public DbSet<Estudante> Estudantes { get; set; }
    public DbSet<Cursos> Cursos { get; set; }
    public DbSet<Inscricao> Inscricoes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // 2. ISTO É OBRIGATÓRIO: Tem de ser a PRIMEIRA linha dentro deste método!
        base.OnModelCreating(builder);

        builder.Entity<Estudante>()
            .HasIndex(e => e.Email)
            .IsUnique();

        builder.Entity<Estudante>()
            .HasIndex(e => e.UsuarioIdentityId)
            .IsUnique();

        builder.Entity<Cursos>()
            .HasIndex(c => c.Categoria);

        builder.Entity<Inscricao>()
            .HasIndex(i => new { i.EstudanteId, i.CursoId })
            .IsUnique();
    }
}