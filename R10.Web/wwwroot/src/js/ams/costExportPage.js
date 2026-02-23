import ActivePage from "../activePage";

export default class CostExportPage extends ActivePage {

    constructor() {
        super();
    }

    initializeSearchResultPage(searchResultPage) {
        searchResultPage.showBreadCrumbTrail = true;
        super.initializeSearchResultPage(searchResultPage);
        this.sidebar.container.addClass("collapse-lg");

        const page = $(searchResultPage.container);
        page.find(".export-costs").on("click", (e) => {
            this.exportCosts(e);
        });
    }

    searchResultGridDataBound = (e) => {
        const data = e.sender.dataSource.data();
        const grid = e.sender.element;

        if (data.length > 0) {
            grid.find(".export-option").on("click", (e) => {
                this.setExportOption(e);
            });
        }
    }

    setExportOption(e) {
        const el = $(e.target);
        const grid = el.closest(".kendo-Grid").data("kendoGrid");
        const item = grid.dataItem(el.closest("tr"));
        const form = el.closest("form");
        const url = form.data("save-export-option-url");
        if (url) {
            const value = !item.Exclude;
            const data = {
                dueId: item.DueID,
                exclude: value,
                tStamp: item.tStamp,
                __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
            };

            cpiLoadingSpinner.show();

            $.post(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();

                    item.Exclude = value;
                    item.tStamp = result.tStamp;

                    if (value) {
                        el.addClass("fa-check-square");
                        el.removeClass("fa-square");
                    }
                    else {
                        el.removeClass("fa-check-square");
                        el.addClass("fa-square");
                    }

                    //pageHelper.showSuccess(result.message);
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error);
                });

        }
    }

    exportCosts = (e) => {
        const el = $(e.target);
        const grid = this.searchResultGrid.data("kendoGrid");

        const url = el.data("url");
        if (url) {
            cpiConfirm.warning(window.cpiBreadCrumbs.getTitle(), el.data("confirm-message"), () => {
                    const form = el.closest("form");
                    const pageData = grid.dataSource.data();
                    let data = [];

                    cpiLoadingSpinner.show();

                    for (var i = 0; i < pageData.length; i++) {
                        const rowData = pageData[i];

                        if (!rowData.Exclude)
                            data.push({
                                DueID: rowData.DueID,
                                AppId: rowData.AppId,
                                CaseNumber: rowData.CaseNumber,
                                Country: rowData.Country,
                                SubCase: rowData.SubCase,
                                CostType: rowData.CostType,
                                InvoiceDate: rowData.InvoiceDate.toISOString(),
                                InvoiceNumber: rowData.InvoiceNumber,
                                InvoiceAmount: rowData.InvoiceAmount,
                                PayDate: rowData.PayDate.toISOString(),
                                LogID: rowData.LogID,
                                Exclude: rowData.Exclude,
                                tStamp: rowData.tStamp
                            });
                    }

                    $.post(url, {
                        costs: data,
                        __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
                    })
                        .done(function (result) {
                            cpiLoadingSpinner.hide();
                            grid.dataSource.read().always(function () {
                                pageHelper.showSuccess(result.message);
                            });
                        })
                        .fail(function (error) {
                            cpiLoadingSpinner.hide();
                            grid.dataSource.read().always(function () {
                                pageHelper.showErrors(error);
                            });
                        });
                }
            );
        }
    }
}