import ActivePage from "../activePage";

export default class PatCEAnnuitySetup extends ActivePage {

    constructor() {
        super();
    }

    tabChangeSetListener = () => {
        const self = this;
        $('#patCEAnnuitySetup-tab a').on('click',
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

    showCopyScreen() {
        const popupContainer = $(".site-content .popup").last();
        const dialogContainer = popupContainer.find("#patCEAnnuitySetupCopyDialog");
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

    getSetupCountry = () => {
        var country = $('#Country_ceAnnuitySetupDetailsView').data('kendoMultiColumnComboBox');
        if (country) {
            country = country.value();
        }
        else {
            country = $('#Country').val();
        }

        return { country: country }
    }
}
