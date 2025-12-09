using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace SporSalonuYonetim.Controllers
{
    [Authorize] // Sadece "Giriş yapmış" herhangi biri girebilir
    public class YapayZekaController : Controller
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public YapayZekaController(IConfiguration configuration)
        {
            _apiKey = configuration["GeminiApiKey"];
            _httpClient = new HttpClient();
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
                    // BURASI ÇOK ÖNEMLİ: Gemini'ye "Bize resim için kod ver" diyoruz.
                    promptBuilder.AppendLine("\n--- ÖZEL İSTEK ---");
                    promptBuilder.AppendLine("Cevabının EN SONUNA, '###RESIM_KODU:' diye bir başlık aç.");
                    promptBuilder.AppendLine("Bu başlığın yanına, bu kişinin hedefine ulaştığındaki halini tarif eden KISA ve İNGİLİZCE bir cümle yaz.");
                    promptBuilder.AppendLine("Örnek format: ###RESIM_KODU: muscular man in gym, fitness model body, cinematic lighting, 8k");
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
                        string fullText = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

                        // 2. Cevabı Parçalama (Metin ve Resim Kodu)
                        if (fullText.Contains("###RESIM_KODU:"))
                        {
                            var parts = fullText.Split(new string[] { "###RESIM_KODU:" }, StringSplitOptions.RemoveEmptyEntries);
                            ViewBag.Cevap = parts[0].Trim(); // Antrenman Programı

                            // Resim Promptunu alıyoruz (İngilizce cümle)
                            if (parts.Length > 1)
                            {
                                ViewBag.ResimPrompt = parts[1].Trim();
                            }
                        }
                        else
                        {
                            ViewBag.Cevap = fullText;
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