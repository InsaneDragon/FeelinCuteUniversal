using FeelinCute.Models;
using Microsoft.AspNetCore.Mvc;

namespace FeelinCute.ViewComponents
{
    public class ProductsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<ProductForCookie> products)
        {
            return View(products);
        }
    }
}
