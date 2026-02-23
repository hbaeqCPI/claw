import BasePage from "./basePage";

export default class SearchPage extends BasePage {

    constructor() {
        super();

        this.searchUrl = "";
        this.searchContainer = "";
        this.searchResultGrid = null;
        this.searchResultContainer = "";
        this.refineSearchContainer = "";
        this.showNoRecordError = true;
        this.mainSearchRecordIds = [];
    }


    initializeSearchPage(searchPage) {
        this.searchContainer = searchPage.container;
        this.searchUrl = searchPage.url;
        this.exportExcelUrl = "";

        const searchForm = $(searchPage.form).cpiSearchForm();
        const refreshSearchPage = function () {
            searchForm.resetPickLists();
            pageHelper.initializeMainSearchTabs(searchPage.container);
        };

        window.cpiBreadCrumbs.addNode({
            name: $(searchPage.container).attr("id"),
            label: searchPage.title,
            url: searchPage.url,
            refresh: true,
            refreshHandler: refreshSearchPage,
            updateHistory: true,
            classNames: "hide"
        });

        $(searchPage.container).cpiMainButtons();
        pageHelper.initializeMainSearchTabs(searchPage.container);

        $(searchPage.container).floatLabels();
        $(searchPage.container).moreInfo();

        //auto submit form
        $(searchForm).submit();
    }
    
    initializeSearchResultPage(searchResultPage) {
        this.searchResultGrid = $(searchResultPage.grid);
        this.searchResultContainer = searchResultPage.container;
        this.refineSearchContainer = searchResultPage.refineSearchContainer;
        const filterCount = pageHelper.initializeSidebar(this);

        if (this.searchResultGrid.length > 0) {
            let resultsGrid = this.searchResultGrid.data("kendoGrid");

            if (resultsGrid === undefined)
                resultsGrid = this.searchResultGrid.data("kendoListView");

            //no need to set pageSize. it is already defined in the grid
            //if (parseInt(searchResultPage.gridPageSize) > 0) {
            //    resultsGrid.dataSource.pageSize(searchResultPage.gridPageSize);
            //}
            //else {
            //    resultsGrid.dataSource.read();
            //}

            //show initial result if refine search is on
            if (!$(this.searchResultGrid).hasClass("no-refine-search")) {
                cpiLoadingSpinner.show();
                resultsGrid.dataSource.read().always(function () {
                    cpiLoadingSpinner.hide();
                });
            }

            //show details when main search returns 1 record
            //resultsGrid.one("dataBound", function () {
            //    if (this.dataSource.total() === 1) {
            //        $(this.wrapper).find(".detailsLink").click();
            //    }
            //});

            this.searchResultGrid.on("click",
                ".details-link",
                function (e) {
                    e.preventDefault();
                    const link = $(this);
                    cpiStatusMessage.hide();
                    pageHelper.openDetailsLink(link);
                });
        }

        const refreshSearchResultPage = function () {
            pageHelper.moveBreadcrumbs(searchResultPage.container);

            filterCount.refreshAll();
            filterCount.openDefault();
            const grid = $(searchResultPage.grid).data("kendoGrid");
            if (grid)
                grid.dataSource.read();

            const listView = $(searchResultPage.grid).data("kendoListView");
            if (listView)
                listView.dataSource.read();
        };

        window.cpiBreadCrumbs.addNode({
            name: $(searchResultPage.container).attr("id"),
            label: searchResultPage.title,
            url: searchResultPage.url,
            refresh: true,
            updateHistory: true,
            refreshHandler: refreshSearchResultPage,
            classNames: (searchResultPage.showBreadCrumbTrail || false) ? "" : "hide"
        });
        $(searchResultPage.container).cpiMainButtons();

        this.cpiStatusMessage.hide();
        pageHelper.moveBreadcrumbs(searchResultPage.container);
        this.initializeServerExcelExport();
    }

