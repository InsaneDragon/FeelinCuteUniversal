using FeelinCute.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;

namespace FeelinCute.Controllers
{
    public class TokenServices
    {
        private readonly HttpClient _httpClient;
        private readonly SJAuthentication _auth;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Session keys for access token and expiry date
        private const string AccessTokenKey = "CJ_AccessToken";
        private const string TokenExpiryKey = "CJ_TokenExpiry";

        public TokenServices(HttpClient httpClient, SJAuthentication auth, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _auth = auth;
            _httpContextAccessor = httpContextAccessor;
        }

        // Method to get the access token and store in session
        public async Task<string> GetAccessTokenAsync()
        {
            var session = _httpContextAccessor.HttpContext.Session;

            // Check if token exists in session and is still valid
            var accessToken = session.GetString(AccessTokenKey);
            var expiryDateString = session.GetString(TokenExpiryKey);
            if (!string.IsNullOrEmpty(accessToken) && DateTime.TryParse(expiryDateString, out DateTime tokenExpiryDate) && tokenExpiryDate > DateTime.UtcNow)
            {
                return accessToken; // Return cached token
            }

            // Request a new token if it's not available or expired
            var json = JsonConvert.SerializeObject(new
            {
                email = _auth.Email,
                password = _auth.Password
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://developers.cjdropshipping.com/api2.0/v1/authentication/getAccessToken", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<AuthResponse>(jsonResponse);

                if (tokenResponse.Code == 200 && tokenResponse.Result)
                {
                    // Save the token and expiry date to session
                    accessToken = tokenResponse.Data.AccessToken;
                    var _tokenExpiryDate = DateTime.Parse(tokenResponse.Data.AccessTokenExpiryDate);

                    session.SetString(AccessTokenKey, accessToken);
                    session.SetString(TokenExpiryKey, _tokenExpiryDate.ToString());

                    return accessToken;
                }
                else
                {
                    throw new Exception("Failed to get access token: " + tokenResponse.Message);
                }
            }

            throw new HttpRequestException("Failed to get access token.");
        }


        // Method to make API requests with the access token
        public async Task<string> GetProtectedDataAsync(string url)
        {
            var accessToken = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new HttpRequestException($"Request to {url} failed with status code {response.StatusCode}");
            }
        }
        public async Task<string> GetProductIdBySKUAsync(string sku)
        {
            try
            {
                // Use the existing access token logic
                string accessToken = await GetAccessTokenAsync();
                int pageNum = 1;
                int pageSize = 100; // Adjust according to your needs
                string foundPid = null;

                while (true)
                {
                    var requestUrl = $"https://developers.cjdropshipping.com/api2.0/v1/product/list?pageNum={pageNum}&pageSize={pageSize}";
                    var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                    request.Headers.Add("CJ-Access-Token", accessToken);

                    HttpResponseMessage response = await _httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var productsResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);

                        // Check for matching SKU in the current page of products
                        var foundProducts = productsResponse.Data.List.Where(p => p.ProductSku == sku).ToList();

                        if (foundProducts.Any())
                        {
                            foundPid = foundProducts.First().Pid; // Get the first matching product ID
                            break; // Exit the loop if the PID is found
                        }

                        // Check if more products are available
                        if (productsResponse.Data.Total <= (pageNum * pageSize))
                        {
                            break; // Exit the loop if no more products are available
                        }

                        pageNum++; // Increment the page number for the next request
                        await Task.Delay(1000);
                    }
                    else
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Failed to retrieve products. Status Code: {response.StatusCode}, Error: {errorMessage}");
                    }
                }

