export default class GlobalSearch {

    constructor() {
        this.BASICSEARCH = "b";
        this.SEARCH_MIN_LENGTH = 2;

        this.getSearchCriteriaData = this.getSearchCriteriaData.bind(this);
        this.getSearchCriteriaDoc = this.getSearchCriteriaDoc.bind(this);

        this.getBasicError = this.getBasicError.bind(this);
        this.getAdvancedError = this.getAdvancedError.bind(this);

        this.refreshResults = this.refreshResults.bind(this);
        this.loadCriteriaToScreen = this.loadCriteriaToScreen.bind(this);
        this.doInitialSearch = this.doInitialSearch.bind(this);
        this.doBasicSearch = this.doBasicSearch.bind(this);
        this.doAdvancedSearch = this.doAdvancedSearch.bind(this);

        this.showSpinner = this.showSpinner.bind(this);
        this.hideSpinner = this.hideSpinner.bind(this);

        this.spinnerCount = 0;          // spinner semaphore

    }

    initializeSearchIndex(searchPage) {
        //const self = this;
        //$(searchPage.containerId).floatLabels();          // not sure if useful for checkbox

        // sidebar link - n/a now
        //const showFilterButtonContainer = $(`${searchPage.containerId} .page-main .sidebar-link`);
        //$(`${searchPage.containerId} .page-sidebar.refine-search`).collapsibleSidebar(showFilterButtonContainer);

        window.cpiBreadCrumbs.addNode({
            name: $(searchPage.containerId).attr("id"),
            label: searchPage.title,
            url: searchPage.url,
            refresh: true,
            updateHistory: true
        });
        pageHelper.moveBreadcrumbs(searchPage.containerId);
    }

    initializeMidPane(searchString, defaultDataFieldList) {
        const self = this;
        const formName = "#globalSearchForm";
        const screenName = "globalSearch"

        // use Advanced mode if default criteria list is not empty and have search value
        // this will be overriden if user has saved default criteria
        if (defaultDataFieldList && defaultDataFieldList.length > 0) {
            // switch search mode
            var baseMode = $("#SearchModeOptionBasic");
            baseMode.checked = false;
            baseMode.parent("label.btn-outline.gsLabel").removeClass("active");
            var advancedMode = $("#SearchModeOptionAdvanced");
            advancedMode.checked = true;
            advancedMode.parent("label.btn-outline.gsLabel").addClass("active");
            $("#SearchMode").trigger("change");

            // prepare data for grid                       
            var gridData = [];
            defaultDataFieldList.forEach(function (item, index, arr) {
                var tempField = {
                    "FieldId": 0,
                    "Field": { "FieldId": item.FieldId, "FieldLabel": item.FieldLabel },
                    "Criteria": item.FieldValue,
                    "OrderEntry": 0,
                    "LogicalOperator": "",
                    "LeftParen": "",
                    "RightParen": ""
                };
                gridData.push(tempField);
            });
            if (gridData.length > 0) {
                const dataGrid = $("#gridDataFilter").data("kendoGrid");
                if (dataGrid)
                    dataGrid.dataSource.data(gridData);
            }            
        }


        // SEARCH MODE 
        self.showSearchContainer(self.getSearchMode() );
        $("#SearchMode input").on("change", function () {
            self.showSearchContainer($(this).val());
        });

        // SEARCH SCOPE 
        
        $("#chkSearchDoc").change(function () {
            self.showDocumentTypeCriteria(this.checked);
        });

        $("#chkSearchData").change(function () {
            self.showBasicQueryType(!this.checked);
        });


        // open query help form
        $(".globalsearch-help").on("click", function (e) {
            //const form = $(e.currentTarget).closest("form");
            //const url = form.data("help-url");
            const url = $(e.currentTarget).data("help-url");
            self.showHelp(url);
        });

        // load filter
        $(".globalsearch-load-filters").on("click", function (e) {
            pageHelper.getSearchCriteriaScreen(formName, screenName, false, function (response) { return globalSearch.loadCriteriaToScreen(response); });
        })

        // save filter
        $(".globalsearch-save-filters").on("click", function (e) {
            pageHelper.getSearchCriteriaScreen(formName, screenName, true, null, function () { return globalSearch.formDataToCriteriaList(); });
        })

        // clear filter
        $(".globalsearch-search-clear").on("click", function (e) {
            const form = $("#globalSearchForm");
            form.clearSearch();
            $("#gridDataFilter").data("kendoGrid").dataSource.data([]);
            $("#gridDocFilter").data("kendoGrid").dataSource.data([]);
            $("input[name=SearchModeOption][value=b]").click();
        })

        // load default criteria
        pageHelper.loadDefaultSearchCriteria(formName, screenName, null,
            function (response) { globalSearch.loadCriteriaToScreen(response); },
            function () { self.showBasicQueryType(!self.isSearchDataChecked()); self.doInitialSearch(searchString); });
        
    }

