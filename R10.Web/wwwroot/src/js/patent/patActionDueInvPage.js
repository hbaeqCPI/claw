import Image from "../image";
import ActivePage from "../activePage";

export default class PatActionDueInvPage extends ActivePage {

    constructor() {
        super();
        this.invInfo = [];
        this.image = new Image();
        this.editableGrids = [];
        this.tabsLoaded = [];
        this.docServerOperation = true;
        this.systemTypeCode = "P";
    }

    init(addMode, parentId, pageId, isSharePointIntegrationOn) {

        this.docServerOperation = !isSharePointIntegrationOn;
        this.editableGrids = [
            {
                name: `actionsGrid_${pageId}`, isDirty: false, filter: { parentId: parentId }, afterSubmit: (e) => { this.updateRecordStamps(); pageHelper.handleEmailWorkflow(e);},
            }
        ];

        this.invInfo = [{ CaseNumber: "", DisclosureStatus: ""}];
        this.tabsLoaded = [];
        this.tabChangeSetListener();

        $(document).ready(()=> {
            if (addMode) {
                const caseNumber = this.getKendoComboBox("CaseNumber");
                const actionType = this.getKendoComboBox("ActionType");

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
                const parent = $(this).closest(".verified-date");
                const responseDate = parent.find("input[name='DateVerified']").data("kendoDatePicker");
                const currentDate = new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate());
                responseDate.value(currentDate);
                responseDate.trigger("change");
                parent.find("div.float-label").removeClass("inactive").addClass("active");
            });
        });
    }

    actionTypeLinkData=()=> {
        const actionType = this.getKendoComboBox("ActionType");
        //const country = this.getKendoComboBox("Country");
        return { actionType: actionType.value(), country: "" };
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
            //pageHelper.handleEmailWorkflow(result);
        });
    }

    showDetails(result) {
        let id = result;
        if (isNaN(result))
            id = result.id;
        
        pageHelper.showDetails(this, id, () => {
            //pageHelper.handleEmailWorkflow(result);
        });
    }

    deleteDueDate = (e, grid, deleteActionDuePrompt) => {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        if (grid.dataSource.total() === 1) {
            grid.options.editable.confirmDelete = deleteActionDuePrompt;
            pageHelper.deleteGridRow(e, dataItem, (result) => {
                
                //if (result.emailWorkflows) {
                //    const promise = pageHelper.handleEmailWorkflow(result);
                //    promise.then(() => {
                //        window.cpiBreadCrumbs.showPreviousNode();
                //    });
                //}
                //else {
                    if (result.success)
                        cpiAlert.warning(result.success);

                    window.cpiBreadCrumbs.showPreviousNode();
                //}
                    
            });
        }
        else {
            pageHelper.deleteGridRow(e, dataItem, (result) => {
                this.updateRecordStamps();
                //pageHelper.handleEmailWorkflow(result);
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
    
    getInvInfo(callback) {
        const caseNumber = this.getKendoComboBox("CaseNumber");

        this.cpiLoadingSpinner.show();
        $.get(caseNumber._form.data("inv-info-url"), { caseNumber: caseNumber.value() })
            .done((data)=> {
                this.cpiLoadingSpinner.hide();
                this.invInfo = data;

                if (typeof callback !== "undefined" && callback !== null) {
                    callback();
                }
            })
            .fail((e)=> {
                this.cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            });
    }
    
    showInvInfo(sender, caseNumber) {
        const elName = sender.element[0].name;
        const invInfo = this.invInfo.find(function (invInfo) {
            switch (elName) {
                default:
                    return invInfo.CaseNumber === caseNumber;
            }
        });

        if (invInfo === undefined) {
            sender.value("");
            this.clearInvInfo();
            return;
        }
        const cboActionType = this.getKendoComboBox("ActionType");
        cboActionType.enable(true);

        for (const property in invInfo) {
            let value = invInfo[property];
            const element = this.getElement(property);
            if (element.length > 0 && element.data("role") !== "combobox") {
                if (isNaN(value) && Date.parse(value))
                    value = pageHelper.cpiDateFormatToDisplay(new Date(value));

                element.val(value).trigger("change");
            }
        }
    }

    
    clearInvInfo() {
        const caseNumber = this.getKendoComboBox("CaseNumber");
        const actionType = this.getKendoComboBox("ActionType");

        if (caseNumber.value() === "") {

            //only when empty
            if (!actionType.value()) {
                actionType.value("");
                actionType.element.trigger("change");
            }

            actionType.enable(false);
        }        

        for (const property in this.invInfo[0]) {
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
                pageHelper.showErrors(caseNumber.element.data("invalid"));
                this.clearInvInfo();
                caseNumber.value("");
                caseNumber.focus();
            }
            else {
                this.getInvInfo(() => {
                    this.showInvInfo(caseNumber, caseNumber.value());

                    const actionType = this.getKendoComboBox("ActionType");

                    actionType.dataSource.read();
                });
            }
        }
        else {
            this.clearInvInfo();
        }
        
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

        if (actionType && actionType.IsOfficeAction) {           
            $('#IsOfficeAction').attr('checked', actionType.IsOfficeAction);
        }

        if (actionType && actionType.ActionTypeID) {
            $('#ActionTypeID').val(actionType.ActionTypeID);
        }
    }

    getActionTypeListData=()=> {
        //const country = this.getKendoComboBox("Country");
        return { country: "" };
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