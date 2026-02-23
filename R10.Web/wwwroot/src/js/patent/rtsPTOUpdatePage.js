import SearchPage from "../searchPage";

export default class RTSPTOUpdatePage extends SearchPage {

    constructor() {
        super();
        this.showNoRecordError = false;
    }

    initializeSearchPage(searchPage) {
        this.searchContainer = searchPage.container;
        this.searchUrl = searchPage.url;
        
        $(searchPage.form).cpiSearchForm();
        $(searchPage.container).cpiMainButtons();

        this.initializeMainSearchTabs(searchPage.container);

        $(searchPage.container).floatLabels();
        $(searchPage.container).moreInfo();

        //
        this.searchResultGrid = $(searchPage.grid);
        this.searchResultContainer = searchPage.container;
        this.refineSearchContainer = searchPage.refineSearchContainer;

        pageHelper.initializeSidebar(this);

        if (this.searchResultGrid.length > 0) {
            let resultsGrid = this.searchResultGrid.data("kendoGrid");

            if (resultsGrid === undefined) 
                resultsGrid = this.searchResultGrid.data("kendoListView");
          
            resultsGrid.dataSource.read();
        }
        this.cpiStatusMessage.hide();
    }

    initializeMainSearchTabs(searchContainer) {
        const vtabFilterCount = $(`${searchContainer} .nav-tabs-vertical`).filterCount();
        vtabFilterCount.refreshAll();

        //$(`${searchContainer} .tab-content.search`).liveSearch(function (el) {
        $(`${searchContainer} .accordion-content`).liveSearch(function (el) {
            vtabFilterCount.refresh(el);
        });

        //clear filter count
        $("body").on("click", `${searchContainer} .search-clear`, function () {
            vtabFilterCount.refreshAll();
        });
    }

}




