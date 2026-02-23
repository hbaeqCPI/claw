import ActivePage from "../activePage";

export default class DMSActionDuePage extends ActivePage {

    constructor() {
        super();
        this.discInfo = [];
        //this.image = window.image;                    // no image for DMS action screen
        this.editableGrids = [];
        this.tabsLoaded = [];
        this.systemTypeCode = "D";
    }

    init(addMode, parentId, pageId) {
        this.editableGrids = [
            {
                name: `actionsGrid_${pageId}`, isDirty: false, filter: { parentId: parentId }, afterSubmit: this.updateRecordStamps
            }
        ];

        this.discInfo = [{ DisclosureNumber: "", DisclosureDate: "", DisclosureStatus: "", DisclosureTitle: "" }];
        this.tabsLoaded = [];
        this.tabChangeSetListener();

        $(document).ready(() => {
            if (addMode) {
                const disclosureNumber = this.getKendoComboBox("DisclosureNumber");
                const actionType = this.getKendoComboBox("ActionType");

                actionType.enable(disclosureNumber.value());

                if (disclosureNumber.value()) {
                    this.getKendoComboBox("ActionType").input.attr("autofocus", "autofocus");
                }
                else {
                    disclosureNumber.input.attr("autofocus", "autofocus");
                }

            }
            $("#actionDueDetail-Content").on("click", ".response-date-default", function () {
                const parent = $(this).closest(".response-date");
                const responseDate = parent.find("input[name='ResponseDate']").data("kendoDatePicker");
                const currentDate = new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate());
                responseDate.value(currentDate);
                responseDate.trigger("change");
                parent.find("div.float-label").removeClass("inactive").addClass("active");
            });
        });
    }

    actionTypeLinkData = () => {
        const actionType = this.getKendoComboBox("ActionType");
        return { actionType: actionType.value() };
    }


    tabChangeSetListener() {
        $('#actionDueTab a').on('click', (e) => {
            e.preventDefault();
            const tab = e.target.id;
            if (this.tabsLoaded.indexOf(tab) === -1) {
                this.tabsLoaded.push(tab);
                this.loadTabContent(tab);
            }
        });
    }

    afterInsert = (id) => {
        pageHelper.showDetails(this, id, () => {
            $(`#${this.mainDetailContainer}`).find("#actionDueDetailDueDatesTab").click();
        });
    }

    deleteDueDate = (e, grid, deleteActionDuePrompt) => {
        if (grid.dataSource.total() === 1) {
            const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));

            grid.options.editable.confirmDelete = deleteActionDuePrompt;
            pageHelper.deleteGridRow(e, dataItem, (result) => {
                if (result.success)
                    cpiAlert.warning(result.success);

                window.cpiBreadCrumbs.showPreviousNode();
            });
        }
        else {
            this.deleteGridRow(e, grid);
        }
    }

    loadTabContent(tab) {
        switch (tab) {
            case "actionDueDetailDueDatesTab":
                $(document).ready(() => {
                    const grid = $(`#actionsGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    if (parseInt(grid.options.dataSource.pageSize) > 0)
                        grid.dataSource.pageSize(grid.options.dataSource.pageSize);
                    else
                        grid.dataSource.read();
                });
                break;

            case "docsOutTab":
                $(document).ready(() => {
                    const grid = $(`#docsOutGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "":
                break;
        }
    }

    //disclosureNumberSearchValueMapper(options) {
    //    $.ajax({
    //        url: this.disclosureNumberSearchValueMapperUrl,
    //        data: { value: options.value },
    //        success: function (data) {
    //            options.success(data);
    //        }
    //    });
    //}

    disclosureNumberDetailValueMapper(options) {        
        $.ajax({
            url: this.disclosureNumberDetailValueMapperUrl,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }

    getDiscInfo(callback) {
        const disclosureNumber = this.getKendoComboBox("DisclosureNumber");

        this.cpiLoadingSpinner.show();
        $.get(disclosureNumber._form.data("app-info-url"), { caseNumber: disclosureNumber.value() })
            .done((data) => {
                this.cpiLoadingSpinner.hide();
                this.discInfo = data;

                if (typeof callback !== "undefined" && callback !== null) {
                    callback();
                }
            })
            .fail((e) => {
                this.cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            });
    }

    showDiscInfo(sender, disclosureNumber) {
        const elName = sender.element[0].name;
        const discInfo = this.discInfo.find(function (discInfo) {
            return discInfo.DisclosureNumber === disclosureNumber;
        });

        if (discInfo === undefined) {
            sender.value("");
            this.clearDiscInfo();
            return;
        }

        const cboActionType = this.getKendoComboBox("ActionType");

        cboActionType.enable(true);

        for (const property in discInfo) {
            let value = discInfo[property];
            const element = this.getElement(property);
            if (element.length > 0 && element.data("role") !== "combobox") {
                if (isNaN(value) && Date.parse(value))
                    value = pageHelper.cpiDateFormatToDisplay(new Date(value));

                element.val(value).trigger("change");
            }
        }
    }

    clearDiscInfo() {
        const disclosureNumber = this.getKendoComboBox("DisclosureNumber");
        const actionType = this.getKendoComboBox("ActionType");

        if (disclosureNumber.value() === "") {
            actionType.value("");
            actionType.element.trigger("change");
            actionType.enable(false);
        }

        for (const property in this.discInfo[0]) {
            const element = this.getElement(property);
            if (element.length > 0 && element.data("role") !== "combobox") {
                element.val("").trigger("change");
            }
        }
    }

    onDisclosureNumberChange = (e) => {
        const disclosureNumber = e.sender;
        this.cpiStatusMessage.hide();
        if (disclosureNumber.value()) {
            if (disclosureNumber.selectedIndex === -1) {
                //this.cpiStatusMessage.error(disclosureNumber.element.data("invalid"));
                pageHelper.showErrors(disclosureNumber.element.data("invalid"));
                this.clearDiscInfo();
                disclosureNumber.value("");
                disclosureNumber.focus();
            }
            else {
                this.getDiscInfo(() => {
                    this.showDiscInfo(disclosureNumber, disclosureNumber.value());
                    const actionType = this.getKendoComboBox("ActionType");
                    actionType.dataSource.read();
                });
            }
        }
        else {
            this.clearDiscInfo();
        }
    }

    onActionTypeChange = (e) => {
        const actionType = e.sender.dataItem();
        const responsible = this.getKendoComboBox("ResponsibleID");
        const responsibleName = $(`#${responsible.element[0].id}_Name`);

        if (actionType && actionType.ResponsibleID && !responsible.value()) {
            responsible.value(actionType.ResponsibleID);
            responsible.element.trigger("change");
            responsibleName.val(actionType.ResponsibleName).trigger("change");
        }
    }

    getActionTypeListData = () => {
        const disclosureNumber = this.getKendoComboBox("DisclosureNumber");
        return { disclosureNumber: disclosureNumber.value() };
    }

    onDueDateAttorneyChange = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");
            const grid = $(`#actionsGrid_${this.mainDetailContainer}`).data("kendoGrid");
            const dataItem = grid.dataItem(row);

            dataItem.AttorneyID = e.dataItem["AttorneyID"];
        }
    }

    clearDueDateAttorney = (e) => {
        const row = $(`#${e.sender.element[0].id}`).closest("tr");
        const grid = $(`#actionsGrid_${this.mainDetailContainer}`).data("kendoGrid");
        const dataItem = grid.dataItem(row);

        if (dataItem.DueDateAttorneyName == null || dataItem.DueDateAttorneyName == "") {
            dataItem.AttorneyID = null;
            dataItem.DueDateAttorneyName = "";
        }
    }

    showDueDateEmailScreen(e, grid) {
        const form = $("#" + this.detailContentContainer).find("form");
        const url = form.data("duedate-email-url");
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const ddId = dataItem.DDId;

        $.ajax({
            url: url,
            data: { id: ddId },
            success: function (result) {
                const popupContainer = $(".site-content .popup");
                popupContainer.html(result);
            },
            error: function (e) {
                pageHelper.showErrors(e);
            }
        });
    }
}
