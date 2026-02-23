import ActivePage from "../activePage";

export default class AMSRemindersPage extends ActivePage {

    constructor() {
        super();
    }

    initializeSearchResultPage(searchResultPage) {
        searchResultPage.showBreadCrumbTrail = true;
        super.initializeSearchResultPage(searchResultPage);
        
        const page = $(searchResultPage.container);
        page.find(".print-recipients").on("click", (e) => {
            this.printRecipients(e);
        });
        page.find(".send-reminder").on("click", (e) => {
            this.sendReminder(e);
        });
    }

    searchResultGridDataBound = (e) => {
        const data = e.sender.dataSource.data();
        const grid = e.sender.element;

        if (data.length > 0) {
            grid.find(".reminder-option").on("click", (e) => {
                this.setReminderOption(e);
            });

            grid.find(".portfolio-review").on("click", (e) => {
                this.openPortfolioreview(e);
            });

            const previewReport = grid.find(".preview-report");
            if (grid.closest("form").data("preview-report-url")) {
                previewReport.show();
                previewReport.on("click", (e) => {
                    this.previewReport(e);
                });
            } else {
                previewReport.hide();
            }

            const rows = e.sender.tbody.children();
            for (var i = 0; i < rows.length; i++) {
                const row = rows[i];
                const rowData = data[i];
                const previewEmail = $(row).find(".preview-email");

                if (rowData.ReceiveReminderOnline || rowData.ReceiveReminderReport)
                    previewEmail.show();
                else
                    previewEmail.hide();

                previewEmail.on("click", (e) => {
                    this.previewEmail(e);
                });
            }
        }
    }

