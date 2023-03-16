namespace ProductShop;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Data;
using DTOs.Import;
using Models;

public class StartUp
{
    public static void Main()
    {   
        ProductShopContext dbContext = new ProductShopContext();
        //string inputJson = File.ReadAllText(@"../../../Datasets/categories-products.json");

        string result = GetUsersWithProducts(dbContext);

        Console.WriteLine(result);
    }

    public static string ImportUsers(ProductShopContext context, string inputJson)
    {
        IMapper mapper = CreateMapper();

        ImportUserDto[] userDtos = 
            JsonConvert.DeserializeObject<ImportUserDto[]>(inputJson);

        ICollection<User> users = new HashSet<User>();

        foreach (var userDto in userDtos)
        {
            User user = mapper.Map<User>(userDto);

            users.Add(user);
        }

        context.Users.AddRange(users);
        context.SaveChanges();

        return $"Successfully imported {users.Count}";
    }

    public static string ImportProducts(ProductShopContext context, string inputJson)
    {
        IMapper mapper = CreateMapper();

        ImportProductDto[] productDtos = 
            JsonConvert.DeserializeObject<ImportProductDto[]>(inputJson);

        Product[] products = mapper.Map<Product[]>(productDtos);

        context.AddRange(products);
        context.SaveChanges();

        return $"Successfully imported {products.Length}";
    }

    public static string ImportCategories(ProductShopContext context, string inputJson)
    {
        IMapper mapper = CreateMapper();

        ImportCategoryDto[] categoryDtos = JsonConvert.DeserializeObject<ImportCategoryDto[]>(inputJson);

        ICollection<Category> validCategories = new HashSet<Category>();

        foreach (var catDto in categoryDtos)
        {
            if(string.IsNullOrEmpty(catDto.Name))
            {
                continue;
            }

            Category category = mapper.Map<Category>(catDto);
            validCategories.Add(category);    
        }
        context.AddRange(validCategories);
        context.SaveChanges();

        return $"Successfully imported {validCategories.Count}";
    }

    public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
    {
        IMapper mapper = CreateMapper();

        ImportCategoryProductDto[] cpDtos =
            JsonConvert.DeserializeObject<ImportCategoryProductDto[]>(inputJson);

        ICollection<CategoryProduct> validEntries = new HashSet<CategoryProduct>();
        foreach (ImportCategoryProductDto cpDto in cpDtos)
        {
            // This is not wanted in the description but we do it for security
            // In Judge locally they may not be added previously
            // JUDGE DO NOT LIKE THIS VALIDATION BELOW!!!!!
            if (!context.Categories.Any(c => c.Id == cpDto.CategoryId) ||
                !context.Products.Any(p => p.Id == cpDto.ProductId))
            {
                continue;
            }

            CategoryProduct categoryProduct =
                mapper.Map<CategoryProduct>(cpDto);
            validEntries.Add(categoryProduct);
        }

        context.CategoriesProducts.AddRange(validEntries);
        context.SaveChanges();

        return $"Successfully imported {validEntries.Count}";
    }

    public static string GetProductsInRange(ProductShopContext context)
    {
        IMapper mapper = CreateMapper();

        var products = context.Products
            .Where(p => p.Price >= 500 && p.Price <= 1000)
            .OrderBy(p => p.Price)
            .Select(p => new
            {
                name = p.Name,
                price = p.Price,
                seller = p.Seller.FirstName + " " + p.Seller.LastName
            })
            .AsNoTracking()
            .ToArray();

        return JsonConvert.SerializeObject(products, Formatting.Indented);
    }

    public static string GetSoldProducts(ProductShopContext context)
    {
        IContractResolver contractResolver = ConfigureCamelCaseNaming();

        var usersWithSoldProducts = context.Users
            .Where(u => u.ProductsSold.Any(p => p.Buyer != null))
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Select(u => new
            {
                u.FirstName,
                u.LastName,
                SoldProducts = u.ProductsSold
                    .Where(p => p.Buyer != null)
                    .Select(p => new
                    {
                        p.Name,
                        p.Price,
                        BuyerFirstName = p.Buyer.FirstName,
                        BuyerLastName = p.Buyer.LastName
                    })
                    .ToArray()
            })
            .AsNoTracking()
            .ToArray();

        return JsonConvert.SerializeObject(usersWithSoldProducts,
            Formatting.Indented,
            new JsonSerializerSettings()
            {
                ContractResolver = contractResolver
            });
    }

    public static string GetCategoriesByProductsCount(ProductShopContext context)
    {
        IContractResolver contractResolver = ConfigureCamelCaseNaming();

        var categories = context.Categories
            .OrderByDescending(c => c.CategoriesProducts.Count)
            .Select(c => new
            {
                Category = c.Name,
                ProductsCount = c.CategoriesProducts.Count,
                AveragePrice = c.CategoriesProducts.Average(p => p.Product.Price).ToString("f2"),
                TotalRevenue = c.CategoriesProducts.Sum(p => p.Product.Price).ToString("f2")
            })
            .AsNoTracking()
            .ToArray();

        return JsonConvert.SerializeObject(categories,
                Formatting.Indented,
                new JsonSerializerSettings()
                {
                    ContractResolver = contractResolver
                });
    }

    public static string GetUsersWithProducts(ProductShopContext context)
    {
        IContractResolver contractResolver = ConfigureCamelCaseNaming();

        var users = context
            .Users
            .Where(u => u.ProductsSold.Any(p => p.Buyer != null))
            .Select(u => new
            {
                u.FirstName,
                u.LastName,
                u.Age,
                SoldProducts = new
                {
                    Count = u.ProductsSold
                        .Count(p => p.Buyer != null),
                    Products = u.ProductsSold
                        .Where(p => p.Buyer != null)
                        .Select(p => new
                        {
                            p.Name,
                            p.Price
                        })
                        .ToArray()
                }
            })
            .OrderByDescending(u => u.SoldProducts.Count)
            .AsNoTracking()
            .ToArray();

        var userWrapperDto = new
        {
            UsersCount = users.Length,
            Users = users
        };

        return JsonConvert.SerializeObject(userWrapperDto,
            Formatting.Indented,
            new JsonSerializerSettings()
            {
                ContractResolver = contractResolver,
                NullValueHandling = NullValueHandling.Ignore
            });
    }


    private static IMapper CreateMapper()
    {
        return new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProductShopProfile>();
        }));
    }

    private static IContractResolver ConfigureCamelCaseNaming()
    {
        return new DefaultContractResolver()
        {
            NamingStrategy = new CamelCaseNamingStrategy(false, true)
        };
    }
}