    doInitialSearch(searchString) {
        if (searchString.length) {
            // search basic search term = searchString
            $("#BasicSearchTerm").val(searchString);

            // set advanced search terms = searchString
            $("#CascadeTerm").val(searchString);

            const dataGrid = $("#gridDataFilter").data("kendoGrid");
            const docGrid = $("#gridDocFilter").data("kendoGrid");
            this.setGridCriteriaField(dataGrid, searchString);
            this.setGridCriteriaField(docGrid, searchString);


            if (this.getSearchMode() === this.BASICSEARCH) {
                this.doBasicSearch();
            }
            else {
                this.doAdvancedSearch();
            }
        }

    }

    loadCriteriaToScreen(response) {
        const criteria = JSON.parse(response);
        if (criteria.length > 0) {

            const keyValues = {};
            for (const item of criteria) {
                keyValues[item.property] = item.value;
            }
            const form = $("#globalSearchForm");
            pageHelper.populateForm(form, keyValues);

            const dataGrid = $("#gridDataFilter").data("kendoGrid");
            if (dataGrid)
                dataGrid.dataSource.data(keyValues.gridDataFilter);
            
            const docGrid = $("#gridDocFilter").data("kendoGrid");
            if (docGrid)
                docGrid.dataSource.data(keyValues.gridDocFilter);

            this.showSearchContainer(this.getSearchMode() );
            this.showDocumentTypeCriteria(this.isSearchDocChecked());
            this.showBasicQueryType(!this.isSearchDataChecked());
        }
    }

    // for saving criteria
    formDataToCriteriaList() {
        const self = globalSearch;
        const filters = [];

        // basic search criteria
        filters.push({ property: "SearchModeOption", operator: "", value: self.getSearchMode()});                         // basic, advanced
        filters.push({ property: "BasicSearchTerm", operator: "", value: self.getBasicSearchTerm() });
        filters.push({ property: "BasicSearchMode", operator: "", value: self.getBasicSearchMode() });                      // basic azure search mode
        filters.push({ property: "BasicQueryType", operator: "", value: self.getBasicQueryType() });                        // basic azure query type

        $(".cpiGSCriteriaCheckBox").each(function () {
            const name = $(this).attr("name");
            filters.push({ property: name, operator: "", value: this.checked });              
        })

        // advanced search criteria
        filters.push({ property: "CascadeTerm", operator: "", value: self.getCascadeTerm() });
        const dataGrid = $("#gridDataFilter").data("kendoGrid");
        if (dataGrid) {
            filters.push({ property: "gridDataFilter", operator: "", value: dataGrid.dataSource.data() });
        }        
        const docGrid = $("#gridDocFilter").data("kendoGrid");
        if (docGrid) {
            filters.push({ property: "gridDocFilter", operator: "", value: docGrid.dataSource.data() });
        }        

        // client filters
        var clientCodeValue = self.getComboValue("#ClientCode_globalSearch");
        if (clientCodeValue) {
            filters.push({ property: "ClientCode_input", operator: "", value: clientCodeValue });
        }
        var clientNameValue = self.getComboValue("#ClientName_globalSearch");
        if (clientNameValue) {
            filters.push({ property: "ClientName_input", operator: "", value: clientNameValue });
        }        

        return filters;
    }

    showHelp = (url) => {
        $.get(url)
            .done((result) => {
                //clear all existing hidden popups to avoid kendo id issue
                $(".cpiContainerPopup").empty();

                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
            })
            .fail((e) => {
                pageHelper.showErrors(e.responseText);
            });
    }

    showDocumentTypeCriteria(isChecked) {
        if (isChecked) {
            $("#cpiDocParamContainer").show();
            $("#cpiDocParamContainerHeader").show();
        }
        else {
            $("#cpiDocParamContainer").hide();
            $("#cpiDocParamContainerHeader").hide();
        }
    }

