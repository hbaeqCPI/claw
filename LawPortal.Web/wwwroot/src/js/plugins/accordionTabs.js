
//Transform bootstrap nav-tabs to accordion
(function ($) {
    if (typeof $.fn.accordionTabs === "undefined") {
        $.fn.accordionTabs = function () {
            const el = $(this);

            if ($(el).length === 0) {
                console.error("accordionTabs error: Tabs element not found.");
                return this;
            }

            $(el).addClass("nav-tabs-accordion");

            const tabs = $(el).find(".nav-item .nav-link");
            let activeTab = $(el).find(".nav-item .nav-link.active")[0];
            if (activeTab == undefined)
                activeTab = tabs[0];

            $(tabs).each(function () {
                const tab = $(this);

                $(tab).append("<i class='fal fa-chevron-down float-right chevron'></i>");
                $("<div class='accordion-content'></div>").insertAfter($(tab));

                const accordionContent = $(tab).next();
                const tabContent = $(tab).attr("href");

                //$(tab).removeClass("active");
                //make sure there is only one active tab
                if ($(activeTab)[0] != $(tab)[0])
                    $(tab).removeClass("active");

                $(tabContent).addClass("form-sidebar");
                $(tabContent).appendTo($(accordionContent));
            });

            $(el).show();

            $(tabs).off("click");
            $(tabs).on("click", function () {
                const tab = $(this);
                const tabContent = $(tab).next(".accordion-content");
                const activeTab = $(".nav-tabs-accordion .nav-item .nav-link.show");
                const activeContent = $(activeTab).next(".accordion-content");

                if ($(tab)[0] === $(activeTab)[0]) {
                    tabContent.slideUp("fast", function () {
                        tab.removeClass("show");
                        tabContent.removeClass("show");
                    });
                }
                else {
                    if ($(activeTab).length === 0) {
                        $(tab).addClass("show");
                        $(tabContent).addClass("show");
                        tabContent.slideDown("fast");
                    }
                    else {
                        activeContent.slideUp("fast", function () {
                            if ($(activeTab)[0] !== $(tab)[0]) {
                                activeTab.removeClass("show");
                                activeContent.removeClass("show");
                                $(tab).addClass("show");
                                $(tabContent).addClass("show");
                                tabContent.slideDown("fast");
                            }
                        });
                    }
                }
            });

            return this;
        };
    }
    
}(jQuery));