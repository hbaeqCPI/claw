import ActivePage from "../activePage";

export default class ReportSchedulerPage extends ActivePage {

    constructor() {
        super();
        this.image = window.image;
    }

    init(addMode) {
        this.tabsLoaded = [];
        this.tabChangeSetListener();
        actionAddSetUp();

        if (addMode) {
            const scheduleName = this.getKendoComboBox("ScheduleName");
            scheduleName.focus();

        }
        //this.initialize();
    }

    tabChangeSetListener = () => {
        $(document).ready(() => {

            const scheduleActionGrid = $("#scheduleActionGrid");
            scheduleActionGrid.find(".k-grid-toolbar").on("click",
                ".k-grid-AddNewAction",
                () => {
                    const parent = $("#scheduleActionGrid").parent();
                    const url = parent.data("url-add");
                    const grid = scheduleActionGrid.data("kendoGrid");
                    const data = {
                        taskId: parent.data("parentid"),
                    };
                    this.openActionEntry(grid, url, data, true);

                });
        });
        $('#criteriaTabContent a').on('click',
            (e) => {
                e.preventDefault();
                const tab = e.target.id;
                if (this.tabsLoaded.indexOf(tab) === -1) {
                    this.tabsLoaded.push(tab);
                    this.loadTabContent(tab);
                }
            });
        $('#recordsAffectedTab').on('click',
            (e) => {
                e.preventDefault();
                const tab = e.target.id;
                this.loadTabContent(tab);
            });
    }

    loadTabContent(tab) {
        switch (tab) {
            case "actionTab":
                $(document).ready(function () {
                    const grid = $("#scheduleActionGrid").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;
            case "printOptionTab":
                $(document).ready(function () {
                    const grid = $("#schedulePrintOptionGrid").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;
            case "criteriaTabContent":
                $(document).ready(function () {
                    const grid = $("#scheduleCriteriaGrid").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;
            case "recordsAffectedTab":
                $(document).ready(function () {
                    const grid = $("#scheduleRecordsAffectedGrid").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;
            case "historyTab":
                $(document).ready(function () {
                    const grid = $("#scheduleHistoryGrid").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;
            case "":
                break;
        }
    }

    onComboBoxSelect(e, name) {
        if (e.item) {
            const element = e.sender.element.closest('td').next('td');
            if (element.length > 0) {
                const value = e.dataItem[name];
                if (element.is("input"))
                    element.val(value);
                else
                    element.html(value);
            }
        }
    }

    refreshGrid = function (name) {
        const grid = $("#" + name);
        grid.data("kendoGrid").dataSource.read();
    };

    editActionRecord(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const parent = $("#scheduleActionGrid").parent();

        const url = parent.data("url-edit");

        const data = { ActionId: dataItem.ActionId };
        this.openActionEntry(grid, url, data, true);
    }

    openActionEntry(grid, url, data, closeOnSave) {
        const self = this;

        $.ajax({
            url: url,
            data: data,
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $("#rSActionEntryDialog");

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
                        afterSubmit: function () {
                            grid.dataSource.read();
                            //self.updateRecordStamps();
                            dialogContainer.modal("hide");
                        }
                    }
                );
            },
            error: function (error) {
                pageHelper.showErrors(error.responseText);
            }
        });
    }

    openHistoryRecord(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const parent = $("#scheduleHistoryGrid").parent();

        const url = parent.data("url-open");

        const data = { LogId: dataItem.LogId };
        this.openHistoryEntry(grid, url, data, true);
    }

    openHistoryEntry(grid, url, data, closeOnSave) {
        const self = this;

        $.ajax({
            url: url,
            data: data,
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $("#rSHistoryEntryDialog");

                let entryForm = dialogContainer.find("form")[0];
                dialogContainer.modal("show");
                entryForm = $(entryForm);
                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialogContainer,
                        closeOnSubmit: closeOnSave,
                        beforeSubmit: function () {
                        },
                        afterSubmit: function () {
                            grid.dataSource.read();
                            self.updateRecordStamps();
                            dialogContainer.modal("hide");
                        }
                    }
                );
            },
            //error: function (error) {
            //    pageHelper.showErrors(error.responseText);
            //}
        });
    }

    showCopyScreen() {
        //const popupContainer = $(".cpiContainerPopup").last();
        const popupContainer = $(".site-content .popup").last();
        const dialogContainer = popupContainer.find("#rSScheduleCopyDialog");
        let entryForm = dialogContainer.find("form")[0];
        dialogContainer.modal("show");
        const self = this;

        $(dialogContainer.find("#rSCopyButton")).click(function () { self.copySchedules(); });


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

    copySchedules() {
        const parent = $("#rSCopyBody");
        const pageId = "RSMainCopy";

            const param = new Object();
        param.CopyTaskId = parseInt($("#CopyTaskId").val());
        param.CopyScheduleName = $("#CopyScheduleName").val();
        param.CopySettings = $("#CopySettings").is(":checked");
        param.CopyActions = $("#CopyActions").is(":checked");
        param.CopyCriteria = $("#CopyCriteria").is(":checked");
        param.CopyPrintOptions = $("#CopyPrintOptions").is(":checked");
        if (param.CopyScheduleName != "") {
            if (param.CopySettings || param.CopyActions || param.CopyCriteria || param.CopyPrintOptions) {
                $.ajax({
                    url: parent.data("url-copy"),
                    data: { copy: param },
                    type: "POST",
                    headers: { "RequestVerificationToken": $("[name='__RequestVerificationToken']").val() },
                    success: function (result) {
                        if (result.NewID == "")
                            alert(result.Message);
                        else {
                            window.location.replace(parent.data("url-detail") + "/" + result.NewID);
                        }
                    },
                    error: function (error) {
                        pageHelper.showErrors(error.responseText);
                    }
                });
            }
            else
                alert("No data selected. No records copied.");
        } else
            alert("No new Schedule Name. No records copied.");
    }
}