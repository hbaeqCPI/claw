import ActivePage from "../activePage";

export default class DMSFAQDocPage extends ActivePage {
    constructor() {
        super();        
    }    
   
    initializeEditor = () => {
        const container = $("#documentEditorDialog");
        container.modal("show");
        const form = $("#documentEditorForm");
        form.floatLabels();
        const self = this;
        
        // configure form submit logic
        form.submit((e) => {
            e.preventDefault();             
            
            if (!this.validateUploadAction()) {                
                return; 
            }                

            pageHelper.hideErrors();  
            
            var formData = new FormData(form[0]);            
            cpiLoadingSpinner.show();
            $.ajax({
                type: "POST",
                url: form.attr("action"),
                data: formData,
                contentType: false, // needed for file upload
                processData: false, // needed for file upload
                success: (result) => {
                    self.cpiLoadingSpinner.hide();
                    $("#documentEditorDialog").modal("hide");
                    const grid = $("#dmsFAQDocSearchResults-Grid").data("kendoGrid");
                    if (grid) {
                        grid.dataSource.read().then(function() {
                            setTimeout(function() {pageHelper.showSuccess(result.success);},500);
                        });
                    }
                    else {
                        setTimeout(function() {pageHelper.showSuccess(result.success);},500);
                    }                    
                },
                error: function (e) {
                    self.cpiLoadingSpinner.hide();
                    pageHelper.showErrors(e);
                }
            });
            
        });
    }

    validateUploadAction = () => {
        const container = $("#documentEditorDialog");        

        const isInsertAction = () => {
            const el = container.find("#FaqId");
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
                alert(error);
            }
        };

        const docType = container.find("input[name=DocTypeId]").data("kendoComboBox");
        const action = docType == undefined ? "" : docType.text().toLowerCase();

        if (action === "link") {
            container.find("input[name=FileId]").val("");
            container.find("input[name=Message]").val(null);
            if (isEmpty("DocUrl")) {
                showError("UrlError");
                container.find("#urlRow").addClass("border border-danger");
                return false;
            }
        }
        else if (action === "notification") {
            container.find("input[name=FileId]").val("");
            container.find("input[name=DocTypeId]").val("0");
            container.find("input[name=DocUrl]").val(null);
            if (isEmpty("Message")) {
                showError("MessageError");
                container.find("#messageRow").addClass("border border-danger");
                return false;
            }
        }
        else {
            container.find("input[name=DocUrl]").val(null);
            container.find("input[name=Message]").val(null);
            if (isEmpty("UploadedFiles") && isInsertAction()) {
                showError("UploadedFilesError");
                container.find("#uploadImageRow").addClass("border border-danger");
                return false;
            }
        }        
        
        return true;
    }

    viewImage = (dataItem) => {
        let url = $("body").data("base-url") + "/DMS/FAQDoc/ViewFAQ";
        const param = { docFileName: dataItem.DocFileName };
        $.get(url, param)
            .done((result) => {
                cpiLoadingSpinner.hide();
                const popupContainer = $(".site-content .popup");
                popupContainer.empty();
                popupContainer.html(result);
            })
            .fail((e) => {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(e.responseText);
            });  
    }

    editFaq = (faqId) => {
        let url = $("body").data("base-url") + "/DMS/FAQDoc/AddFAQ";
        const param = { faqId: faqId };
        $.get(url, param)
            .done((result) => {
                $(".site-content .popup").empty();
                const popupContainer = $(".site-content .popup").last();
                popupContainer.html(result);
                dmsFAQDocPage.initializeEditor();
            })
            .fail((e) => {
                pageHelper.showErrors(e.responseText);
            });
    }

    editSPFaq = (faqId) => {
        const baseUrl = $("body").data("base-url");
        const authenticatedCheckUrl = `${baseUrl}/Shared/SharePointGraph/IsAuthenticated`;
        $.get(authenticatedCheckUrl)
            .done(function () {
                initializeSPEditor();
            })
            .fail(function (e) {
                if (e.status == 401) {
                    const baseUrl = $("body").data("base-url");
                    const url = `${baseUrl}/Graph/SharePoint`;

                    sharePointGraphHelper.getGraphToken(url, () => {  
                        initializeSPEditor();
                    });
                }
                else {
                    pageHelper.showErrors(e.responseText);
                }
            });

        function initializeSPEditor() {
            let url = $("body").data("base-url") + "/DMS/FAQDoc/AddFAQ";
            const param = { faqId: faqId };
            $.get(url, param)
                .done((result) => {
                    $(".site-content .popup").empty();
                    const popupContainer = $(".site-content .popup").last();
                    popupContainer.html(result);
                    dmsFAQDocPage.initializeEditor();
                })
                .fail((e) => {
                    pageHelper.showErrors(e.responseText);
                });
        }        
   }  
   
    viewSPFaq = (docLibrary, driveItemId) => {
        const baseUrl = $("body").data("base-url");
        const authenticatedCheckUrl = `${baseUrl}/Shared/SharePointGraph/IsAuthenticated`;
        $.get(authenticatedCheckUrl)
            .done(function () {
                sharePointGraphHelper.previewFile(docLibrary,driveItemId);
            })
            .fail(function (e) {
                if (e.status == 401) {
                    const baseUrl = $("body").data("base-url");
                    const url = `${baseUrl}/Graph/SharePoint`;

                    sharePointGraphHelper.getGraphToken(url, () => {  
                        sharePointGraphHelper.previewFile(docLibrary,driveItemId);
                    });
                }
                else {
                    pageHelper.showErrors(e.responseText);
                }
            });
    }

}