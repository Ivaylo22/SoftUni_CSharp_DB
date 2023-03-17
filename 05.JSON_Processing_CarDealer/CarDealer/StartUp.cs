namespace CarDealer;

using AutoMapper;
using CarDealer.Data;
using CarDealer.Models;
using Newtonsoft.Json;

public class StartUp
{
    public static void Main()
    {
        CarDealerContext dbContext = new CarDealerContext();
        string inputJson = File.ReadAllText(@"../../../Datasets/suppliers.json");

        string result = ImportSuppliers(dbContext, inputJson);
        Console.WriteLine(result);
    }

    public static string ImportSuppliers(CarDealerContext context, string inputJson)
    {
        IMapper mapper = CreateMapper();

        Supplier[] supplierDtos = JsonConvert.DeserializeObject<Supplier[]>(inputJson);

        Supplier[] validSuppliers = mapper.Map<Supplier[]>(supplierDtos);

        context.Suppliers.AddRange(validSuppliers);
        context.SaveChanges();

        return $"Successfully imported {validSuppliers.Length}.";
    }

    private static IMapper CreateMapper()
    {
        return new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CarDealerProfile>();
        }));
    }

}

