
export default class Letter {

    constructor() {
        this.gridDisplayDetail = this.gridDisplayDetail.bind(this);
        this.generateLetter = this.generateLetter.bind(this);
        this.letterContainer = null;
        this.systemType = '';
        this.screenCode = '';
    }

    initialize = (options) => {
        this.letterContainer = $(options.letterContainer);
        this.systemType = options.systemType;
        this.screenCode = options.screenCode;
        const self = this;

        const dialogContainer = $("#letterGenDialog");
        dialogContainer.modal("show");

        $("#previewLetterButton").on("click", function () {
            self.generateLetter(false);
        });

        $("#generateLetterButton").on("click", function () {
            self.generateLetter(true);
        });

        const letterDialog = $(options.letterDialog);
        letterDialog.on('hidden.bs.modal', function () {
            self.refreshDocsOutGrid(letterDialog);
        })

        letterDialog.on('click', ".letter-search-submit", function () {
            self.refreshGrid("#letterListGrid")
        })

        if ($(document).ready(() => {
            self.refreshGrid("#letterListGrid");
            letterDialog.find("#letter-search").on("keydown", (e) => {
                const keyCode = e.keyCode || e.which;
                if (keyCode === 13) {
                    self.refreshGrid("#letterListGrid")
                }
            });
        }));
    }

    letterCategory_change = () => this.refreshGrid("#letterListGrid");

    refreshDocsOutGrid = (entryForm) => {
        const container = entryForm.closest(".cpiDataContainer");
        if (container) {
            if (container.attr("id")) {
                const id = container.attr("id").split("-")[0];
                const grid = $(`#docsOutGrid_${id}`);
                if (grid.length > 0)
                    grid.data("kendoGrid").dataSource.read();
            }
        }
    }

    // pop-up/main screen letter generation; mass generation on setup screen is in letterSetup.js
    generateLetter(isLog) {
        const self = this;

        const letInfo = self.getSelectedLetInfo();
        if (letInfo === null) {
            const msg = this.letterContainer.data("no-letter-selected");
            cpiAlert.warning(msg);
            return;
        }

        const contactGrid = $("#contactGrid").data("kendoGrid");
        const contactData = contactGrid.dataSource.data();
        if (contactData.length === 0) {
            const msg = this.letterContainer.data("no-contacts");
            cpiAlert.warning(msg);
            return;
        }

        const selectedContacts = [];
        // don't use grid dataSource - the ClientTemplate will not be updated
        //for (let i = 0; i < contactData.length; i++) {
        //    if (contactData[i].IsGenerate) {
        //        selectedContacts.push({ EntityId: contactData[i].EntityId, ContactId: contactData[i].ContactId });
        //    }
        //}

        const checkedContacts = contactGrid.element.find(".let-generate:checked");
        checkedContacts.each(function (i, e) {
            let row = $(e).closest("tr");
            let dataItem = contactGrid.dataItem(row);
            selectedContacts.push({ EntityId: dataItem.EntityId, ContactId: dataItem.ContactId });
        });

        if (selectedContacts.length === 0) {
            const msg = this.letterContainer.data("no-selected-contacts");
            cpiAlert.warning(msg);
            return;
        }

        if (letInfo.templateFile === "") {
            const msg = this.letterContainer.data("no-template-file");
            cpiAlert.warning(msg);
            return;
        }

        const title = this.letterContainer.data("gen-title");
        let msg = "";
        if (isLog)
            msg = this.letterContainer.data("gen-log-message");
        else
            msg = this.letterContainer.data("gen-nolog-message");

        msg +=  "<br>" + "<span style='font-style:italic; font-weight: bold;' class='pt-2 pb-2'>" + letInfo.letName + "</span >";

        

        cpiConfirm.confirm(title, msg, function () {
            const data = {
                letId: letInfo.letId, isLog: isLog, systemType: letInfo.systemType, letterScreenCode: letInfo.letterScreenCode, recordId: letInfo.recordId,
                selectedContacts: selectedContacts, screenSource: "genpopup"
                //__RequestVerificationToken: $($(`input[name='${pageHelper.verificationTokenFormData}']`)[0]).val()
            };
            $("#letterFormParams").val(JSON.stringify(data));
            $("#letterForm").submit();
            //submitForm();
        });
    }

