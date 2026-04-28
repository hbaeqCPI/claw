import "./isInViewport";

//Pin tabs on top of page when scrolling
(function ($) {
    if (typeof $.fn.stickyTabs === "undefined") {
        $.fn.stickyTabs = function () {
            let tabs = $(this);

            if ($(tabs).length === 0) {
                console.error("stickyTabs error: Tab element not found.");
                return this;
            }

            if ($(tabs).hasClass("scrolling")) {
                tabs = $(tabs).closest(".nav-tabs-scroll");
            }
            $(`<div class="nav-tabs-pin"></div>`).insertBefore(tabs);
            $(tabs).addClass("nav-tabs-sticky");

            const tabContent = $(tabs).next(".tab-content");

            function checkPin() {
                if ($(tabs).prev(".nav-tabs-pin").isInViewport(true)) {
                    $(tabs).removeClass("pinned");
                }
                else {
                    if (tabContent.length == 0 || tabContent.isInViewport(true))
                        $(tabs).addClass("pinned");
                }
            }
            function showTop() {
                if ($(tabs).hasClass("pinned")) {
                    $("body, html").animate({ scrollTop: $(tabs).prev(".nav-tabs-pin").offset().top }, 300);
                }
            }

            $(window).off("scroll", checkPin);
            $(window).on("scroll", checkPin);

            $(tabs).find(".nav-link").each(function () {
                $(this).off("click", showTop);
                $(this).on("click", showTop);
            });

            checkPin();
            showTop();

            return this;
        };
    }
    
}(jQuery));