import Image from "../image";
import ActivePage from "../activePage";

export default class TmkActionDuePage extends ActivePage {

    constructor() {
        super();
        this.tmkInfo = [];
        this.image = new Image();
        this.editableGrids = [];
        this.tabsLoaded = [];
        this.docServerOperation = true;
        this.systemTypeCode = "T";
    }

    init(addMode, parentId, pageId, isSharePointIntegrationOn) {
        this.editableGrids = [
            {
                name: `actionsGrid_${pageId}`, isDirty: false, filter: { parentId: parentId }, afterSubmit: (e) => { this.showCheckDocket(e); this.updateRecordStamps(); pageHelper.handleEmailWorkflow(e); },
            }
        ];

        this.tmkInfo = [{ CaseNumber: "", Country: "", SubCase: "", CaseType: "", TrademarkStatus: "", AppNumber: "", FilDate: "" }];
        this.tabsLoaded = [];
        this.docServerOperation = !isSharePointIntegrationOn;
        this.tabChangeSetListener();

        $(document).ready(()=> {
            if (addMode) {
                const caseNumber = this.getKendoComboBox("CaseNumber");
                const country = this.getKendoComboBox("Country");
                const subCase = this.getKendoComboBox("SubCase");
                const actionType = this.getKendoComboBox("ActionType");

                country.enable(caseNumber.value());
                subCase.enable(country.value());
                actionType.enable(country.value());

                if (country.value()) {
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
        const country = this.getKendoComboBox("Country");
        return { actionType: actionType.value(), country: country.value() };
    }

    countryLinkData = () => {
        const caseNumber = this.getKendoComboBox("CaseNumber");
        const country = this.getKendoComboBox("Country");
        const subCase = this.getKendoComboBox("SubCase");
        return { caseNumber: caseNumber.value(), country: country.value(), subCase: subCase.value() };
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
            //this.handleSaveWorkflow(result);
        });
    }

    //    if (result.emailWorkflows) {
    //handleSaveWorkflow = (result) => {
    //        let promise = this.processEmailWorkflow(result.id, result.emailWorkflows[0].isAutoEmail, result.emailWorkflows[0].qeSetupId, result.emailWorkflows[0].autoAttachImages);
    //        for (let i = 1; i < result.emailWorkflows.length; i++) {
    //            const workflow = result.emailWorkflows[i];
    //            promise = promise.then(() => {
    //                return this.processEmailWorkflow(result.id, workflow.isAutoEmail, workflow.qeSetupId, workflow.autoAttachImages);
    //            });
    //        }
    //    }
    //}

    //processEmailWorkflow = (id, isAutoEmail, qeSetupId, autoAttachImages) => {
    //    const deferred = $.Deferred();
    //    const baseUrl = $("body").data("base-url");
    //    let url = `${baseUrl}/Trademark/ActionDue/Email`;

    //    $.get(url, { id: id, sendImmediately: isAutoEmail, qeSetupId: qeSetupId, autoAttachImages: autoAttachImages  })
    //        .done((emailResult) => {
    //            if (!isAutoEmail) {
    //                const popupContainer = $(".cpiContainerPopup").last();
    //                popupContainer.html(emailResult);
    //                const dialog = $("#quickEmailDialog");
    //                dialog.modal("show");
    //                dialog.find("#ok, #cancel,.close").on("click", () => {
    //                    $(".modal-backdrop").remove();
    //                    deferred.resolve();
    //                });
    //            }
    //            else deferred.resolve();
    //        })
    //        .fail(function (error) {
    //            pageHelper.showErrors(error.responseText);
    //            deferred.resolve();
    //        });
    //    return deferred.promise();
    //}

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
    
    getTmkInfo(callback) {
        const caseNumber = this.getKendoComboBox("CaseNumber");

        this.cpiLoadingSpinner.show();
        $.get(caseNumber._form.data("tmk-info-url"), { caseNumber: caseNumber.value() })
            .done((data)=> {
                this.cpiLoadingSpinner.hide();
                this.tmkInfo = data;

                if (typeof callback !== "undefined" && callback !== null) {
                    callback();
                }
            })
            .fail((e)=> {
                this.cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            });
    }

    showTmkInfo(sender, caseNumber, country, subCase) {
        const elName = sender.element[0].name;
        const tmkInfo = this.tmkInfo.find(function (tmkInfo) {
            switch (elName) {
                case "SubCase":
                    return tmkInfo.CaseNumber === caseNumber && tmkInfo.Country === country && tmkInfo.SubCase === subCase;
                case "Country":
                    return tmkInfo.CaseNumber === caseNumber && tmkInfo.Country === country;
                default:
                    return tmkInfo.CaseNumber === caseNumber;
            }
        });

        if (tmkInfo === undefined) {
            sender.value("");
            this.clearTmkInfo();
            return;
        }

        const cboCountry = this.getKendoComboBox("Country");
        const countryName = $(`#${cboCountry.element[0].id}_Name`);
        const cboSubCase = this.getKendoComboBox("SubCase");
        const cboActionType = this.getKendoComboBox("ActionType");

        cboCountry.value(tmkInfo["Country"]);
        cboCountry.element.trigger("change");
        countryName.val(tmkInfo["CountryName"]).trigger("change");

        cboSubCase.value(tmkInfo["SubCase"]);
        cboSubCase.element.trigger("change");

        if (elName !== "SubCase") {
            //only when empty
            if (!cboActionType.value()) {
                cboActionType.value("");
                cboActionType.element.trigger("change");
            }
        }

        cboCountry.enable(true);
        cboSubCase.enable(true);
        cboActionType.enable(true);

        for (const property in tmkInfo) {
            let value = tmkInfo[property];
            const element = this.getElement(property);
            if (element.length > 0 && element.data("role") !== "combobox") {
                if (isNaN(value) && Date.parse(value))
                    value = pageHelper.cpiDateFormatToDisplay(new Date(value));

                element.val(value).trigger("change");
            }
        }
    }

    clearTmkInfo() {
        const caseNumber = this.getKendoComboBox("CaseNumber");
        const country = this.getKendoComboBox("Country");
        const countryName = $(`#${country.element[0].id}_Name`);
        const subCase = this.getKendoComboBox("SubCase");
        const actionType = this.getKendoComboBox("ActionType");

        if (caseNumber.value() === "") {
            country.value("");
            country.element.trigger("change");
            countryName.val("").trigger("change");

            subCase.value("");
            subCase.element.trigger("change");

            //only when empty
            if (!actionType.value()) {
                actionType.value("");
                actionType.element.trigger("change");
            }

            country.enable(false);
            subCase.enable(false);
            actionType.enable(false);
        }
        else if (country.value() === "") {
            countryName.val("").trigger("change");
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

        for (const property in this.tmkInfo[0]) {
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
                this.clearTmkInfo();
                caseNumber.value("");
                caseNumber.focus();
            }
            else {
                this.getTmkInfo(()=> {
                    this.showTmkInfo(caseNumber, caseNumber.value());

                    const country = this.getKendoComboBox("Country");
                    const subCase = this.getKendoComboBox("SubCase");
                    const actionType = this.getKendoComboBox("ActionType");

                    country.dataSource.read();
                    subCase.dataSource.read();
                    actionType.dataSource.read();
                });
            }
        }
        else {
            this.clearTmkInfo();
        }
    }

    onCountryChange=(e)=> {
        const comboBox = e.sender;
        const caseNumber = this.getKendoComboBox("CaseNumber").value();
        const country = comboBox.value();

        const subCase = this.getKendoComboBox("SubCase");
        subCase.dataSource.read();

        const actionType = this.getKendoComboBox("ActionType");
        actionType.dataSource.read();

        if (this.tmkInfo.length === 0 || this.tmkInfo[0].CaseNumber === "")
            this.getTmkInfo(()=> {
                this.showTmkInfo(comboBox, caseNumber, country);
            });
        else
            this.showTmkInfo(comboBox, caseNumber, country);
    }

    onSubCaseChange=(e)=>  {
        const comboBox = e.sender;
        const caseNumber = this.getKendoComboBox("CaseNumber").value();
        const country = this.getKendoComboBox("Country").value();
        const subCase = comboBox.value();

        if (this.tmkInfo.length === 0 || this.tmkInfo[0].CaseNumber === "")
            this.getTmkInfo(()=> {
                this.showTmkInfo(comboBox, caseNumber, country, subCase);
            });
        else
            this.showTmkInfo(comboBox, caseNumber, country, subCase);
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

    getTmkCountryListData=()=> {
        const caseNumber = this.getKendoComboBox("CaseNumber");
        return { caseNumber: caseNumber.value() };
    }

    getTmkSubCaseListData=()=> {
        const caseNumber = this.getKendoComboBox("CaseNumber");
        const country = this.getKendoComboBox("Country");
        return { caseNumber: caseNumber.value(), country: country.value() };
    }

    getActionTypeListData=()=> {
        const country = this.getKendoComboBox("Country");
        return { country: country.value() };
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