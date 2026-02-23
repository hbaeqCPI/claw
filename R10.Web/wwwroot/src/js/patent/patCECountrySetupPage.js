import ActivePage from "../activePage";

export default class PatCECountrySetup extends ActivePage {

    constructor() {
        super();
        this.isCPIAdmin = false;
        this.CostDataType = { 
            STRING: 0, 
            DATE: 1, 
            DATERANGE: 2, 
            NUMERIC: 3, 
            BOOLEAN: 4, 
            SELECTION: 5, 
            NUMERICRANGE: 6 
        };
        this.TranslationType = {
            PAGES: { Text: "Pages", Value: 0 },
            WORDS: { Text: "Words", Value: 1 }
        }
        this.parentGridExpandedRows = [];
        this.childGridExpandedRows = [];
    }

    tabChangeSetListener = () => {
        const self = this;
        $('#patCECountrySetup-tab a').on('click',
            (e) => {
                e.preventDefault();
                const tab = e.target.id;
                if (this.tabsLoaded.indexOf(tab) === -1) {
                    this.tabsLoaded.push(tab);
                    this.loadTabContent(tab);
                }
            });

        $(document).ready(function () {

        });
    }

    loadTabContent(tab) {
        switch (tab) {

            case "":
                break;
        }
    }

    getParentTStamp = () => {
        const container = $(`#${this.detailContentContainer}`);
        const tStamp = container.find("input[name='tStamp']");
        return tStamp.val();
    }    

    /* main copy */
    getCountryFromCopyScreen = () => {
        const country = $("#patCECountrySetupCopyDialog").find("input[name='Country']");
        return { country: country.val() };
    }

    showCopyScreen() {
        const popupContainer = $(".site-content .popup").last();
        const dialogContainer = popupContainer.find("#patCECountrySetupCopyDialog");
        let entryForm = dialogContainer.find("form")[0];
        dialogContainer.modal("show");
        const self = this;

        entryForm = $(entryForm);
        const afterSubmit = function (result) {
            const dataContainer = $('#' + self.mainDetailContainer).find(".cpiDataContainer");
            if (dataContainer.length > 0) {
                setTimeout(function () {
                    dataContainer.empty();
                    dataContainer.html(result);
                }, 1000);
            }
        };
        entryForm.cpiPopupEntryForm({ dialogContainer: dialogContainer, afterSubmit: afterSubmit });
    }
        
    //---------------------------------------------------------
    // Main grid
    mainGridEdit(e) {
        var editorContainer = $(e.container);
        var model = e.model;
                
        var elements = {
            descriptionInput: { input: editorContainer.find("#Description") },
            stageDropDown: { input: editorContainer.find("#Stage").data("kendoDropDownList") },
            costTypeDropDown: { input: editorContainer.find("#CostType").data("kendoDropDownList") },
            dataTypeDropDown: { input: editorContainer.find("#DataType").data("kendoDropDownList") },
            datePicker: { divElement: editorContainer.find('#datePickerDiv'), input: editorContainer.find("#DefaultValueDate").data("kendoDatePicker") },
            dateRangePicker: { divElement: editorContainer.find('#dateRangePickerDiv'), input: editorContainer.find("#DefaultValueDateRange").data("kendoDateRangePicker") },
            numericOpt: { divElement: editorContainer.find('#numericOptDiv'), input: editorContainer.find("#NumericOpt").data("kendoDropDownList") },
            numericValue: { divElement: editorContainer.find('#numericValueDiv'), input: editorContainer.find("#DefaultValueNumeric").data("kendoNumericTextBox") },
            boolDropDown: { divElement: editorContainer.find('#booleanDiv'), input: editorContainer.find("#DefaultValueBool").data("kendoDropDownList") },
            cost: { divElement: editorContainer.find('#costDiv'), input: editorContainer.find("#Cost").data("kendoNumericTextBox") },
            altCost: { divElement: editorContainer.find('#altCostDiv'), input: editorContainer.find("#AltCost").data("kendoNumericTextBox") },
            multCost: { divElement: editorContainer.find('#multCostDiv'), input: editorContainer.find("#MultCost").data("kendoNumericTextBox") },
            costFactorSettings: { divElement: editorContainer.find('#costFactorSettings') },
            translationType: { input: editorContainer.find("#TranslationType").data("kendoDropDownList"), divElement: editorContainer.find("#translationTypeDiv") },
            useCostFactorSwitch: { divElement: editorContainer.find('#useCostFactorSwitch'), input: editorContainer.find('input[name="UseCostFactor"]') },
            patCPICostLabel: { divElement: editorContainer.find('#patCPiCostLabel') },        
            costFactor1: { input: editorContainer.find("#CostFactor1").data("kendoNumericTextBox") },
            costFactor2: { input: editorContainer.find("#CostFactor2").data("kendoNumericTextBox") },
            costFactor3: { input: editorContainer.find("#CostFactor3").data("kendoNumericTextBox") },            
            costFactor1Divs: { divElement: editorContainer.find(".costFactor1Div") }
        };

        // --- 1. INITIAL RECORD STATE ---
        if (model.CostId === 0) {
            model.ActiveSwitch = true;
            editorContainer.find('input[name="ActiveSwitch"]').prop('checked', true);
        } else {
            editorContainer.find('input[name="DateCreated"]').val(kendo.toString(model.DateCreated, "dd-MMM-yyyy hh:mm tt"));
            editorContainer.find('input[name="LastUpdate"]').val(kendo.toString(model.LastUpdate, "dd-MMM-yyyy hh:mm tt"));
        }

        // --- 2. SET VISIBILITY BASED ON DATA TYPE ---
        patCECountrySetupPage.hideAllElements(elements); // Hide all elements first

        // --- 3. CPIADMIN READ-ONLY STATE ---
        if (model.CPICost && !patCECountrySetupPage.isCPIAdmin) {
            elements.patCPICostLabel.divElement.show();
            editorContainer.find('input, select, textarea').attr('disabled', 'disabled');
            
            for (var key in elements) {
                if (elements.hasOwnProperty(key) && elements[key].input) {
                    try {
                        elements[key].input.enable(false);
                    } catch (ex) { }
                }
            }
            editorContainer.find('input[name="ActiveSwitch"]').removeAttr('disabled');
            elements.useCostFactorSwitch.input.prop('disabled', 'disabled');
        }

        switch (model.DataType) {
            case patCECountrySetupPage.CostDataType.STRING:
                elements.cost.divElement.show();
                break;
            case patCECountrySetupPage.CostDataType.DATE:
                elements.datePicker.divElement.show();
                elements.cost.divElement.show();
                if (model.DefaultValue && elements.datePicker.input) {
                    elements.datePicker.input.value(kendo.parseDate(model.DefaultValue, "dd-MMM-yyyy"));
                }
                break;
            case patCECountrySetupPage.CostDataType.DATERANGE:
                elements.dateRangePicker.divElement.show();
                elements.cost.divElement.show();
                elements.altCost.divElement.show();
                if (model.DefaultValue && elements.dateRangePicker.input) {
                    var dateRange = model.DefaultValue.split(";");
                    var range = { start: kendo.parseDate(dateRange[0], "dd-MMM-yyyy"), end: kendo.parseDate(dateRange[1], "dd-MMM-yyyy") };
                    elements.dateRangePicker.input.range(range);
                }
                break;
            case patCECountrySetupPage.CostDataType.NUMERIC:                
                if (model.CostType && model.CostType.toLowerCase() === "translation") {
                    elements.useCostFactorSwitch.divElement.show();                    
                }
            
                if (model.UseCostFactor) {
                    elements.costFactorSettings.divElement.show();
                    elements.translationType.divElement.show();

                    if (elements.costFactor1.input) elements.costFactor1.input.value(model.CostFactor1);
                    if (elements.costFactor2.input) elements.costFactor2.input.value(model.CostFactor2);
                    if (elements.costFactor3.input) elements.costFactor3.input.value(model.CostFactor3);
                } else {
                    elements.numericValue.divElement.show();
                    elements.numericOpt.divElement.show();
                    elements.multCost.divElement.show();
                    elements.cost.divElement.show();
                    elements.altCost.divElement.show();
                    if (elements.numericValue.input) elements.numericValue.input.value(model.DefaultValue);
                    if (elements.numericOpt.input) elements.numericOpt.input.value(model.Opts);
                }
                break;
            case patCECountrySetupPage.CostDataType.BOOLEAN:
                elements.boolDropDown.divElement.show();
                elements.cost.divElement.show();
                elements.altCost.divElement.show();
                if (elements.boolDropDown.input) {
                    elements.boolDropDown.input.value(model.DefaultValue);
                }
                break;
        }
    
        // --- 4. BIND EVENT HANDLERS AND INITIALIZE COMPONENTS ---    
        // Handle 'UseCostFactor' switch change event
        elements.useCostFactorSwitch.input.on("change", function() {
            if ($(this).is(":checked")) {
                elements.costFactorSettings.divElement.show();
                elements.translationType.divElement.show();

                elements.numericValue.divElement.hide();
                elements.numericOpt.divElement.hide();
                elements.multCost.divElement.hide();
                elements.cost.divElement.hide();
                elements.altCost.divElement.hide();
            } else {                
                if (elements.numericOpt.input) { elements.numericOpt.input.select(0); }
            
                elements.numericValue.divElement.show();
                elements.numericOpt.divElement.show();
                elements.multCost.divElement.show();
                elements.cost.divElement.show();
                elements.altCost.divElement.show();
            
                elements.costFactorSettings.divElement.hide();
                elements.translationType.divElement.hide();
            }
        });
        
        // Bind TranslationType input change handler
        elements.translationType.input.bind("change", function(event) {
            patCECountrySetupPage.mainGridEditorTranslationTypeChange({
                data: {
                    selectedType: event.sender.text(),
                    model: model,
                    costFactor1NumericTextBox: elements.costFactor1.input,
                    costFactor1Divs: elements.costFactor1Divs.divElement
                }
            });
        });

        // Initial call to set translation type handler state
        const currentTranslationType = model.TranslationType == patCECountrySetupPage.TranslationType.PAGES.Value ? patCECountrySetupPage.TranslationType.PAGES.Text : model.TranslationType == patCECountrySetupPage.TranslationType.WORDS.Value ? patCECountrySetupPage.TranslationType.WORDS.Text : patCECountrySetupPage.TranslationType.PAGES.Text;        
        patCECountrySetupPage.mainGridEditorTranslationTypeChange({
            data: {
                selectedType: currentTranslationType,
                model: model,
                costFactor1NumericTextBox: elements.costFactor1.input,
                costFactor1Divs: elements.costFactor1Divs.divElement
            }
        });

        // Bind CostType input change handler        
        elements.costTypeDropDown.input.bind("change", function(event) {
            patCECountrySetupPage.mainGridEditorCostTypeChange({
                data: {
                    sender: event.sender,                    
                    editorContainer: editorContainer
                }
            });
        });

        // Bind DataType input change handler
        elements.dataTypeDropDown.input.bind("change", function(event) {
            patCECountrySetupPage.mainGridEditorDataTypeChange({
                data: {
                    sender: event.sender,                    
                    editorContainer: editorContainer
                }
            });
        });
    }

