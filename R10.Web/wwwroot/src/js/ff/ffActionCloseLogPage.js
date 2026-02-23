import ActivePage from "../activePage";

export default class FFActionCloseLogPage extends ActivePage {

    constructor() {
        super();
        this.initLoadDetails = false;
    }

    initializeSearchResultPage(searchResultPage) {
        searchResultPage.showBreadCrumbTrail = true;
        super.initializeSearchResultPage(searchResultPage);

        $(this.searchResultContainer).find(".no-results-hide").hide();

        $(this.searchResultContainer).find(".send-letter").on("click", (e) => {
            this.sendLetter(e);
        });

        $(this.refineSearchContainer).floatLabels();

        //attach refine search to log headers grid
        const searchCriteriaGrid = $(this.refineSearchContainer).find(`${this.searchResultContainer}-Headers`);
        searchCriteriaGrid.refineSearch(this.refineSearchContainer); 

        const clearSearchCriteriaGrid = () => {
            this.initLoadDetails = false;

            const dataSource = searchCriteriaGrid.data("kendoGrid").dataSource;
            dataSource.query({
                sort: dataSource.sort(),
                page: 1,
                pageSize: dataSource.pageSize()
            });
        }

        //refresh log headers on clear
        //delegate so would fire last, after inputs are cleared
        $("body").on("click", `${this.searchResultContainer} .search-clear`, function () {
            clearSearchCriteriaGrid();
        });
    }

    searchResultGridDataBound = (e) => {
        const data = e.sender.dataSource.data();

        if (data.length > 0) {
            const grid = e.sender.element;

            grid.find(".view-countries").on("click", this.showCountries);
            grid.find(".view-history").on("click", this.showHistory);
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

    popUpTitle(item) {
        return `<div class="row mt-1">
                    <div class="col h4" style="padding-top:0;">${item.CaseNumber} ${item.Country} ${item.SubCase}</div>
                </div>
                <div class="row mt-1">
                    <div class="col label">${item.ActionDue}</div>
                    <div class="col label text-right">${pageHelper.cpiDateFormatToDisplay(item.DueDate)}</div>
                </div>`;
    }

    getLogId = (e) => {
        const form = $(this.searchResultContainer).find("form");
        const logId = form.find("#LogId");

        return {
            logId: logId.val(),
            __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
        };
    }

    logHeaderGridDataBound = (e) => {
        //select first row on inital page load
        if (!this.initLoadDetails) {
            const grid = e.sender;

            setTimeout(function () {
                grid.select(grid.tbody.find("tr:first"));
            }, 50);
        }

        this.initLoadDetails = true;
    }

    logHeaderGridRowSelect = (e) => {
        const grid = e.sender;
        this.showDetails(grid.dataItem(grid.select()));
    }

    showDetails = (dataItem) => {
        const logId = $(this.searchResultContainer).find("#LogId");
        logId.val(dataItem.LogId);

        const logHeader = $(this.searchResultGrid).find(".log-header");
        logHeader.find(".close-date span").text(kendo.format("{0:dd-MMM-yyyy hh:mm:ss tt}", dataItem.CloseDate));
        logHeader.find(".updated-by span").text(dataItem.CreatedBy);

        this.refreshDetails();
    }

    refreshDetails = () => {
        const grid = $(this.searchResultGrid).data("kendoGrid");
        const dataSource = grid.dataSource;
        cpiLoadingSpinner.show();
        dataSource.query({
            sort: dataSource.sort(),
            page: 1,
            pageSize: dataSource.pageSize()
        }).always(function () {
            cpiLoadingSpinner.hide();
        });
    }

    sendLetter(e) {
        const form = $(this.searchResultContainer).find(".page-grid form");
        const preview = form.find("#PreviewLetter").prop("checked");
        const el = $(e.target);
        const entityType = el.data("entity-type");
        const title = el.data("title");
        const message = preview ? el.data("preview-message") : el.data("message");
        const logId = form.find("#LogId").val();
        const url = form.data("send-letter-url");
        const previewUrl = form.data("preview-letter-url");
        const tokenUrl = form.data("token-url");
        const username = form.data("username");
        const verificationToken = form.find("input[name=__RequestVerificationToken]").val();
        const downloadName = el.data("download-name");

        const sendLetter = (accessToken) => {
            cpiLoadingSpinner.show();
            const criteria = {
                LogId: logId,
                entityType: entityType,
                token: accessToken
            }
            $.post(url, {
                criteria: criteria,
                __RequestVerificationToken: verificationToken
            })
            .done((result) => {
                cpiLoadingSpinner.hide();
                cpiAlert.open({ title: title, message: result, noPadding: true, onClose: this.refreshDetails });
            })
            .fail(function (error) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(error);
            });
        };

        const getReport = function (accessToken) {
            pageHelper.fetchReport(previewUrl, {
                logId: logId,
                entityType: entityType,
                token: accessToken
            }, verificationToken, downloadName);
        };

        if (url) {
            cpiConfirm.confirm(title, message,
                function () {
                    if (tokenUrl) {
                        pageHelper.callWithAuthToken(tokenUrl, username, function (authToken) {
                            if (preview)
                                getReport(authToken);
                            else
                                sendLetter(authToken);
                        })
                    }
                },
                preview ? null :
                {
                    "action": { "class": "btn-primary", "label": form.data("label-send"), "icon": "fa fa-share" },
                    "close": { "class": "btn-secondary", "label": form.data("label-cancel"), "icon": "fa fa-undo-alt" }
                });
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