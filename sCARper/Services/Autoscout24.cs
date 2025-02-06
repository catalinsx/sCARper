using HtmlAgilityPack;
using System.Collections.Concurrent;
using System.Linq;
using sCARper.Models;

namespace webscraper;

    public class Autoscout24
    {
        private readonly HtmlWeb _web = new()
        {
            UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36"
        };

        public List<Product> ScrapeProducts(string? brand, string? model, int threadCount = 5)
        {
            var productsFromArticle = new List<Product>();
            var baseUrl = $"https://www.autoscout24.ro/lst/{brand}/{model}";
            int pageNumber = 1;
            int lastPageNumber = 1;
            var firstDoc = _web.Load(baseUrl);
            var paginationHtmlElements = firstDoc.QuerySelectorAll("li.pagination-item");
            var lastPageElement = paginationHtmlElements.LastOrDefault();
            
            if (lastPageElement != null)
            {
                var lastPageText = lastPageElement.InnerText.Trim();
                if (int.TryParse(lastPageText, out int parsedLastPage))
                {
                    lastPageNumber = parsedLastPage;
                }
            }

            // looping through all pages of the search results, starting from 1 to lastpagenumber
            // we have all our cars in productHtmlElements, stored in that article[id]
            while (pageNumber <= lastPageNumber)
            {
                var currentPageUrl = pageNumber == 1
                    ? baseUrl
                    : $"{baseUrl}?atype=C&page={pageNumber}";

                var pageDoc = _web.Load(currentPageUrl);
                var productHtmlElements = pageDoc.DocumentNode.QuerySelectorAll("article[id]");

                if (productHtmlElements == null || productHtmlElements.Count == 0)
                {
                    break;
                }
                
                foreach (var productHtmlElement in productHtmlElements)
                {
                    var h2Node = productHtmlElement.QuerySelector("h2");
                    if (h2Node == null) continue;
                    var title = HtmlEntity.DeEntitize(h2Node.InnerText);
                    
                    var linkNode = productHtmlElement.QuerySelector("a");
                    if (linkNode == null) continue;
                    var relativeUrl = linkNode.Attributes["href"]?.Value;
                    if (string.IsNullOrWhiteSpace(relativeUrl)) continue;
                    var fullUrl = "https://www.autoscout24.ro" + relativeUrl;
                    
                    var pictureElement = productHtmlElement.QuerySelector("picture.NewGallery_picture__fNsZr");
                    var imgElement = pictureElement?.QuerySelector("source");
                    var smallImage = HtmlEntity.DeEntitize(imgElement?.GetAttributeValue("srcset", "N/A") ?? "N/A");
                    
                    var priceElement = productHtmlElement.QuerySelector("p.Price_price__APlgs.PriceAndSeals_current_price__ykUpx");
                    var price = HtmlEntity.DeEntitize(priceElement?.InnerText ?? "N/A");
                    
                    var mileageElement = productHtmlElement.QuerySelector("span[data-testid='VehicleDetails-mileage_road']");
                    var mileage = mileageElement != null
                        ? HtmlEntity.DeEntitize(mileageElement.InnerText)
                        : "N/A";
                    
                    var yearElement = productHtmlElement.QuerySelector("span[data-testid='VehicleDetails-calendar']");
                    var year = yearElement != null
                        ? HtmlEntity.DeEntitize(yearElement.InnerText)
                        : "N/A";
                    
                    var fuelElement = productHtmlElement.QuerySelector("span[data-testid='VehicleDetails-gas_pump']");
                    var fuelType = fuelElement != null
                        ? HtmlEntity.DeEntitize(fuelElement.InnerText)
                        : "N/A";

                    var locationElement = productHtmlElement.QuerySelector("span[data-testid='sellerinfo-address']");
                    var location = locationElement != null
                        ? HtmlEntity.DeEntitize(locationElement.InnerText)
                        : "Unknown Location";

                    
                    productsFromArticle.Add(new Product
                    {
                        Name = title,
                        Price = price,
                        Image = smallImage,
                        Url = fullUrl,
                        Mileage = mileage,
                        Year = year,
                        FuelType = fuelType,
                        Location = location
                    });
                }

                pageNumber++;
            }

            // concurrentbag is a threadsafe collection designed and optimized for scenarios where multiple threads add or remove items simultaneously
            var finalProductsBag = new ConcurrentBag<Product>(); // thread-safe

            // parallel is a method in.net that allows to execute a loop in parallel processing items simultaneously
            Parallel.ForEach(productsFromArticle, new ParallelOptions
            {
                // maxdegreeofparallelism is the property that controls the maximum number of threads
                MaxDegreeOfParallelism = threadCount
            },
            basicInfo =>
            {
                // what happen inside?
                /*
                 * for each item inside the loop
                 * 1. loads the detailed page
                 * 2. extracts the bigimage url
                 * 3. creates a product object
                 * 4. adds the product in the concurrentbag
                 */
                
                // how threads work together?
                /*
                 * lets say productsfromarticle contains 10items and we have 3 threads
                 * thread1 starts with item1
                 * thread2 starts with item2
                 * thread3 starts with item3
                 * thread1 continue with item4 and so on till 10.
                 */
                
                // loading the page from the url
                var detailDoc = _web.Load(basicInfo.Url);

                // search for the image in that URL 
                var bigImgNode = detailDoc.DocumentNode
                    .QuerySelector("div.image-gallery-slide.image-gallery-center picture img");

                string bigImageUrl = "N/A";
                if (bigImgNode != null)
                {
                    bigImageUrl = bigImgNode.GetAttributeValue("src", "N/A");
                }
                else
                {
                    var picNode = detailDoc.DocumentNode
                        .QuerySelector("div.image-gallery-slide.image-gallery-center picture");
                    if (picNode != null)
                    {
                        var allSources = picNode.QuerySelectorAll("source");
                        var lastSource = allSources.LastOrDefault();
                        if (lastSource != null)
                        {
                            bigImageUrl = lastSource.GetAttributeValue("srcset", "N/A");
                        }
                    }
                }
                
                var product = new Product
                {
                    Name = basicInfo.Name,
                    Price = basicInfo.Price,
                    Image = bigImageUrl,              
                    Url = basicInfo.Url,
                    Mileage = basicInfo.Mileage,
                    Year = basicInfo.Year,
                    FuelType = basicInfo.FuelType,
                    Location = basicInfo.Location
                };
                
                finalProductsBag.Add(product);
                
                Console.WriteLine($"Autoscout -> ${basicInfo.Name}");
            });
            
            var finalProducts = finalProductsBag.ToList();

            return finalProducts;
        }
    }


