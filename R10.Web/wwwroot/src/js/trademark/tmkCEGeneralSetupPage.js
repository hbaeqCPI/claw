import ActivePage from "../activePage";

export default class TmkCEGeneralSetup extends ActivePage {

    constructor() {
        super();
    }
    
    tabChangeSetListener = () => {
        const self = this;
        $('#tmkCEGeneralSetup-tab a').on('click',
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
    showCopyScreen() {
        const popupContainer = $(".site-content .popup").last();
        const dialogContainer = popupContainer.find("#tmkCEGeneralSetupCopyDialog");
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

    costSetupDataTypeChange = (e) => {
        var selectedType = $("#" + e.sender.element[0].id).data("kendoDropDownList").text();

        var datePicker = document.getElementById('guideDatePicker');
        var dateRangePicker = document.getElementById('guideDateRangePicker');
        var boolBox = document.getElementById('guideBoolean');
        var altCostBox = document.getElementById('guideAltCost');
        var defaultCostBox = document.getElementById('guideDefaultCost');
        var numBox = document.getElementById('guideNumeric');
        var multCostBox = document.getElementById('guideMultCost');

        datePicker.style.display = 'none'
        dateRangePicker.style.display = 'none';
        boolBox.style.display = 'none'
        numBox.style.display = 'none';
        multCostBox.style.display = 'none';
        altCostBox.style.display = 'none';
        defaultCostBox.style.display = 'none';

        if (selectedType == 'String') {
            defaultCostBox.style.display = 'block';
        }
        else if (selectedType == 'Date') {
            datePicker.style.display = 'block'
        }
        else if (selectedType == 'Date Range') {
            dateRangePicker.style.display = 'block';
            altCostBox.style.display = 'block';
            defaultCostBox.style.display = 'block';
        }
        else if (selectedType == 'Numeric') {
            numBox.style.display = 'contents';
            $('#NumericOpt').data('kendoDropDownList').select(0);
            multCostBox.style.display = 'block';
            altCostBox.style.display = 'block';
            defaultCostBox.style.display = 'block';
        }
        else if (selectedType == 'Boolean') {
            boolBox.style.display = 'block'
            altCostBox.style.display = 'block';
            defaultCostBox.style.display = 'block';
        }
    }
}
