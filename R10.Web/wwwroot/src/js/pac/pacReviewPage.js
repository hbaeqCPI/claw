import SearchPage from "../searchPage";

export default class PacReviewPage extends SearchPage {

    constructor() {
        super();
    }

    initializeSearchResultPage(searchResultPage) {
        searchResultPage.showBreadCrumbTrail = true;
        super.initializeSearchResultPage(searchResultPage);
        this.sidebar.container.addClass("collapse-lg");
    }

    initializeSidebarPage(sidebarPage) {
        //$(".kendo-Grid .k-grid-toolbar").addClass("sidebar-link");
        $(".kendo-Grid .k-grid-toolbar").addClass("show");
        super.initializeSidebarPage(sidebarPage);
        this.sidebar.container.addClass("collapse-lg");
    }

    searchResultGridDataBound = (e) => {
        const data = e.sender.dataSource.data();

        if (data.length > 0) {
            const grid = e.sender.element;
            grid.find(".grid-remarks").on("click", this.remarksOnClick);
            grid.find(".grid-status").on("click", this.statusOnClick);
            grid.find(".grid-keywords").on("click", this.keywordsOnClick);
        }
    }

    popUpTitle(item) {
        return `<div class="h2"><div>${item.CaseNumber}</div></div>
                <div class="h6" style="margin-top: -10px;"><span class="pr-2">${item.ClientCode ? item.ClientCode : ''}</span><span>${item.ClientName ? item.ClientName  : ''}</span></div>
                `;
    }    

    keywordsOnClick = (e) => {
        const el = $(e.target);
        const form = el.closest("tr").closest("form");
        const url = form.data("patent-search-url");
        const grid = this.searchResultGrid.data("kendoGrid");
        const item = grid.dataItem(el.closest("tr"));        
        const linkUrl = url.replace("actualValue", item.Keywords);        
        window.open(linkUrl, "_blank");
    }

    //---------------------- clearance remarks
    remarksOnClick = (e) => {
        const el = $(e.target);
        this.showRemarksEditor(el.closest("tr"));
    }

    showRemarksEditor = (row) => {
        const form = row.closest("form");
        const url = form.data("save-remarks-url");
        const grid = this.searchResultGrid.data("kendoGrid");
        const item = grid.dataItem(row);
        const inputId = `remarks-${item.PacId}`;
        const popUpContent = `
            <div class="form-group float-label">
                <label for="${inputId}">${form.data("label-remarks")}</label>
                <textarea rows="4" class="form-control form-control-sm" id="${inputId}" ${url ? "" : "disabled"}>${item.Remarks ? item.Remarks : ""}</textarea>
            </div>`;

        if (url) {
            cpiConfirm.save(this.popUpTitle(item), popUpContent,
                function () {
                    const remarks = $(`textarea[id=${inputId}]`).val();

                    if (remarks !== item.Remarks) {
                        cpiLoadingSpinner.show();

                        const remarksIcon = $(row).find(".grid-actions.grid-remarks");
                        const data = {
                            pacId: item.PacId,
                            remarks: remarks,
                            tStamp: item.tStamp,
                            __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
                        };
                        $.post(url, data)
                            .done(function (result) {
                                cpiLoadingSpinner.hide();

                                item.Remarks = remarks;
                                item.tStamp = result.tStamp;

                                if (remarks)
                                    remarksIcon.show();
                                else
                                    remarksIcon.hide();
                            })
                            .fail((error) => {
                                cpiLoadingSpinner.hide();

                                cpiAlert.warning(pageHelper.getErrorMessage(error), function () {
                                    grid.dataSource.read();
                                });
                            });
                    }
                }
            );

            setTimeout(function () {
                $(`#${inputId}`).focus();
            }, 500);
        }
        else {
            cpiAlert.popUp(this.popUpTitle(item), popUpContent, null, true);
        }
    }

