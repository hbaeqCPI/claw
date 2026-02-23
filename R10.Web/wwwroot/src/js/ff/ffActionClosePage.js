import ActivePage from "../activePage";

export default class FFActionClosePage extends ActivePage {

    constructor() {
        super();
        this.locale = "en";
        this.totalSpinner = "<i class='fa-spin fal fa-sync'></i>";
    }

    searchResultsInit(locale) {
        this.locale = locale;
    }

    initializeSearchResultPage(searchResultPage) {
        searchResultPage.showBreadCrumbTrail = true;
        super.initializeSearchResultPage(searchResultPage);
        this.sidebar.container.addClass("collapse-lg");

        $(this.searchResultContainer).find(".action-close").on("click", (e) => {
            this.actionCloseConfirm(e);
        });

        $(this.searchResultContainer).find(".get-report").on("click", (e) => {
            this.getReport(e);
        });

        $(this.searchResultContainer).find(".k-grid-footer").hide();

        const sentLogslink = $(this.searchResultContainer).find(".show-sent-logs");
        const sentLogsUrl = sentLogslink.data("url");

        if (sentLogsUrl) {
            sentLogslink.on("click", function () {
                pageHelper.openLink(sentLogsUrl);
            });
        }
    }

    searchResultGridDataBound = (e) => {
        const data = e.sender.dataSource.data();

        if (data.length > 0) {
            const grid = e.sender.element;

            grid.find(".view-countries").on("click", this.showCountries);
            grid.find(".view-history").on("click", this.showHistory);
            grid.find(".update-option").on("click", this.setUpdateOption);

            const rows = e.sender.tbody.children();
            for (var i = 0; i < rows.length; i++) {
                const row = rows[i];
                const rowData = data[i];

                //if (rowData.IsInstructable) {
                //    if (rowData.IsInGracePeriod)
                //        $(row).addClass("warning-grace-period");
                //}
                //else {
                //    $(row).addClass("warning-not-instructable");
                //}
            }

            //this.showPayTotal();
        }
        else
            $(this.searchResultContainer).find(".k-grid-footer").hide();
    }

