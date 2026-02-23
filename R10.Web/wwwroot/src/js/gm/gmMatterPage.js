import Image from "../image";
import ActivePage from "../activePage";

export default class MatterPage extends ActivePage {

    constructor() {
        super();
        this.image = new Image();
        this.showOutstandingActionsOnly = true;
        this.patMenuChoice = null;
        this.tmkMenuChoice = null;
        this.caseNumberSearchValueMapperUrl = "";
        this.docServerOperation = true;
    }

    init(addMode, isClientMatterOn, screen, isSharePointIntegrationOn) {
        this.tabsLoaded = [];
        this.docServerOperation = !isSharePointIntegrationOn;

        this.tabChangeSetListener();

        $(document).ready(() => {
            if (addMode) {
                if (isClientMatterOn) {
                    const client = this.getKendoComboBox("ClientID");
                    client.input.focus();
                }
                else {
                    const respOffice = this.getKendoComboBox("RespOffice");
                    if (respOffice) {
                        respOffice.input.focus();
                    }
                    else {
                        const caseNumber = this.getElement("CaseNumber");
                        caseNumber.focus();
                    }
                }
            }

            const countriesGrid = $("#gmCountriesGrid");
            countriesGrid.on("click", ".countryLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = countriesGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.Country);
                pageHelper.openLink(linkUrl, false);
            });

            const attorneysGrid = $("#gmAttorneysGrid");
            attorneysGrid.on("click", ".attorneyLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = attorneysGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.AttorneyID);
                pageHelper.openLink(linkUrl, false);
            });

            const otherPartiesGrid = $("#gmOtherPartiesGrid");
            otherPartiesGrid.on("click", ".otherPartyLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = otherPartiesGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.OtherParty);
                pageHelper.openLink(linkUrl, false);
            });
            //otherPartiesGrid.on("click", ".contactLink", (e) => {
            //    e.stopPropagation();

