import ActivePage from "../activePage";

export default class RMSActionClosePage extends ActivePage {

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
            this.sendToCPiConfirm(e);
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

        const agentResplink = $(this.searchResultContainer).find(".send-agent-responsibility");
        const agentRespUrl = agentResplink.data("url");

        if (agentRespUrl) {
            agentResplink.on("click", function () {
                pageHelper.openLink(agentRespUrl);
            });
        }
    }

    searchResultGridDataBound = (e) => {
        const data = e.sender.dataSource.data();

        if (data.length > 0) {
            const grid = e.sender.element;

            grid.find(".edit-next-renewal-date").on("click", this.getNextRenewalDateEditor);
            grid.find(".edit-agent-paid-date").on("click", this.getAgentPaymentDateEditor);
            grid.find(".update-option").on("click", this.setUpdateOption);
            grid.find(".view-history").on("click", this.showHistory);

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

    getNextRenewalDateEditor = (e) => {
        const el = $(e.target);
        const row = el.closest("tr");
        const form = row.closest("form");
        const url = form.data("edit-next-renewal-date-url");
        const saveUrl = form.data("save-next-renewal-date-url");
        const title = form.data("edit-next-renewal-date-title");

        if (url) {
            cpiLoadingSpinner.show();

            const item = this.searchResultGrid.data("kendoGrid").dataItem(row);
            const token = form.find("input[name=__RequestVerificationToken]").val();
            const showNextRenewalDateEditor = this.showNextRenewalDateEditor;

            const data = {
                dueId: item.DueId,
                __RequestVerificationToken: token
            };

            $.post(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    showNextRenewalDateEditor(result, title, saveUrl, item.DueId, item.tStamp, token);
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.warning(pageHelper.getErrorMessage(error));
                });
        }
    }

    showNextRenewalDateEditor = (editor, title, saveUrl, dueId, tStamp, token) => {
        if (saveUrl) {
            const grid = this.searchResultGrid.data("kendoGrid");
            cpiConfirm.save(title, editor,
                function () {
                    const input = $("#NextRenewalDate_nextRenewalDateEditor")
                    const nextRenewalDate = input.data('kendoDatePicker').value();
                    const validation = $("#NextRenewalDate_nextRenewalDateEditor-error").closest(".field-validation-error");

                    if (validation.length > 0 && !nextRenewalDate) {
                        validation.show();
                        input.closest(".k-picker-wrap").addClass("k-is-invalid");
                        input.addClass("input-validation-error");
                        input.val("");
                        input.focus();
                        throw validation.text();
                    }

                    if (input.val() && !nextRenewalDate) {
                        input.val("");
                        input.focus();
                        throw "Invalid date.";
                    }

                    cpiLoadingSpinner.show();
                    
                    const data = {
                        dueId: dueId,
                        nextRenewalDate: pageHelper.cpiDateFormatToSave(nextRenewalDate),
                        tStamp: tStamp,
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

    getAgentPaymentDateEditor = (e) => {
        const el = $(e.target);
        const row = el.closest("tr");
        const form = row.closest("form");
        const url = form.data("edit-agent-paid-date-url");
        const saveUrl = form.data("save-agent-paid-date-url");
        const title = form.data("edit-agent-paid-date-title");

        if (url) {
            cpiLoadingSpinner.show();

            const item = this.searchResultGrid.data("kendoGrid").dataItem(row);
            const token = form.find("input[name=__RequestVerificationToken]").val();
            const showAgentPaymentDateEditor = this.showAgentPaymentDateEditor;

            const data = {
                dueId: item.DueId,
                __RequestVerificationToken: token
            };

            $.post(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    showAgentPaymentDateEditor(result, title, saveUrl, item.DueId, item.tStamp, token);
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    cpiAlert.warning(pageHelper.getErrorMessage(error));
                });
        }
    }

    showAgentPaymentDateEditor = (editor, title, saveUrl, dueId, tStamp, token) => {
        if (saveUrl) {
            const grid = this.searchResultGrid.data("kendoGrid");
            cpiConfirm.save(title, editor,
                function () {
                    const input = $("#AgentPaymentDate_agentPaymentDateEditor")
                    const agentPaymentDate = input.data('kendoDatePicker').value();
                    const validation = $("#AgentPaymentDate_agentPaymentDateEditor-error").closest(".field-validation-error");

                    if (validation.length > 0 && !agentPaymentDate) {
                        validation.show();
                        input.closest(".k-picker-wrap").addClass("k-is-invalid");
                        input.addClass("input-validation-error");
                        input.val("");
                        input.focus();
                        throw validation.text();
                    }

                    if (input.val() && !agentPaymentDate) {
                        input.val("");
                        input.focus();
                        throw "Invalid date.";
                    }

                    cpiLoadingSpinner.show();

                    const data = {
                        dueId: dueId,
                        agentPaymentDate: pageHelper.cpiDateFormatToSave(agentPaymentDate),
                        tStamp: tStamp,
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
        return `<div class="row mt-1">
                    <div class="col h4" style="padding-top:0;">${item.CaseNumber} ${item.SubCase}</div>
                </div>
                <div class="row" style="margin-top: -10px;">
                    <div class="col label">${item.TrademarkName}</div>
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
            $.post(messageUrl, { instructions: this.mainSearchRecordIds })
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
                dueId: item.RMSDueId,
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