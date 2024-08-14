using FeelinCute.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;

namespace FeelinCute.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            Product[] products = DbOperations.GetProducts();
            CartService service = new CartService(_httpContextAccessor);
            Dictionary<int, string> valuePairs = new Dictionary<int, string>();
            foreach (Product product in products)
            {
              string secondary =  DbOperations.GetProductSecondaryImage(product.Id);
                if (!string.IsNullOrEmpty(secondary)) valuePairs.Add(product.Id,secondary);
            }
            ViewBag.SecondaryImages=valuePairs;
            return View(service.CheckIfLiked(products));
        }
        public IActionResult ProductDetails(int productId)
        {
            ViewBag.ProductImages = DbOperations.GetProductImages(productId);
            CartService service = new CartService(_httpContextAccessor);
            ViewBag.IsInCart = service.CheckIfInCookie(productId);
            return View(service.CheckIfLiked(DbOperations.GetProduct(productId)));
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult PrivacyPolicy()
        {
            return View();
        }
        public IActionResult TermsOfService()
        {
            return View();
        }

    }
}