            //    let url = $(e.target).data("url");
            //    const row = $(e.target).closest("tr");
            //    const dataItem = otherPartiesGrid.data("kendoGrid").dataItem(row);
            //    const linkUrl = url.replace("actualValue", dataItem.Contact.ContactID);
            //    pageHelper.openLink(linkUrl, false);
            //});
            otherPartiesGrid.on("click", ".otherPartyTypeLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = otherPartiesGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.OtherPartyType);
                pageHelper.openLink(linkUrl, false);
            });

            const productsGrid = $(`#productsGrid_${this.mainDetailContainer}`);
            productsGrid.on("click", ".productLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = productsGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.ProductId);
                pageHelper.openLink(linkUrl, false);
            });

            const actionsGrid = $(`#actionsGrid_${this.mainDetailContainer}`);
            actionsGrid.on("click", ".action-link", (e) => {
                e.stopPropagation();
                e.preventDefault();

                if (this.actionActIds && this.actionActIds.length > 0) {
                    gmActionDuePage.mainSearchRecordIds = this.actionActIds;
                }
                const link = $(e.target);
                pageHelper.openDetailsLink(link);
            });

            $("#IsObligationContinue").change(function (e) {
                $(".obligation-length").removeClass("d-none").hide();
                if (e.target.checked)
                    $(".obligation-length").show();
            })

            if (document.getElementById("StartTimeTrack") != null) {
                $("#StartTimeTrack").off("click").on("click", function (e) {
                    let url = $(e.target).closest("#StartTimeTrack").data("url");
                    $.ajax({
                        type: "Get",
                        url: url,
                        contentType: false, // needed for file upload
                        processData: false, // needed for file upload
                        success: (result) => {
                            const popupContainer = $(".cpiContainerPopup").last();
                            popupContainer.empty();
                            popupContainer.html(result);
                            var dialogContainer = $("#timeTrackAttorneyDialog");
                            dialogContainer.modal("show");
                            let entryForm = popupContainer.find("form")[0];
                            entryForm = $(entryForm);
                            entryForm.cpiPopupEntryForm(
                                {
                                    dialogContainer: dialogContainer,
                                    closeOnSubmit: true,
                                    beforeSubmit: function () {
                                        GetSelectedAttorneyIds();
                                    },
                                    afterSubmit: function () {
                                        dialogContainer.modal("hide");
                                        document.getElementById("StartTimeTrack").setAttribute("hidden", "hidden");
                                        document.getElementById("StopTimeTrack").removeAttribute("hidden");
                                    }
                                }
                            );
                        },
                        error: function (e) {
                            pageHelper.showErrors(e);
                        }
                    });
                });
            }
        });

        this.emphasizeInactiveStatus(screen);
    }

    emphasizeInactiveStatus = (screen) => {
        $(document).ready(function () {
            const form = $(`#${screen}`).find("form");
            const activeSwitch = form.find("#IsActive");
            const matterStatus = form.find("input[name='MatterStatus_input']");
            if (activeSwitch.val() === "False") {
                matterStatus.attr("style", "color: #dc3545 !important");
            }
            if (matterStatus.val() && matterStatus.val().length > 0) {
                matterStatus.closest(".float-label").removeClass("inactive").addClass("active");
            }
            else {
                matterStatus.closest(".float-label").removeClass("active").addClass("inactive");
            }

        });
    }

    caseNumberSearchValueMapper = (options) => {
        const url = this.getCaseNumberSearchValueMapper();
        $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }

    getCaseNumberSearchValueMapper = () => {
        if (this.caseNumberSearchValueMapperUrl === "") {
            this.caseNumberSearchValueMapperUrl = $("#matterSearchCaseInfoTabContent").data("case-number-mapper-url");
            if (this.caseNumberSearchValueMapperUrl === undefined || this.caseNumberSearchValueMapperUrl === "")
                this.caseNumberSearchValueMapperUrl = $("#docMgtMainTabContent").data("case-number-mapper-url");
        }
        return this.caseNumberSearchValueMapperUrl;
    }

    onMatterTypeSelect(e) {
        if (e.item) {
            $(".agreement-info").removeClass("d-none").hide();
            $(".court-info").removeClass("d-none").hide();

            if (e.dataItem["MatterType"].toLowerCase() === "agreement") {
                $(".agreement-info").show();
            }

            if (e.dataItem["MatterType"].toLowerCase() === "litigation") {
                $(".court-info").show();
            }
        }
    }

    tabChangeSetListener() {
        $('#matterTab a').on('click', (e) => {
            e.preventDefault();
            const tab = e.target.id;
            if (this.tabsLoaded.indexOf(tab) === -1) {
                this.tabsLoaded.push(tab);
                this.loadTabContent(tab);
            }
        });
    }

    showDetails(result) {
        let id = result;
        if (isNaN(result))
            id = result.id;

        pageHelper.showDetails(this, id, () => {
            pageHelper.handleEmailWorkflow(result);
            //this.handleSaveWorkflow(result);
        });
    }

    //handleSaveWorkflow = (result) => {
    //    if (result.emailWorkflows) {
    //        let promise = this.processEmailWorkflow(result.id, result.emailWorkflows[0].isAutoEmail, result.emailWorkflows[0].qeSetupId, result.emailWorkflows[0].autoAttachImages);
    //        for (let i = 1; i < result.emailWorkflows.length; i++) {
    //            const workflow = result.emailWorkflows[i];
    //            promise = promise.then(() => {
    //                return this.processEmailWorkflow(result.id, workflow.isAutoEmail, workflow.qeSetupId, workflow.autoAttachImages);
    //            });
    //        }
    //    }
    //}

    //processEmailWorkflow = (id, isAutoEmail, qeSetupId, autoAttachImages) => {
    //    const deferred = $.Deferred();
    //    const baseUrl = $("body").data("base-url");
    //    let url = `${baseUrl}/GeneralMatter/Matter/Email`;

    //    $.get(url, { id: id, sendImmediately: isAutoEmail, qeSetupId: qeSetupId, autoAttachImages: autoAttachImages  })
    //        .done((emailResult) => {
    //            if (!isAutoEmail) {
    //                const popupContainer = $(".cpiContainerPopup").last();
    //                popupContainer.html(emailResult);
    //                const dialog = $("#quickEmailDialog");
    //                dialog.modal("show");
    //                dialog.find("#ok, #cancel,.close").on("click", () => {
    //                    $(".modal-backdrop").remove();
    //                    deferred.resolve();
    //                });
    //            }
    //            else deferred.resolve();
    //        })
    //        .fail(function (error) {
    //            pageHelper.showErrors(error.responseText);
    //            deferred.resolve();
    //        });
    //    return deferred.promise();
    //}

    deleteDueDate = (e, grid, deleteActionDuePrompt) => {
        this.deleteGridRow(e, grid);
    }

    loadTabContent(tab) {
        switch (tab) {
            case "matterDetailEntitiesTab":
                $(document).ready(() => {
                    const grid = $("#gmAttorneysGrid").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "matterDetailOtherPartiesTab":
                $(document).ready(() => {
                    const grid = $("#gmOtherPartiesGrid").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "matterDetailPatentsTab":
                $(document).ready(() => {
                    this.handleRelatedPatentsMenu();
                });
                break;

            case "matterDetailTrademarksTab":
                $(document).ready(() => {
                    this.handleRelatedTrademarksMenu();
                });
                break;

            case "matterDetailRelatedMattersTab":
                $(document).ready(() => {
                    const relatedMattersGrid = $(`#relatedMattersGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    relatedMattersGrid.dataSource.read();
                });
                break;

            case "matterDetailKeywordsTab":
                $(document).ready(() => {
                    const grid = $("#keywordsGridgmMatterDetail").data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "matterDetailDocumentsTab":
                $(document).ready(() => {
                    this.image.initializeImage(this, this.docServerOperation);
                });
                break;

            case "matterDetailCostsTab":
                $(document).ready(() => {
                    const costsGrid = $(`#costsGrid_${this.mainDetailContainer}`);
                    console.log(costsGrid);
                    const grid = costsGrid.data("kendoGrid");
                    grid.dataSource.read();
                    pageHelper.addBreadCrumbsRefreshHandler(costsGrid, () => {
                        grid.dataSource.read();
                        this.updateRecordStamps();
                    });
                });
                break;

            case "matterDetailActionsTab":
                $(document).ready(() => {
                    const actionsGrid = $(`#actionsGrid_${this.mainDetailContainer}`);
                    const grid = actionsGrid.data("kendoGrid");
                    $(".grid-options #showOutstandingActionsOnly").prop('checked', this.showOutstandingActionsOnly);
                    if (parseInt(grid.options.dataSource.pageSize) > 0)
                        grid.dataSource.pageSize(grid.options.dataSource.pageSize);
                    else
                        grid.dataSource.read();

                    pageHelper.addBreadCrumbsRefreshHandler(actionsGrid, () => {
                        grid.dataSource.read();
                        this.updateRecordStamps();
                        iManage.initializeViewer(this);
                        docViewer.initializeViewer(this);
                    });
                });
                break;

            case "matterDetailCorrespondenceTab":
                $(document).ready(() => {
                    const docsOutGrid = $(`#docsOutGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    docsOutGrid.dataSource.read();
                });
                break;

            case "matterDetailProductsTab":
                $(document).ready(() => {
                    const productsGrid = $(`#${this.mainDetailContainer}`).find(`#productsGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    if (productsGrid)
                        productsGrid.dataSource.read();
                });
                break;

            case "":
                break;
        }
    }

    handleRelatedPatentsMenu() {
        const relatedPatentInfo = $("#relatedPatentInfo");

        const url = relatedPatentInfo.data("url");
        const data = {
            matId: relatedPatentInfo.data("matid"),
            patMenuChoice: this.patMenuChoice ? this.patMenuChoice : "OurPatents"
        };

        this.loadOurPat = true;
        this.loadOtherPartyPat = true;

        const self = this;
        relatedPatentInfo.find("a").click(function () {
            const selected = $(this).data("value");
            data.patMenuChoice = selected;
            self.loadPatentGrid(url, data);

        });
        this.loadPatentGrid(url, data);
    }

    loadPatentGrid(url, data) {

        if (this.loadOurPat || this.loadOtherPartyPat) {
            $.get(url, data)
                .done((html) => {
                    if (data.patMenuChoice === "OurPatents") {
                        const tabContent = $("#ourPatentTabContent");
                        //tabContent.html(html);
                        if (!tabContent.hasClass("active"))
                            tabContent.addClass("active")
                        const name = "relatedPatentsGrid";
                        const el = $(`#${name}`);
                        const grid = el.data("kendoGrid");
                        grid.dataSource.read();

                        this.setOurPatentSettings();
                    } else {
                        const tabContent = $("#otherPartyPatentTabContent");
                        tabContent.html(html);
                        if (!tabContent.hasClass("active"))
                            tabContent.addClass("active")

                        this.setOtherPartyPatentSettings();
                    }
                })
                .fail(function (e) {
                    pageHelper.showErrors(e.responseText);
                });
        }

    }

    setOurPatentSettings() {
        if (this.loadOurPat) {
            const name = "gmMatterPatentsGrid";
            const el = $(`#${name}`);
            const grid = el.data("kendoGrid");
            const self = this;

            if (grid) {
                grid.dataSource.read();
                this.loadOurPat = false;
                const matId = el.closest("div").data("matid");

                if (!this.isEditableGridRegistered(name)) {
                    const gridInfo = {
                        name: name,
                        isDirty: false,
                        filter: { parentId: matId },
                        afterSubmit: this.updateRecordStamps
                    };
                    this.addEditableGrid(gridInfo);
                    pageHelper.kendoGridDirtyTracking(el, gridInfo);
                }
            }
        }
    }

    setOtherPartyPatentSettings() {
        if (this.loadOtherPartyPat) {
            const name = "gmOtherPartyPatentsGrid";
            const el = $(`#${name}`);
            const grid = el.data("kendoGrid");
            const self = this;

            if (grid) {
                grid.dataSource.read();
                this.loadOtherPartyPat = false;
                const matId = el.closest("div").data("matid");

                if (!this.isEditableGridRegistered(name)) {
                    const gridInfo = {
                        name: name,
                        isDirty: false,
                        filter: { parentId: matId },
                        afterSubmit: this.updateRecordStamps
                    };
                    this.addEditableGrid(gridInfo);
                    pageHelper.kendoGridDirtyTracking(el, gridInfo);
                }
            }
        }
    }

    handleRelatedTrademarksMenu() {
        const relatedTrademarkInfo = $("#relatedTrademarkInfo");

        const url = relatedTrademarkInfo.data("url");
        const data = {
            matId: relatedTrademarkInfo.data("matid"),
            tmkMenuChoice: this.tmkMenuChoice ? this.tmkMenuChoice : "OurTrademarks"
        };

        this.loadOurTmk = true;
        this.loadOtherPartyTmk = true;

        const self = this;
        relatedTrademarkInfo.find("a").click(function () {
            const selected = $(this).data("value");
            data.tmkMenuChoice = selected;
            self.loadTrademarkGrid(url, data);

        });
        this.loadTrademarkGrid(url, data);
    }

    loadTrademarkGrid(url, data) {

        if (this.loadOurTmk || this.loadOtherPartyTmk) {
            $.get(url, data)
                .done((html) => {
                    if (data.tmkMenuChoice === "OurTrademarks") {
                        const tabContent = $("#ourTrademarkTabContent");
                        //tabContent.html(html);
                        if (!tabContent.hasClass("active"))
                            tabContent.addClass("active")
                        const name = "relatedTrademarksGrid";
                        const el = $(`#${name}`);
                        const grid = el.data("kendoGrid");
                        grid.dataSource.read();
                        //this.setOurTrademarkSettings();
                    } else {
                        const tabContent = $("#otherPartyTrademarkTabContent");
                        tabContent.html(html);
                        if (!tabContent.hasClass("active"))
                            tabContent.addClass("active")

                        this.setOtherPartyTrademarkSettings();
                    }
                })
                .fail(function (e) {
                    pageHelper.showErrors(e.responseText);
                });
        }

    }

    setOurTrademarkSettings() {
        if (this.loadOurTmk) {
            const name = "gmMatterTrademarksGrid";
            const el = $(`#${name}`);
            const grid = el.data("kendoGrid");
            const self = this;

            if (grid) {
                grid.dataSource.read();
                this.loadOurTmk = false;
                const matId = el.closest("div").data("matid");

                el.find(".k-grid-toolbar").on("click",
                    ".ShowAddOurTrademark",
                    () => {
                        const url = $(`#gmMatterTrademarksGrid`).parent().data("url-add-related-trademark");
                        const id = $($(`#${this.detailContentContainer}`).find("form")[0]).find("#MatId").val();
                        const data = {
                            matId: id,
                        };

                        this.openTrademarkPopup("gmRelatedTrademarkSelectionGrid", "gmMatterTrademarksGrid", "gmRelatedTrademarkAddDialog", id, 0, url, data, true);
                    }
                );

                if (!this.isEditableGridRegistered(name)) {
                    const gridInfo = {
                        name: name,
                        isDirty: false,
                        filter: { parentId: matId },
                        afterSubmit: this.updateRecordStamps
                    };
                    this.addEditableGrid(gridInfo);
                    pageHelper.kendoGridDirtyTracking(el, gridInfo);
                }
            }
        }
    }

    showDefaultImage(tmkId, imgPath) {
        if (imgPath != "null") {
            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/FileViewer/GetImage?system=Trademark&filename=${imgPath}&screenCode=tmk&key=${tmkId}`;
            $("#defaultImage").prop("src", url);
            $("#imageWindow").show();
        }
    }

    showSharePointTmkDefaultImage(tmkId) {
        const element = $(`#tmk-sr-${tmkId}`);
        const displayUrl = element.data("display-url");
        console.log(displayUrl);

        if (displayUrl) {
            $("#defaultImage").prop("src", displayUrl);
            $("#imageWindow").show();
        }
    }

    showOtherTrademarkImage(gmId, imgPath) {
        if (imgPath != "null") {
            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/FileViewer/GetImage?system=GeneralMatter&filename=${imgPath}&screenCode=gmt&key=${gmId}`;
            $("#otherTmkImage").prop("src", url);
            $("#otherTmkImageWindow").show();
        }
    }

    showSharePointOtherTmkImage(gmoptId) {
        const element = $(`#gmoptId-sr-${gmoptId}`);
        const displayUrl = element.data("display-url");
        console.log(displayUrl);

        if (displayUrl) {
            $("#otherTmkImage").prop("src", displayUrl);
            $("#otherTmkImageWindow").show();
        }
    }

    hideDefaultImage(widndow) {
        $(`#${widndow}`).hide();
    }

    setOtherPartyTrademarkSettings() {
        if (this.loadOtherPartyTmk) {
            const name = "gmOtherPartyTrademarksGrid";
            const el = $(`#${name}`);
            const grid = el.data("kendoGrid");
            const self = this;

            if (grid) {
                grid.dataSource.read();
                this.loadOtherPartyTmk = false;
                const matId = el.closest("div").data("matid");
                const uploadForm = $("#otherPartyTrademarkLogoUpload");
                let selectedNPLRow = null;

                if (!this.isEditableGridRegistered(name)) {
                    const gridInfo = {
                        name: name,
                        isDirty: false,
                        filter: { parentId: matId },
                        afterSubmit: this.updateRecordStamps
                    };
                    this.addEditableGrid(gridInfo);
                    pageHelper.kendoGridDirtyTracking(el, gridInfo);
                }

                el.on("click", ".delete-file-link", function (e) {
                    e.preventDefault();

                    const uploadLink = $(e.currentTarget);
                    const gmoptId = uploadLink.data("id");

                    const title = el.parent().data("upload-title");
                    const msg = el.parent().data("delete-file-msg");
                    cpiConfirm.confirm(title, msg, function () {
                        const baseUrl = $("body").data("base-url");
                        const url = `${baseUrl}/GeneralMatter/MatterTrademarks/OtherPartyTrademarkLogoDelete`;

                        $.post(url, { gmoptId: gmoptId })
                            .done(function () {
                                grid.dataSource.read();
                                self.updateRecordStamps();
                            })
                            .fail(function (e) {
                                pageHelper.showErrors(e.responseText);
                            });
                    });
                });

                el.on("click", ".upload-link", function (e) {
                    e.preventDefault();

                    selectedNPLRow = $(e.target).closest("tr");
                    const uploadLink = $(e.currentTarget);
                    uploadForm.find("#GMOPTId").val(uploadLink.data("id"));

                    if ($.isNumeric(uploadLink.data("file"))) {

                        const title = el.parent().data("upload-title");
                        const msg = el.parent().data("upload-msg");
                        cpiConfirm.confirm(title, msg, function () { uploadForm.find("input").trigger("click"); });
                    }
                    else {
                        uploadForm.find("input").trigger("click");
                    }
                });

                uploadForm.find("input").on("change", (e) => {
                    uploadForm.submit();
                });

                uploadForm.on("submit", (e) => {
                    e.preventDefault();
                    e.stopPropagation();

                    const formData = new FormData(e.target);

                    const name = "gmOtherPartyTrademarksGrid";
                    const el = $(`#${name}`);
                    const grid = el.data("kendoGrid");

                    $.ajax({
                        type: "POST",
                        url: $(e.target).attr("action"),
                        data: formData,
                        contentType: false, // needed for file upload
                        processData: false, // needed for file upload
                        success: (result) => {
                            const msg = el.parent().data("upload-success");
                            pageHelper.showSuccess(msg);

                            //const row = grid.select();
                            const dataItem = grid.dataItem(selectedNPLRow);

                            dataItem.FileId = result.fileId;
                            dataItem.CurrentDocFile = result.origFileName;
                            dataItem.DocFilePath = result.fileName;

                            const rowHtml = grid.rowTemplate(dataItem);
                            selectedNPLRow.replaceWith(rowHtml);
                            uploadForm.find("input[Name='DocFile']").val(null);
                            self.updateRecordStamps();

                            grid.dataSource.read();
                        },
                        error: function (e) {
                            pageHelper.showErrors(e);
                        }
                    });

                });
            }
        }
    }

    openTrademarkPopup(gridName, parentGridName, dialogContainerName, matId, gmtId, url, data, closeOnSave) {
        const self = this;

        $.ajax({
            url: url,
            data: data,
            success: function (result) {

                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $(`#${dialogContainerName}`);

                dialogContainer.find("#save").on("click", function () {
                    const el = $(`#${gridName}`);
                    const grid = el.data("kendoGrid");
                    const from = grid.selectedKeyNames();
                    const saveUrl = $(this).data("url");
                    const saveData = {
                        matId: matId,
                        gmtId: gmtId,
                        from: from,
                    };

                    $.post(saveUrl, saveData)
                        .done(function () {
                            const parentGrid = $(`#${parentGridName}`).data("kendoGrid");

                            parentGrid.dataSource.read();
                            dialogContainer.modal("hide");
                            console.log(el);
                            console.log(el.parent());

                            var msg = el.parent().data("update-success");
                            if (saveData.from && saveData.from.length > 1) {
                                msg = el.parent().data("updates-success");
                            }
                            console.log(msg);
                            pageHelper.showSuccess(msg);
                        })
                        .fail(function (error) { pageHelper.showErrors(error.responseText); });
                });

                dialogContainer.find("#showActiveOnly").change(function () {
                    self.getRelatedTrademarkSources();
                });

                let entryForm = dialogContainer.find("form")[0];
                dialogContainer.modal("show");
                entryForm = $(entryForm);
                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialogContainer,
                        closeOnSubmit: closeOnSave,
                        beforeSubmit: function () {
                        },
                        afterSubmit: function (e) {
                            grid.dataSource.read();
                            self.updateRecordStamps();
                            dialogContainer.modal("hide");
                        }
                    }
                );
            },
        });
    }

    getRelatedTrademarkSources() {
        const grid = $("#gmRelatedTrademarkSelectionGrid").data("kendoGrid");
        grid._selectedIds = {};
        grid.clearSelection();
        grid.dataSource.read();
    }

    getRrelatedTrademarkParam(dialogContainerName) {
        let form = $(`#${dialogContainerName}`).find("form")[0];
        //let form = $("#gmRelatedTrademarkAddDialog").find("form")[0];
        form = $(form);

        const param = {
            caseNumber: form.find("input[name = 'CaseNumber']").val(),
            country: form.find("input[name = 'Country']").val(),
            subCase: form.find("input[name = 'SubCase']").val(),
            appNumber: form.find("input[name = 'AppNumber']").val(),
            regNumber: form.find("input[name = 'RegNumber']").val(),
            classIdList: form.find("select[name = 'ClassId']").data("kendoMultiSelect").value(),
            goods: form.find("input[name = 'Goods']").val(),
            trademarkName: form.find("input[name = 'TrademarkName']").val(),
            activeOnly: form.find("#showActiveOnly").is(':checked'),
            matId: form.find("#MatId").val()
        };

        if (form.find("#SearchText").length > 0)
            param.searchText = form.find("#SearchText").val();

        return param;

    }

    relatedTrademarkSelectionChange(parent) {
        const addButton = $(`#${parent}`).find("#save");

        const grid = $("#gmRelatedTrademarkSelectionGrid").data("kendoGrid");
        if (grid.selectedKeyNames().length > 0)
            addButton.removeAttr("disabled");
        else
            addButton.attr("disabled", "disabled");
    }

    editMatterTrademarksGridRow = (e, grid) => {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));

        const url = $(`#gmMatterTrademarksGrid`).parent().data("url-edit-related-trademark");
        const id = $($(`#${this.detailContentContainer}`).find("form")[0]).find("#MatId").val();
        const data = {
            GMTId: dataItem.GMTId,
        };

        this.openTrademarkPopup("gmRelatedTrademarkSelectionGrid", "gmMatterTrademarksGrid", "gmRelatedTrademarkEditDialog", dataItem.MatId, dataItem.GMTId, url, data, true);
    }

    ongmMatterTrademarkReplaceGridDataBound(e) {
        var rows = e.sender.tbody.children();
        for (var j = 0; j < rows.length; j++) {
            var row = $(rows[j]);
            row.addClass("k-alt");
        }

        const data = e.sender.dataSource.data();
        if (data.length > 0) {
            const listView = e.sender.element;
            $.each(listView.find(".tmkNameSearchResult-collapsible"), function () {
                $(this).textOverflow();

            });
        }
    }

    ongmMatterTrademarksGridDataBound(e) {
        var gridId = e.sender.element[0].id;
        var grid = $("#" + gridId).data("kendoGrid");
        var view = grid.dataSource.view();

        for (var i = 0; i < view.length; i++) {
            if (view[i].ReadOnly) {
                grid.tbody.find("tr[data-uid='" + view[i].uid + "']").addClass("read-only");
            }
        }

        const data = e.sender.dataSource.data();
        if (data.length > 0) {
            const listView = e.sender.element;
            $.each(listView.find(".tmkNameSearchResult-collapsible"), function () {
                $(this).textOverflow();
            });
        }
    }

    caseNumberDetailValueMapper(options) {
        const url = gmMatterPage.caseNumberDetailValueMapperUrl;

        $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }

    onChange_RelatedMatter = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");

            const caseNumber = e.dataItem["CaseNumber"];
            const title = e.dataItem["MatterTitle"];
            const status = e.dataItem["MatterStatus"];

            const grid = $("#gmMatterRelatedMattersGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            dataItem.RelatedId = e.dataItem["RelatedId"];
            dataItem.MatterTitle = title ? title : "";
            dataItem.MatterStatus = status ? status : "";
            dataItem.CaseNumber = caseNumber;

            $(row).find(".caseNumber-field").html(kendo.htmlEncode(caseNumber));
            $(row).find(".matterTitle-field").html(kendo.htmlEncode(dataItem.MatterTitle));
            $(row).find(".matterStatus-field").html(kendo.htmlEncode(dataItem.MatterStatus));
        }
    }

    gmMatterTrademarksGridRequestEnd = (e) => {
        if (e.response) {
            if (e.response.Data.length > 0) {
                const baseUrl = $("body").data("base-url");
                const authenticatedCheckUrl = `${baseUrl}/Shared/SharePointGraph/IsAuthenticated`;

                $.get(authenticatedCheckUrl)
                    .done(function () {
                        getThumbnails();
                    })
                    .fail(function (e) {
                        if (e.status == 401) {
                            const baseUrl = $("body").data("base-url");
                            const url = `${baseUrl}/Graph/SharePoint`;

                            sharePointGraphHelper.getGraphToken(url, () => {
                                getThumbnails();
                            });
                        }
                        else {
                            pageHelper.showErrors(e.responseText);
                        }
                    });

                function getThumbnails() {
                    const url = `${baseUrl}/Shared/SharePointGraph/GetDefaultWithThumbnailUrl?docLibrary=Trademark`;
                    let driveId = "";

                    e.response.Data.forEach((r) => {
                        if (r.SharePointRecKey && r.SharePointRecKey != "") {
                            const recKey = { Id: r.TmkId, RecKey: r.SharePointRecKey };
                            $.post(url, { docLibraryFolder: 'Trademark', driveId, recKey })
                                .done(function (result) {
                                    if (result && result.DriveId) {
                                        driveId = result.DriveId;
                                        const element = $(`#tmk-sr-${result.Id}`);
                                        element.attr("src", result.ThumbnailUrl);
                                        element.data("display-url", result.DisplayUrl);
                                    }
                                })
                                .fail(function (e) {
                                    pageHelper.showErrors(e.responseText);
                                });
                        }

                    });

                }
            }
        }
    }

    gmOtherPartyTrademarksGridRequestEnd = (e) => {
        if (e.response) {
            if (e.response.Data.length > 0) {
                const baseUrl = $("body").data("base-url");
                const authenticatedCheckUrl = `${baseUrl}/Shared/SharePointGraph/IsAuthenticated`;

                $.get(authenticatedCheckUrl)
                    .done(function () {
                        getThumbnails();
                    })
                    .fail(function (e) {
                        if (e.status == 401) {
                            const baseUrl = $("body").data("base-url");
                            const url = `${baseUrl}/Graph/SharePoint`;

                            sharePointGraphHelper.getGraphToken(url, () => {
                                getThumbnails();
                            });
                        }
                        else {
                            pageHelper.showErrors(e.responseText);
                        }
                    });

                function getThumbnails() {
                    const url = `${baseUrl}/Shared/SharePointGraph/GetThumbnailUrl`;
                    let driveId = "";

                    e.response.Data.forEach((r) => {
                        if (r.DriveItemId && r.DriveItemId != "") {
                            $.post(url, { docLibrary: 'General Matter', id: r.DriveItemId })
                                .done(function (result) {
                                    if (result) {
                                        const element = $(`#gmoptId-sr-${r.GMOPTId}`);
                                        element.attr("src", result.url.SmallThumbnailUrl);
                                        element.data("display-url", result.url.BigThumbnailUrl);
                                    }
                                })
                                .fail(function (e) {
                                    pageHelper.showErrors(e.responseText);
                                });
                        }

                    });

                }
            }
        }
    }

    handleGridDocViewer(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const filePath = dataItem.DocFilePath;
        const gmoptId = dataItem.GMOPTId;

        const container = $(grid.element[0]).closest("div.grid-container")
        const sharePointOn = container.data("is-sharepoint-on");

        if (sharePointOn == "1") {
            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/GeneralMatter/MatterTrademarks/GetSharePointPreviewUrl`;

            $.get(url, { fileName: filePath, matId: container.data("MatId"), gmoptId: gmoptId })
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
                    pageHelper.showErrors(e.responseText);
                });
        }
        else {
            const url = container.data("viewer-url") + "&docfile=" + filePath + "&key=" + gmoptId;
            documentPage.zoomDocument(url);
        }

    }

    filterCtryApp = (e) => {
        var grid = $("#gmMatterPatentsGrid").data("kendoGrid");
        var selectedData = grid.dataItem(grid.select());

        return {
            InvID: selectedData.InvID,
            request: e
        };
    }

    onChange_RelatedPatentInv = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");

            const country = e.dataItem["Country"] = "";
            const subCase = e.dataItem["SubCase"] = "";
            const caseType = e.dataItem["CaseType"] = "";
            const Patent = e.dataItem["Patent"];

            const grid = $("#gmMatterPatentsGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            dataItem.InvID = e.dataItem["InvID"];
            dataItem.Patent = Patent;
            dataItem.caseNumber = e.dataItem["CaseNumber"];

            dataItem.AppID = e.dataItem["AppID"] = null;
            dataItem.Country = country;
            dataItem.SubCase = subCase;
            dataItem.CaseType = caseType;
            dataItem.AppNumber = e.dataItem["AppNumber"] = null;
            dataItem.PubNumber = e.dataItem["PubNumber"] = null;
            dataItem.PatNumber = e.dataItem["PatNumber"] = null;

            $(row).find(".country-field").html(kendo.htmlEncode(country));
            $(row).find(".subCase-field").html(kendo.htmlEncode(subCase));
            $(row).find(".caseType-field").html(kendo.htmlEncode(caseType));

            $(row).find(".title-field").html(kendo.htmlEncode(Patent));
        }
    }

    onChange_RelatedPatentCtryApp = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");

            const subCase = e.dataItem["SubCase"];
            const caseType = e.dataItem["CaseType"];
            const Patent = e.dataItem["Patent"];

            const grid = $("#gmMatterPatentsGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            dataItem.AppID = e.dataItem["AppID"];
            dataItem.Country = e.dataItem["Country"];
            dataItem.SubCase = subCase;
            dataItem.CaseType = caseType;
            dataItem.Patent = Patent;
            dataItem.caseNumber = e.dataItem["CaseNumber"];
            dataItem.AppNumber = e.dataItem["AppNumber"];
            dataItem.PubNumber = e.dataItem["PubNumber"];
            dataItem.PatNumber = e.dataItem["PatNumber"];

            $(row).find(".subCase-field").html(kendo.htmlEncode(subCase));
            $(row).find(".caseType-field").html(kendo.htmlEncode(caseType));
            $(row).find(".title-field").html(kendo.htmlEncode(Patent));

        }
    }

    onChange_RelatedTrademark = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");

            const country = e.dataItem["Country"];
            const subCase = e.dataItem["SubCase"];
            const caseType = e.dataItem["CaseType"];
            const trademarkName = e.dataItem["Trademark"];

            const grid = $("#gmMatterTrademarksGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            dataItem.TmkId = e.dataItem["TmkId"];
            dataItem.Country = country;
            dataItem.SubCase = subCase;
            dataItem.CaseType = caseType;
            dataItem.Trademark = trademarkName;
            dataItem.AppNumber = e.dataItem["AppNumber"];
            dataItem.PubNumber = e.dataItem["PubNumber"];
            dataItem.RegNumber = e.dataItem["RegNumber"];
            dataItem.caseNumber = e.dataItem["CaseNumber"];

            $(row).find(".country-field").html(kendo.htmlEncode(country));
            $(row).find(".subCase-field").html(kendo.htmlEncode(subCase));
            $(row).find(".caseType-field").html(kendo.htmlEncode(caseType));
            $(row).find(".trademarkName-field").html(kendo.htmlEncode(trademarkName));

        }
    }

    //------------------------------------------------------------ COPY
    showCopyScreen() {
        //const popupContainer = $(".cpiContainerPopup").last();
        const popupContainer = $(".site-content .popup").last();
        const dialogContainer = popupContainer.find("#gmMatterCopyDialog");
        let entryForm = dialogContainer.find("form")[0];
        dialogContainer.modal("show");
        const self = this;

        //$(dialogContainer.find("#tmkCopyButton")).click(function () { self.copyTrademarks(); });

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

    mainCopyInitialize = (copyMatId) => {
        const baseUrl = $("body").data("base-url");
        const mainUrl = `${baseUrl}/GeneralMatter/Matter`;

        $(document).ready(() => {
            const container = $("#gmMatterCopyDialog");
            container.find(".case-info-settings").hide();

            container.on("click", ".case-info-set", () => {
                container.find(".case-info-settings").show();
                container.find(".data-to-copy").hide();

                const url = `${mainUrl}/GetCopySettings`;
                $.get(url, { copyMatId: copyMatId })
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
            container.on("change", ".data-to-copy input[type='checkbox']", function () {
                const checkbox = $(this);
                const name = checkbox.attr("name");
                const value = checkbox.prop("checked");
                const url = `${mainUrl}/UpdateCopyMainSetting`;
                $.post(url, { name: name, copy: value })
                    .fail(function (e) {
                        pageHelper.showErrors(e.responseText);
                    });
            });
        });
    }

    /* product */
    onChange_Product = (e) => {
        if (e.sender) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");
            const grid = $("#productsGrid_gmMatterDetail").data("kendoGrid");
            const dataItem = grid.dataItem(row);

            var comboDataItem = e.sender.dataItem();
            dataItem.ProductId = comboDataItem["ProductId"];
            dataItem.ProductName = comboDataItem["ProductName"];

        }
    }
    productRefreshIndicator = (e) => {
        const data = e.sender.dataSource.data();
        if (data.length == 0)
            $("#matterDetailProductsTab").removeClass("has-products");
        else
            $("#matterDetailProductsTab").addClass("has-products");
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

    showDueDateEmailScreen(e, grid) {
        const form = $("#" + this.detailContentContainer).find("form");
        const url = form.data("duedate-email-url");
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const ddId = dataItem.DDId;

        $.ajax({
            url: url,
            data: { id: ddId },
            success: function (result) {
                const popupContainer = $(".site-content .popup");
                popupContainer.html(result);
            },
            error: function (e) {
                pageHelper.showErrors(e);
            }
        });
    }
}





