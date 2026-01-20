// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Navbar dil seçimi
document.addEventListener("DOMContentLoaded", () => {
    const langSelector = document.querySelector(".lang-selector");
    langSelector.addEventListener("click", () => {
        alert("Dil seçimi açılacak (TR/EN)!");
    });
});

// Scroll efekti
window.addEventListener("scroll", () => {
    const nav = document.querySelector("nav");
    if (window.scrollY > 50) {
        nav.style.boxShadow = "0 4px 10px rgba(0,0,0,0.1)";
    } else {
        nav.style.boxShadow = "0 2px 5px rgba(0,0,0,0.05)";
    }
});

/*
////register için
document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('registerForm');

    if (form) {
        form.addEventListener('submit', function (event) {
            // Mevcut tüm hata mesajlarını temizle
            document.querySelectorAll('.error-message').forEach(el => el.remove());

            let isValid = true;

            // 1. İsim Doğrulama
            const fullNameInput = document.querySelector('input[name="FullName"]');
            if (fullNameInput.value.trim().length < 2) {
                displayError(fullNameInput, 'Lütfen geçerli bir isim giriniz (en az 2 karakter).');
                isValid = false;
            }

            // 2. Email Doğrulama (Basit Kontrol)
            const emailInput = document.querySelector('input[name="Email"]');
            if (!/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/.test(emailInput.value)) {
                displayError(emailInput, 'Lütfen geçerli bir email adresi giriniz.');
                isValid = false;
            }

            // 3. Şifre Doğrulama (Minimum uzunluk)
            const passwordInput = document.querySelector('input[name="Password"]');
            if (passwordInput.value.length < 6) {
                displayError(passwordInput, 'Şifre en az 6 karakter uzunluğunda olmalıdır.');
                isValid = false;
            }

            // 4. Telefon Doğrulama (Basit Kontrol)
            const phoneInput = document.querySelector('input[name="Phone"]');
            if (!/^\d{10,11}$/.test(phoneInput.value.replace(/\s/g, ''))) {
                displayError(phoneInput, 'Lütfen 10 veya 11 haneli geçerli bir telefon numarası giriniz.');
                isValid = false;
            }

            // 5. Hesap Türü Doğrulama
            const roleSelect = document.querySelector('select[name="Role"]');
            if (roleSelect.value === "") {
                displayError(roleSelect, 'Lütfen bir hesap türü seçiniz.');
                isValid = false;
            }

            if (!isValid) {
                event.preventDefault(); // Formun sunucuya gönderilmesini engelle
                // İlk hata alanına odaklan
                document.querySelector('.error-message').closest('div').querySelector('input, select').focus();
            }
        });
    }

    /**
     * Hata mesajını ilgili inputun altına ekler.
     * @param {HTMLElement} inputElement - Hatanın gösterileceği giriş alanı veya select.
     * @param {string} message - Gösterilecek hata metni.
     /*
    function displayError(inputElement, message) {
        const errorDiv = document.createElement('div');
        errorDiv.className = 'error-message';
        errorDiv.textContent = message;
        errorDiv.style.display = 'block'; // Mesajı görünür yap

        // Input'u içeren div'i bul
        const parentDiv = inputElement.closest('div');
        if (parentDiv) {
            parentDiv.appendChild(errorDiv);
        }
    }
});*/