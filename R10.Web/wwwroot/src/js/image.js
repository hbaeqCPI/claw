import * as pageHelper from "./pageHelper";

export default class Image {

    constructor() {
        this.SavedViewOption = "";                 // used to save image view option selected by user; the screen option resets to default/gallery as we move between records
        this.parentPage = null;
        this.container = null;
        this.hasViewScreen = true;
        this.serverOperation = true;
    }

    initializeImage = (parentPage, serverOperation) => {
        const self = this;
        this.parentPage = parentPage;          // save the parent page
        this.container = $(`#${parentPage.detailContentContainer}`);

        if (serverOperation != null)
            this.serverOperation = serverOperation;

        this.container.find("#UploadZone").hide();

        if (this.SavedViewOption === "")
            this.SavedViewOption = this.GetScreenViewOption();

        this.refreshDisplay();

        this.container.find('input[name="ImageViewOption"]').change((e) => {
            this.SavedViewOption = $(e.target).val();
            this.refreshDisplay();
        });

        // _ImageGallery.cshtml button events
        const galleryViewButtons = this.container.find("#imageListButtonsContainer");
        galleryViewButtons.find("#view").click((e) => {
            e.preventDefault();
            const selectedImage = this.getSelected();
            this.view(selectedImage);
        });

        galleryViewButtons.find("#add").click((e) => {
            e.preventDefault();
            this.add();
        });

        galleryViewButtons.find("#edit").click((e) => {
            e.preventDefault();
            const selectedImage = this.getSelected();
            this.edit(selectedImage);
        });

        galleryViewButtons.find("#delete").click((e) => {
            e.preventDefault();
            const selectedImage = this.getSelected();
            this.kendoListViewDeleteImage(selectedImage);
        });

        galleryViewButtons.find("#search").click((e) => {
            e.preventDefault();
            this.showSearchScreen();
        });

        this.container.find(".image-search .k-combobox > input").each(function () {
            const comboBox = $(this).data("kendoComboBox");
            if (comboBox) {
                comboBox.bind("change", function () {
                    self.container.find(".image-search #FromTreeView").val("0");
                    if (self.SavedViewOption === "GalleryView")
                        self.refreshListViewPage();
                    else
                        self.refreshGridPage();
                });
            }
        });

        this.container.find(".image-search .k-datepicker input").each(function () {
            const datePicker = $(this).data("kendoDatePicker");
            if (datePicker) {
                datePicker.bind("change", function () {
                    self.container.find(".image-search #FromTreeView").val("0");
                    if (self.SavedViewOption === "GalleryView")
                        self.refreshListViewPage();
                    else
                        self.refreshGridPage();
                });
            }
        });

        this.container.find(".image-search #FolderId,.image-search #DocId,.image-search #ScreenCode,.image-search #DataKeyValue")
            .bind("change", function () {
            self.container.find(".image-search #FromTreeView").val("0");
            if (self.SavedViewOption === "GalleryView")
                self.refreshListViewPage();
            else
                self.refreshGridPage();
            });

        const treeView = this.container.find('.doc-treeview');
        if (treeView.length > 0) {
            treeView.data("kendoTreeView").dataSource.read();
        }
    }

    showSearchScreen = (source) => {
        this.container.find("#imageSearchContainer").toggleClass("d-none");

    }

    getSearchCriteria = (source) => {
        const form = this.container.find("#imageSearchContainer").clone(true);
        const memoryForm = $("<form>").append(form);
        const criteria = pageHelper.formDataToCriteriaList(memoryForm);
        return { criteria: criteria.payLoad };
    }

    // _ImageGrid.cshtml events
    addImage = () => {
        this.add();
    }

    viewImage = (e, grid) => {
        const selectedImage = grid.dataItem($(e.currentTarget).closest("tr"));
        this.view(selectedImage);
    }

    editImage = (e, grid) => {
        const selectedImage = grid.dataItem($(e.currentTarget).closest("tr"));
        this.edit(selectedImage);
    }

