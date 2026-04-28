
//Test if element is visible
(function ($) {
    if (typeof $.fn.isInViewport === "undefined") {
        $.fn.isInViewport = function (partialInView) {
            const el = $(this);

            if ($(el).length === 0) {
                return false;
            }

            const elementTop = $(el).offset().top;
            const elementLeft = $(el).offset().left;
            const elementBottom = elementTop + $(el).height();
            const elementRight = elementLeft + $(el).width();

            const viewportTop = $(window).scrollTop();
            const viewportLeft = $(window).scrollLeft();
            const viewportBottom = viewportTop + $(window).height();
            const viewportRight = viewportLeft + $(window).width();

            partialInView = partialInView || false;

            if (partialInView)
                return elementTop <= viewportBottom && elementBottom >= viewportTop;
            else
                return elementTop >= viewportTop && elementLeft >= viewportLeft && elementBottom <= viewportBottom && elementRight <= viewportRight;
        };
    }
}(jQuery));