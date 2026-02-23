import ActivePage from "../activePage";

export default class TmcWorkflow extends ActivePage {

    constructor() {
        super();
    }

    initialize = (screen, id) => {
        this.editableGrids = [            
            { name: "tmcWorkflowActionGrid", filter: { parentId: id }, afterSubmit: this.updateRecordStamps }           
        ];

        this.tabsLoaded = [];
        this.tabChangeSetListener();       
    }
   
    tabChangeSetListener = () => {
        const self = this;
        $('#tmcWorkflow-tab a').on('click',
            (e)=> {
                e.preventDefault();
                const tab = e.target.id;
                if (this.tabsLoaded.indexOf(tab) === -1) {
                    this.tabsLoaded.push(tab);
                    this.loadTabContent(tab);
                }
            });

        $(document).ready(function() {
            const actionsGrid = $("#tmcWorkflowActionGrid");
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
                const url = `${baseUrl}/SearchRequest/Workflow/AttachmentFilterLoad`;

                const row = $(e.target).closest("tr");
                const dataItem = actionsGrid.data("kendoGrid").dataItem(row);

                $.get(url, { actId: dataItem.ActId }).done((result) => {
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
            var triggerValue = $("#TriggerValueId_workflowSearch").data("kendoComboBox");
            triggerValue.dataSource.read({ triggerType: selectedItem });
        }
    }

    //----------------------------------------------------------------- trigger, trigger value
    getTriggerType() {
        return {
            triggerType: $("#Trigger").data("kendoDropDownList").value()
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

        var triggerValue = $("#TriggerValue").data("kendoDropDownList");
        triggerValue.dataSource.read({ triggerType: selectedItem.Value });
        triggerValue.value("0");
        triggerValue.trigger("change");
    }


    //------------------------------------------------------------------ action, action value
    actionTypeOnChange = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");
            const grid = $("#tmcWorkflowActionGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            dataItem.ActionTypeId = e.dataItem["Value"];

            dataItem.ActionValueId = -1;
            $(row).find(".action-value-field").html("");
        }
    }

    actionValueOnChange = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");
            const grid = $("#tmcWorkflowActionGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            dataItem.ActionValueId = e.dataItem["Value"];
        }
    }

    getActionType = (e) => {
        var grid = $("#tmcWorkflowActionGrid").data("kendoGrid");
        var selectedData = grid.dataItem(grid.select());

        return {
            actionTypeId: selectedData.ActionTypeId,
            request: e
        };
    }
}
