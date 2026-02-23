import "./isInViewport";

//Make horizontal tabs scrollable
(function ($) {
    if (typeof $.fn.scrollingTabs === "undefined") {
        $.fn.scrollingTabs = function () {
            const el = $(this);

            if ($(el).length === 0) {
                console.error("scrollingTabs error: Tabs element not found.");
                return this;
            }

            const init = $(el).closest(".nav-tabs-scroll").length === 0;
            if (init) {
                $(`<div class="nav-tabs-scroll"><span class="far fa-arrow-left left" style="display: none;"></span>
            <span class="far fa-arrow-right right" style="display: none;"></span>`).insertBefore(el);

                const scroll = $(el).prev(".nav-tabs-scroll");
                $(el).appendTo($(scroll));
            }

            const scrollRight = $(el).closest(".nav-tabs-scroll").find(".right");
            const scrollLeft = $(scrollRight).closest(".nav-tabs-scroll").find(".left");

            $(el).addClass("scrolling");

            if (init) {
                checkScroll();
            }

            $(el).off("scroll", checkScroll);
            $(el).on("scroll", checkScroll);

            $(window).off("resize", onWindowResize);
            $(window).on("resize", onWindowResize);

            bindButtons();

            function onWindowResize() {
                checkScroll();
                bindButtons();
            }

            function bindButtons() {
                const step = $(window).width() - 100;

                $(scrollLeft).off("click");
                $(scrollLeft).on("click", function () {
                    event.preventDefault();
                    $(el).animate({
                        scrollLeft: `-=${step}px`
                    });
                });
                $(scrollRight).off("click");
                $(scrollRight).on("click", function () {
                    event.preventDefault();
                    $(el).animate({
                        scrollLeft: `+=${step}px`
                    });
                });
            }

            function checkScroll() {
                //use partialInView (isInViewport(true))
                //tabs container right is sometimes (in Google Chrome) 
                //greater than viewport right by a fraction of a pixel
                if ($(el).isInViewport(true)) {
                    if ($(el).find("li:first-child").isInViewport()) {
                        $(scrollLeft).fadeOut("slow");
                    }
                    else {
                        $(scrollLeft).fadeIn("slow");
                    }

                    if ($(el).find("li:last-child").isInViewport()) {
                        $(scrollRight).fadeOut("slow");
                    }
                    else {
                        $(scrollRight).fadeIn("slow");
                    }
                }
            }

            return this;
        };
    }

}
)(jQuery);

