document.addEventListener("DOMContentLoaded", function () {

    /* =====================================================
       GALLERY ELEMENTS
    ====================================================== */

    const galleryItems = Array.from(
        document.querySelectorAll(".gallery-item")
    );

    const filterButtons = document.querySelectorAll(
        ".gallery-filter"
    );

    const lightbox = document.getElementById(
        "galleryLightbox"
    );

    const lightboxImage = document.getElementById(
        "lightboxImage"
    );

    const lightboxTitle = document.getElementById(
        "lightboxTitle"
    );

    const lightboxDescription = document.getElementById(
        "lightboxDescription"
    );

    const lightboxCategory = document.getElementById(
        "lightboxCategory"
    );

    const lightboxCurrent = document.getElementById(
        "lightboxCurrent"
    );

    const lightboxTotal = document.getElementById(
        "lightboxTotal"
    );

    const previousButton = document.getElementById(
        "lightboxPrev"
    );

    const nextButton = document.getElementById(
        "lightboxNext"
    );

    const closeButtons = document.querySelectorAll(
        "[data-gallery-close]"
    );


    let visibleItems = [...galleryItems];

    let currentIndex = 0;


    /* =====================================================
       FILTER GALLERY
    ====================================================== */

    filterButtons.forEach(function (button) {

        button.addEventListener("click", function () {

            const filter = this.dataset.filter;

            filterButtons.forEach(function (item) {
                item.classList.remove("active");
            });

            this.classList.add("active");


            galleryItems.forEach(function (item) {

                const category = item.dataset.category;

                const shouldShow =
                    filter === "all" ||
                    category === filter;


                if (shouldShow) {

                    item.classList.remove(
                        "filter-hide"
                    );

                    item.classList.remove(
                        "is-hidden"
                    );

                    item.classList.add(
                        "filter-show"
                    );

                    setTimeout(function () {

                        item.classList.remove(
                            "filter-show"
                        );

                    }, 500);

                } else {

                    item.classList.add(
                        "filter-hide"
                    );

                    setTimeout(function () {

                        if (
                            item.classList.contains(
                                "filter-hide"
                            )
                        ) {

                            item.classList.add(
                                "is-hidden"
                            );

                        }

                    }, 350);

                }

            });


            updateVisibleItems();

        });

    });


    /* =====================================================
       UPDATE VISIBLE ITEMS
    ====================================================== */

    function updateVisibleItems() {

        visibleItems = galleryItems.filter(function (item) {

            return !item.classList.contains(
                "is-hidden"
            );

        });

    }


    /* =====================================================
       OPEN LIGHTBOX
    ====================================================== */

    galleryItems.forEach(function (item) {

        const button = item.querySelector(
            "[data-gallery-open]"
        );

        if (!button) {
            return;
        }


        button.addEventListener(
            "click",
            function () {

                updateVisibleItems();

                currentIndex =
                    visibleItems.indexOf(item);

                if (currentIndex === -1) {
                    currentIndex = 0;
                }

                openLightbox();

            }
        );

    });


    /* =====================================================
       OPEN
    ====================================================== */

    function openLightbox() {

        if (
            !visibleItems.length ||
            !lightbox
        ) {
            return;
        }

        showLightboxImage(
            currentIndex
        );

        lightbox.classList.add(
            "active"
        );

        lightbox.setAttribute(
            "aria-hidden",
            "false"
        );

        document.body.style.overflow = "hidden";

    }


    /* =====================================================
       CLOSE
    ====================================================== */

    function closeLightbox() {

        if (!lightbox) {
            return;
        }

        lightbox.classList.remove(
            "active"
        );

        lightbox.setAttribute(
            "aria-hidden",
            "true"
        );

        document.body.style.overflow = "";

    }


    closeButtons.forEach(function (button) {

        button.addEventListener(
            "click",
            closeLightbox
        );

    });


    /* =====================================================
       SHOW IMAGE
    ====================================================== */

    function showLightboxImage(index) {

        if (!visibleItems.length) {
            return;
        }

        const item = visibleItems[index];

        const image =
            item.querySelector("img");

        if (!image) {
            return;
        }


        const title =
            item.dataset.title || "School Gallery";

        const description =
            item.dataset.description || "";

        const category =
            item.dataset.category || "Gallery";


        lightboxImage.src =
            image.currentSrc ||
            image.src;

        lightboxImage.alt =
            image.alt || title;

        lightboxTitle.textContent =
            title;

        lightboxDescription.textContent =
            description;

        lightboxCategory.textContent =
            formatCategory(category);


        lightboxCurrent.textContent =
            String(index + 1).padStart(2, "0");

        lightboxTotal.textContent =
            String(visibleItems.length)
                .padStart(2, "0");

    }


    /* =====================================================
       CATEGORY FORMAT
    ====================================================== */

    function formatCategory(category) {

        if (!category) {
            return "Gallery";
        }

        return category.charAt(0).toUpperCase() +
            category.slice(1);

    }


    /* =====================================================
       NEXT IMAGE
    ====================================================== */

    function showNext() {

        if (!visibleItems.length) {
            return;
        }

        currentIndex =
            (currentIndex + 1) %
            visibleItems.length;

        showLightboxImage(
            currentIndex
        );

    }


    /* =====================================================
       PREVIOUS IMAGE
    ====================================================== */

    function showPrevious() {

        if (!visibleItems.length) {
            return;
        }

        currentIndex =
            (currentIndex - 1 +
                visibleItems.length) %
            visibleItems.length;

        showLightboxImage(
            currentIndex
        );

    }


    if (nextButton) {

        nextButton.addEventListener(
            "click",
            showNext
        );

    }


    if (previousButton) {

        previousButton.addEventListener(
            "click",
            showPrevious
        );

    }


    /* =====================================================
       KEYBOARD NAVIGATION
    ====================================================== */

    document.addEventListener(
        "keydown",
        function (event) {

            if (
                !lightbox ||
                !lightbox.classList.contains(
                    "active"
                )
            ) {
                return;
            }


            if (event.key === "Escape") {

                closeLightbox();

            }


            if (event.key === "ArrowRight") {

                showNext();

            }


            if (event.key === "ArrowLeft") {

                showPrevious();

            }

        }
    );


    /* =====================================================
       TOUCH / SWIPE SUPPORT
    ====================================================== */

    let touchStartX = 0;

    let touchEndX = 0;


    if (lightbox) {

        lightbox.addEventListener(
            "touchstart",
            function (event) {

                touchStartX =
                    event.changedTouches[0].screenX;

            },
            {
                passive: true
            }
        );


        lightbox.addEventListener(
            "touchend",
            function (event) {

                touchEndX =
                    event.changedTouches[0].screenX;

                handleSwipe();

            },
            {
                passive: true
            }
        );

    }


    function handleSwipe() {

        const distance =
            touchEndX - touchStartX;


        if (Math.abs(distance) < 50) {
            return;
        }


        if (distance < 0) {

            showNext();

        } else {

            showPrevious();

        }

    }


    /* =====================================================
       SCROLL REVEAL
    ====================================================== */

    const revealElements =
        document.querySelectorAll(
            ".gallery-item, .gallery-video-content, .gallery-video, .gallery-note-inner, .gallery-cta-inner"
        );


    revealElements.forEach(function (element) {

        element.classList.add(
            "gallery-reveal"
        );

    });


    if (
        "IntersectionObserver"
        in window
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
                    threshold: 0.12
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
       IMAGE LOAD
    ====================================================== */

    galleryItems.forEach(function (item) {

        const image =
            item.querySelector("img");

        if (!image) {
            return;
        }


        image.addEventListener(
            "load",
            function () {

                item.classList.add(
                    "image-loaded"
                );

            }
        );

    });


});