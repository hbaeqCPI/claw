//collapsible container for long text.
//resizing the browser window will reset to minimized container.
//
//customize toggle button using data attributes:
//  data-icon-max
//  data-icon-min
//  data-label-max
//  data-label-min
//
//css is included in site.css
(function ($) {
    if (typeof $.fn.textOverflow === "undefined") {
        $.fn.textOverflow = function () {
            const el = $(this);

            let iconMax = el.data("icon-max");
            let iconMin = el.data("icon-min");
            let labelMax = el.data("label-max");
            let labelMin = el.data("label-min");

            if (iconMax == undefined && labelMax == undefined)
                iconMax = "fas fa-caret-down";

            if (iconMin == undefined && labelMin == undefined)
                iconMin = "fas fa-caret-up";

            iconMax = iconMax ? iconMax : "";
            iconMin = iconMin ? iconMin : "";
            labelMax = labelMax ? labelMax : "";
            labelMin = labelMin ? labelMin : "";

            el.addClass("content");
            el.wrap("<div class='text-overflow'></div>");

            const textOverflow = el.closest(".text-overflow");
            textOverflow.append(`<div class='toggle max'><i class="${iconMax}"></i>${labelMax}</div>`);
            textOverflow.append(`<div class='toggle min'><i class="${iconMin}"></i>${labelMin}</div>`);

            const initialize = function () {
                textOverflow.addClass("hidden");
                textOverflow.removeClass("maximized");
            };

            const checkOverflow = function () {
                if (textOverflow.get(0).offsetHeight < textOverflow.get(0).scrollHeight ||
                    textOverflow.get(0).offsetWidth < textOverflow.get(0).scrollWidth) {
                    textOverflow.addClass("hidden");
                } else {
                    textOverflow.removeClass("hidden");
                }
            };

            initialize();
            checkOverflow();

            let resizeTimer;
            $(window).on('resize', function (e) {
                //reset to minimized when resizing window
                initialize();

                clearTimeout(resizeTimer);
                resizeTimer = setTimeout(checkOverflow, 250);
            });

            el.on("click", function () {
                textOverflow.toggleClass("maximized");
            });

            textOverflow.find(".toggle").each(function () {
                $(this).on("click", function () {
                    textOverflow.toggleClass("maximized");
                });
            });

            return this;
        };
    }
})(jQuery);
