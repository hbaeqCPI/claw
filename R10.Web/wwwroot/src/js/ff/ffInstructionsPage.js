import FFPortfolioReviewPage from "./ffPortfolioReviewPage";

export default class FFInstructionsPage extends FFPortfolioReviewPage {

    constructor() {
        super();
    }

    initializeSearchResultPage(searchResultPage) {
        super.initializeSearchResultPage(searchResultPage);
        this.loadAction($(this.searchResultContainer).find(".show-action-closing"));
        this.loadAction($(this.searchResultContainer).find(".show-generate-applications"));
    }

    showCount() {
        super.showCount();
        this.showActionCount($(this.searchResultContainer).find(".show-action-closing"));
        this.showActionCount($(this.searchResultContainer).find(".show-generate-applications"));
    }

    loadAction(link) {
        const url = link.data("url");

        if (url) {
            link.on("click", function () {
                pageHelper.openLink(url);
            });
        }
    }

    showActionCount(link) {
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