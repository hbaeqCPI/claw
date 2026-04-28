//import cpiLoadingSpinner from "../loadingSpinner";
import "./clearSearch";
import "./liveSearch";

//Refresh kendo grid when filters from given filterContainer are updated
(function ($) {
    if (typeof $.fn.refineSearch === "undefined") {
        $.fn.refineSearch = function (filterContainer, callback,beforeSearch) {
            if ($(this).length === 0) {
                console.error("refineSearch error: Grid element not found.");
                return this;
            }

            let grid = $(this).data("kendoGrid");

            if (grid === undefined)
                grid = $(this).data("kendoListView");

            const refreshSearchResults = function (el) {
                let proceed = true;
                if (beforeSearch)
                    proceed = beforeSearch();

                if (proceed) {
                    const sort = grid.dataSource.sort();
                    var queryOptions = {
                        page: 1,
                        pageSize: grid.dataSource.pageSize()
                    };
                    if (sort && sort.length > 0) {
                        queryOptions.sort = { field: sort[0].field, dir: sort[0].dir };
                    }
                    cpiLoadingSpinner.show("", 1);
                    grid.dataSource.query(queryOptions).then(function () {
                        cpiLoadingSpinner.hide();
                        if (callback) {
                            callback(el);
                        }
                        el.focus();
                    });
                }
                
            };

            if ($(this).hasClass("no-refine-search")) {
                if (callback)
                    $(filterContainer).liveSearch(callback);
            }
            else
                $(filterContainer).liveSearch(refreshSearchResults);

            return this;
        };
    }
}(jQuery));
