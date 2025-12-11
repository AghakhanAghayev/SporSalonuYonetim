// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace SporSalonuYonetim.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public LoginModel(SignInManager<IdentityUser> signInManager,
            ILogger<LoginModel> logger,
            UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        // HATA AYIKLAMA İÇİN YENİ ALAN
        [TempData]
        public string DebugMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Beni Hatırla?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // 1. KULLANICIYI BULMA DENEMESİ
                var user = await _userManager.FindByEmailAsync(Input.Email);

                if (user == null)
                {
                    // HATA YAKALANDI: Kullanıcı Yok
                    DebugMessage = $"HATA: '{Input.Email}' adresine sahip bir kullanıcı veritabanında bulunamadı.";
                    return Page();
                }

                // 2. GİRİŞ YAPMA DENEMESİ
                // user.UserName (Ad Soyad) ve Şifre ile deniyoruz
                var result = await _signInManager.PasswordSignInAsync(user.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Giriş başarılı.");
                    return LocalRedirect(returnUrl);
                }

                if (result.IsLockedOut)
                {
                    DebugMessage = "HATA: Hesap kilitlenmiş.";
                    return RedirectToPage("./Lockout");
                }
                if (result.IsNotAllowed)
                {
                    DebugMessage = "HATA: Giriş izni yok (Email onayı gerekebilir).";
                    return Page();
                }

                // Şifre Yanlışsa
                DebugMessage = $"HATA: Kullanıcı bulundu ({user.UserName}) ama şifre yanlış.";
                return Page();
            }

            DebugMessage = "HATA: Form bilgileri geçersiz (Boş alan olabilir).";
            return Page();
        }
    }
}