    openPortfolioreview(e) {
        const el = $(e.target);
        const grid = el.closest(".kendo-Grid").data("kendoGrid");
        const item = grid.dataItem(el.closest("tr"));
        const form = el.closest("form");

        const url = form.data("portfolio-review-url");
        if (url) {
            const filters = [];

            filters.push({
                property: "AMSMain.CPIClient",
                operator: "",
                value: item.ClientCode
            });

            if (el.data("prepay") == "1") {
                filters.push({
                    property: "IncludeInstructed",
                    operator: "",
                    value: el.data("prepay")
                });
            }

            const data = {
                mainSearchFilters: filters,
                __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
            };

            cpiLoadingSpinner.show();

            $.post(url, data)
                .done(function (html) {
                    cpiLoadingSpinner.hide();
                    pageHelper.appendPage(html);
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error);
                });

        }
    }

    previewReport(e) {
        const el = $(e.target);
        const grid = el.closest(".kendo-Grid").data("kendoGrid");
        const item = grid.dataItem(el.closest("tr"));
        const form = el.closest("form");
        const searchResultsContainer = $(this.searchResultContainer);
        const sendReminder = searchResultsContainer.find(".send-reminder");
        const payBeforeSending = searchResultsContainer.find("input[name=PayBeforeSending]");
        const tokenUrl = sendReminder.data("token-url");
        const username = sendReminder.data("username");
        const verificationToken = form.find("input[name=__RequestVerificationToken]").val();        
        const url = form.data("preview-report-url");
        
        if (tokenUrl) {
            pageHelper.callWithAuthToken(tokenUrl, username, function (authToken) {
                pageHelper.fetchReport(url, {
                    remId: payBeforeSending.length == 0 ? 0 : -1,
                    client: item.ClientCode,
                    token: authToken
                }, verificationToken);
            })
        }
    }

    previewEmail = (e) => {
        const el = $(e.target);
        const grid = el.closest(".kendo-Grid").data("kendoGrid");
        const item = grid.dataItem(el.closest("tr"));
        const form = el.closest("form");
        const url = form.data("preview-email-url");
        const sendUrl = form.data("send-email-url");
        const sendButton = form.find("a.send-reminder");
        const tokenUrl = sendButton.data("token-url");
        const username = sendButton.data("username");

        const send = (data) => {
            cpiLoadingSpinner.show();

            $.post(sendUrl,
                {
                    message: data.payLoad,
                    authToken: data.authToken,
                    __RequestVerificationToken: data.verificationToken
                })
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    grid.dataSource.read().then(function () {
                        pageHelper.showSuccess(result.message);
                    });
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    grid.dataSource.read().then(function () {
                        pageHelper.showErrors(error);
                    });
                });
        };

        if (url) {
            cpiStatusMessage.hide();
            cpiLoadingSpinner.show();

            $.post(url, { code: item.ClientCode, contactId: item.ContactID })
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    cpiConfirm.confirm(window.cpiBreadCrumbs.getTitle(), result, function () {
                        const data = pageHelper.formDataToJson($(".reminder-preview").find("form.editor"));
                        if (tokenUrl) {
                            pageHelper.callWithAuthToken(tokenUrl, username, function (authToken) {
                                data.authToken = authToken;
                                send(data);
                            });
                        }
                        else {
                            send(data);
                        }
                    },
                        {
                            "action": { "class": "btn-primary", "label": form.data("label-send"), "icon": "fa fa-share" },
                            "close": { "class": "btn-secondary", "label": form.data("label-cancel"), "icon": "fa fa-undo-alt" }
                        }, true
                    );
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error);
                });
        }
    }

    setReminderOption(e) {
        const el = $(e.target);
        const grid = el.closest(".kendo-Grid").data("kendoGrid");
        const item = grid.dataItem(el.closest("tr"));
        const form = el.closest("form");

        const url = form.data("save-reminder-option-url");
        if (url) {
            const option = el.data("option");
            const value = !item[option];
            const data = {
                clientContactID: item.ClientContactID,
                option: option,
                value: value,
                tStamp: item.tStamp,
                __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
            };

            cpiLoadingSpinner.show();

            $.post(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();

                    item[option] = value;
                    item.tStamp = result.tStamp;

                    if (value) {
                        el.addClass("fa-check-square");
                        el.removeClass("fa-square");
                    }
                    else {
                        el.removeClass("fa-check-square");
                        el.addClass("fa-square");
                    }

                    const previewEmail = el.closest("tr").find(".preview-email");
                    if (item.ReceiveReminderOnline || item.ReceiveReminderReport)
                        previewEmail.show();
                    else
                        previewEmail.hide();

                    //pageHelper.showSuccess(result.message);
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error);
                });

        }
    }

    //todo
    printRecipients = (e) => {
        const el = $(e.target);
        const searchForm = $(this.refineSearchContainer);
        const url = el.data("url");
        const tokenUrl = el.data("token-url");
        const username = el.data("username");
        const downloadName = el.data("download-name");

        if (url) {
            pageHelper.callWithAuthToken(tokenUrl, username, function (authToken) {
                const data = pageHelper.formDataToJson(searchForm);
                pageHelper.fetchReport(url, {
                    criteria: data.payLoad,
                    token: authToken
                }, data.verificationToken, downloadName);
            });
        }
    }

    sendReminder = (e) => {
        const el = $(e.target);
        const url = el.data("url");
        const tokenUrl = el.data("token-url");
        const username = el.data("username");
        const grid = this.searchResultGrid.data("kendoGrid");
        const form = $(this.refineSearchContainer);
        const isTest = window.location.href.includes('t=1');

        const send = (taskUrl, statusUrl, remId, authToken) => {
            cpiLoadingSpinner.show();
            $.post(taskUrl, { remId: remId, authToken: authToken, isTest: isTest })
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    grid.dataSource.read().then(function () {
                        pageHelper.showSuccess(result.message);
                    });
                })
                .fail(function (error) {
                    //start polling only when task fails (timeout etc)
                    getStatus(statusUrl, remId);
                })
        }

        const getStatus = (url, remId) => {
            setTimeout(() => {
                cpiLoadingSpinner.show();
                $.get(url, { remId: remId })
                    .done(function (result) {
                        if (result.status == 1) {
                            cpiLoadingSpinner.hide();
                            grid.dataSource.read().then(function () {
                                pageHelper.showSuccess(result.message);
                            });
                        }
                        else
                            getStatus(url, remId);
                    })
                    .fail(function (error) {
                        cpiLoadingSpinner.hide();
                        grid.dataSource.read().then(function () {
                            pageHelper.showErrors(error);
                        });
                    });
            }, 2000);
        };

        if (url) {
            cpiStatusMessage.hide();

            cpiConfirm.confirm(window.cpiBreadCrumbs.getTitle(), el.data("confirm-message"), function () {
                cpiLoadingSpinner.show();
                const data = pageHelper.gridMainSearchFilters(form);
                $.post(url, data)
                    .done(function (result) {
                        if (result.remId) {
                            if (tokenUrl)
                                pageHelper.callWithAuthToken(tokenUrl, username, function (authToken) {
                                    send(result.taskUrl, result.statusUrl, result.remId, authToken);
                                });
                            else
                                send(result.taskUrl, result.statusUrl, result.remId);
                        }
                        else {
                            cpiLoadingSpinner.hide();
                            grid.dataSource.read().then(function () {
                                pageHelper.showSuccess(result.message);
                            });
                        }
                    })
                    .fail(function (error) {
                        cpiLoadingSpinner.hide();
                        grid.dataSource.read().then(function () {
                            pageHelper.showErrors(error);
                        });
                    });
            },
                {
                    "action": { "class": "btn-primary", "label": el.data("label-send"), "icon": "fa fa-share" },
                    "close": { "class": "btn-secondary", "label": el.data("label-cancel"), "icon": "fa fa-undo-alt" }
                }
            )
        }
    }
}