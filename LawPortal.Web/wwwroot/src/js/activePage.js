import SearchPage from "./searchPage";

export default class ActivePage extends SearchPage {

    constructor() {
        super();

        this.recordNavigator = null;
        this.infoContainer = null;
        this.detailMainButtons = null;
        this.mainDetailContainer = "";
        this.detailContentContainer = "";
        this.currentRecordId = 0;
        this.detailUrl = "";
        this.editableGrids = [];
        this.deleteForm = "";
        this.recordStampsUrl = "";
        this.recordStampsContainer = "";
        this.entryFormInstance = null;

        this.showDetails = this.showDetails.bind(this);
        this.afterInsert = this.afterInsert.bind(this);
        this.doneAdding = this.doneAdding.bind(this);
        this.afterCancelledInsert = this.afterCancelledInsert.bind(this);
        this.updateRecordStamps = this.updateRecordStamps.bind(this);
        //this.searchPage = searchPage;

        //if (!this.searchPage) {
        //    this.searchPage = new SearchPage();
        //}
    }

    //if we need to support searchpage to be injected
    /* search functions */
    //initializeSearchPage(searchPage) {
    //    this.searchPage.initializeSearchPage(searchPage);
    //}

    //initializeSearchResultPage(searchResultPage) {
    //    this.searchPage.initializeSearchResultPage(searchResultPage);
    //}

    //gridMainSearchFilters = (e) => {
    //    return this.searchPage.gridMainSearchFilters(e);
    //}

    //searchResultGridRequestEnd = (e) => {
    //    this.searchPage.searchResultGridRequestEnd(e);
    //}

    //searchResultGridError = (e) => {
    //    this.searchPage.searchResultGridError(e);
    //}

    //get searchUrl() {
    //    return this.searchPage.searchUrl;
    //}
    //set searchUrl(url) {
    //    this.searchPage.searchUrl = url; 
    //}

    //get searchContainer() {
    //    return this.searchPage.searchContainer;
    //}
    //set searchContainer(searchContainer) {
    //    this.searchPage.searchContainer = searchContainer;
    //}

    //get searchResultGrid() {
    //    return this.searchPage.searchResultGrid;
    //}
    //set searchResultGrid(grid) {
    //    this.searchPage.searchResultGrid = grid;
    //}

    //get mainSearchRecordIds() {
    //    return this.searchPage.mainSearchRecordIds;
    //}
    //set mainSearchRecordIds(ids) {
    //    this.searchPage.mainSearchRecordIds = ids;
    //}

    /* detail functions */
    //initializes breadcrumbs, record navigator, record details container and urls
    initializeDetailPage(detailPage) {
        this.detailUrl = detailPage.detailUrl;
        this.searchUrl = detailPage.searchUrl;
        this.mainDetailContainer = detailPage.mainDetailContainer;
        this.detailContentContainer = detailPage.detailContentContainer;

        window.cpiBreadCrumbs.addNode({
            name: detailPage.mainDetailContainer,
            label: detailPage.title,
            url: detailPage.detailUrl,
            refresh: false,
            updateHistory: true
        });

        $(() => {
            this.initializeInfoContainer();
            this.recordNavigator = $(`#${detailPage.recordNavigatorContainer}`);

            //setup record navigator 
            if (this.recordNavigator) {
                if (detailPage.singleRecord || detailPage.recordId == 0) {
                    this.mainSearchRecordIds = []; //clear existing
                    if (detailPage.recordId > 0)
                        this.mainSearchRecordIds.push(detailPage.recordId);
                }

                this.recordNavigator.cpiRecordNavigator({
                    recordIds: this.mainSearchRecordIds,
                    currentId: detailPage.recordId,
                    navigateHandler: detailPage.recordNavigateHandler
                        ? detailPage.recordNavigateHandler
                        : this.showDetails
                });
            };
            $(this.infoContainer).on("click", ".k-grid .details-link", function (e) {
                e.preventDefault();
                const link = $(this);

                if (link.attr("target") === "_blank") {
                    window.open(link.attr("href"), "_blank");
                }
                else
                    pageHelper.openDetailsLink(link);
            });
        });

        if ($(`#${detailPage.mainDetailContainer} .page-sidebar`).length > 0)
            this.sidebar = pageHelper.initializeSidebarPage(`#${detailPage.mainDetailContainer} .page-sidebar`, `#${detailPage.mainDetailContainer} .page-main`);

        pageHelper.moveBreadcrumbs(`#${detailPage.mainDetailContainer}`);
    }

