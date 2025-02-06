using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using sCARper.Models;

namespace webscraper
{
    public class Olx
    {
        private readonly HtmlWeb _web = new HtmlWeb
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36"
        };

        public List<Product> ScrapeProducts(string? brand, string? model)
        {
            var products = new ConcurrentBag<Product>();
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--headless");
            chromeOptions.AddArgument("--disable-gpu");

            using var driver = new ChromeDriver(chromeOptions);
            int currentPage = 1;

            while (true)
            {
                string currentUrl = currentPage == 1
                    ? $"https://www.olx.ro/auto-masini-moto-ambarcatiuni/autoturisme/{brand}/?currency=EUR&search%5Bfilter_enum_model%5D%5B0%5D={model}"
                    : $"https://www.olx.ro/auto-masini-moto-ambarcatiuni/autoturisme/{brand}/?page={currentPage}&currency=EUR&search%5Bfilter_enum_model%5D%5B0%5D={model}";
                
                Console.WriteLine($"Navigating to: {currentUrl}");
                driver.Navigate().GoToUrl(currentUrl);
                string renderedHtml = driver.PageSource;

                var doc = new HtmlDocument();
                doc.LoadHtml(renderedHtml);

                var listingNodes = doc.DocumentNode.QuerySelectorAll("div.css-qfzx1y");
                if (listingNodes == null || listingNodes.Count == 0)
                {
                    Console.WriteLine("No listings found. Stopping...");
                    break;
                }

                Parallel.ForEach(listingNodes, node =>
                {
                    var title = HtmlEntity.DeEntitize(node.QuerySelector("h4.css-1sq4ur2")?.InnerText ?? "N/A");
                    var partialOrFullUrl = HtmlEntity.DeEntitize(node.QuerySelector("a")?.Attributes["href"]?.Value) ?? string.Empty;
                    if (partialOrFullUrl.Contains("autovit.ro"))
                        return;
                    var price = HtmlEntity.DeEntitize(node.QuerySelector("p.css-6j1qjp")?.InnerText ?? "N/A");
                    var restOfThem = HtmlEntity.DeEntitize(node.QuerySelector("span.css-6as4g5")?.InnerText ?? "N/A");
                    var location = HtmlEntity.DeEntitize(node.QuerySelector("p.css-1mwdrlh")?.InnerText ?? "N/A");

                    var fullUrl = $"https://www.olx.ro{partialOrFullUrl}";

                    
                    // task launches a new async task to execute enclosed function
                    // wait forces the program to wait for the task to finish before continuing
                    
                    Task.Run(async () =>
                    {
                        var detailImage = await Task.Run(() => ScrapeDetailImage(fullUrl));
                        products.Add(new Product
                        {
                            Name = title,
                            Price = price,
                            Url = fullUrl,
                            Image = detailImage,
                            Year = restOfThem,
                            Location = location
                        });
                    }).Wait();


                    Console.WriteLine($"Olx -> {title}");
                });

                try
                {
                    var nextButton = driver.FindElement(By.CssSelector("a[data-testid='pagination-forward']"));
                    string href = nextButton.GetAttribute("href");

                    if (!href.Contains($"page={currentPage + 1}"))
                    {
                        Console.WriteLine("Next page link does not have a next page. Stopping...");
                        break;
                    }
                }
                catch (NoSuchElementException)
                {
                    Console.WriteLine("No next button. Stopping...");
                    break;
                }

                currentPage++;
            }

            return products.ToList();
        }
        
        private string ScrapeDetailImage(string fullUrl)
        {
            if (string.IsNullOrEmpty(fullUrl))
                return string.Empty;

            try
            {
                var doc = _web.Load(fullUrl);
                var galleryDiv = doc.DocumentNode.SelectSingleNode("//div[@data-testid='image-galery-container']");
                if (galleryDiv == null)
                    return string.Empty;
                var imgNode = galleryDiv.SelectSingleNode(".//img");
                return imgNode?.GetAttributeValue("src", "") ?? string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {fullUrl}: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