    mainGridSave(e) {
        var editorContainer = $(e.container);
        var model = e.model;
                
        var elements = {
            datePicker: { input: editorContainer.find("#DefaultValueDate").data("kendoDatePicker") },
            dateRangePicker: { input: editorContainer.find("#DefaultValueDateRange").data("kendoDateRangePicker") },
            numericValue: { input: editorContainer.find("#DefaultValueNumeric").data("kendoNumericTextBox") },
            numericOpt: { input: editorContainer.find("#NumericOpt").data("kendoDropDownList") },
            boolDropDown: { input: editorContainer.find("#DefaultValueBool").data("kendoDropDownList") },
            costFactor1: { input: editorContainer.find("#CostFactor1").data("kendoNumericTextBox") },
            costFactor2: { input: editorContainer.find("#CostFactor2").data("kendoNumericTextBox") },
            costFactor3: { input: editorContainer.find("#CostFactor3").data("kendoNumericTextBox") },
            useCostFactorSwitch: { input: editorContainer.find("#UseCostFactor") }
        };

        // Use a switch statement for a cleaner data type check.
        switch (model.DataType) {
            case patCECountrySetupPage.CostDataType.STRING:
            case patCECountrySetupPage.CostDataType.SELECTION:
            case patCECountrySetupPage.CostDataType.NUMERICRANGE:
                model.set("DefaultValue", "");
                break;
            case patCECountrySetupPage.CostDataType.DATE:
                if (elements.datePicker.input) {
                    model.set("DefaultValue", kendo.toString(elements.datePicker.input.value(), "dd-MMM-yyyy"));
                }
                break;
            case patCECountrySetupPage.CostDataType.DATERANGE:
                var dateRangeString = "";
                if (elements.dateRangePicker.input && elements.dateRangePicker.input.range()) {
                    var range = elements.dateRangePicker.input.range();
                    dateRangeString = kendo.toString(range.start, "dd-MMM-yyyy") + ";" + kendo.toString(range.end, "dd-MMM-yyyy");
                }
                model.set("DefaultValue", dateRangeString);
                break;
            case patCECountrySetupPage.CostDataType.NUMERIC:
                if (model.CostType && model.CostType.toLowerCase() === 'translation' && elements.useCostFactorSwitch.input.is(":checked")) {
                    model.set("DefaultValue", elements.costFactor1.input.value() + ", " + elements.costFactor2.input.value() + ", " + elements.costFactor3.input.value());
                    model.set("Opts", "=");
                } else {
                    model.set("DefaultValue", elements.numericValue.input.value());
                    model.set("Opts", elements.numericOpt.input.value());
                    model.set("CostFactor1", 0);
                    model.set("CostFactor2", 0);
                    model.set("CostFactor3", 0);
                }
                break;
            case patCECountrySetupPage.CostDataType.BOOLEAN:
                if (elements.boolDropDown.input) {
                    model.set("DefaultValue", elements.boolDropDown.input.value());
                }
                break;
        }

        var isNumericTranslation = model.DataType === patCECountrySetupPage.CostDataType.NUMERIC && model.CostType && model.CostType.toLowerCase() === 'translation';

        if (isNumericTranslation && model.UseCostFactor) {
            if (!model.CostFactor1 || model.CostFactor1 === 0) {
                e.preventDefault();
                alert('Estimated number of words per page is required.');
            } else if (!model.CostFactor2 || model.CostFactor2 === 0) {
                e.preventDefault();
                alert('Estimated fee is required.');
            } else if (!model.CostFactor3 || model.CostFactor3 === 0) {
                e.preventDefault();
                alert('Estimated fee per word/page is required.');
            }
        } else if (model.DataType !== patCECountrySetupPage.CostDataType.STRING
            && model.DataType !== patCECountrySetupPage.CostDataType.SELECTION 
            && model.DataType !== patCECountrySetupPage.CostDataType.NUMERICRANGE 
            && (model.DefaultValue === "" || !model.DefaultValue)) {
            e.preventDefault();
            alert('Default value is required.');
        }
                
        if (!(isNumericTranslation && elements.useCostFactorSwitch.input)) {
            model.set("UseCostFactor", 0);
            model.set("CostFactor1", 0);
            model.set("CostFactor2", 0);
            model.set("CostFactor3", 0);
        }
            
        if (!model.Cost) {
            model.Cost = 0;
        }
    }