    showBasicQueryType(isChecked) {
        if (isChecked) {
            $("#basicQueryTypeContainer").show();
        }
        else {
            $("#basicQueryTypeContainer").hide();
        }
    }

    // BASIC SEARCH
    initializeBasicSearch(searchWarning) {
        const self = this;
        self.systemError = searchWarning.systemError;
        self.scopeError = searchWarning.scopeError;
        self.basicSearchError = searchWarning.basicSearchError;
        self.documentError = searchWarning.documentError;

        //const simpleContainer = $("#searchBasicContainer");

        //simpleContainer.find(".cpiGlobalSearchBasic").on("click", function (e) {
        //    e.preventDefault();
        //    self.doBasicSearch();
        //});

        const btnContainer = $("#searchBtnContainer");

        btnContainer.find(".cpiGlobalSearchBasic").on("click", function (e) {
            e.preventDefault();
            self.doBasicSearch();
        });

        btnContainer.find(".cpiGlobalSearchAdvanced").on("click", function (e) {
            e.preventDefault();
            self.doAdvancedSearch(searchWarning);
        });

        // toggle - open/close screens per system
        $("#toggleScreenList").click(function () {
            const button = $(this).find("i");
            button.toggleClass("fal fa-chevron-down").toggleClass("fal fa-chevron-up");
            if (button.attr('class').includes('up')) {
                $("#cpiSysParamContainer .collapse").collapse("show");
            }
            else {
                $("#cpiSysParamContainer .collapse").collapse("hide");
            }
        });

        // check all system
        $("#chkSystemAll").change(function () {
            const isChecked = $(this).is(":checked");
            $("#cpiSysParamContainer .k-checkbox").prop("checked", isChecked);
            $("#cpiSysParamContainer .cpiSystemCheck").prop("checked", isChecked);
        });

        // system checkboxes change event
        $("#cpiSysParamContainer .cpiSystemType .k-checkbox").change(function () {
            const parentChkboxId = $(this).attr("id");
            const isChecked = $("#" + parentChkboxId).is(":checked");
            const childrenSelector = "#cpiSysParamContainer .cpiScreenType ." + parentChkboxId;       // the children has the parent's id in its class
            $(childrenSelector).prop("checked", isChecked);
        });

        // children checkbox change event
        $("#cpiSysParamContainer .cpiSystemCheck").change(function () {
            const classes = $(this).attr("class").split(" ");           // parent id is in the child checkbox's class
            const parentChkboxId = classes[classes.length - 1];
            // if child chkbox checked, check its parent
            if ($(this).is(":checked")) {
                $("#" + parentChkboxId).prop("checked", true);
            }
            // else uncheck its parent only if all children are unchecked
            else {
                let allClear = true;
                const childrenSelector = "#cpiSysParamContainer .cpiScreenType ." + parentChkboxId;       // the children has the parent's id in its class
                $(childrenSelector).each(function () {
                    allClear = allClear && !this.checked;       // faster bit-wise op
                })
                if (allClear) {
                    $("#" + parentChkboxId).prop("checked", false);
                }
            }
        });

        // check all document
        $("#chkDocumentAll").change(function () {
            const isChecked = $(this).is(":checked");
            $("#cpiDocParamContainer .cpiDocumentCheck").prop("checked", isChecked);
        });
    }

    // ADVANCED SEARCH
    initializeAdvancedSearch(searchWarning) {
        const self = this;
        self.advancedNoFilterError = searchWarning.advancedNoFilterError;
        self.advancedDataError = searchWarning.advancedDataError;
        self.advancedDocError = searchWarning.advancedDocError;
        self.advancedSQLMissingQuote = searchWarning.advancedSQLMissingQuote;
        self.advancedSQLError = searchWarning.advancedSQLError;

        const advancedContainer = $("#searchAdvancedContainer");
        const dataGrid = $("#gridDataFilter");
        const docGrid = $("#gridDocFilter");
        
        // cascade search term to criteria fields in the grid
        advancedContainer.find(".cpiCascadeTermLink").on("click", function (e) {
            const searchTerm = self.getCascadeTerm();
            if (searchTerm && searchTerm.length) {
                const tab = $("#gsCriteriaGridContainer").find(".tab-content .active").attr("id");
                if (tab) {
                    var gridData;
                    if (tab.toLowerCase().includes("data")) {
                        gridData = dataGrid.data("kendoGrid");
                    }
                    else {
                        gridData = docGrid.data("kendoGrid");
                    }
                    self.setGridCriteriaField(gridData, searchTerm);
                }
            }
        });

        if (dataGrid.length > 0)
            self.initializeCriteriaGrid(dataGrid);

        if (docGrid.length > 0)
            self.initializeCriteriaGrid(docGrid);

        //advancedContainer.find(".cpiGlobalSearchAdvanced").on("click", function (e) {
        //    e.preventDefault();
        //    self.doAdvancedSearch(searchWarning);
        //});
     
    }

