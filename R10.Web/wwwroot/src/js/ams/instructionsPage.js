import PortfolioReviewPage from "./portfolioReviewPage";

export default class InstructionsPage extends PortfolioReviewPage {

    constructor() {
        super();
    }

    initializeSearchResultPage(searchResultPage) {
        super.initializeSearchResultPage(searchResultPage);
        this.loadSendToCPi();
    }

    showTotals() {
        super.showTotals();
        this.showSendToCPiCount();
    }

    loadSendToCPi() {
        const link = $(this.searchResultContainer).find(".show-send-to-cpi");
        const url = link.data("url");

        if (url) {
            link.on("click", function () {
                pageHelper.openLink(url);
            });
        }
    }

    showSendToCPiCount() {
        const link = $(this.searchResultContainer).find(".show-send-to-cpi");
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