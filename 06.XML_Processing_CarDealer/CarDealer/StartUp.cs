using AutoMapper;
using CarDealer.Data;
using CarDealer.DTOs.Import;
using CarDealer.Models;
using CarDealer.Utilities;

namespace CarDealer;

public class StartUp
{

    public static void Main()
    {
        using CarDealerContext context = new CarDealerContext();
        string inputXml =
            File.ReadAllText("../../../Datasets/sales.xml");

        string result = ImportSales(context, inputXml);
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


    private static IMapper InitializeAutoMapper()
           => new Mapper(new MapperConfiguration(cfg =>
           {
               cfg.AddProfile<CarDealerProfile>();
           }));
}