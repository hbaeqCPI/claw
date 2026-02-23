import RMSPortfolioReviewPage from "./rmsPortfolioReviewPage";

export default class RMSInstructionsPage extends RMSPortfolioReviewPage {

    constructor() {
        super();
    }

    initializeSearchResultPage(searchResultPage) {
        super.initializeSearchResultPage(searchResultPage);
        this.loadActionClosing();
    }

    showCount() {
        super.showCount();
        this.showActionClosingCount();
    }

    loadActionClosing() {
        const link = $(this.searchResultContainer).find(".show-action-closing");
        const url = link.data("url");

        if (url) {
            link.on("click", function () {
                pageHelper.openLink(url);
            });
        }
    }

    showActionClosingCount() {
        const link = $(this.searchResultContainer).find(".show-action-closing");
        const tickler = link.closest(".tickler");
        const url = link.data("count-url");

        if (url) {
            $.post(url, { __RequestVerificationToken: tickler.find("input[name=__RequestVerificationToken]").val() })
                .done(function (result) {
                    const badge = link.find(".badge");
                    if (badge) {
                        badge.text(result);
                        if (result > 0)
                            badge.show();
                        else
                            badge.hide();
                    }
                })
                .fail(function (error) {
                    console.error(error.responseText);
                });
        }
    }

}