using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Web;
using SporSalonuYonetim.Data;
using SporSalonuYonetim.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace SporSalonuYonetim.Controllers
{
    [Authorize]
    public class YapayZekaController : Controller
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context; // Veritabanı bağlantısı eklendi

        public YapayZekaController(IConfiguration configuration, ApplicationDbContext context)
        {
            _apiKey = configuration["GeminiApiKey"];
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Sayfa açılınca geçmiş kayıtları getir
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gecmis = await _context.AiAnalizGecmisleri
                .Where(x => x.UyeId == userId)
                .OrderByDescending(x => x.Tarih)
                .ToListAsync();

            ViewBag.GecmisListesi = gecmis;
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
                    promptBuilder.AppendLine("Bu başlığın yanına, bu kişinin hedefine ulaştığındaki halini tarif eden İNGİLİZCE bir cümle yaz.");
                    promptBuilder.AppendLine("Lütfen 'photorealistic, real photo, 4k, highly detailed, gym environment' kelimelerini MUTLAKA kullan.");
                    promptBuilder.AppendLine("Asla çizim veya karikatür olmasın.");
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

                string finalCevap = "Yanıt alınamadı.";
                string finalResimUrl = null;

                if (response.IsSuccessStatusCode)
                {
                    using (JsonDocument doc = JsonDocument.Parse(responseString))
                    {
                        if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                        {
                            var textPart = candidates[0].GetProperty("content").GetProperty("parts")[0];
                            string fullText = textPart.GetProperty("text").GetString();

                            if (fullText.Contains("###RESIM_KODU:"))
                            {
                                var parts = fullText.Split(new string[] { "###RESIM_KODU:" }, StringSplitOptions.RemoveEmptyEntries);
                                finalCevap = parts[0].Trim();

                                if (parts.Length > 1)
                                {
                                    string rawPrompt = parts[1].Trim();
                                    finalResimUrl = $"https://image.pollinations.ai/prompt/{HttpUtility.UrlEncode(rawPrompt)}?width=1024&height=1024&nologo=true&seed={new Random().Next(1, 9999)}";
                                }
                            }
                            else
                            {
                                finalCevap = fullText;
                            }

                            // --- CRUD: VERİTABANINA KAYDET ---
                            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                            var yeniKayit = new AiAnalizGecmisi
                            {
                                UyeId = userId,
                                KullaniciSorusu = $"{yas} yaş, {boy}cm, {kilo}kg, {hedef} ({cinsiyet})",
                                AiCevabi = finalCevap,
                                ResimUrl = finalResimUrl,
                                Tarih = DateTime.Now
                            };
                            _context.AiAnalizGecmisleri.Add(yeniKayit);
                            await _context.SaveChangesAsync();
                            // ---------------------------------
                        }
                    }
                }

                ViewBag.Cevap = finalCevap;
                ViewBag.ResimUrl = finalResimUrl;
            }
            catch (Exception ex)
            {
                ViewBag.Cevap = $"Hata: {ex.Message}";
            }

            // Form verilerini koru
            ViewBag.EskiYas = yas; ViewBag.EskiBoy = boy; ViewBag.EskiKilo = kilo; ViewBag.GorselIstendi = gorselIste;

            // Geçmiş listesini tekrar yükle
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.GecmisListesi = await _context.AiAnalizGecmisleri.Where(x => x.UyeId == currentUserId).OrderByDescending(x => x.Tarih).ToListAsync();

            return View("Index");
        }

        // --- CRUD: SİLME İŞLEMİ ---
        [HttpPost]
        public async Task<IActionResult> GecmisiTemizle()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kayitlar = await _context.AiAnalizGecmisleri.Where(x => x.UyeId == userId).ToListAsync();

            _context.AiAnalizGecmisleri.RemoveRange(kayitlar);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}