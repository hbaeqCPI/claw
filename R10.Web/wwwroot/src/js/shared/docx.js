
export default class DOCX {

    constructor() {
        this.gridDisplayDetail = this.gridDisplayDetail.bind(this);
        this.generateDOCX = this.generateDOCX.bind(this);
        this.docxContainer = null;
        this.systemType = '';
        this.screenCode = '';
    }

    initialize = (options) => {
        this.docxContainer = $(options.docxContainer);
        this.systemType = options.systemType;
        this.screenCode = options.screenCode;
        const self = this;

        const dialogContainer = $("#docxGenDialog");
        dialogContainer.modal("show");

        $("#previewDOCXButton").on("click", function () {
            self.generateDOCX(false);
        });

        $("#generateDOCXButton").on("click", function () {
            self.generateDOCX(true);
        });

        const docxDialog = $(options.docxDialog);
        docxDialog.on('hidden.bs.modal', function () {
            self.refreshDocsOutGrid(docxDialog);
        })

        docxDialog.on('click', ".docx-search-submit", function () {
            self.refreshGrid("#docxListGrid")
        })

        if ($(document).ready(() => {
            self.refreshGrid("#docxListGrid");
            docxDialog.find("#docx-search").on("keydown", (e) => {
                const keyCode = e.keyCode || e.which;
                if (keyCode === 13) {
                    self.refreshGrid("#docxListGrid")
                }
            });
        }));
    }

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

    // pop-up/main screen docx generation; mass generation on setup screen is in docxSetup.js
    generateDOCX(isLog) {
        const self = this;

        const docxInfo = self.getSelectedDOCXInfo();
        if (docxInfo === null) {
            const msg = this.docxContainer.data("no-docx-selected");
            cpiAlert.warning(msg);
            return;
        }

        //const contactGrid = $("#contactGrid").data("kendoGrid");
        //const contactData = contactGrid.dataSource.data();
        //if (contactData.length === 0) {
        //    const msg = this.docxContainer.data("no-contacts");
        //    cpiAlert.warning(msg);
        //    return;
        //}

        //const selectedContacts = [];
        // don't use grid dataSource - the ClientTemplate will not be updated
        //for (docx i = 0; i < contactData.length; i++) {
        //    if (contactData[i].IsGenerate) {
        //        selectedContacts.push({ EntityId: contactData[i].EntityId, ContactId: contactData[i].ContactId });
        //    }
        //}

        //const checkedContacts = contactGrid.element.find(".docx-generate:checked");
        //checkedContacts.each(function (i, e) {
        //    let row = $(e).closest("tr");
        //    let dataItem = contactGrid.dataItem(row);
        //    selectedContacts.push({ EntityId: dataItem.EntityId, ContactId: dataItem.ContactId });
        //});

        //if (selectedContacts.length === 0) {
        //    const msg = this.docxContainer.data("no-selected-contacts");
        //    cpiAlert.warning(msg);
        //    return;
        //}

        if (docxInfo.templateFile === "") {
            const msg = this.docxContainer.data("no-template-file");
            cpiAlert.warning(msg);
            return;
        }

        const title = this.docxContainer.data("gen-title");
        let msg = "";
        if (isLog)
            msg = this.docxContainer.data("gen-log-message");
        else
            msg = this.docxContainer.data("gen-nolog-message");

        msg +=  "<br>" + "<span style='font-style:italic; font-weight: bold;' class='pt-2 pb-2'>" + docxInfo.docxName + "</span >";

        

        cpiConfirm.confirm(title, msg, function () {
            const data = {
                docxId: docxInfo.docxId, isLog: isLog, systemType: docxInfo.systemType, docxScreenCode: docxInfo.docxScreenCode, recordId: docxInfo.recordId, screenSource: "genpopup"
                //selectedContacts: [], 
                //__RequestVerificationToken: $($(`input[name='${pageHelper.verificationTokenFormData}']`)[0]).val()
            };
            $("#docxFormParams").val(JSON.stringify(data));
            $("#docxForm").submit();
            //submitForm();
        });
    }

    //submitFormPromise = () => {
    //    return new Promise(resolve => $("#docxForm").submit());
    //};

    //submitForm = async () => {
    //    await submitFormPromise();
    //    console.log("after promise");
    //};

    //enableGenerateButton() {      // need to run this after completion of contact grid read
    //    const isGridEmpty = $("#contactGrid").data("kendoGrid").dataSource.data().length === 0;
    //    $("#generateDOCXButton").prop('disabled', isGridEmpty);
    //    $("#previewDOCXButton").prop('disabled', isGridEmpty);
    //}


    gridDisplayDetail(e) {
        if (e === null)
            return;
        const self = this;

        // add ajax to call proc_UpdatePopupFilter
        const url = this.docxContainer.data("filter-url");
        const recordKey = $("#docxRecordKey").val();
        const recordId = $("#docxRecordId").val();
        const data = $.extend({}, self.getSelectedDOCXId(), { recordKey: recordKey, recordId: recordId });
        
        $.ajax({
            type: "POST",
            url: url,
            headers: { "RequestVerificationToken": $($(`input[name='${pageHelper.verificationTokenFormData}']`)[0]).val() },
            data: data
        })
        .done(function () {
            self.refreshGrid("#docxFilterGrid");
            //self.refreshGrid("#contactGrid");
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

    getSelectedDOCXId() {
        const grid = $("#docxListGrid").data("kendoGrid");
        const dataItem = grid.dataItem(grid.select());
        return { docxId: dataItem.DOCXId };
    }

    getSelectedDOCXInfo() {
        const docxScreenCode = $("#docxScreenCode").val();
        const recordId = $("#docxRecordId").val();
        const grid = $("#docxListGrid").data("kendoGrid");
        if (grid.select().length === 0)
            return null;
        const dataItem = grid.dataItem(grid.select());
        return {
            docxId: dataItem.DOCXId, docxName: dataItem.DOCXName, templateFile: dataItem.TemplateFile,
            systemType: dataItem.SystemType, docxScreenCode: docxScreenCode, recordId: recordId
        };
    }

    getSearchedDOCX=()=> {
        return {
            systemType: this.systemType,
            screenCode: this.screenCode,
            docxName: $("#docxGenDialog #docx-search").val()
        }
    }

}