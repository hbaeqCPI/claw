import Image from "../image";
import ActivePage from "../activePage";

export default class TmkTrademarkPage extends ActivePage {

    constructor() {
        super();
        this.caseNumberSearchValueMapperUrl = "";
        this.showOutstandingActionsOnly = true;
        this.defaultImageUrl = "";
        this.goodsDescriptionUrl = "";
        this.RenewalUrl = ""; //window
        this.AnyRenewalChangeUrl = "";
        this.image = new Image();
        this.previousClient = null;
        this.docServerOperation = true;
        this.isSharePointIntegrationOn = false;
        this.selectedDesCountry = "";
    }

    initialize = (screen, id, isSharePointIntegrationOn) => {
        this.docServerOperation = !isSharePointIntegrationOn;
        this.isSharePointIntegrationOn = isSharePointIntegrationOn;

        this.editableGrids = [
            {
                name: `assignmentsGrid_${screen}`, filter: { parentId: id },
                afterSubmit: this.updateRecordStamps,
                onDirty: this.assignmentsGrid_onDirty,
                onSave: this.assignmentsGrid_onSaveCancel,
                onCancel: this.assignmentsGrid_onSaveCancel,
            },
            { name: "classesGrid", filter: { parentId: id }, afterSubmit: this.updateRecordStamps },
            { name: "keywordsGrid", filter: { parentId: id }, afterSubmit: this.updateRecordStamps },
            { name: `actionsGrid_${screen}`, filter: { parentId: id }, afterSubmit: this.updateRecordStamps },
            { name: `licenseesGrid_${screen}`, filter: { parentId: id }, afterSubmit: this.updateRecordStamps },
            { name: "tmkDesignatedCountriesGrid", filter: { parentId: id }, afterSubmit: this.updateRecordStamps },
            { name: "conflictsGrid", filter: { parentId: id }, afterSubmit: this.updateRecordStamps },
            { name: "relatedPatentsGrid", filter: { parentId: id }, afterSubmit: this.updateRecordStamps },
            { name: `productsGrid_${screen}`, filter: { parentId: id }, afterSubmit: this.updateRecordStamps },
            { name: `ownersGrid_${screen}`, filter: { parentId: id }, afterSubmit: this.updateRecordStamps },
            { name: "relatedTrademarksGrid", filter: { parentId: id }, afterSubmit: this.updateRecordStamps },
            { name: `relatedMattersGrid_${screen}`, filter: { parentId: id }, afterSubmit: this.updateRecordStamps }
        ];

        this.tabsLoaded = [];
        this.tabChangeSetListener();
        this.emphasizeInactiveStatus(screen);

        $(document).ready(() => {
            const self = this;
            let selectedUploadRow = null;
            let selectedUploadGrid = null;

            const classesGrid = $("#classesGrid");
            classesGrid.on("click", ".classLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = classesGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.TmkStandardGood.ClassId);
                pageHelper.openLink(linkUrl, false);
            });

            classesGrid.on("click", ".k-grid-Copy", () => {
                const baseUrl = $("body").data("base-url");
                const url = `${baseUrl}/Trademark/TmkTrademark/GetClassCopyScreen`;

                $.get(url, { tmkId: this.currentRecordId }).done((result) => {
                    $(".cpiContainerPopup").empty();
                    const popupContainer = $(".cpiContainerPopup").last();
                    popupContainer.html(result);
                    const dialog = $("#tmkCopyGoodsDialog");
                    dialog.modal("show");

                    dialog.find("#copy").on("click", () => {
                        const grid = dialog.find(".kendo-Grid").data("kendoGrid");
                        const copyUrl = `${baseUrl}/Trademark/TmkTrademarkClass/CopyGoods`;
                        const data = {
                            tmkId: this.currentRecordId,
                            from: grid.selectedKeyNames(),
                        };
                        $.post(copyUrl, data)
                            .done(function (response) {
                                classesGrid.data("kendoGrid").dataSource.read();
                                dialog.modal("hide");
                                pageHelper.showSuccess(response.success);
                            })
                            .fail(function (error) { pageHelper.showErrors(error.responseText); });
                    });

                }).fail(function (error) {
                    pageHelper.showErrors(error.responseText);
                });
            });

            const ownersGrid = $(`#ownersGrid_${this.mainDetailContainer}`);
            ownersGrid.on("click", ".ownerLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = ownersGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.OwnerID);
                pageHelper.openLink(linkUrl, false);
            });

            //for assignments, licensees
            const uploadForm = $("#tmkFileUpload");
            uploadForm.find("input").on("change", (e) => {
                uploadForm.submit();
            });

            uploadForm.on("submit", (e) => {
                e.preventDefault();
                e.stopPropagation();

                const formData = new FormData(e.target);

                $.ajax({
                    type: "POST",
                    url: $(e.target).attr("action"),
                    data: formData,
                    contentType: false, // needed for file upload
                    processData: false, // needed for file upload
                    success: (result) => {
                        const msg = selectedUploadGrid.parent().data("upload-success");
                        const grid = selectedUploadGrid.data("kendoGrid");

                        pageHelper.showSuccess(msg);

                        const dataItem = grid.dataItem(selectedUploadRow);

                        dataItem.FileId = result.fileId;
                        dataItem.CurrentDocFile = result.origFileName;
                        dataItem.DocFilePath = result.fileName;

                        const rowHtml = grid.rowTemplate(dataItem);
                        selectedUploadRow.replaceWith(rowHtml);
                        uploadForm.find("input[Name='DocFile']").val(null);
                        self.updateRecordStamps();
                    },
                    error: function (e) {
                        pageHelper.showErrors(e);
                    }
                });
            });

            const assignmentsGrid = $(`#assignmentsGrid_${this.mainDetailContainer}`);
            assignmentsGrid.on("click", ".assignmentStatusLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = assignmentsGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.AssignmentStatus);
                pageHelper.openLink(linkUrl, false);
            });

            assignmentsGrid.on("click", ".upload-link", function (e) {
                const baseUrl = $("body").data("base-url");
                const url = `${baseUrl}/Trademark/TmkAssignment/AssignmentFileUpload`;
                selectedUploadRow = $(e.target).closest("tr");
                selectedUploadGrid = assignmentsGrid;
                self.fileUpload(uploadForm, url, assignmentsGrid, e);
            });


