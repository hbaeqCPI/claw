export default class FormExtract {

    constructor() {
        this.mainContainerId = "#formExtract";
        this.searchFormId = this.mainContainerId + "-RefineSearch";
        this.subSearchContainerId = this.mainContainerId + "-SubSearchContainer";
        this.mainPaneContainerId = this.mainContainerId + "-MainPaneContainer";
        this.sourceComboId = "#SourceCode_formExtract";
        this.searchButtonId = "#formExtractSearch";
        this.systemTabId = "#systemTab";

        this.mainContainer = null;
        this.subSearchContainer = null;
        this.mainPaneContainer = null;

        this.getSourceCodeCombo = this.getSourceCodeCombo.bind(this);
    }

    //------------------------- SEARCH PAGE

    // search tab header - contains the system type, screen comboboxes
    initializeSearchTabHeader(searchPage) {

        const self = this;

        this.mainContainer = $(this.mainContainerId);
        this.mainContainer.floatLabels();

        const searchTab = $(`${self.mainContainerId} .nav-tabs:not('.nav-tabs-accordion')`);
        $(searchTab).accordionTabs();

        // open system search tab
        $(this.systemTabId).addClass("show")
        $(this.systemTabId).next(".accordion-content").slideDown("fast")


        //pageHelper.initializeSidebar(this);                                   // this needs: mainPaneContainer, refineSearchContainer, searchResultGrid; also: searchUrl
        const showFilterButtonContainer = $(`${this.mainContainerId} .page-main .sidebar-link`);
        $(`${this.mainContainerId} .page-sidebar.refine-search`).collapsibleSidebar(showFilterButtonContainer);


        // bind system type radio button change event
        $("#FRSystemType input").on("change", function () {
            const screenCombo = self.getSourceCodeCombo();
            if (screenCombo.element.data("fetched") === 1) {
                screenCombo.element.data("fetched", 0);             // set screen combobox status for fetching so it loads new set of screens based on system type
                screenCombo.text("");                               // clear previous selection
            }
        });

        window.cpiBreadCrumbs.addNode({
            name: $(this.mainContainerId).attr("id"),
            label: searchPage.title,
            url: searchPage.url,
            refresh: true,
            updateHistory: true
        });

        pageHelper.moveBreadcrumbs(this.mainContainerId);

        // zoom to specific records, from main screens - N/A
        const defaultSystemType = searchPage.defaultSystemType;
        const defaultSourceCode = searchPage.defaultSourceCode;

        if (defaultSystemType.length) {
            const systemSelector = `#FRSystemType button[value='${defaultSystemType}']`;
            $(systemSelector).click();

            if (defaultSourceCode.length) {
                var combo = self.getSourceCodeCombo();
                combo.dataSource.read();
                combo.value(defaultSourceCode);

                const data = searchPage.defaultFormType + "|" + searchPage.defaultFormName + "|" + searchPage.defaultDocDesc;
                self.loadSourcePages(defaultSourceCode, data)
            }
        }
        

    }


    // initialize all-containing search tab container
    initializeSearchTabMain() {
       
        const self = this;
        const searchContainer = self.mainContainer;
        const sourceCombo = $(this.sourceComboId);

        const clearButton = $(searchContainer).find(".search-clear");
        if (clearButton.length > 0) {
            clearButton.on("click", function (e) {
                e.preventDefault();
                searchContainer.clearSearch();
            });
        }

        sourceCombo.on("change", function (e) {
            if (this.value) {
                const combo = $(this).data("kendoComboBox");
                if (combo.selectedIndex === -1) {        // limit to list
                    combo.value("");
                }
                else {
                    self.loadSourcePages(this.value);
                }
            }
        });
    }

    loadSourcePages = function (sourceCode, data = "") {
        this.mainPaneContainer = $(this.mainPaneContainerId);
        this.subSearchContainer = $(this.subSearchContainerId);

        const self = this;
        self.subSearchContainer.empty();
        self.mainPaneContainer.empty();

        if (this.subSearchContainer && this.mainPaneContainer) {
            const systemType = self.getSystemTypeValue();
            const searchUrl = self.subSearchContainer.data("subsearch-url") + "?systemType=" + systemType + "&sourceCode=" + sourceCode + "&data=" + data;
            const mainUrl = self.mainPaneContainer.data("mainpane-url") + "?systemType=" + systemType + "&sourceCode=" + sourceCode;

            $.when($.get(searchUrl)
                .then(function (searchResponse) {
                    self.subSearchContainer.empty();
                    self.subSearchContainer.html(searchResponse);
                    $.get(mainUrl)
                        .then(function (mainResponse) {
                            self.mainPaneContainer.html(mainResponse);
                        });
                })
            );
        }
    }

    // initialize sub-search page (invention, ctryapp, tmk, etc.)
    initializeSubSearchPage(searchPage) {
        const searchContainer = this.mainContainer;
        searchContainer.floatLabels();

        // close system tab
        const systemTab = $(this.systemTabId);
        systemTab.removeClass("show");
        systemTab.next(".accordion-content").slideUp("fast")

        const searchTab = $(`${this.mainContainerId} .nav-tabs:not('.nav-tabs-accordion')`);
        $(searchTab).accordionTabs();


        // open sub search tab
        const subSearchTab = $(searchPage.tabNameId);
        subSearchTab.addClass("show");
        subSearchTab.next(".accordion-content").slideDown("fast");

        // remove event listeners to search button; new event will be bound in search form's initialize (for example, in formExtractIFW.initializePage())
        $(this.searchButtonId).replaceWith($(this.searchButtonId).clone());

    }

    //------------------------- DATA FILTERS
    getSystemTypeValue() {
        return $("#FRSystemType").find(".btn.active input")[0].value;
    }

    getSystemTypeFilter() {
        const systemType = $("#FRSystemType").find(".btn.active input")[0].value;
        return { systemType: systemType };
    }

    getSourceCodeCombo() {
        return $(this.sourceComboId).data("kendoComboBox");
    }
}
