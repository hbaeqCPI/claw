import ActivePage from "../activePage";

export default class InstructionsToCPiPage extends ActivePage {

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

        $(this.searchResultContainer).find(".send-to-cpi").on("click", (e) => {
            this.sendToCPiConfirm(e);
        });

        $(this.searchResultContainer).find(".get-report").on("click", (e) => {
            this.getReport(e);
        });

        $(this.searchResultContainer).find(".bulk-edit-client-payment-date").on("click", (e) => {
            this.getBulkPaymentDateEditor(e);
        }).hide();

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
        const updatePaymentDateLink = $(this.searchResultContainer).find(".bulk-edit-client-payment-date");
        updatePaymentDateLink.hide();

        if (data.length > 0) {
            const grid = e.sender.element;

            grid.find(".client-instruction-remarks").popover({
                container: '.kendo-Grid.annuities',
                trigger: 'focus'
            });
            grid.find(".not-instructable").popover({
                container: '.kendo-Grid.annuities',
                trigger: 'focus'
            });
            grid.find(".grace-period").popover({
                container: '.kendo-Grid.annuities',
                trigger: 'focus'
            });
            grid.find(".edit-client-payment-date").on("click", this.getPaymentDateEditor);

            const rows = e.sender.tbody.children();
            for (var i = 0; i < rows.length; i++) {
                const row = rows[i];
                const rowData = data[i];

                if (rowData.IsInstructable) {
                    if (rowData.IsInGracePeriod)
                        $(row).addClass("warning-grace-period");
                }
                else {
                    $(row).addClass("warning-not-instructable");
                }

                if (rowData.IsClientPaymentNeeded && !rowData.ClientPaymentDate)
                    updatePaymentDateLink.show();
            }

            this.showPayTotal();
        }
        else
            $(this.searchResultContainer).find(".k-grid-footer").hide();
    }

    totalsFormatter() {
        return new Intl.NumberFormat(this.locale, { minimumFractionDigits: 2 });
    }

    showPayTotal() {
        const footer = $(this.searchResultContainer).find(".k-footer-template");
        const totalAmount = footer.find(".total-cost .annuity-cost");
        const formatter = this.totalsFormatter();
        const noTotal = formatter.format(0);

        footer.addClass("d-none d-lg-table-row");
        totalAmount.html(this.totalSpinner);

        const url = totalAmount.data("totals-url");
        if (url) {
            //get dueIds of pay (Y) instructions
            const payDueIds = Object.fromEntries(Object.entries(this.mainSearchRecordIds).filter(([k, v]) => v == "Y"));
            $.post(url, { payDueIds: Object.keys(payDueIds) })
                .done(function (result) {
                    totalAmount.html(formatter.format(result.TotalAmount));
                })
                .fail(function (error) {
                    console.error(error.responseText);
                    totalAmount.html(noTotal);
                });
        }
    }

    getBulkPaymentDateEditor = (e) => {
        const el = $(e.target);
        const form = el.closest("form")
        const url = form.data("edit-payment-date-url");
        const saveUrl = form.data("save-payment-date-url");
        const title = form.data("edit-payment-date-title");
        const warning = form.data("edit-payment-date-warning");

        if (url) {
            const data = this.searchResultGrid.data("kendoGrid").dataSource.data();
            let dueIds = [];
            let tStamps = [];

            for (var i = 0; i < data.length; i++) {
                const rowData = data[i];

                if (rowData.IsClientPaymentNeeded && !rowData.ClientPaymentDate) {
                    dueIds.push(rowData.DueID);
                    tStamps.push(rowData.tStamp);
                }
            }

            if (dueIds.length == 0)
                cpiAlert.warning(warning);
            else {
                cpiLoadingSpinner.show();

                const token = form.find("input[name=__RequestVerificationToken]").val();
                const showPaymentDateEditor = this.showPaymentDateEditor;

                const data = {
                    dueId: 0,
                    __RequestVerificationToken: token
                };

                $.post(url, data)
                    .done(function (result) {
                        cpiLoadingSpinner.hide();
                        showPaymentDateEditor(result, title, saveUrl, dueIds, tStamps, token);
                    })
                    .fail(function (error) {
                        cpiLoadingSpinner.hide();
                        cpiAlert.warning(pageHelper.getErrorMessage(error));
                    });
            }
        }
    }

    getPaymentDateEditor = (e) => {
        const el = $(e.target);
        const row = el.closest("tr");
        const form = row.closest("form");
        const url = form.data("edit-payment-date-url");
        const saveUrl = form.data("save-payment-date-url");
        const title = form.data("edit-payment-date-title");

        if (url) {
            cpiLoadingSpinner.show();

            const item = this.searchResultGrid.data("kendoGrid").dataItem(row);
            const token = form.find("input[name=__RequestVerificationToken]").val();
            const showPaymentDateEditor = this.showPaymentDateEditor;

            const data = {
                dueId: item.DueID,
                __RequestVerificationToken: token
            };

            $.post(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    showPaymentDateEditor(result, title, saveUrl, [item.DueID], [item.tStamp], token);
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.warning(pageHelper.getErrorMessage(error));
                });
        }
    }

    showPaymentDateEditor = (editor, title, saveUrl, dueIds, tStamps, token) => {
        if (saveUrl) {
            const grid = this.searchResultGrid.data("kendoGrid");
            cpiConfirm.save(title, editor,
                function () {
                    const input = $("#ClientPaymentDate_clientPaymentDateEditor")
                    const paymentDate = input.data('kendoDatePicker').value();
                    const validation = $("#ClientPaymentDate_clientPaymentDateEditor-error").closest(".field-validation-error");

                    if (validation.length > 0 && !paymentDate) {
                        validation.show();
                        input.closest(".k-picker-wrap").addClass("k-is-invalid");
                        input.addClass("input-validation-error");
                        input.val("");
                        input.focus();
                        throw validation.text();
                    }

                    if (input.val() && !paymentDate) {
                        input.val("");
                        input.focus();
                        throw "Invalid date.";
                    }

                    cpiLoadingSpinner.show();
                    
                    const data = {
                        dueIds: dueIds,
                        paymentDate: pageHelper.cpiDateFormatToSave(paymentDate),
                        tStamps: tStamps,
                        __RequestVerificationToken: token
                    };
                    $.post(saveUrl, data)
                        .done(function (result) {
                            cpiLoadingSpinner.hide();
                            grid.dataSource.read().always(function () {
                                pageHelper.showSuccess(result.message);
                            });
                        })
                        .fail((error) => {
                            cpiLoadingSpinner.hide();
                            grid.dataSource.read().always(function () {
                                pageHelper.showErrors(error);
                            })
                        });
                });
        }
    }

    popUpTitle(item) {
        return `<div class="h2"><span class="pr-2">${item.CPIClient ? item.CPIClient : ''}</span><span>${item.CaseNumber}</span><span>${item.SubCase ? '-' + item.SubCase : ''}</span></div>
                <div class="h6" style="margin-top: -10px;"><span class="pr-2">${item.Country ? item.Country : ''}</span><span>${item.CountryName ? item.CountryName : ''}</span></div>`;
    }

    sendToCPiConfirm(e) {
        const el = $(e.target);
        const url = el.data("url");
        const tokenUrl = el.data("token-url");
        const username = el.data("username");
        const messageUrl = el.data("message-url");
        const title = el.data("title");
        const grid = this.searchResultGrid.data("kendoGrid");
        //const ids = this.mainSearchRecordIds;

        const send = (auth) => {
            const form = $("#SendInstructionsToCPi");
            const token = form.find("input[name='Token']");
            token.val(auth);
            let json = pageHelper.formDataToJson(form);
            json.payLoad.Instructions = this.mainSearchRecordIds;
            token.val("");

            cpiLoadingSpinner.show();
            pageHelper.postJson(url, json)
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    grid.dataSource.read().always(function () {
                        if (result.html) {
                            if (result.statusUpdateCount == 0 || !result.statusUpdateUrl)
                                cpiAlert.open({ title: title, message: result.html, noPadding: true });
                            else
                                cpiConfirm.open({
                                    title: title, message: result.html, noPadding: true,
                                    onConfirm: function () {
                                        pageHelper.openLink(result.statusUpdateUrl);
                                    },
                                    buttons: {
                                        "action": { "class": "btn-primary", "label": result.statusUpdateLabel, "icon": "fa fa-file-search" },
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
}