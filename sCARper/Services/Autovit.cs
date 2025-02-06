using System.Globalization;
using HtmlAgilityPack;
using sCARper.Models;

namespace sCARper.Services;

public class Autovit
{
    // instance of HtmlWeb with a custom useragent to avoid being blocked by the website
    private readonly HtmlWeb _web = new()
    {
        UserAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36"
    };

    public List<Product> ScrapeProducts(string? brand, string? model)
    {
        var products = new List<Product>();
        var baseUrl = $"https://www.autovit.ro/autoturisme/{brand}/{model}";
        int pageNumber = 1;
        
        var currentDocument = _web.Load(baseUrl);
        var paginationHtmlElements = currentDocument.QuerySelectorAll("li.ooa-6ysn8b");

        int lastPageNumber = 1;
        var lastPageElement = paginationHtmlElements.LastOrDefault();
        if (lastPageElement != null)
        {
            var lastPageText = lastPageElement.InnerText.Trim();
            if (int.TryParse(lastPageText, out int parsedLastPage))
            {
                lastPageNumber = parsedLastPage;
            }
        }
        
        while (pageNumber <= lastPageNumber)
        {
            var currentPage = pageNumber == 1 ? baseUrl : $"{baseUrl}?page={pageNumber}";
            Console.WriteLine($"Processing page: {currentPage}");
            
            currentDocument = _web.Load(currentPage);
            var productHtmlElements = currentDocument.DocumentNode.QuerySelectorAll("article[data-id]");

            foreach (var productHtmlElement in productHtmlElements)
            {
                var title = HtmlEntity.DeEntitize(productHtmlElement.QuerySelector("h2 a").InnerText);
                var url = HtmlEntity.DeEntitize(productHtmlElement.QuerySelector("a").Attributes["href"].Value);
                var image = HtmlEntity.DeEntitize(productHtmlElement.QuerySelector("img")?.Attributes["src"]?.Value);
                var priceElement = productHtmlElement.QuerySelector("h3.ecit9451");
                var price = HtmlEntity.DeEntitize(priceElement?.InnerText ?? "N/A");
                var mileage = HtmlEntity.DeEntitize(productHtmlElement.QuerySelector("dd[data-parameter='mileage']").InnerText);
                var fuelType = HtmlEntity.DeEntitize(productHtmlElement.QuerySelector("dd[data-parameter='fuel_type']").InnerText);
                var year = HtmlEntity.DeEntitize(productHtmlElement.QuerySelector("dd[data-parameter='year']").InnerText);
                var location = HtmlEntity.DeEntitize(productHtmlElement.QuerySelector("p.ooa-oj1jk2").InnerText);
                
                var product = new Product
                {
                    Name = title,
                    Price = price,
                    Image = image,
                    Url = url,
                    Mileage = mileage,
                    FuelType = fuelType,
                    Year = year,
                    Location = location,
                };
                
                Console.WriteLine($"Autovit -> ${product.Name}");

                products.Add(product);
            }
            pageNumber++;
        }

        return products;
    }
}