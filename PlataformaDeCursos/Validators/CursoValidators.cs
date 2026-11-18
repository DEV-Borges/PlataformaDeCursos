using FluentValidation;
using PlataformaDeCursos.Data.Dtos.Cursos;

namespace PlataformaDeCursos.Validators;

public class CursoCreateDtoValidator : AbstractValidator<CursoCreateDto>
{
    public CursoCreateDtoValidator()
    {
        RuleFor(c => c.Titulo)
            .NotEmpty().WithMessage("Informe o título do curso.")
            .MinimumLength(3).WithMessage("O título do curso precisa ter ao menos 3 caracteres.")
            .MaximumLength(200).WithMessage("O título não pode ultrapassar 200 caracteres.");

        RuleFor(c => c.Descricao)
            .NotEmpty().WithMessage("Informe a descrição do curso.")
            .MinimumLength(10).WithMessage("A descrição deve ter ao menos 10 caracteres.");

        RuleFor(c => c.Categoria)
            .NotEmpty().WithMessage("Informe a categoria do curso.");

        RuleFor(c => c.CargaHoraria)
            .InclusiveBetween(1, 1000).WithMessage("A carga horária deve estar entre 1 e 1000 horas.");
    }
}

public class CursoUpdateDtoValidator : AbstractValidator<CursoUpdateDto>
{
    public CursoUpdateDtoValidator()
    {
        RuleFor(c => c.Titulo)
            .NotEmpty().WithMessage("Informe o título do curso.")
            .MinimumLength(3).WithMessage("O título do curso precisa ter ao menos 3 caracteres.")
            .MaximumLength(200).WithMessage("O título não pode ultrapassar 200 caracteres.");

        RuleFor(c => c.Descricao)
            .NotEmpty().WithMessage("Informe a descrição do curso.")
            .MinimumLength(10).WithMessage("A descrição deve ter ao menos 10 caracteres.");

        RuleFor(c => c.Categoria)
            .NotEmpty().WithMessage("Informe a categoria do curso.");

        RuleFor(c => c.CargaHoraria)
            .InclusiveBetween(1, 1000).WithMessage("A carga horária deve estar entre 1 e 1000 horas.");
    }
}