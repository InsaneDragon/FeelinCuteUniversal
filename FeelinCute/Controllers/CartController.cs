using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using FeelinCute.Models;
using Newtonsoft.Json;

namespace FeelinCute.Controllers
{
    public class CartController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        public JsonResult AddToCart(int productid, int? amount)
        {
            try
            {
                int quantity = amount ?? 1;
                CartService service = new CartService(_httpContextAccessor);
                List<ProductForCookie> CartList = service.GetListFromCookie<ProductForCookie>("Cart");
                ProductForCookie product = CartList.Where(p => p.Id == productid).FirstOrDefault();
                if (product != null)
                    product.PCount+=quantity;
                else
                    CartList.Add(new ProductForCookie(DbOperations.GetProduct(productid), quantity));
                service.SetCookie("Cart", CartList);
                return Json(CartList);
            }
            catch (Exception ex)
            {
                return new JsonResult(ex);
            }
        }
        [HttpPut]
        public JsonResult ChangeProductAmount(int productid, string operation)
        {
            try
            {
                CartService service = new CartService(_httpContextAccessor);
                List<ProductForCookie> CartList = service.GetListFromCookie<ProductForCookie>("Cart");
                if (operation == ">")
                {
                    CartList.Where(p => p.Id == productid).FirstOrDefault().PCount++;
                }
                else if (operation == "<")
                {
                    CartList.Where(p => p.Id == productid).FirstOrDefault().PCount--;
                }
                service.SetCookie("Cart", CartList);
                return new JsonResult("Success");

            }
            catch (Exception ex)
            {
                return new JsonResult(ex);
            }
        }
        [HttpDelete]
        public JsonResult RemoveCartProduct(int productid)
        {
            try
            {
                CartService service = new CartService(_httpContextAccessor);
                List<ProductForCookie> CartList = service.GetListFromCookie<ProductForCookie>("Cart");
                CartList.Remove(CartList.Where(p => p.Id == productid).FirstOrDefault());
                service.SetCookie("Cart", CartList);
                return new JsonResult("Success");
            }
            catch (Exception ex)
            {
                return new JsonResult(ex);
            }
        }
        [HttpPost]
        public JsonResult AddLikedItem(int productid)
        {
            try
            {
                CartService service = new CartService(_httpContextAccessor);
                List<ProductForLiked> LikedList = service.GetListFromCookie<ProductForLiked>("Liked");
                Product product = DbOperations.GetProduct(productid);
                LikedList.Add(new ProductForLiked(product.Id, product.Name, product.Image));
                service.SetCookie("Liked", LikedList);
                return Json(LikedList);
            }
            catch (Exception ex)
            {
                return new JsonResult(ex);
            }
        }
        [HttpDelete]
        public JsonResult RemoveLikedItem(int productid)
        {
            try
            {
                CartService service = new CartService(_httpContextAccessor);
                List<ProductForLiked> LikedList = service.GetListFromCookie<ProductForLiked>("Liked");
                Product product = DbOperations.GetProduct(productid);
                LikedList.Remove(LikedList.Where(p => p.Id == productid).FirstOrDefault());
                service.SetCookie("Liked", LikedList);
                return Json(LikedList);
            }
            catch (Exception ex)
            {
                return new JsonResult(ex);
            }
        }
        public IActionResult CheckOut()
        {
            CartService service = new CartService(_httpContextAccessor);
            List<ProductForCookie> CartList = service.GetListFromCookie<ProductForCookie>("Cart");
            return View(CartList);
        }
        [HttpGet]
        public double GetProductsSum()
        {
            CartService service = new CartService(_httpContextAccessor);
            List<ProductForCookie> CartList = service.GetListFromCookie<ProductForCookie>("Cart");
            return CartList.Sum(product =>
            {
                double sum = product.GetDiscountedPrice() * product.PCount;
                return sum;
            });
        }
    }
}
