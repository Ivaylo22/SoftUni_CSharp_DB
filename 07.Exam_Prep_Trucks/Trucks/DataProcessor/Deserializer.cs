namespace Trucks.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using CarDealer.Utilities;
    using Data;
    using Trucks.Data.Models;
    using Trucks.Data.Models.Enums;
    using Trucks.DataProcessor.ImportDto;

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
        //public static string ImportClient(TrucksContext context, string jsonString)
        //{
        //    throw new NotImplementedException();
        //}

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}