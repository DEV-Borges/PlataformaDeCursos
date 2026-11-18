using AutoMapper;
using PlataformaDeCursos.Data.Dtos.Cursos;
using PlataformaDeCursos.Models;

namespace PlataformaDeCursos.Profiles
{
    public class CursoProfile : Profile
    {
        public CursoProfile()
        {
            CreateMap<CursoCreateDto, Cursos>();
            CreateMap<CursoUpdateDto, Cursos>();
            CreateMap<Cursos, CursoReadDto>();
        }
    }
}