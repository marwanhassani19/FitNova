using System.Text;
using System.Text.Json;

namespace FitNova.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<string> Ask(string prompt)
        {
            // URL CORRETTO E AGGIORNATO (Versione v1beta + gemini-1.5-flash)
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        parts = new[] { new { text = prompt } }
                    }
                }
            };

            try
            {
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // Questo ti aiuterà a capire se il problema è la chiave (403) o l'URL (404)
                    return $"Errore API ({response.StatusCode}): {responseString}";
                }

                using var doc = JsonDocument.Parse(responseString);

                // Parsing sicuro del JSON di Google
                if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                    candidates.GetArrayLength() > 0 &&
                    candidates[0].TryGetProperty("content", out var contentObj) &&
                    contentObj.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0)
                {
                    return parts[0].GetProperty("text").GetString() ?? "L'AI ha risposto senza testo.";
                }

                return "Formato risposta AI non riconosciuto.";
            }
            catch (Exception ex)
            {
                return $"Errore di sistema: {ex.Message}";
            }
        }
    }
}