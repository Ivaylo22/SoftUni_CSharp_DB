namespace BookShop
{
    using BookShop.Models.Enums;
    using Data;
    using Initializer;
    using System.Linq;
    using System.Text;

    public class StartUp
    {
        public static void Main()
        {
            using var dbContext = new BookShopContext();
            //DbInitializer.ResetDatabase(dbContext);

            var input = Console.ReadLine();

            Console.WriteLine(GetBooksByCategory(dbContext, input));
            
        }

        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {

            var hasParsed = Enum.TryParse<AgeRestriction>(command, true, out AgeRestriction ageRestrictionObj);

            if(hasParsed)
            {
                var books = context.Books
                    .Where(b => b.AgeRestriction == ageRestrictionObj)
                    .OrderBy(b => b.Title)
                    .Select(b => b.Title)
                    .ToArray();

                return String.Join(Environment.NewLine, books);
            }

            return null;

        }

        public static string GetGoldenBooks(BookShopContext context)
        {
            var books = context.Books
                .OrderBy(b => b.BookId)
                .Where(b => b.Copies < 5000 &&
                            b.EditionType == EditionType.Gold)
                .Select(b => b.Title)
                .ToArray();

            return String.Join(Environment.NewLine, books);
        }

        public static string GetBooksByPrice(BookShopContext context)
        {
            var sb = new StringBuilder();
            var books = context.Books
                .Where(b => b.Price > 40)
                .OrderByDescending(b => b.Price)
                .Select(b => new
                {
                    b.Title,
                    b.Price
                })
                .ToArray();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} - ${book.Price:f2}");
            }

            return sb.ToString().Trim();
        }

        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            var books = context.Books
                .Where(b => b.ReleaseDate.Value.Year != year)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)
                .ToArray();

            return String.Join(Environment.NewLine, books);
        }

        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            var categories = input
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Select(b => b.ToLower())
                .ToArray();

            var books = context.Books
                .Where(b => b.BookCategories
                    .Any(bc => categories.Contains(bc.Category.Name.ToLower())))
                .OrderBy(b => b.Title)
                .Select(b => b.Title)
                .ToArray();

            return String.Join(Environment.NewLine, books);
        }
    }
}


