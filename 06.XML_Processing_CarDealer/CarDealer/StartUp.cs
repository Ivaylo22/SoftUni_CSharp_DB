﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using CarDealer.Data;
using CarDealer.DTOs.Export;
using CarDealer.DTOs.Import;
using CarDealer.Models;
using CarDealer.Utilities;

namespace CarDealer;

public class StartUp
{

    public static void Main()
    {
        using CarDealerContext context = new CarDealerContext();
        //string inputXml =
        //    File.ReadAllText("../../../Datasets/sales.xml");

        string result = GetSalesWithAppliedDiscount(context);
        Console.WriteLine(result);

    }
    public static string ImportSuppliers(CarDealerContext context, string inputXml)
    {
        IMapper mapper = InitializeAutoMapper();
        XmlHelper xmlHelper = new XmlHelper();
        ImportSupplierDto[] supplierDtos =
            xmlHelper.Deserialize<ImportSupplierDto[]>(inputXml, "Suppliers");

        ICollection<Supplier> validSuppliers = new HashSet<Supplier>();
        foreach (ImportSupplierDto supplierDto in supplierDtos)
        {
            if (string.IsNullOrEmpty(supplierDto.Name))
            {
                continue;
            }

            Supplier supplier = mapper.Map<Supplier>(supplierDto);

            validSuppliers.Add(supplier);
        }

        context.Suppliers.AddRange(validSuppliers);
        context.SaveChanges();

        return $"Successfully imported {validSuppliers.Count}";
    }

    public static string ImportParts(CarDealerContext context, string inputXml)
    {
        IMapper mapper = InitializeAutoMapper();
        XmlHelper xmlHelper = new XmlHelper();

        ImportPartDto[] partDtos = xmlHelper
            .Deserialize<ImportPartDto[]>(inputXml, "Parts");

        ICollection<Part> validParts = new HashSet<Part>();

        foreach (var dto in partDtos)
        {
            if (!context.Suppliers.Any(s => s.Id == dto.SupplierId))
            {
                continue;
            }

            Part part = mapper.Map<Part>(dto);
            validParts.Add(part);
        }

        context.Parts
            .AddRange(validParts);
        context.SaveChanges();

        return $"Successfully imported {validParts.Count}";

    }

    public static string ImportCars(CarDealerContext context, string inputXml)
    {
        IMapper mapper = InitializeAutoMapper();
        XmlHelper xmlHelper = new XmlHelper();

        ImportCarDto[] carDtos =
            xmlHelper.Deserialize<ImportCarDto[]>(inputXml, "Cars");

        ICollection<Car> validCars = new HashSet<Car>();
        foreach (var carDto in carDtos)
        {
            Car car = mapper.Map<Car>(carDto);

            foreach (var partDto in carDto.Parts.DistinctBy(p => p.PartId))
            {
                if (!context.Parts.Any(p => p.Id == partDto.PartId))
                {
                    continue;
                }

                PartCar carPart = new PartCar()
                {
                    PartId = partDto.PartId
                };
                car.PartsCars.Add(carPart);
            }

            validCars.Add(car);
        }

        context.Cars.AddRange(validCars);
        context.SaveChanges();

        return $"Successfully imported {validCars.Count}";
    }

    public static string ImportCustomers(CarDealerContext context, string inputXml)
    {
        IMapper mapper = InitializeAutoMapper();
        XmlHelper xmlHelper = new XmlHelper();

        ImportCustomerDto[] customerDtos =
            xmlHelper.Deserialize<ImportCustomerDto[]>(inputXml, "Customers");

        ICollection<Customer> validCustomers = new HashSet<Customer>();
        foreach (ImportCustomerDto dto in customerDtos)
        {
            Customer customer = mapper.Map<Customer>(dto);
            validCustomers.Add(customer);
        }

        context.Customers.AddRange(validCustomers);
        context.SaveChanges();

        return $"Successfully imported {validCustomers.Count}";
    }

    public static string ImportSales(CarDealerContext context, string inputXml)
    {
        IMapper mapper = InitializeAutoMapper();
        XmlHelper xmlHelper = new XmlHelper();

        ImportSaleDto[] saleDtos = xmlHelper
            .Deserialize<ImportSaleDto[]>(inputXml, "Sales");

        ICollection<Sale> validSales = new HashSet<Sale>();

        foreach (var dto in saleDtos)
        {
            if(!context.Cars.Any(c => c.Id == dto.CarId))
            {
                continue;
            }

            Sale sale = mapper.Map<Sale>(dto);

            validSales.Add(sale);
        }

        context.Sales.AddRange(validSales);
        context.SaveChanges();

        return $"Successfully imported {validSales.Count}";
    }

