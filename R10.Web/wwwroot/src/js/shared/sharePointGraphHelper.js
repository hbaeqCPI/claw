import ActivePage from "../activePage";

export default class SharePointGraphHelper extends ActivePage {

    constructor() {
        super();
        this.image = window.image;
    }

    getSearchCriteria = () => {
        const imageDataContainer = $("#imageDataContainer");
        const form = imageDataContainer.find("#spImageSearchContainer").clone(true);
        const memoryForm = $("<form>").append(form);
        const criteria = pageHelper.formDataToCriteriaList(memoryForm);
        return { criteria: criteria.payLoad };
    }

    viewFileGrid(e, grid) {
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + grid).data("kendoGrid").dataItem(row);
        if (dataItem)
          this.viewFile(dataItem);
    }

    viewFileGallery(listName) {
        const dataItem = this.getSelected(listName);
        if (dataItem)
          this.viewFile(dataItem);
    }

    viewFile(dataItem) {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Shared/SharePointGraph/GetPreviewUrl${(dataItem.Name.endsWith('.url') ? "ForLinks":"")}`;

        let retry = 0;
        openPreviewFile();

        function openPreviewFile() {
            $.get(url, { docLibrary: dataItem.DocLibrary, id: dataItem.Id })
                .done(function (result) {
                    const a = document.createElement("a");
                    document.body.appendChild(a);
                    a.href = result.previewUrl;
                    a.target = "_blank"
                    a.click();
                    setTimeout(() => {
                        document.body.removeChild(a);
                    }, 0);
                })
                .fail(function (e) {
                    if (e.status == 401 && retry < 3) {
                        retry++;
                        const baseUrl = $("body").data("base-url");
                        const url = `${baseUrl}/Graph/SharePoint`;

                        sharePointGraphHelper.getGraphToken(url, () => {
                            openPreviewFile();
                        });
                    }
                    else {
                        pageHelper.showErrors(e.responseText);
                    }
                });
        }
    }

    viewForSignatureFile(e,grid) {
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + grid).data("kendoGrid").dataItem(row);
    
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Shared/SharePointGraph/GetPreviewUrl`;

        $.get(url, { docLibrary: dataItem.DocLibrary, id: dataItem.Id })
            .done(function (result) {
                const a = document.createElement("a");
                document.body.appendChild(a);
                a.href = result.previewUrl;
                a.target = "_blank"
                a.click();
                setTimeout(() => {
                    document.body.removeChild(a);
                }, 0);
            })
            .fail(function (e) {
                pageHelper.showErrors(e.responseText);
            });
    
    }

    refreshGrid(gridName) {
        //cpiLoadingSpinner.show();
        const grid = $("#" + gridName).data("kendoGrid");
        grid.dataSource.read();
        grid.refresh();
        //cpiLoadingSpinner.hide();
    }

    refreshListView = (listName) => {
        const listViewImages = $("#" + listName).data("kendoListView");
        listViewImages.dataSource.read();
    }

    showSearchScreen = () => {
        const imageDataContainer = $("#imageDataContainer");
        imageDataContainer.find("#spImageSearchContainer").toggleClass("d-none");
    }


    deleteFileGrid(e, gridName) {
        e.preventDefault();
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + gridName).data("kendoGrid").dataItem(row);

        const grid = $("#" + e.delegateTarget.id).data("kendoGrid");
        const form = $(e.currentTarget).closest("#imageDataContainer");

        const deletePrompt = form.data("delete-message");
        const title = form.data("delete-title");

        cpiConfirm.delete(title, deletePrompt, function () {
            cpiLoadingSpinner.show();

            $.ajax({
                url: grid.dataSource.transport.options.destroy.url,
                data: { docLibrary: dataItem.DocLibrary, id: dataItem.Id },
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

    deleteFileGallery(listName) {
        const dataItem = this.getSelected(listName);
        const listView = $("#" + listName).data("kendoListView");
        const form = $("#" + listName).closest("#imageDataContainer");
        const deletePrompt = form.data("delete-message");
        const title = form.data("delete-title");

        cpiConfirm.delete(title, deletePrompt, function () {
            cpiLoadingSpinner.show();
            $.ajax({
                url: listView.dataSource.transport.options.destroy.url,
                data: { docLibrary: dataItem.DocLibrary, id: dataItem.Id },
                type: "POST",
                success: function (result) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showSuccess(result.success);
                    listView.dataSource.remove(dataItem);
                },
                error: function (e) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(e);
                }
            });
        });
    }


    editFileGrid(e, grid) {
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + grid).data("kendoGrid").dataItem(row);
        if (dataItem)
           this.editFile(dataItem);
        
    }

    editFileGallery(listName) {
        const dataItem = this.getSelected(listName);
        if (dataItem)
           this.editFile(dataItem);
    }

    editFile(dataItem) {
        var url = dataItem.EditUrl;
        const a = document.createElement("a");
        document.body.appendChild(a);
        a.href = url;
        a.target = "_blank";
        a.click();
        setTimeout(() => {
            document.body.removeChild(a);
        }, 0);
    }

    checkoutFileGrid(e, gridName) {
        e.preventDefault();
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + gridName).data("kendoGrid").dataItem(row);
        if (dataItem)
            this.checkoutFile(dataItem, () => { this.refreshGrid(gridName); })

    }

    checkoutFileGallery(listName) {
        const listView = $("#" + listName).data("kendoListView");
        const dataItem = this.getSelected(listName);
        if (dataItem) 
            this.checkoutFile(dataItem, () => { this.refreshListView(listName);})
    }

    checkoutFile(dataItem,callBack) {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Shared/SharePointGraph/CheckoutFile`;
        var self = this;

        cpiLoadingSpinner.show();
        $.get(url, { docLibrary: dataItem.DocLibrary, id: dataItem.Id })
            .done(function (result) {
                pageHelper.showSuccess(result.success);
                callBack();
                cpiLoadingSpinner.hide();
            })
            .fail(function (e) {
                pageHelper.showErrors(e.responseText);
                cpiLoadingSpinner.hide();
            });
    }

    checkinFileGrid(e, gridName) {
        e.preventDefault();
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + gridName).data("kendoGrid").dataItem(row);
        if (dataItem)
            this.checkinFile(dataItem, () => { this.refreshGrid(gridName); })
    }

    checkinFileGallery(listName) {
        const dataItem = this.getSelected(listName);
        if (dataItem)
            this.checkinFile(dataItem, () => { this.refreshListView(listName); })
    }

    checkinFile(dataItem,callBack) {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Shared/SharePointGraph/CheckinFile`;
        var self = this;

        cpiLoadingSpinner.show();
        $.get(url, { docLibrary: dataItem.DocLibrary, id: dataItem.Id })
            .done(function (result) {
                pageHelper.showSuccess(result.success);
                callBack();
                cpiLoadingSpinner.hide();
            })
            .fail(function (e) {
                pageHelper.showErrors(e.responseText);
                cpiLoadingSpinner.hide();
            });
    }

    fileVersionGrid(e, gridName) {
        e.preventDefault();
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + gridName).data("kendoGrid").dataItem(row);
        if (dataItem)
            this.fileVersion(dataItem);
    }

    fileVersionGallery(listName) {
        const dataItem = this.getSelected(listName);
        if (dataItem)
            this.fileVersion(dataItem);
    }

    fileVersion(dataItem) {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Shared/SharePointGraph/Version`;
        cpiLoadingSpinner.show();

        $.get(url, { docLibrary: dataItem.DocLibrary, name: dataItem.Name, driveItemId: dataItem.Id })
            .done(function (result) {
                const popupContainer = $(".site-content .popup");
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
            })
            .fail(function (e) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(e.responseText);
            });
    }

    downloadFileGrid(e, grid) {
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + grid).data("kendoGrid").dataItem(row);
        if (dataItem)
            this.downloadFile(dataItem);
    }

    downloadFileGallery(listName) {
        const dataItem = this.getSelected(listName);
        if (dataItem)
            this.downloadFile(dataItem);
    }

    downloadFile(dataItem) {
        var url = dataItem.DownloadUrl;
        var downloadName = dataItem.Name;

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


    addFileGrid(gridName) {
        const grid = $("#" + gridName).data("kendoGrid");
        const imageContainer = $("#" + gridName).closest(".image-container");
        if (grid)
            this.addFile(grid, imageContainer);
    }

    addFileGallery(listName) {
        const listView = $("#" + listName).data("kendoListView");
        const imageContainer = $("#" + listName).closest(".image-container");

        if (listView)
            this.addFile(listView, imageContainer);
    }


    addFile(grid, imageContainer) {
        const folderId = imageContainer.data("folder-id");

        $.ajax({
            url: grid.dataSource.transport.options.create.url,
            data: {folderId},
            type: "Get",
            success: function (result) {
                const popupContainer = $(".site-content .popup");
                popupContainer.html(result);
                const dialogContainer = $("#documentEditorDialog");

                let entryForm = dialogContainer.find("form")[0];
                dialogContainer.modal("show");
                entryForm = $(entryForm);
                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialogContainer,
                        closeOnSubmit: false,
                        beforeSubmit: function () {
                            cpiLoadingSpinner.show();
                        },
                        afterSubmit: function (response) {
                            cpiLoadingSpinner.hide();
                            pageHelper.handleSignatureWorkflow(response);
                            pageHelper.handleEmailWorkflow(response);
                            
                            dialogContainer.modal("hide");
                            setTimeout(() => {
                                grid.dataSource.read();
                                grid.refresh();
                            }, 2000);
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

    modifyFileGrid(e, gridName, activePage) {
        const grid = $("#" + gridName).data("kendoGrid");
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + gridName).data("kendoGrid").dataItem(row);
        console.log(dataItem);
        if (dataItem)
            this.modifyFile(dataItem, grid, activePage);
    }

    modifyFileGallery(listName) {
        const listView = $("#" + listName).data("kendoListView");
        const dataItem = this.getSelected(listName);
        if (dataItem)
            this.modifyFile(dataItem, listView);
    }

    modifyFile(dataItem, grid,activePage) {
        cpiLoadingSpinner.show();

        $.ajax({
            url: grid.dataSource.transport.options.update.url,
            data: { docLibrary: dataItem.DocLibrary, docLibraryFolder: dataItem.DocLibraryFolder, recKey: dataItem.RecKey, driveItemId: dataItem.Id, listItemId: dataItem.ListItemId, parentId: dataItem.ParentId },
            type: "Get",
            success: function (result) {
                const popupContainer = $(".site-content .popup");
                popupContainer.html(result);
                const dialogContainer = $("#documentEditorDialog");

                let entryForm = dialogContainer.find("form")[0];
                dialogContainer.modal("show");
                cpiLoadingSpinner.hide();
                entryForm = $(entryForm);

                let formChanged = false;
                entryForm.find(":input").each(function () {
                    $(this).bind("change", function () {
                        formChanged = true;
                    });
                });

                $('#documentEditorDialog [type="button"][data-dismiss="modal"]').on('click', function (e) {            
                    var actGrid = $("#documentVerificationActGrid").data("kendoGrid")
                    if (actGrid) {
                        var actGridDS = actGrid.dataSource;
                        $.each(actGridDS._data, function ()
                        {
                            if (this.dirty == true)
                            {
                                formChanged = true;
                            }
                        });
                    }   

                    if (formChanged && !confirm(dialogContainer.data("cancel-message"))) {                
                        e.stopPropagation();
                    }            
                        
                    //check New Actions workflow
                    var tempFormData = new FormData(entryForm[0]);   
                    $.get(dialogContainer.data("action-workflow-url"), { docId: tempFormData.get('DocId'), driveItemId: tempFormData.get('DriveItemId') })
                        .done((result) => {                    
                            if (result && result.length > 0) {                        
                                let promise = pageHelper.handleEmailWorkflow({ id: 0, sendEmail: "", emailurl: "", emailWorkflows: result });
                                promise = promise.then(() => {
                                    self.refreshDocListView();
                                });
                            }                    
                        });
                });

                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialogContainer,
                        closeOnSubmit: false,
                        beforeSubmit: function (e) {                            
                            cpiLoadingSpinner.show();
                        },
                        afterSubmit: function (response) {
                            cpiLoadingSpinner.hide();
                            if (response) {
                                pageHelper.handleEmailWorkflow(response);
                            }
                            else {
                                pageHelper.showSuccess(result.success);
                            }                            
                            grid.dataSource.read();
                            grid.refresh();
                            dialogContainer.modal("hide");
                            const currentPage = window[activePage];
                            currentPage.image.refreshSharePointDefaultImage(currentPage);
                        }
                    }
                );

                entryForm.on("click", "#save", (e) => {
                    e.preventDefault();

                    let formData = new FormData(entryForm[0]);
                    let actGrid = $("#documentVerificationActGrid").data("kendoGrid");
                    ////show error if IsActRequired = true and Action grid is empty
                    //if (formData && formData.has('IsActRequired') && formData.get('IsActRequired') === 'true' && actGrid) {
                    //    var gridData = actGrid.dataSource.data();
                    //    if (gridData && gridData.length <= 0) {
                    //        alert(dialogContainer.data("missing-action"));
                    //        return;
                    //    }                
                    //}

                    //auto save changes on Action grid  
                    if (actGrid && actGrid.dataSource.hasChanges()) {
                        $.when(pageHelper.kendoGridSave({
                            name: 'documentVerificationActGrid'
                            //popup windows closes after saving, no need to refresh
                            //,afterSubmit: () => { $("#documentVerificationActGrid").data("kendoGrid").dataSource.read(); }
                        })).then(
                            function () {
                                entryForm.submit();
                            },
                            function (fail) {
                                alert(fail);
                                return;
                            },
                            null
                        );
                    }
                    else {
                        entryForm.submit();
                    }                                       
                });

            },
            error: function (e) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            }
        });
    }


    getSelected(listName) {
        const listView = $("#" + listName).data("kendoListView");
        const index = listView.select().index();
        const dataItem = listView.dataSource.view()[index];
        return dataItem;
    }

    previewFile(docLibrary,id) {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Shared/SharePointGraph/GetPreviewUrl`;

        $.get(url, { docLibrary, id})
            .done(function (result) {
                const a = document.createElement("a");
                document.body.appendChild(a);
                a.href = result.previewUrl;
                a.target = "_blank"
                a.click();
                setTimeout(() => {
                    document.body.removeChild(a);
                }, 0);
            })
            .fail(function (e) {
                pageHelper.showErrors(e.responseText);
            });
    }
   
    restoreVersion(e, grid) {
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + grid).data("kendoGrid").dataItem(row);
        if (dataItem) {
            const container = $("#spDocumentVersionDialog");
            const restorePrompt = container.data("restore-message");
            const title = container.data("restore-title");

            cpiConfirm.confirm(title, restorePrompt, function () {
                $.get(dataItem.RestoreUrl)
                    .done(() => {
                        $("#" + grid).data("kendoGrid").dataSource.read();
                    })
                    .fail(function (e) {
                        pageHelper.showErrors(e.responseText);
                    });
            });
            
        }
            
    }

    downloadDocs = (gridName, docLibrary,docLibraryFolder,recKey) => {
        const grid = $(gridName).data("kendoGrid");
        const keys = grid.selectedKeyNames();
        const selection = [];

        if (keys.length > 0) {

            const gridData = grid.dataSource.data();

            for (const item in keys) {
                const rec = gridData.find(r => r.Id == keys[item]);
                if (rec) {
                    const selected = { DriveItemId: rec.Id, Name: rec.Name };
                    selection.push(selected);
                }
            }

            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Shared/SharePointGraph/DownloadFiles`;

            let downloadForm = $("#documentsDownload").last();
            if (downloadForm.length > 0) {
                downloadForm.remove();
            }
            const selectionString = JSON.stringify(selection);
            console.log("selectionString", selectionString);
            //$(`<form action="${url}" method="post" id="documentsDownload"><input type="hidden" name="docLibrary" value="${docLibrary}"/><input type="hidden" name="docLibraryFolder" value="${docLibraryFolder}"/><input type="hidden" name="recKey" value="${recKey}"/><input type="hidden" name="selection" value='${selectionString}'/></form>`).appendTo('body').submit();
            $(`<form action="${url}" method="post" id="documentsDownload"><input type="hidden" name="docLibrary" value="${docLibrary}"/><input type="hidden" name="selection" value='${selectionString}'/></form>`).appendTo('body').submit();
        }
    }

    mergeDocs = (event, gridName, docLibrary, docLibraryFolder, recKey, parentId) => {
        const grid = $(gridName).data("kendoGrid");
        const keys = grid.selectedKeyNames();
        const self = this;
        const mergeButton = $(event);   
        const baseUrl = $("body").data("base-url");
        const viewUrl = `${baseUrl}/Shared/SharePointGraph/FileMerge`;
        const mergeUrl = `${baseUrl}/Shared/SharePointGraph/MergeFiles`;

        if (keys.length <= 1) {            
            pageHelper.showErrors(mergeButton.data("no-files-selected-error"));
            return;
        }
        else {
            //get list of documents for merge
            const selectedDocs = [];

            var gridData = grid.dataSource.data();

            for (const item in keys) {
                const rec = gridData.find(r => r.Id == keys[item]);
                if (rec) {
                    const selected = { Id: rec.Id, Name: rec.Name, DocLibrary: rec.DocLibrary, DocLibraryFolder: rec.DocLibraryFolder };
                    selectedDocs.push(selected);
                }
            }

            if (selectedDocs && selectedDocs.length > 0) {                

                const nonPdfFiles = selectedDocs.filter(f => !f.Name.endsWith(".pdf"));
                if (nonPdfFiles && nonPdfFiles.length > 0) {
                    pageHelper.showErrors(mergeButton.data("file-type-error"));
                    return;
                }

                cpiLoadingSpinner.show();
                $.get(viewUrl)
                    .done((result) => {
                        cpiLoadingSpinner.hide();
                        //clear all existing hidden popups to avoid kendo id issue
                        $(".site-content .popup").empty();
                        const popupContainer = $(".site-content .popup").last();
                        popupContainer.html(result);
                        const container = $("#documentMergeDialog");
                        container.modal("show");

                        var imageMergeGrid = container.find("#imageMergeGrid");
                        if (imageMergeGrid) {
                            $(imageMergeGrid).data("kendoGrid").setDataSource(selectedDocs);
                            $(imageMergeGrid).data("kendoGrid").dataSource.pageSize(selectedDocs.length);
                        }

                        container.on("click", "#mergeDocument", (e) => {
                            e.preventDefault();
                            let mergedDocName = "";
                            const mergedDocNameInput = container.find("#MergedDocName");

                            if (mergedDocNameInput && mergedDocNameInput.length > 0)
                                mergedDocName = mergedDocNameInput[0].value;

                            const error = container.find("#MergedDocName-error");
                            const errorSpan = $(error).closest(".field-validation-error");

                            if (mergedDocName > "") {
                                const mergeDocs = $(imageMergeGrid).data("kendoGrid").dataSource.data();
                                const orderedDocs = mergeDocs.map(m => ({ Id: m.Id, Name: m.Name, DocLibrary: m.DocLibrary, DocLibraryFolder: m.DocLibraryFolder }));

                                cpiLoadingSpinner.show();                                
                                $.post(mergeUrl, { docLibrary: docLibrary, docLibraryFolder: docLibraryFolder, recKey: recKey, parentId: parentId, mergedDocName: mergedDocName, docList: orderedDocs })
                                    .done(function (result) {
                                        cpiLoadingSpinner.hide();
                                        container.modal("hide");
                                        setTimeout(() => {
                                            grid.dataSource.read();
                                            grid.refresh();
                                            pageHelper.showSuccess(result);
                                        }, 2000);                                        
                                    })
                                    .fail((error) => {
                                        cpiLoadingSpinner.hide();
                                        pageHelper.showErrors(error)
                                    });

                            }
                            else {
                                errorSpan.show();
                                $(mergedDocNameInput).addClass("input-validation-error");
                                return;                             
                            }
                        });

                    })
                    .fail((e) => {
                        cpiLoadingSpinner.hide();
                        this.showErrors(e.responseText);
                    });
            }
        }
    }

    pullDocuSignDoc = (e, grid,callBack) => {
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + grid).data("kendoGrid").dataItem(row);
        let retry = 0;

        if (dataItem && dataItem.SentToDocuSign) {
            if (dataItem.SignatureCompleted) {
                const repullPrompt = $("#" + grid).closest(".image-container").data("repull-prompt");
                const title = $("#" + grid).closest(".image-container").data("confirm");

                cpiConfirm.confirm(title, repullPrompt, function () {
                    pullCompletedDoc();
                });
            }
            else {
                const pullPrompt = $("#" + grid).closest(".image-container").data("pull-prompt");
                const title = $("#" + grid).closest(".image-container").data("confirm");

                cpiConfirm.confirm(title, pullPrompt, function () {
                    pullCompletedDoc();
                });
            }

            function pullCompletedDoc() {
                const baseUrl = $("body").data("base-url");
                const url = `${baseUrl}/Shared/DocuSign/GetSignedDocumentsAndSaveToSharePoint`;

                cpiLoadingSpinner.show();
                $.post(url, {
                    viewModelParam: {
                        EnvelopeId: dataItem.EnvelopeId,
                        DocLibrary: dataItem.DocLibrary,
                        DocLibraryFolder: dataItem.DocLibraryFolder,
                        ParentId: dataItem.ParentId,
                        Id: dataItem.Id,
                        DocName: dataItem.Name,
                    }
                })
                    .done((result) => {
                        if (callBack) callBack();
                        $("#" + grid).data("kendoGrid").dataSource.read();
                        cpiLoadingSpinner.hide();
                    })
                    .fail(function (error) {
                        if ((error.status == 401 || error.responseText.indexOf("InvalidAuthenticationToken") > 0) && retry < 3) {
                            retry++;
                            const baseUrl = $("body").data("base-url");
                            const url = `${baseUrl}/Graph/SharePoint`;

                            sharePointGraphHelper.getGraphToken(url, () => {
                                pullCompletedDoc();
                            });
                        }
                        else {
                            cpiLoadingSpinner.hide();
                            if (error.responseJSON) {
                                const jsonError = error.responseJSON;
                                pageHelper.showErrors(jsonError.errorMessage);
                                if (jsonError.consentRequired) {
                                    console.log(jsonError.url);
                                    window.open(jsonError.url);
                                }
                            }
                            else
                                pageHelper.showErrors(error);
                        }
                    });
            }
        }
    }

    pushDocuSignDoc = (e, grid,callBack) => {
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + grid).data("kendoGrid").dataItem(row);
        let retry = 0;

        if (dataItem) {
            const title = $("#" + grid).closest(".image-container").data("confirm");
            if (dataItem.SentToDocuSign) {
                const resendPrompt = $("#" + grid).closest(".image-container").data("resend-prompt");
                cpiConfirm.confirm(title, resendPrompt, function () {
                    pushDoc();
                });
            }
            else {
                const sendPrompt = $("#" + grid).closest(".image-container").data("send-prompt");
                cpiConfirm.confirm(title, sendPrompt, function () {
                    pushDoc();
                });
            }

            function pushDoc() {
                const baseUrl = $("body").data("base-url");
                const url = `${baseUrl}/Shared/DocuSign/ResendEnvelopeFromFileUpload`;
                const workflow = {
                    UserFile: {
                        FileName: dataItem.Name,
                        StrId: dataItem.Id,
                        Name: dataItem.Name
                    },
                    QESetupId: dataItem.QESetupId,
                    ParentId: dataItem.ParentId,
                    ScreenCode: dataItem.ScreenCode,
                    RoleLink: dataItem.RoleLink,
                    SystemTypeCode: dataItem.SystemTypeCode,
                    SharePointDocLibrary: dataItem.DocLibrary
                };

                cpiLoadingSpinner.show();
                $.post(url, { workflow })
                    .done((result) => {
                        cpiLoadingSpinner.hide();
                        $("#" + grid).data("kendoGrid").dataSource.read();
                        if (callBack) callBack();
                    })
                    .fail(function (error) {
                        if ((error.status == 401 || error.responseText.indexOf("InvalidAuthenticationToken") > 0) && retry < 3) {
                            retry++;
                            const baseUrl = $("body").data("base-url");
                            const url = `${baseUrl}/Graph/SharePoint`;

                            sharePointGraphHelper.getGraphToken(url, () => {
                                pushDoc();
                            });
                        }
                        else {
                            cpiLoadingSpinner.hide();
                            if (error.responseJSON) {
                                const jsonError = error.responseJSON;
                                pageHelper.showErrors(jsonError.errorMessage);
                                if (jsonError.consentRequired) {
                                    console.log(jsonError.url);
                                    window.open(jsonError.url);
                                }
                            }
                            else
                                pageHelper.showErrors(error);
                        }

                    });
            }

        }
    }

    downloadLetterTemplate(e, grid) {
        e.preventDefault();
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const container = $("#fileTemplateGridContainer");
        const url = container.data("download-url");
        console.log(dataItem);
        const data = { sys: dataItem.SystemType, templateFile: dataItem.TemplateFile, id: dataItem.Id};
        window.fileUtility.getFileStream(url, data);
    }

    openSharePointUrl(url) {
        let retry = 0;
        openSharePointUrlWithRetry();

        function openSharePointUrlWithRetry() {
            $.get(url)
                .done(function (result) {
                    const a = document.createElement("a");
                    document.body.appendChild(a);
                    a.href = result.previewUrl;
                    a.target = "_blank";
                    a.click();
                    setTimeout(() => {
                        document.body.removeChild(a);
                    }, 0);
                })
                .fail(function (e) {
                    if (e.status == 401 && retry < 3) {
                        retry++;
                        const baseUrl = $("body").data("base-url");
                        const url = `${baseUrl}/Graph/SharePoint`;

                        sharePointGraphHelper.getGraphToken(url, () => {
                            openSharePointUrlWithRetry();
                        });
                    }
                    else {
                        pageHelper.showErrors(e.responseText);
                    }
                });
        }
        
    }


    imageReadErrorHandler(e,docTree) {
        if (e.xhr.status == 401) {
            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Graph/SharePoint`;
            $(".documents-tab").addClass("d-none");

            sharePointGraphHelper.getGraphToken(url, () => {
                //this.read();
                e.sender.read();

                if (docTree.length > 0) {
                    const docTreeView = $(`#${docTree}`);
                    if (docTreeView) {
                        docTreeView.data("kendoTreeView").dataSource.read();
                    }
                }
                $(".documents-tab").removeClass("d-none");
            });
        }
    }

    //Opens Graph authentication in new tab
    //  Auth endpoint closes new tab after authentication if returnUrl is empty
    //  Auth endpoint redirects to returnUrl if not empty
    //Executes callback function after authentication
    getGraphToken(url, callback)  {
        const statusKey = "graph-signin";
        const intervalId = setInterval(function () { checkAuth(); }, 1000);
        const checkAuth = () => {
            const status = localStorage.getItem(statusKey);
            if (status) {
                clearInterval(intervalId);
                localStorage.removeItem(statusKey);

                if (status == "ok" && callback)
                    callback();
            }
        };

        localStorage.setItem(statusKey, "");
        window.open(url);
    }

    //Get search results grid images sequentially
    getDefaultGridImage = (page, thumbnailUrl, tokenUrl, docLibraryFolder) => {
        page.find(".sp-default-image:not(.loaded)").each((i, e) => {
            const el = $(e);
            let driveId = "";

            if (el.data("recKey")) {
                //do not show spinner. this is a background process
                //cpiLoadingSpinner.show();

                const recKey = { Id: el.data("id"), RecKey: el.data("recKey") };
                $.post(thumbnailUrl, { docLibraryFolder: docLibraryFolder, driveId, recKey })
                    .done((result) => {
                        //cpiLoadingSpinner.hide();
                        el.addClass("loaded");

                        if (result && result.DriveId) {
                            driveId = result.DriveId;

                            el.attr("src", result.ThumbnailUrl);
                            el.data("img-src", result.DisplayUrl);
                            el.closest(".image").addClass("found");
                            el.closest(".image-default").find("i").hide();
                            el.show();
                        }
                        else {
                            el.hide();
                            el.closest(".image-default").find("i.fa-spin").hide();
                            el.closest(".image-default").find("i.no-image").show();
                        }

                        //fetch next
                        this.getDefaultGridImage(page, thumbnailUrl, tokenUrl, docLibraryFolder);
                    })
                    .fail((e) => {
                        //cpiLoadingSpinner.hide();
                        if (e.status == 401) {
                            sharePointGraphHelper.getGraphToken(tokenUrl, () => {
                                this.getDefaultGridImage(page, thumbnailUrl, tokenUrl, docLibraryFolder);
                            });
                        }
                        else {
                            console.error(e.responseText);
                        }
                    });

                //fetch next image after $.post.done()
                return false;
            }
        })
    }
}