    checkoutImage = (e, grid) => {
        let row = $(e.currentTarget).closest("tr");
        const selectedImage = grid.dataItem(row);
        const checkoutUrl = row.find(".k-grid-Checkout").data("url");

        this.checkout(selectedImage, checkoutUrl,grid);
    }

    deleteImage = (e, grid) => {
        e.preventDefault();
        const selectedImage = grid.dataItem($(e.currentTarget).closest("tr"));
        this.kendoGridDeleteImage(e, selectedImage);
    }

    downloadImage = (parentId, gridName, url) => {
        const grid = $(gridName).data("kendoGrid");
        const selection = grid.selectedKeyNames();

        if (selection.length > 0) {
            let downloadForm = $("#documentsDownload").last();
            if (downloadForm.length > 0) {
                downloadForm.remove();
            }
            $(`<form action="${url}" method="post" id="documentsDownload"><input type="hidden" name="ParentId" value="${parentId}"/><input type="hidden" name="Selection" value="${selection.join()}"/></form>`).appendTo('body').submit();
        }
    }

    mergeImage = (event, parentId, gridName, viewUrl, mergeUrl) => {
        const grid = $(gridName).data("kendoGrid");
        const selection = grid.selectedKeyNames();
        const self = this;
        const mergeButton = $(event);        

        if (selection.length <= 1) {            
            pageHelper.showErrors(mergeButton.data("no-files-selected-error"));
            return;
        }
        else {
            //get list of documents for merge
            var docIds = selection.map(function (x) {
                return parseInt(x, 10);
            });

            if (docIds && docIds.length > 0) {
                var gridData = grid.dataSource.data();
                const selectedDocs = gridData.filter(o => docIds.includes(o.DocId));

                const nonPdfFiles = selectedDocs.filter(f => !f.DocFileName.endsWith(".pdf"));
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
                                const orderedDocs = mergeDocs.map(m => ({ DocId: m.DocId, DocName: m.DocName, DocFileName: m.DocFileName }));

                                cpiLoadingSpinner.show();
                                $.post(mergeUrl, { mergedDocName: mergedDocName, docList: orderedDocs })
                                    .done(function (result) {
                                        cpiLoadingSpinner.hide();
                                        container.modal("hide");
                                        self.refreshDisplay(true);
                                        pageHelper.showSuccess(result);
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

    GetScreenViewOption = () => {
        const viewOptions = this.container.find('input[name="ImageViewOption"]');
        return viewOptions.val();
    }

    configureEditor = (action) => {
        if (action === "edit") {
            const selectedImage = this.getSelected();

            if (selectedImage && selectedImage.ThumbnailFile && selectedImage.ThumbnailFile.includes("url")) {
                const imageFile = this.container.find('#ImageFile').val();
                this.container.find('#Url').val(imageFile);
                this.container.find('[name="uploadAction"]').removeAttr('checked');
                this.container.find('input:radio[name="uploadAction"][value="url"]').attr('checked', true);
            }
        }

        //default Public Access to check if RestrictPrivateDocument is enabled
        const isPublic = this.container.find('input:checkbox[name="IsPublic"]');
        if (isPublic.is(":visible")) {
            isPublic.attr('checked', true);
        }

        const value = this.container.find("input[name=uploadAction]:checked").val();
        this.configureEditorRows(value);
    }

    configureEditorRows = (currentValue) => {
        const uploadActionContainer = this.container.find("#uploadActionContainer");
        uploadActionContainer.find("input:radio").each(function () {
            const value = $(this).val();
            const selector = "#" + value + "Row";
            if (value === currentValue) {
                $(selector).show();
            }
            else {
                $(selector).hide();
            }
        });

    }

    configureListButtons = () => {
        const dataSource = this.container.find(".imageListView").data("kendoListView").dataSource;
        dataSource.fetch(() => {
            this.showListButtons(dataSource.total() > 0);
        });
    }

    showListButtons = (showButtons) => {
        const listViewButtons = this.container.find("#imageListButtonsContainer");

        if (showButtons) {
            listViewButtons.find("#view").show();
            listViewButtons.find("#edit").show();
            listViewButtons.find("#delete").show();
        }
        else {
            listViewButtons.find("#view").hide();
            listViewButtons.find("#edit").hide();
            listViewButtons.find("#delete").hide();
        }
    }

    refreshDisplay = (isUpdateRecordStamps,folderId) => {

        if (!this.hasViewScreen) //when image upload is from screen like QD
            return;

        const screenOption = this.GetScreenViewOption();

        if (this.SavedViewOption === "GalleryView") {
            this.refreshListView();
            this.container.find("#imageGalleryContainer").show();
            this.container.find("#imageGridContainer").hide();
            if (screenOption !== this.SavedViewOption)
                this.container.find("#ImageGalleryView").prop("checked", true);
        }
        else {
            this.refreshGrid();
            this.container.find("#imageGridContainer").show();
            this.container.find("#imageGalleryContainer").hide();
            if (screenOption !== this.SavedViewOption)
                this.container.find("#ImageGridView").prop("checked", true);
        }
        
        if (isUpdateRecordStamps !== undefined && isUpdateRecordStamps) {
            pageHelper.updateRecordStamps(this.parentPage);                              // refresh parent stamps on screen
            this.refreshDefaultImage(this.parentPage);

            const tree = this.container.find(".doc-treeview").data("kendoTreeView");
            const nodeId = this.container.find(".doc-tree-node").val();

            if (nodeId > '') {
                const node = tree.findByUid(nodeId);
                let dataItem = tree.dataItem(node);

                if (dataItem != null) {
                    if (!dataItem.hasChildren) {
                        const parentNode = tree.parent(node);
                        dataItem = tree.dataItem(parentNode);
                    }
                    if (dataItem.hasChildren) {
                        dataItem.loaded(false);
                        dataItem.load();
                    }
                }
            }
            else {
                tree.dataSource.read();
            }
        }
        if (folderId > 0) {
            const gridContainer = $(this.container.find(".image-container"));
            gridContainer.data("folder-id", folderId);
        }
    }

    refreshDefaultImage = (parentPage) => {
        const deferred = $.Deferred();
        if (parentPage.defaultImageUrl) {
            $.get(parentPage.defaultImageUrl.replaceAll('amp;', ''))
                .done((response) => {
                    let imageContainer = null;
                    if (this.container)
                        imageContainer = this.container.find(".default-image-container");
                    else
                        imageContainer = $(".default-image-container").last();
                    $(imageContainer).html(response);
                    deferred.resolve();
                })
                .fail(() => {
                    deferred.reject();
                });

            return deferred.promise();
        }
    }

    refreshSharePointDefaultImage = (parentPage,abort) => {
        if (parentPage.defaultImageUrl) {

            $.get(parentPage.defaultImageUrl.replaceAll('amp;', ''))
                .done((response) => {
                   let imageContainer = null;
                   if (this.container)
                      imageContainer = this.container.find(".default-image-container");
                   else
                      imageContainer = $(".default-image-container").last();
                   $(imageContainer).html(response);
                })
                .fail((e) => {
                    if (!abort) {
                        if (e.status == 401) {
                            const baseUrl = $("body").data("base-url");
                            const url = `${baseUrl}/Graph/SharePoint`;

                            sharePointGraphHelper.getGraphToken(url, () => {
                                this.refreshSharePointDefaultImage(parentPage, true);
                            });
                        }
                    }
                });
        }
    }

    refreshGridPage = () => {
        const grid = this.container.find(".imageGridView");
        if (grid.length > 0) {
            console.log("refreshGridPage");
            grid.data("kendoGrid").dataSource.read();
            //grid.data("kendoGrid").dataSource.page(1); //issue with sharepoint
        }
    }

    refreshListViewPage = () => {
        const listViewImages = this.container.find(".imageListView");
        if (listViewImages.length > 0) {
            console.log("refreshListViewPage");
            listViewImages.data("kendoListView").dataSource.read();
            //listViewImages.data("kendoListView").dataSource.page(1); //issue with sharepoint
        }
    }

    refreshGrid = () => {
        const grid = this.container.find(".imageGridView");
     
        if (grid.length > 0)
            if (this.serverOperation) {
                grid.data("kendoGrid").dataSource.fetch(() => {
                    const images = grid.find("img");
                    if (images.length > 0) {
                        images.attr("data-src-retry", 3);

                        const self = this;
                        images.on("error", function () {
                            self.imageOnError(this);
                        });
                    }
                });
            }
            else {
                grid.data("kendoGrid").dataSource.read();
            }
           
    }

    refreshListView = () => {
        const listViewImages = this.container.find(".imageListView");
        const dataSource = listViewImages.data("kendoListView").dataSource;

        if (this.serverOperation) {
            dataSource.fetch(() => {
                this.showListButtons(dataSource.total() > 0);
                const images = listViewImages.find("img");
                images.attr("data-src-retry", 3);

                const self = this;
                images.on("error", function () {
                    self.imageOnError(this);
                });
            });
        }
        else {
            cpiLoadingSpinner.show();
            dataSource.read().then(() => {
                cpiLoadingSpinner.hide();
            });
        }
    }

    imageOnError=(img)=> {
        const image = $(img);
        const retryAttr = image.data("src-retry");
        let retry = +retryAttr;
        retry = retry - 1;

        if (retry > 0) {
            image.data("src-retry", retry);
            setTimeout(() => {
                const src = image.attr("src");
                image.attr("src", src);
            }, 1000);
        }
    }

    // on gallery view data-bound, select first image
    onDataBound(e) {
        // automatically select first image
        const listView = e.sender;
        const firstImage = listView.element.children().first();
        listView.select(firstImage);
    }

    // Drag and Drop Events
    onUpload = (e) => {
        const gridContainer = $(this.container.find(".image-container"));
        const folderId = gridContainer.data("folder-id");
        e.data = { folderId: folderId};
    }

    onUploadAsyncFail(e) {
        let error = e.XMLHttpRequest.responseText;
        if (error === "")
            error = "Error occurred during upload.";
        //alert(error);
        pageHelper.showErrors(error);
    }

    onUploadAsyncSuccess = (e) => {
        pageHelper.handleSignatureWorkflow(e.response, () => { this.refreshDisplay(true, e.response.folderId); });
        pageHelper.handleEmailWorkflow(e.response);
    }

    onUploadDefaultImageSuccess = (e,parentPage) => {
        pageHelper.handleEmailWorkflow(e.response);
        this.refreshDefaultImage(parentPage);
    }

    showErrors = function (errors) {
        pageHelper.showErrors(errors);
    }

    hideErrors = function () {
        const errorSummary = $(pageHelper.errorSummary);
        const errorContainer = $(errorSummary).find(".content");
        errorContainer.empty();
        errorSummary.addClass("d-none");
    }


    contextMenuOnOpen(e) {
        const items = [{
            text: "<span class='fal fa-search fa-fixed-width'></span>View",
            attr: { "data-action": "view" },
            encoded: false
        }];

        items.push({
            text: "<span class='fal fa-file-plus fa-fixed-width'></span>Add",
            attr: { "data-action": "add" },
            encoded: false
        });

        items.push({
            text: "<span class='fal fa-file-edit fa-fixed-width'></span>Edit",
            attr: { "data-action": "edit" },
            encoded: false
        });

        items.push({
            text: "<span class='fal fa-file-times fa-fixed-width'></span>Delete",
            attr: { "data-action": "delete" },
            encoded: false
        });

        this.setOptions({
            dataSource: items
        });

    }

    contextMenuOnSelect = (e) => {
        const selectedImage = this.getSelected();
        const selected = $(e.item);
        const action = selected.data("action");

        switch (action) {
            case "view":
                this.view(selectedImage);
                break;

            case "add":
                this.add();
                break;

            case "edit":
                this.edit(selectedImage);
                break;

            case "delete":
                e.preventDefault();
                this.kendoListViewDeleteImage(selectedImage);
                break;
        }
    }

    view = (dataItem) => {
        const listViewButtons = this.container.find("#imageListButtonsContainer");
        const button = listViewButtons.find("#view");
        const imageName = dataItem.DocFileName;

        // filter out url/links
        if (!imageName) {
            const docUrl = dataItem.DocUrl;
            if (docUrl.startsWith("www") || docUrl.startsWith("http") || docUrl.startsWith("ftp") || docUrl.startsWith("file")) {
                window.open(docUrl, '_blank');
                return;
            }
        }
        const url = button.attr('data-viewer-url') + "&imageFile=" + imageName + "&screenCode=" + dataItem.ScreenCode + "&key=" + dataItem.ParentId + "&fileType=" + (dataItem.FolderId > 0 ? 7 : 0);
        documentPage.zoomDocument(url);
    }

    add = () => {
        const grid = this.container.find(".imageGridView").data("kendoGrid");
        const url = grid.dataSource.transport.options.create.url;
        this.openPopEditor(url,0,true);
    }

    edit = (dataItem) => {
        const grid = this.container.find(".imageGridView").data("kendoGrid");
        let url = grid.dataSource.transport.options.update.url;
        this.openPopEditor(url, dataItem.DocId,false);
    }

    checkout = (dataItem, checkoutUrl, grid) => {
        const gridContainer = $(this.container.find(".image-container"));
        const documentLink = gridContainer.data("link");
        let url = checkoutUrl;
        //url = url + "?parentId=" + dataItem.ParentId + "&id=" + dataItem.DocId + "&documentLink=" + documentLink;
        url = url + "?id=" + dataItem.DocId + "&documentLink=" + documentLink;

        $.get(url.replace("ImageCheckout","ImageIsLocked")).done(function () {
            let checkoutForm = $("#documentCheckout").last();
            if (checkoutForm.length > 0) {
                checkoutForm.remove();
            }
            $(`<form action="${url}" method="post" id="documentCheckout"></form>`).appendTo('body').submit();
            setTimeout(() => { grid.dataSource.read(); }, 1500);
        })
            .fail((e) => { this.showErrors(e.responseText) });
    }

    // delete from gallery view
    kendoListViewDeleteImage = (dataItem) => {
        const grid = this.container.find(".imageGridView").data("kendoGrid");
        const deletePrompt = grid.options.editable.confirmDelete;

        const listView = this.container.find(".imageListView").data("kendoListView");
        if (confirm(deletePrompt)) {
            $.ajax({
                url: listView.dataSource.transport.options.destroy.url,
                data: { deleted: pageHelper.getKendoDataItemProperties(dataItem) },
                type: "POST",
                success: () => {
                    listView.dataSource.remove(dataItem);
                    this.hideErrors();
                    this.configureListButtons();
                    this.refreshDisplay(true);
                },
                error: (e) => {
                    this.showErrors(e.responseText);
                }
            });
        }
    }

    kendoGridDeleteImage = (e, dataItem) => {
        const grid = this.container.find("#" + e.delegateTarget.id).data("kendoGrid");
        const deletePrompt = grid.options.editable.confirmDelete;
        if (confirm(deletePrompt)) {
            $.ajax({
                url: grid.dataSource.transport.options.destroy.url,
                data: { deleted: pageHelper.getKendoDataItemProperties(dataItem) },
                type: "POST",
                success: () => {
                    grid.removeRow($(e.currentTarget).closest("tr"));
                    grid.dataSource._destroyed = [];
                    this.hideErrors();
                    this.refreshDisplay(true);
                },
                error: (e) => {
                    this.showErrors(e.responseText);
                }
            });
        }
    }

    openPopEditor = (url, id, addMode) => {
        const deferred = new $.Deferred();
        const gridContainer = $(this.container.find(".image-container"));
        const param = {
            documentLink: gridContainer.data("link"),
            roleLink: gridContainer.data("role-link")
        }
        if (addMode)
            param.folderId = gridContainer.data("folder-id");
        else
            param.id = id;

        //console.log(url, param);
        cpiLoadingSpinner.show();
        $.get(url,param)
            .done((result) => {
                cpiLoadingSpinner.hide();
                //clear all existing hidden popups to avoid kendo id issue
                $(".site-content .popup").empty();
                const popupContainer = $(".site-content .popup").last();
                popupContainer.html(result);

                this.initializeEditor().then(() => {
                    deferred.resolve();
                });
            })
            .fail((e) => {
                cpiLoadingSpinner.hide();
                this.showErrors(e.responseText);
                return deferred.reject(e);
            });

        return deferred.promise();
    }

    initializeEditor = () => {
        const deferred = new $.Deferred();
        const self = this;
        const container = $("#documentEditorDialog");
        container.modal("show");
        const form = $("#documentEditorForm");
        form.floatLabels();

        var formChanged = false;
        form.find(":input").each(function () {
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

            if (formChanged && !confirm(container.data("cancel-message"))) {                
                e.stopPropagation();
            }            
                        
            //check New Actions workflow
            var tempFormData = new FormData(form[0]);   
            $.get(container.data("action-workflow-url"), { docId: tempFormData.get('DocId'), driveItemId: tempFormData.get('DriveItemId') })
                .done((result) => {                    
                    if (result && result.length > 0) {                        
                        let promise = pageHelper.handleEmailWorkflow({ id: 0, sendEmail: "", emailurl: "", emailWorkflows: result });
                        promise = promise.then(() => {
                            self.refreshDocListView();
                        });
                    }                    
                });
        });

        // configure form submit logic
        form.submit((e) => {
            e.preventDefault();
            
            if (!this.validateUploadAction())
                return deferred.promise();

            pageHelper.hideErrors();

            var formData = new FormData(form[0]);

            var actGrid = $("#documentVerificationActGrid").data("kendoGrid");
            ////show error if IsActRequired = true and Action grid is empty
            //if (formData && formData.has('IsActRequired') && formData.get('IsActRequired') === 'true' && actGrid) {
            //    var gridData = actGrid.dataSource.data();
            //    if (gridData && gridData.length <= 0) {
            //        alert(container.data("missing-action"));
            //        return deferred.promise();
            //    }                
            //}

            //auto save changes on Action grid            
            if (actGrid && actGrid.dataSource.hasChanges()) {  
                $.when(pageHelper.kendoGridSave({
                    name: 'documentVerificationActGrid'
                    //popup windows closes after saving, no need to refresh
                    //,afterSubmit: () => { $("#documentVerificationActGrid").data("kendoGrid").dataSource.read(); }
                })).then(
                    function (e) {
                        save();
                    },
                    function (e) {
                        alert(e);
                        return deferred.promise();
                    },
                    null
                );                
            }
            else {
                save();
            }

            function save() {
                cpiLoadingSpinner.show();            
                $.ajax({
                    type: "POST",
                    url: form.attr("action"),
                    data: formData,
                    contentType: false, // needed for file upload
                    processData: false, // needed for file upload
                    success: (response) => {
                        cpiLoadingSpinner.hide();
                        pageHelper.handleSignatureWorkflow(response);
                        pageHelper.handleEmailWorkflow(response);
                        self.refreshDisplay(true);
                        $("#documentEditorDialog").modal("hide");
                        deferred.resolve();
                    },
                    error: function (e) {
                        cpiLoadingSpinner.hide();
                        pageHelper.showErrors(e);
                        deferred.reject();
                    }
                });
            }
            
        });
        return deferred.promise();
    }


    getSelected = () => {
        const listView = this.container.find(".imageListView").data("kendoListView");
        const index = listView.select().index();
        const dataItem = listView.dataSource.view()[index];
        return dataItem;
    }

    validateUploadAction = () => {
        const container = $("#documentEditorDialog");
        //console.log(container);

        const isInsertAction = () => {
            const el = container.find("#DocId");
            const id = el.val();

            if (id == 0) //should be ==
                return true;
            else
                return false;
        };

        const isEmpty = (name) => {
            const el = container.find("input[name=" + name + "]");
            if (el.length > 0) {
                const value = el.val();
                if (value === "") {
                    return true;
                }
            }
            return false;
        };

        const showError = (name) => {
            const el = container.find("input[name=" + name + "]");
            if (el.length > 0) {
                const error = el.val();
                pageHelper.showErrors(error);
            }
        };

        const docType = container.find("input[name=DocTypeId]").data("kendoComboBox");
        const action = docType == undefined ? "" : docType.text().toLowerCase();

        if (action === "link") {
            container.find("input[name=FileId]").val("");
            if (isEmpty("DocUrl")) {
                showError("UrlError");
                container.find("#urlRow").addClass("border border-danger");
                return false;
            }
        }
        else {
            container.find("input[name=DocUrl]").val("");
            if (isEmpty("UploadedFiles") && isInsertAction()) {
                showError("UploadedFilesError");
                container.find("#uploadImageRow").addClass("border border-danger");
                return false;
            }
        }
        
        //else if (action === "copyImage") {
        //    if (isEmpty("ImageSelected")) {
        //        showError("ImageSelectedError");
        //        return false;
        //    }
        //}
        return true;
    }
    
    allowDropMail = (event) =>  {
        event.preventDefault();
    }

    dropMail = (event, successCallback) => {
        event.preventDefault();

        const el = $(event.target);
        const dropZone = el.closest(".drop-zone");
        const targetId = event.dataTransfer.getData("targetId");
        const mailbox = event.dataTransfer.getData("mailbox");
        let messageIds = [];
        let fileNames = [];

        if (event.dataTransfer.getData("messageIds"))
            messageIds = JSON.parse(event.dataTransfer.getData("messageIds"));

        if (event.dataTransfer.getData("fileNames"))
            fileNames = JSON.parse(event.dataTransfer.getData("fileNames"));

        const saveMessage = () => {
            const url = dropZone.data("save-email-url");

            if (url) {
                const detailContainer = el.closest(".cpiDataContainer");
                const folderId = detailContainer.find(".image-container").data("folder-id");
                const verificationToken = detailContainer.find("form").find("input[name=__RequestVerificationToken]").val()
                let data = { ids: messageIds, fileNames: fileNames, folderId: folderId, mailbox: mailbox, __RequestVerificationToken: verificationToken };

                if (event.data != undefined)
                    jQuery.extend(data, event.data);

                cpiLoadingSpinner.show();
                $.post(url, data)
                    .done((response) => {
                        cpiLoadingSpinner.hide();
                        updateStatus("ok");
                        if (successCallback)
                            successCallback({ response: response });
                        else
                            this.onUploadAsyncSuccess({ response: response });
                    })
                    .fail((error) => {
                        cpiLoadingSpinner.hide();
                        updateStatus("fail");
                        this.showErrors(error);
                    });
            }
            else {
                setTimeout(() => {
                    updateStatus("cancel");
                }, 1000);
            }
        }

        const updateStatus = (status) => {
            localStorage.setItem(targetId, status);
        }

        if (messageIds.length > 0 && fileNames.length > 0) {
            let dropConfirm = messageIds.length == 1 ? dropZone.data("drop-confirm") : dropZone.data("drop-confirm-multiple");

            if (dropConfirm) {
                dropConfirm = dropConfirm.replace("{subject}", event.dataTransfer.getData("subject")).replace("{sender}", event.dataTransfer.getData("sender")).replace("{count}", messageIds.length);
                cpiConfirm.confirm(window.cpiBreadCrumbs.getTitle(), dropConfirm, saveMessage, null, null, function () {
                    updateStatus("cancel");
                });
            }
            else
                saveMessage();
        }
    }
}

//const instance = new Image();
//export default instance;
