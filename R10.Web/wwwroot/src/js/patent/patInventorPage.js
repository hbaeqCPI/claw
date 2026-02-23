import ActivePage from "../activePage";

export default class PatInventorPage extends ActivePage {

    constructor() {
        super();    

        this.contactLettersFilter = {};
        this.lastSelectedLetter = null;
    }

    initialize(inventorId, isLettersGridOn) {
        this.editableGrids = [
            {
                name: 'inventorAwardsGrid', isDirty: false, filter: { inventorId: inventorId }, afterSubmit: (e) => { this.updateRecordStamps(); pageHelper.handleEmailWorkflow(e); }
            },
            {
                name: 'inventorLettersGrid', isDirty: false, filter: this.getInventorLettersFilter,
                afterSubmit: this.updateRecordStamps             
            }
        ];
       
        $(document).ready(() => {  
            if (`${isLettersGridOn}` === 'true') {
                this.contactLettersFilter = { EntityType: "I", EntityId: inventorId, ContactId: 0 };
                $(`#inventorLettersContainer`).show();
                const lettersGrid = $("#inventorLettersGrid").data("kendoGrid");
                lettersGrid.dataSource.read();
            }

            const inventorAwardsGrid = $("#inventorAwardsGrid");
            inventorAwardsGrid.find(".k-grid-toolbar").on("click",
                    ".ShowPaymentDateUpdateScreen",
                    () => {
                        const parent = $("#inventorAwardsGrid").parent();
                        const url = parent.data("url-mass-update");
                        const grid = inventorAwardsGrid.data("kendoGrid");
                        const data = {
                            inventorId: parent.data("inventorid"),
                        };
                        this.openAwardMassUpdateEntry(grid, url, data, true);

                    });
        });
    }    

    LetSendAsOnSelect = (e) => {
        const LetSendAsDropDown = $("#LetSendAs").data("kendoDropDownList");
        var dataItem = LetSendAsDropDown.dataItem(e.item.index());
        $("#LetSendAsHidden").val(dataItem.Value);
    }

    getInventorLettersFilter = () => {
        return { contactLettersFilter: this.contactLettersFilter };
    }

    inventorAwardOnDataBound = () =>{
    document.querySelectorAll('.data-CaseNumber').forEach(element => {
        var currentDataItem = $("#inventorAwardsGrid").data("kendoGrid").dataItem($(element).closest("tr"));
        if (currentDataItem.AwardSource == "DMS") {
            element.children[0].setAttribute("href", element.children[0].getAttribute("href").replace("Patent/CountryApplication/DetailLink","DMS/Disclosure/Detail"));
        }
    })

    }

    emailAwardGridRow = (e, grid, afterEmail) => {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        e.preventDefault();
        const parent = $("#" + e.delegateTarget.id).parent();
        const url = parent.data("url-email")+"?id="+dataItem.AwardId + "&awardSource="+dataItem.AwardSource;

        $.ajax({
            url: url,
            type: "Get",
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);

                if (afterEmail)
                    afterEmail(result);
            },
            error: function (e) {
                showErrors(e);
            }
        });
    }

    inventorAwardsSearchParam = () => {
        const searchInventorId = document.getElementById("SearchInventorId").value;
        const searchSystemType = document.getElementById("SearchSystemType_patInventorDetails").value;
        const searchCaseNumber = document.getElementById("SearchCaseNumber_patInventorDetails").value;
        const searchCountry = document.getElementById("SearchCountry_patInventorDetails").value;
        const searchSubCase = document.getElementById("SearchSubCase_patInventorDetails").value;
        const searchAmountFrom = document.getElementById("SearchAmountFrom").value;
        const searchAmountTo = document.getElementById("SearchAmountTo").value;
        const searchAwardType = document.getElementById("SearchAwardType_patInventorDetails").value;
        const searchAwardDateFrom = document.getElementById("SearchAwardDateFrom_patInventorDetails").value !== "" ? new Date(document.getElementById("SearchAwardDateFrom_patInventorDetails").value) : "";
        const searchAwardDateTo = document.getElementById("SearchAwardDateTo_patInventorDetails").value !== "" ? new Date(document.getElementById("SearchAwardDateTo_patInventorDetails").value) : "";
        const searchPaymentDateFrom = document.getElementById("SearchPaymentDateFrom_patInventorDetails").value !== "" ? new Date(document.getElementById("SearchPaymentDateFrom_patInventorDetails").value) : "";
        const searchPaymentDateTo = document.getElementById("SearchPaymentDateTo_patInventorDetails").value !== "" ? new Date(document.getElementById("SearchPaymentDateTo_patInventorDetails").value) : "";
        return {
            SearchInventorId: searchInventorId,
            SearchSystemType: searchSystemType,
            SearchCaseNumber: searchCaseNumber,
            SearchCountry: searchCountry,
            SearchSubCase: searchSubCase,
            SearchAmountFrom: searchAmountFrom,
            SearchAmountTo: searchAmountTo,
            SearchAwardType: searchAwardType,
            SearchAwardDateFrom: searchAwardDateFrom,
            SearchAwardDateTo: searchAwardDateTo,
            SearchPaymentDateFrom: searchPaymentDateFrom,
            SearchPaymentDateTo: searchPaymentDateTo
        };
    };

    InventorAwardsGridRead = () => {
        if ($("#inventorAwardsGrid") === "undefined") {
            const inventorAwardsGrid = $("#inventorAwardsGrid").data("kendoGrid");
            inventorAwardsGrid.dataSource.read();
        }
    };

    openAwardMassUpdateEntry(grid, url, data, closeOnSave) {
        const self = this;

        $.ajax({
            url: url,
            data: data,
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $("#inventorAwardMassUpdateEntryDialog");

                let entryForm = dialogContainer.find("form")[0];
                dialogContainer.modal("show");
                entryForm = $(entryForm);
                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialogContainer,
                        closeOnSubmit: closeOnSave,
                        beforeSubmit: function () {
                            //const parentStamp = self.getParentTStamp();
                            //dialogContainer.find("#ParentTStamp").val(parentStamp);
                        },
                        afterSubmit: function (e) {
                            grid.dataSource.read();
                            self.updateRecordStamps();
                            dialogContainer.modal("hide");

                            if (e.emailWorkflows) {
                                const promise = pageHelper.handleEmailWorkflow(e);
                                promise.then(() => {
                                });
                            }
                        }
                    }
                );
            },
            //error: function (error) {
            //    pageHelper.showErrors(error.responseText);
            //}
        });
    }
}