    //submitFormPromise = () => {
    //    return new Promise(resolve => $("#letterForm").submit());
    //};

    //submitForm = async () => {
    //    await submitFormPromise();
    //    console.log("after promise");
    //};

    //enableGenerateButton() {      // need to run this after completion of contact grid read
    //    const isGridEmpty = $("#contactGrid").data("kendoGrid").dataSource.data().length === 0;
    //    $("#generateLetterButton").prop('disabled', isGridEmpty);
    //    $("#previewLetterButton").prop('disabled', isGridEmpty);
    //}


    gridDisplayDetail(e) {
        if (e === null)
            return;
        const self = this;

        // add ajax to call proc_UpdatePopupFilter
        const url = this.letterContainer.data("filter-url");
        const recordKey = $("#letterRecordKey").val();
        const recordId = $("#letterRecordId").val();
        const data = $.extend({}, self.getSelectedLetId(), { recordKey: recordKey, recordId: recordId });
        
        $.ajax({
            type: "POST",
            url: url,
            headers: { "RequestVerificationToken": $($(`input[name='${pageHelper.verificationTokenFormData}']`)[0]).val() },
            data: data
        })
        .done(function () {
            self.refreshGrid("#letterFilterGrid");
            self.refreshGrid("#contactGrid");
        })
        .fail(function (e) {
            pageHelper.showErrors(e);
        });

    }

    refreshGrid (gridId) {
        const gridHandle = $(gridId);
        const grid = gridHandle.data("kendoGrid");
        grid.dataSource.read();
       
    }

    getSelectedLetId() {
        const grid = $("#letterListGrid").data("kendoGrid");
        const dataItem = grid.dataItem(grid.select());
        return { letId: dataItem.LetId };
    }

    getSelectedLetInfo() {
        const letterScreenCode = $("#letterScreenCode").val();
        const recordId = $("#letterRecordId").val();
        const grid = $("#letterListGrid").data("kendoGrid");
        if (grid.select().length === 0)
            return null;
        const dataItem = grid.dataItem(grid.select());
        return {
            letId: dataItem.LetId, letName: dataItem.LetName, templateFile: dataItem.TemplateFile,
            systemType: dataItem.SystemType, letterScreenCode: letterScreenCode, recordId: recordId
        };
    }

    getSearchedLetter = () => {
        console.log($("#letterGenDialog [name='Tag']"));
        console.log($("#letterGenDialog [name='Tag']").val());
        return {
            systemType: this.systemType,
            screenCode: this.screenCode,
            letterName: $("#letterGenDialog #letter-search").val(),
            letCatId: $("#letterGenDialog [name='LetterCategory']").data("kendoComboBox").value(),
            letSubCatId: $("#letterGenDialog [name='LetSubCat']").data("kendoComboBox").value(),
            tags: $("#letterGenDialog [name='Tag']").val()
        }
    }

    pushDocsOutFileToDocuSign = (e, letterDocLibrary, efsLogDocLibrary, roleLink, grid,callBack) => {
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + grid).data("kendoGrid").dataItem(row);
        let retry = 0;

