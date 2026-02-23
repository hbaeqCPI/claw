import Image from "../image";
import ActivePage from "../activePage";

export default class TmkCostTrackingPage extends ActivePage {

    constructor() {
        super();
        this.tmkInfo = [];
        this.image = new Image();
        this.docServerOperation = true;
        this.systemTypeCode = "T";
    }



    init(addMode, isSharePointIntegrationOn) {
        this.tmkInfo = [{ CaseNumber: "", Country: "", SubCase: "", TrademarkName: "", CaseType: "", TrademarkStatus: "", AppNumber: "", FilDate: "" }];
        this.tabsLoaded = [];
        this.docServerOperation = !isSharePointIntegrationOn;

        this.tabChangeSetListener();

        $(document).ready(() => {
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

    caseNumberLinkData = () => {
        const caseNumber = this.getKendoComboBox("CaseNumber");
        const country = this.getKendoComboBox("Country");
        const subCase = this.getKendoComboBox("SubCase");
        return { caseNumber: caseNumber.value(), country: country.value(), subCase: subCase.value() };
    }

    tabChangeSetListener = function () {
        $('#costTrackingTab a').on('click', (e) => {
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

        pageHelper.showDetails(this, id, () => {
            pageHelper.handleEmailWorkflow(result);
        });
    }

    getTmkInfo = function (callback) {
        const caseNumber = this.getKendoComboBox("CaseNumber");

        this.cpiLoadingSpinner.show();
        $.get(caseNumber._form.data("tmk-info-url"), { caseNumber: caseNumber.value() })
            .done((data) => {
                this.cpiLoadingSpinner.hide();
                this.tmkInfo = data;

                if (typeof callback !== "undefined" && callback !== null) {
                    callback();
                }
            })
            .fail(function (e) {
                this.cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            });
    }

    showTmkInfo = function (sender, caseNumber, country, subCase) {
        const elName = sender.element[0].name;
        const tmkInfo = this.tmkInfo.find(function (tmkInfo) {
            switch (elName) {
                case "SubCase":
                    return tmkInfo.CaseNumber == caseNumber && tmkInfo.Country == country && tmkInfo.SubCase == subCase;
                case "Country":
                    return tmkInfo.CaseNumber == caseNumber && tmkInfo.Country == country;
                default:
                    return tmkInfo.CaseNumber == caseNumber;
            }
        });

        if (tmkInfo === undefined) {
            sender.value("");
            this.clearTmkInfo();
            return;
        }

        country = this.getKendoComboBox("Country");
        const countryName = $(`#${country.element[0].id}_Name`);
        subCase = this.getKendoComboBox("SubCase");

        country.value(tmkInfo["Country"]);
        country.element.trigger("change");
        countryName.val(tmkInfo["CountryName"]).trigger("change");

        subCase.value(tmkInfo["SubCase"]);
        subCase.element.trigger("change");

        country.enable(true);
        subCase.enable(true);

        //populate agent only if blank
        const agent = this.getKendoComboBox("AgentID");

        if ((agent.value() === "" || agent.value() === 0) && tmkInfo["AgentID"]) {
            const agentName = $(`#${agent.element[0].id}_Name`);

            agent.value(tmkInfo["AgentID"]);
            agent.element.trigger("change");
            agentName.val(tmkInfo["AgentName"]).trigger("change");
        }

        for (const property in tmkInfo) {
            let value = tmkInfo[property];
            const element = this.getElement(property);
            if (element.length > 0 && !element.data("role").includes("combobox")) {
                if (isNaN(value) && Date.parse(value))
                    value = pageHelper.cpiDateFormatToDisplay(new Date(value));

                element.val(value).trigger("change");
            }
        }
    }

    clearTmkInfo = function () {
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

        for (const property in this.tmkInfo[0]) {
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
                //cpiAlert.warning(caseNumber.element.data("invalid"), function () {
                //    this.clearTmkInfo();
                //    caseNumber.value("");
                //    caseNumber.focus();
                //});
                //this.cpiStatusMessage.error(caseNumber.element.data("invalid"));
                pageHelper.showErrors(caseNumber.element.data("invalid"));
                this.clearTmkInfo();
                caseNumber.value("");
                caseNumber.focus();
            }
            else {
                this.getTmkInfo(() => {
                    this.showTmkInfo(caseNumber, caseNumber.value());

                    const country = this.getKendoComboBox("Country");
                    const subCase = this.getKendoComboBox("SubCase");

                    country.dataSource.read();
                    subCase.dataSource.read();
                });
            }
        }
        else {
            this.clearTmkInfo();
        }
    }

    onCountryChange = (comboBox) => {
        const caseNumber = this.getKendoComboBox("CaseNumber").value();
        const country = comboBox.value();

        const subCase = this.getKendoComboBox("SubCase");
        subCase.dataSource.read();

        if (this.tmkInfo.length === 0 || this.tmkInfo[0].CaseNumber === "")
            this.getTmkInfo(() => {
                this.showTmkInfo(comboBox, caseNumber, country);
            });
        else
            this.showTmkInfo(comboBox, caseNumber, country);
    }

    onSubCaseChange = (comboBox) => {
        const caseNumber = this.getKendoComboBox("CaseNumber").value();
        const country = this.getKendoComboBox("Country").value();
        const subCase = comboBox.value();

        if (this.tmkInfo.length === 0 || this.tmkInfo[0].CaseNumber === "")
            this.getTmkInfo(() => {
                this.showTmkInfo(comboBox, caseNumber, country, subCase);
            });
        else
            this.showTmkInfo(comboBox, caseNumber, country, subCase);
    }

    onCostTypeSelect = (e) => {
        var invoiceAmount = this.getElement('InvoiceAmount');
        invoiceAmount.data("kendoNumericTextBox").value(e.dataItem.DefaultCost);
    }

    getTmkCountryListData = () => {
        const caseNumber = this.getKendoComboBox("CaseNumber");
        return { caseNumber: caseNumber.value() };
    }

    getTmkSubCaseListData = () => {
        const caseNumber = this.getKendoComboBox("CaseNumber");
        const country = this.getKendoComboBox("Country");
        return { caseNumber: caseNumber.value(), country: country.value() };
    }

}

