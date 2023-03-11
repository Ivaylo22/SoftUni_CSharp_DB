namespace BookShop
{
    using BookShop.Models.Enums;
    using Data;
    using Initializer;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public class StartUp
    {
        public static void Main()
        {
            using var dbContext = new BookShopContext();
            //DbInitializer.ResetDatabase(dbContext);

            //var input = int.Parse(Console.ReadLine());

            Console.WriteLine(GetMostRecentBooks(dbContext));
            
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

        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            StringBuilder sb = new StringBuilder();

            DateTime fullDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var books = context.Books
                .Where(b => b.ReleaseDate.Value < fullDate)
                .OrderByDescending(b => b.ReleaseDate)
                .Select(b => new
                {
                    b.Title,
                    b.EditionType,
                    b.Price
                })
                .ToArray();

            foreach (var b in books)
            {
                sb
                    .AppendLine($"{b.Title} - {b.EditionType} - ${b.Price:f2}");
            }

            return sb.ToString().Trim();
        }

        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var authors = context.Authors
                .ToArray()
                .Where(a => a.FirstName.EndsWith(input))
                .Select(a => $"{a.FirstName} {a.LastName}")
                .OrderBy(a => a)
                .ToArray();

            return String.Join(Environment.NewLine, authors);
        }

        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            var books = context.Books
                .ToArray()
                .Where(b => b.Title.ToLower().Contains(input.ToLower()))
                .OrderBy(b => b.Title)
                .Select(b => b.Title)
                .ToArray();

            return String.Join(Environment.NewLine, books);
        }

        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var sb = new StringBuilder();

            var books = context.Books
                .Where(b => b.Author.LastName.ToLower().StartsWith(input.ToLower()))
                .OrderBy(b => b.BookId)
                .Select(b => new
                {
                    b.Title,
                    Author = $"{b.Author.FirstName} {b.Author.LastName}"
                })
                .ToArray();

            foreach (var b in books)
            {
                sb
                    .AppendLine($"{b.Title} ({b.Author})");
            }

            return sb.ToString().Trim();
        }

        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            var books = context.Books
                .Where(b => b.Title.Length > lengthCheck)
                .ToArray();

            return books.Length;
        }

        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var authors = context.Authors
                .Select(a => new
                {
                    FullName = $"{a.FirstName} {a.LastName}",
                    CountBooks = a.Books.Sum(b => b.Copies)
                })
                .OrderByDescending(a => a.CountBooks)
                .ToArray();

            string result = string.Join(Environment.NewLine, authors.Select(a => $"{a.FullName} - {a.CountBooks}"));
            return result;
        }

        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var categories = context.Categories
                .Select(c => new
                {
                    c.Name,
                    Profit = c.CategoryBooks
                        .Sum(cb => cb.Book.Price * cb.Book.Copies)
                })
                .ToArray()
                .OrderByDescending(c => c.Profit)
                .ThenBy(c => c.Name);

            return String.Join(Environment.NewLine, categories.Select(c => $"{c.Name} ${c.Profit:f2}"));
        }

        public static string GetMostRecentBooks(BookShopContext context)
        {
            StringBuilder sb = new StringBuilder();

            var categories = context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    c.Name,
                    Books = c.CategoryBooks
                            .Select(cb => new
                            {
                                cb.Book.Title,
                                cb.Book.ReleaseDate
                            })
                            .OrderByDescending(cb => cb.ReleaseDate)
                            .Take(3)
                            .ToArray()
                })
                .ToArray();

            foreach (var c in categories)
            {
                sb
                    .AppendLine($"--{c.Name}");

                foreach (var b in c.Books)
                {
                    sb
                        .AppendLine($"{b.Title} ({b.ReleaseDate.Value.Year})");
                }
            }

            return sb.ToString().Trim();
        }

        public static void IncreasePrices(BookShopContext context)
        {
            var books = context.Books
                .Where(b => b.ReleaseDate.Value.Year < 2010 &&
                            b.ReleaseDate.HasValue)
                .ToArray();

            foreach (var book in books)
            {
                book.Price += 5;
            }

            context.SaveChanges();
        }

        public static int RemoveBooks(BookShopContext context)
        {
            var booksToDelete = context.Books
                .Where(b => b.Copies < 4200)
                .ToArray();

            context.RemoveRange(booksToDelete);

            context.SaveChanges();

            return booksToDelete.Length;
        }

    }
}