        if (dataItem) {
            const docLibrary = dataItem.DocumentCode == "Let" ? letterDocLibrary : efsLogDocLibrary;
            const title = $("#" + grid).closest(".content-signature").data("confirm");
            if (dataItem.SentToDocuSign) {
                const resendPrompt = $("#" + grid).closest(".content-signature").data("resend-prompt");
                cpiConfirm.confirm(title, resendPrompt, function () {
                    pushDoc();
                });
            }
            else {
                const sendPrompt = $("#" + grid).closest(".content-signature").data("send-prompt");
                cpiConfirm.confirm(title, sendPrompt, function () {
                    pushDoc();
                });
            }

            function pushDoc() {
                const baseUrl = $("body").data("base-url");
                const url = `${baseUrl}/Shared/DocuSign/SendEnvelopeFromDocsOutLog`;
                const docsOut = {
                    UserFile: {
                        FileName: dataItem.LogFile,
                        StrId: dataItem.ItemId,
                        Name: dataItem.Document
                    },
                    Signer: {
                        Name: dataItem.SignerName,
                        Email: dataItem.SignerEmail,
                        AnchorCode: dataItem.SignerAnchorCode
                    },
                    QESetupId: dataItem.SignatureQESetupId,
                    ParentId: dataItem.RecKey,
                    ScreenCode: dataItem.ScreenCode,
                    RoleLink: roleLink,
                    SystemTypeCode: dataItem.SystemType,
                    SharePointDocLibrary: docLibrary,
                    DocLogId: dataItem.DocLogId,
                    DocumentCode: dataItem.DocumentCode
                };

                cpiLoadingSpinner.show();
                $.post(url, { docsOut })
                    .done((result) => {
                        cpiLoadingSpinner.hide();
                        $("#" + grid).data("kendoGrid").dataSource.read();
                        if (callBack) callBack();

                        pageHelper.showSuccess(result.success);
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

    pullDocsOutFileFromDocuSign = (e, grid,isSharePointOn,callBack) => {
        const row = $(e.target).closest("tr");
        const dataItem = $("#" + grid).data("kendoGrid").dataItem(row);
        let retry = 0;

        if (dataItem && dataItem.SentToDocuSign) {
            if (dataItem.SignatureCompleted) {
                const repullPrompt = $("#" + grid).closest(".content-signature").data("repull-prompt");
                const title = $("#" + grid).closest(".content-signature").data("confirm");

                cpiConfirm.confirm(title, repullPrompt, function () {
                    pullCompletedDoc();
                });
            }
            else {
                const pullPrompt = $("#" + grid).closest(".content-signature").data("pull-prompt");
                const title = $("#" + grid).closest(".content-signature").data("confirm");

                cpiConfirm.confirm(title, pullPrompt, function () {
                    pullCompletedDoc();
                });
            }

            function pullCompletedDoc() {
                const baseUrl = $("body").data("base-url");
                const url = `${baseUrl}/Shared/DocuSign/GetSignedDocsOutAndSave${isSharePointOn ? "ToSharePoint":""}`;

                cpiLoadingSpinner.show();
                $.post(url, {
                    viewModelParam: {
                        DocLogId: dataItem.DocLogId,
                        ParentId: dataItem.RecKey,
                        EnvelopeId: dataItem.EnvelopeId,
                        LetFile: dataItem.LogFile,
                        ScreenCode: dataItem.ScreenCode,
                        SystemTypeCode: dataItem.SystemType,
                        DocumentCode: dataItem.DocumentCode
                    }
                })
                    .done((result) => {
                        $("#" + grid).data("kendoGrid").dataSource.read();
                        if (callBack) callBack();
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

    downloadDocsOutFile(e,grid, isSharePointOn) {
        const row = $(e.target).closest("tr");
        const link = $(row).find("a.download-file");
        if (link) {
            let url = link.data("url");
            if (isSharePointOn) {
                const dataItem = $("#" + grid).data("kendoGrid").dataItem(row);
                let docLibrary = "";

                if (dataItem.DocumentCode == "Let")
                    docLibrary = "Letters Log";
                else if (dataItem.DocumentCode == "EFS")
                    docLibrary = "IP Forms Log";

                url = url.replace("{docLibrary}", docLibrary);
            }
            window.open(url);
        }
    }
}