    doBasicSearch() {
        // check for param errors
        const errors = this.getBasicError();
        if (errors.length) {
            cpiAlert.warning(errors);
            return;
        }

        // refresh grid
        this.refreshResults(this.BASICSEARCH);
    }

    doAdvancedSearch(searchWarning) {
        // check for param errors
        const errors = this.getAdvancedError();
        if (errors.length) {
            cpiAlert.warning(errors);
            return;
        }
        // refresh grid
        this.refreshResults();
    }

    // check basic search parameters
    getBasicError() {
        const isSelectData = this.isSearchDataChecked();
        const isSelectDoc = this.isSearchDocChecked();

        let errors = ""

        // need at least one scope
        if (!(isSelectData || isSelectDoc))
            errors = errors + this.scopeError + "<br/>";

        // need search term
        if (this.getBasicSearchTerm().length < this.SEARCH_MIN_LENGTH)
            errors = errors + this.basicSearchError + this.SEARCH_MIN_LENGTH + "<br/>";

        // need at least one system/screen
        if (!$("#cpiSysParamContainer input[type=checkbox]:checked").length)                  // system-screen selected?
            errors = errors + this.systemError + "<br/>";

        // if document search selected, need at least one document type
        if (isSelectDoc) {
            if (!$("#cpiDocParamContainer input[type=checkbox]:checked").length)                  // document selected?
                errors = errors + this.documentError + "<br/>";
        }

        return errors;
    }

    // check advanced search parameters
    getAdvancedError() {
        let errors = ""
    
        // both grid empty
        if (!this.hasCustomQuery() && !this.hasRowsInGrid("gridDataFilter") && !this.hasRowsInGrid("gridDocFilter")) {
            errors = errors + this.advancedNoFilterError + "<br/>";            
        }

        // data filter has invalid field or filter length
        if (!this.isGridFilterOk("gridDataFilter")) {
            errors = errors + this.advancedDataError + " " + this.SEARCH_MIN_LENGTH + "<br/>";
        }

        // document filter has invalid field or filter length
        if (!this.isGridFilterOk("gridDocFilter")) {
            errors = errors + this.advancedDocError + " " + this.SEARCH_MIN_LENGTH + "<br/>";
        }

        return errors;
    }

    // advanced search - check grid criteria fields
    isGridFilterOk(gridName) {
        // need at least one criteria that passes length requirement
        let isOk = true;
        const grid = $(`#${gridName}`).data("kendoGrid");
        if (grid && grid.dataSource) {
            const data = grid.dataSource.data();
            if (data.length) {
                for (let i = 0; i < data.length; i++) {
                    if (data[i].Criteria.length < this.SEARCH_MIN_LENGTH) {             // if at least one criteria's length is short, return false
                        isOk = false;
                        break;
                    }
                }
            }
        }
        return isOk;
    }

    hasRowsInGrid(gridName) {
        const grid = $(`#${gridName}`).data("kendoGrid");
        if (grid && grid.dataSource) {
            const data = grid.dataSource.data();
            if (data.length)
                return true;
        }
        return false;
    }

    hasCustomQuery() {
        if (!($(".qry-str").hasClass('d-none'))) {
            var customSQL = $("#global-search-qry-str").val();
            if (customSQL.length) {
                return true;
            }
        }
        return false;
    }

    // advanced search - cascade search term to grid
    setGridCriteriaField(grid, searchTerm) {
        if (grid && grid.dataSource) {
            const currentData = grid.dataSource.data();
            for (let i = 0; i < currentData.length; i++) {
                currentData[i]["Criteria"] = searchTerm;
            }
            grid.refresh();
        }
    }

