using Newtonsoft.Json;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Mvc;
using FeelinCute.Models;
using FeelinCute.Controllers;

namespace FeelinCute.Models
{
    public class CartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        [HttpGet]
        public List<T> GetListFromCookie<T>(string CookieName) // Specify the type parameter <T> for the method
        {
            List<T> products = new List<T>();
            try
            {
                string cartlist = _httpContextAccessor.HttpContext.Session.GetString(CookieName);
                if (!string.IsNullOrEmpty(cartlist))
                {
                    products = JsonConvert.DeserializeObject<List<T>>(cartlist);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while deserializing cookie: {ex.Message}");
            }
            return products;
        }
        public string GetStringFromCookie(string CookieName) // Specify the type parameter <T> for the method
        {
            try
            {
                string cookie = _httpContextAccessor.HttpContext.Session.GetString(CookieName);
                return (cookie);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while deserializing cookie: {ex.Message}");
                return null;
            }
        }

        public bool CheckIfInCookie(int id)// Check if product is in cookie
        {
            List<ProductForLiked> InCartList = GetListFromCookie<ProductForLiked>("Cart");
            return InCartList.Any(p => p.Id == id);
        }

        public List<ProductForCookie> CheckIfLiked(Product[] products)// Check if Products are liked
        {
            List<ProductForCookie> CheckedList = new List<ProductForCookie>();
            CartService service = new CartService(_httpContextAccessor);
            List<ProductForLiked> LikedProducts = service.GetListFromCookie<ProductForLiked>("Liked");
            foreach (Product product in products)
            {
                CheckedList.Add(new ProductForCookie(product, LikedProducts.Any(p => p.Id == product.Id)));
            }
            return CheckedList;

        }
        public ProductForCookie CheckIfLiked(Product product)// Check if Products are liked
        {
            List<ProductForCookie> CheckedList = new List<ProductForCookie>();
            CartService service = new CartService(_httpContextAccessor);
            List<ProductForLiked> LikedProducts = service.GetListFromCookie<ProductForLiked>("Liked");
            return new ProductForCookie(product, LikedProducts.Any(p => p.Id == product.Id));
        }
        public void SetCookie(string CookieName, object products)
        {
            _httpContextAccessor.HttpContext.Session.SetString(CookieName, JsonConvert.SerializeObject(products));
        }
    }
}
