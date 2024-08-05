using FeelinCute.Models;
using Microsoft.AspNetCore.Mvc;

namespace FeelinCute.ViewComponents
{
    public class EmailPopupViewComponent : ViewComponent
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmailPopupViewComponent(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public IViewComponentResult Invoke()
        {
            CartService service = new CartService(_httpContextAccessor);
            if (service.GetStringFromCookie("EmailPopUp") == null)
            {
                service.SetCookie("EmailPopUp", string.Empty);
                return View(service.GetListFromCookie<ProductForCookie>("Cart"));
            }
            else return Content(string.Empty);
        }
    }
}
