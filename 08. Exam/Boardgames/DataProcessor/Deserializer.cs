namespace Boardgames.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using Boardgames.Data;
    using Boardgames.Data.Models;
    using Boardgames.Data.Models.Enums;
    using Boardgames.DataProcessor.ImportDto;
    using Boardgames.Utilities;
    using Newtonsoft.Json;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedCreator
            = "Successfully imported creator – {0} {1} with {2} boardgames.";

        private const string SuccessfullyImportedSeller
            = "Successfully imported seller - {0} with {1} boardgames.";

        private static XmlHelper xmlHelper;

        public static string ImportCreators(BoardgamesContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();
            xmlHelper = new XmlHelper();

            ImportCreatorDto[] creators =
                xmlHelper.Deserialize<ImportCreatorDto[]>(xmlString, "Creators");

            ICollection<Creator> validCreators = new HashSet<Creator>();

            foreach (var creator in creators)
            {
                if (!IsValid(creator))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                ICollection<Boardgame> validBoardgames = new HashSet<Boardgame>();
                foreach (ImportBoardgameDto boardDto in creator.Boardgames)
                {
                    if (!IsValid(boardDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    Boardgame boardgame = new Boardgame
                    {
                        Name = boardDto.Name,
                        Rating = boardDto.Rating,
                        YearPublished = boardDto.YearPublished,
                        CategoryType = (CategoryType)boardDto.CategoryType,
                        Mechanics = boardDto.Mechanics
                    };

                    validBoardgames.Add(boardgame);
                }
                Creator validCreator = new Creator()
                {
                    FirstName = creator.FirstName,
                    LastName = creator.LastName,
                    Boardgames = validBoardgames
                };

                validCreators.Add(validCreator);

                sb.AppendLine(String.Format(SuccessfullyImportedCreator, creator.FirstName, creator.LastName, validBoardgames.Count));
            }

            context.AddRange(validCreators);
            context.SaveChanges();

            return sb.ToString().Trim();
        }

        public static string ImportSellers(BoardgamesContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            ImportSellerDto[] sellerDtos =
                JsonConvert.DeserializeObject<ImportSellerDto[]>(jsonString);

            ICollection<int> realBoardgameIds = context.Boardgames
                .Select(b => b.Id)
                .ToArray();

            ICollection<Seller> validSellers = new HashSet<Seller>();

            foreach (var dto in sellerDtos)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Seller seller = new Seller()
                {
                    Name = dto.Name,
                    Address = dto.Address,
                    Country = dto.Country,
                    Website = dto.Website
                };

                foreach (var boardgameId in dto.BoardgameIds.Distinct())
                {
                    if (!realBoardgameIds.Contains(boardgameId))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    BoardgameSeller boardgameSeller = new BoardgameSeller()
                    {
                        Seller = seller,
                        BoardgameId = boardgameId
                    };
                    seller.BoardgamesSellers.Add(boardgameSeller);
                }

                validSellers.Add(seller);

                sb
                     .AppendLine(String.Format(SuccessfullyImportedSeller, seller.Name, seller.BoardgamesSellers.Count));
            }

            context.Sellers.AddRange(validSellers);
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
}
