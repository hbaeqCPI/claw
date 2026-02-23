import Image from "../image";
import ActivePage from "../activePage";

export default class PacClearancePage extends ActivePage {

    constructor() {
        super();
        this.caseNumberSearchValueMapperUrl = "";  
        this.image = new Image();
        this.caseNumberSearchValueMapperUrl = "";
        this.docServerOperation = true;
    }

    initialize = (screen, id, isSharePointIntegrationOn) => {
        this.tabsLoaded = [];
        this.docServerOperation = !isSharePointIntegrationOn;
        this.tabChangeSetListener();

        $(document).ready(() => {

            const inventorsGrid = $(`#inventorsGrid_${screen}`);
            inventorsGrid.on("click", ".inventorLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = inventorsGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.InventorDetail.InventorID);
                pageHelper.openLink(linkUrl, false);
            });
            
            ////Load first question tab
            //const questionTabs = $("#questionTabs").val().split("|");
            //if (questionTabs.length) {
            //    const gridName = `#${questionTabs[0]}Grid_${this.mainDetailContainer}`;
            //    const grid = $(gridName).data("kendoGrid");
            //    grid.dataSource.read();
            //    //prevent click outside the input box
            //    grid.table.on("click", ".answer-box", function () {
            //        return false;
            //    });
            //}  
        })              
    }

    caseNumberSearchValueMapper = (options) => {
        const url = pacClearancePage.caseNumberDetailValueMapperUrl;
           $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }

    caseNumberDetailValueMapper = (options) => {
        const url = pacClearancePage.caseNumberDetailValueMapperUrl;

        $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }

    tabChangeSetListener() {
        $('#clearanceDetailTab a').on('click', (e) => {
            e.preventDefault();
            const tab = e.target.id;
            if (this.tabsLoaded.indexOf(tab) === -1) {
                this.tabsLoaded.push(tab);
                this.loadTabContent(tab);
            }
        });
    }

    loadTabContent = (tab) => {
        const self = this;

        switch (tab) {
            
            case "clearanceDetailDocumentsTab":
                $(document).ready(() => {
                    this.image.initializeImage(this, this.docServerOperation);
                });
                break;
           
            case "clearanceDetailCorrespondenceTab":
                $(document).ready(() => {
                    const docsOutGrid = $(`#docsOutGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    docsOutGrid.dataSource.read();
                });
                break;

            case "clearanceDetailStatusHistoryTab":
                $(document).ready(() => {
                    const statusHistoryGrid = $("#statusHistoryGrid").data("kendoGrid");
                    statusHistoryGrid.dataSource.read();
                });
                break; 
            
            case "clearanceDetailDiscussionsTab":
                $(document).ready(function () {
                    const grid = $("#clearanceDiscussionGrid").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "clearanceDetailKeywordsTab":
                $(document).ready(function () {
                    const grid = $("#keywordsGridPac").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "":
                break;
        }
        // questionnaire tab
        const questionTabs = $("#questionTabs").val().split("|").filter(item => item === tab);
        if (questionTabs.length) {
            const gridName = `#${questionTabs[0]}Grid_${this.mainDetailContainer}`;
            const grid = $(gridName).data("kendoGrid");
            grid.dataSource.read();
            //prevent click outside the input box
            grid.table.on("click", ".answer-box", function () {
                return false;
            });
        }
    }

    //called also after save (update and insert)
    showDetails(result) {
        const self = this;

        let id = result;
        if (isNaN(result))
            id = result.id;

        pageHelper.showDetails(this, id, function () {
            $.when(pageHelper.handleEmailWorkflow(result)).then(
                function () {
                    if (result.needStatusRemark === true) {
                        self.checkStatusHistoryRemark(id);
                    }   
                }
            );            
        });
    }

    checkStatusHistoryRemark(id) {
        let mainForm = $(`#${this.mainDetailContainer}`).find("form")[0];
        mainForm = $(mainForm);
        const url = mainForm.data("check-status-history-remark");
        const data = { pacId: id };
        $.ajax({
            url: url,
            data: data,
            type: "POST",
            headers: { "RequestVerificationToken": $("[name='__RequestVerificationToken']").val() },
            success: function (result) {                
                if (result.logId > 0) {
                    pacClearancePage.showStatusRemark(data.pacId, result.logId);
                }
            },
            error: function (error) {
                pageHelper.showErrors(error.responseText);
            }

        });
    }

    showStatusRemark(pacId, logId) {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/PatClearance/Clearance/StatusRemark`;
        const data = { pacId: pacId, logId: logId };
        $.ajax({
            url: url,
            type: "GET",
            data: data,
            success: function (result) {                
                const popupContainer = $(".site-content .popup");
                popupContainer.empty();
                popupContainer.html(result);
                var dialog = $("#pacClearanceStatusRemarkDialog");
                dialog.modal("show");               
            },
            error: function (error) {
                pageHelper.showErrors(error);
            }
        });
    }

    showStatusRemarkScreen() {
        //const popupContainer = $(".cpiContainerPopup").last();
        const popupContainer = $(".site-content .popup").last();
        const dialogContainer = popupContainer.find("#pacClearanceStatusRemarkDialog");
        const entryForm = $(dialogContainer.find("form")[0]);
        dialogContainer.modal("show");
        const self = this;

        $(dialogContainer.find("#pacSaveStatusRemarkButton")).click(function () {
            $.validator.unobtrusive.parse(entryForm);
            if (entryForm.data("validator") !== undefined) {
                entryForm.data("validator").settings.ignore = "";
            }

            if (entryForm.valid()) {
                self.SaveStatusRemark();
            }
        });
    }

    SaveStatusRemark() {       
        var statusRemark = $("#statusRemarks").val();
        const dialog = $("#pacClearanceStatusRemarkDialog");
        if (statusRemark) {  
            const parent = $("#pacStatusRemarkBody");
            const param = new Object();
            param.PacId = parseInt($("#PacId").val());
            param.LogId = $("#LogId").val();
            param.Remarks = statusRemark;

            if (statusRemark) {
                $.ajax({
                    url: parent.data("url-save"),
                    data: { remark: param },
                    type: "POST",
                    headers: { "RequestVerificationToken": $("[name='__RequestVerificationToken']").val() },
                    success: function (result) {
                        //alert(result.Message);
                        dialog.modal("hide");
                        pacClearancePage.showDetails(param.PacId);
                    },
                    error: function (error) {
                        pageHelper.showErrors(error.responseText);
                    }
                });
            }
        }
        else {
            const noCountryMsg = $(dialog).data("empty-remark-msg");
            alert(noCountryMsg);
        }
    }
        
    //------------------------------------------------------------ export to excel
    initExportToExcel(url) {
        const self = this;
        $('.k-grid-excel-templater').on('click', (e) => {
            e.preventDefault();

            const data = pacClearancePage.gridMainSearchFilters();
            const dataJSON = JSON.stringify(data.mainSearchFilters);
            self.templaterGenerateDocument(url, "mainSearchFiltersJSON", dataJSON);
        });
    }

    templaterGenerateDocument(url, dataName, dataValue) {
        const html = '<form method="POST" action="' + url + '">' +
            '<input type="hidden" name="__RequestVerificationToken" value="' + $("[name='__RequestVerificationToken']").val() + '">';
        let form = $(html);
        form.append($('<input type="hidden" name="' + dataName + '"/>').val(dataValue));
        $('body').append(form);
        form.submit();
    }
   
    onChange_Client = (e) => {
        pageHelper.onComboBoxChangeDisplayName(e, 'ClientName');
        const client = e.sender;

        const value = client.value();
        if (value) {
            const dataItem = client.dataSource._data.find(e => e.ClientID === +value);
            if (dataItem) {
                const atty1 = this.getKendoComboBox("Attorney1ID");
                const atty2 = this.getKendoComboBox("Attorney2ID");
                const atty3 = this.getKendoComboBox("Attorney3ID");

                if (dataItem.Attorney1ID !== null) {
                    atty1.value(dataItem.Attorney1ID);
                    atty1.text(dataItem.Attorney1Code);
                    $(`#${atty1.element[0].id}`).closest(".float-label").removeClass("inactive").addClass("active");
                }
                //else {
                //    atty1.value(null);
                //    atty1.text("");
                //    $(`#${atty1.element[0].id}`).closest(".float-label").removeClass("active").addClass("inactive");
                //}

                if (dataItem.Attorney2ID !== null) {
                    atty2.value(dataItem.Attorney2ID);
                    atty2.text(dataItem.Attorney2Code);
                    $(`#${atty2.element[0].id}`).closest(".float-label").removeClass("inactive").addClass("active");
                }
                //else {
                //    atty2.value(null);
                //    atty2.text("");
                //    $(`#${atty2.element[0].id}`).closest(".float-label").removeClass("active").addClass("inactive");
                //}

                if (dataItem.Attorney3ID !== null) {
                    atty3.value(dataItem.Attorney3ID);
                    atty3.text(dataItem.Attorney3Code);
                    $(`#${atty3.element[0].id}`).closest(".float-label").removeClass("inactive").addClass("active");
                }
                //else {
                //    atty3.value(null);
                //    atty3.text("");
                //    $(`#${atty3.element[0].id}`).closest(".float-label").removeClass("active").addClass("inactive");
                //}
            }
        }
    }
   
    searchResultDataBound(e) {
        const data = e.sender.dataSource.data();

        if (data.length > 0) {
            const listView = e.sender.element;
            $.each(listView.find(".pacNameSearchResult-collapsible"), function () {
                $(this).textOverflow();
            });            
        }
    }    

    //------------------------------------------------------------- submit
    setSubmitButton(submitTitle, submitMessage) {
        const button = $("#submitClearance");
        if (button) {
            button.on("click", (e) => {
                cpiConfirm.confirm(submitTitle, submitMessage, function () {
                    const url = button.data("url");
                    const data = {};
                    data.id = $("#PacId").val();
                    data.tStamp = $("#tStamp").val();

                    $.ajax({
                        url: url,
                        data: data,
                        type: "POST",
                        headers: { "RequestVerificationToken": $("[name='__RequestVerificationToken']").val() },
                        success: function (result) {
                            pacClearancePage.showDetails(data.id);
                            if (result && result.success)
                                pageHelper.showSuccess(result.success);
                            
                            pageHelper.handleEmailWorkflow(result);
                        },
                        error: function (error) {
                            pageHelper.showErrors(error.responseText);
                        }

                    });

                });
            });
        }
    }

    searchResultDataBound(e) {
        const data = e.sender.dataSource.data();

        if (data.length > 0) {
            const listView = e.sender.element;
            $.each(listView.find(".pacTaglineSearchResult-collapsible"), function () {
                $(this).textOverflow();
            });
        }
    }

    //------------------------------------------------------------ COPY
    showCopyScreen() {
        //const popupContainer = $(".cpiContainerPopup").last();
        const popupContainer = $(".site-content .popup").last();
        const dialogContainer = popupContainer.find("#pacClearanceCopyDialog");
        let entryForm = dialogContainer.find("form")[0];
        dialogContainer.modal("show");
        const self = this;

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

    mainCopyInitialize = (copyPacId) => {
        const baseUrl = $("body").data("base-url");
        const mainUrl = `${baseUrl}/PatClearance/Clearance`;

        $(document).ready(() => {
            const container = $("#pacClearanceCopyDialog");
            container.find(".case-info-settings").hide();

            container.on("click", ".case-info-set", () => {
                container.find(".case-info-settings").show();
                container.find(".data-to-copy").hide();

                const url = `${mainUrl}/GetCopySettings`;
                $.get(url, { copyPacId: copyPacId })
                    .done(function (result) {
                        container.find(".case-info-settings-fields").html(result);
                    })
                    .fail(function (e) {
                        pageHelper.showErrors(e.responseText);
                    });
            });
            container.on("click", ".case-info-set-cancel,.case-info-set-save", () => {
                container.find(".case-info-settings").hide();
                container.find(".data-to-copy").show();
            });
            container.on("change", ".case-info-settings-fields input[type='checkbox']", function () {
                const checkbox = $(this);
                const name = checkbox.attr("name");
                const value = checkbox.prop("checked");
                const url = `${mainUrl}/UpdateCopySetting`;
                $.post(url, { copySettingId: name, copy: value })
                    .fail(function (e) {
                        pageHelper.showErrors(e.responseText);
                    });
            });

            //get all questions by default
            let questions = "|";
            const questionDiv = $(".pacQuestions");
            $(questionDiv.find("input")).each(function () {
                const questionName = $(this).attr("name");
                if (this.checked && questionName !== undefined) {
                    questions += questionName + "|";
                }
            });
            $("#copiedQuestionList").val(questions);
            //get copied questions
            container.on("change", ".pacQuestions input[type='checkbox']", function () {
                let questions = "|";
                const questionDiv = $(".pacQuestions");
                $(questionDiv.find("input")).each(function () {
                    const questionName = $(this).attr("name");
                    if (this.checked && questionName !== undefined) {
                        questions += questionName + "|";
                    }
                });
                $("#copiedQuestionList").val(questions);
            }); 
        });
    }

    //------------------------------------------------------------ COPY TO DISCLOSURE
    setCopyDisclosureButton() {
        const button = $("#copyToDisclosureClearance");
        const self = this;
        if (button) {
            button.on("click", function (e) {
                const url = button.data("url");
                cpiLoadingSpinner.show();
                $.get(url)
                    .done(function (response) {
                        const popupContainer = $(".site-content .popup");
                        popupContainer.empty();
                        popupContainer.html(response);
                        var dialog = $("#pacClearanceCopyDisclosureDialog");
                        dialog.modal("show");
                        cpiLoadingSpinner.hide();
                    })
                    .fail(function (error) {
                        cpiLoadingSpinner.hide();
                        pageHelper.showErrors(error);
                    });
                return false;
            });
        }
    }

    showCopyDisclosureScreen() {
        const popupContainer = $(".site-content .popup").last();
        const dialogContainer = popupContainer.find("#pacClearanceCopyDisclosureDialog");
        let entryForm = dialogContainer.find("form")[0];
        dialogContainer.modal("show");
        const self = this;

        entryForm = $(entryForm);
        const afterSubmit = function (result) {
            if (result) {
                pageHelper.showSuccess(result.success.Name);
            }
        };
        entryForm.cpiPopupEntryForm({ dialogContainer: dialogContainer, afterSubmit: afterSubmit });
    }

    mainCopyDisclosureInitialize = (copyPacId) => {
        const baseUrl = $("body").data("base-url");
        const mainUrl = `${baseUrl}/PatClearance/Clearance`;

        $(document).ready(() => {
            const container = $("#pacClearanceCopyDisclosureDialog");
            container.find(".case-info-settings").hide();

            container.on("click", ".case-info-set", () => {
                container.find(".case-info-settings").show();
                container.find(".data-to-copy").hide();

                const url = `${mainUrl}/GetCopyDisclosureSettings`;
                $.get(url, { copyPacId: copyPacId })
                    .done(function (result) {
                        container.find(".case-info-settings-fields").html(result);
                    })
                    .fail(function (e) {
                        pageHelper.showErrors(e.responseText);
                    });
            });
            container.on("click", ".case-info-set-cancel,.case-info-set-save", () => {
                container.find(".case-info-settings").hide();
                container.find(".data-to-copy").show();
            });
            container.on("change", ".case-info-settings-fields input[type='checkbox']", function () {
                const checkbox = $(this);
                const name = checkbox.attr("name");
                const value = checkbox.prop("checked");
                const url = `${mainUrl}/UpdateCopyDisclosureSetting`;
                $.post(url, { copySettingId: name, copy: value })
                    .fail(function (e) {
                        pageHelper.showErrors(e.responseText);
                    });
            });

            ////get all questions by default
            //let questions = "|";
            //const questionDiv = $(".pacQuestions");
            //$(questionDiv.find("input")).each(function () {
            //    const questionName = $(this).attr("name");
            //    if (this.checked && questionName !== undefined) {
            //        questions += questionName + "|";
            //    }
            //});
            //$("#copiedQuestionList").val(questions);
            ////get copied questions
            //container.on("change", ".pacQuestions input[type='checkbox']", function () {
            //    let questions = "|";
            //    const questionDiv = $(".pacQuestions");
            //    $(questionDiv.find("input")).each(function () {
            //        const questionName = $(this).attr("name");
            //        if (this.checked && questionName !== undefined) {
            //            questions += questionName + "|";
            //        }
            //    });
            //    $("#copiedQuestionList").val(questions);
            //});
        });
    }

    onChange_RelatedTrademark = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");

            const trademarkStatus = e.dataItem["TrademarkStatus"];
            const trademarkStatusDate = e.dataItem["TrademarkStatusDate"];
            const clientCode = e.dataItem["ClientCode"];
            const title = e.dataItem["TrademarkName"];            

            const grid = $("#clearanceRelatedTrademarksGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            
            dataItem.TmkId = e.dataItem["TmkId"];
            dataItem.TrademarkStatus = trademarkStatus;
            dataItem.TrademarkStatusDate = trademarkStatusDate;
            dataItem.ClientCode = clientCode;
            dataItem.TrademarkName = title;

            $(row).find(".status-field").html(trademarkStatus);
            $(row).find(".date-field").html(trademarkStatusDate);
            $(row).find(".client-field").html(clientCode);
            $(row).find(".title-field").html(title);
        }
    }  

    //------------------------------------------------------------- DISCUSSION
    refreshDiscussionGrid(e) {
        //const gridId = e.sender.element[0].id;
        $("#clearanceDiscussionGrid").data("kendoGrid").dataSource.read();
    }

    discussionDataBound(e) {
        const gridId = e.sender.element[0].id;
        const grid = $("#" + gridId).data("kendoGrid");
        const data = grid.dataSource.view();

        for (var i = 0; i < data.length; i++) {
            const trow = grid.tbody.find("tr[data-uid='" + data[i].uid + "']");
            if (!data[i].CanDelete) {
                trow.find(".k-grid-delete").hide();
            }
            else if (data[i].CanDelete && data[i].CanEditRecord) {
                trow.find(".k-grid-delete").show();
            }
            else {
                trow.find(".k-grid-delete").hide();
            }

            if (!data[i].CanEdit) {
                trow.find(".k-grid-edit").hide();
            }
            else if (data[i].CanEdit && data[i].CanEditRecord) {
                trow.find(".k-grid-edit").show();
            }
            else {
                trow.find(".k-grid-edit").hide();
            }
        }
    }

    onDiscussionEdit(e) {
        var arg = e;
        arg.container.data('kendoWindow').bind('activate', function () {
            arg.container.find("textarea[name='DiscussionMsg']").focus();
        })
    }

    onDiscussionRequestEnd(e) {
        //type: create, update, destroy
        //if type is create - check workflow for new discussion
        if (e.type === "create") {
            if (e.response && e.response.emailWorkflows) {
                pageHelper.handleEmailWorkflow(e.response);
            }
        }
    }

    replyRequestEnd(e) {
        if (e.type == "destroy") {
            $("#clearanceDiscussionGrid").data("kendoGrid").dataSource.read();
        }
        if (e.type === "create") {
            if (e.response && e.response.emailWorkflows) {
                pageHelper.handleEmailWorkflow(e.response);
            }
        }
    }

    // ------------------------------ inventor logic

    onEditInventor(e) {
        const gridId = e.sender.element[0].id;
        if (e.model.isNew()) {
            //Remove move class on newly added row as we do not want to move record which is not yet saved.
            $("#" + gridId + " tbody").find("tr[data-uid=" + e.model.uid + "]").css({ 'cursor': "default", 'HintHandler': '' });
            $("#" + gridId + " tbody").find("tr[data-uid=" + e.model.uid + "]").removeAttr("onChange");
        }
    }

    inventorGridDataBound = function (e) {
        const gridId = e.sender.element[0].id;
        const grid = $("#" + gridId).data("kendoGrid");
        const data = grid.dataSource.view();

        for (var i = 0; i < data.length; i++) {
            const trow = grid.tbody.find("tr[data-uid='" + data[i].uid + "']");
            //window.fely = trow;
            if (!data[i].CanModifyName && !data[i].CanModifyInitials) {
                trow.addClass("read-only");
            }
            else if (!data[i].CanModifyName) {
                trow.children("td.editable-cell.inventor-cell").removeClass("editable-cell");
            }
            else if (!data[i].CanModifyInitials) {
                trow.children("td.editable-cell.initials-cell").removeClass("editable-cell");
            }
            //if (data[i].IsDefaultInventor) {
            //    trow.find(".k-grid-Delete").hide();
            //}
        }
    }

    isInventorInitialEditable(dataItem) {
        return dataItem.CanModifyInitials;
    }

    isInventorNameEditable(dataItem) {
        return dataItem.CanModifyName;
    }

    //called on inventor grid's afterSubmit to refresh inventor-based detail page permissions
    refreshDetail = () => {
        pacClearancePage.showDetails(this.currentRecordId);
    }

    //inventor grid delete handler that calls refreshDetail after delete
    //delete button is disabled when grid is dirty
    //if delete is allowed if there are pending updates, it will be lost when refreshDetail is called
    deleteInventorRow(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        this.addMoreKeyFields(dataItem);

        const self = this;
        //pageHelper.deleteGridRow(e, dataItem, function () { self.updateRecordStamps(); });
        pageHelper.deleteGridRow(e, dataItem, function () { self.refreshDetail(); });
    }

    showDisclosureLink = (screen, title, dmsUrl) => {
            const container = $(`#${screen}`).find(".cpiButtonsDetail");
            const pageNav = container.find(".nav");
            pageNav.prepend(`<a class="nav-link disclosure-link" href="#" target="_self" title="${title}" role="button"><i class="fal fa-external-link pr-2"></i>${title}</a>`);
            container.find(".disclosure-link").on("click", function () {
                pageHelper.openLink(dmsUrl, false);
            });
    }
}