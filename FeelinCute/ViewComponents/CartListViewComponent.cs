using FeelinCute.Controllers;
using FeelinCute.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FeelinCute.ViewComponents
{
    public class CartListViewComponent : ViewComponent
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartListViewComponent(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public IViewComponentResult Invoke()
        {
            CartService service = new CartService(_httpContextAccessor);
            return View(service.GetListFromCookie<ProductForCookie>("Cart"));
        }
    }
}
