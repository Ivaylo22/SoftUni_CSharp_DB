namespace CarDealer;

using AutoMapper;
using CarDealer.Data;
using CarDealer.DTOs.Import;
using CarDealer.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;

public class StartUp
{
    public static void Main()
    {
        CarDealerContext dbContext = new CarDealerContext();
        //string inputJson = File.ReadAllText(@"../../../Datasets/sales.json");

        string result = GetSalesWithAppliedDiscount(dbContext);
        Console.WriteLine(result);
    }

    public static string ImportSuppliers(CarDealerContext context, string inputJson)
    {
        IMapper mapper = CreateMapper();

        ImportSupliersDto[] supplierDtos = 
            JsonConvert.DeserializeObject<ImportSupliersDto[]>(inputJson);

        Supplier[] validSuppliers = mapper.Map<Supplier[]>(supplierDtos);

        context.Suppliers.AddRange(validSuppliers);
        context.SaveChanges();

        return $"Successfully imported {validSuppliers.Length}.";
    }

    public static string ImportParts(CarDealerContext context, string inputJson)
    {
        IMapper mapper = CreateMapper();

        ImportPartsDto[] partDtos = JsonConvert.DeserializeObject<ImportPartsDto[]>(inputJson);

        ICollection<Part> parts = new List<Part>();

        foreach (var dto in partDtos)
        {
            if(context.Suppliers.Any(s => s.Id == dto.SupplierId))
            {
                Part part = mapper.Map<Part>(dto);
                parts.Add(part);
            }
        }

        context.Parts.AddRange(parts);
        context.SaveChanges();

        return $"Successfully imported {parts.Count}.";

    }

    public static string ImportCars(CarDealerContext context, string inputJson)
    {
        List<PartCar> parts = new List<PartCar>();
        List<Car> cars = new List<Car>();

        ImportCarsDto[] carDtos = JsonConvert.DeserializeObject<ImportCarsDto[]>(inputJson);


        foreach (var dto in carDtos)
        {
            Car car = new Car()
            {
                Make = dto.Make,
                Model = dto.Model,
                TraveledDistance = dto.TraveledDistance
            };

            cars.Add(car);

            foreach (var part in dto.PartsId.Distinct())
            {
                PartCar partCar = new PartCar()
                {
                    Car = car,
                    PartId = part,
                };
                parts.Add(partCar);
            }
        }

        context.Cars.AddRange(cars);
        context.PartsCars.AddRange(parts);
        context.SaveChanges();

        return $"Successfully imported {cars.Count}.";
    }

    public static string ImportCustomers(CarDealerContext context, string inputJson)
    {
        IMapper mapper = CreateMapper();

        ImportCustomersDto[] customerDtos = JsonConvert.DeserializeObject<ImportCustomersDto[]>(inputJson);

        Customer[] customers = mapper.Map<Customer[]>(customerDtos);

        context.Customers.AddRange(customers);
        context.SaveChanges();

        return $"Successfully imported {customers.Length}.";
    }

    public static string ImportSales(CarDealerContext context, string inputJson)
    {
        IMapper mapper = CreateMapper();

        ImportSalesDto[] saleDtos = JsonConvert.DeserializeObject<ImportSalesDto[]>(inputJson);

        Sale[] sales = mapper.Map<Sale[]>(saleDtos);

        context.Sales.AddRange(sales);
        context.SaveChanges();

        return $"Successfully imported {sales.Length}.";
    }

    public static string GetOrderedCustomers(CarDealerContext context)
    {
        var customers = context.Customers
            .OrderBy(c => c.BirthDate)
            .ThenBy(c => c.IsYoungDriver)
            .Select(c => new
            {
                c.Name,
                BirthDate = c.BirthDate.ToString(@"dd/MM/yyyy", CultureInfo.InvariantCulture),
                c.IsYoungDriver
            })
            .AsNoTracking()
            .ToArray();

        return JsonConvert.SerializeObject(customers, Formatting.Indented);
    }

    public static string GetCarsFromMakeToyota(CarDealerContext context)
    {
        var cars = context.Cars
            .Where(c => c.Make == "Toyota")
            .OrderBy(c => c.Model)
            .ThenByDescending(c => c.TraveledDistance)
            .Select(c => new
            {
                c.Id,
                c.Make,
                c.Model,
                c.TraveledDistance
            })
            .AsNoTracking()
            .ToArray();

        return JsonConvert.SerializeObject(cars, Formatting.Indented);
    }

    public static string GetLocalSuppliers(CarDealerContext context)
    {
        var suppliers = context.Suppliers
            .Where(s => s.IsImporter == false)
            .Select(s => new
            {
                s.Id,
                s.Name,
                PartsCount = s.Parts.Count
            })
            .AsNoTracking()
            .ToArray();

        return JsonConvert.SerializeObject(suppliers, Formatting.Indented);
    }

    public static string GetCarsWithTheirListOfParts(CarDealerContext context)
    {
        var cars = context.Cars
            .Select(c => new
            {
                car = new
                {
                    c.Make,
                    c.Model,
                    c.TraveledDistance
                },
                parts = c.PartsCars
                    .Select(p => new
                        {
                            p.Part.Name,
                            Price = $"{p.Part.Price:f2}"
                        })
                .ToArray()
            })
            .ToArray();

        return JsonConvert.SerializeObject(cars, Formatting.Indented);

    }

    public static string GetTotalSalesByCustomer(CarDealerContext context)
    {
        var customers = context.Customers
            .Where(c => c.Sales.Any())
            .Select(c => new
            {
                fullName = c.Name,
                boughtCars = c.Sales.Count,
                salePrices = c.Sales.SelectMany(s => s.Car.PartsCars.Select(p => p.Part.Price))
            })
            .ToArray();

        var totalSalesByCustomer = customers.Select(t => new
        {
            t.fullName,
            t.boughtCars,
            spentMoney = t.salePrices.Sum()
        })
            .OrderByDescending(t => t.spentMoney)
            .ThenByDescending(t => t.boughtCars)
            .ToArray();

        return JsonConvert.SerializeObject(totalSalesByCustomer, Formatting.Indented);
    }

    public static string GetSalesWithAppliedDiscount(CarDealerContext context)
    {
        var salesWithDiscount = context.Sales
                .Take(10)
                .Select(s => new
                {
                    car = new
                    {
                        s.Car.Make,
                        s.Car.Model,
                        s.Car.TraveledDistance
                    },
                    customerName = s.Customer.Name,
                    discount = $"{s.Discount:f2}",
                    price = $"{s.Car.PartsCars.Sum(p => p.Part.Price):f2}",
                    priceWithDiscount = $"{s.Car.PartsCars.Sum(p => p.Part.Price) * (1 - s.Discount / 100):f2}"
                })
                .ToArray();

        return JsonConvert.SerializeObject(salesWithDiscount, Formatting.Indented);
    }

    private static IMapper CreateMapper()
    {
        return new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CarDealerProfile>();
        }));
    }

}