    mainGridDataBound(e) {
        var grid = this;
        var columns = e.sender.columns;
        var columnIndex = grid.wrapper.find(".k-grid-header [data-field=" + "DefaultValue" + "]").index();
    
        grid.tbody.find("tr[role='row']").each(function () {
            var row = grid.dataItem(this);
        
            // 1. Format Numeric data type display
            if (row.DataType === patCECountrySetupPage.CostDataType.NUMERIC) {
                var cell = $(this).children().eq(columnIndex);
                cell.html(row.Opts + " " + row.DefaultValue);
            }
        
            // 2. Disable hierarchy grid based on data type            
            if (row.DataType !== patCECountrySetupPage.CostDataType.SELECTION 
                && row.DataType !== patCECountrySetupPage.CostDataType.BOOLEAN 
                && row.DataType !== patCECountrySetupPage.CostDataType.NUMERICRANGE) {
                $(this).find(".k-hierarchy-cell a").remove();
            }
        
            // 3. Persist expanded rows
            if (patCECountrySetupPage.parentGridExpandedRows && row) {
                if ($.inArray(row.CostId, patCECountrySetupPage.parentGridExpandedRows) >= 0) {
                    grid.expandRow($(this)); 
                }
            }
        });
    }

    mainGridDetailExpand(e) {
        let dataItem = this.dataItem(e.masterRow);
        if (dataItem) {
            patCECountrySetupPage.parentGridExpandedRows.push(dataItem.CostId);
        }
    }

    mainGridDetailCollapse(e) {
        let dataItem = this.dataItem(e.masterRow);
        if (dataItem) {
            patCECountrySetupPage.parentGridExpandedRows = patCECountrySetupPage.parentGridExpandedRows.filter(id => id !== dataItem.CostId);
        }
    }  

    mainGridEditorTranslationTypeChange(e) {
        var model = e.data.model;
        var costFactor1NumericTextBox = e.data.costFactor1NumericTextBox;
        var costFactor1Divs = e.data.costFactor1Divs;
        
        switch (e.data.selectedType.toLowerCase()) {
            case 'pages':
                costFactor1Divs.show();
                break;
            case 'words':
                costFactor1Divs.hide();            
                if (costFactor1NumericTextBox) {
                    costFactor1NumericTextBox.value(1);
                    model.CostFactor1 = 1;
                }
                break;
            default:
                break;
        }
    }

    mainGridEditorCostTypeChange(e) {
        var editorContainer = e.data.editorContainer;
        var selectedCostType = e.data.sender.text();
                
        var elements = {  
            dataTypeDropDown: { input: editorContainer.find('#DataType').data('kendoDropDownList') },
            numericOpt: { divElement: editorContainer.find('#numericOptDiv'), input: editorContainer.find("#NumericOpt").data("kendoDropDownList") },
            numericValue: { divElement: editorContainer.find('#numericValueDiv'), input: editorContainer.find("#DefaultValueNumeric").data("kendoNumericTextBox") },     
            cost: { divElement: editorContainer.find('#costDiv'), input: editorContainer.find("#Cost").data("kendoNumericTextBox") },
            altCost: { divElement: editorContainer.find('#altCostDiv'), input: editorContainer.find("#AltCost").data("kendoNumericTextBox") },
            multCost: { divElement: editorContainer.find('#multCostDiv'), input: editorContainer.find("#MultCost").data("kendoNumericTextBox") },
            costFactorSettings: { divElement: editorContainer.find('#costFactorSettings') },
            translationType: { divElement: editorContainer.find('#translationTypeDiv') },
            useCostFactorSwitch: { divElement: editorContainer.find('#useCostFactorSwitch'), input: editorContainer.find('input[name="UseCostFactor"]') }
        };

        if (selectedCostType) {
            elements.useCostFactorSwitch.divElement.hide(); 

            if (elements.dataTypeDropDown.input && elements.dataTypeDropDown.input.value() === patCECountrySetupPage.CostDataType.NUMERIC.toString()) {
                
                patCECountrySetupPage.hideAllElements(elements);

                if (selectedCostType.toLowerCase() === "translation") {
                    elements.useCostFactorSwitch.divElement.show();
                                        
                    if (elements.useCostFactorSwitch.input.is(':checked')) {
                        elements.costFactorSettings.divElement.show();
                        elements.translationType.divElement.show();
                        
                        elements.numericOpt.divElement.hide();
                        elements.numericValue.divElement.hide();
                        elements.multCost.divElement.hide();
                        elements.altCost.divElement.hide();
                        elements.cost.divElement.hide();
                    } else {
                        elements.numericOpt.divElement.show();
                        elements.numericValue.divElement.show();
                        elements.multCost.divElement.show();
                        elements.altCost.divElement.show();
                        elements.cost.divElement.show();

                        elements.costFactorSettings.divElement.hide();
                        elements.translationType.divElement.hide();
                        
                        if (elements.numericOpt.input) {
                            elements.numericOpt.input.select(0);
                        }
                    }
                }
                else {
                    elements.useCostFactorSwitch.input.prop('checked', false);
                    
                    elements.numericOpt.divElement.show();
                    elements.numericValue.divElement.show();
                    elements.multCost.divElement.show();
                    elements.altCost.divElement.show();
                    elements.cost.divElement.show();

                    elements.costFactorSettings.divElement.hide();
                    elements.translationType.divElement.hide();
                        
                    if (elements.numericOpt.input) {
                        elements.numericOpt.input.select(0);
                    }
                }
            }            
        }
    }

