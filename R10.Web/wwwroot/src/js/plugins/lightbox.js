//modal image viewer
(function ($) {
    if (typeof $.fn.lightBox === "undefined") {
        $.fn.lightBox = function () {
            const el = $(this);

            if ($(el).length === 0) {
                console.error("lightbox error: Image element not found.");
                return this;
            }

            $(el).off("click", openLightBox);
            $(el).on("click", openLightBox);
            if ($("#lightBox").length === 0) {
                $("body").append(`
                            <div id="lightBox">
                                <span class="close cursor">&times;</span>
                                <div class="content">
                                    <div class="slide">
                                        <img />
                                        <div class="title"></div>
                                    </div>
                                </div>
                            </div>`);
                $("#lightBox").find(".close").on("click", closeLightBox);
                $("#lightBox").find("img").on("click", toggleZoom);
                $(document).on("keydown", closeLightBox);
            }

            function openLightBox(e) {
                var image = $("#lightBox").find("img");
                var slide = image.parent();
                var title = slide.find(".title");
                var src = $(e.target).data("img-src") ? $(e.target).data("img-src") : $(e.target).attr("src");

                slide.addClass("fit-to-screen");
                image.attr("src", src);
                title.html($(e.target).attr("title"));

                $("#lightBox").show();
            }

            function closeLightBox(e) {
                if (e === undefined || e.key == undefined || e.key === "Escape")
                    $("#lightBox").hide();
            }

            function toggleZoom(e) {
                var slide = $(e.target).parent();
                slide.toggleClass("fit-to-screen");
            }
        };
    }
}(jQuery));

