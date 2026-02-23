import ActivePage from "../activePage";

export default class QuickEmailSetup extends ActivePage {

    constructor() {
        super();
    }

    initialize = (id) => {
        this.handleSenderOptionsChange();
        this.handleSetupRecipient();
        this.editableGrids = [
            {
                name: 'qeSetupRecipientGrid',
                isDirty: false,
                filter: this.getKeyFields,
                afterSubmit: this.updateRecordStamps
            },
            {
                name: 'qeTagsGrid',
                isDirty: false,
                filter: this.getKeyFields,
                afterSubmit: this.updateRecordStamps
            },
        ];
    }

    tabChangeSetListener() {
        const self = this;
        self.tabsLoaded = [];

        $('#qeSetup-tab a').on('click', (e) => {
            e.preventDefault();
            self.detailActiveTabId = e.target.id;
            self.loadTabContent(self.detailActiveTabId);
        });
    }

    loadTabContent(tab) {

        const refreshGrid = function (gridId) {
            const gridHandle = $(gridId);
            const grid = gridHandle.data("kendoGrid");
            grid.dataSource.read();
        }

        switch (tab) {
            case "qeTagTab":
                $(document).ready(() => {
                    refreshGrid("#qeTagsGrid");
                });
                break;

            case "":
                break;
        }
    }

    getKeyFields = () => {
        const container = $(`#${this.detailContentContainer}`);
        return {
            qeSetupId: container.find("#QESetupID").val(),
            tStamp: container.find("input[name='tStamp']").val()
        };
    }

    handleSenderOptionsChange = () => {
        const senderContainer = $("#senderAddressContainer");
        senderContainer.on("change", function (e) {
            senderContainer.find("#fromAddress").toggleClass("d-none");
        });

        const replyToContainer = $("#replyToAddressContainer");
        replyToContainer.on("change", function (e) {
            replyToContainer.find("#replyToAddress").toggleClass("d-none");
        });
    }

    handleSetupRecipient = () => {
        const recipientsGrid = $("#qeSetupRecipientGrid");
        recipientsGrid.on("click", ".anchorCodeLink", (e) => {
            e.stopPropagation();

            let url = $(e.target).data("url");
            const row = $(e.target).closest("tr");
            const dataItem = recipientsGrid.data("kendoGrid").dataItem(row);
            const linkUrl = url.replace("actualValue", dataItem.AnchorCode);
            pageHelper.openLink(linkUrl, false);
        });
    }

    showDetails(id) {
        //return pageHelper.showDetails(this, id);
        this.showQEDetails(id);
    }

    recordNavigateHandler = (id) => {
        this.showQEDetails(id);
    }

    showQEDetails = (id) => {
        //get body nav active tab info before loading the next record
        const container = $(`#${this.detailContentContainer}`);
        const activeTabId = container.find(".body-nav .nav-link.active").attr("id");
        const activeContentPaneId = container.find(".body-nav .tab-content .tab-pane.active").attr("id");

        pageHelper.showDetails(this, id,
            () => {
                //restore prev body nav active tab
                if (activeTabId) {
                    container.find(".body-nav .nav-link").removeClass("active");
                    $(`#${activeTabId}`).addClass("active");

                    if (activeContentPaneId) {
                        container.find(".body-nav .tab-content .tab-pane").removeClass("active show");
                        $(`#${activeContentPaneId}`).addClass("active show");
                    }
                }
            });
    };

    onChange_SubjectDataFields = (e) => {
        const subject = $(`#${this.detailContentContainer}`).find("#Subject");

        // Insert data field to end of Subject
        subject.val(subject.val() + "{{" + e.sender.value() + "}}");
        subject.trigger("input");
    }

    getDataFieldFilter = () => {
        const dataSource = $(`#${this.detailContentContainer}`).find("input[name='DataSourceID']");
        if (dataSource.val() === "")
            return { dataSourceId: 0 };
        else
            return { dataSourceId: dataSource.val() };
    }

    editorModified = (e) => {
        let editorText = e.sender.value();
        editorText = editorText.replace(/href="(javascript.*?)"/, 'href=#');

        e.sender.value(editorText);
        $(this.entryFormInstance).trigger('markDirty');
    }

    onChange_DetailDataFields(e) {
        const editor = $("#Detail").data("kendoEditor");

        // Insert data field, preserve formatting
        editor.paste("{{" + e.sender.value() + "}}", { split: false });
    }

    showCopyScreen() {
        const popupContainer = $(".site-content .popup");
        const dialogContainer = popupContainer.find("#copyQuickEmailDialog");
        let entryForm = dialogContainer.find("form")[0];
        dialogContainer.modal("show");

        entryForm = $(entryForm);
        const afterSubmit = (result) => {
            const dataContainer = $(`#${this.mainDetailContainer}`).find(".cpiDataContainer");
            if (dataContainer.length > 0) {
                setTimeout(function () {
                    dataContainer.empty();
                    dataContainer.html(result);
                }, 1000);
            }
        };
        entryForm.cpiPopupEntryForm({ dialogContainer: dialogContainer, afterSubmit: afterSubmit, showSuccessMsg: false });
    }

    getDataSourceParam = (systemType) => {
        const container = $(`#${this.detailContentContainer}`);
        const param =  {
            systemType: systemType,
            screenId: container.find("input[name='ScreenId']").val()
        };
        console.log("test", param);
        return param;

    }
}





