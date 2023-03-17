using AutoMapper;
using CarDealer.DTOs.Import;
using CarDealer.Models;

namespace CarDealer;

public class CarDealerProfile : Profile
{
    public CarDealerProfile()
    {
        CreateMap<ImportSupliersDto, Supplier>();

        CreateMap<ImportPartsDto, Part>();

        CreateMap<ImportCarsDto, Car>();

        CreateMap<ImportCustomersDto, Customer>();

        CreateMap<ImportSalesDto, Sale>();

    }
}
