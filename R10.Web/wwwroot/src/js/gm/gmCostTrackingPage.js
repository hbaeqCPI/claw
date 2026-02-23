import Image from "../image";
import ActivePage from "../activePage";

export default class GMCostTrackingPage extends ActivePage {

    constructor() {
        super();
        this.matterInfo = [];
        this.image = new Image();
        this.systemTypeCode = "G";
    }

    init(addMode) {
        this.matterInfo = [{ CaseNumber: "", SubCase: "", MatterTitle: "", MatterType: "", MatterStatus: "", EffectiveOpenDate: "", TerminationEndDate: "" }];
        this.tabsLoaded = [];
        this.tabChangeSetListener();

        $(document).ready(()=> {
            if (addMode) {
                const caseNumber = this.getKendoComboBox("CaseNumber");
                const subCase = this.getKendoComboBox("SubCase");
                subCase.enable(caseNumber.value());

                if (caseNumber.value()) {
                    this.getKendoComboBox("CostType").input.attr("autofocus", "autofocus");
                }
                else {
                    caseNumber.input.attr("autofocus", "autofocus");
                }
            }

            $("input[name='CurrencyType']").change(function () {
                let comboBox = $("#CurrencyType_costTrackingDetail").data("kendoComboBox");
                if (!comboBox)
                    comboBox = $("#CurrencyType_costTrackingDetail").data('kendoMultiColumnComboBox');

                var exchangeRate = 0;
                var allowanceRate = 0;

                if (comboBox.value() && comboBox.selectedIndex === -1)
                    comboBox.value("");

                if (comboBox.selectedIndex > 0) {
                    exchangeRate = comboBox.dataItem()['ExchangeRate'];
                    allowanceRate = comboBox.dataItem()['AllowanceRate'];
                }
                else if (comboBox.selectedIndex == 0 && comboBox.value() != "") {
                    var dataValueField = comboBox.options.dataValueField;
                    var dataValue = comboBox.value();
                    var len = comboBox.dataSource._data.length;
                    if (len > 0) {
                        var i;
                        for (i = 0; i < len; i++) {
                            if (comboBox.dataSource._data[i][dataValueField] == dataValue) {
                                exchangeRate = comboBox.dataSource._data[i]['ExchangeRate'];
                                allowanceRate = comboBox.dataSource._data[i]['AllowanceRate'];
                            }
                        }
                    }
                }

                var exchangeInput = $("input[name='ExchangeRate']");
                if (exchangeInput) {
                    exchangeInput.val(exchangeRate).trigger("change");
                }
                var allowanceInput = $("input[name='AllowanceRate']");
                if (allowanceInput) {
                    allowanceInput.val(allowanceRate).trigger("change");
                }
            })
        });
    }

    caseNumberLinkData = () => {
        const caseNumber = this.getKendoComboBox("CaseNumber");
        const subCase = this.getKendoComboBox("SubCase");
        return { caseNumber: caseNumber.value(), subCase: subCase.value() };
    }

    tabChangeSetListener() {
        $('#costTrackingTab a').on('click', (e)=> {
            e.preventDefault();
            const tab = e.target.id;
            if (this.tabsLoaded.indexOf(tab) === -1) {
                this.tabsLoaded.push(tab);
                this.loadTabContent(tab);
            }
        });
    }

