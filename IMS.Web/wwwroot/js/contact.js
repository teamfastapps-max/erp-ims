document.addEventListener("DOMContentLoaded", function () {

    const contactForm = document.getElementById("contactForm");
    const message = document.getElementById("message");
    const messageCount = document.getElementById("messageCount");
    const phone = document.getElementById("phone");

    /* =====================================================
       MESSAGE CHARACTER COUNTER
       ===================================================== */

    if (message && messageCount) {

        function updateMessageCount() {
            messageCount.textContent = message.value.length;
        }

        message.addEventListener("input", updateMessageCount);

        updateMessageCount();
    }


    /* =====================================================
       PHONE NUMBER
       ===================================================== */

    if (phone) {

        phone.addEventListener("input", function () {

            let value = this.value;

            // Allow numbers, spaces, +, -, and brackets
            value = value.replace(/[^\d+\-\s()]/g, "");

            this.value = value;
        });
    }


    /* =====================================================
       BOOTSTRAP VALIDATION
       ===================================================== */

    if (contactForm) {

        contactForm.addEventListener("submit", function (event) {

            if (!contactForm.checkValidity()) {

                event.preventDefault();
                event.stopPropagation();

                contactForm.classList.add("was-validated");

                const firstInvalid =
                    contactForm.querySelector(":invalid");

                if (firstInvalid) {
                    firstInvalid.focus();
                }

                return;
            }

            contactForm.classList.add("was-validated");

            /*
             * Keep normal form submission enabled.
             *
             * Your ASP.NET Core HomeController will handle
             * the submitted form.
             */
        });
    }


    /* =====================================================
       SCROLL REVEAL
       ===================================================== */

    const revealElements =
        document.querySelectorAll(
            ".contact-detail, .contact-form-card, .hours-card, .map-container, .contact-cta-inner"
        );

    if ("IntersectionObserver" in window) {

        const observer = new IntersectionObserver(
            function (entries, observer) {

                entries.forEach(function (entry) {

                    if (entry.isIntersecting) {

                        entry.target.classList.add("contact-visible");

                        observer.unobserve(entry.target);
                    }
                });

            },
            {
                threshold: 0.12
            }
        );

        revealElements.forEach(function (element) {

            element.classList.add("contact-reveal");

            observer.observe(element);
        });
    }

});
