using FeelinCute.Controllers;
using FeelinCute.Models;
using Microsoft.AspNetCore.Mvc;

namespace FeelinCute.ViewComponents
{
    public class ProductsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<ProductForCookie> products)
        {
            Dictionary<int, string> valuePairs = new Dictionary<int, string>();
            foreach (Product product in products)
            {
                string secondary = DbOperations.GetProductSecondaryImage(product.Id);
                if (!string.IsNullOrEmpty(secondary)) valuePairs.Add(product.Id, secondary);
            }
            ViewBag.SecondaryImages = valuePairs;
            return View(products);
        }
    }
}
