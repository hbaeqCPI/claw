import ActivePage from "../activePage";

export default class InstructionsToCPiLogPage extends ActivePage {

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

    getSendId = (e) => {
        const form = $(this.searchResultContainer).find("form");
        const sendId = form.find("#SendId");

        return {
            sendId: sendId.val(),
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
        const sendId = $(this.searchResultContainer).find("#SendId");
        sendId.val(dataItem.SendId);

        const logHeader = $(this.searchResultGrid).find(".log-header");
        logHeader.find(".sent-date span").text(kendo.format("{0:dd-MMM-yyyy hh:mm:ss tt}", dataItem.SendDate));
        logHeader.find(".sent-by span").text(dataItem.CreatedBy);

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
        const sendId = form.find("#SendId").val();
        const url = form.data("send-letter-url");
        const previewUrl = form.data("preview-letter-url");
        const tokenUrl = form.data("token-url");
        const username = form.data("username");
        const verificationToken = form.find("input[name=__RequestVerificationToken]").val();
        const downloadName = el.data("download-name");

        const sendLetter = (accessToken) => {
            cpiLoadingSpinner.show();
            const criteria = {
                sendId: sendId,
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
                sendId: sendId,
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
}