            assignmentsGrid.on("click", ".delete-file-link", function (e) {
                e.preventDefault();

                const uploadLink = $(e.currentTarget);
                const childId = uploadLink.data("id");

                const title = assignmentsGrid.parent().data("upload-title");
                const msg = assignmentsGrid.parent().data("delete-file-msg");
                cpiConfirm.confirm(title, msg, function () {
                    const baseUrl = $("body").data("base-url");
                    const url = `${baseUrl}/Trademark/TmkAssignment/AssignmentFileDelete`;

                    $.post(url, { childId: childId })
                        .done(function () {
                            assignmentsGrid.data("kendoGrid").dataSource.read();
                            self.updateRecordStamps();
                        })
                        .fail(function (e) {
                            pageHelper.showErrors(e.responseText);
                        });
                });
            });

            const licenseesGrid = $(`#licenseesGrid_${this.mainDetailContainer}`);
            licenseesGrid.on("click", ".delete-file-link", function (e) {
                e.preventDefault();

                const uploadLink = $(e.currentTarget);
                const childId = uploadLink.data("id");

                const title = licenseesGrid.parent().data("upload-title");
                const msg = licenseesGrid.parent().data("delete-file-msg");
                cpiConfirm.confirm(title, msg, function () {
                    const baseUrl = $("body").data("base-url");
                    const url = `${baseUrl}/Trademark/TmkLicensee/LicenseeFileDelete`;

                    $.post(url, { childId: childId })
                        .done(function () {
                            licenseesGrid.data("kendoGrid").dataSource.read();
                            self.updateRecordStamps();
                        })
                        .fail(function (e) {
                            pageHelper.showErrors(e.responseText);
                        });
                });
            });

