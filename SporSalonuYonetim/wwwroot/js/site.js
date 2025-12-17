$(document).ready(function () {
    // Sadece Profil (Manage) sayfasındaysak çalışır
    if (window.location.href.indexOf("Account/Manage") > -1) {

        // --- 1. GÖRÜNÜM DÜZELTMELERİ ---

        // "Profile" başlığını bul ve ortala
        $("h3, h2").addClass("text-center w-100 fw-bold text-primary");

        // Menü sekmelerini (soldaki linkleri) gizle ki sadece form kalsın (İsteğe bağlı, daha temiz durur)
        $(".nav-pills").parent().hide();

        // Formu içeren kutuyu bul, ortala ve gölgeli kutu yap
        var mainColumn = $("main .row .col-md-6").first();
        if (mainColumn.length > 0) {
            // col-md-6 sınıfını kaldırıp col-md-8 yaparak biraz genişletelim
            mainColumn.removeClass("col-md-6").addClass("col-md-8");

            // Satırı ortala
            mainColumn.parent().addClass("justify-content-center");

            // Beyaz kart görünümü ver
            mainColumn.addClass("shadow-lg p-5 rounded bg-white mt-3 border");
        }

        // --- 2. TÜRK TELEFON NUMARASI FORMATI (Maskeleme) ---

        var phoneInput = $("input[type='tel'], input[name='Input.PhoneNumber']");

        // Kullanıcıya ipucu ver
        phoneInput.attr("placeholder", "0555 123 45 67");
        phoneInput.attr("maxlength", "15"); // Fazla karakteri engelle

        phoneInput.on("input", function (e) {
            // 1. Sadece rakamları al
            var input = e.target.value.replace(/\D/g, '');

            // 2. Başında 0 yoksa ekle (İsteğe bağlı, Türk numaraları 0 ile başlar)
            if (input.length > 0 && input[0] !== '0') {
                // input = '0' + input; // İstersen bunu açabilirsin
            }

            // 3. Formatı uygula: 05XX XXX XX XX
            var formatted = input;
            if (input.length > 4) {
                formatted = input.substring(0, 4) + ' ' + input.substring(4);
            }
            if (input.length > 7) {
                formatted = input.substring(0, 4) + ' ' + input.substring(4, 7) + ' ' + input.substring(7);
            }
            if (input.length > 9) {
                formatted = input.substring(0, 4) + ' ' + input.substring(4, 7) + ' ' + input.substring(7, 9) + ' ' + input.substring(9);
            }

            // 4. Değeri kutuya geri yaz
            e.target.value = formatted;
        });
    }
});