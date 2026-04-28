namespace FitNova.Services;

public class FoodService
{
    private readonly HttpClient _http;

    public FoodService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> SearchFood(string name)
    {
        var url = $"https://world.openfoodfacts.org/cgi/search.pl?search_terms={name}&json=true";
        return await _http.GetStringAsync(url);
    }
}