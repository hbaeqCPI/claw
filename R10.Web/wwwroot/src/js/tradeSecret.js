export default class TradeSecret {
    constructor() {
    }

    initialize = (detailContentPage) => {
        const detailContainer = $(`#${detailContentPage.activePage.detailContentContainer}`);

        // request trade secret access button
        const tsButton = detailContainer.find(".cpiButtonsDetail .ts-view");
        tsButton.on("click", (e) => {
            e.preventDefault();
            const el = $(e.currentTarget);
            const title = el.attr("title");
            const locator = el.data("locator");

            // validate token
            const validateRequest = (url) => {
                cpiLoadingSpinner.show();

                //get form
                $.get(url, { locator: locator })
                    .done((result) => {
                        cpiLoadingSpinner.hide();
                        if (result) {
                            cpiConfirm.warning(title, result, () => {
                                //validate form
                                const form = $("form#tsTokenValidation");
                                if (!form.valid()) {
                                    form.wasValidated();
                                    throw "Token validation error.";
                                }

                                //get token
                                const token = form.find("#token").val();
                                const url = form.attr('action')
                                if (url) {
                                    $.post(url, { locator: locator, token: token })
                                        .done(() => {
                                            this.refreshDetailContent(detailContentPage);
                                        })
                                        .fail((error) => {
                                            cpiLoadingSpinner.hide();
                                            pageHelper.showErrors(error);
                                        });
                                }
                            });
                        }
                    })
                    .fail((error) => {
                        cpiLoadingSpinner.hide();
                        pageHelper.showErrors(error);
                    });
            };

            cpiLoadingSpinner.show();
            // get request status
            $.get(el.data("status-url"), { locator: locator })
                .done((result) => {
                    cpiLoadingSpinner.hide();
                    if (result.status == 0) {
                        // confirm new request
                        cpiConfirm.warning(title, result.prompt, () => {
                            cpiLoadingSpinner.show();
                            $.get(result.url, { locator: locator })
                                .done((result) => {
                                    cpiLoadingSpinner.hide();
                                    if (!result.granted) {
                                        cpiAlert.open({
                                            title: title, message: result.message,
                                            onClose: (e) => {
                                                this.refreshDetailContent(detailContentPage);
                                            }
                                        });
                                    }
                                    else {
                                        // validate granted request
                                        validateRequest(result.url);
                                    }
                                })
                                .fail((error) => {
                                    cpiLoadingSpinner.hide();
                                    pageHelper.showErrors(error);
                                });
                        });
                    }
                    else {
                        // validate granted request
                        validateRequest(result.url);
                    }
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error);
                });
        });

        if (tsButton.hasClass("ts-granted"))
            tsButton.trigger("click");

        this.monitor(detailContentPage, detailContainer.find(".cpiButtonsDetail .ts-cleared"));
    }

    refreshDetailContent = (detailContentPage) => {
        pageHelper.showDetails(detailContentPage.activePage, detailContentPage.id)
    }

    monitor = (detailContentPage, el) => {
        if (el.length > 0) {
            const interval = setInterval(() => {
                if (el.is(":visible")) {
                    $.get(`/Admin/TradeSecret/Monitor`, { locator: el.data("locator") })
                        .fail((e) => {
                            this.refreshDetailContent(detailContentPage);
                            clearInterval(interval);
                        });
                }
            }, 10 * 1000);
        }
    }
}