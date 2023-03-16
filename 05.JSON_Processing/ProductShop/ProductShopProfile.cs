namespace ProductShop;

using AutoMapper;
using ProductShop.DTOs.Import;
using ProductShop.Models;

public class ProductShopProfile : Profile
{
    public ProductShopProfile() 
    {
        CreateMap<ImportUserDto, User>();
        CreateMap<ImportProductDto, Product>();
    }
}
