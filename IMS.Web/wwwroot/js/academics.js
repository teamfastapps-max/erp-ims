document.addEventListener("DOMContentLoaded", function () {

    /* =====================================================
       ACADEMIC STAGE TABS
       ===================================================== */

    const stageTabs =
        document.querySelectorAll(".stage-tab");

    const stagePanels =
        document.querySelectorAll(".stage-panel");


    stageTabs.forEach(function (tab) {

        tab.addEventListener("click", function () {

            const selectedStage =
                this.dataset.stage;


            /* Remove active state */

            stageTabs.forEach(function (item) {

                item.classList.remove("active");

            });


            stagePanels.forEach(function (panel) {

                panel.classList.remove("active");

            });


            /* Activate selected tab */

            this.classList.add("active");


            const selectedPanel =
                document.querySelector(
                    '.stage-panel[data-panel="' +
                    selectedStage +
                    '"]'
                );


            if (selectedPanel) {

                selectedPanel.classList.add("active");

            }

        });

    });


    /* =====================================================
       SMOOTH SCROLL
       ===================================================== */

    document.querySelectorAll(
        'a[href^="#"]'
    ).forEach(function (link) {

        link.addEventListener("click", function (event) {

            const targetId =
                this.getAttribute("href");

            if (
                !targetId ||
                targetId === "#"
            ) {
                return;
            }


            const target =
                document.querySelector(targetId);


            if (!target) {
                return;
            }


            event.preventDefault();


            target.scrollIntoView({
                behavior: "smooth",
                block: "start"
            });

        });

    });


    /* =====================================================
       SCROLL REVEAL
       ===================================================== */

    const revealElements =
        document.querySelectorAll(
            ".philosophy-grid, " +
            ".stage-panel, " +
            ".learning-card, " +
            ".teaching-content, " +
            ".teaching-visual, " +
            ".assessment-item, " +
            ".beyond-card, " +
            ".timeline-item, " +
            ".academics-cta-inner"
        );


    revealElements.forEach(function (element) {

        element.classList.add(
            "academic-reveal"
        );

    });


    if (
        "IntersectionObserver" in window
    ) {

        const observer =
            new IntersectionObserver(
                function (entries, observer) {

                    entries.forEach(
                        function (entry) {

                            if (
                                entry.isIntersecting
                            ) {

                                entry.target.classList.add(
                                    "visible"
                                );

                                observer.unobserve(
                                    entry.target
                                );

                            }

                        }
                    );

                },
                {
                    threshold: 0.12,
                    rootMargin: "0px 0px -30px 0px"
                }
            );


        revealElements.forEach(
            function (element) {

                observer.observe(
                    element
                );

            }
        );

    } else {

        revealElements.forEach(
            function (element) {

                element.classList.add(
                    "visible"
                );

            }
        );

    }


    /* =====================================================
       SUBJECT CARD MICRO INTERACTION
       ===================================================== */

    const subjectCards =
        document.querySelectorAll(
            ".subject-card"
        );


    subjectCards.forEach(function (card) {

        card.addEventListener(
            "mouseenter",
            function () {

                const icon =
                    this.querySelector("i");


                if (icon) {

                    icon.style.transform =
                        "translateY(-3px)";

                }

            }
        );


        card.addEventListener(
            "mouseleave",
            function () {

                const icon =
                    this.querySelector("i");


                if (icon) {

                    icon.style.transform =
                        "";

                }

            }
        );

    });


    /* =====================================================
       REDUCED MOTION
       ===================================================== */

    const prefersReducedMotion =
        window.matchMedia(
            "(prefers-reduced-motion: reduce)"
        );


    if (prefersReducedMotion.matches) {

        document.documentElement.style
            .setProperty(
                "scroll-behavior",
                "auto"
            );

    }

});