    initializeInfoContainer() {
        this.infoContainer = $("#" + this.detailContentContainer);
        const initialActiveTab = "MainInfo";
        const container = this.infoContainer.find(".tab-content");
        if (container) {
            container.data("activeTab", initialActiveTab);
        }
    }

    initializeDetailContentPage(detailContentPage) {
        const options = detailContentPage;
        this.recordStampsUrl = options.recordStampsUrl;
        this.recordStampsContainer = options.recordStampsContainer;
        this.deleteForm = options.deleteForm;
        options.activePage = this;

        pageHelper.manageDetailPage(options);

        // generic dialog handlers
        const detailContentContainer = $(`#${this.detailContentContainer}`);
        detailContentContainer.find(".open-confirm").on("click", (e) => { this.openConfirm($(e.currentTarget)); });
        detailContentContainer.find(".open-popup").on("click", (e) => { this.openPopup($(e.currentTarget)); });

        const refreshDetails = () => {
            if (options.id > 0) {
                this.showDetails(options.id)
            }
        }
        window.cpiBreadCrumbs.updateLastNode({
            name: this.mainDetailContainer,
            label: detailContentPage.title,
            url: detailContentPage.detailUrl,
            refresh: true,
            refreshHandler: refreshDetails,
            updateHistory: true
        });

        pageHelper.initializeDetailTabs(this);
        $(`#${this.detailContentContainer}`).floatLabels();
    }

    showDetails(id) {
        return pageHelper.showDetails(this, id);
    }

    cancelAddFromOtherScreen(searchUrl) {
        //window.cpiBreadCrumbs.deleteLastNode();
        //pageHelper.showSearchScreen(searchUrl);
        if (window.cpiBreadCrumbs.getNodes().length > 1)
            window.cpiBreadCrumbs.showPreviousNode();
        else
            pageHelper.openLink(searchUrl, true);

    }

    afterCancelledInsert(id) {
        kendo.destroy(this.infoContainer);
        pageHelper.getDetails(this, id);
    }

    updateRecordStamps() {
        pageHelper.updateRecordStamps(this);
    }

    deleteGridRow(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        this.addMoreKeyFields(dataItem);

        const self = this;
        pageHelper.deleteGridRow(e, dataItem, function () { self.updateRecordStamps(); });
    }

