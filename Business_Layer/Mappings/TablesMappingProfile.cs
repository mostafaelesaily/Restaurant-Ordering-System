using Application.DTOs.TablesDTOs;
using AutoMapper;
using Domain_Layer.Entities;

namespace Business_Layer.Mappings
{
    public class TablesMappingProfile : Profile
    {
        public TablesMappingProfile()
        {
            CreateMap<Tables, GetTablesDto>();
            CreateMap<GetTablesDto, Tables>();

            CreateMap<CreateTablesDto, Tables>();
            CreateMap<Tables, CreateTablesDto>();

            CreateMap<UpdateTablesDto, Tables>();
            CreateMap<Tables, UpdateTablesDto>();
        }
    }
}
