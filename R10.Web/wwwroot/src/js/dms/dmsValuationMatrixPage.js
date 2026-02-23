import ActivePage from "../activePage";

export default class DMSValuationMatrixPage extends ActivePage {

    constructor() {
        super();
    }

    matrixRateGridDataBound(e) {
        dmsValuationMatrixPage.setGridReadOnlyRow(e);
        var grid = $(`#${e.sender.element[0].id}`).data("kendoGrid");
        const ratingSystem = $("#RatingSystem").val();  
        if (ratingSystem != "Numeric Range") {
            grid.hideColumn("WeightMax");
            grid.thead.find("[data-field='WeightMin']").html('Value');
        }
        if (ratingSystem == "") {
            grid.hideColumn("WeightMin");
        }          

        var items = e.sender.items();
        items.each(function (index) {
            var dataItem = grid.dataItem(this);
            if (dataItem.RateId == 0 || dataItem.CanEditWeight) {
                this.cells[2].className += "editable-cell";
                this.cells[3].className += "editable-cell";
            }
        })
    }

    matrixRateGridEdit(e) {
        if (e.model.fields["WeightMin"] && e.container.find("input[name='WeightMin']").length > 0) {
            var weightMinInput = e.container.find("input[name='WeightMin']");
            weightMinInput.on("input", function () { 
                dmsValuationMatrixPage.validateWeightInput(this);
            });
        }

        if (e.model.fields["WeightMax"] && e.container.find("input[name='WeightMax']").length > 0) {
            var weightMaxInput = e.container.find("input[name='WeightMax']");
            weightMaxInput.on("input", function () { 
                dmsValuationMatrixPage.validateWeightInput(this);
            });
        }
    }

    validateWeightInput(inputElement) {
        var value = $(inputElement).val();
        var maxLength = 1; // Default to 1 for alphabetic

        // Check if the input is numeric
        if (/^\d*$/.test(value)) { // Checks if the value consists only of digits
            maxLength = 2;
        }

        // Apply the maxLength attribute
        $(inputElement).attr("maxlength", maxLength);

        // Trim the value if it exceeds the maxLength
        if (value.length > maxLength) {
            $(inputElement).val(value.substring(0, maxLength));
        }
    }

    showCopyScreen() {
        const popupContainer = $(".site-content .popup").last();
        const dialogContainer = popupContainer.find("#dmsValuationMatrixCopyDialog");
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

    clientEntityFilterValueMapper = (options) => {
        var multiSelect = $("#ReviewerEntityFilterList_valuationMatrixDetail").data("kendoMultiSelect");
        const data = multiSelect.dataSource.data();

        var value = options.value;
        var dataTemp = [];
        value = $.isArray(value) ? value : [value];
        for (var idx = 0; idx < value.length; idx++) {
            var filteredData = data.find(c => c.ClientID === parseInt(value[idx]));
            if (filteredData) {
                dataTemp.push({ ClientID: filteredData.ClientID, ClientCode: filteredData.ClientCode });
            }            
        }

        setTimeout(function () { options.success(dataTemp); }, 100);        
    }

    areaEntityFilterValueMapper = (options) => {
        var multiSelect = $("#ReviewerEntityFilterList_valuationMatrixDetail").data("kendoMultiSelect");
        const data = multiSelect.dataSource.data();

        var value = options.value;
        var dataTemp = [];
        value = $.isArray(value) ? value : [value];
        for (var idx = 0; idx < value.length; idx++) {
            var filteredData = data.find(c => c.AreaID === parseInt(value[idx]));
            if (filteredData) {
                dataTemp.push({ AreaID: filteredData.AreaID, Area: filteredData.Area });
            }            
        }

        setTimeout(function () { options.success(dataTemp); }, 100);        
    }
}