    public static string GetCarsWithDistance(CarDealerContext context)
    {
        string ret = "";

        // Not needed for manual mapping.
        IMapper mapper = InitializeAutoMapper();
        XmlHelper xmlHelper = new XmlHelper();

        ExportCarDto[] exportCarDTOs = context.Cars
            .Where(c => c.TraveledDistance > 2000000)
            .OrderBy(c => c.Make)
            .ThenBy(c => c.Model)
            .Take(10)
            .ProjectTo<ExportCarDto>(mapper.ConfigurationProvider)
            .ToArray();

        ret = xmlHelper.Serialize<ExportCarDto[]>(exportCarDTOs, "cars");


        return ret;
    }

    public static string GetCarsFromMakeBmw(CarDealerContext context)
    {
        IMapper mapper = InitializeAutoMapper();
        XmlHelper xmlHelper = new XmlHelper();

        ExportBmwCarDto[] bmwCars = context.Cars
            .Where(c => c.Make.ToUpper() == "BMW")
            .OrderBy(c => c.Model)
            .ThenByDescending(c => c.TraveledDistance)
            .ProjectTo<ExportBmwCarDto>(mapper.ConfigurationProvider)
            .ToArray();

        return xmlHelper.Serialize(bmwCars, "cars");
    }

    public static string GetLocalSuppliers(CarDealerContext context)
    {
        string ret = "";
        XmlHelper xmlHelper = new XmlHelper();

        // Not needed for manual mapping.
        IMapper mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CarDealerProfile>();
        }));

        ExportLocalSupplierDto[] exportLocalSupplierDTOs = context.Suppliers
            .Where(s => s.IsImporter == false)
            .ProjectTo<ExportLocalSupplierDto>(mapper.ConfigurationProvider)
            .ToArray();

        // Helper class from Utilities folder.
        ret = xmlHelper.Serialize<ExportLocalSupplierDto[]>(exportLocalSupplierDTOs, "suppliers");


        return ret;
    }

    public static string GetCarsWithTheirListOfParts(CarDealerContext context)
    {
        IMapper mapper = InitializeAutoMapper();
        XmlHelper xmlHelper = new XmlHelper();

        ExportCarWithPartsDto[] carsWithParts = context
            .Cars
            .OrderByDescending(c => c.TraveledDistance)
            .ThenBy(c => c.Model)
            .Take(5)
            .ProjectTo<ExportCarWithPartsDto>(mapper.ConfigurationProvider)
            .ToArray();

        return xmlHelper.Serialize(carsWithParts, "cars");
    }

    public static string GetTotalSalesByCustomer(CarDealerContext context)
    {
        XmlHelper xmlHelper = new XmlHelper();

        var tempDto = context.Customers
            .Where(c => c.Sales.Any())
            .Select(c => new
            {
                FullName = c.Name,
                BoughtCars = c.Sales.Count(),
                SalesInfo = c.Sales.Select(s => new
                {
                    Prices = c.IsYoungDriver
                        ? s.Car.PartsCars.Sum(p => Math.Round((double)p.Part.Price * 0.95, 2))
                        : s.Car.PartsCars.Sum(p => (double)p.Part.Price)
                }).ToArray(),
            })
            .ToArray();

        TotalSalesByCustomerDto[] totalSalesDtos = tempDto
            .OrderByDescending(t => t.SalesInfo.Sum(s => s.Prices))
            .Select(t => new TotalSalesByCustomerDto()
            {
                FullName = t.FullName,
                BoughtCars = t.BoughtCars,
                SpentMoney = t.SalesInfo.Sum(s => s.Prices).ToString("f2")
            })
            .ToArray();

        return xmlHelper.Serialize<TotalSalesByCustomerDto[]>(totalSalesDtos, "customers");
    }

    public static string GetSalesWithAppliedDiscount(CarDealerContext context)
    {
        XmlHelper xmlHelper = new XmlHelper();

        SalesWithAppliedDiscountDto[] salesDtos = context
            .Sales
            .Select(s => new SalesWithAppliedDiscountDto()
            {
                SingleCar = new SingleCar()
                {
                    Make = s.Car.Make,
                    Model = s.Car.Model,
                    TraveledDistance = s.Car.TraveledDistance
                },
                Discount = (int)s.Discount,
                CustomerName = s.Customer.Name,
                Price = s.Car.PartsCars.Sum(p => p.Part.Price),
                PriceWithDiscount = Math.Round((double)(s.Car.PartsCars.Sum(p => p.Part.Price) * (1 - (s.Discount / 100))), 4)
            })
            .ToArray();

        return xmlHelper.Serialize<SalesWithAppliedDiscountDto[]>(salesDtos, "sales");
    }


    private static IMapper InitializeAutoMapper()
           => new Mapper(new MapperConfiguration(cfg =>
           {
               cfg.AddProfile<CarDealerProfile>();
           }));
}