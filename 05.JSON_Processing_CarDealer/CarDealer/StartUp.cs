namespace CarDealer;

using AutoMapper;
using CarDealer.Data;
using CarDealer.DTOs.Import;
using CarDealer.Models;
using Newtonsoft.Json;

public class StartUp
{
    public static void Main()
    {
        CarDealerContext dbContext = new CarDealerContext();
        string inputJson = File.ReadAllText(@"../../../Datasets/sales.json");

        string result = ImportSales(dbContext, inputJson);
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

    private static IMapper CreateMapper()
    {
        return new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CarDealerProfile>();
        }));
    }

}

