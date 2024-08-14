using FeelinCute.Models;
using Dapper;
using Microsoft.SqlServer;
using Microsoft.Data.SqlClient;
using EmailService;
using FeelinCute.ViewComponents.Models;

namespace FeelinCute.Controllers
{
    public class DbOperations
    {
        public static string ConnectionString = "Data Source=Home-PC;Initial Catalog=Estore;Integrated Security=True;TrustServerCertificate=true";
        public static Product[] GetProducts()
        {
            using (var dbContext = new SqlConnection(ConnectionString))
            {
                return dbContext.Query<Product>("select * from Products where Active='True'").ToArray();
            }
        }
        public static Product[] GetProducts(int start, int end)
        {
            using (var dbContext = new SqlConnection(ConnectionString))
            {
                return dbContext.Query<Product>($"SELECT * FROM Products ORDER BY (SELECT NULL) OFFSET {start} ROWS FETCH NEXT {end} ROWS ONLY").ToArray();
            }
        }
        public static Product[] GetProductsWithFilters(int start, int end, Filters filters)
        {
            string filtertext = "";
            if (filters.startprice != 0)
            {
                filtertext += $"AND (CASE WHEN Discount IS NOT NULL THEN Price - (Discount / 100.0 * Price) ELSE Price END) >= {filters.startprice} ";
            }
            if (filters.endprice != 0)
            {
                filtertext += $"AND (CASE WHEN Discount IS NOT NULL THEN Price - (Discount / 100.0 * Price) ELSE Price END) <= {filters.endprice} ";
            }
            using (var dbContext = new SqlConnection(ConnectionString))
            {
                return dbContext.Query<Product>($"SELECT * FROM Products Where 1=1 " + filtertext + $"ORDER BY (SELECT NULL) OFFSET {start} ROWS FETCH NEXT {end} ROWS ONLY").ToArray();
            }
        }
        public static Product GetProduct(int productId)
        {
            using (var dbContext = new SqlConnection(ConnectionString))
            {
                return dbContext.QueryFirstOrDefault<Product>($"select * from Products where Id={productId}");
            }
        }
        public static int GetProductsCount()
        {
            using (var dbContext = new SqlConnection(ConnectionString))
            {
                return dbContext.QueryFirstOrDefault<int>("select count(*) from Products");
            }
        }
        public static string[] GetProductImages(int productId)
        {
            using (var dbContext = new SqlConnection(ConnectionString))
            {
                return dbContext.Query<string>($"select Imagename from Images where Productid={productId}").ToArray();
            }
        }
        public static string GetProductSecondaryImage(int productId)
        {
            using (var dbContext = new SqlConnection(ConnectionString))
            {
                return dbContext.QueryFirstOrDefault<string>($"select TOP 1 Imagename from Images where Productid={productId}");
            }
        }
        public static (int minPrice, int maxPrice) GetMinAndMax()
        {
            using (var dbContext = new SqlConnection(ConnectionString))
            {
                var result = dbContext.QueryFirstOrDefault<(int minPrice, int maxPrice)>(
                @"SELECT MIN(CASE WHEN Discount IS NOT NULL THEN Price - (Discount / 100.0 * Price) ELSE Price END) AS MinPrice,
                         MAX(CASE WHEN Discount IS NOT NULL THEN Price - (Discount / 100.0 * Price) ELSE Price END) AS MaxPrice
                  FROM Products"
            );
                return result;
            }
        }
        public static Product[] GetProductsLevenshtein(string search, int maxdist)
        {
            using (var dbContext = new SqlConnection(ConnectionString))
            {
                var query = $"SELECT * FROM (SELECT *, dbo.LevenshteinDistance(Name, @search, @maxdist) AS Distance FROM Products) AS Distances WHERE Distance < @maxdist ORDER BY Distance;";
                return dbContext.Query<Product>(query, new { search, maxdist }).ToArray();
            }
        }
        public static List<(string Id, string Email)> GetUsersEmailsAndIds()
        {
            using (var dbContext = new SqlConnection(ConnectionString))
            {
                var emailsAndIds = dbContext.Query<(string Id, string Email)>(
                  @"SELECT Id, Email FROM GuestsEmails
                  UNION
                  SELECT Id, Email FROM AspNetUsers").ToList();
                return emailsAndIds;

            }
        }
        public static bool AddGuestEmail(string Email)
        {
            using (var dbContext = new SqlConnection(ConnectionString))
            {
                // Check if the email already exists
                bool emailExists = dbContext.QuerySingleOrDefault<bool>("SELECT CASE WHEN EXISTS (SELECT 1 FROM GuestsEmails WHERE Email = @Email) THEN 1 ELSE 0 END", new { Email });
                    
                // If the email already exists, return false
                if (emailExists)
                {
                    return false;
                }

                // If the email doesn't exist, attempt to insert it
                try
                {
                    // Insert the email
                    string Id = GenerateUniqueCode(449);
                    dbContext.Execute("INSERT INTO GuestsEmails VALUES (@Id, @Email)", new { Id, Email });
                    return true; // Return true indicating successful insertion
                }
                catch (SqlException ex)
                {
                    return false; // Return false indicating failure
                }
            }
        }
        public Promotion[] GetPromotions()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                string query = @"
                SELECT Id, Title, Description, ImagePath, ExpirationDate, Code
                FROM Promotions
                WHERE Url IS NULL OR Url = ''";
                Promotion[] promotions = connection.Query<Promotion>(query).ToArray();
                return promotions;
            }
        }
        public static Promotion GetOnePromotionToSend(string userId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var today = DateTime.Today;
                var query = @"
        SELECT TOP 1 p.Id, p.Title, p.Description, p.ImagePath, p.ExpirationDate
        FROM Promotions p
        WHERE p.ExpirationDate >= @Today
        AND p.Id NOT IN (
            SELECT up.PromotionId
            FROM UserPromotions up
            WHERE up.UserId = @UserId
        )
        ORDER BY NEWID()"; // Random promotion (SQL Server syntax)

                return connection.QueryFirstOrDefault<Promotion>(query, new { UserId = userId, Today = today });
            }
        }
        public static void RecordPromotionSent(string userId, int promotionId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Execute("INSERT INTO UserPromotions (UserId, PromotionId, SentDate) VALUES (@UserId, @PromotionId, @SentDate)",
    new { UserId = userId, PromotionId = promotionId, SentDate = DateTime.Now });
            }
        }
        static string GenerateUniqueCode(int count)
        {
            var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var code = new char[count];
            var random = new Random();
            for (int i = 0; i < code.Length; i++)
            {
                code[i] = characters[random.Next(characters.Length)];
            }

            return new string(code);
        }
    }

}