    //---------------------- context menu
    contextMenuOnOpen = (e) => {
        const menu = e.sender;
        const row = $(e.target).closest("tr");
        const item = this.searchResultGrid.data("kendoGrid").dataItem(row);
        const form = $(row).closest("form");

        var items = [];

        if (form.data("save-status-url")) {
            items.push({
                text: `<span class='fal fa-thumbs-up fa-fixed-width'></span>${form.data("label-update-recommendation")}`,
                attr: { "data-action": "status" },
                encoded: false
            });
        }

        if (form.data("save-remarks-url")) {
            if (item.Remarks) {
                items.push({
                    text: `<span class='fal fa-comment-alt-edit fa-fixed-width'></span>${form.data("label-edit-remarks")}`,
                    attr: { "data-action": "remarks" },
                    encoded: false
                });
            }
            else {
                items.push({
                    text: `<span class='fal fa-comment-alt-plus fa-fixed-width'></span>${form.data("label-add-remarks")}`,
                    attr: { "data-action": "remarks" },
                    encoded: false
                });
            }
        }

        items.push({
            text: `<span class='fal fa-calendar-check fa-fixed-width'></span>${form.data("label-view-status-history")}`,
            attr: { "data-action": "status-history" },
            encoded: false
        });


        menu.setOptions({
            dataSource: items
        });
    }

    contextMenuOnSelect = (e) => {
        const selected = e.item;
        const action = $(selected).data("action");
        const label = selected.innerText;
        const row = $(e.target).closest("tr");

        switch (action) {
            case "status":
                this.getStatusEditor(row);
                return;

            case "status-history":
                this.showStatusHistory(row);
                return;

            case "remarks":
                this.showRemarksEditor(row);
                return;
        }
    }

    //---------------------- status history
    showStatusHistory = (row) => {
        const form = row.closest("form");
        const url = form.data("get-status-history-url");

        if (url) {
            cpiLoadingSpinner.show();

            const item = this.searchResultGrid.data("kendoGrid").dataItem(row);
            const title = this.popUpTitle(item);

            const data = {
                pacId: item.PacId,
                __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
            };

            $.post(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.open({ title: title, message: result, largeModal: true, noPadding: true });
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.warning(pageHelper.getErrorMessage(error));
                });
        }
    }

    //---------------------- status selection
    statusOnClick = (e) => {
        const el = $(e.target);
        this.getStatusEditor(el.closest("tr"));
    }

    getStatusEditor = (row) => {
        const form = row.closest("form");
        const url = form.data("edit-status-url");

        if (url) {
            cpiLoadingSpinner.show();

            const item = this.searchResultGrid.data("kendoGrid").dataItem(row);
            const showStatusEditor = this.showStatusEditor;

            const data = {
                clearanceStatus: item.ClearanceStatus,
                __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
            };

            $.post(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    showStatusEditor(row, result);
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.warning(pageHelper.getErrorMessage(error));
                });
        }
    }

    showStatusEditor = (row, editor) => {
        const form = row.closest("form");
        const url = form.data("save-status-url");

        if (url) {
            const grid = this.searchResultGrid.data("kendoGrid");
            const item = grid.dataItem(row);

            cpiConfirm.save(this.popUpTitle(item), editor,
                function () {
                    const clearanceStatus = $("#ClearanceStatus_pacReviewPage").data('kendoComboBox').value();                    

                    const remarksInput = $("#statusRemarks");
                    const statusRemarks = remarksInput.val();
                    const remarksError = $('#statusRemarks-error').closest(".field-validation-error");

                    if (clearanceStatus !== item.ClearanceStatus) {
                        if (item.ClearanceStatus.toLowerCase() == "submitted" && !statusRemarks) {
                            remarksError.show();
                            remarksInput.addClass("input-validation-error");
                            remarksInput.focus();
                            throw "Remarks is required.";
                        }
                        else {
                            remarksError.hide();

                            cpiLoadingSpinner.show();

                            const statusColumn = $(row).find(".grid-status");
                            const data = {
                                pacId: item.PacId,
                                clearanceStatus: clearanceStatus,
                                remarks: statusRemarks,
                                tStamp: item.tStamp,
                                __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
                            };
                            $.post(url, data)
                                .done(function (result) {
                                    cpiLoadingSpinner.hide();

                                    statusColumn.text(clearanceStatus);
                                    item.ClearanceStatus = clearanceStatus;
                                    item.tStamp = result.tStamp;

                                    if (result && result.emailWorkflows) {
                                        pageHelper.handleEmailWorkflow(result);
                                    }
                                })
                                .fail((error) => {
                                    cpiLoadingSpinner.hide();

                                    cpiAlert.warning(pageHelper.getErrorMessage(error), function () {
                                        grid.dataSource.read();
                                    });
                                });
                        }                        
                    }
                });
        }
    }    
}