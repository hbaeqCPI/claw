import ActivePage from "../activePage";

export default class SharePointHelper extends ActivePage {

    constructor() {
        super();
        this.image = window.image;
    }

    RefreshGrid(gridName) {
        cpiLoadingSpinner.show();
        const grid = $("#" + gridName).data("kendoGrid");
        grid.dataSource.read();
        grid.refresh();
        cpiLoadingSpinner.hide();
    }

    ViewFile(e, grid) {
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + grid).data("kendoGrid").dataItem(row);
        var url = dataItem.ViewUrl;
        const a = document.createElement("a");
        document.body.appendChild(a);
        a.href = url;
        a.target = "_blank"
        a.click();
        setTimeout(() => {
            document.body.removeChild(a);
        }, 0);
    }

    ViewFileGallery(listName) {
        const dataItem = sharePointHelper.GetSelected(listName);
        var url = dataItem.ViewUrl;
        const a = document.createElement("a");
        document.body.appendChild(a);
        a.href = url;
        a.target = "_blank"
        a.click();
        setTimeout(() => {
            document.body.removeChild(a);
        }, 0);
    }

    EditFile(e, grid) {
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + grid).data("kendoGrid").dataItem(row);
        var url = dataItem.EditUrl.replace("action=interactivepreview", "action=edit");
        const a = document.createElement("a");
        document.body.appendChild(a);
        a.href = url;
        a.target = "_blank";
        a.click();
        setTimeout(() => {
            document.body.removeChild(a);
        }, 0);
    }

    EditFileGallery(listName) {
        const dataItem = sharePointHelper.GetSelected(listName);
        var url = dataItem.EditUrl.replace("action=interactivepreview", "action=edit");
        const a = document.createElement("a");
        document.body.appendChild(a);
        a.href = url;
        a.target = "_blank";
        a.click();
        setTimeout(() => {
            document.body.removeChild(a);
        }, 0);
    }

    DownloadFile(e, grid) {
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + grid).data("kendoGrid").dataItem(row);
        var url = dataItem.DownloadUrl;
        var downloadName = url.substring(url.lastIndexOf("/"), url.length);

        const a = document.createElement("a");
        document.body.appendChild(a);
        a.href = url;
        a.target = "_blank";
        a.download = downloadName;
        a.click();
        setTimeout(() => {
            document.body.removeChild(a);
        }, 0);
    }

    DownloadFileGallery(listName) {
        const dataItem = sharePointHelper.GetSelected(listName);
        var url = dataItem.DownloadUrl;
        var downloadName = url.substring(url.lastIndexOf("/"), url.length);

        const a = document.createElement("a");
        document.body.appendChild(a);
        a.href = url;
        a.target = "_blank";
        a.download = downloadName;
        a.click();
        setTimeout(() => {
            document.body.removeChild(a);
        }, 0);
    }

    DeleteFile(e, gridName) {
        e.preventDefault();
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + gridName).data("kendoGrid").dataItem(row);

        const grid = $("#" + e.delegateTarget.id).data("kendoGrid");
        const form = $(e.currentTarget).closest("form");

        const deletePrompt = grid.options.editable.confirmDelete;
        const title = form.data("delete-title");

        cpiConfirm.delete(title, deletePrompt, function () {
            cpiLoadingSpinner.show();

            $.ajax({
                url: grid.dataSource.transport.options.destroy.url,
                data: { caseFolder: dataItem.CaseFolder, id: dataItem.Id },
                type: "POST",
                success: function (result) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showSuccess(result.success);

                    grid.removeRow($(e.currentTarget).closest("tr"));
                    grid.dataSource._destroyed = [];
                },
                error: function (e) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(e);
                }
            });
        });
    }


    DeleteFileGallery(listName) {
        const dataItem = sharePointHelper.GetSelected(listName);

        const listView = $("#" + listName).data("kendoListView");

        cpiConfirm.delete("Delete", "Are you sure you want to delete this record ?", function () {
            cpiLoadingSpinner.show();

            $.ajax({
                url: listView.dataSource.transport.options.destroy.url,
                data: { caseFolder: dataItem.CaseFolder, id: dataItem.Id },
                type: "POST",
                success: function (result) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showSuccess(result.success);
                    listView.dataSource.remove(dataItem);
                    if (afterDelete)
                        afterDelete(result);
                },
                error: function (e) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(e);
                }
            });
        });
    }

    FileVersion(e, gridName) {
        const grid = $("#" + gridName).data("kendoGrid");
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + gridName).data("kendoGrid").dataItem(row);
        const url = $("#SPImageContainer").data("versionUrl");
        cpiLoadingSpinner.show();

        $.ajax({
            url: url,
            data: { serverRelativeUrl: dataItem.ServerRelativeUrl },
            type: "Get",
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $("#spDocumentVersionDialog");

                let entryForm = dialogContainer.find("form")[0];
                dialogContainer.modal("show");
                cpiLoadingSpinner.hide();
                entryForm = $(entryForm);
                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialogContainer,
                        closeOnSubmit: false,
                    }
                );
            },
            error: function (e) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            }
        });
    }

    FileVersionGallery(listName) {
        const listView = $("#" + listName).data("kendoListView");
        const dataItem = sharePointHelper.GetSelected(listName);
        const url = $("#SPImageContainer").data("versionUrl");
        cpiLoadingSpinner.show();

        $.ajax({
            url: url,
            data: { serverRelativeUrl: dataItem.ServerRelativeUrl },
            type: "Get",
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $("#spDocumentVersionDialog");

                let entryForm = dialogContainer.find("form")[0];
                dialogContainer.modal("show");
                cpiLoadingSpinner.hide();
                entryForm = $(entryForm);
                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialogContainer,
                        closeOnSubmit: false,
                    }
                );
            },
            error: function (e) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            }
        });
    }

    DownloadVersion(e, grid) {
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + grid).data("kendoGrid").dataItem(row);
        var url = dataItem.Url;
        var downloadName = url.substring(url.lastIndexOf("/"), url.length);

        const a = document.createElement("a");
        document.body.appendChild(a);
        a.href = url;
        a.target = "_blank";
        a.download = downloadName;
        a.click();
        setTimeout(() => {
            document.body.removeChild(a);
        }, 0);
    }

    ModifyFile(e, gridName) {
        const grid = $("#" + gridName).data("kendoGrid");
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + gridName).data("kendoGrid").dataItem(row);
        cpiLoadingSpinner.show();

        $.ajax({
            url: grid.dataSource.transport.options.update.url,
            data: { caseFolder: dataItem.CaseFolder, id: dataItem.Id },
            type: "Get",
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $("#spDocumentEditorDialog");

                let entryForm = dialogContainer.find("form")[0];
                dialogContainer.modal("show");
                cpiLoadingSpinner.hide();
                entryForm = $(entryForm);
                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialogContainer,
                        closeOnSubmit: false,
                        beforeSubmit: function () {
                            cpiLoadingSpinner.show();
                        },
                        afterSubmit: function () {
                            cpiLoadingSpinner.hide();
                            pageHelper.showSuccess(result.success);
                            grid.dataSource.read();
                            grid.refresh();
                            dialogContainer.modal("hide");
                        }
                    }
                );
            },
            error: function (e) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            }
        });
    }

    ModifyFileGallery(listName) {
        const listView = $("#" + listName).data("kendoListView");
        const dataItem = sharePointHelper.GetSelected(listName);
        cpiLoadingSpinner.show();

        $.ajax({
            url: listView.dataSource.transport.options.update.url,
            data: { caseFolder: dataItem.CaseFolder, id: dataItem.Id },
            type: "Get",
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $("#spDocumentEditorDialog");

                let entryForm = dialogContainer.find("form")[0];
                dialogContainer.modal("show");
                cpiLoadingSpinner.hide();
                entryForm = $(entryForm);
                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialogContainer,
                        closeOnSubmit: false,
                        beforeSubmit: function () {
                            cpiLoadingSpinner.show();
                        },
                        afterSubmit: function () {
                            cpiLoadingSpinner.hide();
                            pageHelper.showSuccess(result.success);
                            //listView.dataSource.read();
                            //listView.refresh();
                            dialogContainer.modal("hide");
                        }
                    }
                );
            },
            error: function (e) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            }
        });
    }

    AddFile(gridName) {
        const grid = $("#" + gridName).data("kendoGrid");
        cpiLoadingSpinner.show();

        $.ajax({
            url: grid.dataSource.transport.options.create.url,
            data: {},
            type: "Get",
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $("#spDocumentEditorDialog");

                let entryForm = dialogContainer.find("form")[0];
                dialogContainer.modal("show");
                cpiLoadingSpinner.hide();
                entryForm = $(entryForm);
                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialogContainer,
                        closeOnSubmit: false,
                        beforeSubmit: function () {
                            cpiLoadingSpinner.show();
                        },
                        afterSubmit: function () {
                            cpiLoadingSpinner.hide();
                            pageHelper.showSuccess(result.success);
                            grid.dataSource.read();
                            grid.refresh();
                            dialogContainer.modal("hide");
                        }
                    }
                );
            },
            error: function (e) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            }
        });
    }

    AddFileGallery(listName) {
        const listView = $("#" + listName).data("kendoListView");
        cpiLoadingSpinner.show();

        $.ajax({
            url: listView.dataSource.transport.options.create.url,
            data: {},
            type: "Get",
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $("#spDocumentEditorDialog");

                let entryForm = dialogContainer.find("form")[0];
                dialogContainer.modal("show");
                cpiLoadingSpinner.hide();
                entryForm = $(entryForm);
                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialogContainer,
                        closeOnSubmit: false,
                        beforeSubmit: function () {
                            cpiLoadingSpinner.show();
                        },
                        afterSubmit: function () {
                            cpiLoadingSpinner.hide();
                            pageHelper.showSuccess(result.success);
                            listView.dataSource.read();
                            listView.refresh();
                            dialogContainer.modal("hide");
                        }
                    }
                );
            },
            error: function (e) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            }
        });
    }

    CheckoutFile(e, gridName) {
        e.preventDefault();
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + gridName).data("kendoGrid").dataItem(row);

        const grid = $("#" + e.delegateTarget.id).data("kendoGrid");
        const form = $(e.currentTarget).closest("form");
        const url = $("#SPImageContainer").data("checkoutUrl");// + "?caseFolder=" + dataItem.CaseFolder + "&id=" + dataItem.Id;
        cpiLoadingSpinner.show();

        $.ajax({
            url: url,
            data: { caseFolder: dataItem.CaseFolder, id: dataItem.Id },
            type: "POST",
            success: function (result) {
                cpiLoadingSpinner.hide();
                pageHelper.showSuccess(result.success);

                //do not refresh grid to keep new and modified rows
                grid.dataSource.read();
                grid.refresh();
                if (afterDelete)
                    afterDelete(result);
            },
            error: function (e) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            }
        });
    }

    CheckoutFileGallery(listName) {
        const listView = $("#" + listName).data("kendoListView");
        const dataItem = sharePointHelper.GetSelected(listName);

        const url = $("#SPImageContainer").data("checkoutUrl");// + "?caseFolder=" + dataItem.CaseFolder + "&id=" + dataItem.Id;
        cpiLoadingSpinner.show();

        $.ajax({
            url: url,
            data: { caseFolder: dataItem.CaseFolder, id: dataItem.Id },
            type: "POST",
            success: function (result) {
                cpiLoadingSpinner.hide();
                pageHelper.showSuccess(result.success);

                if (afterDelete)
                    afterDelete(result);
            },
            error: function (e) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            }
        });
    }

    CheckinFile(e, gridName) {
        e.preventDefault();
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + gridName).data("kendoGrid").dataItem(row);

        const grid = $("#" + e.delegateTarget.id).data("kendoGrid");
        const form = $(e.currentTarget).closest("form");
        const url = $("#SPImageContainer").data("checkinUrl");// + "?caseFolder=" + dataItem.CaseFolder + "&id=" + dataItem.Id;
        cpiLoadingSpinner.show();

        $.ajax({
            url: url,
            data: { caseFolder: dataItem.CaseFolder, id: dataItem.Id },
            type: "POST",
            success: function (result) {
                cpiLoadingSpinner.hide();
                pageHelper.showSuccess(result.success);

                //do not refresh grid to keep new and modified rows
                grid.dataSource.read();
                grid.refresh();
                if (afterDelete)
                    afterDelete(result);
            },
            error: function (e) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            }
        });
    }

    CheckinFileGallery(listName) {
        const listView = $("#" + listName).data("kendoListView");
        const dataItem = sharePointHelper.GetSelected(listName);

        const url = $("#SPImageContainer").data("checkinUrl");// + "?caseFolder=" + dataItem.CaseFolder + "&id=" + dataItem.Id;
        cpiLoadingSpinner.show();

        $.ajax({
            url: url,
            data: { caseFolder: dataItem.CaseFolder, id: dataItem.Id },
            type: "POST",
            success: function (result) {
                cpiLoadingSpinner.hide();
                pageHelper.showSuccess(result.success);

                if (afterDelete)
                    afterDelete(result);
            },
            error: function (e) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            }
        });
    }

    GetSelected(listName) {
        const listView = $("#" + listName).data("kendoListView");
        const index = listView.select().index();
        const dataItem = listView.dataSource.view()[index];
        return dataItem;
    }

    GetSearchCriteria = (source) => {
        const imageDataContainer = $("#imageDataContainer");
        const form = imageDataContainer.find("#spImageSearchContainer").clone(true);
        const memoryForm = $("<form>").append(form);
        const criteria = pageHelper.formDataToCriteriaList(memoryForm);
        return { criteria: criteria.payLoad };
    }

    ShowSearchScreen = (source) => {
        const imageDataContainer = $("#imageDataContainer");
        imageDataContainer.find("#spImageSearchContainer").toggleClass("d-none");
    }
}