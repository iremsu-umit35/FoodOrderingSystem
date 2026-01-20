document.querySelectorAll(".nav-dropdown").forEach(drop => {
    drop.addEventListener("click", (e) => {

        // Menü tıklamasının form, radio button vb. elementlere yayılmasını engeller
        e.stopPropagation();

        let menu = drop.querySelector(".dropdown-menu");
        if (menu) {
            menu.style.display = (menu.style.display === "block" ? "none" : "block");
        }
    });
});

// Sayfanın herhangi bir yerine tıklayınca menüyü kapat
document.addEventListener("click", () => {
    document.querySelectorAll(".dropdown-menu").forEach(menu => {
        menu.style.display = "none";
    });
});
