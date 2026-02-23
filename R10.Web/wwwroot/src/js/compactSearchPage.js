import BasePage from "./basePage";

export default class CompactSearchPage extends BasePage {

    constructor() {
        super();

        this.searchUrl = "";
        this.searchContainer = "";
        this.searchForm = "";
        this.searchResultGrid = null;
        this.showNoRecordError = true;
        this.mainSearchRecordIds = [];
    }

    initializeSearchPage(searchPage) {
        this.searchContainer = searchPage.container;
        this.searchUrl = searchPage.url;
        this.searchForm = searchPage.form;
        this.searchResultGrid = searchPage.searchResultGrid;

        if ($(this.searchForm).data("validator"))
           $(this.searchForm).data("validator").settings.ignore = ".data-val-ignore, .d-none";

        const refreshSearchPage = function () {
            $(this.searchForm).resetPickLists();
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

        this.makeTabsAccordion();
        pageHelper.focusLabelControl($(searchPage.container));

        $(searchPage.container).cpiMainButtons();
        $(searchPage.container).floatLabels();
        $(searchPage.container).moreInfo();
        this.initializeTabFilters();
        this.initializeSidebar();
        this.initializeSearchResult();

        pageHelper.moveBreadcrumbs(this.searchContainer);
        this.onSearchInitialized();
    }

    makeTabsAccordion() {
        var tabsLink = $(`${this.searchContainer} .nav-tabs-accordion a.nav-link`);
        $.each(tabsLink, function (index, value) {
            const tabLink = $(value);
            if (index===0)
                $(tabLink).addClass("show");
            
            $(tabLink).append(`<i class='fal fa-chevron-down float-right chevron'></i>`);
        });

        var tabs = $(`${this.searchContainer} .nav-tabs-accordion li.nav-item`);
        var contents = $(`${this.searchContainer} .tab-content.search .tab-pane`);
        $.each(tabs, function (index, value) {
            const tab = $(value);

            const tabContent = $(contents[index]);
            tabContent.addClass("form-sidebar");
            $(tabContent).appendTo(tab);
            $(tabContent).wrap(`<div class="accordion-content show" ${index === 0 ? 'style="display:block"':''}></div>`);
        });
        $(`${this.searchContainer} .tab-content.search`).remove();

        tabsLink.on("click", function () {
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

    }
     
    initializeTabFilters() {
        const container = $(this.searchContainer);
        const sidebar = container.find(".page-sidebar");
        const tabs = container.find(".nav-tabs-accordion");
        const tabsFilterCount = tabs.filterCount();
        tabsFilterCount.refreshAll();
        sidebar.data("filter-count", tabsFilterCount.total());

        const form = $(this.searchForm);
        $("body").on("click", `${this.searchContainer} .search-clear`, ()=> {
            form.clearSearch();
            this.onSearchCleared();

            //reset totals
            tabsFilterCount.refreshAll();
            sidebar.data("filter-count", tabsFilterCount.total());
        });

        tabs.liveSearch(function (el) { 
            //update totals
            tabsFilterCount.refresh(el);
            sidebar.data("filter-count", tabsFilterCount.total());
        });

        $("body").on("click", `${this.searchContainer} .search-submit`, () => {
            var ok = this.onSearchSubmit();
            if (!ok)
                return;

            pageHelper.resetFormValidator(this.searchForm); //for dynamically added fields

            const form = $(this.searchForm);
            if (form.valid()) {
                this.refreshSearchResults();
            }
        });
        
    };

    refreshSearchResults = () => {
        let grid = $(this.searchResultGrid).data("kendoGrid");

        if (grid === undefined)
            grid = $(this.searchResultGrid).data("kendoListView");

        cpiLoadingSpinner.show("", 1);
        if (grid.dataSource.options.serverSorting) {
            const sort = grid.dataSource.sort();
            grid.dataSource.query({
                sort: { field: sort[0].field, dir: sort[0].dir },
                page: 1,
                pageSize: grid.dataSource.pageSize()
            }).then(function () {
                cpiLoadingSpinner.hide();
            });
        }
        else {
            grid.dataSource.read().then(function () {
                cpiLoadingSpinner.hide();
            });
        }
    }

    //override to respond to the events
    onSearchInitialized = () => { }
    onSearchCleared = () => { }
    onSearchSubmit = () => { return true; }
    onSearchCriteriaLoaded = () => {}

    gridMainSearchFilters = (e) => {
        //kendo will pass an object if called from datasource.Data()
        const filterContainer = typeof e === "string" ? e : this.searchForm;
        return pageHelper.gridMainSearchFilters($(filterContainer));
    }

    //sets the list of record Ids for record navigation
    searchResultGridRequestEnd = (e) => {
        cpiStatusMessage.hide();
        
        if (e.response) {
            $(this.searchContainer).find(".total-results-count").html(e.response.Total);

            if (e.response.Data.length > 0) {
                this.mainSearchRecordIds = e.response.Ids;
            }
            else if (this.showNoRecordError) {
                const form = $(`${this.searchContainer}-MainSearch`);
                pageHelper.showErrors($(form).data("no-results") || $("body").data("no-results"));
            }
        }
    }

    searchResultGridError = (e) => {
        cpiLoadingSpinner.hide();
        pageHelper.showErrors(e.xhr.responseText || "Error retrieving search results.");
    }

    initializeSearchResult() {
        if (this.searchResultGrid) {

            $(this.searchResultGrid).on("click",
                ".details-link",
                function (e) {
                    e.preventDefault();
                    const link = $(this);
                    cpiStatusMessage.hide();
                    pageHelper.openDetailsLink(link);
                });
        }
    }

    
    initializeSidebar() {
        const searchContainer = $(this.searchContainer);
        const sidebar = searchContainer.find(".page-sidebar");
        let openButton = null;

        //for show filter link on the result view
        searchContainer.find(".kendo-Grid .k-grid-toolbar").addClass("sidebar-link"); 
        const showFilterLink = searchContainer.find(".page-main .sidebar-link");
        if (showFilterLink.length !== 0) {
            const showFiltersLabel = sidebar.data("show-filters");
            $(showFilterLink).prepend(`<div class="open-collapsed-sidebar" role="button" title="${showFiltersLabel}" aria-label="${showFiltersLabel}"><i class="fal fa-filter pr-2"></i><span>${showFiltersLabel}</span><span class="total-filter-count"></span></div>`);

            openButton = $(showFilterLink).find(".open-collapsed-sidebar[role='button']");
            $(openButton).on("click", function () {
                open(this);
            });
        }
        
        const closeButton = $(sidebar).find(".close[role='button']");
        $(closeButton).on("click", function () {
            close();
        });
           
        const showResultsButton = $(sidebar).find(".show-results[role='button']");
        $(showResultsButton).on("click", function () {
            close();
        });

        const open = function (openButton) {
            sidebar.addClass("show");
            sidebar.removeClass("collapsed");

            if (openButton) {
                $(openButton).addClass("show");
                $(openButton).removeClass("collapsed");
            } 
        };

        const close = function () {
            $(sidebar).addClass("collapsed");
            $(sidebar).removeClass("show");

            if (openButton) {
                $(openButton).addClass("collapsed");
                $(openButton).removeClass("show");

                if (showFilterLink.length !== 0) {
                    const filterCount = $(sidebar).data("filter-count");
                    $(showFilterLink).find(".total-filter-count").html(filterCount > 0 ? filterCount : "");
                }
            }
        };

        const isFloating = function () {
            return $(sidebar).css("position") === "fixed";
        };

        const onMouseUp = function (e) {
            if (isFloating() && $(sidebar).hasClass("show") && !sidebar.is(e.target) && sidebar.has(e.target).length === 0) {
                close();
            }
        };
        $(".site-main").unbind("mouseup", onMouseUp).mouseup(onMouseUp);
        $(".site-footer").unbind("mouseup", onMouseUp).mouseup(onMouseUp);

        searchContainer.find(".save-filters").on("click", () => {
            pageHelper.getSearchCriteriaScreen(this.searchForm, null, true);
        });

        searchContainer.find(".load-filters").on("click", () => {
            pageHelper.getSearchCriteriaScreen(this.searchForm, null, false, (response) => {
                $(this.searchForm).clearSearch();
                this.loadSearchCriteria(response);
            });
        });        
    }

    loadSearchCriteria(response) {
        const criteria = JSON.parse(response);
        if (criteria.length > 0) {
            const keyValues = {};
            for (const item of criteria) {
                keyValues[item.property] = item.value;
            }
            pageHelper.populateForm($(this.searchForm), keyValues);
            this.onSearchCriteriaLoaded();

            //auto refresh
            //if (this.searchResultGrid) {
            //    let resultsGrid = $(this.searchResultGrid).data("kendoGrid");
            //    if (!resultsGrid)
            //        resultsGrid = $(this.searchResultGrid).data("kendoListView");
            //    resultsGrid.dataSource.read();
            //}
        }
    }
    
}

