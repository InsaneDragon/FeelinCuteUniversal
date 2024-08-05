using FeelinCute.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FeelinCute.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly string _connectionString;
        public AdminController(string connectionString)
        {
            _connectionString = connectionString;
        }
        public IActionResult Index()
        {
            PurchasePackage purchases = new PurchasePackage(_connectionString);
            return View("PurchaseTable", purchases.GetPurchasePackagesWithPurchases());
        }
        [HttpGet]
        public IActionResult SearchForPurchaseById(string purchaseId)
        {
            PurchasePackage purchasePackage = new PurchasePackage(_connectionString);
            PurchasePackage[] purchasePackages;
            if (string.IsNullOrEmpty(purchaseId))
            {
                purchasePackages = new PurchasePackage[0];
            }
            else
            {
                PurchasePackage purchaseFromId = purchasePackage.SearchPurchasePackageById(purchaseId);
                purchasePackages = purchaseFromId != null ? new PurchasePackage[] { purchaseFromId } : new PurchasePackage[0];
            }
            return View("PurchaseTable", purchasePackages);
        }
        [HttpGet]
        public IActionResult SearchForPurchase(string searchText)
        {
            int maxDist = 1;
            PurchasePackage purchasePackage = new PurchasePackage(_connectionString);
            PurchasePackage[] purchaseFromSearch = purchasePackage.SearchPurchasePackagesLevenshtein(searchText, maxDist);
            return View("PurchaseTable", purchaseFromSearch);
        }
        [HttpPut]
        public void ChangePurchasePackageStatus(string status, string packageId)
        {
            PurchasePackage purchase = new PurchasePackage(_connectionString);
            purchase.ChangeStatus(status, packageId);

        }
    }
}
