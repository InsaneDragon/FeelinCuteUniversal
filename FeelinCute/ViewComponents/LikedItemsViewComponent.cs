using FeelinCute.Controllers;
using FeelinCute.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FeelinCute.ViewComponents
{
    public class LikedItemsViewComponent : ViewComponent
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LikedItemsViewComponent(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public IViewComponentResult Invoke()
        {
            CartService service = new CartService(_httpContextAccessor);
            return View(service.GetListFromCookie<ProductForLiked>("Liked"));
        }
    }
}
