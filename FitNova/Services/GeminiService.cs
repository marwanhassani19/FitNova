using System.Net.Http.Json;
using System.Text.Json;

namespace FitNova.Services;

public class GeminiService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public GeminiService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<string> Ask(string prompt)
    {
        var key = _config["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(key))
            return "⚠️ Chiave API Gemini non configurata in appsettings.json → Gemini:ApiKey";

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={key}";
        var body = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } }
        };

        try
        {
            var res = await _http.PostAsJsonAsync(url, body);
            var json = await res.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("error", out var err))
                return $"⚠️ Errore Gemini: {err.GetProperty("message").GetString()}";

            var text = root
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "Risposta vuota.";

            return text;
        }
        catch (Exception ex)
        {
            return $"⚠️ Errore di connessione: {ex.Message}";
        }
    }
}
