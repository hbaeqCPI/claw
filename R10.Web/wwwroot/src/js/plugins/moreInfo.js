(function ($) {
    if (typeof $.fn.moreInfo === "undefined") {
        $.fn.moreInfo = function () {
            $(this).find(".more-info").each(function () {
                const container = $(this);
                const id = this.id;
                const labelMore = $(container).data("label-more") === undefined ? "" : $(container).data("label-more");
                const labelLess = $(container).data("label-less") === undefined ? "" : $(container).data("label-less");

                container.addClass("collapse");
                container.nextAll().appendTo(container);

                $(`<div class='more-info-toggle'><a href='#${id}' data-toggle='collapse'>${labelMore}</a></div>`).insertBefore(container);

                container.on('hidden.bs.collapse', function () {
                    $(`.more-info-toggle a[href='#${id}']`).text(labelMore).addClass("more").removeClass("less");
                });

                container.on('shown.bs.collapse', function () {
                    $(`.more-info-toggle a[href='#${id}']`).text(labelLess).addClass("less").removeClass("more");
                });
            });
            return this;
        };
    }
}(jQuery));

