using FluentValidation;
using PlataformaDeCursos.Data.Dtos.Estudantes;

namespace PlataformaDeCursos.Validators;

public class EstudanteCreateDtoValidator : AbstractValidator<EstudanteCreateDto>
{
    public EstudanteCreateDtoValidator()
    {
        RuleFor(e => e.UsuarioIdentityId)
            .NotEmpty().WithMessage("O identificador do Identity é obrigatório.");

        RuleFor(e => e.NomeCompleto)
            .NotEmpty().WithMessage("O nome completo é obrigatório.")
            .MinimumLength(3).WithMessage("O nome completo deve ter ao menos 3 caracteres.")
            .MaximumLength(150);

        RuleFor(e => e.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("Informe um endereço de e-mail válido.");
    }
}

public class EstudanteUpdateDtoValidator : AbstractValidator<EstudanteUpdateDto>
{
    public EstudanteUpdateDtoValidator()
    {
        RuleFor(e => e.NomeCompleto)
            .NotEmpty().WithMessage("O nome completo é obrigatório.")
            .MinimumLength(3).WithMessage("O nome completo deve ter ao menos 3 caracteres.")
            .MaximumLength(150);

        RuleFor(e => e.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("Informe um endereço de e-mail válido.");
    }
}