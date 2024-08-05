using EmailService;
using FeelinCute.Areas.Identity.Data;
using FeelinCute.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FeelinCute.Controllers
{
    public class CheckOutController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<AppUser> _userManager;
        private readonly string _connectionString;
        private readonly AdminOptions _adminOptions;
        private readonly EmailSender _emailSender;

        public CheckOutController(IHttpContextAccessor context, UserManager<AppUser> userManager, string connectionString, AdminOptions adminOptions, EmailConfiguraion emailConfig)
        {
            _connectionString = connectionString;
            _httpContextAccessor = context;
            _userManager = userManager;
            _adminOptions = adminOptions;
            _emailSender = new EmailSender(emailConfig);
        }

        public ActionResult CheckOutView(int? productId)
        {
            CartService service = new CartService(_httpContextAccessor);
            List<ProductForCookie> products = new List<ProductForCookie>();
            if (productId.HasValue)
            {
                List<ProductForCookie> CartList = service.GetListFromCookie<ProductForCookie>("Cart");
                ProductForCookie ExistingProduct = CartList.FirstOrDefault(p => p.Id == productId);
                if (ExistingProduct == null)
                {
                    ProductForCookie newProduct = new ProductForCookie(DbOperations.GetProduct(productId.Value));
                    products.Add(newProduct);
                    CartList.Add(newProduct);
                }
                else
                {
                    products.Add(ExistingProduct);
                }
                service.SetCookie("Cart", CartList);
            }
            else
            {
                products = service.GetListFromCookie<ProductForCookie>("Cart");
            }
            ViewBag.ProductList = products;
            service.SetCookie("CheckOut", products);
            SetUserInfoIfExists(_userManager);
            return View();
        }

        public async void SetUserInfoIfExists(UserManager<AppUser> userManager)
        {
            AppUser user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                ViewBag.User = user;
            }
        }

        public void MakeAPurchase(UserInfoForCheckOut userInfo)
        {
            try
            {
                CartService service = new CartService(_httpContextAccessor);
                List<ProductForCookie> Purchases = service.GetListFromCookie<ProductForCookie>("CheckOut");
                if (Purchases.Count > 0)
                {
                    string PurchaseId = GenerateRandomID(49);
                    using (var dbContext = new SqlConnection(_connectionString))
                    {
                        while (PurchasePackage.CheckIfIdExists(PurchaseId, dbContext))
                        {
                            PurchaseId = GenerateRandomID(49);
                        }
                    }
                    PurchasePackage package = new PurchasePackage(_connectionString, PurchaseId, userInfo.FirstName + " " + userInfo.LastName, userInfo.Email, userInfo.PhoneNumber, userInfo.StreetAddress, userInfo.State, userInfo.Apt, userInfo.ZipCode);
                    package.AddPurchasePackageToDb();
                    List<Purchase> purchaseList = new List<Purchase>();
                    Purchases.ForEach(product => purchaseList.Add(new Purchase(_connectionString, product.Price, product.Discount, product.Id, PurchaseId, product.PCount)));
                    purchaseList.ForEach(x => x.AddPurchaseToDb());
                    SendOrdersToAdminEmail(package, Purchases);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void SendOrdersToAdminEmail(PurchasePackage package, List<ProductForCookie> purchaseList)
        {
            var productIds = purchaseList.Select(p => p.Id.ToString()).ToList();
            var productPrices = purchaseList.Select(p => p.Price).ToList();
            var productDiscounts = purchaseList.Select(p => p.Discount).ToList();
            var productCounts = purchaseList.Select(p => p.PCount).ToList();
            var imageNames = purchaseList.Select(p => p.Image).ToList();

            _emailSender.SendOrdersToAdminEmail(
                package.GetId(),
                package.ClientName,
                package.ClientEmail,
                package.ClientPhoneNumber.ToString() ?? "N/A",
                package.ClientAddress,
                package.State,
                package.AptNumber.ToString() ?? "N/A",
                package.ZipCode,
                package.PurchaseDate.ToString(),
                package.Status,
                productIds,
                productPrices,
                productDiscounts,
                productCounts,
                imageNames,
                _adminOptions.AdminEmail
            );
        }

        [HttpPost]
        public ActionResult ConfirmPurchaseView(UserInfoForCheckOut userInfo)
        {
            MakeAPurchase(userInfo);
            return View();
        }

        public static string GenerateRandomID(int length)
        {
            Random random = new Random();
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] id = new char[length];
            for (int i = 0; i < length; i++)
            {
                id[i] = chars[random.Next(chars.Length)];
            }
            return new string(id);
        }
    }
}
