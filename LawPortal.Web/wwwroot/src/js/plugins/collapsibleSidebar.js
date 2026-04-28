//Collapsible sidebar
(function ($) {
    if (typeof $.fn.collapsibleSidebar === "undefined") {
        $.fn.collapsibleSidebar = function (openButtonContainer) {
            const sidebar = $(this);

            if ($(sidebar).length === 0) {
                console.error("collapsibleSidebar error: Sidebar element not found.");
                return null;
            }

            const hideFiltersLabel = $(sidebar).data("hide-filters");

            $(sidebar).addClass("collapsible");
            $(sidebar).prepend(`<div class="close" role="button" title="${hideFiltersLabel}" aria-label="${hideFiltersLabel}"><span aria-hidden="true">&times;</span></div>`);

            if ($(openButtonContainer).length !== 0) {
                const showFiltersLabel = $(sidebar).data("show-filters");
                const showFiltersIcon = $(sidebar).data("show-filters-icon") || "fal fa-filter";
                $(openButtonContainer).prepend(`<div class="open-collapsed-sidebar" role="button" title="${showFiltersLabel}" aria-label="${showFiltersLabel}"><i class="${showFiltersIcon} pr-2"></i><span>${showFiltersLabel}</span></div>`);            
            }

            const showResultsButton = $(sidebar).parent().find(".page-sidebar.collapsible .show-results[role='button']");
            const closeButton = $(sidebar).parent().find(".page-sidebar.collapsible .close[role='button']");
            const openButton = $(openButtonContainer).find(".open-collapsed-sidebar[role='button']");

            const isFloating = function () {
                return $(sidebar).css("position") === "fixed";
            };

            const close = function () {
                $(sidebar).addClass("collapsed");
                $(sidebar).removeClass("show");
                $(openButton).addClass("collapsed");
                $(openButton).removeClass("show");
            };

            const open = function () {
                $(sidebar).addClass("show");
                $(sidebar).removeClass("collapsed");
                $(openButton).addClass("show");
                $(openButton).removeClass("collapsed");
            };

            $(showResultsButton).off().on("click", function () {
                close();
            });

            $(closeButton).off().on("click", function () {
                close();
            });

            $(openButton).off().on("click", function () {
                open();
            });

            const onMouseUp = function (e) {
                if (isFloating() && $(sidebar).hasClass("show") && !sidebar.is(e.target) && sidebar.has(e.target).length === 0) {
                    close();
                }
            };
            $(".site-main").unbind("mouseup", onMouseUp).mouseup(onMouseUp);
            $(".site-footer").unbind("mouseup", onMouseUp).mouseup(onMouseUp);

            return {
                isFloating: isFloating,
                close: close,
                open: open,
                container: sidebar,
                openButton: openButton
            };
            
        };
    }
}(jQuery));