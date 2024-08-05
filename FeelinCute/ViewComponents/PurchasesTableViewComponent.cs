using FeelinCute.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FeelinCute.ViewComponents
{
    public class PurchasesTableViewComponent:ViewComponent
    {
        public IViewComponentResult Invoke(PurchasePackage[] purchasePackages)
        {
            return View(purchasePackages);
        }
    }
}
