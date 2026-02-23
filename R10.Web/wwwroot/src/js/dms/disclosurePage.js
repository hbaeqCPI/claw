import Image from "../image";
import TradeSecret from "../tradeSecret";
import ActivePage from "../activePage";

export default class DisclosurePage extends ActivePage {
    constructor() {
        super();
        this.image = new Image();
        this.tradeSecret = new TradeSecret();
        this.showOutstandingActionsOnly = true;
        this.docServerOperation = true;

        //this.questionGridsData = [];
    }

    initializeDetailContentPage(detailContentPage) {
        super.initializeDetailContentPage(detailContentPage);
        this.tradeSecret.initialize(detailContentPage);
    }

    init(screen, isSharePointIntegrationOn) {
        this.docServerOperation = !isSharePointIntegrationOn;

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
            inventorsGrid.on("click", ".inventorSignature", (e) => {
                e.stopPropagation();

                if (this.docServerOperation === false) {
                    var baseUrl = $("body").data("base-url");
                    const authenticatedCheckUrl = `${baseUrl}/Shared/SharePointGraph/IsAuthenticated`;
                    $.get(authenticatedCheckUrl)
                        .fail(function (e) {
                            if (e.status == 401) {
                                const baseUrl = $("body").data("base-url");
                                const url = `${baseUrl}/Graph/SharePoint`;
                                sharePointGraphHelper.getGraphToken(url, () => { });
                            }
                            else {
                                pageHelper.showErrors(e.responseText);
                            }
                        });
                }

                //var notAllReviewed = inventorsGrid.data("kendoGrid").dataSource.data().some(function (dataItem) {
                //    return dataItem.IsReviewed == false;        
                //});

                //if (notAllReviewed) {
                //    cpiAlert.warning($(e.target).data("error"));
                //    return;
                //}

                cpiConfirm.confirm($(e.target).data("label"), $(e.target).data("msg"), () => {
                    cpiLoadingSpinner.show();
                    let url = $(e.target).data("url");
                    $.ajax({
                        url: url,
                        type: "POST",
                        success: (response) => {
                            cpiLoadingSpinner.hide();
                            disclosurePage.showDetails(this.currentRecordId);
                            pageHelper.showSuccess(response.success);
                        },
                        error: function (e) {
                            cpiLoadingSpinner.hide();
                            pageHelper.showErrors(e);
                        }
                    });
                });
            });

            const atty = this.getKendoComboBox("AttorneyID");
            $("input[name='ClientID']").change(function () {
                const baseUrl = $("body").data("base-url");
                $.ajax({
                    url: `${baseUrl}/Shared/Client/GetPatDefaultAttorney`,
                    data: { clientId: this.value },
                    dataType: 'json',
                    success: function (data) {
                        if (data.length > 0) {
                            if (atty.value() == '' && data[0].PatAttorney1ID != null) {
                                atty.value(data[0].PatAttorney1ID);
                                atty.element.trigger("change");
                            }
                        }
                    }
                });
            })

            const actionsGrid = $(`#actionsGrid_${screen}`);
            actionsGrid.on("click", ".action-link", (e) => {
                e.stopPropagation();
                e.preventDefault();

                if (this.actionActIds && this.actionActIds.length > 0) {
                    dmsActionDuePage.mainSearchRecordIds = this.actionActIds;
                }
                const link = $(e.target);
                pageHelper.openDetailsLink(link);
            });
        });
    }

    tabChangeSetListener() {
        $('#disclosureTab a').on('click', (e) => {
            e.preventDefault();
            const tab = e.target.id;
            if (this.tabsLoaded.indexOf(tab) === -1) {
                this.tabsLoaded.push(tab);
                this.loadTabContent(tab);
            }
        });        
    }

    editorModified = (e) => {
        let editorText = e.sender.value();
        editorText = editorText.replace(/href="(javascript.*?)"/, 'href=#');

        e.sender.value(editorText);
        $(this.entryFormInstance).trigger('markDirty');
    }

    //called also after save (update and insert)
    showDetails(result) {
        const self = this;

        let id = result;
        if (isNaN(result))
            id = result.id;

        pageHelper.showDetails(this, id, function () {
            pageHelper.handleEmailWorkflow(result);            
        });
    }

    afterInsert(result, options) {
        if (this.recordNavigator && this.recordNavigator.length > 0) {
            let recId = result;
            if (isNaN(result))
                recId = result.id;

            this.recordNavigator.addRecordId(recId);
        }
        return this.showDetails(result);
    }

    deleteDueDate = (e, grid, deleteActionDuePrompt) => {
        this.deleteGridRow(e, grid);
    }

    loadTabContent(tab) {
        const self = this;

        switch (tab) {
            case "actionTab":
                $(document).ready(() => {
                    const grid = $(`#actionsGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    $(".grid-options #showOutstandingActionsOnly").prop('checked', this.showOutstandingActionsOnly);
                    if (parseInt(grid.options.dataSource.pageSize) > 0)
                        grid.dataSource.pageSize(grid.options.dataSource.pageSize);
                    else
                        grid.dataSource.read();
                });
                break;

            case "keywordsTab":
                $(document).ready(function () {
                    const grid = $("#keywordsGridDMS").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "disclosureDetailDocumentsTab":
                $(document).ready(() => {
                    this.image.initializeImage(this, this.docServerOperation);
                });
                break;

            case "disclosureDetailRelatedDisclosuresTab":
                $(document).ready(function () {
                    const grid = $("#disclosureRelatedDisclosuresGrid").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "previewsTab":
                $(document).ready(function () {
                    const grid = $("#previewsGrid").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "reviewsTab":
                $(document).ready(function () {
                    const grid = $("#reviewsGrid").data("kendoGrid");
                    grid.dataSource.read();
                    //use reviewsGridDataBound to also initialize rating when sorting
                    //.done(function () {
                    //    $("#reviewsGrid").find(".star-rating").rating(disclosureReviewPage.getRatingOptions());
                    //});
                });
                break;

            case "statusHistoryTab":
                $(document).ready(function () {
                    const grid = $("#statusHistoryGrid").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "combinedsTab":
                $(document).ready(function () {
                    const grid = $("#disclosureCombinedDisclosuresGrid").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "relatedInventionTab":
                $(document).ready(function () {
                    const grid = $("#relatedInventionGrid").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "relatedApplicationTab":
                $(document).ready(function () {
                    const grid = $("#relatedApplicationGrid").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "disclosureDetailDiscussionsTab":
                $(document).ready(function () {
                    const grid = $("#disclosureDiscussionGrid").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "disclosureDetailCorrespondenceTab":
                $(document).ready(() => {
                    const grid = $(`#docsOutGrid_${this.mainDetailContainer}`).data("kendoGrid");
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

    reviewsGridDataBound(e) {
        //READONLY REVIEWS GRID
        let ratingOptions = disclosureReviewPage.getRatingOptions();
        ratingOptions.displayOnly = true;
        $("#reviewsGrid").find(".star-rating").rating(ratingOptions);
    }

    setSubmitButton(submitTitle, submitMessage) {
        const button = $("#submitDisclosure");
        if (button) {
            button.on("click", (e) => {
                cpiConfirm.confirm(submitTitle, submitMessage, function () {
                    const url = button.data("url");
                    const data = {};
                    data.id = $("#DMSId").val();
                    data.tStamp = $("#tStamp").val();

                    $.ajax({
                        url: url,
                        data: data,
                        type: "POST",
                        headers: { "RequestVerificationToken": $("[name='__RequestVerificationToken']").val() },
                        success: function (result) {
                            disclosurePage.showDetails(data.id);
                            pageHelper.showSuccess(result.success);
                            
                            if (result && result.emailWorkflows) {
                                pageHelper.handleEmailWorkflow(result);
                            }
                        },
                        error: function (error) {
                            pageHelper.showErrors(error.responseText);
                        }

                    });

                });
            });
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

            if (!data[i].CanModifyName) {
                trow.children("td.editable-cell.inventor-cell").removeClass("editable-cell");
            }

            if (!data[i].CanModifyInitials) {
                trow.children("td.editable-cell.initials-cell").removeClass("editable-cell");
            }

            if (data[i].IsDefaultInventor) {
                trow.find(".k-grid-Delete").hide();
            }

            if (data[i].IsNonEmployee) {
                trow.children("td.initials-cell").html("");
            }

            if (!data[i].CanModifyReview) {
                trow.children("td.editable-cell.isReviewed-checkbox").removeClass("editable-cell");
            }
        }
    }

    isInventorInitialEditable(dataItem) {
        return dataItem.CanModifyInitials;
    }

    isInventorNameEditable(dataItem) {
        return dataItem.CanModifyName;
    }

    isInventorReviewEditable(dataItem) {
        return dataItem.CanModifyReview;
    }

    //called on inventor grid's afterSubmit to refresh inventor-based detail page permissions
    refreshDetail = (result) => {
        disclosurePage.showDetails(this.currentRecordId);
        
        if (result && result.emailWorkflows) {
            pageHelper.handleEmailWorkflow(result);
        }
    }

    //inventor grid delete handler that calls refreshDetail after delete
    //delete button is disabled when grid is dirty
    //if delete is allowed if there are pending updates, it will be lost when refreshDetail is called
    deleteInventorRow(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        this.addMoreKeyFields(dataItem);

        const self = this;
        //pageHelper.deleteGridRow(e, dataItem, function () { self.updateRecordStamps(); });
        pageHelper.deleteGridRow(e, dataItem, function (afterDeleteResult) { self.refreshDetail(afterDeleteResult); });
    }

    // ------------------------------ question logic
    questionGridDataBound = function (e) {
        const gridId = e.sender.element[0].id;
        const grid = $("#" + gridId).data("kendoGrid");

        var dataSource = grid.dataSource;
        if (dataSource && !dataSource.filter()) {
            dataSource.filter({
                field: "IsVisible",
                operator: "eq",
                value: true
            });
        }

        grid.tbody.find('tr').each(function () {
            var item = grid.dataItem(this);
            kendo.bind(this, item);
        });
    }

    refreshDiscussionGrid(e) {
        //const gridId = e.sender.element[0].id;
        $("#disclosureDiscussionGrid").data("kendoGrid").dataSource.read();
    }

    onDiscussionDataBound(e) {
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

        var recommendationDropDown = $("#Discussion_Recommendation").data("kendoDropDownList");
        recommendationDropDown.value(e.model.Recommendation);
        recommendationDropDown.trigger("change");

        var $previewPrivateContainer = $('#IsPreviewPrivateContainer');
        var $privateContainer = $('#IsPrivateContainer');
        var $isPreviewCheckbox = $('#IsPreview');
        var $isPreviewPrivateCheckbox = $('#IsPreviewPrivate');
        var $isPrivateCheckbox = $('#IsPrivate');

        if (!e.model.CanSeePrivateCB) {
            $privateContainer.hide();
        }

        $isPreviewCheckbox.prop('checked', e.model.IsPreview || false);
        $isPreviewPrivateCheckbox.prop('checked', e.model.IsPreviewPrivate || false);
        $isPrivateCheckbox.prop('checked', e.model.IsPrivate || false);
        
        function togglePreviewControls() {
            if ($isPreviewCheckbox.is(':checked')) {
                $previewPrivateContainer.show();

                $isPrivateCheckbox.prop('disabled', true);
                $isPrivateCheckbox.prop('checked', false);
            } else {
                $previewPrivateContainer.hide();
                $isPreviewPrivateCheckbox.prop('checked', false);

                $isPrivateCheckbox.prop('disabled', false);
            }
        }
        togglePreviewControls();
        
        $isPreviewCheckbox.off('change').on('change', function () {
            togglePreviewControls();
        });
    }

    onDiscussionSave(e) {
        var recommendationDropDown = $("#Discussion_Recommendation").data("kendoDropDownList");
        e.model.set("Recommendation", recommendationDropDown.value());

        var isPreviewChecked = $('#IsPreview').is(':checked');
        e.model.set("IsPreview", isPreviewChecked);
    
        var isPreviewPrivateChecked = $('#IsPreviewPrivate').is(':checked');
        e.model.set("IsPreviewPrivate", isPreviewPrivateChecked);
            
        var isPrivateChecked = $('#IsPrivate').is(':checked');
        e.model.set("IsPrivate", isPrivateChecked);
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

    onReplyDataBound(e) {
        var grid = e.sender;
        grid.tbody.find(".k-grid-edit").hide();

        if ($('#' + grid.element[0].id + ' .k-grid-toolbar')[0]) {
            $('#' + grid.element[0].id + ' .k-grid-toolbar')[0].style.backgroundColor = "#8a9197";
            $('#' + grid.element[0].id + ' .k-grid-add')[0].style.color = "#FFFFFF";
            $('#' + grid.element[0].id + ' .k-grid-pager')[0].style.cssText = "background-color: #dedede !important";
        }

        const gridId = e.sender.element[0].id;
        const childGrid = $("#" + gridId).data("kendoGrid");
        const data = childGrid.dataSource.view();

        for (var i = 0; i < data.length; i++) {
            var trow = childGrid.tbody.find("tr[data-uid='" + data[i].uid + "']");

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

    onReplyEdit(e) {
        var arg = e;
        arg.container.data('kendoWindow').bind('activate', function () {
            arg.container.find("textarea[name='ReplyMsg']").focus();
        })
    }

    onReplyRequestEnd(e) {
        if (e.type == "destroy") {
            $("#disclosureDiscussionGrid").data("kendoGrid").dataSource.read();
        }
        if (e.type === "create") {
            if (e.response && e.response.emailWorkflows) {
                pageHelper.handleEmailWorkflow(e.response);
            }
        }
    }    

    //------------------------------------------------------------ COPY
    showCopyScreen() {
        //const popupContainer = $(".cpiContainerPopup").last();
        const popupContainer = $(".site-content .popup").last();
        const dialogContainer = popupContainer.find("#disclosureCopyDialog");
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

    mainCopyInitialize = (copyDMSId) => {
        const baseUrl = $("body").data("base-url");
        const mainUrl = `${baseUrl}/DMS/Disclosure/`;

        $(document).ready(() => {

            const container = $("#disclosureCopyDialog");
            container.find(".case-info-settings").hide();

            container.on("click", ".case-info-set", () => {
                container.find(".case-info-settings").show();
                container.find(".data-to-copy").hide();

                const url = `${mainUrl}GetCopySettings`;
                $.get(url, { copyDMSId: copyDMSId })
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
                const url = `${mainUrl}UpdateCopySetting`;
                $.post(url, { copySettingId: name, copy: value })
                    .fail(function (e) {
                        pageHelper.showErrors(e.responseText);
                    });
            });
            container.on("change", ".data-to-copy input[type='checkbox']", function () {
                const checkbox = $(this);
                const name = checkbox.attr("name");
                const value = checkbox.prop("checked");
                const url = `${mainUrl}UpdateCopyMainSetting`;
                $.post(url, { name: name, copy: value })
                    .fail(function (e) {
                        pageHelper.showErrors(e.responseText);
                    });
            });

            //get all questions by default
            let questions = "|";
            const questionDiv = $(".dmsQuestions");
            $(questionDiv.find("input")).each(function () {
                const questionName = $(this).attr("name");
                if (this.checked && questionName !== undefined) {
                    questions += questionName + "|";
                }
            });
            $("#copiedQuestionList").val(questions);
            //get copied questions
            container.on("change", ".dmsQuestions input[type='checkbox']", function () {
                let questions = "|";
                const questionDiv = $(".dmsQuestions");
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

    /* related disclosure */
    onChange_RelatedDisclosure = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");

            const disclosureTitle = e.dataItem["DisclosureTitle"];
            const disclosureStatus = e.dataItem["DisclosureStatus"];
            const disclosureDate = e.dataItem["DisclosureDate"];

            const grid = $("#disclosureRelatedDisclosuresGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            dataItem.RelatedDMSId = e.dataItem["DMSId"];
            dataItem.DisclosureTitle = disclosureTitle;
            dataItem.DisclosureStatus = disclosureStatus;
            dataItem.DisclosureDate = disclosureDate;

            $(row).find(".title-field").html(disclosureTitle);
            $(row).find(".status-field").html(disclosureStatus);
            $(row).find(".date-field").html(disclosureDate);
        }
    }

    onDueDateAttorneyChange = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");
            const grid = $(`#actionsGrid_${this.mainDetailContainer}`).data("kendoGrid");
            const dataItem = grid.dataItem(row);

            dataItem.AttorneyID = e.dataItem["AttorneyID"];
        }
    }

    clearDueDateAttorney = (e) => {
        const row = $(`#${e.sender.element[0].id}`).closest("tr");
        const grid = $(`#actionsGrid_${this.mainDetailContainer}`).data("kendoGrid");
        const dataItem = grid.dataItem(row);

        if (dataItem.DueDateAttorneyName == null || dataItem.DueDateAttorneyName == "") {
            dataItem.AttorneyID = null;
            dataItem.DueDateAttorneyName = "";
        }
    }

    /* Copy to Patent Clearance*/
    setCopyClearanceButton() {
        const button = $("#copyToClearanceDisclosure");
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
                        var dialog = $("#disclosureCopyClearanceDialog");
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

    showCopyClearanceScreen() {
        const popupContainer = $(".site-content .popup").last();
        const dialogContainer = popupContainer.find("#disclosureCopyClearanceDialog");
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

    mainCopyClearanceInitialize = (copyDMSId) => {
        const baseUrl = $("body").data("base-url");
        const mainUrl = `${baseUrl}/DMS/Disclosure`;

        $(document).ready(() => {

            const container = $("#disclosureCopyClearanceDialog");
            container.find(".case-info-settings").hide();

            container.on("click", ".case-info-set", () => {
                container.find(".case-info-settings").show();
                container.find(".data-to-copy").hide();

                const url = `${mainUrl}/GetCopyClearanceSettings`;
                $.get(url, { copyDMSId: copyDMSId })
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
                const url = `${mainUrl}/UpdateCopyClearanceSetting`;
                $.post(url, { copySettingId: name, copy: value })
                    .fail(function (e) {
                        pageHelper.showErrors(e.responseText);
                    });
            });

            ////get all questions by default
            //let questions = "|";
            //const questionDiv = $(".dmsQuestions");
            //$(questionDiv.find("input")).each(function () {
            //    const questionName = $(this).attr("name");
            //    if (this.checked && questionName !== undefined) {
            //        questions += questionName + "|";
            //    }
            //});
            //$("#copiedQuestionList").val(questions);
            ////get copied questions
            //container.on("change", ".dmsQuestions input[type='checkbox']", function () {
            //    let questions = "|";
            //    const questionDiv = $(".dmsQuestions");
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

    showDueDateEmailScreen(e, grid) {
        const form = $("#" + this.detailContentContainer).find("form");
        const url = form.data("duedate-email-url");
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const ddId = dataItem.DDId;

        $.ajax({
            url: url,
            data: { id: ddId },
            success: function (result) {
                var popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
            },
            error: function (e) {
                pageHelper.showErrors(e);
            }
        });
    }

    showAverageRating = (screen, title, id, score) => {
        const container = $(`#${screen}`).find(".cpiButtonsDetail");
        const pageNav = container.find(".nav");
        pageNav.prepend(`<a class="nav-link" href="#" style="pointer-events: none; cursor: default;" title="${title}" role="button"><span class='tickler-patent-score badge'>${score}</span></a>`);
    }

    showClearanceLink = (screen, title, pacUrl) => {
        const container = $(`#${screen}`).find(".cpiButtonsDetail");
        const pageNav = container.find(".nav");
        pageNav.prepend(`<a class="nav-link clearance-link" href="#" target="_self" title="${title}" role="button"><i class="fal fa-external-link pr-2"></i>${title}</a>`);
        container.find(".clearance-link").on("click", function () {
            pageHelper.openLink(pacUrl, false);
        });
    }

    setRevertPendingSignatureBtn() {
        const button = $("#revertPendingSignatureStatus");
        const self = this;
        if (button) {
            button.on("click", function (e) {
                const url = button.data("url");
                const needVoidReason = button.data("need-void");

                if (needVoidReason) {
                    const popUpContent = `     
                    <span>${button.data("confirm-msg")}</span>
                    <div class="form-group float-label">
                        <label for="dmsDocuSignVoidReason">${button.data("void-msg")}</label>
                        <textarea rows="4" class="form-control form-control-sm" id="dmsDocuSignVoidReason" required></textarea>
                        <span class="field-validation-error" style="display:none;" data-valmsg-for="dmsDocuSignVoidReason"><span id="dmsDocuSignVoidReason-error">${button.data("void-error")}</span></span>
                    </div>`;
                    cpiConfirm.save(button.data("confirm-title"), popUpContent,
                        function () {
                            const voidReason = $(`textarea[id=dmsDocuSignVoidReason]`);
                            const error = $(`#dmsDocuSignVoidReason-error`).closest(".field-validation-error");
                            const reasonForVoid = voidReason.val().trim();

                            if (reasonForVoid === "") {
                                error.show();
                                voidReason.addClass("input-validation-error");
                                voidReason.focus();
                                throw validationError;
                            }
                            else {
                                error.hide();
                                voidReason.removeClass("input-validation-error");

                                cpiLoadingSpinner.show();
                                $.ajax({
                                    url: url,
                                    data: { voidReason: reasonForVoid },
                                    type: "POST",
                                    headers: { "RequestVerificationToken": $("[name='__RequestVerificationToken']").val() },
                                    success: (response) => {
                                        cpiLoadingSpinner.hide();
                                        disclosurePage.showDetails(self.currentRecordId);
                                        pageHelper.showSuccess(response.success);
                                    },
                                    error: function (e) {
                                        cpiLoadingSpinner.hide();
                                        pageHelper.showErrors(e);
                                    }
                                });
                            }
                        }
                    );
                }
                else {
                    cpiConfirm.confirm(button.data("confirm-title"), button.data("confirm-msg"),
                        function () {
                            cpiLoadingSpinner.show();
                            $.ajax({
                                url: url,
                                type: "POST",
                                headers: { "RequestVerificationToken": $("[name='__RequestVerificationToken']").val() },
                                success: (response) => {
                                    cpiLoadingSpinner.hide();
                                    disclosurePage.showDetails(self.currentRecordId);
                                    pageHelper.showSuccess(response.success);
                                },
                                error: function (e) {
                                    cpiLoadingSpinner.hide();
                                    pageHelper.showErrors(e);
                                }
                            });
                        }
                    );
                }

            });
        }
    }

    showFAQLink = (screen, title) => {
        const container = $(`#${screen}`).find(".cpiButtonsDetail");
        const navRefreshBtn = container.find(".nav .refresh-record");
        const self = this;
        if (navRefreshBtn) {
            navRefreshBtn.after(`<a class="nav-link dms-faq-link" href="#" target="_self" title="${title}" role="button"><i class="fal fa-circle-info pr-2"></i>${title}</a>`);
            container.find(".dms-faq-link").on("click", function () {
                self.viewFAQ();
            });
        }
    }

    viewFAQ() {
        let url = $("body").data("base-url") + "/DMS/FAQDoc/FAQZoom";
        let retry = 0;
        openFAQPopup();

        function openFAQPopup() {
            cpiLoadingSpinner.show();
            $.get(url)
                .done((result) => {
                    cpiLoadingSpinner.hide();
                    const popupContainer = $(".site-content .popup");
                    popupContainer.empty();
                    popupContainer.html(result);
                    popupContainer.find("#dmsFAQDocDialog").modal("show");
                })
                .fail((e) => {
                    cpiLoadingSpinner.hide();
                    if (e.status == 401 && retry < 3) {
                        retry++;
                        const baseUrl = $("body").data("base-url");
                        const url = `${baseUrl}/Graph/SharePoint`;

                        sharePointGraphHelper.getGraphToken(url, () => {
                            openFAQPopup();
                        });
                    }
                    else {
                        pageHelper.showErrors(e.responseText);
                    }
                });
        }
    }
}


