using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using sCARper.Models;
//ReSharper disable All

namespace sCARper.Controllers;

public class ResultController : Controller
{
   
    public IActionResult ShowResults(int page = 1, string? fuelType = null, int? minYear = null, int? maxMileage = null, decimal? maxPrice = null)
    {
        // retrieving the products from the current session 
        var productJson = HttpContext.Session.GetString("products");
        var products = string.IsNullOrEmpty(productJson)
            ? new List<Product>()
            : JsonConvert.DeserializeObject<List<Product>>(productJson);

        
        // applying filters
        if (!string.IsNullOrEmpty(fuelType))
            products = products?.Where(p => p.FuelType == fuelType).ToList();

        if (minYear.HasValue)
            products = products?.Where(p => int.TryParse(p.Year, out var year) && year >= minYear.Value).ToList();

        if (maxMileage.HasValue)
            products = products?.Where(p => int.TryParse(p.Mileage, out var mileage) && mileage <= maxMileage.Value).ToList();

        if (maxPrice.HasValue)
            products = products?.Where(p => decimal.TryParse(p.Price, out var price) && price <= maxPrice.Value).ToList();

        // pagination
        const int pageSize = 30;
        int skip = (page - 1) * pageSize;
        var paginatedProducts = products?.Skip(skip).Take(pageSize).ToList();

        int totalProducts = products.Count;
        int totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

        //viewbag is used to pass data to the view
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        
        ViewBag.FuelType = fuelType;
        ViewBag.MinYear = minYear;
        ViewBag.MaxMileage = maxMileage;
        ViewBag.MaxPrice = maxPrice;

        return View(paginatedProducts);
    }

}