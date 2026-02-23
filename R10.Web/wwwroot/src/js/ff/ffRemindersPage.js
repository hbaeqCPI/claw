import ActivePage from "../activePage";

export default class FFRemindersPage extends ActivePage {

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
                
                if (grid.closest("form").data("preview-email-url") && (rowData.ReceiveFFReminder || rowData.ReceiveFFReminderReport))
                    previewEmail.show();
                else
                    previewEmail.hide();

                previewEmail.on("click", (e) => {
                    this.previewEmail(e);
                });
            }
        }
    }

    openPortfolioreview = (e) => {
        const el = $(e.target);
        const grid = el.closest(".kendo-Grid").data("kendoGrid");
        const item = grid.dataItem(el.closest("tr"));
        const form = el.closest("form");

        const url = form.data("portfolio-review-url");
        if (url) {
            const filters = [];
            const criteria = pageHelper.gridMainSearchFilters($(this.refineSearchContainer));

            //include duedate criteria
            criteria.mainSearchFilters.forEach(function (filter, index) {
                switch (filter.property) {
                    case "DueDateFrom":
                    case "DueDateTo":
                        filters.push(filter);
                        break;
                }
            });

            filters.push({
                property: "Invention.Client.ClientCode",
                operator: "",
                value: item.ClientCode
            });

            const data = {
                mainSearchFilters: filters,
                __RequestVerificationToken: criteria.__RequestVerificationToken
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
        const tokenUrl = sendReminder.data("token-url");
        const username = sendReminder.data("username");
        const verificationToken = form.find("input[name=__RequestVerificationToken]").val();
        const url = form.data("preview-report-url");
        const searchCriteriaTab = searchResultsContainer.find("#ffRemindersSearchMainTabContent");

        if (tokenUrl) {
            pageHelper.callWithAuthToken(tokenUrl, username, function (authToken) {
                pageHelper.fetchReport(url, {
                    remId: 0,
                    client: item.ClientCode,
                    dueDateFrom: searchCriteriaTab.find("input[name=DueDateFrom]").data("kendoDatePicker").value(),
                    dueDateTo: searchCriteriaTab.find("input[name=DueDateTo]").data("kendoDatePicker").value(),
                    token: authToken
                }, verificationToken);
            })
        }
    }

    getInstructByDate = (instructByDate, message, buttons) => {
        return new Promise((resolve, reject) => {
            cpiConfirm.confirm(window.cpiBreadCrumbs.getTitle(), message, function () {
                //validate instruct by date
                const input = $("#InstructByDate_instructByDateEditor");
                if (input.length > 0) {
                    const dateValue = input.data('kendoDatePicker').value();
                    const validation = $("#InstructByDate_instructByDateEditor-error").closest(".field-validation-error");

                    if (validation.length > 0 && !dateValue) {
                        validation.show();
                        input.closest(".k-picker-wrap").addClass("k-is-invalid");
                        input.addClass("input-validation-error");
                        input.val("");
                        input.focus();
                        throw validation.text();
                    }

                    if (input.val() && !dateValue) {
                        input.val("");
                        input.focus();
                        throw "Invalid date.";
                    }

                    instructByDate.val(pageHelper.cpiDateFormatToSave(dateValue));
                }

                resolve();
            }, buttons)
        });
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

        const searchForm = $(this.refineSearchContainer);
        const instructByDateUrl = sendButton.data("instruct-by-date-url");
        const instructByDate = searchForm.find("input[name='InstructByDate']");
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
                    code: item.ClientCode,
                    contactId: item.ContactID,
                    instructByDate: instructByDate.val(),
                    dueDateFrom: dueDateFrom.val(),
                    dueDateTo: dueDateTo.val()
                })
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

        if (url) {
            cpiStatusMessage.hide();

            if (instructByDateUrl) {
                cpiLoadingSpinner.show();
                $.post(instructByDateUrl, {
                    instructByDate: instructByDate.val(),
                    preview: true,
                    __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
                })
                    .done((result) => {
                        cpiLoadingSpinner.hide();
                        this.getInstructByDate(instructByDate, result, {
                            "action": { "class": "btn-primary", "label": form.data("label-preview"), "icon": "fa fa-envelope-open" },
                            "close": { "class": "btn-secondary", "label": form.data("label-cancel"), "icon": "fa fa-undo-alt" }
                        }).then(() => setTimeout(preview, 500));
                    })
                    .fail((error) => {
                        cpiLoadingSpinner.hide();
                        cpiAlert.warning(pageHelper.getErrorMessage(error));
                    });
            }
            else {
                preview();
            }
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
                    if (item.ReceiveFFReminder || item.ReceiveFFReminderReport)
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
        const token = form.find("input[name='Token']");
        const instructByDate = form.find("input[name='InstructByDate']");
        const instructByDateUrl = el.data("instruct-by-date-url");

        const send = () => {
            cpiLoadingSpinner.show();
            const data = pageHelper.gridMainSearchFilters(form);
            token.val("");
            $.post(url, data)
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

        const confirm = (message) => {
            this.getInstructByDate(instructByDate, message, {
                "action": { "class": "btn-primary", "label": el.data("label-send"), "icon": "fa fa-share" },
                "close": { "class": "btn-secondary", "label": el.data("label-cancel"), "icon": "fa fa-undo-alt" }
            }).then(() => {
                if (tokenUrl) {
                    pageHelper.callWithAuthToken(tokenUrl, username, function (authToken) {
                        //set token value
                        token.val(authToken);
                        send();
                    });
                }
                else {
                    send();
                }
            });
        }

        if (url) {
            cpiStatusMessage.hide();

            if (instructByDateUrl) {
                cpiLoadingSpinner.show();
                $.post(instructByDateUrl, {
                    instructByDate: instructByDate.val(),
                    __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
                })
                    .done(function (result) {
                        cpiLoadingSpinner.hide();
                        confirm(result);
                    })
                    .fail(function (error) {
                        cpiLoadingSpinner.hide();
                        cpiAlert.warning(pageHelper.getErrorMessage(error));
                    });
            }
            else {
                confirm(el.data("confirm-message"));
            }
        }
    }

    instructByDateChange = (e) => {
        const input = $(e.sender.element);
        const dateValue = input.data('kendoDatePicker').value();
        const validation = $("#InstructByDate_instructByDateEditor-error").closest(".field-validation-error");
        const form = $(this.refineSearchContainer);
        const instructByDate = form.find("input[name='InstructByDate']");

        if (validation.length > 0 && dateValue) {
            input.closest(".k-picker-wrap").removeClass("k-is-invalid");
            input.removeClass("input-validation-error");
            validation.hide();
        }

        if (dateValue)
            instructByDate.val(pageHelper.cpiDateFormatToSave(dateValue));
    }
}