    // advanced search - initialize criteria grid buttons
    initializeCriteriaGrid(criteriaGrid) {
        const criteriaGridData = criteriaGrid.data("kendoGrid");
        const self = this;
        // grid toolbar - clear criteria fields
        criteriaGrid.on("click", ".k-grid-ClearCriteria", function (e) {
            //e.preventDefault();
            self.setGridCriteriaField(criteriaGridData, "");
        });

        // grid toolbar - delete all rows
        criteriaGrid.on("click", ".k-grid-RemoveFilter", function (e) {
            e.preventDefault();
            const form = $(e.currentTarget).closest("form");
            const deletePrompt = form.data("removefilters-prompt");
            const title = form.data("delete-title");
            cpiConfirm.delete(title, deletePrompt, function () {
                criteriaGridData.dataSource.data([]);
                $("#global-search-qry-str").val("");
            })
        });

        // grid toolbar - load all fields
        criteriaGrid.on("click", ".k-grid-LoadAll", function (e) {
            e.preventDefault();
            const form = $(e.currentTarget).closest("form");
            const loadPrompt = form.data("load-prompt");
            const title = form.data("load-title");
            cpiConfirm.warning(title, loadPrompt, function () {
                criteriaGridData.dataSource.read({ isLoadAll: true })
            })

        });

        // grid toolbar - load default fields
        criteriaGrid.on("click", ".k-grid-LoadDefault", function (e) {
            e.preventDefault();
            const form = $(e.currentTarget).closest("form");
            const loadPrompt = form.data("load-prompt");
            const title = form.data("load-title");
            cpiConfirm.warning(title, loadPrompt, function () {
                criteriaGridData.dataSource.read()
            })
        });

        // grid toolbar - show/hide query string        
        criteriaGrid.find(".k-grid-ShowQuery").hide();
        criteriaGrid.find(".k-grid-RefreshQuery").hide();
        if (criteriaGrid && criteriaGrid[0].id == "gridDataFilter") {
            criteriaGrid.find(".k-grid-ShowQuery").show();
            criteriaGrid.on("click", ".k-grid-ShowQuery", function (e) {
                e.preventDefault();
                const form = $(e.currentTarget).closest("form");
                const showLabel = "<span class='k-icon fal fa-file-alt'></span>" + form.data("show-query");
                const hideLabel = "<span class='k-icon fal fa-file-alt'></span>" + form.data("hide-query");
                const container = $(".qry-str");
                if (container.hasClass("d-none")) {
                    criteriaGrid.find(".k-grid-RefreshQuery").show();
                    container.removeClass("d-none");
                    criteriaGrid.find(".k-grid-ShowQuery")[0].innerHTML = hideLabel;
                    self.refreshSQL();
                }
                else {
                    criteriaGrid.find(".k-grid-ShowQuery")[0].innerHTML = showLabel;
                    container.addClass("d-none");
                    criteriaGrid.find(".k-grid-RefreshQuery").hide();
                }
            });

            criteriaGrid.on("click", ".k-grid-RefreshQuery", function (e) {
                e.preventDefault();
                self.refreshSQL();
            });
        }
    }

    //refresh SQL query for Advanced filter
    refreshSQL = () => {
        const baseUrl = $("body").data("base-url");
        let url = `${baseUrl}/Shared/GlobalSearch/BuildAdvancedSearchCriteria`;

        const criteria = this.getGridDataFilters(true);
        if (criteria.length > 0) {
            $.post(url, { advancedCriteria: criteria }).done((result) => {
                if (result) {
                    $("#global-search-qry-str").val(result);
                }
            }).fail(function (error) {
                pageHelper.showErrors(error.responseText);
            });
        }
        else {
            $("#global-search-qry-str").val("");
        }
    }

