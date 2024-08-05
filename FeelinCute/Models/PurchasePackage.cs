using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace FeelinCute.Models
{
    public class PurchasePackage
    {
        private readonly string _connectionString;
        public PurchasePackage()
        {
        }
        public PurchasePackage(string connectionString)
        {
            _connectionString = connectionString;
        }
        public PurchasePackage(string connectionString, string Id, string ClientName, string ClientEmail, int ClientPhoneNumber, string ClientAddress, string State, int AptNumber, string ZipCode)
        {
            _connectionString = connectionString;
            this.Id = Id;
            this.ClientName = ClientName;
            this.ClientEmail = ClientEmail;
            this.ClientPhoneNumber = ClientPhoneNumber == 0 ? null : ClientPhoneNumber;
            this.ClientAddress = ClientAddress;
            this.State = State;
            this.AptNumber = AptNumber == 0 ? null : AptNumber;
            this.PurchaseDate = DateTime.Now;
            this.ZipCode = ZipCode;
            Status = "Pending";
        }

        [Required(ErrorMessage = "Id is required.")]
        private string Id { get; set; }

        [Required(ErrorMessage = "Client name is required.")]
        public string ClientName { get; set; }

        [Required(ErrorMessage = "Client email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string ClientEmail { get; set; }

        [Required(ErrorMessage = "Client phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public int? ClientPhoneNumber { get; set; }

        [Required(ErrorMessage = "Client address is required.")]
        public string ClientAddress { get; set; }

        [Required(ErrorMessage = "Purchase date is required.")]
        public DateTime PurchaseDate { get; set; }

        [Required(ErrorMessage = "State is required.")]
        public string State { get; set; }

        public int? AptNumber { get; set; }

        [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid zip code format.")]
        public string? ZipCode { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; }
        public Purchase[] Purchases { get; set; }
        public string GetId()
        {
            return Id;
        }
        public static bool CheckIfIdExists(string Id, SqlConnection dbContext)
        {
                int result = dbContext.ExecuteScalar<int>("select case when exists (select 1 from PurchasePackages where Id=@id) then 1 else 0 end", new { id = Id });
                bool exists = result == 1 ? true : false;
                return exists;
        }
        public readonly string[] AllowedStatuses = { "Pending", "Processing", "Shipped", "Delivered", "Cancelled", "On Hold", "Backordered", "Returned" };
        public void AddPurchasePackageToDb()
        {
            using (var dbContext = new SqlConnection(_connectionString))
            {

                string query = @"INSERT INTO PurchasePackages 
                 (Id, ClientName, ClientEmail, ClientPhoneNumber, ClientAddress, 
                  PurchaseDate, State, AptNumber, ZipCode, Status)
                 VALUES (@Id, @ClientName, @ClientEmail, @ClientPhoneNumber, @ClientAddress, 
                         @PurchaseDate, @State, @AptNumber, @ZipCode, @Status)";
                dbContext.Execute(query, new
                {
                    Id = Id,
                    ClientName = ClientName,
                    ClientEmail = ClientEmail,
                    ClientPhoneNumber = ClientPhoneNumber,
                    ClientAddress = ClientAddress,
                    PurchaseDate = PurchaseDate,
                    State = State,
                    AptNumber = AptNumber,
                    ZipCode = ZipCode, // Will insert NULL if ZipCode is null
                    Status = Status
                });
            }
        }
        public PurchasePackage[] GetPurchasePackagesWithPurchases()
        {
            using (var dbContext = new SqlConnection(_connectionString))
            {
                var purchasePackages = dbContext.Query<PurchasePackage>("SELECT * FROM PurchasePackages").ToArray();
                foreach (var package in purchasePackages)
                {
                    package.Purchases = GetPurchasesForPackage(package.Id, dbContext);
                }
                return purchasePackages;
            }
        }

        private Purchase[] GetPurchasesForPackage(string packageId, SqlConnection dbContext)
        {
            var query = $@"SELECT *  
                   FROM Purchases 
                   INNER JOIN Products ON Purchases.ProductId = Products.Id 
                   WHERE Purchases.PurchaseId = '{packageId}'";
            return dbContext.Query<Purchase>(query).ToArray();
        }
        public void ChangeStatus(string Status, string Id)
        {
            if (AllowedStatuses.Contains(Status))
            {
                using (var dbContext = new SqlConnection(_connectionString))
                {
                    dbContext.Execute("UPDATE PurchasePackages SET status = @Status WHERE Id = @id", new { status = Status, id = Id });
                }
            }
        }
        public PurchasePackage[] SearchPurchasePackagesLevenshtein(string search, int maxdist)
        {
            using (var dbContext = new SqlConnection(_connectionString))
            {
                var query = $"SELECT * FROM (SELECT *, dbo.LevenshteinDistance(ClientEmail, @search, @maxdist) AS Distance FROM PurchasePackages) AS Distances WHERE Distance < @maxdist ORDER BY Distance;";
                PurchasePackage[] purchasePackages = dbContext.Query<PurchasePackage>(query, new { search, maxdist }).ToArray();
                foreach (var package in purchasePackages)
                {
                    package.Purchases = GetPurchasesForPackage(package.Id, dbContext);
                }
                return purchasePackages;
            }
        }
        public PurchasePackage SearchPurchasePackageById(string id)
        {
            using (var dbContext = new SqlConnection(_connectionString))
            {
                var query = $"SELECT * FROM PurchasePackages WHERE RIGHT(Id,15)=@search";
                PurchasePackage purchasePackage = dbContext.Query<PurchasePackage>(query, new { search = id }).SingleOrDefault();
                if (purchasePackage != null)
                {
                    purchasePackage.Purchases = GetPurchasesForPackage(purchasePackage.Id, dbContext);
                }
                return purchasePackage;
            }
        }
    }
}
