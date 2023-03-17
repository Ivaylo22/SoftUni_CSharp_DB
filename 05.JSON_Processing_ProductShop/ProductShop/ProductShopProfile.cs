namespace ProductShop;

using AutoMapper;
using ProductShop.DTOs.Import;
using ProductShop.Models;

public class ProductShopProfile : Profile
{
    public ProductShopProfile() 
    {
        //Users
        CreateMap<ImportUserDto, User>();

        //Products
        CreateMap<ImportProductDto, Product>();

        //Categories
        CreateMap<ImportCategoryDto, Category>();

        //ProductsCategories
        CreateMap<ImportCategoryProductDto, CategoryProduct>();
    }
}