    emailGridRow(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));

        pageHelper.emailGridRow(e, dataItem, function () { });
    }

    //override to add more key fields
    addMoreKeyFields = (dataItem) => {
    }

    //disable grid editing if main form is dirty or if row is readonly
    isGridColumnEditable = (data) => {
        var readOnly = data.ReadOnly === undefined ? false : data.ReadOnly;
        return !readOnly && !$(`#${this.detailContentContainer}`).hasClass("dirty");
    }

    //add read-only class if row is readonly
    setGridReadOnlyRow = function (e) {
        var gridId = e.sender.element[0].id;
        var grid = $("#" + gridId).data("kendoGrid");
        var data = grid.dataSource.view();

        for (var i = 0; i < data.length; i++) {
            if (data[i].ReadOnly) {
                grid.tbody.find("tr[data-uid='" + data[i].uid + "']").addClass("read-only");
            }
        }
    }

    //sortable grid
    onGridReorder = function (e, url) {
        const gridId = e.sender.element[0].id;
        const grid = $("#" + gridId).data("kendoGrid");
        const dataItem = grid.dataSource.getByUid(e.item.data("uid"));
        const idField = grid.dataSource.reader.model.idField;

        const dataItems = grid.dataItems();
        const sortColumn = grid.columns.find(col => {
            if (col.template && col.template.includes("sort-handler")) {
                return true;
            }
        });
        const sortColumnName = sortColumn ? sortColumn.field : "OrderOfEntry";

        const id = dataItem[idField];
        const newIndex = dataItems[e.newIndex][sortColumnName] ? dataItems[e.newIndex][sortColumnName] : e.newIndex;

        const self = this;

        cpiLoadingSpinner.show();
        $.ajax({
            type: 'POST',
            url: url,
            dataType: 'json',
            data: {
                id: id,
                newIndex: newIndex
            },
            success: function (result) {
                cpiLoadingSpinner.hide();
                pageHelper.showSuccess(result.success);

                grid.dataSource.read();
                grid.refresh();

                self.updateRecordStamps();
            },
            error: function (e) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);

                grid.dataSource.read();
                grid.refresh();
            }
        });
    }

    onGridInventorReorder = function (e, url) {
        const gridId = e.sender.element[0].id;
        const grid = $("#" + gridId).data("kendoGrid");
        const dataItem = grid.dataSource.getByUid(e.item.data("uid"));
        const idField = grid.dataSource.reader.model.idField;

        const dataItems = grid.dataItems();
        const sortColumn = grid.columns.find(col => {
            if (col.template && col.template.includes("sort-handler")) {
                return true;
            }
        });
        const sortColumnName = sortColumn ? sortColumn.field : "OrderOfEntry";

        const id = dataItem[idField];
        const newIndex = dataItems[e.newIndex][sortColumnName] ? dataItems[e.newIndex][sortColumnName] : e.newIndex;

        const self = this;

        cpiLoadingSpinner.show();
        $.ajax({
            type: 'POST',
            url: url,
            dataType: 'json',
            data: {
                id: id,
                newIndex: newIndex
            },
            success: function (result) {
                cpiLoadingSpinner.hide();
                pageHelper.showSuccess(result.success);

                grid.dataSource.read();
                grid.refresh();

                self.updateRecordStamps();
                if (result.awardEmail) {
                    pageHelper.handleEmailWorkflow(result);
                }
            },
            error: function (e) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);

                grid.dataSource.read();
                grid.refresh();
            }
        });
    }


    isEditableGridRegistered(name) {
        const pos = this.editableGrids.findIndex(function (el) {
            return el.name === name;
        });
        return pos > -1;
    }

    addEditableGrid(grid) {
        this.editableGrids.push(grid);
    }

    //refreshes the current grid display, useful when you navigate back to search result from detail screen
    refreshSearchResultGrid() {
        const data = pageHelper.getFormCriteria();
        const grid = this.searchResultGrid.data("kendoGrid");
        grid.dataSource.read(data);
    }

    //get next step in multiple step record insert
    getNextStep(id, options) {
        var activePage = this;
        var detailUrl = activePage.detailUrl;
        activePage.currentRecordId = id;
        activePage.detailUrl = `${options.url}/${id}?step=${options.step}`;

        pageHelper.showDetails(activePage, id).then(function () {
            activePage.detailUrl = detailUrl;
        });
    }

    //finish button handler in multiple step record insert
    doneAdding(id) {
        pageHelper.afterInsert(this, id);
    }

    afterInsert(id, options) {
        //if (options.url && this.searchContainer && this.searchContainer) { //when Add was triggered from search screen's New button
        //    //new layout: temporary move breadcrumbs to search screen
        //    pageHelper.moveBreadcrumbs(this.searchContainer);

        //    if (isNaN(id))
        //        id = id.id;
        //    pageHelper.openLink(options.url.replace("recid", id), true, this.searchUrl + "/search");
        //}
        //else
        return pageHelper.afterInsert(this, id);  //refresh detail display, update the record navigator 
    }

    kendoGridDeleteRecord = (e, grid) => {
        var dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const self = this;
        pageHelper.kendoGridDeleteRecord(e, dataItem, function () { self.updateRecordStamps(); });
    }

    isDirty() {
        return $(`#${this.detailContentContainer}`).hasClass("dirty");
    }

    getElement = (name) => {
        const selector = "input[name=" + name + "]";
        const el = $("#" + this.detailContentContainer).find(selector);
        return el;
    }

    getKendoComboBox = (name) => {
        const selector = "input[name=" + name + "]";
        const el = $("#" + this.detailContentContainer).find(selector);
        let comboBox = el.data('kendoComboBox');
        if (!comboBox)
            comboBox = el.data('kendoMultiColumnComboBox');

        return comboBox;
    }

    //call using PageViewModel.BeforeSubmit when adding new record to prompt for
    //required entity filter for multiple owners, inventors, or attorneys (gm).
    requiredEntityFilter = (callback) => {
        var entityFilter = $(`#${this.detailContentContainer}`).find("#RequiredEntities");
        var formUrl = entityFilter.data("form-url");
        var errorMesage = entityFilter.data("val-required");

        if (formUrl !== undefined) {
            cpiLoadingSpinner.show();
            $.get(formUrl)
                .done(function (result) {
                    cpiLoadingSpinner.hide();

                    var content = `<form id="RequiredEntityFilterList"><div class="form-group float-label"><label class='required'>${entityFilter.data("label")}</label>${result}<span class="error-message field-validation-error" style="display: none;">${errorMesage}</span></div></form>`;

                    cpiConfirm.save(window.cpiBreadCrumbs.getTitle(), content, function () {
                        var form = $("#RequiredEntityFilterList");
                        var list = form.find("#EntityFilter").data("kendoMultiSelect");

                        if (list.value().length > 0) {
                            entityFilter.val(list.value().join());
                            callback();
                        }
                        else {
                            pageHelper.validateRequiredEntityFilterList();
                            throw errorMesage; //keep confirm dialog open
                        }
                    });
                })
                .fail(function (e) {
                    cpiLoadingSpinner.hide();
                    cpiStatusMessage.error(errorMesage);
                });
        }
        else {
            cpiStatusMessage.error("Unhandled error.");
        }
    };

    showLetterScreen(e, grid) {
        const form = $("#" + this.detailContentContainer).find("form");
        const url = form.data("duedate-letter-url");
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const ddId = dataItem.DDId;
        const actId = dataItem.ActId;

        $.ajax({
            url: url,
            data: { actId: actId, ddId: ddId },
            success: function (result) {
                //var popupContainer = $(".cpiContainerPopup").last();
                const popupContainer = $(".site-content .popup").last();

                popupContainer.html(result);
                //var dialog = $("#letterGenDialog");       // this is already on the result view's document ready code
                //dialog.modal("show");
            },
            error: function (e) {
                pageHelper.showErrors(e);
            }
        });
    }

    showDueDateDelegateScreen(e, grid) {
        const form = $("#" + this.detailContentContainer).find("form");
        const url = form.data("duedate-delegate-url");
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const ddId = dataItem.DDId;
        if (ddId === 0) return;

        const actId = dataItem.ActId;
        const caseId = dataItem.ParentId;
        cpiLoadingSpinner.show();

        $.ajax({
            url: url,
            data: { actId: actId, ddId: ddId, caseId: caseId },
            success: function (result) {
                cpiLoadingSpinner.hide();
                var popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
            },
            error: function (e) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            }
        });
    }

    showCopyScreen() {
        //const popupContainer = $(".cpiContainerPopup").last();
        const popupContainer = $(".site-content .popup").last();
        const dialogContainer = popupContainer.find(".modal");
        let entryForm = dialogContainer.find("form")[0];
        dialogContainer.modal("show");

        entryForm = $(entryForm);
        const afterSubmit = (result) => {
            const dataContainer = $(`#${this.mainDetailContainer}`).find(".cpiDataContainer");
            if (dataContainer.length > 0) {
                setTimeout(function () {
                    dataContainer.empty();
                    dataContainer.html(result);
                }, 1000);
            }
        };
        entryForm.cpiPopupEntryForm({ dialogContainer: dialogContainer, afterSubmit: afterSubmit, showSuccessMsg: false });
    }

    markDirty = () => {
        $(this.entryFormInstance).trigger('markDirty');
    }

    //Display popup modal with content from partial view
    //Target element (el) attributes:
    //  title = popup window title
    //  data-url = partial view result url
    //  data-get-* = data passed to partial view result url
    //  data-large-modal = true/false display large popup
    //Optional form element in partial view:
    //  data-on-close = optional callback function for close/cancel event
    openPopup(el) {
        const url = el.data("url");
        const title = el.attr("title") || window.cpiBreadCrumbs.getTitle();

        if (url) {
            cpiLoadingSpinner.show();
            const data = {};
            $.each(el.data(), function (name, value) {
                if (name.startsWith("get"))
                    data[name.substring(3)] = value;
            });
            $.get(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.open({
                        title: title, message: result, largeModal: el.data("large-modal"), extraLargeModal: el.data("extra-large-modal"), noPadding: el.data("no-padding"),
                        onClose: function (e) {
                            const form = $(e.currentTarget).find("form");
                            if (form && form.data("on-close")) {
                                const onClose = new Function(`return ${form.data("on-close")}`)();
                                onClose(el);
                            }
                        }
                    });
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error);
                });
        }
    }

    //Display confirm dialog with content from partial view
    //Target element (el) attributes:    
    //  title = popup window title
    //  data-url = partial view result url
    //  data-get-* = data passed to partial view result url
    //  data-large-modal = true/false display large popup
    //  data-confirm-buttons = custom buttons or one of built-in cpiConfirm buttons [defaultButtons|saveButtons|okButtons]
    //Required form element in partial view:
    //  data-url = where form data is submitted
    //  data-on-success = optional callback function for submit button
    //  data-on-close = optional callback function for close/cancel event
    openConfirm(el) {
        const url = el.data("url");
        const title = el.attr("title") || window.cpiBreadCrumbs.getTitle();

        if (url) {
            cpiLoadingSpinner.show();
            const data = {};
            $.each(el.data(), function (name, value) {
                if (name.startsWith("get"))
                    data[name.substring(3)] = value;
            });
            $.get(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();

                    let buttons = cpiConfirm.defaultButtons;
                    if (el.data("confirm-buttons"))
                        buttons = cpiConfirm[el.data("confirm-buttons")];

                    cpiConfirm.confirm(title, result,
                        function (e) {
                            const form = $(e.currentTarget).closest(".modal-content").find("form");
                            const status = form.find(".modal-status");

                            if (status.length > 0)
                                status.slideUp();

                            $.validator.unobtrusive.parse(form);
                            if (form.valid()) {
                                const keepOpen = cpiConfirm.keepOpen;
                                cpiConfirm.keepOpen = true;
                                cpiLoadingSpinner.show();
                                pageHelper.postJson(form.data("url"), pageHelper.formDataToJson(form))
                                    .done((result) => {
                                        cpiLoadingSpinner.hide();
                                        cpiConfirm.close();

                                        if (form.data("on-success")) {
                                            const onSuccess = new Function(`return ${form.data("on-success")}`)();
                                            onSuccess(result);
                                        }
                                        else if (result.message)
                                            pageHelper.showSuccess(result.message);

                                        cpiConfirm.keepOpen = keepOpen;
                                    })
                                    .fail((e => {
                                        cpiLoadingSpinner.hide();
                                        const error = pageHelper.getErrorMessage(e);
                                        if (status.length > 0) {
                                            status.find(".message").html(error);
                                            status.slideDown();
                                        }
                                        else {
                                            pageHelper.showErrors(error);
                                            cpiConfirm.close();
                                        }

                                        cpiConfirm.keepOpen = keepOpen;
                                    }));
                            }
                            else {
                                form.wasValidated();
                                throw "Form validation error.";
                            }
                        }, buttons, el.data("large-modal"),
                        function (e) {
                            const form = $(e.currentTarget).find("form");
                            if (form && form.data("on-close")) {
                                const onClose = new Function(`return ${form.data("on-close")}`)();
                                onClose(el);
                            }
                        }, el.data("no-padding"));
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error);
                });
        }
    }
}
