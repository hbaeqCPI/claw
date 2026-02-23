import SearchPage from "./searchPage";


// used by screens with both search and data entry on the same page (data query, letters)
export default class SearchEntryPage extends SearchPage {

    constructor() {
        super();

        this.detailContainer = null;
        this.detailContentContainer = null;         // this is the handler to .cpiDataContainer

        this.searchForm = null;
        this.lastGridSelection = "";
        this.selectSearchResultRow = true;
        this.isParentDirty = false;
        this.editableGrids = [];
        this.searchResultSelectedChanged = false;
        this.fromAdd = false;

        this.gridDisplayDetail = this.gridDisplayDetail.bind(this);
        this.searchGridSelectRow = this.searchGridSelectRow.bind(this);
        this.searchResultGridRequestEnd = this.searchResultGridRequestEnd.bind(this);
        this.manageDetailPageButtons = this.manageDetailPageButtons.bind(this);
        this.showDetails = this.showDetails.bind(this);
        this.moveSideBar = this.moveSideBar.bind(this);

        this.configureEditableGrids = this.configureEditableGrids.bind(this);
        this.refreshNodeDirtyFlag = this.refreshNodeDirtyFlag.bind(this);
        this.getDirtyGridPosition = this.getDirtyGridPosition.bind(this);
    }

    initializeSearchPage(searchPage) {
        this.searchContainer = searchPage.container;
        this.searchUrl = searchPage.url;
        this.searchResultGridId = searchPage.grid;
        this.searchResultGrid = $(this.searchResultGridId);
        this.searchResultContainer = searchPage.container;
        this.refineSearchContainer = searchPage.refineSearchContainer;
        this.searchForm = $(searchPage.form);

        $(searchPage.form).cpiSearchForm();
        $(searchPage.container).cpiMainButtons();

        this.initializeMainSearchTabs(searchPage.container);

        $(searchPage.container).floatLabels();
        //$(searchPage.container).moreInfo();

        //const filterCount = pageHelper.initializeSidebar(this);             // this needs: searchResultContainer, refineSearchContainer, searchResultGrid; also: searchUrl
        pageHelper.initializeSidebar(this);                                   // this needs: searchResultContainer, refineSearchContainer, searchResultGrid; also: searchUrl

        // init search result grid
        if (this.searchResultGrid.length > 0) {
            let resultsGrid = this.searchResultGrid.data("kendoGrid");

            resultsGrid.dataSource.read();
        }
        this.cpiStatusMessage.hide();

        window.cpiBreadCrumbs.addNode({
            name: $(searchPage.container).attr("id"),
            label: searchPage.title,
            url: searchPage.url,
            refresh: true,
            updateHistory: true
            //classNames: "d-none"
        });

        pageHelper.moveBreadcrumbs(searchPage.container);

    }

    initializeMainSearchTabs(searchContainer) {
        const vtabFilterCount = $(`${searchContainer} .nav-tabs-vertical`).filterCount();
        vtabFilterCount.refreshAll();

        $(`${searchContainer} .accordion-content`).liveSearch(function (el) {
            vtabFilterCount.refresh(el);
        });

        //clear filter count
        $("body").on("click", `${searchContainer} .search-clear`, function () {
            vtabFilterCount.refreshAll();
        });
    }

