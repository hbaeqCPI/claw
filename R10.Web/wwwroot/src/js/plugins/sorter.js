//kendo datasource sorter
(function ($) {
    if (typeof $.fn.sorter === "undefined") {
        $.fn.sorter = function (widget, resetPage = false, beforeSort = null, afterSort = null) {
            const sort = widget.dataSource.sort();
            const links = this;
            let allowSort = true;

            $(links).data('sorter', this);

            const createLink = function (link, sort) {
                const icon = sort[0].field === link.data("field") ? `<span class="k-icon k-i-sort-${sort[0].dir}-sm"></span>` : "";
                const label = link.data("label");

                link.html(`<a href="#" class="k-link">${label}${icon}</a>`);
            };

            const refreshLinks = function () {
                const sort = widget.dataSource.sort();

                links.each(function () {
                    createLink($(this), sort);
                });
            };

            links.each(function () {
                const link = $(this);

                link.attr("data-label", link.text());
                createLink(link, sort);

                link.on("click", function () {
                    if (!allowSort)
                        return;

                    const dataSource = widget.dataSource;
                    const sort = dataSource.sort();
                    const field = link.data("field");

                    const sortDescriptor = [{ field: field, dir: sort[0].dir === "desc" || sort[0].field !== field ? "asc" : "desc" }];
                    const query = {
                        sort: sortDescriptor,
                        page: resetPage ? 1 :  dataSource.page(),
                        pageSize: dataSource.pageSize()
                    };

                    if (beforeSort === null && afterSort === null)
                        dataSource.query(query);

                    else if (beforeSort === null)
                        $.when(dataSource.query(query)).then(afterSort());

                    else if (afterSort === null)
                        $.when(beforeSort()).then(dataSource.query(query));

                    else
                        $.when(beforeSort()).then(dataSource.query(query)).then(afterSort());

                    refreshLinks();
                });
            });

            links.disableSort = function () {
                allowSort = false;
            };
            links.enableSort = function () {
                allowSort = true;
            };

            return this;
        };
    }
})(jQuery);