                if (foundPid != null)
                {
                    return foundPid; // Return the found product ID
                }
                else
                {
                    throw new Exception($"Product with SKU {sku} not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving product ID: {ex.Message}");
            }
        }

        // Define the response model for product list
        public class ProductListResponse
        {
            public List<Product> Products { get; set; }
        }

        public class Product
        {
            public string PID { get; set; }
            public string SKU { get; set; }
            // Include other relevant product properties if needed
        }
        public async Task<string> PlaceOrderAsync(UserInfoForCheckOut userinfo, ProductForCookie product)
        {
            try
            {
                // Retrieve product details from CJ Dropshipping by SKU
                string productId = await GetProductIdBySKUAsync(product.SKU);
                string accessToken = await GetAccessTokenAsync();

                // Get product details
                var productDetailUrl = $"https://developers.cjdropshipping.com/api2.0/v1/product/query?pid={productId}";
                var productDetailRequest = new HttpRequestMessage(HttpMethod.Get, productDetailUrl);
                productDetailRequest.Headers.Authorization = new AuthenticationHeaderValue("CJ-Access-Token", accessToken);

                var productResponse = await _httpClient.SendAsync(productDetailRequest);
                if (!productResponse.IsSuccessStatusCode)
                {
                    var error = await productResponse.Content.ReadAsStringAsync();
                    throw new HttpRequestException("Failed to get product details: " + error);
                }

                // Deserialize the product details response
                var productDetailsJson = await productResponse.Content.ReadAsStringAsync();
                var productDetails = JsonConvert.DeserializeObject<ProductDetailsResponse>(productDetailsJson);
                var variants = productDetails.Variants;

                // Prepare the full address
                string fullAddress = userinfo.Apt != 0 ? $"{userinfo.StreetAddress} {userinfo.Apt}" : userinfo.StreetAddress;

                // Build the order payload
                var orderPayload = new
                {
                    orderNumber = "3182954819304c49869fe0956ef55a0b",  // Replace with your order number
                    shippingZip = userinfo.ZipCode,
                    shippingCountry = "US",    // Replace with the shipping country
                    shippingCountryCode = "US",
                    shippingProvince = "YOUR_PROVINCE",  // Replace with the shipping province
                    shippingCity = userinfo.City,
                    shippingCounty = "YOUR_COUNTY",      // Replace with the shipping county
                    shippingPhone = userinfo.PhoneNumber,
                    shippingCustomerName = $"{userinfo.FirstName} {userinfo.LastName}",
                    shippingAddress = fullAddress,
                    taxId = "YOUR_TAX_ID",                // Optional tax ID
                    remark = "YOUR_REMARK",               // Optional remarks
                    email = userinfo.Email,
                    consigneeID = "YOUR_CONSIGNEE_ID",    // Optional consignee ID
                    payType = 2,                          // Set to 2 for balance payment or 3 for other
                    shopAmount = "YOUR_SHOP_AMOUNT",      // Optional shop amount
                    logisticName = "PostNL",              // Example shipping method
                    fromCountryCode = "CN",
                    iossType = "YOUR_IOSS_TYPE",          // Optional IOSS type
                    platform = "shopify",                 // Specify your platform
                    iossNumber = "YOUR_IOSS_NUMBER",      // Optional IOSS number
                    products = variants.Select(v => new
                    {
                        vid = v.VariantID,                // Use the variant ID from the product details
                        quantity = product.PCount       // Product quantity from the order
                    }).ToArray()
                };

                // Serialize the payload to JSON
                var jsonPayload = JsonConvert.SerializeObject(orderPayload);

                // Create the order request
                var orderRequest = new HttpRequestMessage(HttpMethod.Post, "https://developers.cjdropshipping.com/api2.0/v1/shopping/order/createOrderV2")
                {
                    Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                };

                // Add the authorization header
                orderRequest.Headers.Authorization = new AuthenticationHeaderValue("CJ-Access-Token", accessToken);

                // Send the order creation request
                var orderResponse = await _httpClient.SendAsync(orderRequest);

                if (orderResponse.IsSuccessStatusCode)
                {
                    // Success - Return the response content
                    return await orderResponse.Content.ReadAsStringAsync();
                }
                else
                {
                    // Failure - Handle the error response
                    var errorMessage = await orderResponse.Content.ReadAsStringAsync();
                    throw new Exception("Failed to create order: " + errorMessage);
                }
            }
            catch (Exception ex)
            {
                // Catch and throw any errors during order creation
                throw new Exception("Error creating order: " + ex.Message);
            }
        }


    }
}
public class ProductDetailsResponse
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("price")]
    public decimal Price { get; set; }

    [JsonProperty("variants")]
    public List<ProductVariant> Variants { get; set; }
}

public class ProductVariant
{
    [JsonProperty("variantID")]
    public string VariantID { get; set; }

    [JsonProperty("sku")]
    public string SKU { get; set; }

    [JsonProperty("quantity")]
    public int Quantity { get; set; }

    [JsonProperty("price")]
    public decimal Price { get; set; }
}
public class ApiResponse
{
    public int Code { get; set; }
    public bool Result { get; set; }
    public string Message { get; set; }
    public ResponseData Data { get; set; }
    public string RequestId { get; set; }
}

public class ResponseData
{
    public int PageNum { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public List<ProductResponse> List { get; set; }
}

public class ProductResponse
{
    public string Pid { get; set; }
    public string ProductName { get; set; }
    public string ProductNameEn { get; set; }
    public string ProductSku { get; set; }
    public string ProductImage { get; set; }
    public string ProductWeight { get; set; }
    public string ProductType { get; set; }
    public string ProductUnit { get; set; }
    public string SellPrice { get; set; }
    public string CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string SourceFrom { get; set; }
    public string Remark { get; set; }
    public string CreateTime { get; set; }
}