using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace FitNova.Services;

public class FoodService
{
    private readonly HttpClient _http;
    private readonly string _usdaApiKey = "IhfDrgLw5Xr5aNALRWrS8j046Czg3DezWbCLI0nI";

    public FoodService(HttpClient http)
    {
        _http = http;
        // User-Agent richiesto da OpenFoodFacts per non farsi bloccare
        _http.DefaultRequestHeaders.Add("User-Agent", "FitNovaApp/1.0 (info@fitnova.it)");
    }

    // 1. Cerca alimenti generici in inglese tramite USDA (es. "chicken")
    public async Task<string> SearchGenericFood(string query)
    {
        var url = $"https://api.nal.usda.gov/fdc/v1/foods/search?query={Uri.EscapeDataString(query)}&pageSize=6&api_key={_usdaApiKey}";
        return await _http.GetStringAsync(url);
    }

    // 2. Cerca codici a barre nel database italiano di OpenFoodFacts
    public async Task<string> SearchByBarcode(string barcode)
    {
        var url = $"https://it.openfoodfacts.org/api/v0/product/{barcode}.json";
        return await _http.GetStringAsync(url);
    }
}