    manageDetailPageButtons(options) {

        const detailContainer = options.detailContainer;
        const mainControlButtons = $(detailContainer).find(".cpiButtonsDetail");            // Refresh, Add, Copy, Delete buttons
        const saveCancelButtons = $(detailContainer).find("#editActionButtons");            // Save, Cancel buttons  
        const entryForm = $(detailContainer.find("form")[0]);
        const self = this;
        const id = options.id;

        let currentRecordDetailUrl = "";

        // Refresh, Add, Copy, Delete buttons
        if (mainControlButtons.length > 0) {
            const refreshButton = $(mainControlButtons).find(".refresh-record");
            if (refreshButton.length > 0) {
                currentRecordDetailUrl = $(refreshButton).data("url");
                refreshButton.on("click", function (e) {
                    //e.preventDefault();
                    pageHelper.resetComboBoxes(self.searchForm);        // refresh search buttons to reflect refreshed data
                    self.showDetails(currentRecordDetailUrl);
                    return false;
                });
            }

            const addButton = $(mainControlButtons).find(".add-record");
            if (addButton.length > 0) {
                addButton.on("click", function (e) {
                    //e.preventDefault();
                    const button = $(this);
                    const url = button.data("url");
                    self.showDetails(url);
                    return false;                                   // cancel default event
                });
            }

            const copyButton = $(mainControlButtons).find(".copy-record");
            if (copyButton.length > 0) {
                copyButton.on("click", function (e) {
                    //e.preventDefault();
                    const button = $(this);
                    const url = button.data("url");
                    self.showDetails(url);
                    return false;                                   // cancel default event
                });
            }

            const deleteButton = $(mainControlButtons).find(".delete-record");
            if (deleteButton.length > 0) {

                const deleteConfirmation = function (title, content, url) {
                    cpiConfirm.delete(title, content, function () {

                        const form = $(".modal-message .delete-confirm");
                        if (form.length > 0) {
                            $.validator.unobtrusive.parse(form);
                            if (!form.valid()) {
                                form.wasValidated();
                                throw "Delete confirmation failed.";
                            }
                        }

                        const data = {};
                        data.id = id;
                        data.tStamp = self.getRecordStamp();
                        data.__RequestVerificationToken = $(entryForm).find("input[name=__RequestVerificationToken]").val();

                        cpiLoadingSpinner.show();
                        $.post(url, data)
                            .done(function () {
                                cpiLoadingSpinner.hide();
                                pageHelper.resetComboBoxes(self.searchForm);        // refresh search buttons to reflect deletion
                                //const msg = entryForm.data("delete-success");
                                //pageHelper.showSuccess(msg);
                                self.refreshSearchGrid();
                                
                            })
                            .fail(function (error) {
                                cpiLoadingSpinner.hide();
                                pageHelper.showErrors(error);
                            });

                    });
                };

                deleteButton.on("click", function () {
                    const button = $(this);
                    const title = entryForm.data("delete-title");
                    let content = entryForm.data("delete-message");
                    const url = button.data("url");

                    const confirmationUrl = button.data("confirm-url") || "";
                    if (confirmationUrl !== "") {
                        cpiLoadingSpinner.show();
                        $.get(confirmationUrl)
                            .done(function (result) {
                                cpiLoadingSpinner.hide();
                                content = `<div class="row message-wrap"><div class="col-2 text-center pl-md-4 pt-1"><i class="text-danger far fa-exclamation-triangle fa-2x"></i></div><div class="col-10"><p>${content}</p></div></div>${result}`;
                                deleteConfirmation(title, content, url);
                            })
                            .fail(function (e) {
                                cpiLoadingSpinner.hide();
                                pageHelper.showErrors(entryForm.data("error-message") || "An error occurred. No updates were made.");
                            });
                    }
                    else {
                        deleteConfirmation(title, content, url);
                    }
                });
            }
        }

        // Save, Cancel buttons
        if (saveCancelButtons.length > 0) {
            const cancelButton = $(saveCancelButtons).find(".cancel-changes");
            if (cancelButton.length > 0) {
                cancelButton.on("click", function (e) {
                    cpiConfirm.confirm(entryForm.data("cancel-title"), entryForm.data("cancel-message"), function () {
                        self.isParentDirty = false;
                        cpiBreadCrumbs.markLastNode({ dirty: false });

                        if (currentRecordDetailUrl !== "")
                            self.showDetails(currentRecordDetailUrl);
                        else
                            self.refreshSearchGrid();

                        cpiStatusMessage.hide();
                        self.detailContentContainer.removeClass("dirty");
                    });
                    //return false;                                   // cancel default event
                });
            }
            const saveButton = $(saveCancelButtons).find(".save-changes");
            if (saveButton.length > 0) {
                saveButton.on("click", function (e) {
                    entryForm.submit();
                });
            }
        }
    };

    getRecordStamp() {
        const tStampActualContainer = $(".content-stamp");
        return tStampActualContainer.find("#tStamp").val();
    }

    configureEditableGrids () {
        // code adapted from entryForm.js but activePage.recordNavigator removed since it is not applicable here
        const self = this;
        const refreshGridDirtyStatus = function (grid, isDirty) {

            const tab = $(`#${$(grid).closest(".tab-pane").attr("aria-labelledby")}`);
            const tabContent = $(grid).closest(".tab-pane");

            if (isDirty) {
                cpiBreadCrumbs.markLastNode({ dirty: true });
                $(tab).addClass("dirty-grid");
                $(tabContent).addClass("dirty-grid");
            }
            else {
                self.refreshNodeDirtyFlag();
                if ($(grid).closest(".tab-pane").find(".k-grid.dirty").length === 0) {
                    $(tab).removeClass("dirty-grid");
                    $(tabContent).removeClass("dirty-grid");
                }
            }
            
            //const detailContentContainer = $(self.detailContainer).find(".cpiDataContainer");
            if (self.detailContentContainer.find(".k-grid.dirty").length === 0) {
                self.detailContentContainer.removeClass("dirty-grid");
                self.detailContentContainer.disableControls(false);
            }
            else {
                self.detailContentContainer.addClass("dirty-grid");
                self.detailContentContainer.disableControls(true);
            }
        };

        this.editableGrids.forEach((el) => {
            const grid = $(`#${el.name}`);
            const afterSave = function() { refreshGridDirtyStatus(grid, false); };
            const afterCancel = function() { refreshGridDirtyStatus(grid, false); cpiStatusMessage.hide(); };
            const onDirty = function() { refreshGridDirtyStatus(grid, true); };
            pageHelper.kendoGridDirtyTracking(grid, el, afterSave, afterCancel, onDirty);
        });
    }

