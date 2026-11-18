using AutoMapper;
using PlataformaDeCursos.Data.Dtos.Estudantes;
using PlataformaDeCursos.Models;

namespace PlataformaDeCursos.Profiles;

public class EstudanteProfile : Profile
{
    public EstudanteProfile()
    {
        CreateMap<EstudanteCreateDto, Estudante>();
        CreateMap<EstudanteUpdateDto, Estudante>();
        CreateMap<Estudante, EstudanteReadDto>();
    }
}