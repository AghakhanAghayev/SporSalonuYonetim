using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Web; // URL Encode için gerekli

namespace SporSalonuYonetim.Controllers
{
    [Authorize]
    public class YapayZekaController : Controller
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public YapayZekaController(IConfiguration configuration)
        {
            _apiKey = configuration["GeminiApiKey"];
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Danis(int yas, int boy, int kilo, string cinsiyet, string hedef, bool gorselIste)
        {
            try
            {
                // 1. Prompt Hazırlığı
                StringBuilder promptBuilder = new StringBuilder();
                promptBuilder.AppendLine($"Ben {yas} yaşında, {boy} cm boyunda ve {kilo} kg ağırlığında bir {cinsiyet} bireyim.");
                promptBuilder.AppendLine($"Hedefim: {hedef}.");
                promptBuilder.AppendLine("Bana haftalık detaylı antrenman ve beslenme programı hazırla. Türkçe olsun.");

                if (gorselIste)
                {
                    promptBuilder.AppendLine("\n--- ÖZEL İSTEK ---");
                    promptBuilder.AppendLine("Cevabının EN SONUNA, '###RESIM_KODU:' diye bir başlık aç.");
                    // 🔥 GERÇEKÇİ GÖRSEL AYARI 🔥
                    promptBuilder.AppendLine("Bu başlığın yanına, bu kişinin hedefine ulaştığındaki halini tarif eden İNGİLİZCE bir cümle yaz.");
                    promptBuilder.AppendLine("Lütfen 'photorealistic, real photo, 4k, highly detailed, gym environment' kelimelerini MUTLAKA kullan.");
                    promptBuilder.AppendLine("Asla çizim veya karikatür (illustration, cartoon) olmasın.");
                    promptBuilder.AppendLine("Örnek format: ###RESIM_KODU: realistic photo of a fit man in gym, 4k, highly detailed");
                }

                var modelId = "gemini-2.5-flash";
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelId}:generateContent?key={_apiKey}";

                var requestBody = new
                {
                    contents = new[] { new { parts = new[] { new { text = promptBuilder.ToString() } } } }
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, jsonContent);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using (JsonDocument doc = JsonDocument.Parse(responseString))
                    {
                        if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                        {
                            var textPart = candidates[0].GetProperty("content").GetProperty("parts")[0];
                            string fullText = textPart.GetProperty("text").GetString();

                            // 2. Cevabı Parçalama
                            if (fullText.Contains("###RESIM_KODU:"))
                            {
                                var parts = fullText.Split(new string[] { "###RESIM_KODU:" }, StringSplitOptions.RemoveEmptyEntries);
                                ViewBag.Cevap = parts[0].Trim();

                                if (parts.Length > 1)
                                {
                                    // URL İçin Temizleme (Boşlukları %20 yapma vb.)
                                    string rawPrompt = parts[1].Trim();
                                    ViewBag.ResimUrl = $"https://image.pollinations.ai/prompt/{HttpUtility.UrlEncode(rawPrompt)}?width=1024&height=1024&nologo=true&seed={new Random().Next(1, 9999)}";
                                }
                            }
                            else
                            {
                                ViewBag.Cevap = fullText;
                            }
                        }
                        else
                        {
                            ViewBag.Cevap = "Yapay zeka yanıt oluşturamadı.";
                        }
                    }
                }
                else
                {
                    ViewBag.Cevap = "Bağlantı hatası oluştu.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Cevap = $"Hata: {ex.Message}";
            }

            // Form verilerini koru
            ViewBag.EskiYas = yas; ViewBag.EskiBoy = boy; ViewBag.EskiKilo = kilo; ViewBag.GorselIstendi = gorselIste;

            return View("Index");
        }
    }
}