namespace Trucks.DataProcessor;

using System.ComponentModel.DataAnnotations;
using System.Text;
using CarDealer.Utilities;
using Data;
using Trucks.Data.Models;
using Trucks.Data.Models.Enums;
using Trucks.DataProcessor.ImportDto;

using Newtonsoft.Json;

public class Deserializer
{
    private const string ErrorMessage = "Invalid data!";

    private const string SuccessfullyImportedDespatcher
        = "Successfully imported despatcher - {0} with {1} trucks.";

    private const string SuccessfullyImportedClient
        = "Successfully imported client - {0} with {1} trucks.";

    private static XmlHelper xmlHelper;

    public static string ImportDespatcher(TrucksContext context, string xmlString)
    {
        StringBuilder sb = new StringBuilder();
        xmlHelper = new XmlHelper();

        ImportDespatcherDto[] despatchers =
            xmlHelper.Deserialize<ImportDespatcherDto[]>(xmlString, "Despatchers");

        ICollection<Despatcher> validDespatchers = new HashSet<Despatcher>();

        foreach (var despatcher in despatchers)
        {
            if(!IsValid(despatcher))
            {
                sb.AppendLine(ErrorMessage);
                continue;
            }

            ICollection<Truck> validTrucks = new HashSet<Truck>();
            foreach (ImportTruckDto truckDto in despatcher.Trucks)
            {
                if(!IsValid(truckDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Truck truck = new Truck
                {
                    RegistrationNumber = truckDto.RegistrationNumber,
                    VinNumber = truckDto.VinNumber,
                    TankCapacity = truckDto.TankCapacity,
                    CargoCapacity = truckDto.CargoCapacity,
                    CategoryType = (CategoryType)truckDto.CategoryType,
                    MakeType = (MakeType)truckDto.MakeType
                };

                validTrucks.Add(truck);
            }
            Despatcher validDespatcher = new Despatcher()
            {
                Name = despatcher.Name,
                Position = despatcher.Position,
                Trucks = validTrucks
            };
            validDespatchers.Add(validDespatcher);

            sb.AppendLine(String.Format(SuccessfullyImportedDespatcher, despatcher.Name, validTrucks.Count));
        }

        context.AddRange(validDespatchers);
        context.SaveChanges();

        return sb.ToString().Trim();
    }

    public static string ImportClient(TrucksContext context, string jsonString)
    {
        StringBuilder sb = new StringBuilder();

        ImportClientDto[] clientDtos = 
            JsonConvert.DeserializeObject<ImportClientDto[]>(jsonString);

        ICollection<int> realTruckIds = context.Trucks
            .Select(t => t.Id)
            .ToArray();

        ICollection<Client> validClients = new HashSet<Client>();

        foreach (var dto in clientDtos)
        {
            if (!IsValid(dto))
            {
                sb.AppendLine(ErrorMessage);
                continue;
            }

            if(dto.Type == "usual")
            {
                sb.AppendLine(ErrorMessage);
                continue;
            }

            Client client = new Client()
            {
                Name = dto.Name,
                Nationality = dto.Nationality,
                Type = dto.Type
            };

            foreach (var truckId in dto.TrucksId.Distinct())
            {
                if(!realTruckIds.Contains(truckId))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                ClientTruck ct = new ClientTruck
                {
                    Client = client,
                    TruckId = truckId
                };

                client.ClientsTrucks.Add(ct);
            }

            validClients.Add(client);

           sb
                .AppendLine(String.Format(SuccessfullyImportedClient, client.Name, client.ClientsTrucks.Count));
        }

        context.Clients.AddRange(validClients);
        context.SaveChanges();

        return sb.ToString().TrimEnd();
    }

    private static bool IsValid(object dto)
    {
        var validationContext = new ValidationContext(dto);
        var validationResult = new List<ValidationResult>();

        return Validator.TryValidateObject(dto, validationContext, validationResult, true);
    }
}