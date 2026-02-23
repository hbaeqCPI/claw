import ActivePage from "../activePage";

export default class StatusUpdatePage extends ActivePage {

    constructor() {
        super();
    }

    initializeSearchResultPage(searchResultPage) {
        searchResultPage.showBreadCrumbTrail = true;
        super.initializeSearchResultPage(searchResultPage);
        this.sidebar.container.addClass("collapse-lg");

        $(this.searchResultContainer).find(".process-updates").on("click", this.processUpdates);
    }

    searchResultGridDataBound = (e) => {
        const data = e.sender.dataSource.data();

        if (data.length > 0) {
            const listView = e.sender.element;
            const rows = listView.find(".k-listview-row");

            for (var i = 0; i < rows.length; i++) {
                const row = rows[i];
                const rowData = data[i];

                if (i % 2)
                    $(row).addClass("k-alt");

                if (rowData.ApplicationStatus < rowData.UpdateStatus)
                    $(row).addClass("status-conflict");
            }

            listView.find(".status-option").on("click", (e) => {
                this.setStatusOption(e);
            });

            listView.find(".update-status").on("click", (e) => {
                this.updateStatus(e);
            });

            listView.find(".edit-remarks").on("click", (e) => {
                this.editRemarks(e);
            });
        }
    }

    setStatusOption(e) {
        const el = $(e.target);
        const listView = el.closest(".k-listview").data("kendoListView");
        const item = listView.dataItem(el.closest(".k-listview-row"));
        const form = el.closest("form");

        const url = form.data("save-option-url");
        if (url) {
            const processFlag = !item.ProcessFlag;
            const data = {
                id: item.LogID,
                processFlag: processFlag,
                tStamp: item.tStamp,
                __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
            };

            cpiLoadingSpinner.show();

            $.post(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();

                    item.ProcessFlag = processFlag;
                    item.tStamp = result.tStamp;

                    if (processFlag) {
                        el.addClass("fa-check-square");
                        el.removeClass("fa-square");
                    }
                    else {
                        el.removeClass("fa-check-square");
                        el.addClass("fa-square");
                    }

                    //pageHelper.showSuccess(result.message);
                    pageHelper.hideErrors();
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error);
                });

        }
    }

    updateStatus(e) {
        const el = $(e.target);
        const listView = el.closest(".k-listview").data("kendoListView");
        const item = listView.dataItem(el.closest(".k-listview-row"));
        const form = el.closest("form");
        const message = item.ProcessFlag ? form.data("message-process-on") : form.data("message-process-off");

        const url = form.data("update-status-url");
        if (url) {
            cpiConfirm.open({
                title: window.cpiBreadCrumbs.getTitle(), message: kendo.format(message, item.UpdateStatus),
                onConfirm: function () {
                    const data = {
                        LogID: item.LogID,
                        DueID: item.DueID,
                        ProcessFlag: item.ProcessFlag,
                        NewStatus: item.UpdateStatus,
                        Remarks: item.Remarks,
                        tStamp: item.tStamp
                    };

                    cpiLoadingSpinner.show();

                    $.post(url, {
                        updated: data,
                        __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
                        })
                        .done(function (result) {
                            cpiLoadingSpinner.hide();
                            listView.dataSource.read().always(function () {
                                pageHelper.showSuccess(result.message);
                            });
                        })
                        .fail(function (error) {
                            cpiLoadingSpinner.hide();
                            listView.dataSource.read().always(function () {
                                pageHelper.showErrors(error);
                            });
                        });
                }
            });
        }
    }

    processUpdates = (e) => {
        const el = $(e.target);
        const listView = this.searchResultGrid.data("kendoListView");

        const url = el.data("url");
        if (url) {
            cpiConfirm.open({
                title: window.cpiBreadCrumbs.getTitle(), message: el.data("message"),
                onConfirm: function () {
                    const form = el.closest("form");
                    const pageData = listView.dataSource.data();
                    let data = [];

                    cpiLoadingSpinner.show();

                    for (var i = 0; i < pageData.length; i++) {
                        const rowData = pageData[i];
                        data.push({
                            LogID: rowData.LogID,
                            DueID: rowData.DueID,
                            ProcessFlag: rowData.ProcessFlag,
                            NewStatus: rowData.UpdateStatus,
                            Remarks: rowData.Remarks,
                            tStamp: rowData.tStamp
                        });
                    }

                    $.post(url, {
                        updated: data,
                        __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
                        })
                        .done(function (result) {
                            cpiLoadingSpinner.hide();
                            listView.dataSource.read().always(function () {
                                pageHelper.showSuccess(result.message);
                            });
                        })
                        .fail(function (error) {
                            cpiLoadingSpinner.hide();
                            listView.dataSource.read().always(function () {
                                pageHelper.showErrors(error);
                            });
                        });
                }
            });
        }
    }

    editRemarks = (e) => {
        const el = $(e.target);
        const listView = el.closest(".k-listview").data("kendoListView");
        const item = listView.dataItem(el.closest(".k-listview-row"));
        const form = el.closest("form");
        const url = form.data("save-remarks-url");
        const inputId = `remarks-${item.LogID}`;

        if (url) {
            cpiConfirm.save(form.data("label-edit-remarks"), `
            <div class="form-group float-label">
                <label for="${inputId}">${form.data("label-remarks")}</label>
                <textarea rows="4" class="form-control form-control-sm" id="${inputId}" ${url ? "" : "disabled"}>${item.Remarks}</textarea>
            </div>`,
                function () {
                    const remarks = $(`textarea[id=${inputId}]`).val();

                    if (remarks !== item.Remarks) {
                        cpiLoadingSpinner.show();

                        const data = {
                            id: item.LogID,
                            remarks: remarks,
                            tStamp: item.tStamp,
                            __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
                        };
                        $.post(url, data)
                            .done(function (result) {
                                cpiLoadingSpinner.hide();

                                item.Remarks = remarks;
                                item.tStamp = result.tStamp;

                                if (remarks) {
                                    el.removeClass("fa-comment");
                                    el.addClass("fa-comment-alt-lines");
                                }
                                else {
                                    el.removeClass("fa-comment-alt-lines");
                                    el.addClass("fa-comment");
                                }

                                pageHelper.showSuccess(result.message);
                                //pageHelper.hideErrors();
                            })
                            .fail((error) => {
                                cpiLoadingSpinner.hide();

                                cpiAlert.warning(pageHelper.getErrorMessage(error), function () {
                                    listView.dataSource.read();
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
}