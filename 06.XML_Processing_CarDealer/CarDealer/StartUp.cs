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
            File.ReadAllText("../../../Datasets/parts.xml");

        string result = ImportParts(context, inputXml);
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


    private static IMapper InitializeAutoMapper()
           => new Mapper(new MapperConfiguration(cfg =>
           {
               cfg.AddProfile<CarDealerProfile>();
           }));
}