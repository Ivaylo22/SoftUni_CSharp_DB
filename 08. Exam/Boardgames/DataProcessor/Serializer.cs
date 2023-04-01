namespace Boardgames.DataProcessor;

using Boardgames.Data;
using Boardgames.DataProcessor.ExportDto;
using Boardgames.Utilities;
using Newtonsoft.Json;

public class Serializer
{
    private static XmlHelper xmlHelper;

    public static string ExportCreatorsWithTheirBoardgames(BoardgamesContext context)
    {
        xmlHelper = new XmlHelper();

        var creators = context
            .Creators
            .ToArray()
            .Where(c => c.Boardgames.Any())
            .Select(c => new ExportCreatorDto()
            {
                CreatorName = c.FirstName + " " + c.LastName,
                BoardgamesCount = c.Boardgames.Count,
                Boardgames = c.Boardgames
                    .Select(b => new ExportBoardgameDto()
                    {
                        BoardgameName = b.Name,
                        BoardgameYearPublished = b.YearPublished
                    })
                    .OrderBy(b => b.BoardgameName)
                    .ToArray()
            })
            .OrderByDescending(c => c.BoardgamesCount)
            .ThenBy(c => c.CreatorName)
            .ToArray();

        return xmlHelper.Serialize(creators, "Creators");
    }

    public static string ExportSellersWithMostBoardgames(BoardgamesContext context, int year, double rating)
    {
        var sellers = context.Sellers
            .ToArray()
            .Where(s => s.BoardgamesSellers.Any(bs => bs.Boardgame.YearPublished >= year && bs.Boardgame.Rating <= rating))
            .Select(s => new
            {
                Name = s.Name,
                Website = s.Website,
                Boardgames = s.BoardgamesSellers
                    .Where(b => b.Boardgame.YearPublished >= year && b.Boardgame.Rating <= rating)
                    .Select(b => new
                    {
                        Name = b.Boardgame.Name,
                        Rating = b.Boardgame.Rating,
                        Mechanics = b.Boardgame.Mechanics,
                        Category = b.Boardgame.CategoryType.ToString()
                    })
                    .OrderByDescending(b => b.Rating)
                    .ThenBy(b => b.Name)
                    
            })
            .OrderByDescending(s => s.Boardgames.Count())
            .ThenBy(s => s.Name)
            .Take(5)
            .ToArray();

        return JsonConvert.SerializeObject(sellers, Formatting.Indented);
    }
}