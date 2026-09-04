(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        var toggle = document.getElementById("navToggle");
        var nav = document.getElementById("mainNav");

        if (toggle && nav) {
            toggle.addEventListener("click", function () {
                var isOpen = nav.classList.toggle("open");
                toggle.setAttribute("aria-expanded", isOpen ? "true" : "false");
            });
        }

        // Contact form — front-end only for now; wire to a real
        // /Home/ContactSubmit action once that endpoint exists.
        var contactForm = document.getElementById("contactForm");
        if (contactForm) {
            contactForm.addEventListener("submit", function (e) {
                e.preventDefault();
                var status = document.getElementById("contactFormStatus");
                if (status) {
                    status.textContent = "Thanks — your message has been noted. We'll get back to you soon.";
                    status.classList.remove("d-none");
                }
                contactForm.reset();
            });
        }
    });
})();