    initializeSidebarPage(sidebarPage) {
        this.refineSearchContainer = sidebarPage.form;
        this.searchUrl = sidebarPage.url;
        this.searchResultGrid = $(sidebarPage.grid);
        this.searchResultContainer = sidebarPage.container;

        this.exportExcelUrl = "";

        const searchResultsContainer = $(sidebarPage.container);
        const searchForm = $(sidebarPage.form);
        const cpiSearchForm = searchForm.cpiSearchForm();
        const refreshSidebarPage = () => {
            cpiSearchForm.resetPickLists();

            let resultsGrid = this.searchResultGrid.data("kendoGrid");
            if (resultsGrid === undefined)
                resultsGrid = this.searchResultGrid.data("kendoListView");
            if (resultsGrid != undefined) {
                //use .query() to reset to page 1
                //resultsGrid.dataSource.read();
                const dataSource = resultsGrid.dataSource;
                dataSource.query({
                    sort: dataSource.sort(),
                    page: 1,
                    pageSize: dataSource.pageSize()
                });
            }
        };

        //add filters action button first before sidebar toggle button to keep sidebar button on the left most side
        const showFiltersButtonContainer = searchResultsContainer.find(searchForm.data("toggle-button-container"));
        if (showFiltersButtonContainer) {
            const hideButtonLabels = searchForm.data("toggle-button-label-hidden") == "true" || false
            const showFiltersLabel = hideButtonLabels ? "" : searchForm.data("toggle-button-show");
            const hideFiltersLabel = hideButtonLabels ? "" : searchForm.data("toggle-button-hide");
            const showFiltersIcon = searchForm.data("toggle-button-show-icon") || "fal fa-filter";
            const hideFiltersIcon = searchForm.data("toggle-button-hide-icon") || "fal fa-filter";
            const toggleFilters = (e) => {
                searchForm.toggle();
                $(e.currentTarget).find(".label").text(searchForm.is(":visible") ? hideFiltersLabel : showFiltersLabel);
                $(e.currentTarget).find("i").removeClass(searchForm.is(":visible") ? showFiltersIcon : hideFiltersIcon);
                $(e.currentTarget).find("i").addClass(searchForm.is(":visible") ? hideFiltersIcon : showFiltersIcon);
            };
            showFiltersButtonContainer.prepend(`<a href="#" class="k-button toggle-filters ${hideButtonLabels ? 'no-label' : ''}" title="${showFiltersLabel}" aria-label="${showFiltersLabel}"><i class="${showFiltersIcon} ${hideButtonLabels ? '' : 'pr-2'}"></i><span class="label">${showFiltersLabel}</span><span class="total-filter-count badge badge-pill badge-info"></span></a>`);
            const toggle = showFiltersButtonContainer.find(".toggle-filters");
            toggle.on("click", toggleFilters);

            if (showFiltersButtonContainer.parent().hasClass("show"))
                toggle.click();
        }

        this.sidebar = pageHelper.initializeSidebarPage(`${sidebarPage.container} .page-sidebar`, `${sidebarPage.container} .page-main`);

        pageHelper.moveBreadcrumbs(sidebarPage.container);
        window.cpiBreadCrumbs.addNode({
            name: searchResultsContainer.attr("id"),
            label: sidebarPage.title,
            url: sidebarPage.url,
            refresh: true,
            refreshHandler: refreshSidebarPage,
            updateHistory: true
        });

        if (this.searchResultGrid.length > 0) {
            let resultsGrid = this.searchResultGrid.data("kendoGrid");

            if (resultsGrid === undefined)
                resultsGrid = this.searchResultGrid.data("kendoListView");

            if (!$(this.searchResultGrid).hasClass("no-refine-search")) {
                cpiLoadingSpinner.show();
                resultsGrid.dataSource.read().always(function () {
                    cpiLoadingSpinner.hide();
                });
            }

            this.searchResultGrid.on("click",
                ".details-link",
                function (e) {
                    e.preventDefault();
                    const link = $(this);
                    cpiStatusMessage.hide();
                    pageHelper.openDetailsLink(link);
                });
        }

        const filterTabs = $(`${this.searchResultContainer} .nav-tabs.refine-search`);
        filterTabs.scrollingTabs();
        let refreshTotalFilterCount = function () { };
        const filterCount = $(filterTabs).filterCount();
        if (filterCount) {
            filterCount.refreshAll();

            refreshTotalFilterCount = function () {
                const totalFilterCount = filterCount.total();
                showFiltersButtonContainer.find(".total-filter-count").html(totalFilterCount > 0 ? totalFilterCount : "");

                const clearButton = searchResultsContainer.find(".search-clear");
                if (totalFilterCount > 0)
                    clearButton.show();
                else
                    clearButton.hide();
            };
            refreshTotalFilterCount();
        }

        this.searchResultGrid.refineSearch(this.refineSearchContainer, function (el) {
            if (filterCount) {
                filterCount.refresh($(el));
                refreshTotalFilterCount();
            }

            //auto close floating sidebar
            //if (sidebar.isFloating()) {
            //    sidebar.close();
            //}
        }, this.validate);

        //clear filter button
        //delegate so would fire last, after inputs are cleared
        $("body").on("click", `${this.searchResultContainer} .search-clear`, (e) => {
            e.preventDefault();

            const refineSearch = $(this.refineSearchContainer);
            refineSearch.clearSearch();
            if (filterCount) {
                filterCount.refreshAll();
                refreshTotalFilterCount();
            }
            //filterCount.refreshAll();
            //showFiltersButtonContainer.find(".total-filter-count").html("");

            //refresh grid/listview
            if (!($(this.searchResultGrid).hasClass("no-refine-search"))) {
                refreshSidebarPage();
            }

            $(e.currentTarget).hide();
        });

        searchForm.floatLabels();
        searchForm.moreInfo();
        searchResultsContainer.cpiMainButtons();

        this.initializeServerExcelExport();

        return filterCount;
    }

