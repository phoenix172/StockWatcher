using System.Text.Json;
using System.Text.Json.Serialization;
using HtmlAgilityPack;

namespace StockWatcher.Core;

public class ProductSource : IProductSource
{
    private readonly HttpClient _client;

    public ProductSource(HttpClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<Product>> Fetch(string url)
    {
        var html = await _client.GetAsync(url);
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html.Content.ReadAsStringAsync().Result);
        var jsonItems = doc.DocumentNode.SelectNodes("//script[@type='application/ld+json']")?
            .Select(x=>JsonSerializer.Deserialize<JsonItem>(x.InnerText))
            .Where(x=> x?.Type == "Product").Cast<JsonItem>() ?? [];
        var products = jsonItems.Select(product => new Product(product.Name, product.Url, IsAvailable(product)));
        return products.ToList();
        
        HtmlNodeCollection? GetAvailabilityMarker(JsonItem product)
        {
            return doc.DocumentNode.SelectNodes($"//a[@href='{product.Url}']/preceding-sibling::div[.//span/img]/span/img");
        }

        bool IsAvailable(JsonItem product)
        {
            var marker = GetAvailabilityMarker(product);
            return marker?.FindFirst("img").Attributes["src"].Value.Contains("green") is true;
        }
    }
    

    private record JsonItem(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("url")] string Url,
        [property: JsonPropertyName("@type")] string Type);
}