    mainGridEditorDataTypeChange(e) {
        var editorContainer = e.data.editorContainer;
        var selectedType = e.data.sender.text();
                
        var elements = {
            datePicker: { divElement: editorContainer.find('#datePickerDiv') },
            dateRangePicker: { divElement: editorContainer.find('#dateRangePickerDiv') },
            numericOpt: { divElement: editorContainer.find('#numericOptDiv'), input: editorContainer.find("#NumericOpt").data("kendoDropDownList") },
            numericValue: { divElement: editorContainer.find('#numericValueDiv') },
            boolDropDown: { divElement: editorContainer.find('#booleanDiv') },
            cost: { divElement: editorContainer.find('#costDiv') },
            altCost: { divElement: editorContainer.find('#altCostDiv') },
            multCost: { divElement: editorContainer.find('#multCostDiv') },
            costFactorSettings: { divElement: editorContainer.find('#costFactorSettings') },
            translationType: { divElement: editorContainer.find('#translationTypeDiv') },
            useCostFactorSwitch: { divElement: editorContainer.find('#useCostFactorSwitch'), input: editorContainer.find('input[name="UseCostFactor"]') },            
            costTypeDropDown: { input: editorContainer.find('#CostType').data('kendoDropDownList') }
        };    
        
        patCECountrySetupPage.hideAllElements(elements);
                
        switch (selectedType) {
            case 'String':
                elements.cost.divElement.show();
                break;
            case 'Date':
                elements.datePicker.divElement.show();
                break;
            case 'Date Range':
                elements.dateRangePicker.divElement.show();
                elements.altCost.divElement.show();
                elements.cost.divElement.show();
                break;
            case 'Numeric':
                var costTypeDD = elements.costTypeDropDown.input;
                var isTranslation = costTypeDD && costTypeDD.value() && costTypeDD.value().toLowerCase() === "translation";
            
                if (isTranslation) {
                    elements.useCostFactorSwitch.divElement.show();
                    if (elements.useCostFactorSwitch.input.is(':checked')) {
                        elements.costFactorSettings.divElement.show();
                        elements.translationType.divElement.show();
                    } else {
                        elements.numericOpt.divElement.show();
                        elements.numericValue.divElement.show();
                        elements.multCost.divElement.show();
                        elements.altCost.divElement.show();
                        elements.cost.divElement.show();
                        if (elements.numericOpt.input) {
                            elements.numericOpt.input.select(0);
                        }
                    }
                } else {
                    elements.numericOpt.divElement.show();
                    elements.numericValue.divElement.show();
                    elements.multCost.divElement.show();
                    elements.altCost.divElement.show();
                    elements.cost.divElement.show();
                    if (elements.numericOpt.input) {
                        elements.numericOpt.input.select(0);
                    }
                }
                break;
            case 'Boolean':
                elements.boolDropDown.divElement.show();
                elements.altCost.divElement.show();
                elements.cost.divElement.show();
                break;
        }
    }

    //---------------------------------------------------------
    // Child grid
    childGridEdit(e) {
        var editorContainer = $(e.container);
        var model = e.model;
                
        var elements = {
            currencyType: { divElement: null, input: editorContainer.find("#CurrencyTypeChild").data("kendoMultiColumnComboBox") },
            datePicker: { divElement: editorContainer.find('#datePickerDiv'), input: editorContainer.find("#DefaultValueDateChild").data("kendoDatePicker") },
            dateRangePicker: { divElement: editorContainer.find('#dateRangePickerDiv'), input: editorContainer.find("#DefaultValueDateRangeChild").data("kendoDateRangePicker") },
            numericOpt: { divElement: editorContainer.find('#numericOptDiv'), input: editorContainer.find("#NumericOptChild").data("kendoDropDownList") },
            numericValue: { divElement: editorContainer.find('#numericValueDiv'), input: editorContainer.find("#DefaultValueNumericChild").data("kendoNumericTextBox") },
            altNumericOpt: { divElement: editorContainer.find('#altNumericOptDiv'), input: editorContainer.find("#NumericAltOptChild").data("kendoDropDownList") },
            altNumericValue: { divElement: editorContainer.find('#altNumericValueDiv'), input: editorContainer.find("#AltValueNumericChild").data("kendoNumericTextBox") },
            boolDropDown: { divElement: editorContainer.find('#booleanDiv'), input: editorContainer.find("#DefaultValueBoolChild").data("kendoDropDownList") },
            cost: { divElement: editorContainer.find('#costDiv'), input: editorContainer.find("#CCost").data("kendoNumericTextBox") },
            altCost: { divElement: editorContainer.find('#altCostDiv'), input: editorContainer.find("#CAltCost").data("kendoNumericTextBox") },
            multCost: { divElement: editorContainer.find('#multCostDiv'), input: editorContainer.find("#CMultCost").data("kendoNumericTextBox") },            
            patCCPICostLabel: { divElement: editorContainer.find('#patCCPiCostLabel') }
        };

        // --- BINDING EVENTS ---
        // Bind the change event for the SDataType dropdown
        var cDataTypeDropDownList = editorContainer.find("#CDataType").data("kendoDropDownList");
        cDataTypeDropDownList.bind("change", function(event) {
            patCECountrySetupPage.childGridEditorDataTypeChange({
                data: {
                    sender: event.sender,                    
                    editorContainer: editorContainer
                }
            });
        });
    
        // --- 1. INITIAL RECORD STATE ---
        if (model.CCId === 0) {
            model.CActiveSwitch = true;
            editorContainer.find('input[name="CActiveSwitch"]').prop('checked', true);
        } else {
            editorContainer.find('input[name="DateCreated"]').val(kendo.toString(model.DateCreated, "dd-MMM-yyyy hh:mm tt"));
            editorContainer.find('input[name="LastUpdate"]').val(kendo.toString(model.LastUpdate, "dd-MMM-yyyy hh:mm tt"));
        }

        // --- 2. SET VISIBILITY BASED ON DATA TYPE ---
        // Hide all elements first for a clean slate
        patCECountrySetupPage.hideAllElements(elements);

        // --- 3. CPI ADMIN READ-ONLY STATE ---
        if (model.CCPICost && !patCECountrySetupPage.isCPIAdmin) {            
            elements.patCCPICostLabel.divElement.show();
            editorContainer.find('input, select, textarea').attr('disabled', 'disabled');
            
            // Kendo-specific disabling
            for (var key in elements) {
                if (elements.hasOwnProperty(key) && elements[key].input) {
                    elements[key].input.enable(false);
                }
            }
            editorContainer.find('input[name="CActiveSwitch"]').removeAttr('disabled');
        }

        // Then, show only the elements required for the current data type
        switch (model.CDataType) {
            case patCECountrySetupPage.CostDataType.STRING:
                elements.cost.divElement.show();
                break;
            case patCECountrySetupPage.CostDataType.DATE:
                elements.datePicker.divElement.show();
                elements.cost.divElement.show();
                if (model.CDefaultValue && elements.datePicker.input) {
                    elements.datePicker.input.value(kendo.parseDate(model.CDefaultValue, "dd-MMM-yyyy"));
                }
                break;
            case patCECountrySetupPage.CostDataType.DATERANGE:
                elements.dateRangePicker.divElement.show();
                elements.cost.divElement.show();
                elements.altCost.divElement.show();
                if (model.CDefaultValue && elements.dateRangePicker.input) {
                    var dateRange = model.CDefaultValue.split(";");
                    var range = { start: kendo.parseDate(dateRange[0], "dd-MMM-yyyy"), end: kendo.parseDate(dateRange[1], "dd-MMM-yyyy") };
                    elements.dateRangePicker.input.range(range);
                }
                break;
            case patCECountrySetupPage.CostDataType.NUMERIC:
                elements.numericOpt.divElement.show();
                elements.numericValue.divElement.show();
                elements.multCost.divElement.show();
                elements.cost.divElement.show();
                elements.altCost.divElement.show();
                if (elements.numericValue.input) { elements.numericValue.input.value(model.CDefaultValue); }
                if (elements.numericOpt.input) { elements.numericOpt.input.value(model.COpts); }
                break;
            case patCECountrySetupPage.CostDataType.BOOLEAN:
                elements.boolDropDown.divElement.show();
                elements.cost.divElement.show();
                elements.altCost.divElement.show();
                if (elements.boolDropDown.input) { elements.boolDropDown.input.value(model.CDefaultValue); }
                break;
            case patCECountrySetupPage.CostDataType.NUMERICRANGE:
                elements.numericOpt.divElement.show();
                elements.numericValue.divElement.show();
                elements.altNumericOpt.divElement.show();
                elements.altNumericValue.divElement.show();
                elements.multCost.divElement.show();
                elements.cost.divElement.show();
                elements.altCost.divElement.show();
                if (elements.numericValue.input) { elements.numericValue.input.value(model.CDefaultValue); }
                if (elements.altNumericValue.input) { elements.altNumericValue.input.value(model.CAltValue); }
                if (elements.numericOpt.input) { elements.numericOpt.input.value(model.COpts); }
                if (elements.altNumericOpt.input) { elements.altNumericOpt.input.value(model.CAltOpts); }
                break;
        }
    
        // Set CurrencyType value if it exists
        if (model.CurrencyType && elements.currencyType.input) {
            elements.currencyType.input.value(model.CurrencyType);
        }
    }

