import Image from "../image";
import TradeSecret from "../tradeSecret";
import ActivePage from "../activePage";

export default class CountryAppPage extends ActivePage {
    constructor() {
        super();
        this.computeTaxStartExpiration = false;
        this.computeTerminalDisclaimer = false;
        this.checkTerminalDisclaimerChild = false;
        this.caseNumberDetailValueMapperUrl = "";
        this.caseNumberSearchValueMapperUrl = "";
        this.idsMenuChoice = null;
        this.image = new Image();
        this.showOutstandingActionsOnly = true;
        this.showOutstandingAnnuitiesOnly = false;
        this.hasTerminalDisclaimer = false;

        this.idsLib = window.idsLib;
        this.rtsLib = window.rtsLib;
        this.activeRtsTab = null;
        this.idsRefsSelected = [];
        this.docServerOperation = true;
        this.selectedDesCountry = "";
        this.isSharePointIntegrationOn = false;

        this.tradeSecret = new TradeSecret();
    }

    initializeDetailContentPage(detailContentPage) {
        super.initializeDetailContentPage(detailContentPage);
        this.tradeSecret.monitor(detailContentPage, $(`#${detailContentPage.activePage.detailContentContainer}`).find(".cpiMainEntry .ts-cleared"));
    }

    initialize = (screen, id, rtsTabUrl, addMode, hasInpadocSearch, hasTerminalDisclaimer, isSharePointIntegrationOn) => {
        this.docServerOperation = !isSharePointIntegrationOn;
        this.editableGrids = [
            {
                name: `assignmentsGrid_${screen}`, filter: { parentId: id },
                afterSubmit: this.updateRecordStamps,
                onDirty: this.assignmentsGrid_onDirty,
                onSave: this.assignmentsGrid_onSaveCancel,
                onCancel: this.assignmentsGrid_onSaveCancel,
            },
            { name: `actionsGrid_${screen}`, filter: { parentId: id }, afterSubmit: this.updateRecordStamps },
            { name: `licenseesGrid_${screen}`, filter: { parentId: id }, afterSubmit: this.updateRecordStamps },
            { name: `inventorsGrid_${screen}`, filter: { parentId: id }, afterSubmit: this.appInventorsGrid_AfterSubmit },
            { name: "appDesignatedCountriesGrid", filter: { parentId: id }, afterSubmit: this.updateRecordStamps },
            { name: "appRelatedCasesGrid", filter: { parentId: id }, afterSubmit: this.appRelatedCasesGrid_AfterSubmit },
            { name: `ownersGrid_${screen}`, filter: { parentId: id }, afterSubmit: this.updateRecordStamps },
            { name: `productsGrid_${screen}`, filter: { parentId: id }, afterSubmit: this.appProductsGrid_AfterSubmit },
            { name: "subjectMattersGrid", filter: { parentId: id }, afterSubmit: this.updateRecordStamps },
            { name: "relatedTrademarksGrid", filter: { parentId: id }, afterSubmit: this.updateRecordStamps },
            {
                name: "terminalDisclaimersGrid", filter: { parentId: id }, afterSubmit: () => {
                    this.showTerminalDisclaimerFromMultipleConfirm(id);
                    this.updateRecordStamps();
                }
            },
            { name: `relatedMattersGrid_${screen}`, filter: { parentId: id }, afterSubmit: this.updateRecordStamps }
        ];
        this.rtsTabUrl = rtsTabUrl;
        this.tabsLoaded = [];
        this.hasInpadocSearch = hasInpadocSearch;
        this.hasTerminalDisclaimer = hasTerminalDisclaimer;
        this.tabChangeSetListener();
        this.desCountriesGrid_toggleButtons();


        //window.familyTreePage.initFamilyTree();
        this.emphasizeInactiveStatus(screen);

        $(document).ready(() => {
            const self = this;
            let selectedUploadRow = null;
            let selectedUploadGrid = null;

            if (addMode) {
                const caseNumber = this.getKendoComboBox("CaseNumber");
                caseNumber.input.focus();
            }

            const ownersGrid = $(`#ownersGrid_${screen}`);
            ownersGrid.on("click", ".ownerLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = ownersGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.OwnerID);
                pageHelper.openLink(linkUrl, false);
            });

            const inventorsGrid = $(`#inventorsGrid_${screen}`);
            inventorsGrid.on("click", ".inventorLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = inventorsGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.InventorID);
                pageHelper.openLink(linkUrl, false);
            });

            //for assignments, licensees
            const uploadForm = $("#ctryAppFileUpload");
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

            assignmentsGrid.on("click", ".delete-file-link", function (e) {
                e.preventDefault();

                const uploadLink = $(e.currentTarget);
                const childId = uploadLink.data("id");

                const title = assignmentsGrid.parent().data("upload-title");
                const msg = assignmentsGrid.parent().data("delete-file-msg");
                cpiConfirm.confirm(title, msg, function () {
                    const baseUrl = $("body").data("base-url");
                    const url = `${baseUrl}/Patent/PatAssignment/AssignmentFileDelete`;

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

            assignmentsGrid.on("click", ".upload-link", function (e) {
                const baseUrl = $("body").data("base-url");
                const url = `${baseUrl}/Patent/PatAssignment/AssignmentFileUpload`;
                selectedUploadRow = $(e.target).closest("tr");
                selectedUploadGrid = assignmentsGrid;
                self.fileUpload(uploadForm, url, assignmentsGrid, e);
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
                    const url = `${baseUrl}/Patent/PatLicensee/LicenseeFileDelete`;

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
                const url = `${baseUrl}/Patent/PatLicensee/LicenseeFileUpload`;
                selectedUploadRow = $(e.target).closest("tr");
                selectedUploadGrid = licenseesGrid;
                self.fileUpload(uploadForm, url, licenseesGrid, e);
            });

            const relatedCasesGrid = $(`#appRelatedCasesGrid`);
            relatedCasesGrid.on("click", ".countryLink", (e) => {
                e.stopPropagation();

                const row = $(e.target).closest("tr");
                const dataItem = relatedCasesGrid.data("kendoGrid").dataItem(row);

                let url;
                let linkUrl;
                if (dataItem.RelatedAppId !== null) {
                    url = $(e.target).data("url");
                    linkUrl = url.replace("actualValue", dataItem.RelatedAppId);
                    window.open(linkUrl, '_blank');
                }
                else {
                    url = $(e.target).data("country-url");
                    linkUrl = url.replace("actualValue", dataItem.RelatedCountry);
                    pageHelper.openLink(linkUrl, false);
                }
            });

            const terminalDisclaimersGrid = $("#terminalDisclaimersGrid");
            terminalDisclaimersGrid.on("click", ".countryLink", (e) => {
                e.stopPropagation();

                const row = $(e.target).closest("tr");
                const dataItem = terminalDisclaimersGrid.data("kendoGrid").dataItem(row);

                let url;
                let linkUrl;
                if (dataItem.TerminalDisclaimerAppId > 0) {
                    url = $(e.target).data("url");
                    linkUrl = url.replace("actualValue", dataItem.TerminalDisclaimerAppId);
                    window.open(linkUrl, '_blank');
                }
            });

            terminalDisclaimersGrid.on("click", ".k-grid-Add", (e) => {
                e.stopPropagation();

                const baseUrl = $("body").data("base-url");
                const url = `${baseUrl}/Patent/PatTerminalDisclaimer/TerminalDisclaimerAddScreen`;
                const appId = terminalDisclaimersGrid.parent().data("parent-id");
                const self = this;

                $.get(url, { appId }).done((result) => {
                    if (result) {
                        $(".cpiContainerPopup").empty();
                        const popupContainer = $(".cpiContainerPopup").last();
                        popupContainer.html(result);
                        const dialog = $("#terminalDisclaimerAddDialog");
                        dialog.modal("show");
                        dialog.floatLabels();

                        let entryForm = dialog.find("form")[0];
                        entryForm = $(entryForm);

                        entryForm.find(".search-submit").on("click", ()=> {
                            const grid = $("#terminalDisclaimerSelectionGrid").data("kendoGrid");
                            grid._selectedIds = {};
                            grid.clearSelection();
                            grid.dataSource.read();
                        });

                        dialog.find("#save").on("click", function () {
                            const grid = $("#terminalDisclaimerSelectionGrid").data("kendoGrid");
                            const selectedKeys = grid.selectedKeyNames();
                            const saveUrl = $(this).data("url");
                            const saveData = {
                                parentId: appId,
                                from: selectedKeys,
                            };

                            $.post(saveUrl, saveData)
                                .done(function (result) {
                                    dialog.modal("hide");
                                    pageHelper.showSuccess(result.success);
                                    const parentGrid = $('#terminalDisclaimersGrid').data("kendoGrid");
                                    parentGrid.dataSource.read();
                                    self.showTerminalDisclaimerFromMultipleConfirm(id);
                                    self.updateRecordStamps();
                                })
                                .fail(function (error) { pageHelper.showErrors(error.responseText); });
                        });
                    }


                }).fail(function (error) {
                    pageHelper.showErrors(error.responseText);
                });

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
                    patActionDuePage.mainSearchRecordIds = this.actionActIds;
                }
                const link = $(e.target);
                pageHelper.openDetailsLink(link);
            });

            let parent = $(`#${this.mainDetailContainer}`).find("form")[0];
            parent = $(parent);
            parent.find(".cpiTerminalDisclaimerLink").on('click', function (e) {
                $.get(parent.data("terminal-disclaimer-url")).done(function (result) {
                    const popupContainer = $(".site-content .popup");
                    popupContainer.empty();
                    popupContainer.html(result);
                    var dialog = $("#terminalDisclaimerTreeDialog");
                    dialog.modal("show");
                }).fail(function (error) {
                    pageHelper.showErrors(error.responseText);
                });
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

    getCaseNumberSearchValueMapper = () => {
        if (this.caseNumberSearchValueMapperUrl === "") {
            this.caseNumberSearchValueMapperUrl = $("#applicationSearchMainTabContent").data("case-number-mapper-url");
            if (this.caseNumberSearchValueMapperUrl === undefined || this.caseNumberSearchValueMapperUrl === "")
                this.caseNumberSearchValueMapperUrl = $("#docMgtMainTabContent").data("case-number-mapper-url");
        }
        return this.caseNumberSearchValueMapperUrl;
    }

    terminalDisclaimerAddParam = () => {
        let form = $('#terminalDisclaimerAddDialog').find("form")[0];
        form = $(form);
       
        const param = {
            caseNumber: form.find("input[name = 'CaseNumber']").data("kendoComboBox").text(),
            country: form.find("input[name = 'Country']").data("kendoComboBox").text(),
            subCase: form.find("input[name = 'SubCase']").data("kendoComboBox").text(),
            appNumber: form.find("input[name = 'AppNumber']").data("kendoComboBox").text(),
            patNumber: form.find("input[name = 'PatNumber']").data("kendoComboBox").text(),
            applicationStatus: form.find("input[name = 'ApplicationStatus']").data("kendoComboBox").text(),
            title: form.find("input[name = 'AppTitle']").data("kendoComboBox").text(),
            expireDateFrom: form.find("input[name = 'ExpireDateFrom']").data("kendoDatePicker").value(),
            expireDateTo: form.find("input[name = 'ExpireDateTo']").data("kendoDatePicker").value(),
            parentId: form.find("#ParentId").val()
        };
        return param;

    }

    terminalDisclaimerAddGridDataBound = (e) => {
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
            $.each(listView.find(".appTitleSearchResult-collapsible"), function () {
                $(this).textOverflow();
            });
        }
    }
    terminalDisclaimerAddGridSelectionChange = () => {
        const addButton = $('#terminalDisclaimerAddDialog').find("#save");

        const grid = $("#terminalDisclaimerSelectionGrid").data("kendoGrid");
        
        if (grid.selectedKeyNames().length > 0)
            addButton.removeAttr("disabled");
        else
            addButton.attr("disabled", "disabled");
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

    desCountriesGrid_toggleButtons = () => {
        $(document).ready(function () {
            const grid = $("#appDesignatedCountriesGrid");
            if (grid.length > 0) {
                const toolbar = grid.find(".k-grid-toolbar");
                toolbar.on("click", ".k-grid-Save, .k-grid-Cancel", function () {
                    toolbar.find(".k-grid-LoadCL,.k-grid-LoadCS,.k-grid-GenerateAll").removeClass("d-none");
                });
                toolbar.on("click", ".k-grid-add", function () {
                    toolbar.find(".k-grid-LoadCL,.k-grid-LoadCS,.k-grid-GenerateAll").addClass("d-none");
                });
            }
        });
    }

    tabChangeSetListener = () => {
        $("#countryAppTab a").on("click", (e) => {
            e.preventDefault();
            const tab = e.target.id;
            if (this.tabsLoaded.indexOf(tab) === -1) {
                this.tabsLoaded.push(tab);
                this.loadTabContent(tab);
            }
            if (tab === "countryAppMainInfoTab")
                $(".ca-main-info").addClass("d-none");
            else
                $(".ca-main-info").removeClass("d-none");

        });

        if (this.hasInpadocSearch)
            this.rtsInpadocAppSetListener();

        this.taxStartExpSetListener();
    }

    taxStartExpSetListener() {
        let form = $("#countryApplicationDetailsView-Content").find("form")[0];
        form = $(form);

        let triggers = ["FilDate", "IssDate", "PubDate", "ExpDate", "ParentFilDate", "ParentIssDate", "PCTDate", "TaxStartDate", "PatentTermAdj", "Country", "CaseType"];
        let dateFields = "";
        triggers.forEach(function (item) {
            const field = `,input[name="${item}"]`;
            dateFields += field;
        });

        triggers = ["PatentTermAdj", "Country", "CaseType"];
        let nonDateFields = "";
        triggers.forEach(function (item) {
            const field = `,input[name="${item}"]`;
            nonDateFields += field;
        });

        const self = this;
        $(document).ready(function () {
            form.find(dateFields.substr(1)).each(function () {
                const datePickerName = this.name;
                const datePicker = $(this).data("kendoDatePicker");
                if (datePicker) {
                    datePicker.bind("change", function () {
                        self.computeTaxStartExpiration = true;

                        if (datePickerName === "ExpDate") {
                            self.checkTerminalDisclaimerChild = true;
                        }
                    });
                }


            });
            form.find(nonDateFields.substr(1)).each(function () {
                $(this).bind("change", function () {
                    self.computeTaxStartExpiration = true;
                });
            });
            form.find("input[name='TerminalDisclaimerAppId']").each(function () {
                $(this).bind("change", function () {
                    self.computeTerminalDisclaimer = true;
                });
            });


        });

    }

    deleteDueDate = (e, grid) => {
        this.deleteGridRow(e, grid);
    }

    deleteTerminalDisclaimerRow(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const self = this;
        pageHelper.deleteGridRow(e, dataItem, function () { self.showTerminalDisclaimerFromMultipleConfirm(dataItem.AppId); self.updateRecordStamps(); });
    }

    delegateDueDate = (e, grid) => {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        e.preventDefault();
        //const parent = $("#" + e.delegateTarget.id).parent();
        //const url = parent.data("url-generate");

        //$.ajax({
        //    url: url,
        //    data: getDataItemValues(dataItem),
        //    type: "Get",
        //    success: function (result) {
        //        const popupContainer = $(".cpiContainerPopup").last();
        //        popupContainer.html(result);
        //    },
        //    error: function (e) {
        //        showErrors(e);
        //    }
        //});
    }

    recordNavigateHandler = (id) => {
        this.showCADetails(id);
    }

    showCADetails = (id) => {
        if (isNaN(id))
            id = id.id;

        //get active Rts tab before loading the next record
        const container = $(`#${this.detailContentContainer}`);
        this.activeRtsTab = container.find("#rtsMenuChoice").val();
        pageHelper.showDetails(this, id);

    };

    loadTabContent(tab) {
        switch (tab) {
            case "countryAppAssignmentsTab":
                $(document).ready(() => {
                    const assignmentsGrid = $("#countryAppAssignmentsTabContent").find(`#assignmentsGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    assignmentsGrid.dataSource.read();

                    this.handleAssignmentMenu();
                });
                break;

            case "countryAppActionsTab":
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

            case "countryAppCostsTab":
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

            case "countryAppLicensesTab":
                $(document).ready(() => {
                    const licenseesGrid = $(`#${this.mainDetailContainer}`).find(`#licenseesGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    licenseesGrid.dataSource.read();
                });
                break;

            case "countryAppEntitiesTab":
                $(document).ready(() => {
                    const appOwnersGrid = $(`#ownersGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    if (appOwnersGrid) {
                        appOwnersGrid.dataSource.read().then(() => {
                            const form = $($(`#${this.detailContentContainer}`).find("form")[0]);
                            const appId = form.find("#AppId").val();

                            const appInventorsGrid = $(`#inventorsGrid_${this.mainDetailContainer}`).data("kendoGrid");
                            appInventorsGrid.dataSource.read();

                            $(`#inventorsGrid_${this.mainDetailContainer}`).find(".k-grid-toolbar").on("click",
                                ".ShowPaymentDateUpdateScreen",
                                () => {
                                    const url = $(`#inventorsGrid_${this.mainDetailContainer}`).parent().data("url-mass-update");
                                    const data = {
                                        appId: appId,
                                    };

                                    this.openAwardMassUpdateEntry(appInventorsGrid, url, data, true);
                                });
                        });
                    }
                });
                break;

            case "countryAppTerminalDisclaimerTab":
                $(document).ready(() => {
                    const terminalDisclaimersGrid = $(`#${this.mainDetailContainer}`).find("#terminalDisclaimersGrid").data("kendoGrid");
                    terminalDisclaimersGrid.dataSource.read();
                });
                break;

            case "countryAppDesignationTab":
                $(document).ready(() => {
                    this.setDesCountrySettings();
                });
                break;

            case "countryAppDocumentsTab":
                $(document).ready(() => {
                    this.image.initializeImage(this, this.docServerOperation);
                });
                break;

            case "countryAppWebLinksTab":
                $(document).ready(function () {
                    const webLinksGrid = $("#countryAppWebLinksTabContent .webLinksGrid").data("kendoGrid");
                    if (webLinksGrid) {
                        webLinksGrid.dataSource.read();
                    }
                });
                break;

            case "countryAppPDTTab":
                $(document).ready(() => {
                    this.rtsLoadPublicDataMenu();
                });
                break;

            case "countryAppIDSTab":
                $(document).ready(() => {
                    this.handleIDSMenu();
                    this.idsCountRefresh();
                });
                break;

            case "countryAppAMSTab":
                $(document).ready(() => {
                    const grid = $(`#annuitiesGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    $(".grid-options #showOutstandingAnnuitiesOnly").prop('checked', this.showOutstandingAnnuitiesOnly);
                    if (parseInt(grid.options.dataSource.pageSize) > 0)
                        grid.dataSource.pageSize(grid.options.dataSource.pageSize);
                    else
                        grid.dataSource.read();
                });
                break;

            case "countryAppRelatedCasesTab":
                $(document).ready(function () {
                    const appRelatedCasesGrid = $("#appRelatedCasesGrid").data("kendoGrid");
                    appRelatedCasesGrid.dataSource.read();
                });
                break;

            case "countryAppRelatedMatterTab":
                $(document).ready(() => {
                    const relatedMattersGrid = $(`#relatedMattersGrid_${this.mainDetailContainer}`);
                    const grid = relatedMattersGrid.data("kendoGrid");
                    grid.dataSource.read();
                    pageHelper.addBreadCrumbsRefreshHandler(relatedMattersGrid, () => { grid.dataSource.read() });
                });
                break;

            case "countryAppRelatedTrademarkTab":
                $(document).ready(function () {
                    const relatedTrademarksGrid = $("#relatedTrademarksGrid");
                    const grid = relatedTrademarksGrid.data("kendoGrid");
                    grid.dataSource.read();
                    pageHelper.addBreadCrumbsRefreshHandler(relatedTrademarksGrid, () => { grid.dataSource.read() });
                });
                break;

            case "countryAppProductsTab":
                $(document).ready(() => {
                    const productsGrid = $(`#${this.mainDetailContainer}`).find(`#productsGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    if (productsGrid)
                        productsGrid.dataSource.read();
                });
                break;

            case "countryAppSubjectMattersTab":
                $(document).ready(function () {
                    const subjectMattersGrid = $("#subjectMattersGrid").data("kendoGrid");
                    subjectMattersGrid.dataSource.read();
                });
                break;

            case "countryAppCorrespondenceTab":
                $(document).ready(() => {
                    const docsOutGrid = $(`#docsOutGrid_${this.mainDetailContainer}`).data("kendoGrid");
                    docsOutGrid.dataSource.read();
                });
                break;

            case "":
                break;
        }
    }

    //override of activePage
    afterInsert(result, options) {
        if (this.recordNavigator && this.recordNavigator.length > 0) {
            let recId = result;
            if (isNaN(result))
                recId = result.id;

            this.recordNavigator.addRecordId(recId);
        }
        return this.showDetails(result);
    }

    //called also after save (update and insert)
    showDetails(result) {
        const self = this;

        let id = result;
        if (isNaN(result))
            id = result.id;

        //const oldTerminalDiscAppid = $(`#${self.mainDetailContainer}`).find("#OldTerminalDisclaimerAppId").val();
        //const terminalDiscAppid = $(`#${this.mainDetailContainer}`).find("input[name='TerminalDisclaimerAppId']").val();
        //if (!self.computeTaxStartExpiration) {
        //    //if value is removed, recompute expirate date based on CL
        //    if (!terminalDiscAppid && oldTerminalDiscAppid) {
        //        self.computeTaxStartExpiration = true
        //    }
        //}

        pageHelper.showDetails(this, id, function () {
            if (result.checkTD) {
                self.checkTerminalDisclaimerChild = true;
            }
            //self.handleSaveWorkflow(self, oldTerminalDiscAppid, terminalDiscAppid, result);
            self.handleSaveWorkflow(self, true, null, result);
        });
    }

    handleSaveWorkflow = (self, oldTerminalDiscAppid, terminalDiscAppid, result) => {
        let taxDeferred = $.Deferred();
        const id = result.id;

        //1. tax info
        if (self.computeTaxStartExpiration) {
            self.computeTaxStartExpiration = false;
            taxDeferred = self.showTaxInfoConfirm(id, taxDeferred);
        }
        else {
            taxDeferred.resolve(false);
        }

        taxDeferred.then(function (taxResult) {
            //2. expiration cascade to child (terminal disclaimer)
            let terminalDiscChildDeferred = $.Deferred();

            //with new expiration date or TD source record has significant changes
            if ((taxResult || self.checkTerminalDisclaimerChild) && self.hasTerminalDisclaimer) {
                terminalDiscChildDeferred = self.checkTerminalDisclaimerChildConfirm(id, terminalDiscChildDeferred);
            }
            else {
                terminalDiscChildDeferred.resolve(false);
            }

            terminalDiscChildDeferred.then(function () {
                //3. terminal disclaimer modified
                let terminalDisclaimerDeferred = $.Deferred();

                if (self.computeTerminalDisclaimer && terminalDiscAppid) {
                    self.computeTerminalDisclaimer = false;
                    terminalDisclaimerDeferred = self.showTerminalDisclaimerConfirm(id, oldTerminalDiscAppid, terminalDisclaimerDeferred);
                }
                else {
                    terminalDisclaimerDeferred.resolve(false);
                }

                terminalDisclaimerDeferred.then(function () {
                    //4. multiple based on
                    let multipleBasedOnDeferred = $.Deferred();

                    if (result.multipleBasedOnSessionKey > '') {
                        multipleBasedOnDeferred = self.showMultipleBasedConfirm(multipleBasedOnDeferred, result.multipleBasedOnSessionKey);
                    }
                    else {
                        multipleBasedOnDeferred.resolve(false);
                    }

                    multipleBasedOnDeferred.then(function () {
                        //5. patent workflow
                        pageHelper.handleEmailWorkflow(result);
                    });
                });

            });
        });
    };

    emphasizeInactiveStatus = (screen) => {
        $(document).ready(function () {
            const form = $(`#${screen}`).find("form");
            const activeSwitch = form.find("#IsActive");
            const appId = form.find("#AppId");

            if (activeSwitch.val() === "False" && appId.val() > 0) {
                form.find("input[name='ApplicationStatus_input']").attr("style", "color: #dc3545 !important");
            }
        });
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

    showTaxInfoConfirm(id, deferred) {
        let mainForm = $(`#${this.mainDetailContainer}`).find("form")[0];
        mainForm = $(mainForm);
        const url = mainForm.data("tax-confirm");
        const self = this;

        $.get(url, { appId: mainForm.find("#AppId").val() })
            .done(function (html, status, jqXhr) {

                //204 = nocontent
                if (jqXhr.status !== 204) {
                    $(".cpiContainerPopup").empty();
                    const popupContainer = $(".cpiContainerPopup").last();
                    popupContainer.html(html);

                    const dialog = $("#patTaxInfoDialog");
                    dialog.modal("show");

                    dialog.find(".save-start, .save-start-close, .save-exp,.save-exp-close,.save-all").on("click",
                        function () {
                            let form = dialog.find("form")[0];
                            form = $(form);
                            const updateType = $(form.find("input[name='UpdateType']"));
                            if (this.classList.contains("save-all")) {
                                updateType.val(0);
                            }
                            else if (this.classList.contains("save-exp") || this.classList.contains("save-exp-close")) {
                                updateType.val(1);
                            } else {
                                updateType.val(2);
                            }

                            const data = form.serialize();
                            const url = form.attr("action");
                            $.post(url, data)
                                .done(function () {
                                    dialog.modal("hide");
                                    $(".modal-backdrop").remove();

                                    const baseUrl = $("body").data("base-url");
                                    const url = `${baseUrl}/Patent/CountryApplication/GetExpDate`;
                                    $.get(url, { appId: mainForm.find("#AppId").val() })
                                        .done((result) => {
                                            mainForm.find("input[name='tStamp']").val(result.tStamp);
                                            const expDateInput = mainForm.find("input[name='ExpDate']");
                                            const expDate = expDateInput.data("kendoDatePicker");
                                            expDate.value(result.ExpDate ? new Date(result.ExpDate) : null);
                                            expDateInput.closest(".float-label").addClass("active").removeClass("inactive");
                                            deferred.resolve(true);
                                        })
                                        .fail(function (error) {
                                            pageHelper.showErrors(error.responseText);
                                            deferred.reject();
                                        });
                                })
                                .fail(function (error) {
                                    pageHelper.showErrors(error.responseText);
                                    deferred.reject();
                                });
                        });
                    dialog.find("input[name='SelectExpirationDate']").on("change", function () {
                        $(dialog.find("input[name='ExpireDate']")).val(this.value);
                    });
                    dialog.find(".cancel, .cancel-all").on("click", function () {
                        if (self.checkTerminalDisclaimerChild) {
                            self.checkTerminalDisclaimerChild = false;
                            deferred.resolve(true);
                        }
                        else
                            deferred.resolve(false);
                    });
                }
                else if (jqXhr.status === 204) {
                    deferred.resolve(false);
                }
            })
            .fail(function (e) {
                pageHelper.showErrors(e.responseText);
                deferred.reject();
            });

        return deferred.promise();
    }

    showMultipleBasedConfirm(deferred, multipleBasedOnSessionKey) {
        let mainForm = $(`#${this.mainDetailContainer}`).find("form")[0];
        mainForm = $(mainForm);
        const url = mainForm.data("multiple-based-on");
        const self = this;

        $.get(url, { appId: mainForm.find("#AppId").val(), sessionKey: multipleBasedOnSessionKey})
            .done(function (html, status, jqXhr) {

                //204 = nocontent
                if (jqXhr.status !== 204) {
                    $(".cpiContainerPopup").empty();
                    const popupContainer = $(".cpiContainerPopup").last();
                    popupContainer.html(html);

                    const dialog = $("#patMultipleBasedOnDialog");
                    dialog.modal("show");

                    dialog.find(".save").on("click",
                        function () {

                            const grid = $("#multiBasedOnGrid").data("kendoGrid");
                            const list = grid.dataSource.data().map((item) => {
                                return {
                                    LogId: item.LogId,
                                    Accept: item.Accept
                                }
                            });

                            const baseUrl = $("body").data("base-url");
                            const url = `${baseUrl}/Patent/PatMultipleBasedOn/AddSelectedActions`;

                            $.post(url, { list:list })
                                .done(() => {
                                    dialog.modal("hide");
                                    deferred.resolve(true);
                                })
                                .fail(function (error) {
                                    pageHelper.showErrors(error.responseText);
                                    deferred.reject();
                                });
                        });
                    dialog.find(".cancel").on("click", function () {
                        deferred.resolve(true);
                            
                    });
                }
                else if (jqXhr.status === 204) {
                    deferred.resolve(false);
                }
            })
            .fail(function (e) {
                pageHelper.showErrors(e.responseText);
                deferred.reject();
            });

        return deferred.promise();
    }

    showTerminalDisclaimerFromMultipleConfirm(appId) {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Patent/CountryApplication/TerminalDisclaimerFromMultipleConfirm`;

        $.get(url, { appId }).done((result) => {
            if (result) {
                $(".cpiContainerPopup").empty();
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialog = $("#patTerminalDisclaimerDialog");
                dialog.modal("show");
                dialog.floatLabels();

                let entryForm = dialog.find("form")[0];
                entryForm = $(entryForm);
                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialog,
                        afterSubmit: (result) => {
                            dialog.modal("hide");
                            if (result) {
                                let mainForm = $(`#${this.mainDetailContainer}`).find("form")[0];
                                mainForm = $(mainForm);
                                mainForm.find("input[name='tStamp']").val(result.tStamp);
                                const expDateInput = mainForm.find("input[name='ExpDate']");
                                const expDate = expDateInput.data("kendoDatePicker");
                                expDate.value(result.ExpDate ? new Date(result.ExpDate) : null);
                                expDateInput.closest(".float-label").addClass("active").removeClass("inactive");
                            }
                        }
                    }
                );
            }


        }).fail(function (error) {
            pageHelper.showErrors(error.responseText);
            deferred.reject();
        });


    }

    showTerminalDisclaimerConfirm(id, oldTerminalDiscAppid, deferred) {
        const terminalDiscAppid = $(`#${this.mainDetailContainer}`).find("input[name='TerminalDisclaimerAppId']").data("kendoComboBox").value();
        const baseUrl = $("body").data("base-url");
        let url = `${baseUrl}/Patent/CountryApplication/TerminalDisclaimerConfirm`;

        if (terminalDiscAppid !== oldTerminalDiscAppid) {
            $.get(url, { appId: id, terminalDiscAppId: terminalDiscAppid }).done((result) => {
                if (result) {
                    $(".cpiContainerPopup").empty();
                    const popupContainer = $(".cpiContainerPopup").last();
                    popupContainer.html(result);
                    const dialog = $("#patTerminalDisclaimerDialog");
                    dialog.modal("show");
                    dialog.floatLabels();

                    let entryForm = dialog.find("form")[0];
                    entryForm = $(entryForm);
                    entryForm.cpiPopupEntryForm(
                        {
                            dialogContainer: dialog,
                            afterSubmit: (result) => {
                                dialog.modal("hide");
                                if (result) {
                                    let mainForm = $(`#${this.mainDetailContainer}`).find("form")[0];
                                    mainForm = $(mainForm);
                                    mainForm.find("input[name='tStamp']").val(result.tStamp);
                                    const expDateInput = mainForm.find("input[name='ExpDate']");
                                    const expDate = expDateInput.data("kendoDatePicker");
                                    expDate.value(result.ExpDate ? new Date(result.ExpDate) : null);
                                    expDateInput.closest(".float-label").addClass("active").removeClass("inactive");
                                }
                                deferred.resolve();
                            }
                        }
                    );
                }
                else deferred.resolve();

            }).fail(function (error) {
                pageHelper.showErrors(error.responseText);
                deferred.reject();
            });
        }
        else
            deferred.resolve();

        return deferred.promise();
    }


    checkTerminalDisclaimerChildConfirm(id, deferred) {
        const baseUrl = $("body").data("base-url");
        let url = `${baseUrl}/Patent/CountryApplication/CheckTerminalDisclaimerChildConfirm`;

        $.get(url, { appId: id }).done((result) => {
            if (result) {
                $(".cpiContainerPopup").empty();
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialog = $("#patTerminalDisclaimerChildDialog");
                dialog.modal("show");

                dialog.find("#copy").on("click", function () {
                    const grid = $("#terminalDisclaimerChildUpdateGrid").data("kendoGrid");
                    const baseUrl = $("body").data("base-url");
                    const updateUrl = `${baseUrl}/Patent/CountryApplication/TerminalDisclaimerUpdateChildExpiration`;

                    const ids = [];
                    const gridData = grid.dataSource.data();
                    const keys = grid.selectedKeyNames();

                    for (const item in keys) {
                        const rec = gridData.find(r => r.AppId === +keys[item]);
                        if (rec) {
                            //const selected = { AppId: rec.AppId, NewExpDate: pageHelper.cpiDateFormatToSave(rec.NewExpDate)};
                            const selected = { AppId: rec.AppId }; //safer, because of possible date regional setting issue
                            ids.push(selected);
                        }
                    }
                    const data = {
                        appId: id,
                        destination: ids
                    };
                    $.post(updateUrl, data)
                        .done(function () {
                            dialog.modal("hide");
                            $(".modal-backdrop").remove();
                            deferred.resolve();
                        })
                        .fail(function (error) {
                            pageHelper.showErrors(error.responseText);
                            deferred.reject();
                        });

                });

                dialog.find("#cancel").on("click", function () {
                    dialog.modal("hide");
                    $(".modal-backdrop").remove();
                    deferred.resolve();
                });

            }
            else {
                deferred.resolve();
            }

        }).fail(function (error) {
            pageHelper.showErrors(error.responseText);
            deferred.reject();
        });
        return deferred.promise();
    }

    terminalDisclaimerChildUpdateSelectionChange(parent) {
        const copyButton = $(`#${parent}`).find("#copy");
        const grid = $("#terminalDisclaimerChildUpdateGrid").data("kendoGrid");
        if (grid.selectedKeyNames().length > 0)
            copyButton.removeAttr("disabled");
        else
            copyButton.attr("disabled", "disabled");
    }

    terminalDisclaimerRefreshCheckBox(appId) {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Patent/PatTerminalDisclaimer/GetHasTerminalDisclaimer`;
        const self = this;

        $(".modal-backdrop").remove();

        $.get(url, { appId: appId })
            .done(function (result) {
                let parent = $(`#${self.mainDetailContainer}`).find("form")[0];
                var terminalDisclaimer = $(parent).find("#TerminalDisclaimer");
                terminalDisclaimer.prop("checked", result.HasTerminalDisclaimer);
                if (result.HasTerminalDisclaimer) {
                    $(parent).find(".cpiTerminalDisclaimerLink").removeClass("d-none");
                }
                else
                    $(parent).find(".cpiTerminalDisclaimerLink").addClass("d-none");
            })
            .fail(function (e) {
                pageHelper.showErrors(e.responseText);
            });
    }
    
    multipleBasedSelectionChange(e) {

        if (e.action == "itemchange" && e.field == "Accept") {
            const grid = $("#multiBasedOnGrid").data("kendoGrid");
            const data = grid.dataSource.data();

            const modifiedRec = e.items[0];
            const otherRecs = data.filter(item => item.ActionType == modifiedRec.ActionType && item.ActionDue == modifiedRec.ActionDue && item.LogId != modifiedRec.LogId);

            //always clear other recs
            for (const rec of otherRecs) {
                rec.Accept = false;
            }
            if (!modifiedRec.Accept) {
                otherRecs[0].Accept = true;
            }

            const actionNames = [];
            for (const rec of data) {
                const actionName = rec.ActionType + '|' + rec.ActionDue;
                const existing = actionNames.some(r => r==actionName);
                if (!existing) {
                    actionNames.push(actionName);
                }
            }
            
            let allowSave = true;
            for (const rec of actionNames) {
                const existing = data.filter(r => r.ActionType + '|' + r.ActionDue == rec && r.Accept);
                if (existing.length==0) {
                    allowSave =false;
                }
            }
            
            const saveButton = $("#patMultipleBasedOnDialog").find(".save");
            if (allowSave)
                saveButton.removeAttr("disabled");

            grid.refresh();                
        }
    }

    getCountry = () => {
        const country = $(`#${this.detailContentContainer}`).find("input[name='Country']");
        return { country: country.val() };
    }

    onChange_Country = (e) => {
        const caseType = $(`#${this.detailContentContainer}`).find("input[name='CaseType']");
        caseType.data("fetched", 0);
        pageHelper.onComboBoxChangeDisplayName(e, 'CountryName');
        this.toggleOptionalFieldsVisibility();
        this.getUSDefaultTaxSchedule();
    }

    onChange_CaseType = (e) => {
        this.toggleOptionalFieldsVisibility();
    }

    toggleOptionalFieldsVisibility() {
        const form = $("#" + this.detailContentContainer).find("form");
        const country = form.find("input[name='Country']");
        const caseType = form.find("input[name='CaseType']");
        const url = $(form).data("optional-fields-setting-url");

        $.get(url, {
            country: country.val(),
            caseType: caseType.val()
        }).done((result) => {
            const taxSched = form.find("#tax-sched-form-group");
            const claim = form.find("#claim-form-group");
            const natNo = form.find("#nat-no-form-group");
            const certNo = form.find("#cert-no-form-group");
            const confirmation = form.find("#confirmation-form-group");
            const trackOne = form.find("#trackone-form-group");

            /* Unitary Patent start 1 */
            const unitaryPatent = form.find("#unitary-patent-form-group");
            const upcStatus = form.find(".upc-status-form-group");
            const unitaryEffect = form.find(".unitary-effect-form-group");
            /* Unitary Patent end 1 */

            if (result.showTaxScheduleField) {
                taxSched.removeClass("d-none");
                form.find("#lblTaxSchedule").text(result.taxScheduleLabel);
            }
            else
                taxSched.addClass("d-none");

            if (result.showClaimField)
                claim.removeClass("d-none");
            else
                claim.addClass("d-none");

            if (result.showNationalField) {
                natNo.removeClass("d-none");
                certNo.removeClass("d-none");
            }
            else {
                natNo.addClass("d-none");
                certNo.addClass("d-none");
            }

            if (result.showConfirmationField)
                confirmation.removeClass("d-none");
            else
                confirmation.addClass("d-none");

            if (result.showTrackOne)
                trackOne.removeClass("d-none");
            else
                trackOne.addClass("d-none");

            /* Unitary Patent start 2 */
            if (result.showUnitaryEffectFields || result.showUPCStatusFields) {
                unitaryPatent.removeClass("d-none");

                if (result.showUnitaryEffectFields)
                    unitaryEffect.removeClass("d-none");
                else
                    unitaryEffect.addClass("d-none");

                if (result.showUPCStatusFields)
                    upcStatus.removeClass("d-none");
                else
                    upcStatus.addClass("d-none");
            }
            else
                unitaryPatent.addClass("d-none");
            /* Unitary Patent end 2 */
        });

        const idsInfo = form.find(".ids-case-info");
        if (country.val() == "US") {
            idsInfo.removeClass("d-none");
        }
        else {
            idsInfo.addClass("d-none");
        }
    }

    getUSDefaultTaxSchedule(hasRo) {
        const form = $("#" + this.detailContentContainer).find("form");
        const country = form.find("input[name='Country']").val();
        const taxSchedule = form.find("input[name='TaxSchedule']");

        if (country.toUpperCase() === 'US') {
            const caseNumber = form.find("input[name='CaseNumber']").val();
            if (caseNumber) {
                const baseUrl = $("body").data("base-url");
                const url = `${baseUrl}/Patent/CountryApplication/GetUSDefaultTaxSchedule`;

                $.get(url, { caseNumber: caseNumber })
                    .done((result) => {
                        taxSchedule.val(result.taxSchedule);
                        taxSchedule.closest(".float-label").addClass("active").removeClass("inactive");
                    })
                    .fail(function (error) {
                        pageHelper.showErrors(error.responseText);
                    });
            }
        }
        else {
            const appId = form.find("input[name='AppId']").val();
            if (+appId === 0) {
                taxSchedule.val("");
                taxSchedule.closest(".float-label").removeClass("active").addClass("inactive");
            }
        }

        if (hasRo) {
            const caseNumber = form.find("input[name='CaseNumber']").val();
            const appId = form.find("input[name='AppId']").val();

            if (caseNumber && +appId === 0) {
                const baseUrl = $("body").data("base-url");
                const url = `${baseUrl}/Patent/CountryApplication/GetRespOffice`;

                $.get(url, { caseNumber: caseNumber })
                    .done((result) => {
                        const el = form.find("input[name = 'RespOffice']");
                        const respOffice = el.data("kendoMultiColumnComboBox");
                        respOffice.value(result.respOffice);
                        el.closest(".float-label").addClass("active").removeClass("inactive");

                    })
                    .fail(function (error) {
                        pageHelper.showErrors(error.responseText);
                    });
            }
        }
    }


    /* assignments */
    assignmentsEdit(e) {
        pageHelper.addMaxLength(e.container);
    }

    assignmentsGrid_onDirty = () => {
        const container = $("#countryAppAssignmentsTabContent");
        container.find(".k-grid-InventorTransfer").hide();
        container.find(".k-grid-Copy").hide();
    }

    assignmentsGrid_onSaveCancel = () => {
        const container = $("#countryAppAssignmentsTabContent");
        container.find(".k-grid-InventorTransfer").show();
        container.find(".k-grid-Copy").show();
    }

    handleAssignmentMenu = () => {
        const assignmentsGrid = $("#countryAppAssignmentsTabContent").find(`#assignmentsGrid_${this.mainDetailContainer}`);
        const baseUrl = $("body").data("base-url");
        const self = this;

        assignmentsGrid.on("click", ".k-grid-InventorTransfer", () => {
            const url = `${baseUrl}/Patent/PatAssignment/GetInventorTransferScreen`;

            $.get(url, { appId: this.currentRecordId }).done((result) => {
                $(".cpiContainerPopup").empty();
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialog = $("#patInventorTransferDialog");
                dialog.modal("show");
                dialog.floatLabels();

                dialog.on("change", "#IsAssignmentFromAll", (e) => {
                    var isAssignmentFromAll = e.currentTarget.checked;

                    var assignmentFromSingle = $("#AssignmentFromSingle");
                    var assignmentFromMulti = $("#AssignmentFromMulti");
                    var assignmentMultiSelect = $("#MultiAssignmentFrom").data("kendoMultiSelect");

                    if (isAssignmentFromAll) {
                        assignmentMultiSelect.value([]);
                        assignmentMultiSelect.enable(false);

                        assignmentFromSingle.removeClass("d-none");
                        assignmentFromMulti.addClass("d-none");
                    }
                    else {
                        assignmentFromSingle.addClass("d-none");
                        assignmentFromMulti.removeClass("d-none");

                        assignmentMultiSelect.enable(true);
                    }
                });

                let entryForm = dialog.find("form")[0];
                entryForm = $(entryForm);

                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialog,
                        afterSubmit: function () {
                            assignmentsGrid.data("kendoGrid").dataSource.read();
                            dialog.modal("hide");
                            self.updateRecordStamps();
                        }
                    }
                );
            }).fail(function (error) {
                pageHelper.showErrors(error.responseText);
            });
        });

        assignmentsGrid.on("click", ".k-grid-Copy", () => {
            const url = `${baseUrl}/Patent/PatAssignment/GetAssignmentCopyScreen`;

            $.get(url, { appId: this.currentRecordId }).done((result) => {
                $(".cpiContainerPopup").empty();
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialog = $("#assignmentCopyDialog");
                dialog.modal("show");
                dialog.floatLabels();

                dialog.find("#copy").on("click", () => {
                    const grid = dialog.find(".kendo-Grid").data("kendoGrid");

                    const copyUrl = `${baseUrl}/Patent/PatAssignment/CopyAssignments`;
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

    /* designation */
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

    setDesCountrySettings() {
        const el = $("#appDesignatedCountriesGrid");
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
                    self.cpiConfirm.confirm(confirmTitle, confirmMsg, function () {
                        const data = { appId: parent.data("appid"), fromCountryLaw: true };
                        $.post(parent.data("url-load"), data).done(function () {
                            //grid.dataSource.query({ page: 1, pageSize: pageSize })
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
                    self.cpiConfirm.confirm(confirmTitle, confirmMsg, function () {
                        const data = { appId: parent.data("appid"), fromCountryLaw: false };
                        $.post(parent.data("url-load"), data).done(function () {
                            //grid.dataSource.query({ page: 1, pageSize: pageSize })
                            grid.dataSource.read().fail(function (error) {
                                pageHelper.showErrors(error.responseText);
                            });
                        });
                    });
                });
            el.find(".k-grid-toolbar").on("click",
                ".k-grid-GenerateAll",
                function () {
                    if (parent.data("parent-country") == "UP") {
                        const grid = $("#appDesignatedCountriesGrid").data("kendoGrid");
                        if (grid.dataSource.data().length > 0) {
                            const confirmTitle = parent.data("msg-confirm-title");
                            const confirmMsg = parent.data("msg-confirm-gen-all");

                            cpiConfirm.confirm(confirmTitle, confirmMsg, function () {
                                $.ajax({
                                    url: parent.data("url-generate-up"),
                                    data: { parentAppId: parent.data("appid") },
                                    type: "POST",
                                    success: function () {
                                        grid.dataSource.read();
                                    },
                                    error: function (error) {
                                        pageHelper.showErrors(error.responseText);
                                    }
                                });
                            });
                        }
                        else {
                            pageHelper.showErrors(parent.data("msg-gen-all-empty"));
                        }
                    }
                    else {
                        $.get(parent.data("url-select")).done(function (result) {
                            $(".cpiContainerPopup").empty();
                            const popupContainer = $(".cpiContainerPopup").last();
                            popupContainer.html(result);
                            const dialog = $("#patDesCtryGenerateDialog");
                            dialog.modal("show");

                            $(dialog.find("#patDesCtrySelectAll")).click(function () {
                                self.markAllDesigCountries(this.checked);
                            });
                            $(dialog.find("#patDesCtryGenerate")).click(function () { self.generateApplications(); });
                        }).fail(function (error) {
                            pageHelper.showErrors(error.responseText);
                        });
                    }

                });
        }
    }

    getFamilyReferenceParam = () => {
        const form = $($(`#${this.detailContentContainer}`).find("form")[0]);
        const appId = form.find("#AppId").val();
        const caseNumber = form.find("input[name='CaseNumber']").val();
        return { appId: appId, caseNumber: caseNumber };
    }

    getDesCountryParam = () => {
        const form = $($("#" + this.detailContentContainer).find("form")[0]);
        const appId = form.find("#AppId").val();
        const country = form.find("input[name='Country']").val();
        const caseType = form.find("input[name='CaseType']").val();
        return { country: country, caseType: caseType, appId: appId };
    }

    getDesCaseTypeParam = () => {
        const form = $($(`#${this.detailContentContainer}`).find("form")[0]);
        const country = form.find("input[name='Country']").val();
        const caseType = form.find("input[name='CaseType']").val();
        return { country: country, caseType: caseType, desCountry: this.selectedDesCountry };
    }

    getDefaultCaseType = (e) => {
        this.selectedDesCountry = e.dataItem.DesCountry;

        const desCaseType = e.dataItem.DesCaseType;
        const row = $(`#${e.sender.element[0].id}`).closest("tr");
        const grid = $("#appDesignatedCountriesGrid").data("kendoGrid");
        const dataItem = grid.dataItem(row);
        dataItem.DesCaseType = desCaseType;
        $(row).find(".des-case-type").html(kendo.htmlEncode(desCaseType));
    }

    generateApplication(e, grid) {
        const row = $(e.currentTarget).closest("tr");
        const dataItem = grid.dataItem(row);
        const parent = $("#appDesCountryContainer");
        const confirmTitle = parent.data("msg-confirm-title");
        let confirmMsg = parent.data("msg-confirm-gen").replace("{country}", dataItem.DesCountry);

        $.get(parent.data("url-check-ep"), { appId: dataItem.AppId }).done((r) => {
            if (r.showWarning) {
                confirmMsg = parent.data("msg-confirm-no-grant").replace("{country}", dataItem.DesCountry);
            }
            confirmMsg = confirmMsg.replace(/(\r\n|\n|\r)/g, "<br>");
            this.cpiConfirm.confirm(confirmTitle, confirmMsg, function () {
                $.ajax({
                    url: parent.data("url-generate"),
                    //data: { parentAppId: dataItem.AppId, desCountries: `|${dataItem.DesCountry}~${dataItem.GenSubCase}|`, desId: dataItem.DesId },
                    data: { parentAppId: dataItem.AppId, desCountries: `|${dataItem.DesCountry}|`, desId: dataItem.DesId },
                    type: "POST",
                    success: function (result) {
                        const genDate = window.kendo.parseDate(result.GenDate);
                        dataItem.GenDate = pageHelper.cpiDateFormatToDisplay(genDate);
                        let rowHtml = grid.rowTemplate(dataItem);
                        rowHtml = rowHtml.replace("k-grid-Generate", "k-grid-Generate d-none");
                        rowHtml = rowHtml.replace("d-none", "");
                        rowHtml = rowHtml.replace("Detail/0", `Detail/${result.GenAppId}`);
                        row.replaceWith(rowHtml);
                    },
                    error: function (error) {
                        if (error.responseJSON !== undefined)
                            pageHelper.showGridErrors(error.responseJSON);
                        else
                            pageHelper.showErrors(error.responseText);
                    }
                });
            });

        }).fail((e) => {
            pageHelper.showErrors(e.responseText);
        });

    }

    markAllDesigCountries(check) {
        $("#patDesCountries input").each(function () {
            if (!this.disabled)
                this.checked = check;
        });
    }

    generateApplications() {
        let countries = "|";
        const parent = $("#patDesCountries");

        $(parent.find("input")).each(function () {
            if (this.checked && !this.disabled) {
                //countries += $(this).attr("name") + "~|";
                countries += $(this).attr("name") + "|";
            }
        });
        if (countries.length > 1) {
            $.get(parent.data("url-check-ep"), { appId: parent.data("appid") })
                .done((r) => {
                    if (r.showWarning) {
                        let confirmMsg = parent.data("msg-confirm-no-grant-all");
                        confirmMsg = confirmMsg.replace(/(\r\n|\n|\r)/g, "<br>");
                        const confirmTitle = parent.data("msg-confirm-title");

                        this.cpiConfirm.confirm(confirmTitle, confirmMsg, () => {
                            generateSelectedDesigs();
                        });
                    }
                    else {
                        generateSelectedDesigs();
                    }
                })

                .fail((e) => {
                    pageHelper.showErrors(e.responseText);
                });
        }

        function generateSelectedDesigs() {
            $.ajax({
                url: parent.data("url-generate"),
                data: { parentAppId: parent.data("appid"), desCountries: countries, desId: 0 },
                type: "POST",
                success: function () {
                    const grid = $("#appDesignatedCountriesGrid").data("kendoGrid");
                    grid.dataSource.read();
                },
                error: function (error) {
                    pageHelper.showErrors(error.responseText);
                }
            });
        }
    }

    //* IDS */
    handleIDSMenu() {
        const idsInfo = $("#idsInfo");

        const url = idsInfo.data("url");
        const data = {
            appId: idsInfo.data("appid"),
            idsMenuChoice: this.idsMenuChoice ? this.idsMenuChoice : "References"
        };

        this.loadIDSRef = true;
        this.loadIDSNonPat = true;

        const self = this;
        idsInfo.find("a").click(function () {
            const selected = $(this).data("value");
            data.idsMenuChoice = selected;
            self.loadIDSGrid(url, data);

        });
        this.loadIDSGrid(url, data);
        this.setIDSCopyHandler(idsInfo, data.appId);
        this.setIDSPrintHandler(data.appId);
        this.setIDSExportToExcelHandler(idsInfo);
        this.setIDSINPADOCImportHandler(idsInfo, data.appId);
        this.setIDSDOCXHandler(data.appId); //DOCX - IDS
        this.setIDSFileDateUpdateHandler(idsInfo, data.appId);
        this.setIDSFileDateExaminerUpdateHandler(idsInfo, data.appId);
        this.setIDSLoadReferencesHandler();
    }

    loadIDSGrid(url, data) {

        if (this.loadIDSRef || this.loadIDSNonPat) {
            $.get(url, data)
                .done((html) => {
                    if (data.idsMenuChoice === "References") {
                        const tabContent = $("#idsRefTabContent");
                        tabContent.html(html);
                        if (!tabContent.hasClass("active"))
                            tabContent.addClass("active")

                        this.setIDSRefSettings();
                    } else {
                        const tabContent = $("#idsNonPatTabContent");
                        tabContent.html(html);
                        if (!tabContent.hasClass("active"))
                            tabContent.addClass("active")

                        this.setIDSNonPatSettings();
                    }
                })
                .fail(function (e) {
                    pageHelper.showErrors(e.responseText);
                });
        }

    }

    setIDSPrintHandler(appId) {
        $("#idsPrintLink").on("click", function () {
            const url = $(this).data("url");
            const country = $(this).data("country");

            $.get(url, { appId: appId, country: country })
                .done(function (html) {
                    $(".cpiContainerPopup").empty();
                    const popupContainer = $(".cpiContainerPopup").last();
                    popupContainer.html(html);
                    idsLib.showEFSGenForm();
                })
                .fail(function (e) {
                    pageHelper.showErrors(e.responseText);
                });
        });
    }

    setIDSExportToExcelHandler(idsInfo) {
        $("#idsExportExcel").on("click", function () {
            const type = $(idsInfo.find("a.active")).data("value");
            const url = $(this).data("url");
            const form = $("#ctryAppIDSExportForm");
            form.attr("action", url);
            form.find("#IdsMenuChoice").val(type);
            form.submit();
        });
    }

    setIDSINPADOCImportHandler(idsInfo, appId) {
        $("#idsImportINPADOC").on("click", function () {
            const type = $(idsInfo.find("a.active")).data("value");
            const url = $(this).data("url");
            cpiLoadingSpinner.show();

            $.get(url, { appId: appId, idsMenuChoice: type })
                .done(function (html) {
                    cpiLoadingSpinner.hide();
                    $(".cpiContainerPopup").empty();
                    const popupContainer = $(".cpiContainerPopup").last();
                    popupContainer.html(html);
                    const dialog = $("#patIDSINPADOCImportDialog");
                    dialog.modal("show");
                    dialog.floatLabels();

                    dialog.find("#idsCitationInfo a").click(function () {
                        dialog.find("#idsCitationInfoContent .tab-pane").removeClass("active");
                        const selected = $(this).attr("href");
                        const tabContent = $(selected);
                        tabContent.addClass("active");
                    });

                    dialog.find("#import").on("click", function () {
                        const citedGrid = $("#citedImportGrid").data("kendoGrid");
                        const nplGrid = $("#nplImportGrid").data("kendoGrid");

                        const importUrl = $(this).data("url");
                        const citedData = citedGrid.dataSource.data();
                        const citedSelected = citedData.filter(item => item.Import).map((item) => {
                            return {
                                country: item.Country,
                                computedDocNo: item.ComputedDocNo,
                                docDate: pageHelper.cpiDateFormatToSave(item.DocDate),
                                computedKD: item.ComputedKD,
                                firstInventor: item.FirstInventor,
                                docnoType: item.DocNoType,
                                citId: item.CitId
                            }
                        });
                        const nplData = nplGrid.dataSource.data();
                        const nplSelected = nplData.filter(item => item.Import).map((item) => {
                            return {
                                nplText: item.NPLText,
                                citId: item.CitId
                            }
                        });

                        cpiLoadingSpinner.show();
                        const data = { appId: appId, citedSelected: citedSelected, nplSelected: nplSelected };
                        $.post(importUrl, data)
                            .done((result) => {
                                pageHelper.showSuccess(result.success);

                                $("#appIDSRelatedCasesList").data("kendoListView").dataSource.read();

                                const nonPatGrid = $("#appIDSNonPatLiteratureGrid");
                                if (nonPatGrid.length > 0)
                                    nonPatGrid.data("kendoGrid").dataSource.read();

                                dialog.modal("hide");
                                cpiLoadingSpinner.hide();
                            })
                            .fail(function (error) { cpiLoadingSpinner.hide(); pageHelper.showErrors(error.responseText); });
                    });
                })
                .fail(function (e) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(e.responseText);
                });
        });
    }

    setIDSLoadReferencesHandler() {
        const linkButton = $("#loadReferencesInStaging");
        const checkUrl = linkButton.data("check-url");

        $.get(checkUrl)
            .done(function (result) {
                if (result.hasRec) {
                    linkButton.removeClass("d-none");
                    linkButton.on("click", function () {
                        const url = linkButton.data("process-url");
                        cpiLoadingSpinner.show();
                        $.post(url)
                            .done(function () {
                                $("#appIDSRelatedCasesList").data("kendoListView").dataSource.read();
                                linkButton.addClass("d-none");
                                cpiLoadingSpinner.hide();
                            })
                            .fail(function (e) {
                                pageHelper.showErrors(e.responseText);
                                cpiLoadingSpinner.hide();
                            });
                    });
                }
                else { linkButton.addClass("d-none"); }
            })
            .fail(function (e) {
                pageHelper.showErrors(e.responseText);
            });


        
    }

    setIDSCopyHandler(idsInfo, appId) {
        const self = this;
        $("#idsCopyLink").on("click", function () {
            const type = $(idsInfo.find("a.active")).data("value");
            const url = $(this).data("url");

            $.get(url, { appId: appId, idsMenuChoice: type })
                .done(function (html) {
                    const popupContainer = $(".site-content .popup");
                    popupContainer.empty();
                    popupContainer.html(html);
                    const dialog = $("#patIDSCopyDialog");
                    dialog.modal("show");
                    dialog.floatLabels();

                    dialog.find("#idsCopyInfo a").click(function () {
                        dialog.find("#idsCopyInfoContent .tab-pane").removeClass("active");
                        const selected = $(this).attr("href");
                        const tabContent = $(selected);
                        tabContent.addClass("active");
                    });

                    dialog.find("#showActiveOnly").change(function () {
                        self.getIDSCopySources();
                    });

                    dialog.find("#copy").on("click", function () {
                        const grid = $("#idsCopyGrid").data("kendoGrid");

                        const copyUrl = $(this).data("url");
                        const data = {
                            appId: appId,
                            from: getFromParam(type),
                        };

                        if (type === "References") {
                            const gridRelated = $("#idsCopyRelatedGrid").data("kendoGrid");
                            data.fromRelated = gridRelated.selectedKeyNames();
                        }
                        $.post(copyUrl, data)
                            .done(function () {
                                let grid;
                                if (type === "References")
                                    grid = $("#appIDSRelatedCasesList").data("kendoListView");
                                else
                                    grid = $("#appIDSNonPatLiteratureGrid").data("kendoGrid");

                                grid.dataSource.read();
                                dialog.modal("hide");
                            })
                            .fail(function (error) { pageHelper.showErrors(error.responseText); });

                        function getFromParam(type) {
                            const ids = [];
                            const data = grid.dataSource.data();
                            const keys = grid.selectedKeyNames();

                            if (type === "References") {
                                for (const item in keys) {
                                    const rec = data.find(r => r.RelatedCasesId === +keys[item]);
                                    if (rec) {
                                        const selected = { RelatedCasesId: rec.RelatedCasesId, BaseAppId: rec.AppId };
                                        ids.push(selected);
                                    }
                                }
                                return ids;
                            }
                            else
                                return keys;
                        }

                    });


                })
                .fail(function (e) {
                    pageHelper.showErrors(e.responseText);
                });


        });
    }

    getIDSCopyParam() {
        let form = $("#patIDSCopyDialog").find("form")[0];
        form = $(form);

        const param = {
            caseNumber: form.find("input[name = 'CaseNumber']").val(),
            country: form.find("input[name = 'Country']").val(),
            subCase: form.find("input[name = 'SubCase']").val(),
            inventor: form.find("input[name = 'Inventor']").val(),
            keyword: form.find("input[name = 'Keyword']").val(),
            artUnit: form.find("input[name = 'ArtUnit']").val(),
            activeOnly: form.find("#showActiveOnly").is(':checked'),
            excludeAppId: form.find("#AppId").val()
        };

        if (form.find("#SearchText").length > 0)
            param.searchText = form.find("#SearchText").val();
        return param;

    }

    getIDSCopySources() {
        const grid = $("#idsCopyGrid").data("kendoGrid");
        grid._selectedIds = {};
        grid.clearSelection();
        grid.dataSource.read();

        const relatedGrid = $("#idsCopyRelatedGrid").data("kendoGrid");
        if (relatedGrid) {
            relatedGrid._selectedIds = {};
            relatedGrid.clearSelection();
            relatedGrid.dataSource.read();
        }
    }

    setIDSFileDateUpdateHandler(idsInfo, appId) {
        $("#idsUpdateDateLink").on("click", function () {
            const type = $(idsInfo.find("a.active")).data("value");
            const recordType = type.toLowerCase() === "references" ? "R" : "N";

            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Patent/PatIDSManage/UpdateFilDateScreen`;

            $.get(url, { appId: appId, recordType: recordType })
                .done((result) => {
                    const popupContainer = $(".site-content .popup");
                    popupContainer.empty();
                    popupContainer.html(result);
                    const dialogContainer = $("#patIDSUpdateFilDateDialog");
                    let entryForm = dialogContainer.find("form")[0];
                    dialogContainer.modal("show");
                    entryForm = $(entryForm);

                    entryForm.cpiPopupEntryForm(
                        {
                            dialogContainer: dialogContainer,
                            afterSubmit: function () {

                                let grid = null;
                                if (recordType === "R")
                                    grid = $("#appIDSRelatedCasesList").data("kendoListView");
                                else
                                    grid = $("#appIDSNonPatLiteratureGrid").data("kendoGrid");

                                grid.dataSource.read();
                                dialogContainer.modal("hide");
                            }
                        }
                    );
                })
                .fail((e => {
                    pageHelper.showErrors(e);
                }));

        });
    }

    setIDSFileDateExaminerUpdateHandler(idsInfo, appId) {
        $("#idsUpdateExaminerLink").on("click", function () {
            const type = $(idsInfo.find("a.active")).data("value");
            const recordType = type.toLowerCase() === "references" ? "R" : "N";

            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Patent/PatIDSManage/UpdateFilDateExaminerScreen`;

            $.get(url, { appId: appId, recordType: recordType })
                .done((result) => {
                    const popupContainer = $(".site-content .popup");
                    popupContainer.empty();
                    popupContainer.html(result);
                    const dialogContainer = $("#patIDSUpdateConsideredExaminerDialog");
                    let entryForm = dialogContainer.find("form")[0];
                    dialogContainer.modal("show");
                    entryForm = $(entryForm);

                    entryForm.cpiPopupEntryForm(
                        {
                            dialogContainer: dialogContainer,
                            afterSubmit: function () {

                                let grid = null;
                                if (recordType === "R")
                                    grid = $("#appIDSRelatedCasesList").data("kendoListView");
                                else
                                    grid = $("#appIDSNonPatLiteratureGrid").data("kendoGrid");

                                grid.dataSource.read();
                                dialogContainer.modal("hide");
                            }
                        }
                    );
                })
                .fail((e => {
                    pageHelper.showErrors(e);
                }));

        });
    }

    //DOCX
    setIDSDOCXHandler(appId) {
        $("#idsDOCXLink").on("click", function () {
            const url = $(this).data("url");

            $.get(url, { Id: appId, screenCode: "IDS-DOCX" })
                .done(function (html) {
                    $(".cpiContainerPopup").empty();
                    const popupContainer = $(".cpiContainerPopup").last();
                    popupContainer.html(html);
                    const dialog = $("#patIDSDOCXDialog");
                    dialog.modal("show");
                })
                .fail(function (e) {
                    pageHelper.showErrors(e.responseText);
                });
        });
    }

    idsCopySelectionChange(parent) {
        const copyButton = $(`#${parent}`).find("#copy");

        const grid = $("#idsCopyGrid").data("kendoGrid");
        const gridRelated = $("#idsCopyRelatedGrid").data("kendoGrid");
        if (grid.selectedKeyNames().length > 0 || gridRelated.selectedKeyNames().length > 0)
            copyButton.removeAttr("disabled");
        else
            copyButton.attr("disabled", "disabled");
    }

    idsCopyToFamilySelectionChange(parent) {
        const copyButton = $(`#${parent}`).find("#copy");

        const grid = $("#idsCopyToFamilyGrid").data("kendoGrid");
        if (grid.selectedKeyNames().length > 0)
            copyButton.removeAttr("disabled");
        else
            copyButton.attr("disabled", "disabled");
    }

    setIDSRefSettings() {
        if (this.loadIDSRef) {
            const name = "appIDSRelatedCasesList";
            const el = $(`#${name}`);
            const grid = el.data("kendoListView");
            const self = this;

            if (grid) {
                grid.dataSource.read();
                this.loadIDSRef = false;
                const appId = el.closest("div").data("appid");

                if (!this.isEditableGridRegistered(name)) {
                    const gridInfo = {
                        name: name,
                        isDirty: false,
                        filter: { parentId: appId },
                        afterSubmit: this.updateRecordStamps
                    };
                    this.addEditableGrid(gridInfo);
                    const idsRelatedCasesList = $("#appIDSRelatedCasesTable");
                    idsRelatedCasesList.find(".k-link").sorter(grid, true);
                    //listview editor
                    this.handleIDSListViewEntry(el, grid);

                    //file download
                    this.idsRefsSelected = [];
                    idsRelatedCasesList.on("input", ".k-selector-all", (e) => {
                        const checkbox = $(e.target);
                        const selected = checkbox.prop("checked");

                        idsRelatedCasesList.find("input.k-selector").each((i, cb) => {
                            const id = $(cb).closest("tr").data("id");
                            const index = self.idsRefsSelected.indexOf(id);
                            if (selected) {
                                $(cb).attr("checked", "checked");
                                if (index === -1) {
                                    self.idsRefsSelected.push(id);
                                }
                            }
                            else {
                                $(cb).removeAttr("checked");
                                if (index >= 0) {
                                    self.idsRefsSelected.splice(index, 1);
                                }
                            }
                        });
                    });
                    idsRelatedCasesList.on("input", "input.k-selector", (e) => {
                        const checkbox = $(e.target);
                        const id = checkbox.closest("tr").data("id");
                        const index = self.idsRefsSelected.indexOf(id);

                        if (checkbox.prop("checked")) {
                            if (index === -1) {
                                self.idsRefsSelected.push(id);
                            }
                        }
                        else {
                            if (index >= 0) {
                                self.idsRefsSelected.splice(index, 1);
                            }
                        }
                    });
                }

                const uploadForm = $("#idsFileUpload");
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
                            const msg = el.parent().data("upload-success");
                            pageHelper.showSuccess(msg);
                            grid.dataSource.read();
                            uploadForm.find("input[Name='DocFile']").val(null);
                            self.updateRecordStamps();
                        },
                        error: function (e) {
                            pageHelper.showErrors(e);
                        }
                    });

                });
            }
        }
    }

    downloadIDSRefImage = (parentId, url) => {
        if (this.idsRefsSelected.length > 0) {
            let downloadForm = $("#documentsDownload").last();
            if (downloadForm.length > 0) {
                downloadForm.remove();
            }
            $(`<form action="${url}" method="post" id="documentsDownload"><input type="hidden" name="ParentId" value="${parentId}"/><input type="hidden" name="Selection" value="${this.idsRefsSelected.join()}"/></form>`).appendTo('body').submit();
        }
    }

    showCopyToFamilyScreen = (appId, relatedCasesId) => {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Patent/PatIDS/GetIDSCopyToFamilyScreen`;
        const self = this;

        $.post(url, { appId: appId, relatedCasesId: relatedCasesId })
            .done(function (html) {
                $(".cpiContainerPopup").empty();
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(html);
                const dialog = $("#patIDSCopyToFamilyDialog");
                dialog.modal("show");
                dialog.find("#copy").on("click", function () {
                    const grid = $("#idsCopyToFamilyGrid").data("kendoGrid");
                    const selected = grid.selectedKeyNames();

                    const allData = grid.dataSource.data();
                    const selection = selected.map((item) => {
                        let genAction = false;
                        const existing = allData.find(r => r.AppId === parseInt(item));
                        if (existing) {
                            let dataItem = grid.dataSource.getByUid(existing.uid);
                            genAction = dataItem.GenerateAction;
                        }
                        return { appId: item, genAction: genAction }
                    });
                    const data = {
                        appId: appId,
                        relatedCasesIds: [relatedCasesId],
                        actionToGenerate: dialog.find("input[name='ActionToGenerate']").val(),
                        baseDate: pageHelper.cpiDateFormatToSave(dialog.find("#BaseDate").data("kendoDatePicker").value()),
                        dueDate: dialog.find("#DueDate").val(),
                        indicator: dialog.find("#Indicator").val(),
                        selection: selection
                    };
                    self.idsCopyToFamily(dialog, data);
                });
            })
            .fail(function (e) {
                pageHelper.showErrors(e.responseText);
            });
    }

    idsCopyToFamily(screen, data) {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Patent/PatIDS/IDSCopyToFamily`;

        $.post(url, { selection: data })
            .done(function (result) {
                pageHelper.showSuccess(result.success);
                screen.modal("hide");
            })
            .fail(function (e) {
                pageHelper.showErrors(e.responseText);
            });

    }

    handleIDSListViewEntry(el, grid) {
        const form = $("#idsEntry");
        const pager = $("#appIDSRelatedCasesList_pager");
        const self = this;
        let countrySrc = "";
        let tabPressed = false;

        if (pager) {
            pager.addClass("pt-3");
            form.append(pager);
        }

        form.on("focusout", "input[name='RelatedCaseNumber_input']", (e) => {
            if (tabPressed) {
                tabPressed = false;
                form.find("input[name='RelatedCountryS_input']").focus();
                form.find("input[name='RelatedCountryR_input']").focus();
            }
        });

        form.on("keydown", "input[name='RelatedCaseNumber_input']", (e) => {
            tabPressed = false;
            const keyCode = e.keyCode || e.which;
            if (keyCode === 9) {
                tabPressed = true;
            }
        });

        form.on("focusout", "input[name='RelatedCountryS_input']", (e) => {
            countrySrc = "S";

            if (tabPressed) {
                tabPressed = false;
                form.find("input[name='RelatedPubNumber']").focus();
            }
        });

        form.on("keydown", "input[name='RelatedCountryS_input']", (e) => {
            countrySrc = "S";
            tabPressed = false;
            const keyCode = e.keyCode || e.which;
            if (keyCode === 9) {
                tabPressed = true;
            }
        });

        form.on("focusout", "input[name='RelatedCountryR_input']", (e) => {
            countrySrc = "R";

            if (tabPressed) {
                tabPressed = false;
                form.find("input[name='RelatedDateFiled']").focus();
            }
        });

        form.on("keydown", "input[name='RelatedCountryR_input']", (e) => {
            countrySrc = "R";
            tabPressed = false;
            const keyCode = e.keyCode || e.which;
            if (keyCode === 9) {
                tabPressed = true;
            }
        });

        form.on("focusout", "input[name='RefPages']", (e) => {
            if (tabPressed) {
                tabPressed = false;
                if (countrySrc === "S")
                    form.find("input[name='RelatedPubDateR']").focus();
                else
                    form.find("input[name='ReferenceSrc_input']").focus();
            }
        });

        form.on("keydown", "input[name='RefPages']", (e) => {
            tabPressed = false;
            const keyCode = e.keyCode || e.which;
            if (keyCode === 9) {
                tabPressed = true;
            }
        });

        $("#idsAddNew").on("click", () => {
            const page = grid.pager.page();
            if (page !== 1 && page > 0)
                grid.pager.page(1);
            else
                grid.pager.page(0);

            this.idsListGetEditTemplate(grid, null);
            this.markIdsEntryDirty();
        });

        $("#idsRefDownload").on("click", () => {
            if (this.idsRefsSelected.length > 0) {
                let downloadForm = $("#documentsDownload").last();
                if (downloadForm.length > 0) {
                    downloadForm.remove();
                }
                const baseUrl = $("body").data("base-url");
                const url = `${baseUrl}/Patent/PatIDS/DownloadIDSRefImage`;
                const parentId = $("#patIDSRelatedEntryDialog").data("appid");
                $(`<form action="${url}" method="post" id="documentsDownload"><input type="hidden" name="ParentId" value="${parentId}"/><input type="hidden" name="Selection" value="${this.idsRefsSelected.join()}"/></form>`).appendTo('body').submit();
            }
        });

        el.on("click", "tr.item-view", function (e) {
            if (!$(e.target).hasClass("k-selector"))
                self.idsListGetEditTemplate(grid, $(this));
        });
        el.on("click", ".ids-upload", function (e) {
            self.handleIDSListViewUpload(e, grid);
        });
        el.on("click", ".ids-doc-open", function (e) {
            e.stopPropagation();
        });
        el.on("click", ".delete-file-link", function (e) {
            e.stopPropagation();
            const uploadLink = $(e.currentTarget);
            const relatedCasesId = uploadLink.data("id");
            const uploadForm = $("#idsFileUpload");

            const title = uploadForm.data("upload-title");
            const msg = uploadForm.data("delete-file-msg");

            cpiConfirm.confirm(title, msg, function () {
                const baseUrl = $("body").data("base-url");
                const url = `${baseUrl}/Patent/PatIDS/RelatedCasesFileDelete`;
                $.post(url, { relatedCasesId: relatedCasesId })
                    .done(function () {
                        grid.dataSource.read();
                        self.updateRecordStamps();
                    })
                    .fail(function (e) {
                        pageHelper.showErrors(e.responseText);
                    });
            });

        });
        el.on("click", ".ids-delete", function (e) {
            self.handleIDSListViewDelete(e, grid);
        });

        el.on("click", ".ids-view", function (e) {
            self.handleIDSListViewViewer(e, grid);
        });

        el.on("click", ".ids-copy-family", function (e) {
            e.stopPropagation();
            const relatedCasesId = $(this).closest("tr").data("id");
            const appId = el.closest("div").data("appid");
            self.showCopyToFamilyScreen(appId, relatedCasesId);
        });
        el.on("click", ".referenceSrcLink", function (e) {
            e.stopPropagation();
            e.preventDefault();
            let url = $(e.currentTarget).data("url");
            //const row = $(e.target).closest("tr");
            //const dataItem = grid.dataItem(row);            
            const referenceSrc = $(e.currentTarget).data("referencesrc");
            const linkUrl = url.replace("actualValue", referenceSrc);
            pageHelper.openLink(linkUrl, false);
        });

        $("#idsCancelAll").on("click", () => {
            const container = $("#patIDSRelatedEntryDialog");
            const title = container.data("confirm-title");
            const cancelPrompt = container.data("cancel-confirm-message");

            cpiConfirm.confirm(title, cancelPrompt, () => {
                $("#appIDSRelatedCasesList").data("kendoListView").dataSource.read();
                this.setIdsEntryNotDirty();
                $("#appIDSRelatedCasesTable").find(".k-link").data("sorter").enableSort();
            });

        });

        $("#idsSaveAll").on("click", () => {
            self.handleIDSListViewSave(grid);
            $("#appIDSRelatedCasesTable").find(".k-link").data("sorter").enableSort();
        });

        form.on("submit", (e) => {
            e.preventDefault();
            e.stopPropagation();

            if (form.valid()) {
                this.idsListGetFormData(form, grid);
                window.cpiStatusMessage.hide();

            } else {
                //window.cpiStatusMessage.error(form.data("invalid-msg"));
                pageHelper.showErrors(form.data("invalid-msg"));
                form.wasValidated();
            }
        });
    }

    idsListGetFormData(form, grid) {
        const data = pageHelper.formDataToJson(form);

        //disabled combo will not get posted
        data.payLoad.RelatedFirstNamedInventor = form.find("input[name = 'RelatedFirstNamedInventorR']").data("kendoComboBox").text();

        if (!data.payLoad.RelatedPubDateR)
            data.payLoad.RelatedPubDateR = null;

        if (!data.payLoad.RelatedIssDateR)
            data.payLoad.RelatedIssDateR = null;

        const relatedCase = this.mapRawDataToRelatedCase(data.payLoad);
        relatedCase.dirty = true;

        const allData = grid.dataSource.data();
        const existing = allData.find(r => r.RelatedCasesId === relatedCase.RelatedCasesId);

        if (existing) {
            let dataItem = grid.dataSource.getByUid(existing.uid);

            const entries = Object.entries(relatedCase);
            for (const [prop, val] of entries) {
                dataItem[prop] = val;
            }
        }
    }

    idsListGetEditTemplate(grid, row) {

        //can modify
        if ($("#idsAddNew").length === 0)
            return;

        $("#appIDSRelatedCasesTable").find(".k-link").data("sorter").disableSort();

        const form = $("#idsEntry");
        //submit the previous one first
        if (form.find("#RelatedPubNumber").length > 0) {
            form.submit();
            if (!form.valid())
                return;

            const id = form.find("#RelatedCasesId").val();
            const data = grid.dataSource.data().find(r => r.RelatedCasesId === parseInt(id));
            if (data) {
                const template = kendo.template($("#idsRefTemplate").html());
                const result = template(data);

                const oldRow = $(`#appIDSRelatedCasesList tr[data-id=${id}]`);
                $(result).insertBefore(oldRow[0]);
                oldRow.remove();
            }
        }

        let id = 0;
        if (row)
            id = row.data("id");

        //add
        else {
            id = this.genTempRelatedCaseId();
        }

        const container = $("#patIDSRelatedEntryDialog");
        let templateUrl = container.data("url-edit-template");
        let param;

        const data = grid.dataSource.data().find(r => r.RelatedCasesId === id);
        if (data) {
            param = this.mapRawDataToRelatedCase(data);
            this.idsFormatDatesForServer(param);
        }
        else {
            param = {
                relatedCasesId: id,
                appId: container.data("appid"),
                relatedAppId: 0,
                activeSwitch: true,
                reliedUpon: true,
                matchTypeUsed: "Cited",
                isDirty: false
            };
            const inserted = grid.dataSource.insert(0, {});
            inserted.dirty = true;
            inserted.RelatedCasesId = id;
        }

        $.get(templateUrl, param, (response) => {
            const listId = "appIDSRelatedCasesList";
            const idSelector = row === null ? 0 : id; //new row starts with 0 id

            const oldRow = $(`#${listId} tr[data-id=${idSelector}]`);
            $(response).insertBefore(oldRow[0]);
            oldRow.remove();

            idsLib.stripeTableRows(listId);

            if (param.RelatedAppId > 0)
                idsLib.toggleIDSRelatedInfo(form, false);

            idsLib.rtsInpadocIDSSetListener(form);
            this.handleFormChanges(form);

            $.validator.unobtrusive.parse(form);
            if (form.data("validator") !== undefined) {
                form.data("validator").settings.ignore = ""; //include hidden fields (kendo controls)
            }
            pageHelper.addMaxLength(form);
            pageHelper.clearInvalidKendoDate(form);

        });
    }

    handleIDSListViewSave(grid) {
        const form = $("#idsEntry");

        if (form.find("#RelatedPubNumber").length > 0) {
            form.submit();
            if (!form.valid())
                return;
        }

        const relatedCasesToSave = grid.dataSource.data().filter(r => r.dirty);

        if (relatedCasesToSave.length > 0) {
            const container = $("#patIDSRelatedEntryDialog");
            const saveError = container.data("save-error");
            const saveMsg = container.data("save-message");
            const saveMsgAll = container.data("save-message-all");

            window.pageHelper.hideErrors();
            const saveUrl = container.data("save-url");

            const list = [];
            relatedCasesToSave.forEach(e => {
                const relatedCase = this.mapRawDataToRelatedCase(e);
                this.idsFormatDatesForServer(relatedCase);
                list.push(relatedCase);
            });

            $.post(saveUrl, { idsRelatedCases: list })
                .done(() => {
                    const msg = relatedCasesToSave.length === 1 ? saveMsg : saveMsgAll;

                    grid.dataSource.read();
                    this.setIdsEntryNotDirty();
                    this.updateRecordStamps();
                    pageHelper.showSuccess(msg);
                })
                .fail((error) => {
                    if (error)
                        pageHelper.showErrors(error);
                    else
                        pageHelper.showErrors(saveError);
                });
        }
    }

    handleIDSListViewUpload(e, grid) {
        e.stopPropagation();

        const parent = $($(e.target).parents("tr")[0]);
        const id = parent.data("id");
        const appId = parent.data("appid");
        const uploadForm = $("#idsFileUpload");

        uploadForm.find("#RelatedCasesId").val(id);
        uploadForm.find("#AppId").val(appId);

        if ($.isNumeric($(e.target).closest("a").data("file"))) {
            const title = uploadForm.data("upload-title");
            const msg = uploadForm.data("upload-msg");
            cpiConfirm.confirm(title, msg, function () { uploadForm.find("input").trigger("click"); });
        }
        else {
            uploadForm.find("input").trigger("click");
        }
    }

    handleIDSListViewDelete(e, grid) {
        e.stopPropagation();

        const parent = $($(e.target).parents("tr")[0]);
        const id = parent.data("id");
        const appId = parent.data("appid");
        const tStamp = parent.data("stamp");

        const container = $("#patIDSRelatedEntryDialog");
        const msg = container.data("delete-success-message");
        const title = container.data("confirm-title");
        const deletePrompt = container.data("delete-confirm-message");
        const deleteUrl = container.data("delete-url");

        cpiConfirm.delete(title, deletePrompt, () => {
            cpiLoadingSpinner.show();

            $.post(deleteUrl, { relatedCasesId: id, appId: appId, tStamp: tStamp })
                .done(() => {
                    grid.dataSource.read();
                    pageHelper.showSuccess(msg);
                    cpiLoadingSpinner.hide();

                    if (grid.dataSource.data().length === 0) {
                        $("#idsSaveAll").addClass("d-none");
                        $("#idsCancelAll").addClass("d-none");
                    }
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error);
                });
        });
    }
    handleIDSListViewViewer(e, grid) {
        e.stopPropagation();

        const parent = $($(e.target).parents("tr")[0]);
        const idsFile = parent.data("idsfile");
        const container = $("#patIDSRelatedEntryDialog");
        const sharePointOn = container.data("is-sharepoint-on");
        const documentStorageType = container.data("document-storage-type");
        const baseUrl = $("body").data("base-url");

        if (documentStorageType === 1) {            
            const url = `${baseUrl}/Patent/PatIDS/GetSharePointPreviewUrl`;

            $.get(url, { refType: "R", fileName: idsFile, appId: parent.data("appid") })
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
        else if (documentStorageType === 2) {
            const fileId = parent.data("fileid");
            const url = `${baseUrl}/iManageWork/OpenFile/${fileId}`;
            window.open(url);
        }
        else if (documentStorageType === 3) {
                const fileId = parent.data("fileid");
                const url = `${baseUrl}/NetDocs/OpenFile/${fileId}`;
                window.open(url);
        }
        else if (documentStorageType === 0) {
            const url = container.data("viewer-url") + "?docfile=" + idsFile + "&key=" + parent.data("appid");
            documentPage.zoomDocument(url);
        }

    }

    handleGridDocViewer(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const idsFile = dataItem.DocFilePath;

        const container = $(grid.element[0]).closest("div.grid-container")
        const sharePointOn = container.data("is-sharepoint-on");

        if (sharePointOn == "1") {
            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Patent/PatIDS/GetSharePointPreviewUrl`;

            $.get(url, { refType: "L", fileName: idsFile, appId: container.data("appid") })
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
            const url = container.data("viewer-url") + "&docfile=" + idsFile + "&key=" + dataItem.AppId;
            documentPage.zoomDocument(url);
        }

    }


    markIdsEntryDirty(el) {
        $("#idsSaveAll").removeClass("d-none");
        $("#idsCancelAll").removeClass("d-none");
        $("#appIDSRelatedCasesList_pager").hide();
        cpiBreadCrumbs.markLastNode({ dirty: true });
        this.recordNavigator.hide();

        if (el !== undefined) {
            const parent = $($(el).parents("tr")[0]);
            const id = parent.data("id");
            const record = $("#appIDSRelatedCasesList").find(`tr[data-id=${id}]`);
            record.find(".ids-copy-family").addClass("d-none");
        }
    }
    setIdsEntryNotDirty() {
        $("#idsSaveAll").addClass("d-none");
        $("#idsCancelAll").addClass("d-none");
        $("#appIDSRelatedCasesList_pager").show();
        window.cpiStatusMessage.hide();
        cpiBreadCrumbs.markLastNode({ dirty: false });
        this.recordNavigator.show();
    }

    handleFormChanges(form) {
        const self = this;
        form.on("input", "input,textarea", (e) => { self.markIdsEntryDirty(e.target); });
        form.on("change", "input[type='checkbox']", (e) => { self.markIdsEntryDirty(e.target); });
        form.find(".k-combobox > input").each(function () {
            const comboBox = $(this).data("kendoComboBox");
            if (comboBox) {
                comboBox.bind("change", (e) => {
                    self.markIdsEntryDirty(e.sender.element[0]);
                });
            }
        });

        form.find("input[data-role='multicolumncombobox']").each(function () {
            const comboBox = $(this).data("kendoMultiColumnComboBox");
            if (comboBox) {
                comboBox.bind("change", function () {
                    self.markIdsEntryDirty(e.sender.element[0]);
                });
            }
        });

        form.find(".k-dropdownlist > input").each(function () {
            const dropdownList = $(this).data("kendoDropDownList");
            if (dropdownList) {
                dropdownList.bind("change", (e) => {
                    self.markIdsEntryDirty(e.sender.element[0]);
                });
            }
        });
        form.find(".k-datepicker input").each(function () {
            const datePicker = $(this).data("kendoDatePicker");
            if (datePicker) {
                datePicker.bind("change", (e) => {
                    self.markIdsEntryDirty(e.sender.element[0]);
                });
            }
        });
    }

    genTempRelatedCaseId() {
        const date = new Date();
        const hour = date.getHours();
        const mins = date.getMinutes();
        const secs = date.getSeconds();
        const ms = date.getMilliseconds();

        const id = parseInt([hour, mins, secs, ms].join("")) * -1;
        return id;
    }

    idsFormatDatesForServer(e) {
        if (e.RelatedPubDate && typeof e.RelatedPubDate !== "string") {
            e.RelatedPubDate = pageHelper.cpiDateFormatToSave(e.RelatedPubDate);
        }
        if (e.RelatedIssDate && typeof e.RelatedIssDate !== "string") {
            e.RelatedIssDate = pageHelper.cpiDateFormatToSave(e.RelatedIssDate);
        }
        if (e.RelatedDateFiled && typeof e.RelatedDateFiled !== "string") {
            e.RelatedDateFiled = pageHelper.cpiDateFormatToSave(e.RelatedDateFiled);
        }
        if (e.ReferenceDate && typeof e.ReferenceDate !== "string") {
            e.ReferenceDate = pageHelper.cpiDateFormatToSave(e.ReferenceDate);
        }
        if (e.DateCreated && typeof e.DateCreated !== "string") {
            e.DateCreated = pageHelper.cpiDateFormatToSave(e.DateCreated);
        }
    }


    mapRawDataToRelatedCase(data) {
        const relatedCase = {
            RelatedCasesId: data.RelatedCasesId ? parseInt(data.RelatedCasesId) : 0,
            AppIDConnect: data.AppIDConnect ? data.AppIDConnect : 0,
            AppId: data.AppId ? data.AppId : 0,
            RelatedAppId: data.RelatedAppId ? data.RelatedAppId : 0,

            tStamp: data.tStamp ? data.tStamp : "",
            MatchTypeUsed: data.MatchTypeUsed ? data.MatchTypeUsed : "",
            RelatedCaseNumber: data.RelatedCaseNumber ? data.RelatedCaseNumber : "",
            RelatedPubNumber: data.RelatedPubNumber ? data.RelatedPubNumber : "",
            RelatedPatNumber: data.RelatedPatNumber ? data.RelatedPatNumber : "",
            RelatedFirstNamedInventor: data.RelatedFirstNamedInventor ? data.RelatedFirstNamedInventor : "",
            KindCode: data.KindCode ? data.KindCode : "",
            RefPages: data.RefPages ? data.RefPages : "",
            RelatedCountry: data.RelatedCountry ? data.RelatedCountry : "",
            RelatedSubCase: data.RelatedSubCase ? data.RelatedSubCase : "",
            ReferenceSrc: data.ReferenceSrc ? data.ReferenceSrc : "",
            Remarks: data.Remarks ? data.Remarks : "",
            RelatedDateFiled: data.RelatedDateFiled ? new Date(data.RelatedDateFiled) : null,
            ReferenceDate: data.ReferenceDate ? new Date(data.ReferenceDate) : null,

            //RelatedDateFiled: data.RelatedDateFiled,
            //ReferenceDate: data.ReferenceDate,

            ReliedUpon: data.ReliedUpon !== undefined ? data.ReliedUpon : 0,
            ActiveSwitch: data.ActiveSwitch !== undefined ? data.ActiveSwitch : 0,
            HasTranslation: data.HasTranslation !== undefined ? data.HasTranslation : 0,
            ConsideredByExaminer: data.ConsideredByExaminer !== undefined ? data.ConsideredByExaminer : 0,
            //CopyToFamily: data.CopyToFamily !== undefined ? data.CopyToFamily : 0,
            isDirty: data.dirty,
            CreatedBy: data.CreatedBy ? data.CreatedBy : "",
            DateCreated: data.DateCreated ? new Date(data.DateCreated) : null
        };

        if (data.RelatedPubDateR !== undefined)
            relatedCase.RelatedPubDate = data.RelatedPubDateR ? new Date(data.RelatedPubDateR) : null; //from Form
        else
            relatedCase.RelatedPubDate = data.RelatedPubDate ? new Date(data.RelatedPubDate) : null;
        //relatedCase.RelatedPubDate = data.RelatedPubDate;


        if (data.RelatedIssDateR !== undefined)
            relatedCase.RelatedIssDate = data.RelatedIssDateR ? new Date(data.RelatedIssDateR) : null;
        else
            relatedCase.RelatedIssDate = data.RelatedIssDate ? new Date(data.RelatedIssDate) : null;
        //relatedCase.RelatedIssDate = data.RelatedIssDate;

        return relatedCase;
    }

    relatedCasesListDataBound = (e) => {
        idsLib.stripeTableRows("appIDSRelatedCasesList");

        $("#appIDSRelatedCasesTable").find("input.k-selector").each((i, cb) => {
            const id = $(cb).closest("tr").data("id");
            const index = this.idsRefsSelected.indexOf(id);
            if (index >= 0) {
                $(cb).attr("checked", "checked");
            }
        });

        //for color coding the excess records
        //var dataSource = e.sender.dataSource;
        //const idsNoFeeMax = parseInt($("#countryAppIDSTabContent").data("ids-nofee-max"));
        //const page = dataSource.page(); 
        //const pageSize = dataSource.pageSize(); 
        //const excess = page * pageSize - idsNoFeeMax;

        ////this page has records that will exceed the limit
        //if (excess > 0) {
        //    let recsToMarkStart = 1;
        //    if (excess < pageSize) 
        //        recsToMarkStart = (pageSize - excess) + 1;

        //    $("#appIDSRelatedCasesList td.related-country").each(function (index) {
        //        const row = index + 1;
        //        if (row >= recsToMarkStart) {
        //            $(this).addClass("ids-with-fee");
        //        }
        //    });
        //}
        this.idsCountRefresh();
    }

    nonPatLiteratureGridDataBound = (e) => {
        var literatureIndex = e.sender.wrapper.find(".k-grid-header [data-field=" + "NonPatLiteratureInfo" + "]").index();
        var referenceSrcIndex = e.sender.wrapper.find(".k-grid-header [data-field=" + "ReferenceSrc" + "]").index();

        var dataItems = e.sender.dataSource.view();
        for (var j = 0; j < dataItems.length; j++) {
            var filed = dataItems[j].get("RelatedDateFiled");
            var withFee = dataItems[j].get("WithFee");

            var row = e.sender.tbody.find("[data-uid='" + dataItems[j].uid + "']");
            var literatureCell = row.children().eq(literatureIndex);
            var referenceSrcCell = row.children().eq(referenceSrcIndex);

            if (filed != null) {
                literatureCell.addClass("ids-filed");
            }
            else {
                literatureCell.addClass("ids-not-filed");
            }
            if (withFee) {
                referenceSrcCell.addClass("ids-with-fee");
            }
        }
        this.idsCountRefresh();
    }

    idsCountRefresh = () => {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Patent/PatIDS/GetIDSTotal`;
        let form = $("#countryApplicationDetailsView-Content").find("form")[0];
        form = $(form);

        $.get(url, { appId: form.find("#AppId").val() })
            .done(function (result) {
                form.find("#idsFiledCount").html(result.FiledCount);
                form.find("#idsUnfiledCount").html(result.UnfiledCount);
                form.find("#idsXMLCount").html(result.XMLCount);
                form.find("#idsXMLCountLastUpdate").html(result.XMLCountLastUpdateFormatted);

                const idsFeeRisk = form.find("#idsFeeRisk");
                if (result.FeeRiskStatus > 0) {
                    idsFeeRisk.removeClass("d-none");
                    idsFeeRisk.removeClass("ids-risk-ok");
                    idsFeeRisk.removeClass("ids-risk-not-ok");
                    idsFeeRisk.removeClass("ids-risk-warning");

                    if (result.FeeRiskStatus == 1) {
                        idsFeeRisk.addClass("ids-risk-ok");
                    }
                    else if (result.FeeRiskStatus == 3) {
                        idsFeeRisk.addClass("ids-risk-not-ok");
                    }
                    else {
                        idsFeeRisk.addClass("ids-risk-warning");
                    }
                }
                else {
                    idsFeeRisk.addClass("d-none");
                }
            })
            .fail(function (e) {
                pageHelper.showErrors(e.responseText);
            });
    }

    setIDSNonPatSettings() {
        if (this.loadIDSNonPat) {
            const name = "appIDSNonPatLiteratureGrid";
            const el = $(`#${name}`);
            const grid = el.data("kendoGrid");
            const self = this;
            let selectedNPLRow = null;

            if (grid) {
                const uploadForm = $("#idsNonPatFileUpload");
                this.loadIDSNonPat = false;

                grid.dataSource.read();
                el.find(".k-grid-toolbar").on("click",
                    ".k-grid-AddIDSNonPat",
                    function () {
                        const parent = el.parent();
                        const url = parent.data("url-add");
                        const data = { appId: parent.data("appid") };
                        idsLib.openIDSNonPatEntry(grid, url, data, false);
                    });

                el.on("input, change", "input", function () {
                    el.find(".k-grid-AddIDSNonPat").addClass("d-none");
                    el.find(".k-grid-Edit").addClass("d-none");
                });

                grid.dataSource.bind('change', function (e) {
                    if (e.action !== "itemchange") {
                        el.find(".k-grid-AddIDSNonPat").removeClass("d-none");
                    }
                });

                if (!this.isEditableGridRegistered(name)) {
                    const gridInfo = {
                        name: name,
                        isDirty: false,
                        filter: { parentId: el.parent().data("appid") },
                        afterSubmit: this.updateRecordStamps
                    };
                    this.addEditableGrid(gridInfo);
                    //this.entryFormInstance.setGridDirtyTracking(gridInfo);
                    pageHelper.kendoGridDirtyTracking(el, gridInfo);
                }
                el.on("click", ".delete-file-link", function (e) {
                    e.preventDefault();

                    const uploadLink = $(e.currentTarget);
                    const nonPatLiteratureId = uploadLink.data("id");

                    const title = el.parent().data("upload-title");
                    const msg = el.parent().data("delete-file-msg");
                    cpiConfirm.confirm(title, msg, function () {
                        const baseUrl = $("body").data("base-url");
                        const url = `${baseUrl}/Patent/PatIDS/NonPatLiteratureFileDelete`;

                        $.post(url, { nonPatLiteratureId: nonPatLiteratureId })
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
                    uploadForm.find("#NonPatLiteratureId").val(uploadLink.data("id"));

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
                        },
                        error: function (e) {
                            pageHelper.showErrors(e);
                        }
                    });

                });

                //grid.bind("excelExport", function (e) {
                //    e.preventDefault();
                //    var exportForm = $("#ctryAppExportForm");
                //    const baseUrl = $("body").data("base-url");
                //    const url = `${baseUrl}/Patent/PatIDS/NonPatLiteratureExport`;
                //    exportForm.attr("action", url);
                //    exportForm.submit();
                //});

                el.on("click", ".referenceSrcLink, .referenceSrcButtonLink", function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                    let url = $(e.currentTarget).data("url");
                    const row = $(e.target).closest("tr");
                    const dataItem = grid.dataItem(row);
                    const linkUrl = url.replace("actualValue", dataItem.ReferenceSrc);
                    pageHelper.openLink(linkUrl, false);
                });

            }
        }
    }


    /* Related Cases */
    relatedCasesEdit = (e) => {

        pageHelper.addMaxLength(e.container);
        this.relatedCaseNumber =
            e.model.RelatedCaseNumber !== undefined ? e.model.RelatedCaseNumber : "";

        if (e.model.RelatedAppId > 0) {
            e.container.find("#RelatedPatNumber").prop("disabled", true);
        }

    }

    relatedCountryGetParam = () => {
        const caseNumber = this.relatedCaseNumber !== undefined ? this.relatedCaseNumber : "";
        return {
            caseNumber: caseNumber
        };
    }

    relatedShowOtherInfo = (e) => {
        if (e.dataItem["RelatedAppId"] > 0) {
            const container = $(`#${e.sender.element[0].id}`).closest("tr");
            container.find(".data-SubCase").html(e.dataItem["SubCase"]);
            container.find(".data-CaseType").html(e.dataItem["CaseType"]);
            container.find(".data-Status").html(e.dataItem["Status"]);
            container.find(".data-PatentNo").html(e.dataItem["PatentNo"] != null ? e.dataItem["PatentNo"] : "");
            container.find(".data-IssDate").html(e.dataItem["IssDate"]);
            container.find(".data-AppNo").html(e.dataItem["AppNo"] != null ? e.dataItem["AppNo"] : "");
            container.find(".data-FilDate").html(e.dataItem["FilDate"]);
            this.relatedAppId = e.dataItem["RelatedAppId"];
            this.relatedSubCase = e.dataItem["SubCase"];
        } else {
            this.relatedAppId = 0;
        }
    }

    relatedCasesRowSave = (e) => {
        if (e.model.RelatedCaseNumber !== null && e.model.RelatedCaseNumber > "" && this.relatedAppId > 0) {
            e.model.RelatedAppId = this.relatedAppId;
            e.model.RelatedSubCase = this.relatedSubCase;
        }
    }

    //* RTS */
    rtsLoadPublicDataMenu() {
        this.cpiLoadingSpinner.show();
        const self = this;
        const baseUrl = $("body").data("base-url");
        const pdtUrl = `${baseUrl}/Patent/RTSInfo/PublicDataMenu`;

        const param = {
            country: $(`#Country_${this.mainDetailContainer}`).val(),
            appId: $(`#${this.mainDetailContainer}`).find("#AppId").val(),
            activeTab: `${this.activeRtsTab ? this.activeRtsTab : ''}`
        };

        if (!param.country)
            param.country = $(`#${this.mainDetailContainer}`).find("#Country").val();

        $.post(pdtUrl, param)
            .done(function (html) {
                $("#countryAppPDTTabContent").html(html);
            })
            .fail(function (e) {
                pageHelper.showErrors(e.responseText);
            })
            .always(function () {
                self.cpiLoadingSpinner.hide();
            });
    }

    rtsGetIdParams() {
        const form = $("#rtsPublicDataInfo");
        return {
            AppId: form[0].AppId.value,
            PLAppId: form[0].PLAppId.value
        };
    }

    rtsActionDisplaySelect(e, screen) {

        if (e.dataItem) {
            const value = e.dataItem.Value;

            const downloadedGrid = $(`#rtsActionAsDownloadedGrid_${screen}`);
            const matchedGrid = $(`#rtsActionAsMatchedGrid_${screen}`);

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

    rtsHandleMenu() {
        const rtsForm = $("#rtsPublicDataInfo");
        rtsForm.on("submit", function (e) {
            e.preventDefault();
            e.stopPropagation();

            const params = rtsForm.serialize();

            cpiLoadingSpinner.show();

            $.post(rtsForm.attr("action"), params)
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    $("#rtsPublicDataInfoContainer").html(result);
                })
                .fail(function (e) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(e.responseText);
                });
        });
        rtsForm.find("a").click(function (e) {
            e.preventDefault();
            const choices = rtsForm.find("a");
            $.each(choices, function () {
                $(this).removeClass("active");
            });

            const selected = $(this).data("value");
            $(this).addClass("active");

            $(rtsForm.find("#rtsMenuChoice")[0]).val(selected);
            rtsForm.submit();
        });
        rtsForm.submit();
    }

    rtsGetActionUpdHistory_RevertType() {
        const revertType = $("#RevertType_rtsActionUpdHistory").val();
        return revertType;
    }

    rtsGetActionUpdHistory_ChangeDate() {
        const jobId = $("#JobId_rtsActionUpdHistory").val();
        return (jobId == null || jobId === "") ? 0 : jobId;
    }

    rtsActionUpdHistory_RevertTypeChange = () => {
        this.rtsActionUpdHistoryGridRead();
    }

    rtsGetActionUpdHistory_ChangeDateChange = () => {
        this.rtsActionUpdHistoryGridRead();
    }

    rtsActionUpdHistoryGridRead() {
        const grid = $("#rtsActionUpdHistoryGrid").data("kendoGrid");
        grid.dataSource.read();

        const undoButton = $("#rtsActionUpdHistory_undo");
        if (undoButton) {

            if (this.rtsGetActionUpdHistory_RevertType() === "0" &&
                this.rtsGetActionUpdHistory_ChangeDate() > 0) {
                undoButton.removeClass("d-none");
            } else {
                undoButton.addClass("d-none");
            }
        }
    }

    rtsActionUpdHistorySetBtns() {
        $("#rtsActionUpdHistory_undo").click(() => {
            const data = {
                plAppId: $("#rtsActionUpdHistory_plAppId").val(),
                RevertType: this.rtsGetActionUpdHistory_RevertType,
                jobId: this.rtsGetActionUpdHistory_ChangeDate
            };
            $.post($(this).data("url"), data)
                .done(() => {
                    this.rtsActionUpdHistoryGridRead();
                })
                .fail(function (error) { pageHelper.showErrors(error.responseText); });
        });
    }

    rtsActionUpdHistoryFilter = () => {
        return {
            revertType: this.rtsGetActionUpdHistory_RevertType(),
            jobId: this.rtsGetActionUpdHistory_ChangeDate()
        };
    }

    rtsActionClosedBatchesChange() {
        const grid = $("#rtsActionClosedUpdHistoryGrid").data("kendoGrid");
        grid.dataSource.read();
    }

    rtsActionClosedUpdHistoryFilter() {
        let jobId = $("#JobId_rtsActionClosedUpdHistory").val();

        if (jobId === null || jobId === "") {
            jobId = 0;
        };
        return { jobId: jobId };
    }

    //* RTS Inpadoc */
    rtsInpadocAppSetListener() {
        let form = $("#countryApplicationDetailsView-Content").find("form")[0];
        form = $(form);
        //form.find("#AppNumber,#PubNumber,#PatNumber").keyup(pageHelper.setDelay(function () {
        form.find("#AppNumber,#PubNumber,#PatNumber").siblings(".inp-search").click(function () {
            //const input = $(this);
            const input = $(this).siblings("input");
            const value = input.val();
            if (value.length >= 5) {
                let dateEl;
                const numType = input.data("num-type");
                switch (numType) {
                    case "A":
                        dateEl = "FilDate";
                        break;
                    case "U":
                        dateEl = "PubDate";
                        break;
                    default:
                        dateEl = "IssDate";
                }
                const date = form.find("input[name='" + dateEl + "']").data("kendoDatePicker").value();
                const searchInput = {
                    searchNo: value,
                    searchDate: date,
                    searchCaseType: form.find("input[name='CaseType']").val(),
                    searchCountry: form.find("input[name='Country']").val(),
                    searchNumberType: numType
                };

                input.siblings(".rtsInpSpinner").removeClass("d-none");
                input.siblings(".btn-link").addClass("d-none");
                rtsLib.rtsInpadocSearch(form, input, searchInput);

            }
        });
        //}, 500)); //trigger the Inpadoc search only when the user stops typing for no. of ms

        form.on("onInpadocSelected", (event, data, entryForm) => {
            this.rtsInpadocAppSelect(data, entryForm);
            form.find("#PubNumber").trigger("input");
        });
    }

    rtsInpadocAppSelect(data, form) {
        //if (data.NumberType !== "A")
        form.find("#AppNumber").val(data.AppNo.trim());
        //if (data.NumberType !== "U")
        form.find("#PubNumber").val(data.PubNo.trim());
        //if (data.NumberType !== "P")
        form.find("#PatNumber").val(data.PatNo.trim());

        const title = form.find("#AppTitle");
        if (title.length > 0 && data.Title)
            title.val(data.Title);

        if (data.FilDate) {
            const filDate = form.find("input[name='FilDate']");
            if (filDate.length > 0)
                filDate.data("kendoDatePicker").value(new Date(data.FilDate));
        }
        if (data.PubDate) {
            const pubDate = form.find("input[name='PubDate']");
            if (pubDate.length > 0)
                pubDate.data("kendoDatePicker").value(new Date(data.PubDate));
        }

        if (data.IssDate) {
            const issDate = form.find("input[name='IssDate']");
            if (issDate.length > 0)
                issDate.data("kendoDatePicker").value(new Date(data.IssDate));
        }


        //form.find("input[name='FilDate']").val(data.FilDateDisplay);
        //form.find("input[name='PubDate']").val(data.PubDateDisplay);
        //form.find("input[name='IssDate']").val(data.IssDateDisplay);

        //float labels
        const inpFillable = form.find(".rtsInpFillable .float-label");
        inpFillable.each(function () {
            $(this).removeClass("inactive").addClass("active");
        });
        this.computeTaxStartExpiration = true;
    }

    /* related trademark */
    onChange_RelatedTrademark = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");

            const country = e.dataItem["Country"];
            const subCase = e.dataItem["SubCase"];
            const caseType = e.dataItem["CaseType"];
            const trademarkName = e.dataItem["TrademarkName"];

            const grid = $("#relatedTrademarksGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            dataItem.TmkId = e.dataItem["TmkId"];
            dataItem.Country = country;
            dataItem.SubCase = subCase;
            dataItem.CaseType = caseType;
            dataItem.TrademarkName = trademarkName;

            $(row).find(".country-field").html(kendo.htmlEncode(country));
            $(row).find(".subCase-field").html(kendo.htmlEncode(subCase));
            $(row).find(".caseType-field").html(kendo.htmlEncode(caseType));
            $(row).find(".trademarkName-field").html(kendo.htmlEncode(trademarkName));

        }
    }

    onChange_Product = (e) => {
        if (e.sender) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");
            const grid = $("#productsGrid_countryApplicationDetailsView").data("kendoGrid");
            const dataItem = grid.dataItem(row);

            var comboDataItem = e.sender.dataItem();
            dataItem.ProductId = comboDataItem["ProductId"];
            dataItem.ProductName = comboDataItem["ProductName"];

        }
    }

    onChange_TerminalDisclaimer = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");

            const country = e.dataItem["Country"];
            const subCase = e.dataItem["SubCase"];
            const caseType = e.dataItem["CaseType"];
            let patNumber = e.dataItem["PatNumber"];
            const expDate = e.dataItem["ExpDate"];
            if (!patNumber) patNumber = "";
            let applicationStatus = e.dataItem["ApplicationStatus"];
            if (!applicationStatus) applicationStatus = "";

            const grid = $("#terminalDisclaimersGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            dataItem.TerminalDisclaimerAppId = e.dataItem["AppId"];
            dataItem.Country = country;
            dataItem.SubCase = subCase;
            dataItem.CaseType = caseType;
            dataItem.PatNumber = patNumber;
            dataItem.ExpDate = window.kendo.parseDate(expDate);
            dataItem.ApplicationStatus = applicationStatus;

            $(row).find(".country-field").html(kendo.htmlEncode(country));
            $(row).find(".subCase-field").html(kendo.htmlEncode(subCase));
            $(row).find(".caseType-field").html(kendo.htmlEncode(caseType));
            $(row).find(".patNumber-field").html(kendo.htmlEncode(patNumber));
            $(row).find(".expDate-field").html(kendo.toString(dataItem.ExpDate, 'dd-MMM-yyyy'));
            $(row).find(".status-field").html(kendo.htmlEncode(applicationStatus));
            $(row).find(".status-field").removeClass("inactive");
        }
    }

    /* licensee */
    licenseeRefreshIndicator = (e) => {
        const data = e.sender.dataSource.data();
        if (data.length == 0)
            $("#countryAppLicensesTab").removeClass("has-licensees");
        else
            $("#countryAppLicensesTab").addClass("has-licensees");
    }

    /* products */
    productRefreshIndicator = (e) => {
        const data = e.sender.dataSource.data();
        if (data.length == 0)
            $("#countryAppProductsTab").removeClass("has-products");
        else
            $("#countryAppProductsTab").addClass("has-products");
    }

    /* main copy */
    getCountryFromCopyScreen = () => {
        const country = $("#patCountryAppCopyDialog").find("input[name='Country']");
        return { country: country.val() };
    }

    mainCopyInitialize = () => {
        const baseUrl = $("body").data("base-url");
        const mainUrl = `${baseUrl}/Patent/CountryApplication/`;

        $(document).ready(() => {
            const container = $("#patCountryAppCopyDialog");
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

            container.on("change", "input[type=radio][name=Relationship]", function () {
                const showCaseInfo = this.value === "S";
                triggerCaseInfoSetting(showCaseInfo);
            });

            triggerCaseInfoSetting(false);

            function triggerCaseInfoSetting(showCaseInfo) {
                if (showCaseInfo) {
                    container.find(".case-info-set").show();
                }
                else {
                    container.find(".case-info-set").hide();
                    container.find(".case-info-settings").hide();
                    container.find(".data-to-copy").show();
                }
                container.find("#CopyCaseInfo").attr("checked", showCaseInfo);
                container.find("#CopyCaseInfo").attr("disabled", !showCaseInfo);
            }

        });
    }

    searchResultDataBound = (e) => {
        const data = e.sender.dataSource.data();

        if (data.length > 0) {
            iManage.getDefaultGridImage(this);
            docViewer.getDefaultGridImage(this);
        }
    }

    showInventionLink = (screen, title, isReadOnly) => {
        const container = $(`#${screen}`).find(".cpiButtonsDetail");
        const pageNav = container.find(".nav");
        pageNav.prepend(`<a class="nav-link invention-link" href="#" target="_self" title="${title}" role="button"><i class="fal fa-external-link pr-2"></i>${title}</a>`);
        container.find(".invention-link").on("click", function () {
            if (isReadOnly) {
                $(`#${screen}`).find(".case-number-link").trigger("click");
            }
            else
                $(`#CaseNumber_${screen}_cpiButtonLink`).trigger("click");
        });
    }

    showPatentScore = (screen, title, id, score) => {
        const container = $(`#${screen}`).find(".cpiButtonsDetail");
        const pageNav = container.find(".nav");
        pageNav.prepend(`<a class="nav-link patent-score-link" href="#" title="${title}" role="button"><span class='tickler-patent-score badge'>${score}</span></a>`);
        container.find(".patent-score-link").on("click", () => {

            cpiLoadingSpinner.show();
            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Patent/PatScore/PatentScoreEntry`;
            $.get(url, { appId: id })
                .done((result) => {
                    cpiLoadingSpinner.hide();
                    $(".cpiContainerPopup").empty();
                    const popupContainer = $(".cpiContainerPopup").last();
                    popupContainer.html(result);
                    const dialog = $("#patScoreDialog");
                    dialog.modal("show");
                    dialog.find(".star-rating").rating({
                        filledStar: "<i class='fas fa-star'></i>",
                        emptyStar: "<i class='fa fa-star'></i>",
                        clearButton: "<i class='fa fa-lg fa-minus-circle'></i>",
                        showClear: true,
                        displayOnly: false,
                        showCaption: false,
                        size: "xs",
                        animate: false,
                        hoverEnabled: false
                    }).on("rating:change", function () {
                        displayAverage(entryForm, ".patent-score-average");
                    }).on("rating:clear", function () {
                        displayAverage(entryForm, ".patent-score-average");
                    })

                    let entryForm = dialog.find("form")[0];
                    $(entryForm).on("submit", (e) => {
                        e.preventDefault();
                        const saveButton = $(entryForm).find(".btn-primary");
                        saveButton.attr("disabled", "disabled");
                        const ratings = getRatings();
                        const updateUrl = `${baseUrl}/Patent/PatScore/ScoresUpdate`;
                        $.post(updateUrl, { appId: id, ratings })
                            .done(() => {
                                displayAverage(container, ".tickler-patent-score");
                                dialog.modal("hide");
                                this.updateRecordStamps();
                            })
                            .fail(function (error) {
                                pageHelper.showErrors(error.responseText);
                                saveButton.removeAttr("disabled");
                            });
                    });

                    $(entryForm).on("click", ".toggle-remarks", function () {
                        const categoryId = $(this).data("category-id");
                        $(entryForm).find(`#remarks-con-${categoryId}`).toggleClass("d-none");
                    });

                    function getRatings() {
                        const ratings = [];
                        const form = $(entryForm);
                        form.find(".star-rating").each(function () {
                            const el = $(this);
                            const categoryId = el.data("category-id");
                            const scoreId = el.data("score-id");
                            const remarks = form.find(`#remarks-${categoryId}`).val();
                            const rating = { scoreId, categoryId, score: +el.val(), remarks };
                            ratings.push(rating);
                        });
                        return ratings;
                    }

                    function displayAverage(container, el) {
                        const ratings = getRatings();
                        const average = ratings.map(e => e.score).reduce((total, current) => total + current) / ratings.length;
                        $(container).find(el).html(window.kendo.toString(average, "n1"));
                    }

                })
                .fail(function (error) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error.responseText);
                });
        });
    }

    mainPatentScoreInitialize = (id) => {
        const baseUrl = $("body").data("base-url");
        const mainUrl = `${baseUrl}/Patent/PatScore/`;

        $(document).ready(() => {
            const container = $("#patScoreDialog");
            container.find(".citation-detail").hide();

            container.on("click", ".forward-citation", () => {
                container.find(".citation-detail").show();
                container.find(".patent-score-main").hide();
                container.find(".citation-detail-grid").html("<div class='widget-spinner text-center'><i class='ml-2 fa fa-circle-notch fa-spin'></i></div>");

                const url = `${mainUrl}GetForwardCitationDetail`;
                $.get(url, { appId: id })
                    .done(function (result) {
                        container.find(".widget-spinner").hide();
                        container.find(".citation-detail-grid").html(result);
                        container.find(".exporttopdf").addClass("d-none");
                        container.find(".exporttoppt").addClass("d-none");
                    })
                    .fail(function (e) {
                        container.find(".widget-spinner").hide();
                        pageHelper.showErrors(e.responseText);
                    });
            });
            container.on("click", ".backward-citation", () => {
                container.find(".citation-detail").show();
                container.find(".patent-score-main").hide();
                container.find(".citation-detail-grid").html("<div class='widget-spinner text-center'><i class='ml-2 fa fa-circle-notch fa-spin'></i></div>");

                const url = `${mainUrl}GetBackwardCitationDetail`;
                $.get(url, { appId: id })
                    .done(function (result) {
                        container.find(".widget-spinner").hide();
                        container.find(".citation-detail-grid").html(result);
                        container.find(".exporttopdf").addClass("d-none");
                        container.find(".exporttoppt").addClass("d-none");
                    })
                    .fail(function (e) {
                        container.find(".widget-spinner").hide();
                        pageHelper.showErrors(e.responseText);
                    });
            });
            container.on("click", ".citation-detail-cancel", () => {
                container.find(".citation-detail").hide();
                container.find(".patent-score-main").show();
                container.find(".exporttopdf").removeClass("d-none");
                container.find(".exporttoppt").removeClass("d-none");
            });
        });
    }

    /* terminal disclaimer diagram */
    terminalVisualTemplate = (options) => {
        var dataviz = kendo.dataviz;
        var g = new dataviz.diagram.Group();
        var dataItem = options.dataItem;

        //Add rectangle layout/container
        g.append(new dataviz.diagram.Rectangle({
            width: 300,
            height: 90,
            stroke: {
                width: 1
            }
            //fill: "#81aaac"
        }));

        //Prepare hyperlink for records
        const baseUrl = $("body").data("base-url");
        let recordUrl = `${baseUrl}/Patent/CountryApplication/DetailLink/actualValue`;
        recordUrl = recordUrl.replace('actualValue', dataItem.AppId);

        //Add CaseNumberCtrySub label with hyperlink
        var caseText = new dataviz.diagram.TextBlock({
            text: dataItem.CaseNumberCtrySub,
            x: 5,
            y: 10,
            //color: "#fff"
        });
        caseText.drawingElement.url = recordUrl;
        g.append(caseText);

        //Add Title label with hyperlink
        var layout = new dataviz.diagram.Layout(new dataviz.diagram.Rect(5, 20, 300, 85), {
            alignContent: "center",
            spacing: 4
        });
        g.append(layout);
        var texts = dataItem.AppTitle.split(" ");
        for (var i = 0; i < texts.length; i++) {
            var titleText = new dataviz.diagram.TextBlock({
                text: texts[i],
                x: 5,
                y: 20,
                //color: "#fff"
            });
            titleText.drawingElement.url = recordUrl;

            layout.append(titleText);
        }
        layout.reflow();

        return g;
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

    appProductsGrid_AfterSubmit = (result) => {
        this.updateRecordStamps();
        pageHelper.handleEmailWorkflow(result);
    }

    appInventorsGrid_AfterSubmit = (result) => {
        this.updateRecordStamps();
        pageHelper.handleEmailWorkflow(result);
    }

    appRelatedCasesGrid_AfterSubmit = (result) => {
        this.updateRecordStamps();
        if (result.hasWorkflow) {
            const baseUrl = $("body").data("base-url");
            let url = `${baseUrl}/Patent/PatRelatedCase/ProcessWorkflow`;

            $.get(url, { appId: result.appId }).done(function (result) {
                const popupContainer = $(".site-content .popup");
                popupContainer.empty();
                popupContainer.html(result);
                var dialog = $("#idsActionDialog");
                dialog.modal("show");

                let entryForm = dialog.find("form")[0];
                entryForm = $(entryForm);

                entryForm.on("submit", function (e) {
                    e.preventDefault();

                    const actionTypes = [];
                    entryForm.find(".action-type-row").each(function () {
                        const row = $(this);
                        const generate = row.find("#Generate").is(":checked");
                        if (generate) {
                            const actionTypeId = row.find("#ActionTypeId").val();
                            const baseDate = row.find(`#BaseDate_${actionTypeId}`).data("kendoDatePicker").value();
                            actionTypes.push({
                                Generate: true,
                                ActionTypeId: actionTypeId,
                                BaseDate: pageHelper.cpiDateFormatToSave(baseDate)
                            });
                        }
                    });

                    const payLoad = {
                        appId: entryForm.find("#AppId").val(),
                        actionTypes: actionTypes
                    }
                    $.post(entryForm.attr("action"), payLoad).done(function () {
                        dialog.modal("hide");
                    }).fail(function (error) {
                        pageHelper.showErrors(error.responseText);
                    });

                });

            }).fail(function (error) {
                pageHelper.showErrors(error.responseText);
            });
        }
    }


    getCopyToFamilyCasesParam() {
        const container = $("#patIDSCopyToFamilyDialog");
        return {
            appId: container.find("input[name='AppId']").val(),
            relatedCasesId: container.find("input[name='RelatedCasesId']").val(),
            relatedBy: container.find("input[name='GroupType']:checked").val()
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

    showAward = (e, grid) => {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        e.preventDefault();
        const parent = $("#" + e.delegateTarget.id).parent();
        const url = parent.data("url-award") + "?id=" + dataItem.ParentId + "&inventorid=" + dataItem.InventorID + "&inventor=" + dataItem.InventorDetail.Inventor + "&module=app";

        $.ajax({
            url: url,
            type: "Get",
            success: function (result) {
                const popupContainer = $(".site-content .popup").last();
                popupContainer.html(result);
                $("#awardDialog").modal('show');;
            },
            error: function (e) {
                pageHelper.showErrors(e.responseText);
            }
        });
    }

    openAwardMassUpdateEntry(grid, url, data, closeOnSave) {
        const self = this;

        $.ajax({
            url: url,
            data: data,
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $("#inventorAwardMassUpdateEntryDialog");

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

                            if (e.emailWorkflows) {
                                const promise = pageHelper.handleEmailWorkflow(e);
                                promise.then(() => {
                                });
                            }
                        }
                    }
                );
            },
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
                        const url = `${baseUrl}/Shared/SharePointGraph/GetDefaultWithThumbnailUrl?docLibrary=Patent`;
                        let driveId = "";

                        e.response.Data.forEach((r) => {
                            const recKey = { Id: r.AppId, RecKey: r.SharePointRecKey };
                            $.post(url, { docLibraryFolder: 'Application', driveId, recKey })
                                .done(function (result) {
                                    if (result && result.DriveId) {
                                        driveId = result.DriveId;
                                        const element = $(`#ca-sr-${result.Id}`);
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

}





