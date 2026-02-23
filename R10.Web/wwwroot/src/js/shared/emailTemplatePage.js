import ActivePage from "../activePage";

export default class EmailTemplatePage extends ActivePage {

    constructor() {
        super();
    }

    initializeDetailContentPage(detailContentPage) {
        super.initializeDetailContentPage(detailContentPage);
        pageHelper.initKendoEditor("Template");
    }

    templateDataFieldsChange = (e) => {
        const dataFields = e.sender;
        const body = dataFields.element.closest("form").find("#Template");
        const editor = body.data("kendoEditor");

        editor.paste(dataFields.value(), { split: false });
        e.sender.value("");
    }

}