    // refresh results grid
    refreshResults(searchMode = "") {
        const self = this;
        let refreshData = false;
        let refreshDoc = false;

        if (searchMode === this.BASICSEARCH) {
            refreshData = self.isSearchDataChecked();
            refreshDoc = self.isSearchDocChecked();
        } else {
            refreshData = self.hasRowsInGrid("gridDataFilter");
            refreshDoc = self.hasRowsInGrid("gridDocFilter");
        }
        if (refreshData || this.hasCustomQuery()) {
            self.showSpinner();
            $('a[href="#globalSearchDataTabContent"]').tab('show')
            self.refreshResultsGrid("globalSearchResultGridData");

        }
        if (refreshDoc) {
            self.showSpinner();
            // if data refresh ongoing, delay call to doc refresh to avoid concurrency issue
            if (refreshData) {
                setTimeout(function () { self.refreshResultsGrid("globalSearchResultGridDoc"); }, 3000);        // delay to avoid concurrency issues
            }
            else {
                $('a[href="#globalSearchDocTabContent"]').tab('show')
                self.refreshResultsGrid("globalSearchResultGridDoc");
            }
        }
    }

    refreshResultsGrid(gridName) {
        const grid = $("#" + gridName).data("kendoGrid");
        if (grid.dataSource.page() !== 1) {
            grid.dataSource.page(1);
        }
        grid.dataSource.read();
    }

    //------------------------- MISCELLANEOUS -------------------------
    getSearchMode = () => { return $("#SearchMode").find(".btn.active input")[0].value; }
    isSearchDataChecked = () => { return $("#chkSearchData").is(":checked"); }
    isSearchDocChecked = () => { return $("#chkSearchDoc").is(":checked"); }
    getBasicSearchTerm = () => { return $("#BasicSearchTerm").val(); }
    getBasicSearchMode = () => { return $('[name="BasicSearchMode"]:checked').val(); }
    getBasicQueryType = () => { return $('[name="BasicQueryType"]:checked').val(); }
    getCascadeTerm = () => { return $("#CascadeTerm").val(); }
    getComboValue = (comboName) => { return $(comboName).data("kendoComboBox").value(); }

    getSelectedSystemScreens() {
        const childrenSelector = "#cpiSysParamContainer .cpiScreenType .cpiSystemCheck";       // select all screen types
        var screens = "";
        $(childrenSelector).each(function () {
            if (this.checked) {
                const id = $(this).attr("id").replace("cpiScreen", "");
                screens = screens + "|" + id;
            }
        })
        if (screens.length) screens += "|"
        return screens;
    }

    getSelectedDocumentTypes() {
        const childrenSelector = "#cpiDocParamContainer .cpiDocumentCheck";       // select all screen types
        var docTypes = "";
        $(childrenSelector).each(function () {
            if (this.checked) {
                const id = $(this).attr("id").replace("gsCheck", "");
                docTypes = docTypes + "|" + id;
            }
        })
        if (docTypes.length) docTypes += "|"
        return docTypes;
    }

    getGridDataFilters() {
        const grid = $("#gridDataFilter").data("kendoGrid");
        const records = [];

        var order = 0
        if (grid && grid.dataSource) {
            const data = grid.dataSource.data();
            for (let i = 0; i < data.length; i++) {
                if (data[i].Criteria.length) {
                    order++;
                    records.push({ OrderEntry: order, LogicalOperator: data[i].LogicalOperator, FieldId: data[i].Field.FieldId, Criteria: data[i].Criteria, LeftParen: data[i].LeftParen, RightParen: data[i].RightParen });
                }
            }
        }
        return records;
    }

    getGridDocFilters() {
        const grid = $("#gridDocFilter").data("kendoGrid");
        const records = [];

        if (grid && grid.dataSource) {
            const data = grid.dataSource.data();
            for (let i = 0; i < data.length; i++) {
                if (data[i].Criteria.length) {
                    records.push({ FieldId: data[i].Field.FieldId, Criteria: data[i].Criteria, DocSearchMode: data[i].DocSearchMode.toLowerCase(), DocQueryType: data[i].DocQueryType.toLowerCase() });
                }
            }
        }
        return records;
    }

    //-------------------------grid criteria  -------------------------
    // get search criteria for data - called from data result grid
    getSearchCriteriaData() {
        const self = this;
        const mode = self.getSearchMode();

        // simple search param
        let screens = "";
        let basicSearchTerm = "";

        // advanced search param
        let dataGridFilters = [];

        if (mode === this.BASICSEARCH) {
            screens = self.getSelectedSystemScreens();
            basicSearchTerm = self.getBasicSearchTerm();
            dataGridFilters.push({ OrderEntry: 0, LogicalOperator: "", FieldId: 0, Criteria: "", LeftParen: "", RightParen: "" }); // dummy for controller
        }
        else {
            dataGridFilters = self.getGridDataFilters();
        } 

        const moreFilters = self.getMoreFilters();

        const criteria = { SearchMode: mode, SystemScreens: screens, DocumentTypes: "", BasicSearchTerm: basicSearchTerm, DataFilters: dataGridFilters, MoreFilters: moreFilters };

        return { globalSearchParams: JSON.stringify(criteria) };
    }

