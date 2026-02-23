import ActivePage from "../activePage";

export default class PatWorkflowPage extends ActivePage {
    constructor() {
        super();
    }

    initialize = (screen, id) => {
        this.editableGrids = [            
            { name: "patWorkflowActionGrid", filter: { parentId: id }, afterSubmit: this.updateRecordStamps },
            { name: "patWorkflowActionParametersGrid", filter: { parentId: id }, afterSubmit: this.updateRecordStamps }           
        ];

        this.tabsLoaded = [];
        this.tabChangeSetListener();       
    }
   
    tabChangeSetListener = () => {
        const self = this;
        $('#patWorkflow-tab a').on('click',
            (e)=> {
                e.preventDefault();
                const tab = e.target.id;
                if (this.tabsLoaded.indexOf(tab) === -1) {
                    this.tabsLoaded.push(tab);
                    this.loadTabContent(tab);
                }
            });

        $(document).ready(function() {
            const actionsGrid = $("#patWorkflowActionGrid");
            actionsGrid.on("click", ".workFlowActionLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = actionsGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.ActionValueId);
                pageHelper.openLink(linkUrl, false);
            });
            actionsGrid.on("click", ".attachment-filter-marker", (e) => {
                e.stopPropagation();

                const baseUrl = $("body").data("base-url");
                const url = `${baseUrl}/Patent/Workflow/AttachmentFilterLoad`;

                const row = $(e.target).closest("tr");
                const dataItem = actionsGrid.data("kendoGrid").dataItem(row);

                $.get(url, {actId: dataItem.ActId}).done((result) => {
                    if (result) {
                        $(".cpiContainerPopup").empty();
                        const popupContainer = $(".cpiContainerPopup").last();
                        popupContainer.html(result);
                        const dialog = $("#attachmentFilterDialog");
                        dialog.modal("show");
                        dialog.floatLabels();

                        let entryForm = dialog.find("form")[0];
                        entryForm = $(entryForm);
                        entryForm.cpiPopupEntryForm(
                            {
                                dialogContainer: dialog,
                                afterSubmit: () => {
                                    dialog.modal("hide");
                                }
                            }
                        );
                    }

                }).fail(function (error) {
                    pageHelper.showErrors(error.responseText);
                });

            });
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

    triggerTypeSearchOnChange = (e) => {
        console.log(e);
        var selectedItem = e.sender.value();
        if (selectedItem) {
            var triggerValue = $("#TriggerTypeId_patWorkflowSearch").data("kendoComboBox");
            triggerValue.dataSource.read({ triggerType: selectedItem });
        }
    }
    
    //----------------------------------------------------------------- trigger, trigger value
    getTriggerType() {
        return {
            triggerType: $("#Trigger").data("kendoDropDownList").value(),
            //screenType: $(`#${this.mainDetailContainer}`).find("#ScreenName").val()
            screenType: $("#patWorkflowDetailsView").find("#ScreenName").val()
        };
    }

    screenTypeOnChange = (e) => {
        const triggerType = $("#Trigger").data("kendoDropDownList").value();

        //email sent
        if (+triggerType === 14) {
            const screenType = $(`#ScreenId_${this.mainDetailContainer}`).data("kendoComboBox").text();
            $(`#${this.mainDetailContainer}`).find(`#ScreenName`).val(screenType);

            const triggerValue = $("#TriggerValue").data("kendoDropDownList");
            triggerValue.dataSource.read();
        }
    }

    getTriggerTypeIdForSearch = () => {
        return {
            triggerType: $("#TriggerTypeId_patWorkflowSearch").data("kendoComboBox").value()
        };
    }

    triggerValueOnChange = (e) => {
        var selectedItem = e.sender.dataItem();
        if (selectedItem) {
            $("#TriggerValueId").val(selectedItem.Value);
        }
    }

    triggerTypeOnChange = (e) => {
        var selectedItem = e.sender.dataItem();
        $("#TriggerTypeId").val(selectedItem.Value);

        //value entry = not new file uploaded, record deleted, attorney modified, US related case,
        //ActionDelegated+, inventor renumeration
        if (!(selectedItem.Value == 3 || selectedItem.Value == 5 || selectedItem.Value == 7 ||
            selectedItem.Value == 8 || selectedItem.Value == 9 || selectedItem.Value == 10 ||
            selectedItem.Value == 11 || selectedItem.Value == 12 || selectedItem.Value == 13 ||
            selectedItem.Value == 15 || selectedItem.Value == 22 || selectedItem.Value == 26)) {
            $("#trigger-value-id").removeClass("d-none");
            $("#trigger-value-name").addClass("d-none");

            var triggerValue = $("#TriggerValue").data("kendoDropDownList");
            triggerValue.dataSource.read({ triggerType: selectedItem.Value });
            triggerValue.value("0");
            triggerValue.trigger("change");
        }
        //no value entry = record delete, atty modified, US related case,ActionDelegated+,inventorrenumeration
        else if (selectedItem.Value == 5 || selectedItem.Value == 7 || selectedItem.Value == 8 ||
            selectedItem.Value == 9 || selectedItem.Value == 10 || selectedItem.Value == 11 ||
            selectedItem.Value == 12 || selectedItem.Value == 13 || selectedItem.Value == 15 || selectedItem.Value == 26) {
            $("#trigger-value-id").addClass("d-none");
            $("#trigger-value-name").addClass("d-none");
            
        }
        //new file upload
        else {
            $("#trigger-value-id").addClass("d-none");
            $("#trigger-value-name").removeClass("d-none");
        }

        //screen name = new file upload, recorddeleted, email sent
        if (selectedItem.Value == 3 || selectedItem.Value == 5 || selectedItem.Value == 14 || selectedItem.Value == 22) {
            $("#screen-name").removeClass("d-none");
            $(`#ScreenId_${this.mainDetailContainer}`).data("kendoComboBox").dataSource.read({ triggerType: selectedItem.Value});
        }
        else $("#screen-name").addClass("d-none");
    }


    //------------------------------------------------------------------ action, action value
    actionTypeOnChange = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");
            const grid = $("#patWorkflowActionGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            dataItem.ActionTypeId = e.dataItem["Value"];

            dataItem.ActionValueId = -1;
            $(row).find(".action-value-field").html("");
        }
    }

    actionValueOnChange = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");
            const grid = $("#patWorkflowActionGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            dataItem.ActionValueId = e.dataItem["Value"];
        }
    }

    getActionType = (e) => {
        const grid = $("#patWorkflowActionGrid").data("kendoGrid");
        const selectedData = grid.dataItem(grid.select());
        const screen = $(`#ScreenId_${this.mainDetailContainer}`).data("kendoComboBox");

        return {
            triggerType: $("#Trigger").data("kendoDropDownList").value(),
            screenType: screen ? screen.value() : 0,
            actionTypeId: selectedData.ActionTypeId,
            request: e
        };
    }
}