    loadTabContent = function (tab) {
        switch (tab) {
            case "costTrackingDetailDocumentsTab":
                $(document).ready(() => {
                    if ($(`#imageGridView_${this.mainDetailContainer}`).length > 0)
                        this.image.initializeImage(this);
                    else {
                        const grid = $(`#docsOutGrid_${this.mainDetailContainer}`).data("kendoGrid");
                        grid.dataSource.read();
                    }

                    
                });
                break;

            case "costTrackingDetailCorrespondenceTab":
                $(document).ready(() => {
                    const grid = $(`#docsOutGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "":
                break;
        }
    }

    recordNavigateHandler = (id) => {
        this.cpiStatusMessage.hide();

        pageHelper.showDetails(this, id, () => {
            this.currentRecordId = id;
            this.tabsLoaded = [];

            const tab = $('#costTrackingTab').find('a.active');
            if (tab.length > 0) {
                this.loadTabContent(tab[0].id);
            }
        });
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

        if (this.recordNavigator && this.recordNavigator.length > 0) {
            this.recordNavigator.addRecordId(id);
        }

        pageHelper.showDetails(this, id, () => {
            pageHelper.handleEmailWorkflow(result);
        });
    }

    showDetails(result) {
        let id = result;
        if (isNaN(result))
            id = result.id;

        console.log("result", result);

        pageHelper.showDetails(this, id, () => {
            pageHelper.handleEmailWorkflow(result);
        });
    }

    getmatterInfo(callback) {
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
            .fail(function (e) {
                this.cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            });
    }

    showMatterInfo = function (sender, caseNumber, subCase) {
        const elName = sender.element[0].name;
        const matterInfo = this.matterInfo.find(function (matterInfo) {
            switch (elName) {
                case "SubCase":
                    return matterInfo.CaseNumber == caseNumber && matterInfo.SubCase == subCase;
                default:
                    return matterInfo.CaseNumber == caseNumber;
            }
        });

        if (matterInfo === undefined) {
            sender.value("");
            this.clearMatterInfo();
            return;
        }

        subCase = this.getKendoComboBox("SubCase");

        subCase.value(matterInfo["SubCase"]);
        subCase.element.trigger("change");

        subCase.enable(true);

        //populate agent only if blank
        const agent = this.getKendoComboBox("AgentID");
        if (agent.value() === "" && matterInfo["AgentID"]) {
            const agentName = $(`#${agent.element[0].id}_Name`);

            agent.value(matterInfo["AgentID"]);
            agent.element.trigger("change");
            agentName.val(matterInfo["AgentName"]).trigger("change");
        }

        for (const property in matterInfo) {
            let value = matterInfo[property];
            const element = this.getElement(property);
            if (element.length > 0 && !element.data("role").includes("combobox")) {
                if (isNaN(value) && Date.parse(value))
                    value = pageHelper.cpiDateFormatToDisplay(new Date(value));

                element.val(value).trigger("change");
            }
        }
    }

    clearMatterInfo = function () {
        const caseNumber = this.getKendoComboBox("CaseNumber");
        const subCase = this.getKendoComboBox("SubCase");

        if (caseNumber.value() === "") {
            subCase.value("");
            subCase.element.trigger("change");

            subCase.enable(false);
        }

        for (const property in this.matterInfo[0]) {
            const element = this.getElement(property);
            if (element.length > 0 && element.data("role") !== "combobox") {
                element.val("").trigger("change");
            }
        }
    }

    onCaseNumberChange=(caseNumber)=> {
        this.cpiStatusMessage.hide();
        if (caseNumber.value()) {
            if (caseNumber.selectedIndex === -1) {
                //cpiAlert.warning(caseNumber.element.data("invalid"), function () {
                //    this.clearMatterInfo();
                //    caseNumber.value("");
                //    caseNumber.focus();
                //});
                //this.cpiStatusMessage.error(caseNumber.element.data("invalid"));
                pageHelper.showErrors(caseNumber.element.data("invalid"));
                this.clearMatterInfo();
                caseNumber.value("");
                caseNumber.focus();
            }
            else {
                this.getmatterInfo(()=> {
                    this.showMatterInfo(caseNumber, caseNumber.value());

                    const subCase = this.getKendoComboBox("SubCase");
                    subCase.dataSource.read();
                })
            }
        }
        else {
            this.clearMatterInfo();
        }
    }

    onSubCaseChange = (comboBox)=> {
        const caseNumber = this.getKendoComboBox("CaseNumber").value();
        const subCase = comboBox.value();

        if (this.matterInfo.length === 0 || this.matterInfo[0].CaseNumber === "")
            this.getmatterInfo(()=> {
                this.showMatterInfo(comboBox, caseNumber, subCase);
            })
        else
            this.showMatterInfo(comboBox, caseNumber, subCase);
    }

    onCostTypeSelect = (e) => {
        var invoiceAmount = this.getElement('InvoiceAmount');
        invoiceAmount.data("kendoNumericTextBox").value(e.dataItem.DefaultCost);
    }

    getMatterSubCaseListData = ()=> {
        const caseNumber = this.getKendoComboBox("CaseNumber");
        return { caseNumber: caseNumber.value() };
    }
}

