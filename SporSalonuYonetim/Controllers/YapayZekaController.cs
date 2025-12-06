using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json; // JSON işlemleri için gerekli

namespace SporSalonuYonetim.Controllers
{
    public class YapayZekaController : Controller
    {
        // GET: YapayZeka Sayfasını Aç
        public IActionResult Index()
        {
            return View();
        }

        // POST: Formdan gelen verileri işle
        [HttpPost]
        public async Task<IActionResult> OneriAl(int yas, int kilo, int boy, string hedef, string cinsiyet)
        {
            // 1. Kullanıcıdan gelen verileri birleştirelim
            string prompt = $"Ben {yas} yaşında, {kilo} kg ağırlığında, {boy} cm boyunda bir {cinsiyet} bireyim. " +
                            $"Hedefim: {hedef}. " +
                            $"Bana maddeler halinde kısa ve öz bir egzersiz ve beslenme programı önerir misin?";

            string yapayZekaCevabi = "";

            // --- API ANAHTARI KISMI ---
            string apiKey = "BURAYA_OPENAI_API_KEY_GELECEK";
            // Eğer anahtarın yoksa veya boş bırakırsan aşağıdaki "else" kısmı çalışır ve sahte cevap döner.

            if (apiKey != "BURAYA_OPENAI_API_KEY_GELECEK" && !string.IsNullOrEmpty(apiKey))
            {
                // GERÇEK API BAĞLANTISI (OpenAI)
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                        var requestData = new
                        {
                            model = "gpt-3.5-turbo",
                            messages = new[]
                            {
                                new { role = "system", content = "Sen profesyonel bir spor hocası ve diyetisyensin." },
                                new { role = "user", content = prompt }
                            },
                            max_tokens = 500
                        };

                        var jsonContent = JsonSerializer.Serialize(requestData);
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                        var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);

                        if (response.IsSuccessStatusCode)
                        {
                            var responseString = await response.Content.ReadAsStringAsync();
                            var jsonDoc = JsonDocument.Parse(responseString);
                            yapayZekaCevabi = jsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                        }
                        else
                        {
                            yapayZekaCevabi = "Hata: API bağlantısı kurulamadı. Lütfen daha sonra tekrar deneyin.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    yapayZekaCevabi = "Hata oluştu: " + ex.Message;
                }
            }
            else
            {
                // SİMÜLASYON MODU (API Key yoksa çalışır - Sunumda kurtarıcıdır)
                yapayZekaCevabi = $"(Simülasyon Modu) Merhaba! {yas} yaşında ve {hedef} hedefi olan biri için önerilerim:\n\n" +
                                  "**Egzersiz Programı:**\n" +
                                  "- Haftada 3 gün tüm vücut ağırlık çalışması.\n" +
                                  "- Haftada 2 gün 30dk tempolu yürüyüş.\n\n" +
                                  "**Beslenme Önerisi:**\n" +
                                  "- Günde en az 2.5 litre su iç.\n" +
                                  "- Şeker ve paketli gıdalardan uzak dur.\n" +
                                  "- Protein ağırlıklı beslenmeye özen göster.";
            }

            // Cevabı View'a gönder
            ViewBag.Cevap = yapayZekaCevabi;
            ViewBag.Prompt = prompt; // Kullanıcının ne sorduğunu da geri gönderelim
            return View("Index");
        }
    }
}