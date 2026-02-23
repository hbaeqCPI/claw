import Image from "../image";
import ActivePage from "../activePage";

export default class GMActionDuePage extends ActivePage {

    constructor() {
        super();
        this.matterInfo = [];
        this.image = new Image();
        this.editableGrids = [];
        this.tabsLoaded = [];
        this.docServerOperation = true;
        this.systemTypeCode = "G";
    }

    init(addMode, parentId, pageId, isSharePointIntegrationOn) {
        this.editableGrids = [
            {
                name: `actionsGrid_${pageId}`, isDirty: false, filter: { parentId: parentId }, afterSubmit: (e) => { this.showCheckDocket(e); this.updateRecordStamps(); pageHelper.handleEmailWorkflow(e); },
            }
        ];

        this.matterInfo = [{ CaseNumber: "", SubCase: "", MatterType: "", MatterStatus: "" }];
        this.tabsLoaded = [];
        this.docServerOperation = !isSharePointIntegrationOn;
        this.tabChangeSetListener();

        $(document).ready(()=> {
            if (addMode) {
                const caseNumber = this.getKendoComboBox("CaseNumber");
                const subCase = this.getKendoComboBox("SubCase");
                const actionType = this.getKendoComboBox("ActionType");

                subCase.enable(caseNumber.value());
                actionType.enable(caseNumber.value());

                if (caseNumber.value()) {
                    this.getKendoComboBox("ActionType").input.attr("autofocus", "autofocus");
                }
                else {
                    caseNumber.input.attr("autofocus", "autofocus");
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

            $("#actionDueDetail-Content").on("click", ".verified-date-default", function () {
                const parent = $(this).closest(".verification-dateverified-field");
                const responseDate = parent.find("input[name='DateVerified']").data("kendoDatePicker");
                const currentDate = new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate());
                responseDate.value(currentDate);
                responseDate.trigger("change");
                parent.find("div.float-label").removeClass("inactive").addClass("active");
            });

            $("#actionDueDetail-Content").on("click", "input[name='CheckDocket']", function () {
                const parent = $(this).closest(".verification-fields");
                const verifiedByField = parent.find(".verification-verifiedby-field");
                const dateVerifiedField = parent.find(".verification-dateverified-field");
                const showVerifyAction = parent.data("show-verify-action");

                if (verifiedByField) 
                {
                    if ($(this).is(":checked")) {
                        verifiedByField.removeClass("d-none");
                    }
                    else if (!showVerifyAction) {
                        verifiedByField.addClass("d-none");
                    }
                }

                if (dateVerifiedField) 
                {
                    if ($(this).is(":checked")) {
                        dateVerifiedField.removeClass("d-none");
                        dateVerifiedField.addClass("d-flex");
                    }
                    else if (!showVerifyAction) {
                        dateVerifiedField.addClass("d-none");
                        dateVerifiedField.removeClass("d-flex");
                    }
                }
            });
        });
    }

    showCheckDocket = (e) => {
        if (e) {
            var checkDocketDiv = $(`#${this.mainDetailContainer}`).find(".CheckDocketDiv");
            if (checkDocketDiv) {  
                var checkDocketChkbx = $(`#${this.mainDetailContainer}`).find("#CheckDocket");
                if (e.showCheckDocket) {
                    checkDocketDiv.removeClass("d-none");
                }                    
                else {
                    checkDocketDiv.addClass("d-none");
                    if (checkDocketChkbx)
                        checkDocketChkbx.prop("checked", false);
                }
            }   
        }
    }

    actionTypeLinkData=()=> {
        const actionType = this.getKendoComboBox("ActionType");
        return { actionType: actionType.value() };
    }

    matterLinkData = () => {
        const caseNumber = this.getKendoComboBox("CaseNumber");
        const subCase = this.getKendoComboBox("SubCase");
        return { caseNumber: caseNumber.value(), subCase: subCase.value() };
    }

    tabChangeSetListener() {
        $('#actionDueTab a').on('click', (e)=> {
            e.preventDefault();
            const tab = e.target.id;
            if (this.tabsLoaded.indexOf(tab) === -1) {
                this.tabsLoaded.push(tab);
                this.loadTabContent(tab);
            }
        });
    }

    afterInsert = (result) => {
        let id = result;
        if (isNaN(result))
            id = result.id;

        pageHelper.showDetails(this, id, () => {
            $(`#${this.mainDetailContainer}`).find("#actionDueDetailDueDatesTab").click();
            pageHelper.handleEmailWorkflow(result);
            //this.handleSaveWorkflow(result);
        });
    }

    showDetails(result) {
        let id = result;
        if (isNaN(result))
            id = result.id;

        pageHelper.showDetails(this, id, () => {
            pageHelper.handleEmailWorkflow(result);
        });
    }

    deleteDueDate = (e, grid, deleteActionDuePrompt) => {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        if (grid.dataSource.total() === 1) {
            grid.options.editable.confirmDelete = deleteActionDuePrompt;
            pageHelper.deleteGridRow(e, dataItem, (result) => {
                if (result.emailWorkflows) {
                    const promise = pageHelper.handleEmailWorkflow(result);
                    promise.then(() => {
                        window.cpiBreadCrumbs.showPreviousNode();
                    });
                }
                else {
                    if (result.success)
                        cpiAlert.warning(result.success);
                    window.cpiBreadCrumbs.showPreviousNode();
                }
            });
        }
        else {
            pageHelper.deleteGridRow(e, dataItem, (result) => {
                this.updateRecordStamps();
                pageHelper.handleEmailWorkflow(result);
            });
        }
    }

    loadTabContent(tab) {
        switch (tab) {
            case "actionDueDetailDueDatesTab":
                $(document).ready(()=> {
                    const grid = $(`#actionsGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    if (parseInt(grid.options.dataSource.pageSize) > 0)
                        grid.dataSource.pageSize(grid.options.dataSource.pageSize);
                    else
                        grid.dataSource.read();
                });
                break;

            case "actionDueDetailDocumentsTab":
                $(document).ready(() => {
                    if ($(`#imageGridView_${this.mainDetailContainer}`).length > 0)
                        this.image.initializeImage(this, this.docServerOperation);
                    else {
                        const grid = $(`#docsOutGrid_${this.mainDetailContainer}`).data("kendoGrid");
                        grid.dataSource.read();
                    }
                });
                break;

            case "actionDueDetailCorrespondenceTab":
                $(document).ready(()=> {
                    const grid = $(`#docsOutGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "":
                break;
        }
    }

    caseNumberSearchValueMapper(options) {
        $.ajax({
            url: this.caseNumberSearchValueMapperUrl,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }

    caseNumberDetailValueMapper(options) {
        $.ajax({
            url: this.caseNumberDetailValueMapperUrl,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }
    
    getMatterInfo(callback) {
        const caseNumber = this.getKendoComboBox("CaseNumber");

        this.cpiLoadingSpinner.show();
        $.get(caseNumber._form.data("matter-info-url"), { caseNumber: caseNumber.value() })
            .done((data)=> {
                this.cpiLoadingSpinner.hide();
                this.matterInfo = data;

                if (typeof callback !== "undefined" && callback !== null) {
                    callback();
                }
            })
            .fail((e)=> {
                this.cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            });
    }

    showMatterInfo(sender, caseNumber, subCase) {
        const elName = sender.element[0].name;
        const matterInfo = this.matterInfo.find(function (matterInfo) {
            switch (elName) {
                case "SubCase":
                    return matterInfo.CaseNumber === caseNumber && matterInfo.SubCase === subCase;
                default:
                    return matterInfo.CaseNumber === caseNumber;
            }
        });

        if (matterInfo === undefined) {
            sender.value("");
            this.clearMatterInfo();
            return;
        }

        const cboSubCase = this.getKendoComboBox("SubCase");
        const cboActionType = this.getKendoComboBox("ActionType");

        cboSubCase.value(matterInfo["SubCase"]);
        cboSubCase.element.trigger("change");

        if (elName !== "SubCase") {
            //only when empty
            if (!cboActionType.value()) {
                cboActionType.value("");
                cboActionType.element.trigger("change");
            }
        }

        cboSubCase.enable(true);
        cboActionType.enable(true);

        for (const property in matterInfo) {
            let value = matterInfo[property];
            const element = this.getElement(property);
            if (element.length > 0 && element.data("role") !== "combobox") {
                if (isNaN(value) && Date.parse(value))
                    value = pageHelper.cpiDateFormatToDisplay(new Date(value));

                element.val(value).trigger("change");
            }
        }
    }

    clearMatterInfo() {
        const caseNumber = this.getKendoComboBox("CaseNumber");
        const subCase = this.getKendoComboBox("SubCase");
        const actionType = this.getKendoComboBox("ActionType");

        if (caseNumber.value() === "") {
            subCase.value("");
            subCase.element.trigger("change");

            //only when empty
            if (!actionType.value()) {
                actionType.value("");
                actionType.element.trigger("change");
            }

            subCase.enable(false);
            actionType.enable(false);
        }

        for (const property in this.matterInfo[0]) {
            const element = this.getElement(property);
            if (element.length > 0 && element.data("role") !== "combobox") {
                element.val("").trigger("change");
            }
        }
    }

    onCaseNumberChange=(e)=>  {
        const caseNumber = e.sender;
        this.cpiStatusMessage.hide();
        if (caseNumber.value()) {
            if (caseNumber.selectedIndex === -1) {
                //this.cpiStatusMessage.error(caseNumber.element.data("invalid"));
                pageHelper.showErrors(caseNumber.element.data("invalid"));
                this.clearMatterInfo();
                caseNumber.value("");
                caseNumber.focus();
            }
            else {
                this.getMatterInfo(()=> {
                    this.showMatterInfo(caseNumber, caseNumber.value());

                    const subCase = this.getKendoComboBox("SubCase");
                    const actionType = this.getKendoComboBox("ActionType");

                    subCase.dataSource.read();
                    actionType.dataSource.read();
                });
            }
        }
        else {
            this.clearMatterInfo();
        }
    }

    onSubCaseChange=(e)=>  {
        const comboBox = e.sender;
        const caseNumber = this.getKendoComboBox("CaseNumber").value();
        const subCase = comboBox.value();

        if (this.matterInfo.length === 0 || this.matterInfo[0].CaseNumber === "")
            this.getMatterInfo(()=> {
                this.showMatterInfo(comboBox, caseNumber, subCase);
            });
        else
            this.showMatterInfo(comboBox, caseNumber, subCase);
    }

    onActionTypeChange=(e)=>  {
        const actionType = e.sender.dataItem();
        const responsible = this.getKendoComboBox("ResponsibleID");
        const responsibleName = $(`#${responsible.element[0].id}_Name`);

        if (actionType && actionType.ResponsibleID && !responsible.value()) {
            responsible.value(actionType.ResponsibleID);
            responsible.element.trigger("change");
            responsibleName.val(actionType.ResponsibleName).trigger("change");
        }

        if (actionType && actionType.ActionTypeID) {
            $('#ActionTypeID').val(actionType.ActionTypeID);
        }
    }

    getMatterSubCaseListData=()=> {
        const caseNumber = this.getKendoComboBox("CaseNumber");
        return { caseNumber: caseNumber.value() };
    }

    getActionTypeListData=()=> {
        const caseNumber = this.getKendoComboBox("CaseNumber");
        const subCase = this.getKendoComboBox("SubCase");
        return { caseNumber: caseNumber.value(), subCase: subCase.value() };
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