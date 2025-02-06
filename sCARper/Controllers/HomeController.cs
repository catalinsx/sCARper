using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using sCARper.Models;
using sCARper.Services;
using webscraper;

namespace sCARper.Controllers;
// ReSharper disable All

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult SearchCars()
    {
        return View();
    }
    
    [HttpPost]
    public IActionResult SearchCars(string? brand, string? model, List<string> stores)
    {
        var products = new List<Product>();
        
        foreach(var store in stores){
            Console.WriteLine($"Store: {store}");
        }

        var autovit = new Autovit();
        var autoscout = new Autoscout24();
        var olx = new Olx();

        if (stores.Contains("Autovit"))
        {
            Console.WriteLine("Scraping Autovit");
            products.AddRange(autovit.ScrapeProducts(brand, model));
        }

        if (stores.Contains("AutoScout24"))
        {
            Console.WriteLine("Scraping Autoscout24");
            products.AddRange(autoscout.ScrapeProducts(brand, model));
        }

        if (stores.Contains("Olx"))
        {
            Console.WriteLine("Scraping Olx");
            products.AddRange(olx.ScrapeProducts(brand!.ToLower(), model!.ToLower()));
        }

        
        var productsJson = JsonConvert.SerializeObject(products);
        HttpContext.Session.SetString("products", productsJson);

        return RedirectToAction("ShowResults", "Result");
    }
    

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}