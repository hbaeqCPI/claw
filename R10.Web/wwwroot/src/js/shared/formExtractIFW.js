export default class FormExtractIFW {

    constructor() {
        this.mainContainerId = "#formExtract";
        this.searchFormId = this.mainContainerId + "-RefineSearch";
        this.searchGridId = this.mainContainerId + "-SearchGrid";
        this.detailContainerId = "#ifwDetailContainer";

        this.extractGridId = "#gridFormData";
        this.gridIFWRemarksId = "#gridIFWRemarks";

        this.searchButtonId = "#formExtractSearch";

        this.formTypeComboId = "#FormIFWDocType_FormIFWFormType_FormType_formExtract";
        this.docDescComboId = "#Description_formExtract";

        this.showNoRecordError = true;
        this.detailContainer = null;
        this.extractGrid = null;

        this.showOutstandingPMSActionsOnly = true;
        this.actionDaysRange = null;

        this.actionLoaded = false;
        this.actLoaded = false;
        this.otherTabLoaded = false;
        this.previewTabLoaded = false;

        this.getFormTypeCombo = this.getFormTypeCombo.bind(this);
        this.getDocDescCombo = this.getDocDescCombo.bind(this);
        this.refreshSearchGrid = this.refreshSearchGrid.bind(this);

    }

    initializePage(searchPage) {

        const self = this;

        const searchButton = $(this.searchButtonId);

        if (searchButton) {
            searchButton.click(function (e) {
                e.preventDefault();
                self.refreshSearchGrid();
            });
        }

        if (searchPage.formType.length) {
            const combo = self.getFormTypeCombo();
            combo.dataSource.read();
            combo.value(searchPage.formType);
            combo.focus();

            // give time for grid instantiation, else refresh won't find the grid
            setTimeout(function () {
                self.refreshSearchGrid();
            }, 2000)
        }

    }

    refreshSearchGrid() {
        const searchGrid = $(this.searchGridId).data("kendoGrid");
        if (searchGrid !== undefined) {
            searchGrid.dataSource.read()                       // refresh search results grid
                .then(function () {
                    if (searchGrid.dataSource.data().length) {
                        searchGrid.select("tr:eq(0)");
                    }
                });
        }
    }

    getFormTypeCombo() {
        return $(this.formTypeComboId).data("kendoComboBox");
    }

    getDocDescCombo() {
        return $(this.docDescComboId).data("kendoComboBox");
    }

    // called from view _SearchResultsIFW - grid change
    loadIFWDetail = (e) => {
        if (e === null)
            return;

        const self = this;
        self.detailContainer = $(self.detailContainerId);
        const url = self.detailContainer.data("detail-url");
        const gridDataItem = self.getSearchGridDataItem();

        const param = { ifwId: gridDataItem.IFWId, formType: gridDataItem.FormType };

        $.get(url, $.param(param))
            .done((response) => {
                self.detailContainer.empty();
                self.detailContainer.html(response);
            })
            .fail(function (e) {
                pageHelper.showErrors(e);
            });
    }

    initializeActionPage(actionPage) {
        const self = this;

         // set tabchange listener for document preview
        this.actionLoaded = false;
        this.actLoaded = false;
        this.otherTabLoaded = false;
        this.previewTabLoaded = false;

        $(`#${actionPage.tabName} a`).on('click', (e) => {
            e.preventDefault();
            const tab = e.target.id;
            if (!self.previewTabLoaded && tab === actionPage.previewTab) {
                const docViewerContainer = $(`#${actionPage.docViewerContainer}`);
                self.loadPage(actionPage.previewUrl, docViewerContainer);
                self.previewTabLoaded = true;
            }
            else if (!self.actionLoaded && tab === actionPage.actionTab) {
                const actionContainer = $(`#${actionPage.actionContainer}`);
                self.loadPage(actionPage.actionUrl, actionContainer);
                self.actionLoaded = true;
            }
            else if (!self.actLoaded && tab === actionPage.actTab) {
                const actContainer = $(`#${actionPage.actContainer}`);
                self.loadPage(actionPage.actUrl, actContainer);
                self.actLoaded = true;
            }
            else if (!self.otherTabLoaded && tab === actionPage.otherInfoTab) {
                const otherInfoContainer = $(`#${actionPage.otherInfoContainer}`);
                self.loadPage(actionPage.otherInfoUrl, otherInfoContainer);
                self.otherTabLoaded = true;
            }
        });

    }

    loadPage(url, container) {
        $.get(url)
            .done((response) => {
                container.empty();
                container.html(response);
            })
            .fail(function (e) {
                pageHelper.showErrors(e);
            });
    }

    initializeExtractFormTab(extractPage) {
        this.extractGrid = $(this.extractGridId).data("kendoGrid");
        const self = this;

        $(".cpiExtractFormData").on("click", function (e) {
            const extractData = function () {
                cpiLoadingSpinner.show();
                $.ajax({
                    type: "POST",
                    url: extractPage.extractUrl,
                    header: $($(`input[name='${pageHelper.verificationTokenFormData}']`)[0]).val(),
                    success: function (result) {
                        cpiLoadingSpinner.hide();
                        self.extractGrid.dataSource.read();
                        cpiStatusMessage.success(extractPage.successMessage, 3000);
                        self.actionLoaded = false;
                        self.otherTabLoaded = false;
                    },
                    error: function (e) {
                        cpiLoadingSpinner.hide();
                        pageHelper.showErrors(e);
                    }
                });
            }
            cpiConfirm.confirm(extractPage.extractTitle, extractPage.extractMessage, function () { extractData(); });
        });

        
        const rdo = $("#aiInclude");
        rdo.change(function () {
            const data = {};
            data.ifwId = extractPage.ifwId;
            data.aiInclude = $(this).is(':checked');
            data.__RequestVerificationToken = $($("input[name=__RequestVerificationToken]")[0]).val();

            $.post(extractPage.aiIncludeUrl, data)
                .done(function (response) {
                    if (response.Error)
                        cpiStatusMessage.error(extractPage.updateIncludeFailMessage, 3000);
                    else {
                        cpiStatusMessage.success(extractPage.updateIncludeSuccessMessage, 3000);
                        self.refreshSearchGrid();
                    }
                })
                .fail(function (error) {
                    pageHelper.showErrors(error);
                });
        });
    }

    
    // Action Mapping (NEW)
    initializeActMapTab(actionPage) {
        const self = this;

        const gridPMS = $(actionPage.gridPMSActionId).data("kendoGrid");
        const rdo = $(".grid-options #showOutstandingPMSActionsOnly");
        const numbox = $("#DaysRange").data("kendoNumericTextBox");

        const refreshPMSAction = function () {
            gridPMS.dataSource.page(1);
            gridPMS.dataSource.read();
        }

        if (gridPMS) {
            if (this.actionDaysRange === null)
                this.actionDaysRange = actionPage.actionDaysRange;

            rdo.prop('checked', this.showOutstandingPMSActionsOnly);
            numbox.value(this.actionDaysRange)

            gridPMS.dataSource.read();

            rdo.change(function () {
                var outstandingOnly = $(this).is(':checked');
                self.showOutstandingPMSActionsOnly = outstandingOnly;
                gridPMS.dataSource.page(1);
                gridPMS.dataSource.read();
            });

            numbox.bind("change", function () {
                var daysRange = this.value();
                self.actionDaysRange = daysRange;
            });

            $(".cpiPMSRefreshAction").on("click", function (e) {
                refreshPMSAction();
            });
        }


        // action generation
        $(".cpiGenIFWAction").on("click", function (e) {
            const generateAction = function (data) {
                $.post(actionPage.genActionUrl, data)
                    .done(function (response) {
                        if (response.Error)
                            cpiStatusMessage.error(actionPage.genErrorMessage, 3000);
                        else {
                            refreshPMSAction();
                            const msg = actionPage.genSuccessMessage.replace("{existingActions}", response.ExistingActions).replace("{actionsAdded}", response.ActionsAdded)
                            cpiStatusMessage.success(msg, 3000);
                        }
                    })
                    .fail(function (error) {
                        pageHelper.showErrors(error);
                    });
            }

            const data = {};
            data.ifwId = actionPage.ifwId;
            data.__RequestVerificationToken = $($("input[name=__RequestVerificationToken]")[0]).val();
            cpiConfirm.confirm(actionPage.genTitle, actionPage.genMessage, function () { generateAction(data); });

        });

        // view mapping
        $(".cpiOpenActionMap").on("click", function (e) {
            let url = $("body").data("base-url") + "/Shared/FormIFWActionMap/Detail/" + actionPage.mapHdrId;
            window.open(url, '_blank');
        });

    }

    // Action Mapping (OLD)
    initializeActionMapTab(actionPage) {
        const self = this;

        const gridPMS = $(actionPage.gridPMSActionId).data("kendoGrid");
        const rdo = $(".grid-options #showOutstandingPMSActionsOnlyOld");
        const numbox = $("#DaysRangeOld").data("kendoNumericTextBox");

        const refreshPMSAction = function () {
            gridPMS.dataSource.page(1);
            gridPMS.dataSource.read();
        }

        if (gridPMS) {
            if (this.actionDaysRange === null)
                this.actionDaysRange = actionPage.actionDaysRange;

            rdo.prop('checked', this.showOutstandingPMSActionsOnly);
            numbox.value(this.actionDaysRange)

            gridPMS.dataSource.read();

            rdo.change(function () {
                var outstandingOnly = $(this).is(':checked');
                self.showOutstandingPMSActionsOnly = outstandingOnly;
                gridPMS.dataSource.page(1);
                gridPMS.dataSource.read();
            });

            numbox.bind("change", function () {
                var daysRange = this.value();
                self.actionDaysRange = daysRange;
            });

            $(".cpiPMSRefreshActionOld").on("click", function (e) {
                refreshPMSAction();
            });
        }

        
        $(".cpiGenIFWActionOld").on("click", function (e) {
            const generateAction = function (data) {
                $.post(actionPage.genActionUrl, data)
                    .done(function (response) {
                        if (response.Error)
                            cpiStatusMessage.error(actionPage.genErrorMessage, 3000);
                        else {
                            refreshPMSAction();
                            const msg = actionPage.genSuccessMessage.replace("{existingActions}", response.ExistingActions).replace("{actionsAdded}", response.ActionsAdded)
                            cpiStatusMessage.success(msg, 3000);
                        }
                    })
                    .fail(function (error) {
                        pageHelper.showErrors(error);
                    });
            }

            const gridIFW = $("#gridIFWActionsOld").data("kendoGrid");
            const selection = gridIFW.selectedKeyNames();
            if (selection.length) {
                const data = {};
                data.ifwId = actionPage.ifwId;
                data.mapIds = selection.join();
                data.__RequestVerificationToken = $($("input[name=__RequestVerificationToken]")[0]).val();

                cpiConfirm.confirm(actionPage.genTitle, actionPage.genMessage, function () { generateAction(data); });
            }

        });

    }

    initializeActionMapEditor(actionPage) {
        const modalDialog = $(actionPage.modalDialogId);
        modalDialog.modal("show");

        const input = $(".modal-body").find("*").filter(":input:visible:first");
        input.focus();

        let entryForm = modalDialog.find("form")[0];

        let isParentDirty = false;

        // attach jquery validator
        $.validator.unobtrusive.parse(entryForm);
        entryForm = $(entryForm);
        entryForm.data("validator").settings.ignore = "";               // include hidden fields (kendo controls)
        pageHelper.addMaxLength(entryForm);


        const actionMapGrid = $(actionPage.actionMapGridId).data("kendoGrid");
        const refreshActionGrid = function () {
            actionMapGrid.dataSource.page(1);
            actionMapGrid.dataSource.read();
        }

        const submitButton = $(modalDialog.find("#save"));
        const setToSaveMode = function () {
            submitButton.removeClass("d-none");
        }

        const markDirty = function () {
            if (isParentDirty)
                return;
            isParentDirty = true;
            setToSaveMode();
        };

        // attach markDirty to input fields
        entryForm.on("input", ".cpiMainEntry input, .cpiMainEntry textarea", function () {
            markDirty();
        });

        entryForm.find(".cpiMainEntry .k-combobox > input").each(function () {
            const comboBox = $(this).data("kendoComboBox");
            if (comboBox) {
                comboBox.bind("change", function () {
                    markDirty();
                });
            }
        });

        const messageForm = $("#ifwActionMapForm");
        modalDialog.on('hide.bs.modal', function (e) {
            if (isParentDirty) {
                cpiConfirm.confirm(messageForm.data("cancel-title"), messageForm.data("cancel-message"), function () { }, null, undefined,
                    function () { modalDialog.modal("show"); });    // works but hides this modal so we have to show again
            }
        });

        const submitForm = function () {
            cpiLoadingSpinner.show();

            var form = modalDialog.find("form");
            var formData = new FormData(form[0]);
            $.ajax({
                type: "POST",
                url: form.attr("action"),
                data: formData,
                contentType: false, // needed for file upload
                processData: false,
                success: (result) => {
                    refreshActionGrid();         // refresh the grid
                    isParentDirty = false;
                    modalDialog.modal("hide");
                    cpiLoadingSpinner.hide();
                    pageHelper.showSuccess(messageForm.data("save-message"));
                },
                error: function (e) {
                    pageHelper.showErrors(e);
                    isParentDirty = false;
                    modalDialog.modal("hide");
                    cpiLoadingSpinner.hide();
                }
            });

        };

        entryForm.on("submit", function (e) {
            e.preventDefault();

            //client side validation (using jquery validation)
            if (entryForm.valid()) {
                submitForm();
            }
            else {
                cpiLoadingSpinner.hide();
                entryForm.wasValidated();
            }
        });
    }

    deleteGridRow = (e, grid) => {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        pageHelper.deleteGridRow(e, dataItem );
    }

    addActionMap = () => {
        const grid = $("#gridIFWActionsOld").data("kendoGrid");
        const url = grid.dataSource.transport.options.create.url;

        this.openDialog(url);
    }

    editActionMap(e) {
        const grid = $("#gridIFWActionsOld").data("kendoGrid");
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));

        let url = grid.dataSource.transport.options.update.url;
        url = url + "?mapId=" + dataItem.MapId;

        this.openDialog(url);
    }

    initializeOtherInfoTab(otherPage) {
        const self = this;
        $(".cpiPreviewRemarks").on("click", function (e) {
            self.refreshAppRemarks(otherPage.htmlDelim);
        });

        $(".cpiUpdateRemarks").on("click", function (e) {
            const updateRemarks = function (remarks) {
                const data = { appId: otherPage.appId, newRemarks : remarks }
                $.post(otherPage.updateRemarksUrl, data)
                    .done(function (response) {
                        cpiStatusMessage.success(otherPage.successMessage, 3000);
                        $("#AppRemarks").val(response.Remarks); // refresh old remarks

                        // un-check remarks update checkbox
                        if ($("#gridIFWRemarks tbody input:checkbox").prop("checked")) {
                            $("#gridIFWRemarks tbody input:checkbox").trigger("click");
                        }
                        setTimeout(function () { self.refreshAppRemarks(otherPage.htmlDelim); }, 1000);
                    })
                    .fail(function (error) {
                        pageHelper.showErrors(error);
                    });
            }

            const ifwRemarks = self.computeRemarks(otherPage.dbDelim);
            const pmsRemarks = $("#AppRemarks").val();
            console.log("remarks")
            console.log(ifwRemarks)
            console.log(pmsRemarks)
            if (ifwRemarks.length && pmsRemarks.length) {
                const newRemarks = ifwRemarks + otherPage.dbDelim + pmsRemarks;
                cpiConfirm.confirm(otherPage.updateTitle, otherPage.updateMessage, function () { updateRemarks(newRemarks); });

            }
        });

        setTimeout(function () { self.refreshAppRemarks(otherPage.htmlDelim); }, 1000);
        

    }

    refreshAppRemarks = (htmlDelim) => {
        const ifwRemarks = this.computeRemarks(htmlDelim);
        const pmsRemarks = $("#AppRemarks").val();
        let newRemarks = (ifwRemarks.length ? ("<mark>" + ifwRemarks + "</mark>" + htmlDelim) : "") + pmsRemarks;
        newRemarks = newRemarks.replace(/(?:\r\n|\r|\n)/g, '<br>');
        $("#ifwNewRemarks").html(newRemarks);
    }

    computeRemarks(delim) {
        const grid = $("#gridIFWRemarks").data("kendoGrid");
        const selection = grid.selectedKeyNames().filter(n => n);       // get selected keys, but remove empty keys
        return selection.join(delim);
    }

    openDialog = (url, callback) => {
        $.get(url)
            .done((result) => {
                //clear all existing hidden popups to avoid kendo id issue
                const popupContainer = $(".site-content .popup");
                popupContainer.empty();
                popupContainer.html(result);
                if (callback)
                    callback();
            })
            .fail((e) => {
                pageHelper.showErrors(e);
            });
    }

    searchResultGridRequestEnd = (e) => {
        cpiStatusMessage.hide();

        if (e.response) {
            if (e.response.Data.length > 0) {
                $(this.searchResultContainer).find(".no-results-hide").show();
            }
            else if (this.showNoRecordError) {
                pageHelper.showErrors($("body").data("no-results"));
            }
        }
    }

    gridMainSearchFilters = (e) => {
        //kendo will pass an object if called from datasource.Data()
        const filterContainer = typeof e === "string" ? e : this.searchFormId;
        const x = $(filterContainer).serializeArray()
        console.log(x)
        return pageHelper.gridMainSearchFilters($(filterContainer));
    }

    getSearchGridDataItem = () => {
        // return search grid data item
        const searchGrid = $(this.searchGridId).data("kendoGrid");
        return searchGrid.dataItem(searchGrid.select());

    }

    redirectToFormExtract(link) {
        let url = $("body").data("base-url") + "/";
        url = url + link;
        window.open(url, '_blank');
    }

    
}