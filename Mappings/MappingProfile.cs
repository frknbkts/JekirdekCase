using AutoMapper;
using JekirdekCase.Dtos;
using JekirdekCase.Models;

namespace JekirdekCase.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            CreateMap<Customer, CustomerDto>();

            CreateMap<CreateCustomerDto, Customer>();

            CreateMap<UpdateCustomerDto, Customer>();

            CreateMap<User, UserDto>();
        }
    }
}
