import ActivePage from "../activePage";

export default class EmailSetupDetailPage extends ActivePage {

    constructor() {
        super();
    }

    initializeDetailContentPage(detailContentPage) {
        super.initializeDetailContentPage(detailContentPage);
        this.subject = $(`#${this.detailContentContainer}`).find("#Subject").saveCaretPosition();
        pageHelper.initKendoEditor("Body");
        $(`#${this.detailContentContainer}`).find("#Preview").on("click", this.preview);
    }

    subjectDataFieldsChange = (e) => {
        this.subject.paste(e.sender.value());
        this.subject.trigger("input");

        e.sender.value("");
    }

    bodyDataFieldsChange = (e) => {
        const dataFields = e.sender;
        const body = dataFields.element.closest("form").find("#Body");
        const editor = body.data("kendoEditor");

        editor.paste(dataFields.value(), { split: false });
        e.sender.value("");
    }

    preview = (e) => {
        const navLink = $(e.currentTarget);
        const form = navLink.closest("form");
        const url = navLink.data("url");
        const name = form.find("#Name");
        const language = form.find("#LanguageCulture");

        const data = {
            name: name.val(),
            language: language.val(),
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

    editorModified = (e) => {
        let editorText = e.sender.value();
        editorText = editorText.replace(/href="(javascript.*?)"/, 'href=#');

        e.sender.value(editorText);
        $(this.entryFormInstance).trigger('markDirty');
    }
}