            licenseesGrid.on("click", ".upload-link", function (e) {
                const baseUrl = $("body").data("base-url");
                const url = `${baseUrl}/Trademark/TmkLicensee/LicenseeFileUpload`;
                selectedUploadRow = $(e.target).closest("tr");
                selectedUploadGrid = licenseesGrid;
                self.fileUpload(uploadForm, url, licenseesGrid, e);
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
                    tmkActionDuePage.mainSearchRecordIds = this.actionActIds;
                }
                const link = $(e.target);
                pageHelper.openDetailsLink(link);
            });

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

            if (isSharePointIntegrationOn) {
                this.image.refreshSharePointDefaultImage(this);
            }
            else {
                this.image.refreshDefaultImage(this);
            }
        })
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
            this.caseNumberSearchValueMapperUrl = $("#tmkTrademarkSearchMainTabContent").data("case-number-mapper-url");
            if (this.caseNumberSearchValueMapperUrl === undefined || this.caseNumberSearchValueMapperUrl === "")
                this.caseNumberSearchValueMapperUrl = $("#docMgtMainTabContent").data("case-number-mapper-url");
        }
        return this.caseNumberSearchValueMapperUrl;
    }

    appNumberSearchValueMapper = (options) => {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Trademark/TmkTrademarkLookup/AppNumberSearchValueMapper`;
        $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }

    pubNumberSearchValueMapper = (options) => {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Trademark/TmkTrademarkLookup/PubNumberSearchValueMapper`;
        $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });

    }

    regNumberSearchValueMapper = (options) => {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Trademark/TmkTrademarkLookup/RegNumberSearchValueMapper`;
        $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });

    }

    searchResultGridRequestEnd = (e) => {
        cpiStatusMessage.hide();
        this.mainSearchRecordIds = [];

        if (e.response) {
            $(this.refineSearchContainer).find(".total-results-count").html(e.response.Total);

            if (e.response.Data.length > 0) {
                if (this.isSharePointIntegrationOn) {
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
                        });

                    }

                }

                this.mainSearchRecordIds = e.response.Ids;
                $(this.searchResultContainer).find(".no-results-hide").show();
            }
            else if (this.showNoRecordError) {
                const form = $(`${this.searchContainer}-MainSearch`);
                pageHelper.showErrors($(form).data("no-results") || $("body").data("no-results"));
                $(this.searchResultContainer).find(".no-results-hide").hide();
            }
        }
    }

    classGridDataBound = (e) => {
        //const dataItems = e.sender.dataSource.view();
        const classesGrid = $("#classesGrid");
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Trademark/TmkTrademark/HasCopyScreen`;

        $.get(url, { tmkId: this.currentRecordId }).done((result) => {
            if (result.hasCopy) {
                classesGrid.find(".k-grid-Copy").removeClass("d-none");
            }
            else classesGrid.find(".k-grid-Copy").addClass("d-none");
        }).fail(function (error) {
            pageHelper.showErrors(error.responseText);
        });
        
    }

    fileUpload = (uploadForm, url, grid, e) => {
        e.preventDefault();

        const uploadLink = $(e.currentTarget);
        uploadForm.find("#ChildId").val(uploadLink.data("id"));
        uploadForm.attr("action", url);

        if ($.isNumeric(uploadLink.data("file"))) {
            const title = grid.parent().data("upload-title");
            const msg = grid.parent().data("upload-msg");
            cpiConfirm.confirm(title, msg, function () { uploadForm.find("input").trigger("click"); });
        }
        else {
            uploadForm.find("input").trigger("click");
        }
    };

    handleGridDocViewer(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const idsFile = dataItem.DocFilePath;

        const container = $(grid.element[0]).closest("div.grid-container")
        const url = container.data("viewer-url") + "&docfile=" + idsFile + "&key=" + dataItem.TmkId;
        documentPage.zoomDocument(url);
    }

    isGridDateTakenColumnEditable = (data) => {
        var readOnly = (data.ReadOnly === undefined ? false : data.ReadOnly) || (data.Indicator === "Ren/Due");
        //return !readOnly && !$(`#${this.detailContentContainer}`).hasClass("dirty");
        return !readOnly
    }

    caseNumberDetailValueMapper = (options) => {
        const form = $("#" + this.detailContentContainer).find("form");
        const url = $(form).data("case-number-mapper-url");

        $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }

    trademarkNameValueMapper = (options) => {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Trademark/TrademarkNameSearchValueMapper`;

        $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }


    trademarkNameValueMapper(options) {
        const url = $("#tmkTrademarkSearchMainTabContent").data("case-trademarkname-mapper-url");
        $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }

    emphasizeInactiveStatus = (screen) => {
        $(document).ready(function () {
            const form = $(`#${screen}`).find("form");
            const activeSwitch = form.find("#IsActive");
            if (activeSwitch.val() === "False") {
                form.find("input[name='TrademarkStatus_input']").attr("style", "color: #dc3545 !important");
            }
        });
    }

    tabChangeSetListener() {
        $('#trademarkDetailTab a').on('click', (e) => {
            e.preventDefault();
            const tab = e.target.id;
            if (this.tabsLoaded.indexOf(tab) === -1) {
                this.tabsLoaded.push(tab);
                this.loadTabContent(tab);
            }

            if (tab === "trademarkDetailCaseInfoTab")
                $(".tmk-main-info").addClass("d-none");
            else
                $(".tmk-main-info").removeClass("d-none");

            if (tab === "trademarkDetailEntitiesTab")
                $(".tmk-entities-info").addClass("d-none");
            else
                $(".tmk-entities-info").removeClass("d-none");

            if (tab === "trademarkDetailClassesTab")
                $(".tmk-goods-info").addClass("d-none");
            else
                $(".tmk-goods-info").removeClass("d-none");
        });
    }

    deleteDueDate = (e, grid, deleteActionDuePrompt) => {
        this.deleteGridRow(e, grid);
    }

    recordNavigateHandler = (id) => {
        this.showTmkDetails(id);
    }

    showTmkDetails = (id) => {
        if (isNaN(id))
            id = id.id;

        //get active TL tab before loading the next record
        const container = $(`#${this.detailContentContainer}`);
        this.activeTLTab = container.find("#tlMenuChoice").val();
        pageHelper.showDetails(this, id);
    };

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
    //    let url = `${baseUrl}/Trademark/TmkTrademark/Email`;

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

    loadTabContent = (tab) => {

        switch (tab) {
            case "trademarkDetailClassesTab":
                $(document).ready(function () {
                    const classesGrid = $("#classesGrid").data("kendoGrid");
                    classesGrid.dataSource.read();
                });
                break;
            case "trademarkDetailEntitiesTab":
                $(document).ready(() => {
                    const ownersGrid = $(`#ownersGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    if (ownersGrid) {
                        ownersGrid.dataSource.read();
                    }
                });
                break;
            case "trademarkDetailAssignmentsTab":
                $(document).ready(() => {
                    const assignmentsGrid = $(`#assignmentsGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    assignmentsGrid.dataSource.read();
                    this.handleAssignmentMenu();
                });
                break;
            case "trademarkDetailKeywordsTab":
                $(document).ready(function () {
                    const keywordsGrid = $("#keywordsGrid").data("kendoGrid");
                    keywordsGrid.dataSource.read();
                });
                break;
            case "trademarkDetailActionsTab":
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
            case "trademarkDetailCostsTab":
                $(document).ready(() => {
                    const costsGrid = $(`#costsGrid_${this.mainDetailContainer}`);
                    const grid = costsGrid.data("kendoGrid");
                    grid.dataSource.read();
                    pageHelper.addBreadCrumbsRefreshHandler(costsGrid, () => {
                        grid.dataSource.read();
                        this.updateRecordStamps();
                    });
                });
                break;
            case "trademarkDetailDocumentsTab":
                $(document).ready(() => {
                    this.image.initializeImage(this, this.docServerOperation);
                });
                break;
            case "trademarkDetailLicenseesTab":
                $(document).ready(() => {
                    const licenseesGrid = $(`#licenseesGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    licenseesGrid.dataSource.read();
                });
                break;
            case "trademarkDetailDesCtryTab":
                $(document).ready(() => {
                    this.setDesCountrySettings();
                });
                break;
            case "trademarkDetailConflictsTab":
                $(document).ready(() => {
                    const conflictsGrid = $("#conflictsGrid");
                    const grid = conflictsGrid.data("kendoGrid");
                    grid.dataSource.read();
                    pageHelper.addBreadCrumbsRefreshHandler(conflictsGrid, () => {
                        grid.dataSource.read();
                        this.updateRecordStamps();
                    });
                });
                break;

            case "trademarkPDTTab":
                $(document).ready(() => {
                    this.tlLoadPublicDataMenu();
                });
                break;

            case "trademarkDetailRelaterMattersTab":
                $(document).ready(() => {
                    const relatedMattersGrid = $(`#relatedMattersGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    relatedMattersGrid.dataSource.read();
                });
                break;

            case "trademarkDetailWebLinksTab":
                $(document).ready(function () {
                    const webLinksGrid = $("#trademarkDetailWebLinksTabContent .webLinksGrid").data("kendoGrid");
                    webLinksGrid.dataSource.read();
                });
                break;

            case "trademarkRelatedPatentTab":
                $(document).ready(function () {
                    const relatedPatentsGrid = $("#relatedPatentsGrid");
                    const grid = relatedPatentsGrid.data("kendoGrid");
                    grid.dataSource.read();
                    pageHelper.addBreadCrumbsRefreshHandler(relatedPatentsGrid, () => { grid.dataSource.read() });
                });
                break;

            case "trademarkRelatedTrademarkTab":
                $(document).ready(function () {
                    const relatedTrademarksGrid = $("#relatedTrademarksGrid");
                    const grid = relatedTrademarksGrid.data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "trademarkDetailRelatedSearchRequestTab":
                $(document).ready(() => {
                    const relatedSearchRequestsGrid = $("#tmkRelatedSearchRequestsGrid");
                    const grid = relatedSearchRequestsGrid.data("kendoGrid");
                    grid.dataSource.read();
                });
                break;

            case "trademarkDetailCorrespondenceTab":
                $(document).ready(() => {
                    const docsOutGrid = $(`#docsOutGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    docsOutGrid.dataSource.read();
                });
                break;

            case "trademarkProductsTab":
                $(document).ready(() => {
                    const productsGrid = $(`#productsGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    if (productsGrid)
                        productsGrid.dataSource.read();
                });
                break;
            case "":
                break;
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

    getCountryCaseType() {
        let form = $(`#${this.mainDetailContainer}`).find("form")[0];
        form = $(form);
        const values = form.serializeArray();
        var country = values.find((el) => el.name === 'Country');
        var caseType = values.find((el) => el.name === 'CaseType');
        return {
            country: country.value,
            caseType: caseType.value
        }
    }

    //------------------------------------------------------------ goods/classes
    updateGoods(e) {
        // get goods description
        if (e.item) {
            const url = this.goodsDescriptionUrl + "/" + e.dataItem["ClassId"];
            $.get(url).done(function (response) {
                const grid = $("#classesGrid").data("kendoGrid");

                // grab existing row or new row handle
                const selItem = grid.dataItem(grid.select()) || grid.dataSource.data()[0];    // to be generic, replace 0 with current row index
                if (selItem !== null) {
                    selItem.set("Goods", response);
                    selItem.set("IsStandardGoods", true);
                    return;
                }
                //$(`#${e.sender.element[0].id}`).closest("tr").find(".goods-field").html(response);    // does not change bound control
            });
        }
    }

    //------------------------------------------------------------ renewal date calculation
    calculateRenewal = (callback) => {
        // extract fields that will affect renewal date calculation
        const pageId = "tmkTrademarkDetail";

        var country = $(`#${pageId}`).find("#Country");
        var countryDisabled = country.attr("readonly");
        if (countryDisabled) {
            if (callback)
                callback();
            return;
        }

        const param = new Object();
        param.TmkId = $("#TmkId").val();
        param.Country = $(`#Country_${pageId}`).data('kendoComboBox').value();
        param.CaseType = $(`#CaseType_${pageId}`).data('kendoMultiColumnComboBox').value();

        param.FilDate = pageHelper.cpiDateFormatToSave($(`#FilDate_${pageId}`).data("kendoDatePicker").value());
        param.PubDate = pageHelper.cpiDateFormatToSave($(`#PubDate_${pageId}`).data("kendoDatePicker").value());
        param.RegDate = pageHelper.cpiDateFormatToSave($(`#RegDate_${pageId}`).data("kendoDatePicker").value());
        param.PriDate = pageHelper.cpiDateFormatToSave($(`#PriDate_${pageId}`).data("kendoDatePicker").value());
        param.ParentFilDate = pageHelper.cpiDateFormatToSave($(`#ParentFilDate_${pageId}`).data("kendoDatePicker").value());
        param.AllowanceDate = pageHelper.cpiDateFormatToSave($(`#AllowanceDate_${pageId}`).data("kendoDatePicker").value());
        param.LastRenewalDate = pageHelper.cpiDateFormatToSave($(`#LastRenewalDate_${pageId}`).data("kendoDatePicker").value());
        param.NextRenewalDate = pageHelper.cpiDateFormatToSave($(`#NextRenewalDate_${pageId}`).data("kendoDatePicker").value());

        const tmkId = parseInt(param.TmkId);
        let renewalParamChanged = false;
        const self = this;

        CheckRenewalDate();

        function CheckRenewalDate() {
            if (tmkId < 1) {
                // new record, calculate renewal
                if (param.FilDate !== null || param.PubDate !== null || param.PubDate !== null || param.RegDate !== null || param.PriDate !== null ||
                    param.ParentFilDate !== null || param.AllowanceDate !== null || param.LastRenewalDate !== null || param.NextRenewalDate !== null) {
                    renewalParamChanged = true;
                }
                CalculateRenewalDate();
            }
            else {
                // existing record, check if there are changes to dates that affect renewal calculation
                $.ajax({
                    url: self.AnyRenewalChangeUrl,
                    data: { param: param },
                    type: "POST",
                    success: function (result) {
                        renewalParamChanged = result;
                        CalculateRenewalDate();
                    }
                });
            }
        }

        function CalculateRenewalDate() {
            if (renewalParamChanged) {
                // if renewal parameters changed, calculate renewal
                const paramData = {
                    TmkId: param.TmkId, Country: param.Country, CaseType: param.CaseType, FilDate: param.FilDate, PubDate: param.PubDate, RegDate: param.RegDate,
                    PriDate: param.PriDate, ParentFilDate: param.ParentFilDate, AllowanceDate: param.AllowanceDate, LastRenewalDate: param.LastRenewalDate, NextRenewalDate: param.NextRenewalDate
                };
                $.ajax({
                    url: self.RenewalUrl,
                    //data: { param: param },
                    data: paramData,
                    type: "GET",
                    success: function (result) {
                        PromptForRenewalDate(result);       // verify with user renewal date calculation
                        if (callback)                       // exec callback function
                            callback();
                    }
                });
            }
            else {
                // no change in renewal parameters
                if (callback)
                    callback();                             // exec callback function
            }
        }

        function PromptForRenewalDate(renObj) {
            if (renObj.Message === null || renObj.Message === '')           // empty message ==> no renewal date calculated
                return;

            const nextRenDate = $(`#NextRenewalDate_${pageId}`).data("kendoDatePicker");
            const response = confirm(renObj.Message);
            if (renObj.NoOfRenDates === 1 && response)                   // 1 ren date and OK to apply
                nextRenDate.value(new Date(renObj.CalcRenewalDate));
            if (renObj.NoOfRenDates > 1 && response)                    // multiple ren dates and OK to clear
                nextRenDate.value(null);
        }

    }

    //------------------------------------------------------------ family/designation
    onDesCountryDataBound(e) {
        const gridData = e.sender.dataSource.view();

        for (let i = 0; i < gridData.length; i++) {
            const currentUid = gridData[i].uid;

            if (!(gridData[i].GenApp && gridData[i].GenDate === null)) {
                const currentRow = e.sender.table.find("tr[data-uid='" + currentUid + "']");
                const genButton = $(currentRow).find(".k-grid-Generate");
                genButton.hide();
            }
        }
    }

    setDesCountrySettings = () => {
        const el = $("#tmkDesignatedCountriesGrid");
        const grid = el.data("kendoGrid");
        const self = this;
        if (grid) {
            grid.dataSource.read();
            const pageSize = grid.dataSource.pageSize();
            const parent = el.parent();
            const confirmTitle = parent.data("msg-confirm-title");

            el.find(".k-grid-toolbar").on("click",
                ".k-grid-LoadCL",
                function () {
                    const confirmMsg = parent.data("msg-confirm-cl");
                    cpiConfirm.confirm(confirmTitle, confirmMsg, function () {
                        const data = { tmkId: parent.data("tmkid"), fromCountryLaw: true };
                        $.post(parent.data("url-load"), data).done(function () {
                            //grid.dataSource.query({ page: 1, pageSize: pageSize }).fail(function (error) {
                            //    pageHelper.showErrors(error.responseText);
                            //});
                            grid.dataSource.read().fail(function (error) {
                                pageHelper.showErrors(error.responseText);
                            });
                        });
                    });
                });
            el.find(".k-grid-toolbar").on("click",
                ".k-grid-LoadCS",
                function () {
                    const confirmMsg = parent.data("msg-confirm-client");
                    cpiConfirm.confirm(confirmTitle, confirmMsg, function () {
                        const data = { tmkId: parent.data("tmkid"), fromCountryLaw: false };
                        $.post(parent.data("url-load"), data).done(function () {
                            //grid.dataSource.query({ page: 1, pageSize: pageSize }).fail(function (error) {
                            //    pageHelper.showErrors(error.responseText);
                            //});
                            grid.dataSource.read().fail(function (error) {
                                pageHelper.showErrors(error.responseText);
                            });
                        });
                    });
                });
            el.find(".k-grid-toolbar").on("click",
                ".k-grid-GenerateAll",
                function () {
                    $.get(parent.data("url-select")).done((result) => {
                        const popupContainer = $(".cpiContainerPopup").last();
                        popupContainer.html(result);
                        const dialog = $("#tmkDesCtryGenerateDialog");
                        dialog.modal("show");

                        $(dialog.find("#tmkDesCtrySelectAll")).click(function () {
                            tmkTrademarkPage.markAllDesigCountries(this.checked);
                        });
                        $(dialog.find("#tmkDesCtryGenerate")).click(() => { self.generateApplications(); });
                    }).fail(function (error) {
                        pageHelper.showErrors(error.responseText);
                    });
                });
        }
    }

    getFamilyReferenceParam = () => {
        const form = $($("#" + this.detailContentContainer).find("form")[0]);
        const tmkId = form.find("#TmkId").val();
        const trademarkName = form.find("input[name='TrademarkName']").val();
        return { tmkId: tmkId, trademarkName: trademarkName };
    }

    getDesCountryParam = () => {
        const form = $($("#" + this.detailContentContainer).find("form")[0]);
        const tmkId = form.find("#TmkId").val();
        const country = form.find("input[name='Country']").val();
        const caseType = form.find("input[name='CaseType']").val();
        return { country: country, caseType: caseType, tmkId: tmkId };
    }

    getDesCaseTypeParam = (e) => {
        const form = $($("#" + this.detailContentContainer).find("form")[0]);
        const country = form.find("input[name='Country']").val();
        const caseType = form.find("input[name='CaseType']").val();

        return { country: country, caseType: caseType, desCountry: this.selectedDesCountry };
    }

    getDefaultCaseType = (e) => {
        this.selectedDesCountry = e.dataItem.DesCountry;

        const desCaseType = e.dataItem.DesCaseType;
        const row = $(`#${e.sender.element[0].id}`).closest("tr");
        const grid = $("#tmkDesignatedCountriesGrid").data("kendoGrid");
        const dataItem = grid.dataItem(row);
        dataItem.DesCaseType = desCaseType;
        dataItem.GenSingleClassApp = e.dataItem.GenSingleClassApp == "1";
        $(row).find(".des-case-type").html(kendo.htmlEncode(desCaseType));
    }

    generateTrademark(e, grid) {
        const row = $(e.currentTarget).closest("tr");
        const dataItem = grid.dataItem(row);
        const parent = $("#tmkDesCountryContainer");
        const confirmTitle = parent.data("msg-confirm-title");
        let confirmMsg = parent.data("msg-confirm-gen").replace("{country}", dataItem.DesCountry);
        let buttons = null;

        const checkClassUrl = parent.data("url-check-class");
        $.get(checkClassUrl)
            .done(function (result) {

                if (!result.proceed) {
                    confirmMsg = parent.data("msg-no-class");
                    buttons = {
                        "action": { "class": "btn-primary", "label": parent.data("msg-no-class-yes"), "icon": "fa fa-check" },
                        "close": { "class": "btn-secondary", "label": parent.data("msg-no-class-no"), "icon": "fa fa-undo-alt" }
                    }
                }

                cpiConfirm.confirm(confirmTitle, confirmMsg, function () {
                    $.ajax({
                        url: parent.data("url-generate"),
                        data: { parentTmkId: dataItem.TmkId, desCountries: `|${dataItem.DesCountry}_${dataItem.GenSubCase ? dataItem.GenSubCase : ''}|`, desId: dataItem.DesId },
                        type: "POST",
                        success: function (result) {
                            //const genDate = kendo.parseDate(result.GenDate);
                            //dataItem.GenDate = pageHelper.cpiDateFormatToDisplay(genDate);
                            //let rowHtml = grid.rowTemplate(dataItem);
                            //rowHtml = rowHtml.replace("k-grid-Generate", "k-grid-Generate d-none");
                            //rowHtml = rowHtml.replace("d-none", "");
                            //rowHtml = rowHtml.replace("Detail/0", `Detail/${result.GenTmkId}`);
                            //row.replaceWith(rowHtml);
                            const grid = $("#tmkDesignatedCountriesGrid").data("kendoGrid");
                            grid.dataSource.read();
                        },
                        error: function (error) {
                            if (error.responseJSON !== undefined)
                                pageHelper.showGridErrors(error.responseJSON);
                            else
                                pageHelper.showErrors(error.responseText);
                        }
                    });
                }, buttons);

            })
            .fail(function (error) {
                pageHelper.showErrors(error.responseText);
            });
    }

    markAllDesigCountries(check) {
        $("#tmkDesCountries input").each(function () {
            if (!this.disabled)
                this.checked = check;
        });
    }

    generateApplications() {
        let countries = "|";
        const parent = $("#tmkDesCountries");

        $(parent.find("input")).each(function () {
            if (this.checked && !this.disabled) {
                countries += $(this).attr("name") + "|";
            }
        });
        if (countries.length > 1) {
            const checkClassUrl = parent.data("url-check-class");
            $.get(checkClassUrl)
                .done(function (result) {
                    if (!result.proceed) {
                        const confirmMsg = parent.data("msg-no-class");
                        const buttons = {
                            "action": { "class": "btn-primary", "label": parent.data("msg-no-class-yes"), "icon": "fa fa-check" },
                            "close": { "class": "btn-secondary", "label": parent.data("msg-no-class-no"), "icon": "fa fa-undo-alt" }
                        }
                        const confirmTitle = parent.data("msg-confirm-title");
                        cpiConfirm.confirm(confirmTitle, confirmMsg, function () { generateApps(); }, buttons);
                    }
                    else generateApps();

                    function generateApps() {
                        $.ajax({
                            url: parent.data("url-generate"),
                            data: { parentTmkId: parent.data("tmkid"), desCountries: countries, desId: 0 },
                            type: "POST",
                            success: function () {
                                const grid = $("#tmkDesignatedCountriesGrid").data("kendoGrid");
                                grid.dataSource.read();
                            },
                            error: function (error) {
                                pageHelper.showErrors(error.responseText);
                            }
                        });
                    }
                })


                .fail(function (error) {
                    pageHelper.showErrors(error.responseText);
                });

        }
    }

    //------------------------------------------------------------ COPY
    showCopyScreen() {
        //const popupContainer = $(".cpiContainerPopup").last();
        const popupContainer = $(".site-content .popup").last();
        const dialogContainer = popupContainer.find("#tmkTrademarkCopyDialog");
        const entryForm = $(dialogContainer.find("form")[0]);
        dialogContainer.modal("show");
        const self = this;

        $(dialogContainer.find("#tmkCopyAllCountries")).click(function () {
            self.markCopyCountries(this.checked, "tmkAllCountries");
        });

        $(dialogContainer.find("#tmkCopyButton")).click(function () {

            $.validator.unobtrusive.parse(entryForm);
            if (entryForm.data("validator") !== undefined) {
                entryForm.data("validator").settings.ignore = "";
            }

            if (entryForm.valid()) {
                self.copyTrademarks();
            }
        });
    }

    markCopyCountries(check, container) {
        $("#" + container + " input").each(function () {
            this.checked = check;
        });
    }

    copyTrademarks() {
        // get checked countries
        let countries = "|";
        const countryTab = $("#tmkAllCountries");

        $(countryTab.find("input")).each(function () {
            const ctryCode = $(this).attr("name");
            if (this.checked && ctryCode !== undefined) {
                countries += ctryCode + "|";
            }
        });

        const dialog = $("#tmkTrademarkCopyDialog");
        if (countries.length > 1) {
            const confirmTitle = dialog.data("confirm-title");
            let countriesList = countries.substring(1);
            countriesList = countriesList.substring(0, countriesList.length - 1).replaceAll("|", ",");
            const confirmMsg = dialog.data("confirm-msg").replace("{0}", "<br>" + countriesList + "<br>");

            cpiConfirm.confirm(confirmTitle, confirmMsg, function () {
                const parent = $("#tmkCopyBody");
                const pageId = "tmkTrademarkCopy";

                const param = new Object();
                param.CopyTmkId = parseInt($("#CopyTmkId").val());
                param.CopyCaseNumber = dialog.find("input[name='CopyCaseNumber']").val();
                param.CopySubCase = $("#CopySubCase").val();
                param.CopyCaseInfo = $("#CopyCaseInfo").is(":checked");
                param.CopyRemarks = $("#CopyRemarks").is(":checked");
                param.CopyAssignments = $("#CopyAssignments").is(":checked");
                param.CopyGoods = $("#CopyGoods").is(":checked");
                param.CopyImages = $("#CopyImages").is(":checked");
                param.CopyKeywords = $("#CopyKeywords").is(":checked");
                param.CopyDesCountries = $("#CopyDesCountries").is(":checked");
                param.CopiedCountries = countries;
                param.CopyProducts = $("#CopyProducts").is(":checked");
                param.CopyLicenses = $("#CopyLicenses").is(":checked");

                param.CopyRelationship = dialog.find("input[name='CopyRelationship']:checked").val();

                if (param.CopyCaseInfo || param.CopyRemarks || param.CopyAssignments || param.CopyGoods || param.CopyImages || param.CopyKeywords || param.CopyDesCountries || param.CopyProducts || param.CopyLicenses) {
                    $.ajax({
                        url: parent.data("url-copy"),
                        data: { copy: param },
                        type: "POST",
                        headers: { "RequestVerificationToken": $("[name='__RequestVerificationToken']").val() },
                        success: function (result) {
                            alert(result.Message);
                            dialog.modal("hide");
                        },
                        error: function (error) {
                            pageHelper.showErrors(error.responseText);
                        }
                    });
                }
                else {
                    const noDataMsg = $(dialog).data("no-data-msg");
                    alert(noDataMsg);
                }

            });
        }
        else {
            const noCountryMsg = $(dialog).data("no-country-msg");
            alert(noCountryMsg);
        }
    }

    //------------------------------------------------------------ export to excel
    //initExportToExcel(url) {
    //    const self = this;
    //    $('.k-grid-excel-server').on('click', (e) => {
    //        e.preventDefault();

    //        const data = tmkTrademarkPage.gridMainSearchFilters();
    //        const dataJSON = JSON.stringify(data.mainSearchFilters);
    //        self.serverGenerateDocument(url, "mainSearchFiltersJSON", dataJSON);
    //    });
    //}

    //serverGenerateDocument(url, dataName, dataValue) {
    //    const html = '<form method="POST" action="' + url + '">' +
    //        '<input type="hidden" name="__RequestVerificationToken" value="' + $("[name='__RequestVerificationToken']").val() + '">';
    //    let form = $(html);
    //    form.append($('<input type="hidden" name="' + dataName + '"/>').val(dataValue));
    //    $('body').append(form);
    //    form.submit();
    //}

    //getSearchResultsGridVisibleColumns = () => {
    //    const columns = this.searchResultGrid.data("kendoGrid").columns;
    //    const visibleColumns = [];
    //    $.each(columns, function (index) {
    //        if (!this.hidden) {
    //            visibleColumns.push(this.field);
    //        }
    //    });
    //    console.log(visibleColumns);
    //}

    /* TL */
    tlLoadPublicDataMenu() {
        this.cpiLoadingSpinner.show();
        const self = this;
        const baseUrl = $("body").data("base-url");
        const pdtUrl = `${baseUrl}/Trademark/TLInfo/PublicDataMenu`;

        const param = {
            country: $(`#Country_${this.mainDetailContainer}`).val(),
            tmkId: $(`#${this.mainDetailContainer}`).find("#TmkId").val(),
            activeTab: `${this.activeTLTab ? this.activeTLTab : ''}`
        };
        if (!param.country)
            param.country = $(`#${this.mainDetailContainer}`).find("#Country").val();

        $.post(pdtUrl, param)
            .done(function (html) {
                $("#trademarkPDTTabContent").html(html);
            })
            .fail(function (e) {
                pageHelper.showErrors(e.responseText);
            })
            .always(function () {
                self.cpiLoadingSpinner.hide();
            });
    }

    tlHandleMenu() {
        const tlForm = $("#tlPublicDataInfo");
        tlForm.on("submit", function (e) {
            e.preventDefault();
            e.stopPropagation();

            const params = tlForm.serialize();

            $.post(tlForm.attr("action"), params)
                .done(function (result) {
                    $("#tlPublicDataInfoContainer").html(result);
                })
                .fail(function (e) {
                    pageHelper.showErrors(e.responseText);
                });
        });
        tlForm.find("a").click(function () {
            const choices = tlForm.find("a");
            $.each(choices, function () {
                $(this).removeClass("active");
            });

            const selected = $(this).data("value");
            $(this).addClass("active");

            $(tlForm.find("#tlMenuChoice")[0]).val(selected);
            tlForm.submit();
        });
        tlForm.submit();

        $("#tlPublicDataInfoContainer").on("click", ".to-record-clear", (e) => {
            const element = $(e.currentTarget);
            cpiConfirm.delete(element.attr("title"), element.data("message"), () => {
                const baseUrl = $("body").data("base-url");
                const url = `${baseUrl}/Trademark/TLInfo/ClearPTOData`;
                $.post(url, this.tlGetIdParams())
                    .done(function (result) {
                        $("#tlPublicDataInfoContainer").html(result);
                    })
                    .fail(function (e) {
                        pageHelper.showErrors(e.responseText);
                    });
            });
        });
    }

    tlActionDisplaySelect(e, screen) {
        if (e.dataItem) {
            const value = e.dataItem.Value;

            const downloadedGrid = $(`#tlActionAsDownloadedGrid_${screen}`);
            const matchedGrid = $(`#tlActionAsMatchedGrid_${screen}`);

            let grid;
            if (value === "1") {
                matchedGrid.show();
                grid = matchedGrid.data("kendoGrid");
                downloadedGrid.hide();
            } else {
                downloadedGrid.show();
                grid = downloadedGrid.data("kendoGrid");
                matchedGrid.hide();

            }
            grid.dataSource.read();
        }
    }

    tlGetIdParams() {
        const form = $("#tlPublicDataInfo");
        return {
            TmkId: form[0].TmkId.value,
            TLTmkId: form[0].TLTmkId.value
        };
    }

    //tlGetActionUpdHistory_RevertType() {
    //    const revertType = $("#RevertType_tlActionUpdHistory").val();
    //    return revertType;
    //}

    //tlGetActionUpdHistory_ChangeDate() {
    //    const jobId = $("#JobId_tlActionUpdHistory").val();
    //    return (jobId == null || jobId === "") ? 0 : jobId;
    //}

    //tlActionUpdHistory_RevertTypeChange = () => {
    //    this.tlActionUpdHistoryGridRead();
    //}

    //tlGetActionUpdHistory_ChangeDateChange = () => {
    //    this.tlActionUpdHistoryGridRead();
    //}

    //tlActionUpdHistoryGridRead() {
    //    const grid = $("#tlActionUpdHistoryGrid").data("kendoGrid");
    //    grid.dataSource.read();

    //    const undoButton = $("#tlActionUpdHistory_undo");
    //    if (undoButton) {

    //        if (this.tlGetActionUpdHistory_RevertType() === "0" &&
    //            this.tlGetActionUpdHistory_ChangeDate() > 0) {
    //            undoButton.removeClass("d-none");
    //        } else {
    //            undoButton.addClass("d-none");
    //        }
    //    }
    //}

    //tlActionUpdHistorySetBtns() {
    //    $("#tlActionUpdHistory_undo").click(() => {
    //        const data = {
    //            plAppId: $("#tlActionUpdHistory_plAppId").val(),
    //            RevertType: this.tlGetActionUpdHistory_RevertType,
    //            jobId: this.tlGetActionUpdHistory_ChangeDate
    //        };
    //        $.post($(this).data("url"), data)
    //            .done(() => {
    //                this.tlActionUpdHistoryGridRead();
    //            })
    //            .fail(function (error) { pageHelper.showErrors(error.responseText); });
    //    });
    //}

    //tlActionUpdHistoryFilter = () => {
    //    return {
    //        revertType: this.tlGetActionUpdHistory_RevertType(),
    //        jobId: this.tlGetActionUpdHistory_ChangeDate()
    //    };
    //}

    /* related patent */
    onChange_RelatedPatent = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");

            const country = e.dataItem["Country"];
            const subCase = e.dataItem["SubCase"];
            const caseType = e.dataItem["CaseType"];
            const title = e.dataItem["Title"];

            const grid = $("#relatedPatentsGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            dataItem.AppId = e.dataItem["AppId"];
            dataItem.Country = country;
            dataItem.SubCase = subCase;
            dataItem.CaseType = caseType;
            dataItem.Title = title;

            $(row).find(".country-field").html(kendo.htmlEncode(country));
            $(row).find(".subCase-field").html(kendo.htmlEncode(subCase));
            $(row).find(".caseType-field").html(kendo.htmlEncode(caseType));
            $(row).find(".title-field").html(kendo.htmlEncode(title));

        }
    }

    /* licensee */
    licenseeRefreshIndicator = (e) => {
        const data = e.sender.dataSource.data();
        if (data.length == 0)
            $("#trademarkDetailLicenseesTab").removeClass("has-licensees");
        else
            $("#trademarkDetailLicenseesTab").addClass("has-licensees");
    }

    /* products */
    productRefreshIndicator = (e) => {
        const data = e.sender.dataSource.data();
        if (data.length == 0)
            $("#trademarkProductsTab").removeClass("has-products");
        else
            $("#trademarkProductsTab").addClass("has-products");
    }

    /* assignments */
    //assignmentsEdit(e) {
    //    pageHelper.addMaxLength(e.container);
    //}

    assignmentsGrid_onDirty = () => {
        const container = $("#trademarkDetailAssignmentsTabContent");
        container.find(".k-grid-Copy").hide();
    }

    assignmentsGrid_onSaveCancel = () => {
        const container = $("#trademarkDetailAssignmentsTabContent");
        container.find(".k-grid-Copy").show();
    }

    handleAssignmentMenu = () => {
        const assignmentsGrid = $("#trademarkDetailAssignmentsTabContent").find(`#assignmentsGrid_${this.mainDetailContainer}`);
        const baseUrl = $("body").data("base-url");

        assignmentsGrid.on("click", ".k-grid-Copy", () => {
            const url = `${baseUrl}/Trademark/TmkAssignment/GetAssignmentCopyScreen`;

            $.get(url, { appId: this.currentRecordId }).done((result) => {
                $(".cpiContainerPopup").empty();
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialog = $("#assignmentCopyDialog");
                dialog.modal("show");
                dialog.floatLabels();

                dialog.find("#copy").on("click", () => {
                    const grid = dialog.find(".kendo-Grid").data("kendoGrid");

                    const copyUrl = `${baseUrl}/Trademark/TmkAssignment/CopyAssignments`;
                    const data = {
                        parentId: this.currentRecordId,
                        from: grid.selectedKeyNames(),
                    };
                    $.post(copyUrl, data)
                        .done(function (response) {
                            assignmentsGrid.data("kendoGrid").dataSource.read();
                            dialog.modal("hide");
                            pageHelper.showSuccess(response.success);
                        })
                        .fail(function (error) { pageHelper.showErrors(error.responseText); });
                });

            }).fail(function (error) {
                pageHelper.showErrors(error.responseText);
            });
        });
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
                const atty4 = this.getKendoComboBox("Attorney4ID");
                const atty5 = this.getKendoComboBox("Attorney5ID");
                const self = this;

                if ((atty1.value() == '' && dataItem.Attorney1ID != null) ||
                    (self.previousClient != null && atty1.value() == self.previousClient.Attorney1ID)) {
                    atty1.value(dataItem.Attorney1ID);
                    atty1.text(dataItem.Attorney1Code);
                    $(`#${atty1.element[0].id}`).closest(".float-label").removeClass("inactive").addClass("active");
                }

                if ((atty2.value() == '' && dataItem.Attorney2ID !== null) ||
                    (self.previousClient != null && atty2.value() == self.previousClient.Attorney2ID)) {
                    atty2.value(dataItem.Attorney2ID);
                    atty2.text(dataItem.Attorney2Code);
                    $(`#${atty2.element[0].id}`).closest(".float-label").removeClass("inactive").addClass("active");
                }

                if ((atty3.value() == '' && dataItem.Attorney3ID !== null) ||
                    (self.previousClient != null && atty3.value() == self.previousClient.Attorney3ID)) {
                    atty3.value(dataItem.Attorney3ID);
                    atty3.text(dataItem.Attorney3Code);
                    $(`#${atty3.element[0].id}`).closest(".float-label").removeClass("inactive").addClass("active");
                }

                if ((atty4.value() == '' && dataItem.Attorney4ID !== null) ||
                    (self.previousClient != null && atty4.value() == self.previousClient.Attorney4ID)) {
                    atty4.value(dataItem.Attorney4ID);
                    atty4.text(dataItem.Attorney4Code);
                    $(`#${atty4.element[0].id}`).closest(".float-label").removeClass("inactive").addClass("active");
                }
                if ((atty5.value() == '' && dataItem.Attorney5ID !== null) ||
                    (self.previousClient != null && atty5.value() == self.previousClient.Attorney5ID)) {
                    atty5.value(dataItem.Attorney5ID);
                    atty5.text(dataItem.Attorney5Code);
                    $(`#${atty5.element[0].id}`).closest(".float-label").removeClass("inactive").addClass("active");
                }
                self.previousClient = dataItem;
            }
        }
    }

    mainCopyInitialize = () => {
        const baseUrl = $("body").data("base-url");
        const mainUrl = `${baseUrl}/Trademark/TmkTrademark/`;

        $(document).ready(() => {
            const container = $("#tmkTrademarkCopyDialog");
            container.find(".case-info-settings").hide();

            container.on("click", ".case-info-set", () => {
                container.find(".case-info-settings").show();
                container.find(".data-to-copy").hide();

                const url = `${mainUrl}GetCopySettings`;
                $.get(url)
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
            container.on("change", "input[type=radio][name=CopyRelationship]", function () {
                const showCaseInfo = this.value === "S";
                triggerCaseInfoSetting(showCaseInfo);
            });
            triggerCaseInfoSetting(true);

            function triggerCaseInfoSetting(showCaseInfo) {
                if (showCaseInfo) {
                    container.find(".case-info-set").show();
                }
                else {
                    container.find(".case-info-set").hide();
                }
                container.find("#CopyCaseInfo").attr("checked", showCaseInfo);
                container.find("#CopyCaseInfo").attr("disabled", !showCaseInfo);
            }

        });
    }

    searchResultDataBound = (e) => {
        const data = e.sender.dataSource.data();

        if (data.length > 0) {
            const listView = e.sender.element;
            $.each(listView.find(".tmkNameSearchResult-collapsible"), function () {
                $(this).textOverflow();
            });

            const images = listView.find("img");
            images.attr("data-src-retry", 3);

            images.on("error", function () {
                pageHelper.imageLoadRetry(this);
            });

            iManage.getDefaultGridImage(this);
            docViewer.getDefaultGridImage(this);
        }
    }

    onChange_Product = (e) => {
        if (e.sender) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");
            const grid = $("#productsGrid_tmkTrademarkDetail").data("kendoGrid");
            const dataItem = grid.dataItem(row);

            var comboDataItem = e.sender.dataItem();
            dataItem.ProductId = comboDataItem["ProductId"];
            dataItem.ProductName = comboDataItem["ProductName"];

        }
    }

    /* related trademark */
    onChange_RelatedTrademark = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");

            const country = e.dataItem["Country"];
            const subCase = e.dataItem["SubCase"];
            const caseType = e.dataItem["CaseType"];
            const trademarkStatus = e.dataItem["TrademarkStatus"];
            const trademarkName = e.dataItem["TrademarkName"];

            const grid = $("#tmkRelatedTrademarksGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            dataItem.RelatedTmkId = e.dataItem["TmkId"];
            dataItem.Country = country;
            dataItem.SubCase = subCase;
            dataItem.CaseType = caseType;
            dataItem.TrademarkStatus = trademarkStatus;
            dataItem.TrademarkName = trademarkName;

            $(row).find(".country-field").html(kendo.htmlEncode(country));
            $(row).find(".subCase-field").html(kendo.htmlEncode(subCase));
            $(row).find(".caseType-field").html(kendo.htmlEncode(caseType));
            $(row).find(".status-field").html(kendo.htmlEncode(trademarkStatus));
            $(row).find(".trademarkName-field").html(kendo.htmlEncode(trademarkName));

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

    searchResultGridRequestEndHandler = (e) => {
        this.searchResultGridRequestEnd(e);
        $(document).ready(() => {
            this.searchResultGrid.find(".tickler-web-link").on("click", this.webLinksOnClick);
        });

    }

    webLinksOnClick = (e) => {
        e.preventDefault();

        const el = $(e.target);
        const url = el.data("url");

        cpiLoadingSpinner.show();
        $.post(url)
            .done((result) => {
                cpiLoadingSpinner.hide();
                window.open(result.url, '_blank');
            })
            .fail((error) => {
                cpiLoadingSpinner.hide();
                cpiAlert.warning(pageHelper.getErrorMessage(error));
            });
    }
}