    showCountries = (e) => {
        const el = $(e.target);
        const row = el.closest("tr");
        const form = row.closest("form");
        const url = form.data("show-countries-url");

        if (url) {
            cpiLoadingSpinner.show();

            const item = this.searchResultGrid.data("kendoGrid").dataItem(row);
            const title = this.popUpTitle(item);
            const data = {
                dueId: item.FFDueId,
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

    setUpdateOption(e) {
        const el = $(e.target);
        const grid = el.closest(".kendo-Grid").data("kendoGrid");
        const item = grid.dataItem(el.closest("tr"));
        const form = el.closest("form");

        const url = form.data("save-update-option-url");
        if (url) {
            const option = el.data("option");
            const value = !item[option];
            const data = {
                dueId: item.DueId,
                option: option,
                value: value,
                tStamp: item.tStamp,
                __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
            };

            cpiLoadingSpinner.show();

            $.post(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();

                    grid.dataSource.read()
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error);
                });
        }
    }

    //totalsFormatter() {
    //    return new Intl.NumberFormat(this.locale, { minimumFractionDigits: 2 });
    //}

    //showPayTotal() {
    //    const footer = $(this.searchResultContainer).find(".k-footer-template");
    //    const totalAmount = footer.find(".total-cost .annuity-cost");
    //    const formatter = this.totalsFormatter();
    //    const noTotal = formatter.format(0);

    //    footer.addClass("d-none d-lg-table-row");
    //    totalAmount.html(this.totalSpinner);

    //    const url = totalAmount.data("totals-url");
    //    if (url) {
    //        //get dueIds of pay (Y) instructions
    //        const payDueIds = Object.fromEntries(Object.entries(this.mainSearchRecordIds).filter(([k, v]) => v == "Y"));
    //        $.post(url, { payDueIds: Object.keys(payDueIds) })
    //            .done(function (result) {
    //                totalAmount.html(formatter.format(result.TotalAmount));
    //            })
    //            .fail(function (error) {
    //                console.error(error.responseText);
    //                totalAmount.html(noTotal);
    //            });
    //    }
    //}

    popUpTitle(item) {
        return `<div class="row mt-1">
                    <div class="col h4" style="padding-top:0;">${item.CaseNumber} ${item.SubCase}</div>
                </div>
                <div class="row" style="margin-top: -10px;">
                    <div class="col label">${item.Title}</div>
                </div>
                <div class="row mt-1">
                    <div class="col label">${item.CountryName}</div>
                    <div class="col label text-right">${item.CaseType}</div>
                </div>
                <div class="row mt-1">
                    <div class="col label">${item.ActionDue}</div>
                    <div class="col label text-right">${pageHelper.cpiDateFormatToDisplay(item.DueDate)}</div>
                </div>`;
    }

    actionCloseConfirm(e) {
        const el = $(e.target);
        const url = el.data("url");
        const tokenUrl = el.data("token-url");
        const username = el.data("username");
        const messageUrl = el.data("message-url");
        const title = el.data("title");
        const grid = this.searchResultGrid.data("kendoGrid");
        //const ids = this.mainSearchRecordIds;

        const send = (auth) => {
            const searchCriteria = pageHelper.gridMainSearchFilters($(this.refineSearchContainer));
            const form = $("#CloseActions");
            const token = form.find("input[name='Token']");
            token.val(auth);
            let json = pageHelper.formDataToJson(form);
            json.payLoad.Instructions = this.mainSearchRecordIds;
            json.payLoad.Filters = searchCriteria.mainSearchFilters;
            token.val("");

            cpiLoadingSpinner.show();
            pageHelper.postJson(url, json)
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    grid.dataSource.read().always(function () {
                        if (result.html) {
                            if (result.genAppCount == 0 || !result.genAppUrl)
                                cpiAlert.open({ title: title, message: result.html, noPadding: true });
                            else
                                cpiConfirm.open({
                                    title: title, message: result.html, noPadding: true,
                                    onConfirm: function () {
                                        pageHelper.openLink(result.genAppUrl);
                                    },
                                    buttons: {
                                        "action": { "class": "btn-primary", "label": result.genAppLabel, "icon": "fa fa-file-search" },
                                        "close": { "class": "btn-secondary", "label": "Close", "icon": "fa fa-times" }
                                    }
                                });
                        }
                        else
                            pageHelper.showSuccess(result);
                    });
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    grid.dataSource.read().always(function () {
                        pageHelper.showErrors(error);
                    });
                });
        };

        if (url && messageUrl) {
            cpiStatusMessage.hide();
            cpiLoadingSpinner.show();
            $.post(messageUrl)
                .done(function (result) {
                    cpiLoadingSpinner.hide();

                    cpiConfirm.confirm(title ? title : window.cpiBreadCrumbs.getTitle(), result,
                        function ()
                        {
                            if (tokenUrl) {
                                pageHelper.callWithAuthToken(tokenUrl, username, send);
                            }
                            else {
                                send();
                            }
                        },
                        {
                            "action": { "class": "btn-primary", "label": el.data("label-send"), "icon": "fa fa-share" },
                            "close": { "class": "btn-secondary", "label": el.data("label-cancel"), "icon": "fa fa-undo-alt" }
                        });
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    console.error(error.responseText);
                });
        }
    }

    getReport = (e) => {
        const el = $(e.target);
        const url = el.data("url");
        const tokenUrl = el.data("token-url");
        const username = el.data("username");
        const form = el.closest("form")
        const verificationToken = form.find("input[name=__RequestVerificationToken]").val();
        const ids = this.mainSearchRecordIds;
        const downloadName = el.data("download-name");

        if (tokenUrl) {
            pageHelper.callWithAuthToken(tokenUrl, username, function (authToken) {
                pageHelper.fetchReport(url, {
                    ids: ids,
                    token: authToken
                }, verificationToken, downloadName);
            })
        }
    }

    showHistory = (e) => {
        const el = $(e.target);
        const row = el.closest("tr");
        const form = row.closest("form");
        const url = form.data("show-history-url");

        if (url) {
            cpiLoadingSpinner.show();

            const item = this.searchResultGrid.data("kendoGrid").dataItem(row);
            const title = this.popUpTitle(item);
            const showReason = true;
            const data = {
                dueId: item.FFDueId,
                showReason: showReason,
                __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
            };

            $.post(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.open({ title: title, message: result, largeModal: showReason, noPadding: true });
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.warning(pageHelper.getErrorMessage(error));
                });
        }
    }
}