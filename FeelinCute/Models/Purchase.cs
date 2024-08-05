using Dapper;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Emit;

namespace FeelinCute.Models
{
    public class Purchase : Product
    {
        private readonly string _connectionString;
        public Purchase()
        {
        }
        public Purchase(string connectionString)
        {
            _connectionString = connectionString;
        }
        [Required(ErrorMessage = "ProductPrice is required.")]
        public double ProductPrice { get; set; }
        public double? ProductDiscount { get; set; }
        [Required(ErrorMessage = "ProductId is required.")]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "PurchaseId is required.")]
        public string PurchaseId { get; set; }
        [Required(ErrorMessage = "ProductCount is required.")]
        public int ProductCount { get; set; }
       override public double GetDiscountedPrice()
        {
            return ProductDiscount != null ? ProductPrice - (ProductDiscount.Value / 100 * ProductPrice) : ProductPrice;
        }

        public Purchase(string connectionString, double ProductPrice, double? ProductDiscount, int ProductId, string PurchaseId, int ProductCount)
        {
            _connectionString = connectionString;
            this.Id = Id;
            this.ProductPrice = ProductPrice;
            this.ProductDiscount = ProductDiscount;
            this.ProductId = ProductId;
            this.PurchaseId = PurchaseId;
            this.ProductCount = ProductCount;
        }
        public void AddPurchaseToDb()
        {
            using (var dbContext = new SqlConnection(_connectionString))
            {

                string query = @"INSERT INTO Purchases 
                 (ProductPrice, ProductDiscount, ProductId, PurchaseId, ProductCount)
                 VALUES (@ProductPrice, @ProductDiscount, @ProductId, @PurchaseId, @ProductCount)";
                dbContext.Execute(query, new
                {
                    ProductPrice = ProductPrice,
                    ProductDiscount = ProductDiscount,
                    PurchaseId = PurchaseId,
                    ProductId = ProductId,
                    ProductCount = ProductCount
                });
            }
        }
    }
}
