import ActivePage from "../activePage";

export default class FFGenAppPage extends ActivePage {

    constructor() {
        super();
    }

    initializeSearchResultPage(searchResultPage) {
        searchResultPage.showBreadCrumbTrail = true;
        super.initializeSearchResultPage(searchResultPage);
        
        const page = $(searchResultPage.container);
        page.find(".print-list").on("click", (e) => {
            this.printList(e);
        });
        page.find(".update-selected").on("click", (e) => {
            this.updateSelected(e);
        });
    }

    searchResultGridDataBound = (e) => {
        const data = e.sender.dataSource.data();
        const grid = e.sender.element;

        if (data.length > 0) {
            grid.find(".update-option").on("click", (e) => {
                this.setUpdateOption(e);
            });

            grid.find(".generate-application").on("click", (e) => {
                this.generateApplication(e);
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
                id: item.DueCountryId,
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

    //todo
    printList = (e) => {
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

    generateApplication = (e) => {
        const el = $(e.target);
        const grid = el.closest(".kendo-Grid").data("kendoGrid");
        const item = grid.dataItem(el.closest("tr"));
        const form = el.closest("form");
        const url = form.data("generate-application-url");
        const message = kendo.format(form.data("generate-application-message"), item.CaseNumber, item.Country, item.DesCaseType);

        if (url) {
            cpiConfirm.confirm(window.cpiBreadCrumbs.getTitle(), message, function () {
                cpiLoadingSpinner.show();

                const data = {
                    AppId: item.AppId,
                    DueId: item.DueId,
                    DueCountryId: item.DueCountryId,
                    Source: item.Source,
                    CaseNumber: item.CaseNumber,
                    Country: item.Country,
                    DesCaseType: item.DesCaseType,
                    GenApp: item.GenApp,
                    Exclude: item.Exclude,
                    tStamp: item.tStamp
                };

                $.post(url, {
                    updated: data,
                    __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
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
            });
        }
    }

    updateSelected = (e) => {
        const el = $(e.target);
        const url = el.data("url");
        const grid = this.searchResultGrid.data("kendoGrid");
        const form = $(this.refineSearchContainer);
        const pageData = grid.dataSource.data();
        let data = [];

        for (var i = 0; i < pageData.length; i++) {
            const item = pageData[i];
            if (!item.Exclude)
                data.push({
                    AppId: item.AppId,
                    DueId: item.DueId,
                    DueCountryId: item.DueCountryId,
                    Source: item.Source,
                    CaseNumber: item.CaseNumber,
                    Country: item.Country,
                    DesCaseType: item.DesCaseType,
                    GenApp: item.GenApp,
                    Exclude: item.Exclude,
                    tStamp: item.tStamp
                });
        }

        if (data.length == 0) {
            cpiAlert.warning(el.data("no-data-message"));
            return;
        }

        const send = () => {
            cpiLoadingSpinner.show();

            $.post(url, {
                updated: data,
                __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
                })
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    grid.dataSource.read().then(function () {
                        if (result.html) {
                            cpiAlert.open({ title: window.cpiBreadCrumbs.getTitle(), message: result.html, noPadding: true });
                        }
                        else
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
            cpiConfirm.confirm(window.cpiBreadCrumbs.getTitle(), message, function () {
                send();
            },
                {
                    "action": { "class": "btn-primary", "label": el.data("label-generate"), "icon": "fal fa-bolt" },
                    "close": { "class": "btn-secondary", "label": el.data("label-cancel"), "icon": "fa fa-undo-alt" }
                }
            )
        }

        if (url) {
            cpiStatusMessage.hide();
            confirm(el.data("confirm-message"));
        }
    }
}