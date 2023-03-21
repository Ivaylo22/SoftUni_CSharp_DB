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
            this.CreateMap<Supplier, ExportLocalSupplierDto>()
                .ForMember(dst => dst.COUNT,
                    opt => opt.MapFrom(src => src.Parts.Count));

            //Part
            CreateMap<ImportPartDto, Part>();
            CreateMap<Part, ExportCarPartDto>();

            //Car
            CreateMap<ImportCarDto, Car>()
                .ForSourceMember(s => s.Parts, opt => opt.DoNotValidate());
            CreateMap<Car, ExportCarDto>();
            CreateMap<Car, ExportBmwCarDto>();
            CreateMap<Car, ExportCarWithPartsDto>()
                .ForMember(d => d.Parts,
                    opt => opt.MapFrom(s =>
                        s.PartsCars
                            .Select(pc => pc.Part)
                            .OrderByDescending(p => p.Price)
                            .ToArray()));

            //Customer
            CreateMap<ImportCustomerDto, Customer>()
                .ForMember(d => d.BirthDate,
                    opt => opt.MapFrom(s => DateTime.Parse(s.BirthDate, CultureInfo.InvariantCulture)));

            //Sales
            CreateMap<ImportSaleDto, Sale>();
        }
    }
}
