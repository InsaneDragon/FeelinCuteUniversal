using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FeelinCute.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public double Price { get; set; }
        public double? Discount { get; set; }
        public string Image { get; set; }
        public Product(int Id, string Name, string? Description, double Price, double? Discount, string Image)
        {
            this.Id = Id;
            this.Name = Name;
            this.Description = Description;
            this.Price = Price;
            this.Discount = Discount;
            this.Image = Image;
        }
       virtual public double GetDiscountedPrice()
        {
            return Discount != null ? Price - (Discount.Value / 100 * Price) : Price;
        }
        public Product()
        {

        }
    }
    public class ProductForCookie : Product
    {
        public int PCount { get; set; }
        public bool? Checked{ get; set; }


        public ProductForCookie(Product p) : base(p.Id, p.Name, p.Description, p.Price, p.Discount, p.Image)
        {
            PCount = 1;
            Checked = false;
        }
        public ProductForCookie(Product p,int quantity) : base(p.Id, p.Name, p.Description, p.Price, p.Discount, p.Image)
        {
            PCount = quantity;
            Checked = false;
        }
        public ProductForCookie(Product p, bool Checked) : base(p.Id, p.Name, p.Description, p.Price, p.Discount, p.Image)
        {
            PCount = 1;
            this.Checked = Checked;
        }
        public ProductForCookie()
        {
        }
    }
}
