using FeelinCute.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            CartService service = new CartService(_httpContextAccessor);
            return View(service.CheckIfLiked(DbOperations.GetProducts()));
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