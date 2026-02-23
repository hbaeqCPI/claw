import Image from "../image";
import ActivePage from "../activePage";

export default class PatCostTrackingInvPage extends ActivePage {

    constructor() {
        super();
        this.invInfo = [];
        this.image = new Image();
        this.docServerOperation = true;
    }

    init(addMode, isSharePointIntegrationOn) {
        this.invInfo = [{ CaseNumber: "", InvTitle: "", DisclosureStatus: "" }];
        this.tabsLoaded = [];
        this.docServerOperation = !isSharePointIntegrationOn;
        this.tabChangeSetListener();

        $(document).ready(() => {
            if (addMode) {
                const caseNumber = this.getKendoComboBox("CaseNumber");

                if (caseNumber.value()) {
                    this.getKendoComboBox("CostType").input.attr("autofocus", "autofocus");
                }
                else {
                    caseNumber.input.attr("autofocus", "autofocus");
                }

            }
        });
    }

    tabChangeSetListener() {
        $('#costTrackingInvTab a').on('click', (e) => {
            e.preventDefault();
            const tab = e.target.id;
            if (this.tabsLoaded.indexOf(tab) === -1) {
                this.tabsLoaded.push(tab);
                this.loadTabContent(tab);
            }
        });
    }

    loadTabContent(tab) {
        switch (tab) {
            case "costTrackingInvDetailDocumentsTab":
                $(document).ready(() => {
                    if ($(`#imageGridView_${this.mainDetailContainer}`).length > 0)
                        this.image.initializeImage(this, this.docServerOperation);
                    else {
                        const grid = $(`#docsOutGrid_${this.mainDetailContainer}`).data("kendoGrid");
                        grid.dataSource.read();
                    }

                    
                });
                break;

            case "costTrackingInvDetailCorrespondenceTab":
                $(document).ready(() => {
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

    afterInsert = (result) => {
        let id = result;
        if (isNaN(result))
            id = result.id;

        pageHelper.showDetails(this, id, () => {
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

    getInvInfo(callback) {
        const caseNumber = this.getKendoComboBox("CaseNumber");

        this.cpiLoadingSpinner.show();
        $.get(caseNumber._form.data("inv-info-url"), { caseNumber: caseNumber.value() })
            .done((data) => {
                this.cpiLoadingSpinner.hide();
                this.invInfo = data;

                if (typeof callback !== "undefined" && callback !== null) {
                    callback();
                }
            })
            .fail(function (e) {
                this.cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            });
    }

    showInvInfo(sender, caseNumber) {
        const elName = sender.element[0].name;
        const invInfo = this.invInfo.find(function (invInfo) {
            return invInfo.CaseNumber === caseNumber;
        });

        if (invInfo === undefined) {
            sender.value("");
            this.clearInvInfo();
            return;
        }

        //populate agent only if blank
        const agent = this.getKendoComboBox("AgentID");
        if (agent.value() === "" && invInfo["AgentID"]) {
            const agentName = $(`#${agent.element[0].id}_Name`);

            agent.value(invInfo["AgentID"]);
            agent.element.trigger("change");
            agentName.val(invInfo["AgentName"]).trigger("change");
        }

        for (const property in invInfo) {
            let value = invInfo[property];
            const element = this.getElement(property);
            if (element.length > 0 && element[0].tagName.toLowerCase() === "input") {
                if (isNaN(value) && Date.parse(value))
                    value = pageHelper.cpiDateFormatToDisplay(new Date(value));

                element.val(value).trigger("change");
            }
        }
    }

    clearInvInfo() {
        for (const property in this.invInfo[0]) {
            const element = this.getElement(property);
            if (element.length > 0 && element.data("role") !== "combobox") {
                element.val("").trigger("change");
            }
        }
    }

    onCaseNumberChange = (caseNumber) => {
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
                });
            }
        }
        else {
            this.clearInvInfo();
        }
    }

    onCostTypeSelect = (e) => {
        var invoiceAmount = this.getElement('InvoiceAmount');
        invoiceAmount.data("kendoNumericTextBox").value(e.dataItem.DefaultCost);
    }

    showInventionLink = (screen, title, isReadOnly) => {
        const container = $(`#${screen}`).find(".cpiButtonsDetail");
        const pageNav = container.find(".nav");
        pageNav.prepend(`<a class="nav-link invention-link" href="#" target="_self" title="${title}" role="button"><i class="fal fa-external-link pr-2"></i>${title}</a>`);
        container.find(".invention-link").on("click", function () {
            if (isReadOnly) {
                $(`#${screen}`).find(".case-number-link").trigger("click");
            }
            else
                $(`#CaseNumber_${screen}_cpiButtonLink`).trigger("click");
        });
    }
}