    initializeServerExcelExport = () => {
        const self = this;
        $(this.searchResultContainer).find('.k-grid-excel-server').on('click', (e) => {
            e.preventDefault();

            const exportSettingsUrl = this.searchResultGrid.data("export-excel-settings-url");
            if (!exportSettingsUrl) {
                searchResultsExportToExcel();
            }
            else {
                let mainUrl = exportSettingsUrl.split("/");
                mainUrl.pop();
                mainUrl = mainUrl.join("/");
                const url = `${mainUrl}/HasExportSetting`;

                $.get(url)
                    .done(function (result) {
                        if (result.hasExportSetting) {
                            searchResultsExportToExcel();
                        }
                        else {
                            searchResultsExportSettings(true);
                        }
                    })
            }
        });

        $(this.searchResultContainer).find('.k-export-settings').on('click', (e) => {
            e.preventDefault();
            searchResultsExportSettings(false);
        });

        function searchResultsExportToExcel() {
            const bgCheckUrl = self.searchResultGrid.data("export-excel-bg-check-url");
            if (bgCheckUrl && bgCheckUrl.length > 0) {
                $.get(bgCheckUrl).done((result) => {
                    if (result.ExportInBackGround)
                        searchResultsExportToExcelBackground();
                    else searchResultsExportToExcelForeground();

                }).fail(function (error) {
                    pageHelper.showErrors(error.responseText);
                });
            }
            else searchResultsExportToExcelForeground();
        }

        function searchResultsExportToExcelForeground() {
            const url = self.searchResultGrid.data("export-excel-url");
            if (url) {
                const data = self.gridMainSearchFilters();
                const dataJSON = JSON.stringify(data.mainSearchFilters);
                const columns = self.getSearchResultsGridHiddenColumns();

                let sortField = "";
                let sortDirection = "";

                const sort = self.searchResultGrid.data("kendoGrid").dataSource.sort();
                if (sort) {
                    sortField = sort[0].field;
                    sortDirection = sort[0].dir;
                }

                const html = '<form method="POST" id="excel-server-export-form" action="' + url + '">';
                let form = $(html);
                form.append($('<input type="hidden" name="mainSearchFiltersJSON"/>').val(dataJSON));
                form.append($('<input type="hidden" name="hiddenColumns"/>').val(columns.join()));
                form.append($('<input type="hidden" name="sortField"/>').val(sortField));
                form.append($('<input type="hidden" name="sortDirection"/>').val(sortDirection));
                $("body").append(form);
                form.submit();
                $("#excel-server-export-form").remove();
   
            }
        }

        function searchResultsExportToExcelBackground() {
            const url = self.searchResultGrid.data("export-excel-url");
            if (url) {
                const data = self.gridMainSearchFilters();
                const dataJSON = JSON.stringify(data.mainSearchFilters);
                const columns = self.getSearchResultsGridHiddenColumns();

                let sortField = "";
                let sortDirection = "";

                const sort = self.searchResultGrid.data("kendoGrid").dataSource.sort();
                if (sort) {
                    sortField = sort[0].field;
                    sortDirection = sort[0].dir;
                }

                const silentMessage = self.searchResultGrid.data("export-excel-msg");
                pageHelper.showSuccess(silentMessage);
                $.post(url, {
                    mainSearchFiltersJSON: dataJSON,
                    hiddenColumns: columns.join(),
                    sortField: sortField,
                    sortDirection: sortDirection,
                    silent: true
                }).done(() => {
                }).fail(function (error) {
                    //pageHelper.showErrors(error.responseText);
                });

            }
        }

        function searchResultsExportSettings(showExport) {
            const url = self.searchResultGrid.data("export-excel-settings-url");
            if (url) {
                $.get(url, {showExport})
                    .done(function (result) {
                        const popupContainer = $(".site-content .popup");
                        popupContainer.empty();
                        popupContainer.html(result);

                        const dialogContainer = popupContainer.find(".modal");
                        let entryForm = dialogContainer.find("form")[0];
                        dialogContainer.modal("show");

                        entryForm = $(entryForm);
                        entryForm.on("submit",
                            function (e) {
                                e.preventDefault();
                                dialogContainer.modal("hide");
                                searchResultsExportToExcel();
                        });

                        let mainUrl = url.split("/");
                        mainUrl.pop();
                        mainUrl = mainUrl.join("/");

                        dialogContainer.on("change", ".export-settings-fields input[type='checkbox']", function () {
                            const checkbox = $(this);
                            const name = checkbox.attr("name");
                            const value = checkbox.prop("checked");
                            const url = `${mainUrl}/UpdateExportSetting`;
                            $.post(url, { propertyName: name, include: value })
                                .fail(function (e) {
                                    pageHelper.showErrors(e.responseText);
                                });
                        });
                        dialogContainer.on("change", "#DefaultSetting", function () {
                            const checkbox = $(this);
                            const value = checkbox.prop("checked");
                            var screenCode = dialogContainer.find("#ScreenCode").val();

                            const url = `${mainUrl}/UpdateExportSettingDefault`;
                            $.post(url, { screenCode: screenCode, isDefault: value });
                        });

                        dialogContainer.on("change", "#SelectAll", function () {
                            var fields = dialogContainer.find(".export-settings-fields input[type='checkbox']");
                            const fieldsSelected = [];
                            if ($(this).is(":checked")) {
                                fields.each(function () {
                                    const checkbox = $(this);
                                    if (!checkbox.is(":checked")) {
                                        $(this).attr("checked", true);
                                        const name = checkbox.attr("name");
                                        const value = checkbox.prop("checked");
                                        fieldsSelected.push({ propertyName: name, include: value });
                                    }
                                });
                            }
                            else {
                                fields.each(function () {
                                    const checkbox = $(this);
                                    if (checkbox.is(":checked")) {
                                        $(this).attr("checked", false);
                                        const name = checkbox.attr("name");
                                        const value = checkbox.prop("checked");
                                        fieldsSelected.push({ propertyName: name, include: value });
                                    }
                                });
                            }
                            if (fieldsSelected.length > 0) {
                                const url = `${mainUrl}/UpdateExportSettings`;
                                $.post(url, { exportSettings: fieldsSelected })
                                    .fail(function (e) {
                                        pageHelper.showErrors(e.responseText);
                                    });
                            }
                        });
                        
                    })
                    .fail(function (e) {
                        cpiLoadingSpinner.hide();
                        pageHelper.showErrors(e);
                    });
            }
        }
    }