    // get search criteria for data - called from document result grid
    getSearchCriteriaDoc() {
        const self = this;
        const mode = self.getSearchMode();

        // simple searh params
        const screens = self.getSelectedSystemScreens();
        const docTypes = self.getSelectedDocumentTypes();
        let basicSearchTerm = "";
        let docSearchMode = "any";
        let docQueryType = "simple";

        // advanced search params
        let docGridFilters = [];
        if (mode === this.BASICSEARCH) {
            basicSearchTerm = self.getBasicSearchTerm();
            docSearchMode = self.getBasicSearchMode();
            if (!self.isSearchDataChecked()) {
                docQueryType = self.getBasicQueryType();
            }
            docGridFilters.push({ FieldId: 0, Criteria: "", DocSearchMode: docSearchMode, DocQueryType: docQueryType });             // dummy for controller
        }
        else {
            docGridFilters = self.getGridDocFilters();
        }
        const moreFilters = self.getMoreFilters();

        const criteria = {
            SearchMode: mode, SystemScreens: screens, DocumentTypes: docTypes, BasicSearchTerm: basicSearchTerm,
            DocSearchMode: docSearchMode, DocQueryType: docQueryType, DocFilters: docGridFilters, MoreFilters: moreFilters
        };
        return { globalSearchParams: JSON.stringify(criteria) };
    }

    getMoreFilters = () => {
        let criteria = [];
        const clientCode = this.getComboValue("#ClientCode_globalSearch");
        const clientName = this.getComboValue("#ClientName_globalSearch");
        if (clientCode.length)
            criteria.push({ FieldName: "ClientCode", FieldValue: clientCode });
        if (clientName.length)
            criteria.push({ FieldName: "ClientName", FieldValue: clientName });

        if (!($(".qry-str").hasClass('d-none'))) {
            var customSQL = $("#global-search-qry-str").val();
            if (customSQL.length) {                
                criteria.push({ FieldName: "CustomSQL", FieldValue: customSQL });
            }               
        }

        return criteria;
    }
 
    deleteFilterRow(e) {
        e.preventDefault();

        const grid = $("#" + e.delegateTarget.id).data("kendoGrid");
        const form = $(e.currentTarget).closest("form");
        const deletePrompt = grid.options.editable.confirmDelete;
        const title = form.data("delete-title");
        cpiConfirm.delete(title, deletePrompt, function () {
            grid.removeRow($(e.currentTarget).closest("tr"));
            grid.dataSource._destroyed = [];
        })

        // can't use pageHelper, there is no back-end delete
        //const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        //pageHelper.deleteGridRow(e, dataItem);        

    }

    showSearchContainer(searchMode) {
        if (searchMode === this.BASICSEARCH) {
            $("#searchBasicContainer").show();
            $("#searchAdvancedContainer").hide();
            $("#searchScopeContainer").show();
            $("#searchBtnContainer .cpiGlobalSearchBasic").show();
            $("#searchBtnContainer .cpiGlobalSearchAdvanced").hide();
        }
        else {
            $("#searchBasicContainer").hide()
            $("#searchAdvancedContainer").show()
            $("#searchScopeContainer").hide();
            $("#searchBtnContainer .cpiGlobalSearchBasic").hide();
            $("#searchBtnContainer .cpiGlobalSearchAdvanced").show();
        }
    }

    searchResultGridError = (e) => {
        cpiLoadingSpinner.hide();
        pageHelper.showErrors(e.xhr.responseText || "Error retrieving search results.");
    }

    redirectToRecord(link) {
        let url = $("body").data("base-url") + "/";
        url = url + link;
        window.open(url, '_blank');
    }

    initializeSearchDataResult(excelTitle) {
        const self = this;
        const dataGrid = $("#globalSearchResultGridData");


        // grid toolbar - clear criteria fields
        dataGrid.on("click", ".k-grid-ExportData", function (e) {
            self.exportToExcel(dataGrid, false, excelTitle, "GlobalSearch_Results_YourData.xlsx");
        });
    }

