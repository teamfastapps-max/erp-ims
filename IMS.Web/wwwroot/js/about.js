document.addEventListener("DOMContentLoaded", function () {

    /* ==========================================
       SCROLL REVEAL
    ========================================== */

    const revealElements =
        document.querySelectorAll(".about-reveal");

    const revealObserver = new IntersectionObserver(
        function (entries, observer) {

            entries.forEach(function (entry) {

                if (entry.isIntersecting) {

                    entry.target.classList.add("is-visible");

                    observer.unobserve(entry.target);
                }

            });

        },
        {
            threshold: 0.12
        }
    );

    revealElements.forEach(function (element) {
        revealObserver.observe(element);
    });


    /* ==========================================
       NUMBER COUNTER
    ========================================== */

    const counters =
        document.querySelectorAll(".about-counter");

    const counterObserver = new IntersectionObserver(
        function (entries, observer) {

            entries.forEach(function (entry) {

                if (!entry.isIntersecting) {
                    return;
                }

                const counter = entry.target;
                const target =
                    parseInt(counter.dataset.target);

                let current = 0;

                const duration = 1400;
                const stepTime = 20;
                const increment =
                    target / (duration / stepTime);

                const timer = setInterval(function () {

                    current += increment;

                    if (current >= target) {

                        counter.textContent = target;

                        clearInterval(timer);

                    } else {

                        counter.textContent =
                            Math.floor(current);
                    }

                }, stepTime);

                observer.unobserve(counter);
            });

        },
        {
            threshold: 0.5
        }
    );

    counters.forEach(function (counter) {
        counterObserver.observe(counter);
    });

});