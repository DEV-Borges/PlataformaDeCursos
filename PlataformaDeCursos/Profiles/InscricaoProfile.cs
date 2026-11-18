using AutoMapper;
using PlataformaDeCursos.Data.Dtos.Inscricoes;
using PlataformaDeCursos.Models;

namespace PlataformaDeCursos.Profiles;

public class InscricaoProfile : Profile
{
    public InscricaoProfile()
    {
        CreateMap<Inscricao, InscricaoReadDto>()
            .ForMember(dest => dest.NomeEstudante, opt => opt.MapFrom(src => src.Estudante.NomeCompleto))
            .ForMember(dest => dest.TituloCurso, opt => opt.MapFrom(src => src.Curso.Titulo))
            .ForMember(dest => dest.CategoriaCurso, opt => opt.MapFrom(src => src.Curso.Categoria))
            .ForMember(dest => dest.CargaHorariaCurso, opt => opt.MapFrom(src => src.Curso.CargaHoraria))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}