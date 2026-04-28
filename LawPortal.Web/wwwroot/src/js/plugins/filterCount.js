//Display number of active filters on a tab
(function ($) {
    if (typeof $.fn.filterCount === "undefined") {
        $.fn.filterCount = function () {
            const tabs = $(this);

            if ($(tabs).length === 0) {
                console.error("filterCount error: Tab element not found.");
                return null;
            }

            const countFilters = function (tab) {
                const tabContent = $(tab).attr("href");
                return $(tabContent).countFilters();
            };

            const updateCount = function (tab) {
                $(tab).find(".filter-count").remove();
                const count = countFilters(tab);
                if (count > 0) {
                    $(tab).append(`<span class="filter-count">${count}</span>`);
                }
            };

            const filterCount = {};
            filterCount.tabs = tabs;

            filterCount.refresh = function (el) {
                const tabPane = $(el).closest(".tab-pane");
                const tab = $(filterCount.tabs).find(`a[href="#${$(tabPane).attr("id")}"]`);
                updateCount(tab);
            };

            filterCount.refreshAll = function () {
                $(tabs).find(".nav-item .nav-link").each(function () {
                    updateCount(this);
                });
            };

            filterCount.setTrigger = function (filterContainer) {
                $(filterContainer).liveSearch(filterCount.refresh);
            };

            filterCount.count = function (tab) {
                return countFilters(tab);
            };

            filterCount.openDefault = function () {
                const tabs = filterCount.tabs.find(".nav-item .nav-link");
                let tab = $(tabs)[0];

                //do not open any tab but set first tab as active if no-default-tab
                if ($(filterCount.tabs[0]).hasClass("no-default-tab")) {
                    $(tab).addClass("active");
                    return;
                }

                if ($(tabs).length > 1) {
                    $(tabs).each(function () {
                        //if (filterCount.count($(this)) > 0) {
                        if ($(this).hasClass("active")) {
                            tab = $(this);
                            return false;
                        }
                    });
                }
                if (!$(tab).hasClass("show")) {
                    $(tab).click();
                }
            };

            filterCount.total = function() {
                const tabs = filterCount.tabs.find(".nav-item .nav-link");
                let total = 0;
                $(tabs).each(function() {
                    total = total + countFilters($(this));
                });
                return total;
            };

            return filterCount;
        };
    }
    
}(jQuery));