    initializeSearchDocResult(docViewerUrl, docDownloadUrl, excelTitle) {
        const self = this;

        const docGrid = $("#globalSearchResultGridDoc");
        docGrid.on("click", ".docViewerLink", (e) => {
            e.preventDefault();
            e.stopPropagation();

            const row = $(e.target).closest("tr");
            const dataItem = docGrid.data("kendoGrid").dataItem(row);
            const url = docViewerUrl + "?systemType=" + dataItem.SystemType + "&screenCode=" + dataItem.ScreenCode + "&documentType=" + dataItem.DocumentType +
                "&parentId=" + dataItem.ParentId + "&fileName=" + dataItem.FileName;

            documentPage.zoomDocument(url);
            return false;
        })

        docGrid.on("click", ".k-grid-DownloadDoc", function (e) {
            self.downloadDocs(docDownloadUrl);
        });

        docGrid.on("click", ".k-grid-ExportDoc", function (e) {
            self.exportToExcel(docGrid, true, excelTitle, "GlobalSearch_Results_Documents.xlsx");
        });
    }

    exportToExcel(grid, hasDocumentTypeColumn, excelTitle, fileName) {
        const removeHtmlTags = function (str) {
            if ((str === null) || (str === ''))
                return "";
            else
                str = str.toString();
            return str.replace(/(<([^>]+)>)/ig, '');
        }

        // below adapted from Ty's _GenSearch.cshtml JS logic
        const resultGridData = grid.data("kendoGrid");
        const ds = new kendo.data.DataSource({
            data: resultGridData.dataSource.data()
        });

        let headerCells = [{ value: "SystemName" }, { value: "ScreenName" }, { value: "FieldValues" }];
        if (hasDocumentTypeColumn)
            headerCells.push({ value: "DocumentTypeName" });

        let rows = [{ cells: headerCells }];

        // Use fetch so that you can process the data when the request is successfully completed.
        ds.fetch(function () {
            var data = this.data();
            for (let i = 0; i < data.length; i++) {

                // push single row for every record
                let dataCells = [
                    { value: removeHtmlTags(data[i].SystemName) },
                    { value: removeHtmlTags(data[i].ScreenName) },
                    { value: removeHtmlTags(data[i].FieldValues) }
                ]
                if (hasDocumentTypeColumn)
                    dataCells.push({ value: removeHtmlTags(data[i].DocumentTypeName) });
                rows.push({ cells: dataCells })
            }

            // column settings (width)
            let excelColumns = [{ autoWidth: true }, { autoWidth: true }, { autoWidth: true }];
            if (hasDocumentTypeColumn)
                excelColumns.push({ autoWidth: true });

            const workbook = new kendo.ooxml.Workbook({
                sheets: [
                    {
                        columns: excelColumns,
                        title: excelTitle,
                        rows: rows
                    }
                ]
            });
            // Save the file as an Excel file with the xlsx extension.
            //kendo.saveAs({ dataURI: workbook.toDataURL(), fileName: fileName });
            workbook.toDataURLAsync().then(function(dataURL) {
              kendo.saveAs({
                dataURI: dataURL,
                fileName: fileName
              });
            });
        });
    }

    showSpinner() {
        if (this.spinnerCount === 0) {
            cpiLoadingSpinner.show("", 1);
        }
        this.spinnerCount = this.spinnerCount + 1;
    }

    hideSpinner() {
        this.spinnerCount = this.spinnerCount - 1;
        if (this.spinnerCount < 1) {
            this.spinnerCount = 0;          // just make sure it starts at zero, not with a negative number
            cpiLoadingSpinner.hide();
        }
    }

    downloadDocs = (url) => {
        const grid = $("#globalSearchResultGridDoc").data("kendoGrid");
        const selection = grid.selectedKeyNames();
        
        if (selection.length > 0) {
            let downloadForm = $("#documentsDownload").last();
            if (downloadForm.length > 0) {
                downloadForm.remove();
            }
            $(`<form action="${url}" method="post" id="documentsDownload"><input type="hidden" name="Selection" value="${selection.join()}"/></form>`).appendTo('body').submit();
        }
    }

}


 