import Image from "../image";
import ActivePage from "../activePage";

export default class PatCostTrackingPage extends ActivePage {

    constructor() {
        super();
        this.appInfo = [];
        this.image = new Image();
        this.docServerOperation = true;
        this.systemTypeCode = "P";
    }

    init(addMode, isSharePointIntegrationOn) {
        
        this.appInfo = [{ CaseNumber: "", Country: "", SubCase: "", AppTitle: "", CaseType: "", ApplicationStatus: "", AppNumber: "", FilDate: "" }];
        this.tabsLoaded = [];
        this.docServerOperation = !isSharePointIntegrationOn;

        this.tabChangeSetListener();

        $(document).ready(()=> {
            if (addMode) {
                const caseNumber = this.getKendoComboBox("CaseNumber");
                const country = this.getKendoComboBox("Country");
                const subCase = this.getKendoComboBox("SubCase");

                country.enable(caseNumber.value());
                subCase.enable(country.value());

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

    countryLinkData = () => {
        const caseNumber = this.getKendoComboBox("CaseNumber");
        const country = this.getKendoComboBox("Country");
        const subCase = this.getKendoComboBox("SubCase");
        return { caseNumber: caseNumber.value(), country: country.value(), subCase: subCase.value() };
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
    
    loadTabContent(tab) {
        switch (tab) {
            case "costTrackingDetailDocumentsTab":
                $(document).ready(() => {
                    if ($(`#imageGridView_${this.mainDetailContainer}`).length > 0)
                        this.image.initializeImage(this, this.docServerOperation);
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

        pageHelper.showDetails(this, id, () => {
            pageHelper.handleEmailWorkflow(result);
        });
    }

    getAppInfo(callback) {
        const caseNumber = this.getKendoComboBox("CaseNumber");

        this.cpiLoadingSpinner.show();
        $.get(caseNumber._form.data("app-info-url"), { caseNumber: caseNumber.value() })
            .done((data)=> {
                this.cpiLoadingSpinner.hide();
                this.appInfo = data;

                if (typeof callback !== "undefined" && callback !== null) {
                    callback();
                }
            })
            .fail(function (e) {
                this.cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            });
    }

    showAppInfo(sender, caseNumber, country, subCase) {
        const elName = sender.element[0].name;
        const appInfo = this.appInfo.find(function (appInfo) {
            switch (elName) {
                case "SubCase":
                    return appInfo.CaseNumber === caseNumber && appInfo.Country == country && appInfo.SubCase == subCase;
                case "Country":
                    return appInfo.CaseNumber === caseNumber && appInfo.Country == country;
                default:
                    return appInfo.CaseNumber === caseNumber;
            }
        });

        if (appInfo === undefined) {
            sender.value("");
            this.clearAppInfo();
            return;
        }

        const cboCountry = this.getKendoComboBox("Country");
        const countryName = $(`#${cboCountry.element[0].id}_Name`);
        const cboSubCase = this.getKendoComboBox("SubCase");

        cboCountry.value(appInfo["Country"]);
        cboCountry.element.trigger("change");
        countryName.val(appInfo["CountryName"]).trigger("change");

        cboSubCase.value(appInfo["SubCase"]);
        cboSubCase.element.trigger("change");

        cboCountry.enable(true);
        cboSubCase.enable(true);

        //populate agent only if blank
        const agent = this.getKendoComboBox("AgentID");
        if (agent.value() === "" && appInfo["AgentID"]) {
            const agentName = $(`#${agent.element[0].id}_Name`);

            agent.value(appInfo["AgentID"]);
            agent.element.trigger("change");
            agentName.val(appInfo["AgentName"]).trigger("change");
        }

        for (const property in appInfo) {
            let value = appInfo[property];
            const element = this.getElement(property);
            if (element.length > 0 && element.data("role") && !element.data("role").includes("combobox")) {
                if (isNaN(value) && Date.parse(value))
                    value = pageHelper.cpiDateFormatToDisplay(new Date(value));

                element.val(value).trigger("change");
            }
        }
    }

    clearAppInfo () {
        const caseNumber = this.getKendoComboBox("CaseNumber");
        const country = this.getKendoComboBox("Country");
        const countryName = $(`#${country.element[0].id}_Name`);
        const subCase = this.getKendoComboBox("SubCase");

        if (caseNumber.value() === "") {
            country.value("");
            country.element.trigger("change");
            countryName.val("").trigger("change");

            subCase.value("");
            subCase.element.trigger("change");

            country.enable(false);
            subCase.enable(false);
        }
        else if (country.value() === "") {
            countryName.val("").trigger("change");
            subCase.value("");
            subCase.element.trigger("change");

            subCase.enable(false);
        }

        for (const property in this.appInfo[0]) {
            const element = this.getElement(property);
            if (element.length > 0 && element.data("role") !== "combobox") {
                element.val("").trigger("change");
            }
        }
    }

    onCaseNumberChange = (caseNumber)=> {
        this.cpiStatusMessage.hide();
        if (caseNumber.value()) {
            if (caseNumber.selectedIndex === -1) {
                //cpiAlert.warning(caseNumber.element.data("invalid"), function () {
                //    this.clearAppInfo();
                //    caseNumber.value("");
                //    caseNumber.focus();
                //});
                //this.cpiStatusMessage.error(caseNumber.element.data("invalid"));
                pageHelper.showErrors(caseNumber.element.data("invalid"));
                this.clearAppInfo();
                caseNumber.value("");
                caseNumber.focus();
            }
            else {
                this.getAppInfo(()=> {
                    this.showAppInfo(caseNumber, caseNumber.value());

                    const country = this.getKendoComboBox("Country");
                    const subCase = this.getKendoComboBox("SubCase");

                    country.dataSource.read();
                    subCase.dataSource.read();
                });
            }
        }
        else {
            this.clearAppInfo();
        }
    }

    onCountryChange = (comboBox)=> {
        const caseNumber = this.getKendoComboBox("CaseNumber").value();
        const country = comboBox.value();

        const subCase = this.getKendoComboBox("SubCase");
        subCase.dataSource.read();

        if (this.appInfo.length === 0 || this.appInfo[0].CaseNumber === "")
            this.getAppInfo(()=> {
                this.showAppInfo(comboBox, caseNumber, country);
            });
        else
            this.showAppInfo(comboBox, caseNumber, country);
    }

    onSubCaseChange = (comboBox)=> {
        const caseNumber = this.getKendoComboBox("CaseNumber").value();
        const country = this.getKendoComboBox("Country").value();
        const subCase = comboBox.value();

        if (this.appInfo.length === 0 || this.appInfo[0].CaseNumber === "")
            this.getAppInfo(()=> {
                this.showAppInfo(comboBox, caseNumber, country, subCase);
            });
        else
            this.showAppInfo(comboBox, caseNumber, country, subCase);
    }

    onCostTypeSelect = (e) => {
        var invoiceAmount = this.getElement('InvoiceAmount');
        invoiceAmount.data("kendoNumericTextBox").value(e.dataItem.DefaultCost);
    }

    getAppCountryListData=()=> {
        const caseNumber = this.getKendoComboBox("CaseNumber");
        return { caseNumber: caseNumber.value() };
    }

    getAppSubCaseListData=()=> {
        const caseNumber = this.getKendoComboBox("CaseNumber");
        const country = this.getKendoComboBox("Country");
        return { caseNumber: caseNumber.value(), country: country.value() };
    }
}

