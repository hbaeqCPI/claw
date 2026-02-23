import ActivePage from "../activePage";

export default class FFInstructionTypePage extends ActivePage {

    constructor() {
        super();
    }

    searchResultGridDataBound = (e) => {
        const data = e.sender.dataSource.data();
        const grid = e.sender.element;

        if (data.length > 0) {
            grid.find(".instruction-type-setting").on("click", (e) => {
                this.setInstructionTypeSetting(e);
            });
        }
    }

    setInstructionTypeSetting(e) {
        const el = $(e.target);
        const grid = el.closest(".kendo-Grid").data("kendoGrid");
        const item = grid.dataItem(el.closest("tr"));
        const form = el.closest("form");

        const url = form.data("save-setting-url");
        if (url) {
            const setting = el.data("setting");
            const value = !item[setting];
            const data = {
                instructionId: item.InstructionId,
                setting: setting,
                value: value,
                tStamp: item.tStamp,
                __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
            };

            cpiLoadingSpinner.show();

            $.post(url, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();

                    item[setting] = value;
                    item.tStamp = result.tStamp;

                    if (value) {
                        el.addClass("fa-check-square");
                        el.removeClass("fa-square");
                    }
                    else {
                        el.removeClass("fa-check-square");
                        el.addClass("fa-square");
                    }

                    pageHelper.showSuccess(result.message);
                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error);
                });

        }
    }
}