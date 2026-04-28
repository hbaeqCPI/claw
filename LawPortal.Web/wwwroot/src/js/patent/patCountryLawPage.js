import ActivePage from "../activePage";

export default class PatCountryLaw extends ActivePage {

    constructor() {
        super();
    }

    tabChangeSetListener = () => {
        const self = this;
        $('#patCountryLaw-tab a').on('click',
            (e)=> {
                e.preventDefault();
                const tab = e.target.id;
                if (this.tabsLoaded.indexOf(tab) === -1) {
                    this.tabsLoaded.push(tab);
                    this.loadTabContent(tab);
                }
            });

        $(document).ready(function() {
            const countryDueGrid = $("#patCountryDueGrid");
            countryDueGrid.find(".k-grid-toolbar").on("click",
                ".k-grid-AddCountryDue",
                ()=> {
                    const parent = countryDueGrid.parent();
                    const url = parent.data("url-add");
                    const grid = countryDueGrid.data("kendoGrid");
                    const data = {
                        countryLawId: parent.data("countrylawid"),
                        country: parent.data("country"),
                        caseType: parent.data("casetype"),
                        systems: parent.data("systems")
                    };
                    self.openCountryDueEntry(grid, url, data, true);

                });
        });
    }

    

    loadTabContent(tab) {
        switch (tab) {
        case "countryLawDetailExpirationTab":
            $(document).ready(function() {
                const patCountryExpGrid = $("#patCountryExpGrid").data("kendoGrid");
                patCountryExpGrid.dataSource.read();
            });
            break;

        case "countryLawDetailDesigCountriesTab":
            $(document).ready(function() {
                const patDesCaseTypeGrid = $("#patDesCaseTypeGrid").data("kendoGrid");
                patDesCaseTypeGrid.dataSource.read();
            });
            break;

        case "countryLawDetailWebLinksTab":
            $(document).ready(function() {
                const webLinksGrid = $("#countryLawDetailWebLinksTabContent .webLinksGrid").data("kendoGrid");
                webLinksGrid.dataSource.read();
            });
            break;

        case "":
            break;
        }
    }
    
    editCountryDueRecord(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const parent = grid.element.parent();
        const url = parent.data("url-edit");

        const data = { cDueId: dataItem.CDueId };
        this.openCountryDueEntry(grid, url, data, true);
    }

    copyCountryDueRecord(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const parent = grid.element.parent();
        const url = parent.data("url-copy");

        const data = { cDueId: dataItem.CDueId };
        this.openCountryDueEntry(grid, url, data, true);
    }

    openCountryDueEntry(grid, url, data, closeOnSave) {
        const self = this;         
        $.ajax({
            url: url,
            data: data,
            success: function(result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $("#patCountryDueEntryDialog");

                let entryForm = dialogContainer.find("form")[0];
                dialogContainer.modal("show");
                entryForm = $(entryForm);
                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialogContainer,
                        closeOnSubmit: closeOnSave,
                        beforeSubmit: function () {
                            const parentStamp = self.getParentTStamp();
                            dialogContainer.find("#ParentTStamp").val(parentStamp);
                        },
                        afterSubmit: function() {
                            grid.dataSource.read();
                            self.updateRecordStamps();
                            dialogContainer.modal("hide");
                        }
                    }
                );
                entryForm.floatLabels();
            },
            error: function(error) {
                pageHelper.showErrors(error.responseText);
            }
        });
    }

    addMoreKeyFields = (dataItem) => {
        const container = $(`#${this.detailContentContainer}`);

        const countryLawId = container.find("input[name='CountryLawID']");
        const tStamp = container.find("input[name='tStamp']");

        dataItem.CountryLawID = null;
        dataItem.CountryLawID = parseInt(countryLawId.val());
        dataItem.ParentTStamp = tStamp.val();
    }

    getKeyFields = () => {
        const container = $(`#${this.detailContentContainer}`);
        const countryLawId = container.find("input[name='CountryLawID']");
        const country = container.find("input[name='Country']");
        const caseType = container.find("input[name='CaseType']");
        const tStamp = container.find("input[name='tStamp']");
        return {
            parentId: parseInt(countryLawId.val()),
            country: country.val(),
            caseType: caseType.val(),
            tStamp: tStamp.val()
        };
    }

    getParentTStamp = () => {
        const container = $(`#${this.detailContentContainer}`);
        const tStamp = container.find("input[name='tStamp']");
        return tStamp.val();
    }

    getCountry = () => {
        const country = $("#patCountryDueEntryDialog").find("input[name='Country']");
        const cDueId = $("#patCountryDueEntryDialog").find("input[name='CDueId']");
        
        return { country: country.val(), cDueId: cDueId.val()};
    }

    reComputeCountryLawActions(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const parent = grid.element.parent();
        const url = parent.data("url-compute");

        const data = { cDueId: dataItem.CDueId };
        this.openComputeScreen(url, data, true);
    }

    openComputeScreen(url, data, closeOnSave) {
        $.ajax({
            url: url,
            data: data,
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $("#patCountryDueComputeDialog");

                let entryForm = dialogContainer.find("form")[0];
                dialogContainer.modal("show");
                entryForm = $(entryForm);
                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialogContainer,
                        closeOnSubmit: closeOnSave,
                        beforeSubmit: function() {
                            $(".modal-footer-buttons").hide();
                            $(".modal-footer-status").removeClass("d-none");
                        },
                        afterSubmit: function () {
                            dialogContainer.modal("hide");
                        }
                    }
                );
                entryForm.floatLabels();
            },
            error: function (error) {
                pageHelper.showErrors(error.responseText);
            }
        });
    }
}