    childGridSave(e) {
        var editorContainer = $(e.container);
        var model = e.model;

        var elements = {
            currencyType: { input: editorContainer.find("#CurrencyTypeChild").data("kendoMultiColumnComboBox") },
            datePicker: { input: editorContainer.find("#DefaultValueDateChild").data("kendoDatePicker") },
            dateRangePicker: { input: editorContainer.find("#DefaultValueDateRangeChild").data("kendoDateRangePicker") },            
            numericOpt: { input: editorContainer.find("#NumericOptChild").data("kendoDropDownList") },
            numericValue: { input: editorContainer.find("#DefaultValueNumericChild").data("kendoNumericTextBox") },            
            altNumericOpt: { input: editorContainer.find("#NumericAltOptChild").data("kendoDropDownList") },
            altNumericValue: { input: editorContainer.find("#AltValueNumericChild").data("kendoNumericTextBox") },            
            boolDropDown: { input: editorContainer.find("#DefaultValueBoolChild").data("kendoDropDownList") }            
        };

        switch (model.CDataType) {
            case patCECountrySetupPage.CostDataType.STRING:
                model.set("CDefaultValue", "");
                break;
            case patCECountrySetupPage.CostDataType.DATE:
                if (elements.datePicker.input) {
                    model.set("CDefaultValue", kendo.toString(elements.datePicker.input.value(), "dd-MMM-yyyy"));
                }
                break;
            case patCECountrySetupPage.CostDataType.DATERANGE:
                var dateRangeString = "";
                if (elements.dateRangePicker.input && elements.dateRangePicker.input.range()) {
                    var range = elements.dateRangePicker.input.range();
                    dateRangeString = kendo.toString(range.start, "dd-MMM-yyyy") + ";" + kendo.toString(range.end, "dd-MMM-yyyy");
                }
                model.set("CDefaultValue", dateRangeString);
                break;
            case patCECountrySetupPage.CostDataType.NUMERIC:
                if (elements.numericValue.input) {
                    model.set("CDefaultValue", elements.numericValue.input.value());
                }
                if (elements.numericOpt.input) {
                    model.set("COpts", elements.numericOpt.input.value());
                }
                break;
            case patCECountrySetupPage.CostDataType.BOOLEAN:
                if (elements.boolDropDown.input) {
                    model.set("CDefaultValue", elements.boolDropDown.input.value());
                }
                break;
            case patCECountrySetupPage.CostDataType.NUMERICRANGE:
                if (elements.numericValue.input) {
                    model.set("CDefaultValue", elements.numericValue.input.value());
                }
                if (elements.numericOpt.input) {
                    model.set("COpts", elements.numericOpt.input.value());
                }
                if (elements.altNumericValue.input) {
                    model.set("CAltValue", elements.altNumericValue.input.value());
                }
                if (elements.altNumericOpt.input) {
                    model.set("CAltOpts", elements.altNumericOpt.input.value());
                }
                break;            
        }
                
        if (elements.currencyType.input) {
            e.model.CurrencyType = elements.currencyType.input.value();
        }
                
        if (e.model.CDataType != patCECountrySetupPage.CostDataType.STRING && (e.model.CDefaultValue === "" || e.model.CDefaultValue === null)) {
            e.preventDefault();
            alert('Default value is required.');
        }
                
        if (!e.model.CCost) {
            e.model.CCost = 0;
        }
    }

    childGridDataBound(e) {
        var grid = this;
        var columns = e.sender.columns;
        var columnIndex = grid.wrapper.find(".k-grid-header [data-field=" + "CDefaultValue" + "]").index();
        grid.tbody.find("tr[role='row']").each(function () {
            var row = grid.dataItem(this);
            //if (row.CCPICost) {
            //    $(this).find(".k-grid-edit").attr("hidden", true);
            //    $(this).find(".k-grid-delete").attr("hidden", true);
            //}

            if (row.CDataType == patCECountrySetupPage.CostDataType.NUMERIC) {
                var cell = $(this).children().eq(columnIndex);
                cell.html(row.COpts + " " + row.CDefaultValue);
            }
            else if (row.CDataType == patCECountrySetupPage.CostDataType.NUMERICRANGE) {
                var cell = $(this).children().eq(columnIndex);
                if (row.CAltValue) {
                    cell.html(row.COpts + " " + row.CDefaultValue + "; " + row.CAltOpts + " " + row.CAltValue);
                }
                else {
                    cell.html(row.COpts + " " + row.CDefaultValue);
                }                
            }

            //Disable hierachy grid
            if (!(row.CDataType == patCECountrySetupPage.CostDataType.STRING)) {
                $('tr[data-uid="' + row.uid + '"] ').find(".k-hierarchy-cell a").remove();
            }

            //Persist expanded rows            
            if (patCECountrySetupPage.childGridExpandedRows) {                
                if (row) {                    
                    if ($.inArray(row.CCId, patCECountrySetupPage.childGridExpandedRows) >= 0) {                        
                        grid.expandRow($('tr[data-uid=' + $(this).data('uid') + ']'));
                    }
                }
            }
        });
    }

