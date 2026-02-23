export default class GMDelegationUtility {

    constructor() {
        this.screen = "gmDelegationUtility";
        this.previewContainerId = "updatePreviewContainer";
        this.previewContainer = null;
        this.criteriaUpdated = true;
        this.searchMode = "c";
    }

    getCriteria() {
        const form = $("#delUtilUpdateCriteriaForm");
        const data = pageHelper.formDataToJson(form, false);

        const userId = form.find(`#DelegatedUser_${this.screen}`).data('kendoComboBox').value();
        const groupId = form.find(`#DelegatedGroup_${this.screen}`).data('kendoComboBox').value();

        data.payLoad["UserId"] = userId;
        data.payLoad["GroupId"] = groupId;
        data.payLoad["Mode"] = this.searchMode;

        const userIdDelegate = form.find(`#DelegatedUserDelegate_${this.screen}`).data('kendoComboBox').value();
        const groupIdDelegate = form.find(`#DelegatedGroupDelegate_${this.screen}`).data('kendoComboBox').value();

        data.payLoad["UserIdDelegate"] = userIdDelegate;
        data.payLoad["GroupIdDelegate"] = groupIdDelegate;

        return data.payLoad;
    }

    refreshGrid = () => {
        if (this.criteriaUpdated) {
            const previewGrid = $('#delegationUtilityGrid').data('kendoGrid');
            previewGrid.dataSource.read();
            this.criteriaUpdated = false;
        }

    }

    initialize() {
        const self = this;

        $('#update-tab a').on('click', (e) => {
            const tab = e.target.id;

            if (tab.toLowerCase() === "records-tab") {
                self.refreshGrid();
            }
        });

        self.previewContainer = $(`#${this.previewContainerId}`);
        const previewContainerGrid = this.previewContainer.find("#delegationUtilityGrid");

        previewContainerGrid.on("change", "input[type='checkbox']", function () {
            self.pageSelection(this, this.checked);
        });
        self.previewContainer.on("change", "input[type='checkbox'].page-update", function () {
            const pageSelection = this.checked;
            previewContainerGrid.find("input[type='checkbox']").each(function () {
                self.pageSelection(this, pageSelection);
                $(this).prop("checked", pageSelection);
            });
        });
        const form = $("#delUtilUpdateCriteriaForm");
        form.on("change", "input", () => { self.criteriaUpdated = true; });

        $("input[name='UtilMode']").on("change", function () {
            self.searchMode = $(this).val();

            const previewGrid = $('#delegationUtilityGrid').data('kendoGrid');

            if (self.searchMode === "c") {
                form.find(".clear-criteria").removeClass("d-none");
                form.find(".delegate-criteria").addClass("d-none");

                previewGrid.showColumn("DelegatedUser");
                previewGrid.showColumn("DelegatedGroup");
            }
            else {
                form.find(".clear-criteria").addClass("d-none");
                form.find(".delegate-criteria").removeClass("d-none");

                previewGrid.hideColumn("DelegatedUser");
                previewGrid.hideColumn("DelegatedGroup");
            }

            self.criteriaUpdated = true;
            self.refreshGrid();
        });

        $("#delUtilRunUpdate").on('click', () => {
            const title = form.data("confirm-title");
            let msg = "";
            let delegateTo = [];
            let userIdDelegate = "";
            let groupIdDelegate = "";
            let reassignDelegate = false;

            if (self.searchMode === "c") {
                msg = form.data("clear-task-msg");
            }
            else {
                const delegateToMS = form.find(`#DelegateTo_${this.screen}`).data('kendoMultiSelect').value();
                if (delegateToMS.length === 0) {
                    pageHelper.showErrors(form.data("delegate-to-msg"));
                    return;
                }
                delegateTo = delegateToMS;
                msg = form.data("delegate-task-msg");

                userIdDelegate = form.find(`#DelegatedUserDelegate_${this.screen}`).data('kendoComboBox').value();
                groupIdDelegate = form.find(`#DelegatedGroupDelegate_${this.screen}`).data('kendoComboBox').value();
                reassignDelegate = form.find(`#ReassignDelegate_${this.screen}`).prop("checked");                

            }
            pageHelper.hideErrors();

            cpiConfirm.confirm(title, msg, function () {
                const selectedIds = self.getSelection();

                const triggerWorkflow = $("#delegation-util-mode").find("#TriggerWorkflow").prop("checked");                

                if (selectedIds.length > 0) {
                    const baseUrl = $("body").data("base-url");
                    const url = `${baseUrl}/GeneralMatter/GMDelegationUtility/RunUpdate`;

                    $.post(url, { updateMode: self.searchMode, selection: selectedIds, delegateTo: delegateTo, triggerWorkflow: triggerWorkflow, fromUser: userIdDelegate, fromGroup: groupIdDelegate, reassign: reassignDelegate })
                        .done(function (e) {
                            const completedMsg = form.data("completed-msg");
                            pageHelper.showSuccess(completedMsg);
                            
                            if (self.searchMode === "c") {
                                const previewGrid = $('#delegationUtilityGrid').data('kendoGrid');
                                previewGrid.dataSource.read();
                            }
                            pageHelper.handleEmailWorkflow(e);
                    })
                    .fail(function (e) {
                        pageHelper.showErrors(e.responseText);
                    });
                }
            });

        });
        
    }

    pageSelection = (checkbox, selection) => {
        const previewContainerGrid = this.previewContainer.find("#delegationUtilityGrid");
        const resultGrid = previewContainerGrid.data("kendoGrid");
        const data = resultGrid.dataSource.data();
        const id = $(checkbox).data("id");
        if (id) {
            const item = data.find(e => e.Id == id); 
            if (item) {
                item.Selected = selection;
            }
        }
    }

     getSelection = () => {
        const previewContainerGrid = this.previewContainer.find("#delegationUtilityGrid");
        const resultGrid = previewContainerGrid.data("kendoGrid");
        const selected = resultGrid.dataSource.data().filter(r => r.Selected).map(s => (
             { ActId: s.ActId, Id: s.Id }));
         
        return selected;
    }

}