    gridMainSearchFilters = (e) => {
        //kendo will pass an object if called from datasource.Data()
        const filterContainer = typeof e === "string" ? e : this.refineSearchContainer;
        return pageHelper.gridMainSearchFilters($(filterContainer));
    }

    getSearchResultsGridHiddenColumns = () => {
        const columns = this.searchResultGrid.data("kendoGrid").columns;
        const hiddenColumns = [];
        $.each(columns, function (index) {
            if (this.hidden) {
                hiddenColumns.push(this.field);
            }
        });
        return hiddenColumns;
    }

    //sets the list of record Ids for record navigation
    searchResultGridRequestEnd = (e) => {
        cpiStatusMessage.hide();
        this.mainSearchRecordIds = [];

        if (e.response) {
            $(this.refineSearchContainer).find(".total-results-count").html(e.response.Total);

            if (e.response.Data.length > 0) {
                this.mainSearchRecordIds = e.response.Ids;
                $(this.searchResultContainer).find(".no-results-hide").show();
            }
            else if (this.showNoRecordError) {
                const form = $(`${this.searchContainer}-MainSearch`);
                pageHelper.showErrors($(form).data("no-results") || $("body").data("no-results"));
                $(this.searchResultContainer).find(".no-results-hide").hide();
            }
        }
    }

    searchResultGridError = (e) => {
        cpiLoadingSpinner.hide();
        pageHelper.showErrors(e.xhr.responseText || "Error retrieving search results.");
    }

    searchResultGridSharePointError = (e) => {
        console.log("srg", e);
        if (e.xhr.status == 401) {
            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Graph/SharePoint`;

            sharePointGraphHelper.getGraphToken(url, () => {
                if (this.searchResultGrid) {
                    this.searchResultGrid.data("kendoGrid").dataSource.read();
                    cpiLoadingSpinner.hide();
                }
            });
        }
        else {
            cpiLoadingSpinner.hide();
            pageHelper.showErrors(e.xhr.responseText || "Error retrieving search results.");
        }
    }

    validate = () => {
        return this.checkForInvalidDateRange();
    }

    checkForInvalidDateRange = () => {
        const criteria = pageHelper.gridMainSearchFilters($(this.refineSearchContainer));
        const froms = criteria.mainSearchFilters.filter(c => c.property.toLowerCase().endsWith("from"));
        const tos = criteria.mainSearchFilters.filter(c => c.property.toLowerCase().endsWith("to"));
        for (let from of froms) {
            const fromProperty = from.property.toLowerCase();
            const toProperty = fromProperty.substring(0, (fromProperty.length - 4)) + "to";

            const to = tos.find(c => c.property.toLowerCase() === toProperty);
            if (to && to.value < from.value && to.value.endsWith("T00:00:00")) {
                const message = $("body").data("invalid-date-range");
                pageHelper.showErrors(message);
                return false;
                //break;
            }
        }
        return true;
    }
}