    childGridCancel(e) {
        //refresh to hide expand icon issue
        var grid = this;      
        if (grid) {
            setTimeout(function() {
                grid.tbody.find("tr[role='row']").each(function () {
                    var row = grid.dataItem(this);
                    
                    //Disable hierachy grid
                    if (!(row.CDataType == patCECountrySetupPage.CostDataType.STRING)) {
                        $('tr[data-uid="' + row.uid + '"] ').find(".k-hierarchy-cell a").remove();
                    }
                });
            }, 10);            
        }
    }

    childGridDetailExpand(e) {
        let dataItem = this.dataItem(e.masterRow);
        if (dataItem) {
            patCECountrySetupPage.childGridExpandedRows.push(dataItem.CCId);
        }
    }

    childGridDetailCollapse(e) {
        let dataItem = this.dataItem(e.masterRow);
        if (dataItem) {
            patCECountrySetupPage.childGridExpandedRows = patCECountrySetupPage.childGridExpandedRows.filter(id => id !== dataItem.CCId);
        }
    }

    childGridEditorDataTypeChange(e) {
        var editorContainer = $(e.data.editorContainer);
        var selectedType = e.data.sender.text();
                
        var elements = {
            datePicker: { divElement: editorContainer.find('#datePickerDiv') },
            dateRangePicker: { divElement: editorContainer.find('#dateRangePickerDiv') },
            boolDropDown: { divElement: editorContainer.find('#booleanDiv') },
            cost: { divElement: editorContainer.find('#costDiv') },
            altCost: { divElement: editorContainer.find('#altCostDiv') },
            multCost: { divElement: editorContainer.find('#multCostDiv') },
            numericOpt: { divElement: editorContainer.find('#numericOptDiv') },
            numericValue: { divElement: editorContainer.find('#numericValueDiv') },
            altNumericOpt: { divElement: editorContainer.find('#altNumericOptDiv') },
            altNumericValue: { divElement: editorContainer.find('#altNumericValueDiv') },
        };
                
        patCECountrySetupPage.hideAllElements(elements);
                
        switch (selectedType) {
            case 'String':
                elements.cost.divElement.show();
                break;
            case 'Date':
                elements.datePicker.divElement.show();
                break;
            case 'Date Range':
                elements.dateRangePicker.divElement.show();
                elements.altCost.divElement.show();
                elements.cost.divElement.show();
                break;
            case 'Numeric':
                elements.numericOpt.divElement.show();
                elements.numericValue.divElement.show();
                elements.multCost.divElement.show();
                elements.altCost.divElement.show();
                elements.cost.divElement.show();
                
                editorContainer.find('#NumericOptChild').data('kendoDropDownList').select(0);
                break;
            case 'Boolean':
                elements.boolDropDown.divElement.show();
                elements.altCost.divElement.show();
                elements.cost.divElement.show();
                break;
            case 'Numeric Range':
                elements.numericOpt.divElement.show();
                elements.numericValue.divElement.show();
                elements.altNumericOpt.divElement.show();
                elements.altNumericValue.divElement.show();
                elements.multCost.divElement.show();
                elements.altCost.divElement.show();
                elements.cost.divElement.show();
                
                editorContainer.find('#NumericOptChild').data('kendoDropDownList').select(0);
                editorContainer.find('#NumericAltOptChild').data('kendoDropDownList').select(0);
                break;
        }
    }

    costChildDataTypeData = (e) => {
        var dataTypeDD = $("#CDataType").data("kendoDropDownList");       
        var parentRow = dataTypeDD.wrapper.closest("tr.k-detail-row").prev("tr");
        var parentGrid = parentRow.closest("[data-role=grid]").data("kendoGrid");
        var parentData = parentGrid.dataItem(parentRow);
        
        return {
            parentDataType: parentData.DataType
        };
    }

