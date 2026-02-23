import Image from "../image";
import ActivePage from "../activePage";

export default class TmcClearancePage extends ActivePage {

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
            //Load first question tab
            const questionTabs = $("#questionTabs").val().split("|");
            if (questionTabs.length) {
                const gridName = `#${questionTabs[0]}Grid_${this.mainDetailContainer}`;
                const grid = $(gridName).data("kendoGrid");
                grid.dataSource.read();
                //prevent click outside the input box
                grid.table.on("click", ".answer-box", function () {
                    return false;
                });
            }  
        })

        $(".showCountrySelection").click(function (e) {
            e.preventDefault();
            const button = $(this);
            const url = button.data("url");
            $.get(url).done(function (result) {
                const popupContainer = $(".site-content .popup");
                popupContainer.empty();
                popupContainer.html(result);
                var dialog = $("#tmcClearanceCountryDialog");
                dialog.modal("show");
            }).fail(function (error) {
                page.showErrors(error.responseText);
            });
        });        
    }

    caseNumberSearchValueMapper = (options) => {
        const url = tmcClearancePage.caseNumberDetailValueMapperUrl;
           $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }    

    caseNumberDetailValueMapper = (options) => {
        const url = tmcClearancePage.caseNumberDetailValueMapperUrl;

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

            case "clearanceDetailRequestedTermsTab":
                $(document).ready(function () {
                    const marksGrid = $("#clearanceMarksGrid").data("kendoGrid");
                    marksGrid.dataSource.read();
                });
                break;

            case "clearanceDetailKeywordsTab":
                $(document).ready(function () {
                    const keywordsGrid = $("#keywordsGrid").data("kendoGrid");
                    keywordsGrid.dataSource.read();
                });
                break;

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

            case "clearanceDetailRelatedTrademarksTab":
                $(document).ready(function () {
                    const relatedTrademarkGrid = $("#relatedTrademarksGrid");
                    const grid = relatedTrademarkGrid.data("kendoGrid");
                    grid.dataSource.read();                    
                });
                break;

            case "clearanceDetailListItemsTab":
                $(document).ready(function () {
                    const grid = $("#clearanceListItemsGrid").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "clearanceDetailDiscussionsTab":
                $(document).ready(function () {
                    const grid = $("#clearanceDiscussionGrid").data("kendoGrid");
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

    afterInsert = (result) => {
        let id = result;
        if (isNaN(result))
            id = result.id;

        pageHelper.showDetails(this, id, () => {
            $(`#${this.mainDetailContainer}`).find(".firstTmcQuestionTab").click();            
        });
    }

    checkStatusHistoryRemark(id) {
        let mainForm = $(`#${this.mainDetailContainer}`).find("form")[0];
        mainForm = $(mainForm);
        const url = mainForm.data("check-status-history-remark");
        const data = { tmcId: id };
        $.ajax({
            url: url,
            data: data,
            type: "POST",
            headers: { "RequestVerificationToken": $("[name='__RequestVerificationToken']").val() },
            success: function (result) {                
                if (result.logId > 0) {
                    tmcClearancePage.showStatusRemark(data.tmcId, result.logId);
                }
            },
            error: function (error) {
                pageHelper.showErrors(error.responseText);
            }

        });
    }

    showStatusRemark(tmcId, logId) {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/SearchRequest/TmcClearance/StatusRemark`;

        const data = { tmcId: tmcId, logId: logId };
        $.ajax({
            url: url,
            type: "GET",
            data: data,
            success: function (result) {                
                const popupContainer = $(".site-content .popup");
                popupContainer.empty();
                popupContainer.html(result);
                var dialog = $("#tmcClearanceStatusRemarkDialog");
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
        const dialogContainer = popupContainer.find("#tmcClearanceStatusRemarkDialog");
        const entryForm = $(dialogContainer.find("form")[0]);
        dialogContainer.modal("show");
        const self = this;

        //$(dialogContainer.find("#tmcCopyAllCountries")).click(function () {
        //    self.markCopyCountries(this.checked, "tmcAllCountries");
        //});

        $(dialogContainer.find("#tmcSaveStatusRemarkButton")).click(function () {
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
        const dialog = $("#tmcClearanceStatusRemarkDialog");
        if (statusRemark) {  
            const parent = $("#tmcStatusRemarkBody");
            const param = new Object();
            param.TmcId = parseInt($("#TmcId").val());
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
                        tmcClearancePage.showDetails(param.TmcId);
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

    getCountry = () => {
        const country = $("#" + this.detailContentContainer).find("input[name='Country']");
        return { country: country.val() };
    }

    onChange_Country = (e) => {
        const caseType = $("#" + this.detailContentContainer).find("input[name='CaseType']");
        caseType.data("fetched", 0);

        pageHelper.onComboBoxChangeDisplayName(e, 'CountryName');
    }
    
    //------------------------------------------------------------ export to excel
    initExportToExcel(url) {
        const self = this;
        $('.k-grid-excel-templater').on('click', (e) => {
            e.preventDefault();

            const data = tmcClearancePage.gridMainSearchFilters();
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
            $.each(listView.find(".tmcNameSearchResult-collapsible"), function () {
                $(this).textOverflow();
            });            
        }
    }

    //------------------------------------------------------------ country popup selection - similar to copy function
    showCountrySelectionScreen() {
        //const popupContainer = $(".cpiContainerPopup").last();
        const popupContainer = $(".site-content .popup").last();
        const dialogContainer = popupContainer.find("#tmcClearanceCountryDialog");
        const entryForm = $(dialogContainer.find("form")[0]);
        dialogContainer.modal("show");
        const self = this;

        //$(dialogContainer.find("#tmcCopyAllCountries")).click(function () {
        //    self.markCopyCountries(this.checked, "tmcAllCountries");
        //});

        $(dialogContainer.find("#tmcSaveButton")).click(function () {
            $.validator.unobtrusive.parse(entryForm);
            if (entryForm.data("validator") !== undefined) {
                entryForm.data("validator").settings.ignore = "";
            }

            if (entryForm.valid()) {
                self.SaveCountries();
            }
        });
    }

    markSelectCountries(check, container) {
        $("#" + container + " input").each(function () {
            this.checked = check;
        });
    }

    SaveCountries() {
        // get checked countries
        let countries = "|";
        const countryTab = $("#tmcAllCountries");

        $(countryTab.find("input")).each(function () {
            const ctryCode = $(this).attr("name");
            if (this.checked && ctryCode !== undefined) {
                countries += ctryCode + "|";
            }
        });

        const dialog = $("#tmcClearanceCountryDialog");
        if (countries.length > 1) {            
            let countriesList = countries.substring(1);
            countriesList = countriesList.substring(0, countriesList.length - 1).replaceAll("|", ",");           

            const parent = $("#tmcCountryBody");

            const param = new Object();
            param.TmcId = parseInt($("#TmcId").val());
            param.AreaFieldName = $("#AreaFieldName").val();
            param.SelectedCountries = countries;            

            if (countries) {
                $.ajax({
                    url: parent.data("url-copy"),
                    data: { copy: param },
                    type: "POST",
                    headers: { "RequestVerificationToken": $("[name='__RequestVerificationToken']").val() },                    
                    success: function (result) {
                        //alert(result.Message);
                        dialog.modal("hide");
                        tmcClearancePage.showDetails(param.TmcId);
                    },
                    error: function (error) {
                        pageHelper.showErrors(error.responseText);
                    }
                });               
            }
            //else {
            //    const noDataMsg = $(dialog).data("no-country-msg");
            //    alert(noDataMsg);
            //}
        }
        else {
            const noCountryMsg = $(dialog).data("no-country-msg");
            alert(noCountryMsg);
        }
    }

    //------------------------------------------------------------- submit
    setSubmitButton(submitTitle, submitMessage) {
        const button = $("#submitClearance");
        const self = this;
        if (button) {
            button.on("click", (e) => {
                cpiConfirm.confirm(submitTitle, submitMessage, function () {
                    const url = button.data("url");
                    const data = {};
                    data.id = $("#TmcId").val();
                    data.tStamp = $("#tStamp").val();

                    $.ajax({
                        url: url,
                        data: data,
                        type: "POST",
                        headers: { "RequestVerificationToken": $("[name='__RequestVerificationToken']").val() },
                        success: function (result) {
                            tmcClearancePage.showDetails(data.id);
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
            $.each(listView.find(".tmcTaglineSearchResult-collapsible"), function () {
                $(this).textOverflow();
            });
        }
    }

    //------------------------------------------------------------ COPY
    showCopyScreen() {
        //const popupContainer = $(".cpiContainerPopup").last();
        const popupContainer = $(".site-content .popup").last();
        const dialogContainer = popupContainer.find("#tmcClearanceCopyDialog");
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

    mainCopyInitialize = (copyTmcId) => {
        const baseUrl = $("body").data("base-url");
        const mainUrl = `${baseUrl}/SearchRequest/TmcClearance/`;

        $(document).ready(() => {
            const container = $("#tmcClearanceCopyDialog");
            container.find(".case-info-settings").hide();

            container.on("click", ".case-info-set", () => {
                container.find(".case-info-settings").show();
                container.find(".data-to-copy").hide();

                const url = `${mainUrl}/GetCopySettings`;
                $.get(url, { copyTmcId: copyTmcId })
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
            const questionDiv = $(".tmcQuestions");
            $(questionDiv.find("input")).each(function () {
                const questionName = $(this).attr("name");
                if (this.checked && questionName !== undefined) {
                    questions += questionName + "|";
                }
            });
            $("#copiedQuestionList").val(questions);
            //get copied questions
            container.on("change", ".tmcQuestions input[type='checkbox']", function () {
                let questions = "|";
                const questionDiv = $(".tmcQuestions");
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
}