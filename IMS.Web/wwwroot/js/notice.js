document.addEventListener("DOMContentLoaded", function () {

    const searchInput = document.getElementById("noticeSearch");
    const clearSearch = document.getElementById("clearSearch");

    const filterButtons =
        document.querySelectorAll(".filter-btn");

    const noticeCards =
        document.querySelectorAll(
            ".notice-card, .featured-notice"
        );

    const emptyState =
        document.getElementById("noticeEmpty");

    const resetButton =
        document.getElementById("resetNotices");


    let selectedCategory = "all";


    /* =====================================================
       FILTER NOTICES
       ===================================================== */

    function filterNotices() {

        const searchText =
            searchInput
                ? searchInput.value.trim().toLowerCase()
                : "";

        let visibleCount = 0;


        noticeCards.forEach(function (notice) {

            const category =
                notice.dataset.category || "";

            const title =
                notice.dataset.title || "";

            const description =
                notice.dataset.description || "";


            const categoryMatch =
                selectedCategory === "all" ||
                category === selectedCategory;


            const searchMatch =
                searchText === "" ||
                title.includes(searchText) ||
                description.includes(searchText);


            if (categoryMatch && searchMatch) {

                notice.style.display = "";

                visibleCount++;

            } else {

                notice.style.display = "none";

            }

        });


        if (visibleCount === 0) {

            emptyState.classList.remove("d-none");

        } else {

            emptyState.classList.add("d-none");

        }


        if (clearSearch) {

            clearSearch.style.display =
                searchText.length > 0
                    ? "block"
                    : "none";

        }
    }


    /* =====================================================
       SEARCH
       ===================================================== */

    if (searchInput) {

        searchInput.addEventListener(
            "input",
            filterNotices
        );

    }


    /* =====================================================
       CATEGORY FILTER
       ===================================================== */

    filterButtons.forEach(function (button) {

        button.addEventListener("click", function () {

            filterButtons.forEach(function (btn) {
                btn.classList.remove("active");
            });

            this.classList.add("active");

            selectedCategory =
                this.dataset.category || "all";

            filterNotices();

        });

    });


    /* =====================================================
       CLEAR SEARCH
       ===================================================== */

    if (clearSearch) {

        clearSearch.addEventListener("click", function () {

            searchInput.value = "";

            filterNotices();

            searchInput.focus();

        });

    }


    /* =====================================================
       RESET
       ===================================================== */

    if (resetButton) {

        resetButton.addEventListener("click", function () {

            selectedCategory = "all";

            searchInput.value = "";

            filterButtons.forEach(function (button) {

                button.classList.remove("active");

            });

            const allButton =
                document.querySelector(
                    '.filter-btn[data-category="all"]'
                );

            if (allButton) {
                allButton.classList.add("active");
            }

            filterNotices();

        });

    }


    /* =====================================================
       NOTICE MODAL
       ===================================================== */

    const noticeModalElement =
        document.getElementById("noticeModal");

    if (noticeModalElement) {

        const noticeModal =
            new bootstrap.Modal(noticeModalElement);

        const modalTitle =
            document.getElementById("noticeModalLabel");

        const modalDescription =
            document.getElementById("modalDescription");

        const modalCategory =
            document.getElementById("modalCategory");

        const modalDate =
            document.getElementById("modalDate");


        const openButtons =
            document.querySelectorAll(
                ".notice-open-btn, .notice-read-btn"
            );


        openButtons.forEach(function (button) {

            button.addEventListener("click", function () {

                const title =
                    this.dataset.noticeTitle || "Notice";

                const description =
                    this.dataset.noticeDescription || "";

                const category =
                    this.dataset.noticeCategory || "Notice";

                const date =
                    this.dataset.noticeDate || "";


                modalTitle.textContent = title;

                modalDescription.textContent =
                    description;

                modalCategory.textContent =
                    category;

                modalDate.textContent =
                    date;


                noticeModal.show();

            });

        });

    }


    /* =====================================================
       SIMPLE REVEAL ANIMATION
       ===================================================== */

    const animatedElements =
        document.querySelectorAll(
            ".notice-card, .featured-notice, .calendar-card"
        );


    if ("IntersectionObserver" in window) {

        const observer =
            new IntersectionObserver(
                function (entries, observer) {

                    entries.forEach(function (entry) {

                        if (entry.isIntersecting) {

                            entry.target.classList.add(
                                "notice-visible"
                            );

                            observer.unobserve(
                                entry.target
                            );

                        }

                    });

                },
                {
                    threshold: 0.08
                }
            );


        animatedElements.forEach(function (element) {

            element.classList.add(
                "notice-reveal"
            );

            observer.observe(element);

        });

    }

});