    //---------------------------------------------------------
    // Sub grid
    subGridEdit(e) {
        var editorContainer = $(e.container);
        var model = e.model;

        var subGrid = e.sender;
        var childGridDetailRow = $(subGrid.element).closest(".k-detail-row");
        var childGridRow = childGridDetailRow.prev();
        var mainGridDetailRow = childGridRow.closest(".k-detail-row");
        var mainGridRow = mainGridDetailRow.prev();
        var mainGrid = mainGridRow.closest(".k-grid").data("kendoGrid");
        var mainDataItem = mainGrid.dataItem(mainGridRow);

        var elements = {
            dataType: { divElement: null, input: editorContainer.find("#SDataType").data("kendoDropDownList") },
            datePicker: { divElement: editorContainer.find('#datePickerDiv'), input: editorContainer.find("#DefaultValueDateSub").data("kendoDatePicker") },
            dateRangePicker: { divElement: editorContainer.find('#dateRangePickerDiv'), input: editorContainer.find("#DefaultValueDateRangeSub").data("kendoDateRangePicker") },
            numericOpt: { divElement: editorContainer.find('#numericOptDiv'), input: editorContainer.find("#NumericOptSub").data("kendoDropDownList") },
            numericValue: { divElement: editorContainer.find('#numericValueDiv'), input: editorContainer.find("#DefaultValueNumericSub").data("kendoNumericTextBox") },            
            boolDropDown: { divElement: editorContainer.find('#booleanDiv'), input: editorContainer.find("#DefaultValueBoolSub").data("kendoDropDownList") },
            sCost: { divElement: editorContainer.find('#costDiv'), input: editorContainer.find("#SCost").data("kendoNumericTextBox") },
            altCost: { divElement: editorContainer.find('#altCostDiv'), input: editorContainer.find("#SAltCost").data("kendoNumericTextBox") },
            multCost: { divElement: editorContainer.find('#multCostDiv'), input: editorContainer.find("#SMultCost").data("kendoNumericTextBox") },
            useCostFactorSwitch: { divElement: editorContainer.find('#useCostFactorSwitch'), input: "" },
            translationType: { input: editorContainer.find("#STranslationType").data("kendoDropDownList"), divElement: editorContainer.find("#translationTypeDiv") },
            costFactorSettings: { divElement: editorContainer.find('#costFactorSettings'), input: "" },
            patSCPICostLabel: { divElement: editorContainer.find('#patSCPiCostLabel'), input: "" },
            costFactor1: { divElement: null, input: editorContainer.find("#SCostFactor1").data("kendoNumericTextBox") },
            costFactor2: { divElement: null, input: editorContainer.find("#SCostFactor2").data("kendoNumericTextBox") },
            costFactor3: { divElement: null, input: editorContainer.find("#SCostFactor3").data("kendoNumericTextBox") },
            costFactor1Divs: { divElement: editorContainer.find(".costFactor1Div") }
        };        
                
        //-------------------------------------------------------------------------------------------------------------
        // --- INITIAL STATE SETUP ---
        // 1. Handle New/Existing records
        if (model.SubId === 0) {
            model.SActiveSwitch = true;
            editorContainer.find('input[name="SActiveSwitch"]').prop('checked', true);
        } else {
            editorContainer.find('input[name="DateCreated"]').val(kendo.toString(model.DateCreated, "dd-MMM-yyyy hh:mm tt"));
            editorContainer.find('input[name="LastUpdate"]').val(kendo.toString(model.LastUpdate, "dd-MMM-yyyy hh:mm tt"));
        }

        // 2. Set the initial visibility based on data type
        patCECountrySetupPage.hideAllElements(elements);

        // 3. Handle CPI Admin/Read-only state
        if (model.SCPICost && !patCECountrySetupPage.isCPIAdmin) {
            elements.patSCPICostLabel.divElement.show();
            editorContainer.find('input, select, textarea').attr('disabled', 'disabled');
            
            elements.dateRangePicker.input.enable(false);
            elements.numericOpt.input.enable(false);
            elements.boolDropDown.input.enable(false);
            elements.numericValue.input.enable(false);
            elements.altCost.input.enable(false);
            elements.sCost.input.enable(false);
            elements.multCost.input.enable(false);        
            elements.dataType.input.enable(false);            
            editorContainer.find('input[name="SActiveSwitch"]').removeAttr('disabled');
        }

        //-------------------------------------------------------------------------------------------------------------
        switch (model.SDataType) {
            case patCECountrySetupPage.CostDataType.STRING:
                elements.sCost.divElement.show();
                break;
            case patCECountrySetupPage.CostDataType.DATE:
                elements.datePicker.divElement.show();
                elements.sCost.divElement.show();
                if (model.SDefaultValue && elements.datePicker.input) {
                    elements.datePicker.input.value(kendo.parseDate(model.SDefaultValue, "dd-MMM-yyyy"));
                }
                break;
            case patCECountrySetupPage.CostDataType.DATERANGE:
                elements.dateRangePicker.divElement.show();
                elements.sCost.divElement.show();
                elements.altCost.divElement.show();
                if (model.SDefaultValue) {
                    var dateRange = model.SDefaultValue.split(";");
                    var range = { start: kendo.parseDate(dateRange[0], "dd-MMM-yyyy"), end: kendo.parseDate(dateRange[1], "dd-MMM-yyyy") };
                    elements.dateRangePicker.input.range(range);
                }
                break;
            case patCECountrySetupPage.CostDataType.NUMERIC:
                if (mainDataItem && mainDataItem.CostType && mainDataItem.CostType.toLowerCase() === "translation") {
                    elements.useCostFactorSwitch.divElement.show();
                }
                if (model.SUseCostFactor) {
                    elements.costFactorSettings.divElement.show();
                    elements.translationType.divElement.show();

                    elements.costFactor1.input.value(model.SCostFactor1);
                    elements.costFactor2.input.value(model.SCostFactor2);
                    elements.costFactor3.input.value(model.SCostFactor3);
                } else {
                    elements.numericOpt.divElement.show();
                    elements.numericValue.divElement.show();
                    elements.multCost.divElement.show();
                    elements.sCost.divElement.show();
                    elements.altCost.divElement.show();
                    
                    elements.numericOpt.input.value(model.SOpts);
                    elements.numericValue.input.value(model.SDefaultValue);                    
                }
                break;
            case patCECountrySetupPage.CostDataType.BOOLEAN:
                elements.boolDropDown.divElement.show();
                elements.sCost.divElement.show();
                elements.altCost.divElement.show();
                elements.boolDropDown.input.value(model.SDefaultValue);
                break;
        }

        // --- BINDING EVENTS ---
        // Bind the change event for the SDataType dropdown
        var sDataTypeDropDownList = editorContainer.find("#SDataType").data("kendoDropDownList");
        sDataTypeDropDownList.bind("change", function(event) {
            patCECountrySetupPage.subGridEditorDataTypeChange({
                data: {
                    sender: event.sender,
                    mainDataItem: mainDataItem,
                    editorContainer: editorContainer
                }
            });
        });        

        // Bind TranslationType input change handler
        elements.translationType.input.bind("change", function(event) {
            patCECountrySetupPage.subGridEditorSTranslationTypeChange({
                data: {
                    selectedType: event.sender.text(),
                    model: model,
                    costFactor1NumericTextBox: elements.costFactor1.input,
                    costFactor1Divs: elements.costFactor1Divs.divElement
                }
            });
        });

        // Initial call to set translation type handler state
        const currentSTranslationType = model.STranslationType == patCECountrySetupPage.TranslationType.PAGES.Value ? patCECountrySetupPage.TranslationType.PAGES.Text : model.STranslationType == patCECountrySetupPage.TranslationType.WORDS.Value ? patCECountrySetupPage.TranslationType.WORDS.Text : patCECountrySetupPage.TranslationType.PAGES.Text;        
        patCECountrySetupPage.subGridEditorSTranslationTypeChange({
            data: {
                selectedType: currentSTranslationType,
                model: model,
                costFactor1NumericTextBox: elements.costFactor1.input,
                costFactor1Divs: elements.costFactor1Divs.divElement
            }
        });

        // Bind 'use cost factor' switch change handler ---
        editorContainer.find('input[name="SUseCostFactor"]').on("change", function() {
            if ($(this).is(":checked")) {
                elements.costFactorSettings.divElement.show();
                elements.translationType.divElement.show();

                elements.numericOpt.divElement.hide();
                elements.numericValue.divElement.hide();
                elements.multCost.divElement.hide();
                elements.sCost.divElement.hide();
                elements.altCost.divElement.hide();
            } else {
                elements.numericOpt.input.select(0);
                elements.numericOpt.divElement.show();
                elements.numericValue.divElement.show();
                elements.multCost.divElement.show();
                elements.sCost.divElement.show();
                elements.altCost.divElement.show();

                elements.costFactorSettings.divElement.hide();
                elements.translationType.divElement.hide();
            }
        });   
    }