    refreshNodeDirtyFlag() {
        if (!this.isParentDirty) {
            const pos = this.getDirtyGridPosition();
            if (pos === -1) {
                cpiBreadCrumbs.markLastNode({ dirty: false });
            }
        }
    };

    getDirtyGridPosition() {
        const pos = this.editableGrids.findIndex(function (element) {
            return element.isDirty;
        });
        return pos;
    };

    isGridColumnEditable = (data) => {
        var readOnly = data.ReadOnly === undefined ? false : data.ReadOnly;
        return !readOnly && !this.detailContentContainer.hasClass("dirty");
    }

    deleteGridRow(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));

        const self = this;
        pageHelper.deleteGridRow(e, dataItem, function () { self.updateRecordStamps(); });
    }

    gridDisplayDetail(e) {
        if (e === null)
            return;
        const self = this;

        const confirmGridDisplay = function () {
            self.lastGridSelection = $(".search-container .k-state-selected");
            const selDataItem = e.sender.dataItem(e.sender.select());

            if (selDataItem) {
                const url = $(self.searchContainer).data("detail-url").replace("recId", selDataItem.id);
                self.showDetails(url);

                if (!self.fromAdd)
                    self.searchResultSelectedChanged = true;

                self.fromAdd = false;
            }
        };

        const cancelGridDisplay = function () {
            e.preventDefault();
            if (self.lastGridSelection === null) {
                self.lastGridSelection = $(".search-container .k-state-selected");
            }
            $(".search-container .k-state-selected").removeClass("k-state-selected");
            $(self.lastGridSelection).addClass("k-state-selected");
        }

        if (cpiBreadCrumbs.hasDirtyNode()) {
            const container = $(".search-container");
            cpiConfirm.confirm(container.data("dirty-record-title"), container.data("dirty-record-msg"), confirmGridDisplay, null, null, cancelGridDisplay);
        }
        else {
            confirmGridDisplay();
        }

    }

    showDetails(url) {
        var self = this;
        cpiLoadingSpinner.show();
        $.get(url)
            .done(function (response) {

                const dataContainer = $(".cpiDataContainer");
                if (dataContainer.length > 0) {
                    // move sidebar to main page
                    self.moveSideBar(true);
                    dataContainer.empty();
                    dataContainer.html(response);
                }
                cpiLoadingSpinner.hide();
            })
            .fail(function (error) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(error);
            });
    }

    moveSideBar (isMoveToMain) {
        if (isMoveToMain)
            $(this.searchContainer).find(".page-main").append($(dataQuery.searchContainer).find(".detail-page-nav .sidebar-link"));
        else  // move to detail
            $(this.searchContainer).find(".detail-page-nav").prepend($(this.searchContainer).find(".page-main .sidebar-link"));
    }

    // overrides the base
    searchResultGridRequestEnd = (e) => {
        const self = this;

        cpiStatusMessage.hide();

        if (e.response) {
            $(this.refineSearchContainer).find(".total-results-count").html(e.response.Total);

            if (e.response.Data.length < 1) {
                if (this.showNoRecordError) {
                    const form = $(`${this.searchContainer}-MainSearch`);
                    pageHelper.showErrors($(form).data("no-results") || $("body").data("no-results"));
                }

                // show empty detail
                const url = $(self.searchContainer).data("empty-detail-url");
                self.showDetails(url);
            }
        }
    }

    refreshSearchGrid = function () {
        const grid = this.searchResultGrid.data("kendoGrid");
        grid.dataSource.read();
        // note: auto-selection of record in kendo grid definition is specified in the DataBound event of the grid
    }

    // referenced in _...ResultsGrid.DataBound
    searchGridSelectRow(e) {
        if (this.selectSearchResultRow) {
            e.sender.select('tr:eq(0)');
            return;
        }
        this.selectSearchResultRow = true;
    }

    insertSearchGridRow(gridId, data) {
        // insert new row to grid
        const grid = $(gridId).data("kendoGrid");
        grid.dataSource.add(data);

        // select newly inserted row
        const models = grid.dataSource.data();
        const model = models[models.length - 1];
        const lastRowUid = model.uid;
        const row = grid.table.find("[data-uid=" + lastRowUid + "]");
        grid.select(row);
    };

   
}