import ActivePage from "../activePage";

export default class EmailSetupPage extends ActivePage {

    constructor() {
        super();
    }

    preview(e, grid) {
        const navLink = $(e.currentTarget);
        const dataItem = grid.dataItem(navLink.closest("tr"));
        const form = navLink.closest("form");
        const url = navLink.data("url");
        const name = form.find("#Name");

        const data = {
            name: name.val(),
            language: dataItem.LanguageCulture,
            __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
        };

        cpiLoadingSpinner.show();
        $.post(url, data)
            .done(function (result) {
                cpiLoadingSpinner.hide();
                var content = `<div class="email-preview">${result}</div>`;
                cpiAlert.popUp(navLink.text(), content, null, true);

                $(".email-preview").find("a").on("click.readonly", false);
            })
            .fail(function (error) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(error);
            });
    }

}