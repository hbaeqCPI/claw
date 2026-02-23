import ActivePage from "../activePage";

export default class AgentPage extends ActivePage {

    constructor() {
        super();

        this.contactLettersFilter = {};
        this.currentLetterEntryContact = 0;
        this.agentContactLetterEditMode = false;
        this.lastSelectedContactRow = null;
        this.lastSelectedContact = null;
        this.agentContactLettersContainer = "agentContactLetters";
        this.lastSelectedLetter = null;
        this.TranslationType = {
            PAGES: { Text: "Pages", Value: 0 },
            WORDS: { Text: "Words", Value: 1 }
        }
    }

    initialize(agentId) {
        this.editableGrids = [
            {
                name: 'agentContactsGrid', isDirty: false, filter: { agentId: agentId }, afterSubmit: this.updateRecordStamps
            },
            {
                name: 'agentContactLettersGrid', isDirty: false, filter: this.getContactLettersFilter,
                afterSubmit: this.contacts_resetEditedFlag,
                onDirty: () => { this.agentContactLetterEditMode = true; },
                onCancel: this.contacts_resetEditedFlag
            }
        ];

        $(document).ready(() => {
            const agentContactsGrid = $("#agentContactsGrid");

            agentContactsGrid.on("click", ".contactLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = agentContactsGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.ContactID);
                pageHelper.openLink(linkUrl, false);
            });

            agentContactsGrid.on("click", ".letter-entry", (e) => {
                e.stopPropagation();

                const element = $(e.target);
                const agentId = element.data("agent-id");
                const contactId = element.data("contact-id");

                if (!this.agentContactLetterEditMode) {
                    const parentRow = element.parents("tr.k-state-selected");
                    if (parentRow) {
                        parentRow.addClass("contact-selected");
                        this.lastSelectedContactRow = parentRow;
                    }

                    this.currentLetterEntryContact = contactId;
                    this.contacts_ShowLetterTypeEntry(agentId, contactId);
                }

            });
        });
    }

    valueMapper = (options, url) => {
        if (!url)
            url = $("body").data("base-url") + "/Shared/Agent/ValueMapper";

        $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }

    deleteContactRow = (e,row) => {
        if (!this.agentContactLetterEditMode) {
            this.deleteGridRow(e, row);
        }
    }

    contacts_ShowLetterTypeEntry = (agentId, contactId) => {
        $(`#${this.agentContactLettersContainer}`).show();
        this.contactLettersFilter = { EntityType: "A", EntityId: agentId, ContactId: contactId };
        const agentContactLettersGrid = $("#agentContactLettersGrid").data("kendoGrid");
        agentContactLettersGrid.dataSource.read();
    }

    contacts_onRowSelect = (row) => {
        if (!this.agentContactLetterEditMode) {
            $(`#${this.agentContactLettersContainer}`).hide();
            //const data = row.sender.dataItem(row.sender.select());
            //if (data.GenAllLetters !== 2) {
            //}
        }
        else {
            $(this.lastSelectedContactRow).addClass("k-state-selected");
        }
    }

    contacts_updateName = (property, value,id) => {
        this.lastSelectedContact[property] = value;
        this.lastSelectedContact.dirty = true;
        if (id > 0)
            this.lastSelectedContact.ContactID = id;
    }

    contacts_onEdit = (e) => {
        const selected = e.sender.select();
        if (selected && selected.length > 0)
            this.lastSelectedContact = e.sender.dataItem(selected);
        else
            this.lastSelectedContact = e.sender.dataSource._data[0];

        if (this.agentContactLetterEditMode) {
            e.sender.closeCell();
            e.preventDefault();
        }
    }

    letters_onEdit = (e) => {
        const selected = e.sender.select();
        if (selected && selected.length > 0)
            this.lastSelectedLetter = e.sender.dataItem(selected);
        else
            this.lastSelectedLetter = e.sender.dataSource._data[0];
    }

    letters_updateName = (property, value) => {
        this.lastSelectedLetter[property] = value;
        //this.lastSelectedContact.dirty = true;
        
    }

    //hide letter settings if contact is deleted
    contacts_onRemove() {
        $("#agentContactLetters").hide();
    }

    contacts_resetEditedFlag = () => {
        this.agentContactLetterEditMode = false;
        this.currentLetterEntryContact = 0;
    }

    getContactLettersFilter = () => {
        return { contactLettersFilter: this.contactLettersFilter };
    }

    // Cost Estimator Fee grid
    agentCEFeesGridEdit(e) {
        var editorContainer = $(e.container);
        var model = e.model;
                
        var elements = {            
            countryComboBox: { input: editorContainer.find("#Country").data("kendoComboBox") },
            systemTypeComboBox: { input: editorContainer.find("#SystemType").data("kendoComboBox") },
            costTypeComboBox: { input: editorContainer.find("#CostType").data("kendoComboBox") },
            currencyTypeComboBox: { input: editorContainer.find("#CurrencyType").data("kendoComboBox") },
            costFactorSettings: { divElement: editorContainer.find('#costFactorSettings') },            
            costFactor1: { input: editorContainer.find("#CostFactor1").data("kendoNumericTextBox") },
            costFactor2: { input: editorContainer.find("#CostFactor2").data("kendoNumericTextBox") },
            costFactor3: { input: editorContainer.find("#CostFactor3").data("kendoNumericTextBox") },            
            costFactor1Divs: { divElement: editorContainer.find(".costFactor1Div") },
            originatingLanguage: { divElement: editorContainer.find(".originatingLanguageDiv") },
            translationType: { input: editorContainer.find("#TranslationType").data("kendoDropDownList"), divElement: editorContainer.find("#translationTypeDiv") },
            amount: { divElement: editorContainer.find(".amountDiv") }
        };

        // --- 1. INITIAL RECORD STATE ---
        if (model.FeeID > 0) {            
            editorContainer.find('input[name="DateCreated"]').val(kendo.toString(model.DateCreated, "dd-MMM-yyyy hh:mm tt"));
            editorContainer.find('input[name="LastUpdate"]').val(kendo.toString(model.LastUpdate, "dd-MMM-yyyy hh:mm tt"));
        }

        // --- 2. SET VISIBILITY BASED ON DATA TYPE ---
        patCECountrySetupPage.hideAllElements(elements); // Hide all elements first
        
        if (model.CostType && model.CostType.toLowerCase() === "translation") {
            elements.costFactorSettings.divElement.show();
            elements.originatingLanguage.divElement.show();
            elements.translationType.divElement.show();

            if (elements.costFactor1.input) elements.costFactor1.input.value(model.CostFactor1);
            if (elements.costFactor2.input) elements.costFactor2.input.value(model.CostFactor2);
            if (elements.costFactor3.input) elements.costFactor3.input.value(model.CostFactor3);                    
        }
        else {
            elements.amount.divElement.show();
        }
    
        // --- 3. BIND EVENT HANDLERS AND INITIALIZE COMPONENTS ---
        // Bind CostType input change handler        
        elements.costTypeComboBox.input.bind("change", function(event) {
            agentPage.agentCEFeesGridEditorCostTypeChange({
                data: {
                    sender: event.sender,                    
                    editorContainer: editorContainer
                }
            });
        });

        // Bind Country input change handler        
        elements.countryComboBox.input.bind("change", function(event) {
            agentPage.agentCEFeesGridEditorCountryChange({
                data: {
                    sender: event.sender,                    
                    editorContainer: editorContainer
                }
            });
        });

        // Bind TranslationType input change handler
        elements.translationType.input.bind("change", function(event) {
            agentPage.agentCEFeesGridEditorTranslationTypeChange({
                data: {
                    selectedType: event.sender.text(),
                    model: model,
                    costFactor1NumericTextBox: elements.costFactor1.input,
                    costFactor1Divs: elements.costFactor1Divs.divElement
                }
            });
        });

        // Initial call to set translation type handler state
        const currentTranslationType = model.TranslationType == agentPage.TranslationType.PAGES.Value ? agentPage.TranslationType.PAGES.Text : model.TranslationType == agentPage.TranslationType.WORDS.Value ? agentPage.TranslationType.WORDS.Text : agentPage.TranslationType.PAGES.Text;        
        agentPage.agentCEFeesGridEditorTranslationTypeChange({
            data: {
                selectedType: currentTranslationType,
                model: model,
                costFactor1NumericTextBox: elements.costFactor1.input,
                costFactor1Divs: elements.costFactor1Divs.divElement
            }
        });
    }

    agentCEFeesGridSave(e) {
        var editorContainer = $(e.container);
        var model = e.model;
                
        var elements = {            
            costFactor1: { input: editorContainer.find("#CostFactor1").data("kendoNumericTextBox") },
            costFactor2: { input: editorContainer.find("#CostFactor2").data("kendoNumericTextBox") },
            costFactor3: { input: editorContainer.find("#CostFactor3").data("kendoNumericTextBox") },
            
        };

        if (model.SystemTypeName) {
            model.SystemType = model.SystemTypeName.toLowerCase() === "patent" ? "P" : model.SystemTypeName.toLowerCase() === "trademark" ? "T" : null;
        }
        else {
            model.SystemType = null;
        }

        if (model.CostType && model.CostType.toLowerCase() === 'translation') {
            model.set("Amount", 0);
        } else {            
            model.set("CostFactor1", 0);
            model.set("CostFactor2", 0);
            model.set("CostFactor3", 0);
            model.set("OriginatingLanguage", null);
        }        
    }

    agentCEFeesGridEditorTranslationTypeChange(e) {
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

    agentCEFeesGridEditorCostTypeChange(e) {
        var editorContainer = e.data.editorContainer;
        var selectedCostType = e.data.sender.text();
                
        var elements = {
            costFactorSettings: { divElement: editorContainer.find('#costFactorSettings') },
            originatingLanguage: { divElement: editorContainer.find(".originatingLanguageDiv") },
            translationType: { divElement: editorContainer.find('#translationTypeDiv') },
            amount: { divElement: editorContainer.find(".amountDiv") }
        };

        patCECountrySetupPage.hideAllElements(elements);

        if (selectedCostType && selectedCostType.toLowerCase() === "translation") {
            elements.costFactorSettings.divElement.show();
            elements.originatingLanguage.divElement.show();
            elements.translationType.divElement.show();
        }
        else {
            elements.amount.divElement.show();
        }
    }

    agentCEFeesGridEditorCountryChange(e) {
        var editorContainer = e.data.editorContainer;
        var selectedCountry = e.data.sender.text();
                
        var selectedDataItem = e.data.sender.dataItem();

        if (selectedDataItem && selectedDataItem.Description) {
            var currencyTypeComboBox = editorContainer.find('#CurrencyType').data('kendoComboBox');
            if (currencyTypeComboBox) {
                currencyTypeComboBox.value(selectedDataItem.Description);
                currencyTypeComboBox.trigger('change');
            }
        }
    }

    hideAllElements(elements) {
        for (var key in elements) {
            if (elements.hasOwnProperty(key) && elements[key].divElement && elements[key].divElement.length > 0) {
                elements[key].divElement.hide();
            }
        }
    }
    
}