using FeelinCute.Models;
using FeelinCute.ViewComponents.Models;
using Microsoft.AspNetCore.Mvc;

namespace FeelinCute.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductsController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public IActionResult ProductsList(int page, int startprice, int endprice)
        {
            CartService service = new CartService(_httpContextAccessor);
            return View(service.CheckIfLiked(GetProductsList(page, startprice, endprice)));
        }
        public IActionResult ProductsListAjax(int page, int startprice, int endprice)
        {
            CartService service = new CartService(_httpContextAccessor);
            return ViewComponent("Products", service.CheckIfLiked(GetProductsList(page, startprice, endprice)));
        }
        [HttpPost]
        public IActionResult ProductsList(string search)
        {
            CartService service = new CartService(_httpContextAccessor);
            List<ProductForCookie> LevenshteinList = service.CheckIfLiked(DbOperations.GetProductsLevenshtein(search, 3));
            if (LevenshteinList.Count == 0)
            {
                return RedirectToAction("NoProductsFound");
            }
            ViewBag.Min = (int)LevenshteinList.Min(product => product.GetDiscountedPrice());
            ViewBag.Max = (int)LevenshteinList.Max(product => product.GetDiscountedPrice());
            ViewBag.Page = 1;
            int productsperpage = 20;
            ViewBag.TotalPages = (int)(LevenshteinList.Count / productsperpage);
            return View(LevenshteinList);
        }
        public IActionResult NoProductsFound()
        {
            return View();
        }
        public Product[] GetProductsList(int page, int startprice, int endprice)
        {
            Filters filters = new Filters { startprice = startprice, endprice = endprice };
            CartService service = new CartService(_httpContextAccessor);
            var MinAndMax = DbOperations.GetMinAndMax();
            ViewBag.Min = MinAndMax.minPrice;
            ViewBag.Max = MinAndMax.maxPrice;
            ViewBag.Page = page;
            int productsperpage = 20;
            int totalproducts = DbOperations.GetProductsCount();
            ViewBag.TotalPages = (int)(totalproducts / productsperpage);
            int startpage = (page - 1) * productsperpage;
            int endpage = page * productsperpage;
            Product[] products = new Product[0];
            products = filters == null ? DbOperations.GetProducts(startpage, endpage) : DbOperations.GetProductsWithFilters(startpage, endpage, filters);
            if (endpage > products.Length)
            {
                endpage = products.Length;
            }
            return (products[startpage..endpage]);
        }

    }
}