    subGridSave(e) {        
        var editorContainer = $(e.container);
        var model = e.model;

        var elements = {
            datePicker: { input: editorContainer.find("#DefaultValueDateSub").data("kendoDatePicker") },
            dateRangePicker: { input: editorContainer.find("#DefaultValueDateRangeSub").data("kendoDateRangePicker") },
            numericValue: { input: editorContainer.find("#DefaultValueNumericSub").data("kendoNumericTextBox") },
            boolDropDown: { input: editorContainer.find("#DefaultValueBoolSub").data("kendoDropDownList") },            
            numericOpt: { input: editorContainer.find("#NumericOptSub").data("kendoDropDownList") },
            costFactor1: { input: editorContainer.find("#SCostFactor1").data("kendoNumericTextBox") },
            costFactor2: { input: editorContainer.find("#SCostFactor2").data("kendoNumericTextBox") },
            costFactor3: { input: editorContainer.find("#SCostFactor3").data("kendoNumericTextBox") },
            useCostFactorSwitch: { input: editorContainer.find("#SUseCostFactor") }
        };
    
        switch (model.SDataType) {
            case patCECountrySetupPage.CostDataType.STRING:
                model.set("SDefaultValue", "");
                break;
            case patCECountrySetupPage.CostDataType.DATE:
                if (elements.datePicker.input) {
                    model.set("SDefaultValue", kendo.toString(elements.datePicker.input.value(), "dd-MMM-yyyy"));
                }
                break;
            case patCECountrySetupPage.CostDataType.DATERANGE:
                var range = elements.dateRangePicker.input ? elements.dateRangePicker.input.range() : null;
                var dateRangeString = "";
                if (range && range.start && range.end) {
                    dateRangeString = kendo.toString(range.start, "dd-MMM-yyyy") + ";" + kendo.toString(range.end, "dd-MMM-yyyy");
                }
                model.set("SDefaultValue", dateRangeString);
                break;
            case patCECountrySetupPage.CostDataType.NUMERIC:
                if (elements.useCostFactorSwitch.input.is(":checked")) {
                    // Cost Factor is enabled
                    model.set("SDefaultValue", elements.costFactor1.input.value() + ", " + elements.costFactor2.input.value() + ", " + elements.costFactor3.input.value());
                    model.set("SOpts", "=");
                } else {
                    // Cost Factor is disabled
                    model.set("SDefaultValue", elements.numericValue.input.value());
                    model.set("SOpts", elements.numericOpt.input.value());
                    // Reset cost factor values if not used
                    model.set("SCostFactor1", 0);
                    model.set("SCostFactor2", 0);
                    model.set("SCostFactor3", 0);
                }
                break;
            case patCECountrySetupPage.CostDataType.BOOLEAN:
                if (elements.boolDropDown.input) {
                    model.set("SDefaultValue", elements.boolDropDown.input.value());
                }
                break;
        }
        
        var isNumericAndCostFactor = (model.SDataType == 3 && model.SUseCostFactor);

        if (isNumericAndCostFactor) {
            if (!elements.costFactor1.input.value() || elements.costFactor1.input.value() === 0) {
                e.preventDefault();
                alert('Estimated number of words per page is required.');
            } else if (!elements.costFactor2.input.value() || elements.costFactor2.input.value() === 0) {
                e.preventDefault();
                alert('Estimated fee is required.');
            } else if (!elements.costFactor3.input.value() || elements.costFactor3.input.value() === 0) {
                e.preventDefault();
                alert('Estimated fee per word/page is required.');
            }
        } else if (model.SDataType != patCECountrySetupPage.CostDataType.STRING && (model.SDefaultValue === "" || model.SDefaultValue === null)) {
            e.preventDefault();
            alert('Default value is required.');
        }

        if (!isNumericAndCostFactor) {
            model.set("SUseCostFactor", 0);
            model.set("SCostFactor1", 0);
            model.set("SCostFactor2", 0);
            model.set("SCostFactor3", 0);
        }

        if (!model.SCost) {
            model.SCost = 0;
        }
    }

    subGridDataBound(e) {
        var grid = this;
        var columns = e.sender.columns;
        var columnIndex = grid.wrapper.find(".k-grid-header [data-field=" + "SDefaultValue" + "]").index();
        grid.tbody.find("tr[role='row']").each(function () {
            var row = grid.dataItem(this);
            //if (row.SCPICost) {
            //    $(this).find(".k-grid-edit").attr("hidden", true);
            //    $(this).find(".k-grid-delete").attr("hidden", true);
            //}
                        
            if (row.SDataType == patCECountrySetupPage.CostDataType.NUMERIC) {
                var cell = $(this).children().eq(columnIndex);
                cell.html(row.SOpts + " " + row.SDefaultValue);
            }
        });
    }

    costSubDataTypeData = (e) => {
        var dataTypeDD = $("#SDataType").data("kendoDropDownList");       
        var parentRow = dataTypeDD.wrapper.closest("tr.k-detail-row").prev("tr");
        var parentGrid = parentRow.closest("[data-role=grid]").data("kendoGrid");
        var parentData = parentGrid.dataItem(parentRow);
        
        return {
            parentDataType: parentData.DataType
        };
    }    

    subGridEditorSTranslationTypeChange(e) {
        var model = e.data.model;
        var costFactor1NumericTextBox = e.data.costFactor1NumericTextBox;
        var costFactor1Divs = e.data.costFactor1Divs;
        
        switch (e.data.selectedType.toLowerCase()) {
            case 'pages':
                costFactor1Divs.show();
                break;
            case 'words':
                costFactor1Divs.hide();            
                if (costFactor1NumericTextBox) {
                    costFactor1NumericTextBox.value(1);
                    model.SCostFactor1 = 1;
                }
                break;
            default:
                break;
        }
    }

    subGridEditorDataTypeChange(e) {
        var mainDataItem = e.data.mainDataItem;
        var container = e.data.editorContainer;
        var selectedType = e.data.sender.text();

        var elements = {
            datePicker: { divElement: container.find('#datePickerDiv') },
            dateRangePicker: { divElement: container.find('#dateRangePickerDiv') },
            numericOpt: { divElement: container.find('#numericOptDiv') },
            numericValue: { divElement: container.find('#numericValueDiv') },
            boolDropDown: { divElement: container.find('#booleanDiv') },
            sCost: { divElement: container.find('#costDiv') },
            altCost: { divElement: container.find('#altCostDiv') },
            multCost: { divElement: container.find('#multCostDiv') },
            costFactorSettings: { divElement: container.find('#costFactorSettings') },
            translationType: { divElement: container.find('#translationTypeDiv') },
            useCostFactorSwitch: { divElement: container.find('#useCostFactorSwitch') }
        };

        patCECountrySetupPage.hideAllElements(elements);

        switch (selectedType) {
            case 'String':
                elements.sCost.divElement.show();
                break;
            case 'Date':
                elements.datePicker.divElement.show();
                break;
            case 'Date Range':
                elements.dateRangePicker.divElement.show();
                elements.altCost.divElement.show();
                elements.sCost.divElement.show();
                break;
            case 'Numeric':
                if (mainDataItem && mainDataItem.CostType.toLowerCase() === "translation") {
                    elements.useCostFactorSwitch.divElement.show();
                }
                if (container.find('input[name="SUseCostFactor"]').is(':checked')) {
                    elements.costFactorSettings.divElement.show();
                    elements.translationType.divElement.show();
                } else {
                    elements.numericOpt.divElement.show();
                    elements.numericValue.divElement.show();
                    container.find('#NumericOptSub').data('kendoDropDownList').select(0);
                    elements.multCost.divElement.show();
                    elements.altCost.divElement.show();
                    elements.sCost.divElement.show();
                }
                break;
            case 'Boolean':
                elements.boolDropDown.divElement.show();
                elements.altCost.divElement.show();
                elements.sCost.divElement.show();
                break;
        }
    }
    //---------------------------------------------------------

    hideAllElements(elements) {
        for (var key in elements) {
            if (elements.hasOwnProperty(key) && elements[key].divElement && elements[key].divElement.length > 0) {
                elements[key].divElement.hide();
            }
        }
    }

    nestedGridRequestEnd(e) {
        if(e.type == 'update' || e.type == 'create' || e.type == 'destroy') {
            patCECountrySetupPage.updateRecordStamps(); 
            this.read(); 
            pageHelper.showSuccess(e.response.success); 
            
            var parentGrid = $('#patCECountryCostGrid').data('kendoGrid');
            parentGrid.dataSource.read();        
            
            var feeAsOfDP = $('#FeesEffDate_ceCountrySetupDetailsView').data('kendoDatePicker'); 
            if (feeAsOfDP) { 
                feeAsOfDP.value(new Date());
            }
        }
    }
}
