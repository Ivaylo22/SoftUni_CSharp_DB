using AutoMapper;
using CarDealer.DTOs.Export;
using CarDealer.DTOs.Import;
using CarDealer.Models;
using System.Globalization;

namespace CarDealer
{
    public class CarDealerProfile : Profile
    {
        public CarDealerProfile()
        {
            //Supplier
            CreateMap<ImportSupplierDto, Supplier>();

            //Part
            CreateMap<ImportPartDto, Part>();

            //Car
            CreateMap<ImportCarDto, Car>()
                .ForSourceMember(s => s.Parts, opt => opt.DoNotValidate());
            CreateMap<Car, ExportCarDto>();
            this.CreateMap<Car, ExportBmwCarDto>();

            //Customer
            CreateMap<ImportCustomerDto, Customer>()
                .ForMember(d => d.BirthDate,
                    opt => opt.MapFrom(s => DateTime.Parse(s.BirthDate, CultureInfo.InvariantCulture)));

            //Sales
            CreateMap<ImportSaleDto, Sale>();
        }
    }
}
