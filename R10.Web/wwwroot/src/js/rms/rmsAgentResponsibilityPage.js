import ActivePage from "../activePage";

export default class RMSAgentResponsibilityPage extends ActivePage {

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
        page.find(".send-letter").on("click", (e) => {
            this.sendLetter(e);
        });
    }

    searchResultGridDataBound = (e) => {
        const data = e.sender.dataSource.data();
        const grid = e.sender.element;

        if (data.length > 0) {
            grid.find(".send-option").on("click", (e) => {
                this.setSendOption(e);
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

                if (grid.closest("form").data("preview-email-url") && rowData.SendLetter)
                    previewEmail.show();
                else
                    previewEmail.hide();

                previewEmail.on("click", (e) => {
                    this.previewEmail(e);
                });
            }
        }
    }

    setSendOption(e) {
        const el = $(e.target);
        const grid = el.closest(".kendo-Grid").data("kendoGrid");
        const item = grid.dataItem(el.closest("tr"));
        const form = el.closest("form");

        const url = form.data("save-send-option-url");
        if (url) {
            const option = el.data("option");
            const value = !item.SendLetter;
            const data = {
                agentContactID: item.AgentContactID,
                option: option,
                value: value,
                tStamp: item.tStamp,
                __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
            };

            cpiLoadingSpinner.show();

            $.post(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();

                    item.SendLetter = value;
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
                    if (item.SendLetter)
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

    previewReport(e) {
        const el = $(e.target);
        const grid = el.closest(".kendo-Grid").data("kendoGrid");
        const item = grid.dataItem(el.closest("tr"));
        const form = el.closest("form");
        const searchResultsContainer = $(this.searchResultContainer);
        const sendLetter = searchResultsContainer.find(".send-letter");
        const tokenUrl = sendLetter.data("token-url");
        const username = sendLetter.data("username");
        const verificationToken = form.find("input[name=__RequestVerificationToken]").val();
        const url = form.data("preview-report-url");
        const searchCriteriaTab = searchResultsContainer.find("#rmsAgentResponsibilitySearchMainTabContent");

        if (tokenUrl) {
            pageHelper.callWithAuthToken(tokenUrl, username, function (authToken) {
                pageHelper.fetchReport(url, {
                    logId: 0,
                    entityCode: item.AgentCode,
                    entityType: "agent",
                    dueDateFrom: searchCriteriaTab.find("input[name=DueDateFrom]").data("kendoDatePicker").value(),
                    dueDateTo: searchCriteriaTab.find("input[name=DueDateTo]").data("kendoDatePicker").value(),
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
        const sendButton = form.find("a.send-letter");
        const tokenUrl = sendButton.data("token-url");
        const username = sendButton.data("username");

        const searchForm = $(this.refineSearchContainer);
        const dueDateFrom = searchForm.find("input[name='DueDateFrom']");
        const dueDateTo = searchForm.find("input[name='DueDateTo']");

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

        const preview = () => {
            cpiLoadingSpinner.show();

            $.post(url,
                {
                    code: item.AgentCode,
                    contactId: item.ContactID,
                    dueDateFrom: dueDateFrom.val(),
                    dueDateTo: dueDateTo.val()
                })
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    cpiConfirm.confirm(window.cpiBreadCrumbs.getTitle(), result, function () {
                        const data = pageHelper.formDataToJson($(".email-preview").find("form.editor"));
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

        if (url) {
            cpiStatusMessage.hide();
            preview();
        }
    }

    sendLetter = (e) => {
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