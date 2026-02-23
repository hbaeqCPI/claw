import Image from "../image";
import SearchPage from "../searchPage";

export default class DocumentVerificationPage extends SearchPage {

    constructor() {
        super();
        this.image = new Image();
        this.chkAllAct = false;
        this.chkAllNewDoc = false;
        this.chkAllDoc = false;
        this.chkAllComm = false;
        this.defaultResponsibleId = "";
        this.defaultSystems = "";
        this.newDocTabCriteria = null;
        this.docTabCriteria = null;
        this.actionTabCriteria = null;
        this.documentStorageOption = 0;
        this.inDocketingGroup = false;
        this.inReportingGroup = false;
        this.requestViewRecordIds = [];
        this.actionDocViewRecordIds = [];
        this.commDocViewRecordIds = [];
        this.allEntitiesState = "";
        this.allDdkState = "";
        this.allDkrState = "";
    }

    initializeDocumentVerification(documentStorageOption) {
        this.documentStorageOption = documentStorageOption;
        this.cpiStatusMessage = window.cpiStatusMessage;
        
        $(document).ready(() => {
            $(".container-crumbs").addClass("d-none");
            $("#documentVerification-tab").parent().removeClass("mt-2");

            //to fix weird issue with default selected tab
            setTimeout(function() { $('#documentVerificationSearchResults-RefineSearch ul a:first').trigger("click"); }, 250);

            //On tab changes
            $('#documentVerification-tab li').on('click', (e) => {
                e.preventDefault();
                const tab = $(e.currentTarget).find("a")[0].id;

                $("#OrphanDocSystem").addClass("d-none");
                $("#respDocketingFilter").addClass("d-none");
                $("#respReportingFilter").addClass("d-none");
                $("#docSentToClientFilter").addClass("d-none");
                $("#actionTypeFilter").addClass("d-none");
                $("#requestDocketFilter").addClass("d-none");
                $("#actionFilter").addClass("d-none");

                //hide attorney responsible and due date checkbox, only show on 3rd tab
                $("#attyFilterD").addClass("d-none");
                $("#attyFilterR").addClass("d-none");
                $("#attyFilterRD").addClass("d-none");

                //1st tab
                if (tab == "documentVerificationUpload-tab") {                   
                    $("#OrphanDocSystem").removeClass("d-none");
                    $("#respDocketingFilter").removeClass("d-none");
                    $("#respReportingFilter").removeClass("d-none");                    
                }
                //2nd tab
                else if (tab == "documentVerificationMainInfo-tab") {                    
                    $("#respDocketingFilter").removeClass("d-none");
                    $("#actionTypeFilter").removeClass("d-none");     
                    $("#requestDocketFilter").removeClass("d-none");
                    $("#attyFilterRD").removeClass("d-none");
                }
                //3rd tab
                else if (tab == "documentVerificationAction-tab") {
                    $("#actionTypeFilter").removeClass("d-none");                    
                    $("#respDocketingFilter").removeClass("d-none");
                    $("#respReportingFilter").removeClass("d-none"); 
                    $("#actionFilter").removeClass("d-none");
                    $("#attyFilterD").removeClass("d-none");
                    $("#attyFilterR").removeClass("d-none");                    
                }
                //4th tab
                else if (tab == "documentVerificationCommunication-tab") {
                    $("#respReportingFilter").removeClass("d-none");
                    $("#docSentToClientFilter").removeClass("d-none");
                }                
            });

            this.configureDocVerificationTabs();
            this.configureButtons();

            // Configure pagers
            this.configurePagers([
                { pager: "#documentVerificationSearchResults-Grid_pager", container: "#documentVerificationContainer" },
                { pager: "#documentVerificationNewDocGrid_pager", container: "#newDocVerificationContainer" },
                { pager: "#documentVerificationCommunicationGrid_pager", container: "#communicationContainer" },
                { pager: "#actionVerificationGrid_pager", container: "#actionVerificationContainer" }
            ]);

            //Add default resp docketing criteria        
            if (this.defaultResponsibleId > "" && this.defaultSystems > "") {
                var respDocketingMulti = $("#RespDocketings").data("kendoMultiSelect");
                if (respDocketingMulti) {
                    respDocketingMulti.dataSource.read({ systemType: this.defaultSystems });
                    respDocketingMulti.value([this.defaultResponsibleId]);
                    respDocketingMulti.trigger("change");
                    $("#respDocketingFilter").find("div.float-label").removeClass("inactive").addClass("active");                    
                }

                var respReportingMulti = $("#RespReportings").data("kendoMultiSelect");
                if (respReportingMulti) {
                    respReportingMulti.dataSource.read({ systemType: this.defaultSystems });
                    respReportingMulti.value([this.defaultResponsibleId]);
                    respReportingMulti.trigger("change");
                    $("#respReportingFilter").find("div.float-label").removeClass("inactive").addClass("active");
                }

                //multi select combo issue (slow in selecting values)
                var docGrid = $("#documentVerificationSearchResults-Grid").data("kendoListView");
                if (docGrid) {
                    setTimeout(function () {
                        docGrid.dataSource.read();
                    }, 500);
                }
            }

            //Set default tab
            if (this.inDocketingGroup) {
                $("#dvContentPage #documentVerificationMainInfo-tab").tab("show");                
            }
            else if (this.inReportingGroup) {
                $("#dvContentPage #documentVerificationCommunication-tab").tab("show");              
            }
        });
    }

    configurePagers(pagerMappings) {        
        pagerMappings.forEach(item => {
            const $pager = $(item.pager);
            const $container = $(item.container);

            if ($pager.length > 0 && $container.length > 0) {
                // Move the pager to the target container
                $container.append($pager);
            
                // Remove the small-screen class to show full pager controls
                $pager.removeClass("k-pager-sm");
            }
        });
    }

    configureButtons() {

        const updateResponsibleDocuments = (ids, docNames, notLinked, respType) => {
            const self = this;
            const url = $("body").data("base-url") + "/Shared/DocumentVerification/UpdateResponsible";

            const errorMsg = this.getMessageFromData("#dvContentPage", "label-responsible-error");
            if (notLinked) {
                this.cpiAlert.warning(errorMsg);
                return;
            }

            const warningMsg = this.getMessageFromData("#dvContentPage", "label-responsible-warning");
            const selectedIds = [...new Set(ids.split(";"))].join(";");
            if (!selectedIds) {
                this.cpiAlert.warning(warningMsg);
                return;
            }

            const selectedDocs = [...new Set(docNames.split("|"))].join("|");
            $.get(url, {
                    ids: selectedIds,
                    docNames: selectedDocs,
                    respType
                })
                .done((result) => {
                    $(".site-content .popup").empty();
                    const popupContainer = $(".site-content .popup").last();
                    popupContainer.html(result);
                    const dialog = $("#documentResponsibleDialog");
                    dialog.unbind();
                    dialog.modal("show");
                    self.initializeUpdateResponsible();
                })
                .fail((error) => {
                    pageHelper.showErrors(error);
                });
        };

        //1st tab - New Docs tab toolbar buttons
        $('#newDocVerificationHeaderContainer').on("click", ".add-doc", () => {
            if (this.documentStorageOption == 1) {
                this.editSPDocument('');
            }
            else if (this.documentStorageOption == 2) {
                this.editIMDocument();
            }
            else {
                this.editNewDocument(0);
            }            
        });
        $('#newDocVerificationHeaderContainer').on("click", ".select-all-record", (e) => {
            if (this.chkAllNewDoc) {
                this.chkAllNewDoc = false;
                $("input[name='chkNewDocLink']").prop('checked', false);
            }
            else {
                this.chkAllNewDoc = true;
                $("input[name='chkNewDocLink']").prop('checked', true);
            }
        });
        $('#newDocVerificationHeaderContainer').on("click", ".link-doc", (e) => {
            const self = this;

            let url = $("body").data("base-url") + "/Shared/DocumentVerification/SearchLink?";

            var ids = "";
            var docNames = "";
            var alreadyLinked = false;

            $("input[name='chkNewDocLink']:checked").each(function () {
                if (($(this).data('system-type') === '' || $(this).data('system-type') === 'O' || $(this).data('system-type') === null)) {
                    ids += this.value + "|";
                    docNames += $(this).data('doc-name') + "|";
                }
                else {
                    alreadyLinked = true;
                }                
            });

            const warningMsg = this.getMessageFromData("#dvContentPage", "label-link-warning");
            const errorMsg = this.getMessageFromData("#dvContentPage", "label-link-error");
                        
            if (alreadyLinked) {
                this.cpiAlert.warning(errorMsg);
                return;
            }

            const selectedIds = $.unique(ids.split("|")).join("|");
            if (!selectedIds || selectedIds == "") {                
                this.cpiAlert.warning(warningMsg);
                return;
            }

            var selectedDocs = $.unique(docNames.split("|")).join("|");
            $.get(url, { ids: ids, docNames: selectedDocs })
                .done((result) => {
                    //clear all existing hidden popups to avoid kendo id issue
                    $(".site-content .popup").empty();
                    const popupContainer = $(".site-content .popup").last();
                    popupContainer.html(result);
                    const dialog = $("#searchLinkDialog");
                    dialog.unbind();
                    dialog.modal("show");

                    self.initializeSearchLink();
                })
                .fail(function (error) {
                    pageHelper.showErrors(error);
                });
        });
        $('#newDocVerificationHeaderContainer').on("click", ".responsible-doc", (e) => {
            let ids = "";
            let docNames = "";
            let notLinked = false;

            $("input[name='chkNewDocLink']:checked").each(function() {
                const $this = $(this);
                const systemType = $this.data('system-type');

                if (systemType && systemType !== 'O' && systemType !== null) {
                    if ($this.data('doc-id') > 0) {
                        ids += `${systemType}|docid|${$this.data('doc-id')};`;
                    }
                    docNames += `${$this.data('doc-name')}|`;
                } else {
                    notLinked = true;
                }
            });

            updateResponsibleDocuments(ids, docNames, notLinked, null);
        });
        $('#newDocVerificationHeaderContainer').on("click", ".delete-doc", (e) => {            
            const self = this;

            const warningMsg = this.getMessageFromData("#dvContentPage", "label-delete-warning");
            const deleteMsg = this.getMessageFromData("#dvContentPage", "label-delete-msg");
            const deleteHeader = this.getMessageFromData("#dvContentPage", "label-delete-header");
            const verificationToken = $(this.refineSearchContainer).find("input[name=__RequestVerificationToken]").val();

            var ids = "";
            $("input[name='chkNewDocLink']:checked").each(function () {
                if (self.documentStorageOption == 2) 
                {
                    ids += $(this).data("drive-item-id") + "|";
                }                    
                else {
                    ids += this.value + "|";
                }                    
            });
            const selectedIds = $.unique(ids.split("|").filter(d => d));
            if (!selectedIds || selectedIds.length < 1 || ids === "") {
                this.cpiAlert.warning(warningMsg);
                return;
            }

            let url = $("body").data("base-url");
            if (self.documentStorageOption == 1) {
                url += "/Shared/SharePointGraph/DVDeleteDocuments?";
            }
            else if (self.documentStorageOption == 2) {
                url +=  "/iManageWork/DVDeleteDocuments?";
            }
            else {
                url += "/Shared/DocDocuments/DVDeleteDocuments?";
            }
            cpiConfirm.confirm(deleteHeader, deleteMsg, function () {
                $.ajax({
                    type: "POST",
                    url: url,
                    data: { ids: selectedIds },
                    dataType: "json",
                    headers: { "RequestVerificationToken": verificationToken },
                    success: function (result) {
                        if (result.success) {
                            self.refreshNewDocListViewDS();
                            self.refreshCriteriaDS();
                        }
                        cpiConfirm.close();
                    },
                    error: function (error) {    
                        cpiConfirm.close();
                        kendo.alert(error.message);
                    }
                });
            });
        });
        $('#newDocVerificationHeaderContainer').on("click", ".print-record", () => {
            this.printNewDoc();
        });
        $('#newDocVerificationHeaderContainer').on("click", ".excel-export", () => {
            this.excelExportNewDoc();
        });
        $('#newDocVerificationHeaderContainer').on("click", ".review-filters", (e) => {
            const self = this;
            let url = $("body").data("base-url") + "/Shared/DocumentVerification/ReviewFilters?";            
            $.get(url)
                .done((result) => {
                    //clear all existing hidden popups to avoid kendo id issue
                    $(".site-content .popup").empty();
                    const popupContainer = $(".site-content .popup").last();
                    popupContainer.html(result);
                })
                .fail(function (error) {
                    pageHelper.showErrors(error);
                });
        });   


        //2nd tab - Docs tab toolbar buttons
        $('#docVerificationHeaderContainer').on("click", ".print-record", () => {
            this.printDoc();
        });
        $('#docVerificationHeaderContainer').on("click", ".excel-export", () => {
            this.excelExportDoc();
        });
        $('#docVerificationHeaderContainer').on("click", ".select-all-record", (e) => {
            if (this.chkAllDoc) {
                this.chkAllDoc = false;
                $("input[name='chkDocLink']").prop('checked', false);
            }
            else {
                this.chkAllDoc = true;
                $("input[name='chkDocLink']").prop('checked', true);
            }
        });
        $('#docVerificationHeaderContainer').on("click", ".delete-doc", (e) => {            
            const self = this;

            const warningMsg = this.getMessageFromData("#dvContentPage", "label-delete-warning");
            const deleteMsg = this.getMessageFromData("#dvContentPage", "label-delete-msg");
            const deleteHeader = this.getMessageFromData("#dvContentPage", "label-delete-header");

            var ids = "";
            $("input[name='chkDocLink']:checked").each(function () {
                let systemType = $(this).data("system-type");
                ids += systemType + "|" + this.value + ";";
            });
            const selectedIds = $.unique(ids.split(";").filter(d => d));
            if (!selectedIds || selectedIds.length < 1 || ids === "") {
                this.cpiAlert.warning(warningMsg);
                return;
            }

            let url = $("body").data("base-url") + "/Shared/DocumentVerification/DeleteLinkedAction?";
            cpiConfirm.confirm(deleteHeader, deleteMsg, function () {
                $.ajax({
                    type: "POST",
                    url: url,
                    data: { keyIds: selectedIds },
                    dataType: "json",
                    success: function (response) {
                        if (response.success)
                            self.refreshDocListView();

                        cpiConfirm.close();
                    },
                    error: function (response) {
                        cpiConfirm.close();
                        kendo.alert(response.message);
                    }
                });
            });
        });
        $('#docVerificationHeaderContainer').on("click", ".responsible-doc", (e) => {
            let ids = "";
            let docNames = "";
            let notLinked = false;

            $("input[name='chkDocLink']:checked").each(function() {
                const $this = $(this);
                const systemType = $this.data('system-type');
                const selectedValue = this.value;

                if (systemType && systemType !== 'O' && systemType !== null) {
                    if (selectedValue.indexOf("verifyid") !== -1 && $this.data('doc-id') > 0) {
                        ids += `${systemType}|docid|${$this.data('doc-id')};`;
                    } else {
                        ids += `${systemType}|${selectedValue};`;
                    }
                    docNames += `${$this.data('doc-name')}|`;
                } else {
                    notLinked = true;
                }
            });

            updateResponsibleDocuments(ids, docNames, notLinked, 0);
        });
        $('#docVerificationHeaderContainer').on("click", ".productivity-report", (e) => {
            const self = this;
            let url = $("body").data("base-url") + "/Shared/DocumentVerification/ProductivityReports";
            $.get(url)
                .done((result) => {
                    //clear all existing hidden popups to avoid kendo id issue
                    $(".site-content .popup").empty();
                    const popupContainer = $(".site-content .popup").last();
                    popupContainer.html(result);
                    const dialog = $("#productivityReportsDialog");
                    dialog.unbind();
                    dialog.modal("show");
                })
                .fail(function (error) {
                    pageHelper.showErrors(error);
                });
        });
        $('#docVerificationHeaderContainer').on("click", ".quick-docket", () => {
            let url = $("body").data("base-url") + "/Shared/QuickDocket/index";
            //pageHelper.openLink(`${url}`);
            window.open(url, '_blank');
        });


        //3rd tab - Actions tab toolbar buttons
        $('#actionVerificationContainer').on("click", ".print-record", () => {
            this.printActionDoc();
        });
        $('#actionVerificationContainer').on("click", ".excel-export", () => {
            this.excelExportActionDoc();
        });
        $("#actionVerificationContainer").on("click", ".act-verify-icon", (e) => {
            e.preventDefault();
            const el = $(e.currentTarget);
            const id = el.data("u-id");
            const verifyRow = el.closest(".family-header").next();

            verifyRow.find(`#verify_edit_${id}`).removeClass("d-none");

            var dateVerifiedEntry = verifyRow.find(".verified-date-entry");
            if (dateVerifiedEntry) {
                var dateVerified = $(dateVerifiedEntry).data("value");

                if ($(`#VerifiedDate_${id}`).data("kendoDatePicker") !== undefined) {
                    $(`#VerifiedDate_${id}`).data("kendoDatePicker").destroy(); 
                }
                $(`#VerifiedDate_${id}`).kendoDatePicker({                
                    "format": "dd-MMM-yyyy",
                    "max": new Date(9999, 11, 31, 0, 0, 0, 0),
                    "min": new Date(1800, 0, 1, 0, 0, 0, 0),
                    "parseFormats": ["{0:dd-MMM-yyyy}", "d", "M d yyyy", "M/d/yyyy", "M-d-yyyy", "M.d.yyyy", "MMMM d, yyyy", "MMMM d yyyy", "d MMMM, yyyy", "d MMMM yyyy", "MMM d, yyyy", "MMM. d, yyyy", "M d yy", "M/d/yy", "M-d-yy", "M.d.yy", "M d", "M/d", "M-d", "M.d", "yyyy-M-d"] 
                });
                if (dateVerified) {
                    $(`#VerifiedDate_${id}`).data("kendoDatePicker").value(new Date(dateVerified));
                }
            }
            
            el.hide();
        });
        $("#actionVerificationContainer").on("click", ".save-verify-icon", (e) => {
            const self = this;
            const tickler = $('#actionVerificationContainer').find(".tickler");
            const parent = $(e.currentTarget).parents(".edit-verify");
            const id = parent.attr("id").split("_")[2];
            const actId = $(`#ActId_${id}`);
            const system = $(`#System_${id}`);
            const verifiedDate = $(`#VerifiedDate_${id}`).data("kendoDatePicker");
            
            let selectedDate = verifiedDate.value();
            if (selectedDate) {
                selectedDate = selectedDate.toLocaleDateString();
            }

            let verifiedDateUrl = $("body").data("base-url") + "/Shared/DocumentVerification/SaveAllActionVerify";
            const data = {
                verifiedDate: selectedDate,
                selectedIds: system.val() + actId.val() + "|",
                __RequestVerificationToken: tickler.find("input[name=__RequestVerificationToken]").val()
            };    
            cpiLoadingSpinner.show();
            $.post(verifiedDateUrl, data)
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    self.hideEditVerify(e);
                    self.refreshActListView();
                    if (result) {
                        pageHelper.showSuccess(result.message);                        
                    }
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error);
                });

        });
        $("#actionVerificationContainer").on("click", ".set-current-date-verify-icon", (e) => {
            const parent = $(e.currentTarget).parents(".edit-verify");
            const id = parent.attr("id").split("_")[2];
            const verifiedDatePicker = $(`#VerifiedDate_${id}`).data("kendoDatePicker");
            if (verifiedDatePicker) {
                verifiedDatePicker.value(new Date());
            }
        });
        $("#actionVerificationContainer").on("click", ".cancel-verify-icon", (e) => {
            this.hideEditVerify(e);
        });
        $('#actionVerificationContainer').on("click", ".select-all-record", (e) => {
            if (this.chkAllAct) {
                this.chkAllAct = false;
                $("input[name='chkActVerify']").prop('checked', false);
            }
            else {
                this.chkAllAct = true;
                $("input[name='chkActVerify']").prop('checked', true);
            }
        });
        $('#actionVerificationContainer').on("click", ".verify-record", (e) => {
            this.showVerifyEditor(e);
        });


        //4th tab - Communications toolbar buttons
        $('#communicationHeaderContainer').on("click", ".print-record", () => {
            this.printCommunicationDoc();
        });
        $('#communicationHeaderContainer').on("click", ".excel-export", () => {
            this.excelExportCommunicationDoc();
        });
        $('#communicationHeaderContainer').on("click", ".select-all-record", (e) => {
            if (this.chkAllComm) {
                this.chkAllComm = false;
                $("input[name='chkCommunicationLink']").prop('checked', false);
            }
            else {
                this.chkAllComm = true;
                $("input[name='chkCommunicationLink']").prop('checked', true);
            }
        });       
        $('#communicationHeaderContainer').on("click", ".responsible-doc", (e) => {
            let ids = "";
            let docNames = "";
            let notLinked = false;

            $("input[name='chkCommunicationLink']:checked").each(function() {
                const $this = $(this);
                const systemType = $this.data('system-type');
                const selectedValue = this.value;

                if (systemType && systemType !== 'O' && systemType !== null) {
                    ids += `${systemType}|docid|${selectedValue};`;
                    docNames += `${$this.data('doc-name')}|`;
                } else {
                    notLinked = true;
                }
            });

            updateResponsibleDocuments(ids, docNames, notLinked, 1);
        });
    }

    configureDocVerificationTabs() {

        //Configure tab 1
        this.configureTab1Sort();

        //Configure tab 2
        this.configureTab2Sort();

        $("#documentVerificationContainer").off("click", ".entitiesHeader").on("click", ".entitiesHeader", (e) => {
            e.preventDefault();
            this.toggleAllEntities($(e.currentTarget));
        });
        $("#documentVerificationContainer").off("click", ".entities-container").on("click", ".entities-container", (e) => {
            this.toggleEntities(e.currentTarget);
        });

        $("#documentVerificationContainer").off('click', '.ddkHeader').on("click", ".ddkHeader", (e) => {
            e.preventDefault();
            this.toggleAllDeDocket($(e.currentTarget));
        });

        $("#documentVerificationContainer").off("click", ".ddk-container").on("click", ".ddk-container", (e) => {
            this.toggleDeDocket(e.currentTarget);
        });

        $("#documentVerificationContainer").off('click', '.dkrHeader').on("click", ".dkrHeader", (e) => {
            e.preventDefault();
            this.toggleAllRequestDocket($(e.currentTarget));
        });

        $("#documentVerificationContainer").off("click", ".dkr-container").on("click", ".dkr-container", (e) => {
            this.toggleRequestDocket(e.currentTarget);
        });

        //Configure tab 3
        this.configureTab3Sort();

        //Configure tab 4
        this.configureTab4Sort();        
    }


    //Communications tab - START
    configureTab4Sort() {
        const containers = $(".commHeaderContainer");

        // Add initial sort icon
        const uploadDateCol = containers.find('[data-id="Comm_UploadedDate"]');
        uploadDateCol.append('<span class="k-icon k-i-sort-desc-sm"></span>');

        const self = this;
        // Bind click events using the class
        containers.find("a").on('click', function (e) {
            e.preventDefault();
            self.addCommunicationSortIcon($(this));
        });

        // Sticky Scroll and Resize Logic
        $(window).on('scroll resize', () => {
            this.syncStickyHeader("#commHeaderContainer_real", "#commHeaderContainer_fake");
        });
    }

    addCommunicationSortIcon(event) {
        const sortAscIcon = "k-icon k-i-sort-asc-sm";
        const sortDescIcon = "k-icon k-i-sort-desc-sm";
        let sortOrder = "ASC";
        const sortColumn = event.data("id"); 
    
        // Target icons in both headers simultaneously
        const allRelevantIcons = $(`.commHeaderContainer a[data-id="${sortColumn}"] span`);

        this.hideCommunicationSortIcons(sortColumn);

        if (allRelevantIcons.length === 0) {
            $(`.commHeaderContainer a[data-id="${sortColumn}"]`).append(`<span class="${sortAscIcon}"></span>`);
        } else {
            if (allRelevantIcons.hasClass("k-i-sort-asc-sm")) {
                allRelevantIcons.removeClass("k-i-sort-asc-sm").addClass("k-i-sort-desc-sm");
                sortOrder = "DESC";
            } else {
                allRelevantIcons.removeClass("k-i-sort-desc-sm").addClass("k-i-sort-asc-sm");
                sortOrder = "ASC";
            }
        }
        this.refreshCommunicationListView(sortColumn, sortOrder);
    }

    hideCommunicationSortIcons(columnClicked) {
        $(".commHeaderContainer a").each(function () {
            const column = $(this).data("id");
            if (columnClicked !== column) {
                $(this).find("span.k-icon").remove();
            }
        });
    }

    refreshCommunicationListView(sortColumn = "UploadedDate", sortOrder = "DESC") {
        sortColumn = sortColumn.replace("Comm_", "");
        
        const dataSource = $('#documentVerificationCommunicationGrid').data('kendoListView').dataSource;
        const sortDescriptor = [{ field: sortColumn, dir: sortOrder.toLowerCase() }];
        var container = $(this.refineSearchContainer);
        container.find("#SortCol").val(sortColumn);
        container.find("#SortOrder").val(sortOrder);
        const query = {
            sort: sortDescriptor,
            page: dataSource.page(),
            pageSize: dataSource.pageSize()
        };
        dataSource.query(query);
    }
    
    refreshCommunicationListViewDS() {
        const gridDS = $('#documentVerificationCommunicationGrid').data('kendoListView').dataSource;        
        gridDS.read();        
    }
    
    communicationDataBound(e) {
        $("#documentVerificationCommunicationGrid tr").each(function (index) {
            if (index % 2) {
                $(this).addClass("k-alt");
            }
        });

        //refresh count display
        const dataCount = e.sender.dataSource.total();
        var countSpan = $("#documentVerification-tab").find(".communication-count");
        if (countSpan) {
            if (dataCount && dataCount > 0) {
                countSpan.text("(" + dataCount + ")");
            }                
            else {
                countSpan.text("");
            }                
        }
    }   

    communicationRequestEnd = (e) => {        
        if (e && e.response && e.response.Ids) {
            this.commDocViewRecordIds = [];
            this.commDocViewRecordIds = e.response.Ids;
        }
    }

    commDocumentNavigateHandler = (id) => {
        if (this.commDocViewRecordIds && this.commDocViewRecordIds[id-1]) {
            var idArray = this.commDocViewRecordIds[id-1].split("|");
            this.viewCommDocument(idArray[0], idArray[1], idArray[2], idArray[3]);
        }        
    }

    viewCommDocument(system, docId, driveItemId, parentId) {
        const baseUrl = $("body").data("base-url");
        let url = baseUrl + "/Shared/DocumentVerification/CommDocumentZoom?";
        url = url + "systemTypeCode=" + system + "&docId=" + docId + "&driveItemId=" + driveItemId + "&parentId=" + parentId;

        const self = this;
        var docViewRecordId = system + "|" + docId + "|" + driveItemId + "|" + parentId;
        docViewRecordId = docViewRecordId.replace("null", "");

       let retry = 0;
        openCommDocumentPopup();

        function openCommDocumentPopup() {
            cpiLoadingSpinner.show();
            $.get(url)
            .done((result) => {
                cpiLoadingSpinner.hide();

                const popupContainer = $(".site-content .popup");
                const commDocZoomModalDialog = popupContainer.find("#docVerificationCommDocZoomDialog .modal-content");            
                if (commDocZoomModalDialog && commDocZoomModalDialog.length > 0) {
                    const $tempElement = $("<div>").append(result);                    
                    kendo.destroy(commDocZoomModalDialog);
                    commDocZoomModalDialog.html($tempElement.find("#docVerificationCommDocZoomDialog .modal-content").html());                
                }
                else {
                    popupContainer.empty();
                    popupContainer.html(result);
                    popupContainer.find("#docVerificationCommDocZoomDialog").modal("show");
                }
                
                const docViewerContainer = popupContainer.find("#docVerificationCommDocViewerContainer");
                if (docViewerContainer && docViewerContainer.length > 0) {
                    const docViewerUrl = docViewerContainer.data("viewer-url");
                    $.ajax({
                        url: docViewerUrl,
                        dataType: "html",
                        cache: false,
                        beforeSend: function () { },
                        success: function (docResult) {
                            docViewerContainer.empty();
                            docViewerContainer.html(docResult);
                        },
                        error: function (e) {
                            pageHelper.showErrors(e);
                        }
                    });
                }

                if (self.commDocViewRecordIds && self.commDocViewRecordIds.findIndex(d => d.includes(docViewRecordId)) > -1) {
                    $("#docVerificationCommDocZoomDialogPager").cpiRecordNavigator({
                        recordIds: Array.from({ length: self.commDocViewRecordIds.length }, (v, i) => i + 1),
                        currentId: self.commDocViewRecordIds.findIndex(d => d.includes(docViewRecordId)) + 1,
                        navigateHandler: self.commDocumentNavigateHandler
                    });
                }
            
                popupContainer.find("#docVerificationCommDocZoomDialog").on("hidden.bs.modal", function () {
                    kendo.destroy(popupContainer.find("#docVerificationCommDocZoomDialog .modal-content"));
                    popupContainer.empty();
                    self.refreshCommunicationListViewDS();
                });

                var commDocQEContainer = popupContainer.find("#docVerificationCommDocQEContainer");
                if (commDocQEContainer) {                    
                    var jsonQEParams = $(commDocQEContainer).data("qe-params");
                    if (jsonQEParams) {
                        //Use timeout to delay to fix scroll issue
                        setTimeout(() => {
                            cpiLoadingSpinner.show();
                            $.get(baseUrl + "/Shared/QuickEmail/Email?jsonQEParam=" + JSON.stringify(jsonQEParams))
                            .done(function (result) {
                                commDocQEContainer.html(result);
                                cpiLoadingSpinner.hide();
                            }).fail(function (e) {
                                cpiLoadingSpinner.hide();
                                pageHelper.showErrors(e);
                            });
                        }, 50);                        
                    }
                }

            })
            .fail((e) => {    
                cpiLoadingSpinner.hide();
                if (e.status == 401 && retry < 3) {
                    retry++;
                    const baseUrl = $("body").data("base-url");
                    const url = `${baseUrl}/Graph/SharePoint`;

                    sharePointGraphHelper.getGraphToken(url, () => {
                        openCommDocumentPopup();
                    });
                }
                else {
                    pageHelper.showErrors(e.responseText);
                }
            });
        }         
    }

    commDocumentQENavigateHandler = (id) => {
        if (this.commDocViewRecordIds && this.commDocViewRecordIds[id-1]) {
            var idArray = this.commDocViewRecordIds[id-1].split("|");                 
            this.viewCommDocumentQELog(idArray[0], idArray[1], idArray[2], idArray[3]);
        }        
    }

    viewCommDocumentQELog(system, docId, driveItemId, parentId) {
        const baseUrl = $("body").data("base-url");
        let url = baseUrl + "/Shared/DocumentVerification/CommDocumentZoom?";
        url = url + "systemTypeCode=" + system + "&docId=" + docId + "&driveItemId=" + driveItemId + "&parentId=" + parentId + "&isQELogLookup=true";

        const self = this;
        var docViewRecordId = system + "|" + docId + "|" + driveItemId + "|" + parentId;
        docViewRecordId = docViewRecordId.replace("null", "");

        let retry = 0;
        openCommDocumentPopup();

        function openCommDocumentPopup() {
            cpiLoadingSpinner.show();
            $.get(url)
            .done((result) => {
                cpiLoadingSpinner.hide();
                
                const popupContainer = $(".site-content .popup");                
                const commDocQELogModalDialog = popupContainer.find("#docVerificationCommDocQELogDialog .modal-content");            
                if (commDocQELogModalDialog && commDocQELogModalDialog.length > 0) {
                    const $tempElement = $("<div>").append(result);                    
                    kendo.destroy(commDocQELogModalDialog);
                    commDocQELogModalDialog.html($tempElement.find("#docVerificationCommDocQELogDialog .modal-content").html());                
                }
                else {
                    popupContainer.empty();
                    popupContainer.html(result);
                    popupContainer.find("#docVerificationCommDocQELogDialog").modal("show");
                }
            
                const docViewerContainer = popupContainer.find("#docVerificationCommDocQELogViewerContainer");
                if (docViewerContainer && docViewerContainer.length > 0) {
                    const docViewerUrl = docViewerContainer.data("viewer-url");
                    $.ajax({
                        url: docViewerUrl,
                        dataType: "html",
                        cache: false,
                        beforeSend: function () { },
                        success: function (docResult) {
                            docViewerContainer.empty();
                            docViewerContainer.html(docResult);
                        },
                        error: function (e) {
                            pageHelper.showErrors(e);
                        }
                    });
                }

                if (self.commDocViewRecordIds && self.commDocViewRecordIds.findIndex(d => d.includes(docViewRecordId)) > -1) {
                    $("#docVerificationCommDocQELogDialogPager").cpiRecordNavigator({
                        recordIds: Array.from({ length: self.commDocViewRecordIds.length }, (v, i) => i + 1),
                        currentId: self.commDocViewRecordIds.findIndex(d => d.includes(docViewRecordId)) + 1,
                        navigateHandler: self.commDocumentQENavigateHandler
                    });
                }

                popupContainer.find("#docVerificationCommDocQELogDialog").on("hidden.bs.modal", function () {
                    kendo.destroy(popupContainer.find("#docVerificationCommDocQELogDialog .modal-content"));
                    popupContainer.empty();
                });
            })
            .fail((e) => {    
                cpiLoadingSpinner.hide();
                if (e.status == 401 && retry < 3) {
                    retry++;
                    const baseUrl = $("body").data("base-url");
                    const url = `${baseUrl}/Graph/SharePoint`;

                    sharePointGraphHelper.getGraphToken(url, () => {
                        openCommDocumentPopup();
                    });
                }
                else {
                    pageHelper.showErrors(e.responseText);
                }
            });
        }   
    }  
    //Communications tab - END


    //Actions tab - START   
    hideEditVerify(e) {
        e.preventDefault();
        const parent = $(e.currentTarget).parents(".edit-verify");
        parent.addClass("d-none");
        const id = parent.attr("id").split("_")[2];
        $(`#verify_icon_${id}`).show();
        //if (action === "save") {            
        //    $(`#form_${id}`).find("[name='DueDateRemarks']").val(remarks);
        //}
    }

    configureTab3Sort() {
        const containers = $(".dvActionHeader");

        // Default Icon for Due Date
        const dueDateCols = containers.find('[data-id="Act_DueDate"]');
        dueDateCols.append('<span class="k-icon k-i-sort-asc-sm"></span>');

        const self = this;
        // Click event for all links in both headers
        containers.find("a").on('click', function (e) {
            e.preventDefault();
            self.addActSortIcon($(this));
        });

        // Sticky Scroll Logic
        $(window).on('scroll resize', () => {
            this.syncStickyHeader("#dvActionHeaderContainer_real", "#dvActionHeaderContainer_fake");
        });
    }

    addActSortIcon(event) {        
        const sortAscIcon = "k-icon k-i-sort-asc-sm";
        const sortDescIcon = "k-icon k-i-sort-desc-sm";
        let sortOrder = "ASC";
    
        // Get column key from data-id
        const sortColumn = event.data("id"); 
    
        // Select icon spans in BOTH headers
        const allRelevantIcons = $(`.dvActionHeader a[data-id="${sortColumn}"] span`);

        this.hideActSortIcons(sortColumn);

        if (allRelevantIcons.length === 0) {
            $(`.dvActionHeader a[data-id="${sortColumn}"]`).append(`<span class="${sortAscIcon}"></span>`);
        } else {
            if (allRelevantIcons.hasClass("k-i-sort-asc-sm")) {
                allRelevantIcons.removeClass("k-i-sort-asc-sm").addClass("k-i-sort-desc-sm");
                sortOrder = "DESC";
            } else {
                allRelevantIcons.removeClass("k-i-sort-desc-sm").addClass("k-i-sort-asc-sm");
                sortOrder = "ASC";
            }
        }
        this.refreshActListView(sortColumn, sortOrder);
    }

    hideActSortIcons(columnClicked) {
        $(".dvActionHeader a").each(function () {
            const column = $(this).data("id");
            if (columnClicked !== column) {
                $(this).find("span.k-icon").remove();
            }
        });
    }

    refreshActListView(sortColumn = "DueDate", sortOrder = "ASC") {
        const fieldName = sortColumn.replace("Act_", "");
        const listView = $('#actionVerificationGrid').data('kendoListView');
        if (!listView) return;

        const dataSource = listView.dataSource;
    
        // Update Hidden Inputs for Search Persistence
        const container = $(this.refineSearchContainer);
        container.find("#SortCol").val(fieldName);
        container.find("#SortOrder").val(sortOrder);

        dataSource.query({
            sort: [{ field: fieldName, dir: sortOrder.toLowerCase() }],
            page: dataSource.page(),
            pageSize: dataSource.pageSize()
        });
    }

    actionDataBound = (e) => {        
        const data = e.sender.dataSource.data(); 
        $(".family-wrap").remove();
        if (data.length > 0) {
            const listView = e.sender.element;
            const rows = listView.find(".tickler-row");
            let family = "";

            for (var i = 0; i < rows.length; i++) {
                const row = rows[i];
                const rowData = data[i];

                //if (i % 2)
                //    $(row).addClass("k-alt");

                if (family !== rowData.FamilyId) {
                    family = rowData.FamilyId;
                    listView.append(`<div class="family-wrap" data-familyid="${family}"><div class="family-detail"></div></div>`);
                    $(row).appendTo($(`div[data-familyid='${family}'] .family-detail`));
                }
                else {
                    $(row).appendTo($(`div[data-familyid='${family}'] .family-detail`));
                    //$(row).removeClass("k-alt");
                }                
            } 
        }

        //refresh count display
        const baseUrl = $("body").data("base-url");
        let url = baseUrl + "/Shared/DocumentVerification/ActionVerification_Read";
        let dataTemp = this.getCriteria(this.refineSearchContainer);
        dataTemp["sort"] = "DueDate-asc";       
         $.ajax({
            type: "POST",
            url: url,
            data: dataTemp,
            dataType: "json",
            success: function (result) {
                if (result && result.Data) {                 
                    setActCountSpan(new Set(result.Data.map(obj => obj.ActId)).size);
                }
                else {
                    setActCountSpan(0);
                }
            },
            error: function (response) {
                setActCountSpan(0);
            }
        });

        function setActCountSpan(count) {
             var countSpan = $("#documentVerification-tab").find(".dockets-verification-count");
            if (countSpan) {
                if (count && count > 0) {
                    countSpan.text("(" + count + ")");
                }                
                else {
                    countSpan.text("");
                }               
            }
        }
       
    }

    actionRequestEnd = (e) => {        
        if (e && e.response && e.response.Ids) {
            this.actionDocViewRecordIds = [];
            this.actionDocViewRecordIds = e.response.Ids;
        }
    }

    showVerifyEditor = (e) => {
        const self = this;
        const tickler = $('#actionVerificationContainer').find(".tickler");
        const url = tickler.data("save-verify-url");
        const validationError = tickler.data("validation-error");

        const popUpContent = `<div class="form-row justify-content-center"><div class="col-8">        
            <div class="form-group float-label">
                <label for="VerifiedDate_Popup">${tickler.data("label-verify-date")}</label>
                <input id="VerifiedDate_Popup" title="VerifiedDate_Popup" required/>    
                <span class="field-validation-error" style="display:none;" data-valmsg-for="VerifiedDate_Popup"><span id="VerifiedDate_Popup-error">${validationError}</span></span>
                <script>
	                $(document).ready(function() {               
                        $("#VerifiedDate_Popup").kendoDatePicker({value: new Date(), format: "dd-MMM-yyyy"});
                    });
                </script>
            </div></div>`;

        var tempIds = "";
        $("input[name='chkActVerify']:checked").each(function () {
            tempIds += this.value + "|";
        });

        const selectedIds = $.unique(tempIds.split("|")).join("|");
        if (!selectedIds || selectedIds == "") {
            this.cpiAlert.warning(tickler.data("label-verify-warning"));
            return;
        }

        if (url) {
            cpiConfirm.save(tickler.data("label-verify-title"), popUpContent,
                function () {
                    const verifiedDatePicker = $("#VerifiedDate_Popup").data("kendoDatePicker");
                    const selectedDate = verifiedDatePicker.value();
                    const reason = $(`input[id=VerifiedDate_Popup]`);
                    const error = $(`#VerifiedDate_Popup-error`).closest(".field-validation-error");

                    if (selectedDate) {
                        error.hide();
                        reason.removeClass("input-validation-error");

                        cpiLoadingSpinner.show();

                        const data = {
                            verifiedDate: selectedDate.toLocaleDateString(),
                            selectedIds: selectedIds,
                            __RequestVerificationToken: tickler.find("input[name=__RequestVerificationToken]").val()
                        };

                        $.post(url, data)
                            .done(function (result) {
                                cpiLoadingSpinner.hide();
                                self.refreshActListView();
                            })
                            .fail((error) => {
                                cpiLoadingSpinner.hide();
                                pageHelper.showErrors(error)
                            });
                    }
                    else {
                        error.show();
                        reason.addClass("input-validation-error");
                        reason.focus();
                        throw validationError;
                    }
                }
            );
        }
    }

    actDocumentNavigateHandler = (id) => {
        if (this.actionDocViewRecordIds && this.actionDocViewRecordIds[id-1]) {
            var idArray = this.actionDocViewRecordIds[id-1].split("|");
            this.viewActDocument(idArray[0], idArray[1], idArray[2], idArray[3], idArray[4]);
        }        
    }

    viewActDocument(system, docFileName, parentId, actId, docId) {
        const baseUrl = $("body").data("base-url");
        let url = baseUrl + "/Shared/DocumentVerification/ActionDocumentZoom?";
        url = url + "systemTypeCode=" + system + "&docFileName=" + docFileName + "&parentId=" + parentId + "&actId=" + actId + "&docId=" + docId;

        const self = this;
        const docViewRecordId = system + "|" + docFileName + "|" + parentId + "|" + actId + "|" + docId;

        let retry = 0;
        openActDocumentPopup();

        function openActDocumentPopup() {
            cpiLoadingSpinner.show();
            $.get(url)
            .done((result) => {
                cpiLoadingSpinner.hide();

                const popupContainer = $(".site-content .popup");
                popupContainer.unbind();

                const actDocZoomDialog = popupContainer.find("#docVerificationActDocZoomDialog .modal-content");            
                if (actDocZoomDialog && actDocZoomDialog.length > 0) {
                    const $tempElement = $("<div>").append(result);                    
                    kendo.destroy(actDocZoomDialog);
                    actDocZoomDialog.html($tempElement.find("#docVerificationActDocZoomDialog .modal-content").html());                
                }
                else {
                    popupContainer.empty();
                    popupContainer.html(result);
                    popupContainer.find("#docVerificationActDocZoomDialog").modal("show");
                }                
            
                //loading document
                const docViewerContainer = popupContainer.find("#docVerificationActDocViewerContainer");
                if (docViewerContainer && docViewerContainer.length > 0) {
                    const docViewerUrl = docViewerContainer.data("viewer-url");
                    if (docViewerUrl > "") {
                        $.ajax({
                            url: docViewerUrl,
                            dataType: "html",
                            cache: false,
                            beforeSend: function () { },
                            success: function (docResult) {
                                docViewerContainer.empty();
                                docViewerContainer.html(docResult);
                            },
                            error: function (e) {
                                pageHelper.showErrors(e);
                            }
                        });
                    }                    
                }
                
                //navigator
                if (self.actionDocViewRecordIds && self.actionDocViewRecordIds.findIndex(d => d.includes(docViewRecordId)) > -1) {
                    $("#docVerificationActDocZoomDialogPager").cpiRecordNavigator({
                        recordIds: Array.from({ length: self.actionDocViewRecordIds.length }, (v, i) => i + 1),
                        currentId: self.actionDocViewRecordIds.findIndex(d => d.includes(docViewRecordId)) + 1,
                        navigateHandler: self.actDocumentNavigateHandler
                    });
                }

                //refresh grid on popup close
                popupContainer.find("#docVerificationActDocZoomDialog").on("hidden.bs.modal", function () {
                    kendo.destroy(popupContainer.find("#docVerificationActDocZoomDialog .modal-content"));
                    popupContainer.empty();
                    self.refreshActListView();
                });

                //set current date for Action Verified Date
                $("#docVerificationActDocZoomDialog").off("click", ".verified-date-default");
                $("#docVerificationActDocZoomDialog").on("click", ".verified-date-default", function () {
                    const parent = $(this).closest(".verified-date");
                    const verifiedDatePicker = parent.find("input[name='DateVerified']").data("kendoDatePicker");
                    const currentDate = new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate());
                    verifiedDatePicker.value(currentDate);
                    verifiedDatePicker.trigger("change");
                    parent.find("div.float-label").removeClass("inactive").addClass("active");
                });

                var actDocZoomPopupSummary = popupContainer.find("#actDocZoomPopupSummary");
                actDocZoomPopupSummary.find("button.close").on("click", function () {
                    $(actDocZoomPopupSummary).slideUp();
                });

                //save button
                $("#docVerificationActDocZoomDialog").off("click", ".save-verified-date");
                $("#docVerificationActDocZoomDialog").on("click", ".save-verified-date", function () {                    
                    const actionVerifiedDateKey = popupContainer.find("input[id='actionVerifiedDateKey']");                    
                    const verifiedDatePicker = popupContainer.find("input[name='DateVerified']").data("kendoDatePicker");
                    const entryForm = $(popupContainer.find("#docVerificationActDocZoomDialog").find("form")[0]);

                    $.validator.unobtrusive.parse(entryForm);
                    if (entryForm.data("validator") !== undefined) {
                        entryForm.data("validator").settings.ignore = "";
                    }
                    if (!entryForm.valid()) {
                        return;
                    }                    
                    
                    cpiLoadingSpinner.show();
                    let selectedDate = verifiedDatePicker.value();
                    if (selectedDate) {
                        selectedDate = selectedDate.toLocaleDateString();
                    }
                    
                    let verifiedDateUrl = baseUrl + "/Shared/DocumentVerification/SaveAllActionVerify";
                    const data = {
                        verifiedDate: selectedDate,
                        selectedIds: actionVerifiedDateKey.val(),
                        __RequestVerificationToken: entryForm.find("input[name=__RequestVerificationToken]").val()
                    };                    
                    $.post(verifiedDateUrl, data)
                        .done(function (result) {
                            cpiLoadingSpinner.hide();
                            if (result) {
                                if (result.userName) {
                                    const actionVerifiedBy = popupContainer.find("input[name='VerifiedBy']");
                                    if (actionVerifiedBy) {
                                        actionVerifiedBy.val(result.userName);
                                    }
                                }
                                if (result.message) {
                                    $(actDocZoomPopupSummary).removeClass("alert-danger");
                                    $(actDocZoomPopupSummary).addClass("alert-success");
                                    actDocZoomPopupSummary.find("span.message").html(result.message);
                                    $(actDocZoomPopupSummary).show(function () {
                                        $(actDocZoomPopupSummary).delay(5000).slideUp();
                                    });
                                }
                                else if (result.errorMessage) {
                                    $(actDocZoomPopupSummary).addClass("alert-danger");
                                    $(actDocZoomPopupSummary).removeClass("alert-success");
                                    actDocZoomPopupSummary.find("span.message").html(pageHelper.getErrorMessage(result));        
                                    $(actDocZoomPopupSummary).show();
                                }
                            }
                        })
                        .fail((error) => {
                            cpiLoadingSpinner.hide();
                            pageHelper.showErrors(error);
                        });
                });
            })
            .fail((e) => {    
                cpiLoadingSpinner.hide();
                if (e.status == 401 && retry < 3) {
                    retry++;
                    const baseUrl = $("body").data("base-url");
                    const url = `${baseUrl}/Graph/SharePoint`;

                    sharePointGraphHelper.getGraphToken(url, () => {
                        openActDocumentPopup();
                    });
                }
                else {
                    pageHelper.showErrors(e.responseText);
                }
            });
        }        
    }
    //Actions tab - END


    //Documents tab - START
    configureTab2Sort() {
        const containers = $(".dvHeaderContainer");

        // Add default icon to both headers
        const uploadDateCols = containers.find('[data-id="Doc_UploadedDate"]');
        uploadDateCols.append('<span class="k-icon k-i-sort-asc-sm"></span>');

        const self = this;
        // Bind click to all links in both headers
        containers.find("a").on('click', function (e) {
            e.preventDefault();
            self.addSortIcon($(this));
        });

        // Scroll logic       
        $(window).on('scroll resize', () => {
            this.syncStickyHeader("#dvHeaderContainer_real", "#dvHeaderContainer_fake");
        });
    }

    addSortIcon(event) {
        const sortAscIcon = "k-icon k-i-sort-asc-sm";
        const sortDescIcon = "k-icon k-i-sort-desc-sm";
        let sortOrder = "ASC";
        const sortColumn = event.data("id"); 
    
        // Find icons in BOTH headers
        const allRelevantIcons = $(`.dvHeaderContainer a[data-id="${sortColumn}"] span`);

        this.hideSortIcons(sortColumn);

        if (allRelevantIcons.length === 0) {
            $(`.dvHeaderContainer a[data-id="${sortColumn}"]`).append(`<span class="${sortAscIcon}"></span>`);
        } else {
            if (allRelevantIcons.hasClass("k-i-sort-asc-sm")) {
                allRelevantIcons.removeClass("k-i-sort-asc-sm").addClass("k-i-sort-desc-sm");
                sortOrder = "DESC";
            } else {
                allRelevantIcons.removeClass("k-i-sort-desc-sm").addClass("k-i-sort-asc-sm");
                sortOrder = "ASC";
            }
        }
        this.refreshDocListView(sortColumn, sortOrder);
    }

    hideSortIcons(columnClicked) {        
        $(".dvHeaderContainer a").each(function () {
            const column = $(this).data("id");
            if (columnClicked !== column) {
                $(this).find("span.k-icon").remove();
            }
        });
    }

    refreshDocListView(sortColumn = "UploadedDate", sortOrder = "ASC") {        
        const fieldName = sortColumn.replace("Doc_", "");
        const listView = $('#documentVerificationSearchResults-Grid').data('kendoListView');
        if (!listView) return;

        const dataSource = listView.dataSource;
        const sortDescriptor = [{ field: fieldName, dir: sortOrder.toLowerCase() }];
    
        const container = $(this.refineSearchContainer);
        container.find("#SortCol").val(fieldName);
        container.find("#SortOrder").val(sortOrder);
    
        dataSource.query({
            sort: sortDescriptor,
            page: dataSource.page(),
            pageSize: dataSource.pageSize()
        });
    }

    dataBound() {
        const self = this;
        let highlightRow = 3;
        $("#documentVerificationSearchResults-Grid tr").each(function (index) {
            //if (index % 2) {
            //    $(this).addClass("k-alt");
            //}            
            const row = index + 1;
            if (row >= highlightRow && row <= highlightRow + 1) {
                $(this).addClass("altItem");

                if (row === highlightRow + 1)
                    highlightRow = highlightRow + 4;
            }
        });

        //refresh count display
        const dataCount = self.dataSource.total();
        var countSpan = $("#documentVerification-tab").find(".docketing-requests-count");
        if (countSpan) {
            if (dataCount && dataCount > 0) {
                countSpan.text("(" + dataCount + ")");
            }                
            else {
                countSpan.text("");
            }               
        }

        //refresh actions tab
        const actionGrid = $('#actionVerificationGrid');
        if (actionGrid.length > 0) {
            cpiLoadingSpinner.show("", 1);
            actionGrid.data("kendoListView").dataSource.read().then(cpiLoadingSpinner.hide());
        }

        //refresh new documents tab
        const newDocGrid = $('#documentVerificationNewDocGrid');
        if (newDocGrid.length > 0) {
            cpiLoadingSpinner.show("", 1);
            const newDocDS = newDocGrid.data("kendoListView").dataSource;
            if (newDocDS.page() != 1) {
                newDocDS.page(1);
            }               
            newDocDS.read().then(cpiLoadingSpinner.hide());
        }    
        
        //refresh communication tab
        const communicationGrid = $('#documentVerificationCommunicationGrid');
        if (communicationGrid.length > 0) {
            cpiLoadingSpinner.show("", 1);
            const communicationDS = communicationGrid.data("kendoListView").dataSource;
            if (communicationDS.page() != 1) {
                communicationDS.page(1);
            }               
            communicationDS.read().then(cpiLoadingSpinner.hide());
        }
    }

    deleteLinkedAction(keyId) {
        const self = this;

        const deleteMsg = this.getMessageFromData("#dvContentPage", "label-delete-msg");
        const deleteHeader = this.getMessageFromData("#dvContentPage", "label-delete-header");

        let url = $("body").data("base-url") + "/Shared/DocumentVerification/DeleteLinkedAction?";

        cpiConfirm.confirm(deleteHeader, deleteMsg, function () {
            $.ajax({
                type: "POST",
                url: url,
                data: { keyIds: [keyId] },
                dataType: "json",
                success: function (response) {
                    if (response.success)
                        self.refreshDocListView();

                    cpiConfirm.close();
                },
                error: function (response) {
                    cpiConfirm.close();
                    kendo.alert(response.message);
                }
            });
        });
    } 
    
    toggleEntities(e) {
        this.toggleIcon(e);
        const entities = $(e).siblings(".entities");
        entities.toggleClass("d-none");
    }

    toggleAllEntities(e) {
        // 1. Get the target (the span that was clicked)
        const $clickedElement = $(e.currentTarget || e);
    
        // 2. Determine the new state (+ or -) based on the clicked icon
        const state = this.toggleIcon($clickedElement);

        // 3. FORCE SYNC: Find every header with this class and set them to the same state
        const $allHeaderInstances = $(".entitiesHeader");
    
        // We loop through all instances (real and fake) to ensure icons match
        $allHeaderInstances.each((index, element) => {
            this.toggleIcon($(element), state);
        });

        const self = this;
        // 4. Toggle the actual data columns in the list
        $("#documentVerificationContainer .entities").each(function () {
            const $entitiesElement = $(this);
            self.toggleIcon($entitiesElement.parent(), state);
            self.allEntitiesState = state;

            if (state === "-") {
                $entitiesElement.removeClass("d-none");
            } else {
                $entitiesElement.addClass("d-none");
            }
        });

        // 5. Re-sync the sticky header width because columns shifted
        this.syncStickyHeader("#dvHeaderContainer_real", "#dvHeaderContainer_fake");
    }

    toggleAllDeDocket(e) {        
        // 1. Get the target (the span that was clicked)
        const $clickedElement = $(e.currentTarget || e);
    
        // 2. Determine the new state (+ or -) based on the clicked icon
        const state = this.toggleIcon($clickedElement);

        // 3. FORCE SYNC: Find every header with this class and set them to the same state
        const $allHeaderInstances = $(".ddkHeader");
    
        // We loop through all instances (real and fake) to ensure icons match
        $allHeaderInstances.each((index, element) => {
            this.toggleIcon($(element), state);
        });

        const self = this;
        // 4. Toggle the actual data columns in the list
        $("#documentVerificationContainer .ddk").each(function () {
            const $ddkElement = $(this);
            self.toggleIcon($ddkElement.parent(), state);
            self.allDdkState = state;

            if (state === "-") {
                $ddkElement.removeClass("d-none");
            } else {
                $ddkElement.addClass("d-none");
            }
        });

        // 5. Re-sync the sticky header width because columns shifted
        this.syncStickyHeader("#dvHeaderContainer_real", "#dvHeaderContainer_fake");
    }

    toggleDeDocket(e) {
        this.toggleIcon(e);
        const ddks = $(e).siblings(".ddk");
        ddks.toggleClass("d-none");
    }

    toggleAllRequestDocket(e) {        
        // 1. Get the target (the span that was clicked)
        const $clickedElement = $(e.currentTarget || e);
    
        // 2. Determine the new state (+ or -) based on the clicked icon
        const state = this.toggleIcon($clickedElement);

        // 3. FORCE SYNC: Find every header with this class and set them to the same state
        const $allHeaderInstances = $(".dkrHeader");
    
        // We loop through all instances (real and fake) to ensure icons match
        $allHeaderInstances.each((index, element) => {
            this.toggleIcon($(element), state);
        });

        const self = this;
        // 4. Toggle the actual data columns in the list
        $("#documentVerificationContainer .dkr").each(function () {
            const $dkrElement = $(this);
            self.toggleIcon($dkrElement.parent(), state);
            self.allDkrState = state;

            if (state === "-") {
                $dkrElement.removeClass("d-none");
            } else {
                $dkrElement.addClass("d-none");
            }
        });

        // 5. Re-sync the sticky header width because columns shifted
        this.syncStickyHeader("#dvHeaderContainer_real", "#dvHeaderContainer_fake");
    }

    toggleRequestDocket(e) {
        this.toggleIcon(e);
        const dkrs = $(e).siblings(".dkr");
        dkrs.toggleClass("d-none");
    }

    toggleIcon(e, newState) {

        const element = $(e).find(".icon");
        let state = "";
        if (element[0].className.includes("plus") && (!newState || newState === "-")) {
            element[0].className = element[0].className.replace("plus", "minus");
            state = "-";
        }
        else if (!newState || newState === "+") {
            element[0].className = element[0].className.replace("minus", "plus");
            state = "+";
        }
        return state;
    }

    requestListRequestEnd = (e) => {
        if (e && e.response && e.response.Ids) {
            this.requestViewRecordIds = [];
            this.requestViewRecordIds = e.response.Ids;
        }
    }

    requestNavigateHandler = (id) => {
        if (this.requestViewRecordIds && this.requestViewRecordIds[id-1]) {
            var idArray = this.requestViewRecordIds[id-1].split(":");
            this.viewRequest(idArray[0], idArray[1], idArray[2]);
        }        
    }

    viewRequest(system, parentId, keyId) {
        const baseUrl = $("body").data("base-url");
        let url = baseUrl + "/Shared/DocumentVerification/RequestZoom?";
        url = url + "systemTypeCode=" + system + "&parentId=" + parentId + "&keyId=" + keyId;

        const self = this;
        const currentRequestRecordId = system + ":" + parentId + ":" + keyId;

        let retry = 0;
        openRequestPopup();

        function openRequestPopup() {
            cpiLoadingSpinner.show();
            $.get(url)
            .done((result) => {
                cpiLoadingSpinner.hide();
                var actGridChange = false;

                const popupContainer = $(".site-content .popup");
                popupContainer.unbind();

                const requestZoomDialog = popupContainer.find("#docVerificationRequestDocZoomDialog .modal-content");            
                if (requestZoomDialog && requestZoomDialog.length > 0) {
                    const $tempElement = $("<div>").append(result);                    
                    kendo.destroy(requestZoomDialog);
                    requestZoomDialog.html($tempElement.find("#docVerificationRequestDocZoomDialog .modal-content").html());                
                }
                else {
                    popupContainer.empty();
                    popupContainer.html(result);
                    popupContainer.find("#docVerificationRequestDocZoomDialog").modal("show");
                }                
            
                //loading document
                const docViewerContainer = popupContainer.find("#docVerificationRequestDocViewerContainer");
                if (docViewerContainer && docViewerContainer.length > 0) {
                    const docViewerUrl = docViewerContainer.data("viewer-url");
                    if (docViewerUrl > "") {
                        $.ajax({
                            url: docViewerUrl,
                            dataType: "html",
                            cache: false,
                            beforeSend: function () { },
                            success: function (docResult) {
                                docViewerContainer.empty();
                                docViewerContainer.html(docResult);
                            },
                            error: function (e) {
                                pageHelper.showErrors(e);
                            }
                        });
                    }                    
                }
                
                //navigator
                if (self.requestViewRecordIds && self.requestViewRecordIds.findIndex(d => d.includes(currentRequestRecordId)) > -1) {
                    $("#docVerificationRequestDocZoomDialogPager").cpiRecordNavigator({
                        recordIds: Array.from({ length: self.requestViewRecordIds.length }, (v, i) => i + 1),
                        currentId: self.requestViewRecordIds.findIndex(d => d.includes(currentRequestRecordId)) + 1,
                        navigateHandler: self.requestNavigateHandler
                    });
                }

                //confirm changes on act grid before closing
                popupContainer.find("#docVerificationRequestDocZoomDialog").on("hide.bs.modal", function (e) {
                    var actGrid = $("#documentVerificationActGrid").data("kendoGrid");
                    if (actGrid) {
                        var actGridDS = actGrid.dataSource;
                        $.each(actGridDS._data, function ()
                        {
                            if (this.dirty == true)
                            {
                                actGridChange = true;
                            }
                        });
                    }

                    if (actGridChange && !confirm($("#docVerificationRequestDocZoomDialog").data("cancel-message"))) {                        
                        e.preventDefault();
                        return;
                    }
                });

                //refresh grid on popup close
                popupContainer.find("#docVerificationRequestDocZoomDialog").on("hidden.bs.modal", function (e) {                    
                    kendo.destroy(popupContainer.find("#docVerificationRequestDocZoomDialog .modal-content"));
                    popupContainer.empty();
                    self.refreshDocListView();
                });

                //set current date for Date field
                $("#docVerificationRequestDocZoomDialog").off("click", ".completed-date-default");
                $("#docVerificationRequestDocZoomDialog").on("click", ".completed-date-default", function () {
                    const parent = $(this).closest(".completed-date");
                    const completedDatePicker = parent.find("input[name='CompletedDate']").data("kendoDatePicker");
                    const currentDate = new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate());
                    completedDatePicker.value(currentDate);
                    completedDatePicker.trigger("change");
                    parent.find("div.float-label").removeClass("inactive").addClass("active");
                });

                var requestZoomPopupSummary = popupContainer.find("#requestDocZoomPopupSummary");
                requestZoomPopupSummary.find("button.close").on("click", function () {
                    $(requestZoomPopupSummary).slideUp();
                });

                //save button
                $("#docVerificationRequestDocZoomDialog").off("click", ".save-completed-date");
                $("#docVerificationRequestDocZoomDialog").on("click", ".save-completed-date", function () {                    
                    const requestCompletedDateKey = popupContainer.find("input[id='requestCompletedDateKey']");                    
                    const completedDatePicker = popupContainer.find("input[name='CompletedDate']").data("kendoDatePicker");
                    const entryForm = $(popupContainer.find("#docVerificationRequestDocZoomDialog").find("form")[0]);

                    if (requestCompletedDateKey && requestCompletedDateKey.val() && requestCompletedDateKey.val().toLowerCase().indexOf("docid") !== -1) {
                        var actGrid = $("#documentVerificationActGrid").data("kendoGrid");
                        //auto save changes on Action grid            
                        if (actGrid && actGrid.dataSource.hasChanges()) {                              
                            pageHelper.kendoGridSave({
                                name: 'documentVerificationActGrid'                                
                                ,afterSubmit: (result) => {                                     
                                    if (result.message) {
                                        $(requestZoomPopupSummary).removeClass("alert-danger");
                                        $(requestZoomPopupSummary).addClass("alert-success");
                                        requestZoomPopupSummary.find("span.message").html(result.message);
                                        $(requestZoomPopupSummary).show(function () {
                                            $(requestZoomPopupSummary).delay(3000).slideUp();
                                        });
                                    }
                                    else if (result.errorMessage) {
                                        $(requestZoomPopupSummary).addClass("alert-danger");
                                        $(requestZoomPopupSummary).removeClass("alert-success");
                                        requestZoomPopupSummary.find("span.message").html(pageHelper.getErrorMessage(result));        
                                        $(requestZoomPopupSummary).show();
                                    }
                                    var cancelBtn = $("#documentVerificationActGrid").find(".k-grid-toolbar .k-grid-Cancel");
                                    if (cancelBtn) {
                                        cancelBtn.trigger("click");
                                    }
                                }
                            });                
                        }
                    }
                    else {
                        $.validator.unobtrusive.parse(entryForm);
                        if (entryForm.data("validator") !== undefined) {
                            entryForm.data("validator").settings.ignore = "";
                        }
                        if (!entryForm.valid()) {
                            return;
                        }
                    
                        cpiLoadingSpinner.show();
                        let selectedDate = completedDatePicker.value();
                        if (selectedDate) {
                            selectedDate = selectedDate.toLocaleDateString();
                        }
                    
                        let completedDateUrl = baseUrl + "/Shared/DocumentVerification/MarkRequestsAsCompleted";
                        const data = {
                            completedDate: selectedDate,
                            keyIds: requestCompletedDateKey.val(),
                            __RequestVerificationToken: entryForm.find("input[name=__RequestVerificationToken]").val()
                        };                    
                        $.post(completedDateUrl, data)
                            .done(function (result) {
                                cpiLoadingSpinner.hide();
                                if (result) {
                                    if (result.userName) {
                                        const requestCompletedBy = popupContainer.find("input[name='CompletedBy']");
                                        if (requestCompletedBy) {
                                            requestCompletedBy.val(result.userName);
                                        }
                                    }
                                    if (result.message) {
                                        $(requestZoomPopupSummary).removeClass("alert-danger");
                                        $(requestZoomPopupSummary).addClass("alert-success");
                                        requestZoomPopupSummary.find("span.message").html(result.message);
                                        $(requestZoomPopupSummary).show(function () {
                                            $(requestZoomPopupSummary).delay(5000).slideUp();
                                        });
                                    }
                                    else if (result.errorMessage) {
                                        $(requestZoomPopupSummary).addClass("alert-danger");
                                        $(requestZoomPopupSummary).removeClass("alert-success");
                                        requestZoomPopupSummary.find("span.message").html(pageHelper.getErrorMessage(result));        
                                        $(requestZoomPopupSummary).show();
                                    }
                                }
                            })
                            .fail((error) => {
                                cpiLoadingSpinner.hide();
                                pageHelper.showErrors(error);
                            });
                    }                    
                });

            })
            .fail((e) => {    
                cpiLoadingSpinner.hide();
                if (e.status == 401 && retry < 3) {
                    retry++;
                    const baseUrl = $("body").data("base-url");
                    const url = `${baseUrl}/Graph/SharePoint`;

                    sharePointGraphHelper.getGraphToken(url, () => {
                        openRequestPopup();
                    });
                }
                else {
                    pageHelper.showErrors(e.responseText);
                }
            });
        }        
    }
    //Documents tab - END

    //New Documents tab - START
    configureTab1Sort() {
        const containers = $(".ndHeaderContainer");

        // Add default icon to the UploadedDate column in both headers
        const uploadDateCols = containers.find('[data-id="NewDoc_UploadedDate"]');
        uploadDateCols.append('<span class="NewDoc_UploadedDateSortIcon k-icon k-i-sort-desc-sm"></span>');

        const self = this;
    
        // Attach click events to all links in both headers
        containers.find("a").on('click', function (e) {
            e.preventDefault();
            const $clickedLink = $(this);
            self.addNewDocSortIcon($clickedLink);
        });

        $(window).on('scroll resize', () => {
            this.syncStickyHeader("#ndHeaderContainer_real", "#ndHeaderContainer_fake");
        });
    
        $("#ndHeaderContainer_fake").hide();
    }

    addNewDocSortIcon(event) {
        const sortAscIcon = "k-icon k-i-sort-asc-sm";
        const sortDescIcon = "k-icon k-i-sort-desc-sm";
        let sortOrder = "ASC";
        const sortColumn = event.data("id");
        const allRelevantIcons = $(`.ndHeaderContainer a[data-id="${sortColumn}"] span`);

        this.hideNewDocSortIcons(sortColumn);

        if (allRelevantIcons.length === 0) {
            // If no icon exists, append it to BOTH headers
            $(`.ndHeaderContainer a[data-id="${sortColumn}"]`).append(`<span class="${sortAscIcon}"></span>`);
        } else {
            // Toggle logic
            if (allRelevantIcons.hasClass("k-i-sort-asc-sm")) {
                allRelevantIcons.removeClass("k-i-sort-asc-sm").addClass("k-i-sort-desc-sm");
                sortOrder = "DESC";
            } else {
                allRelevantIcons.removeClass("k-i-sort-desc-sm").addClass("k-i-sort-asc-sm");
                sortOrder = "ASC";
            }
        }

        this.refreshNewDocListView(sortColumn, sortOrder);
    }

    hideNewDocSortIcons(columnClicked) {
        // Find all links in both headers
        $(".ndHeaderContainer a").each(function () {
            const column = $(this).data("id");

            // If this isn't the column we just clicked, clear the icon inside it
            if (columnClicked !== column) {
                $(this).find("span.k-icon").remove();
            }
        });
    }

    refreshNewDocListView(sortColumn = "UploadedDate", sortOrder = "DESC") {
        // Ensure we handle the string correctly if it still has the prefix
        const fieldName = sortColumn.replace("NewDoc_", "");
    
        const listView = $('#documentVerificationNewDocGrid').data('kendoListView');
        if (!listView) return;

        const dataSource = listView.dataSource;
        const sortDescriptor = [{ field: fieldName, dir: sortOrder.toLowerCase() }];
    
        // Update hidden fields for search persistence
        const container = $(this.refineSearchContainer);
        container.find("#SortCol").val(fieldName);
        container.find("#SortOrder").val(sortOrder);

        dataSource.query({
            sort: sortDescriptor,
            page: dataSource.page(),
            pageSize: dataSource.pageSize()
        });
    }
    
    refreshNewDocListViewDS() {
        const gridDS = $('#documentVerificationNewDocGrid').data('kendoListView').dataSource;        
        gridDS.read();        
    }
    
    newDocDataBound(e) {
        $("#documentVerificationNewDocGrid tr").each(function (index) {
            if (index % 2) {
                $(this).addClass("k-alt");
            }
        });

        //refresh count display
        const dataCount = e.sender.dataSource.total();
        var countSpan = $("#documentVerification-tab").find(".documents-review-count");
        if (countSpan) {
            if (dataCount && dataCount > 0) {
                countSpan.text("(" + dataCount + ")");
            }                
            else {
                countSpan.text("");
            }                
        }
    }

    deleteNewDocument(id) {
        const self = this;
        const deleteMsg = this.getMessageFromData("#dvContentPage", "label-delete-msg");
        const deleteHeader = this.getMessageFromData("#dvContentPage", "label-delete-header");
        const verificationToken = $(this.refineSearchContainer).find("input[name=__RequestVerificationToken]").val();

        let url = $("body").data("base-url");
        if (self.documentStorageOption == 1) {
            url += "/Shared/SharePointGraph/DVDeleteDocuments?";
        }
        else if (self.documentStorageOption == 2) {
            url +=  "/iManageWork/DVDeleteDocuments?";
        }
        else {
            url += "/Shared/DocDocuments/DVDeleteDocuments?";
        }
        cpiConfirm.confirm(deleteHeader, deleteMsg, function () {
            $.ajax({
                type: "POST",
                url: url,
                data: { ids: [id] },
                dataType: "json",
                headers: { "RequestVerificationToken": verificationToken },
                success: function (result) {
                    if (result.success) {
                        self.refreshNewDocListViewDS();
                        self.refreshCriteriaDS();
                    }    
                    cpiConfirm.close();
                },
                error: function (error) {
                    cpiConfirm.close();
                    kendo.alert(error.message);
                }
            });
        });
    }

    linkRecord(id, docName) {
        const self = this;
        let url = $("body").data("base-url") + "/Shared/DocumentVerification/SearchLink?";

        $.get(url, { ids: id, docNames: docName })
            .done((result) => {
                //clear all existing hidden popups to avoid kendo id issue
                $(".site-content .popup").empty();
                const popupContainer = $(".site-content .popup").last();
                popupContainer.html(result);
                const dialog = $("#searchLinkDialog");
                dialog.unbind();
                dialog.modal("show");

                self.initializeSearchLink();
            })
            .fail(function (error) {
                pageHelper.showErrors(error);
            });
    }

    onNewDocUploadFail(e) {
        cpiLoadingSpinner.hide();
        let error = e.XMLHttpRequest.responseText;
        if (error === "")
            error = "Error occurred during upload.";
        alert(error);
    }

    onNewDocUploadSuccess = (e) => {
        cpiLoadingSpinner.hide();
        this.refreshNewDocListViewDS();
    }

    getSearchLinkCriteriaData = () => {
        var systemIds = "";
        $("input[name='basicSearchSystem']:checked").each(function () {
            systemIds += this.value + "|";
        });
        const selectedIds = $.unique(systemIds.split("|")).join("|");

        return { searchTerm: $("#BasicSearchTerm").val(), systemIds: selectedIds };
    }

    initializeSearchLink = () => {
        const dialog = $("#searchLinkDialog");
        const self = this;
        const baseUrl = $("body").data("base-url");
        $(document).ready(() => {
            dialog.find(".search-settings").hide();

            dialog.find(".saveLinks").on("click", function (e) {
                e.preventDefault();                
                self.saveSearchLink();                
            });

            dialog.find(".search-submit").on("click", function (e) {
                e.preventDefault();
                self.refreshSearchLink();
            });

            $("#BasicSearchTerm").on("keyup", (e) => {
                const keyPressed = e.key;
                if (keyPressed === "Enter") {
                    self.refreshSearchLink();
                }
            });

            dialog.find(".search-setting").on("click", function (e) {
                e.preventDefault();

                dialog.find(".search-settings").show();
                dialog.find(".mainSearchDialog").hide();

                let url = `${baseUrl}/Shared/DocumentVerification/SearchSettings`;
                $.get(url)
                    .done(function (result) {
                        dialog.find(".search-settings-detail").html(result);
                    })
                    .fail(function (e) {
                        pageHelper.showErrors(e.responseText);
                    });
            });

            dialog.on("click", ".search-settings-cancel", () => {
                dialog.find(".search-settings").hide();
                dialog.find(".mainSearchDialog").show();
            });

            dialog.on("click", ".search-settings-submit", () => {
                var selectedSearchFields = [...$('input[name=searchField]:checked')].map(e => parseInt(e.id));
                if (selectedSearchFields) {
                    const selectedIds = $.uniqueSort(selectedSearchFields);
                    let url = `${baseUrl}/Shared/DocumentVerification/EditSearchSettings`;
                    $.post(url, { keyIds: selectedIds })
                        .done(function (result) {
                        })
                        .fail((error) => {
                            pageHelper.showErrors(error)
                        });
                }
                dialog.find(".search-settings").hide();
                dialog.find(".mainSearchDialog").show();
            });

        });
    }

    saveSearchLink() {
        var docIds = $("#docIds");
        const grid = $("#searchLinkGrid").data("kendoGrid");
        const selection = grid.selectedKeyNames();

        const selectedIds = $.unique(selection);
        if (!selectedIds || selectedIds.length < 1) {
            this.cpiAlert.warning("No records selected to link");
            return;
        }

        var selectedData = [];
        var gridData = grid.dataSource.data();
        selectedIds.forEach(function (id) {
            gridData.forEach(function (dataItem) {
                if (id === dataItem.Link) {
                    selectedData.push(
                        {
                            Link: dataItem.Link,
                            SystemName: dataItem.SystemName,
                            ScreenName: dataItem.ScreenName,
                            FieldValues: dataItem.FieldValues,
                            IsActRequired: dataItem.IsActRequired,
                            RespDocketing: dataItem.RespDocketing,
                            RespDocketings: (dataItem.RespDocketings != null ? dataItem.RespDocketings.map(d => d.Id) : []),
                            RespReporting: dataItem.RespReporting,
                            RespReportings: (dataItem.RespReportings != null ? dataItem.RespReportings.map(d => d.Id) : [])
                        });
                }
            })
        });

        const self = this;
        let url = $("body").data("base-url");
        if (self.documentStorageOption == 1) {
            url += "/Shared/SharePointGraph/SaveSearchLink";
        }
        else {
            url += "/Shared/DocDocuments/SaveSearchLink";
        }

        self.cpiLoadingSpinner.show();
        $.ajax({
            url: url,
            type: 'POST',
            data: { ids: docIds[0].value, selectedRecords: selectedData },
            dataType: "json",
            success: (response) => {
                self.cpiLoadingSpinner.hide();
                pageHelper.handleEmailWorkflow(response);

                const dialog = $("#searchLinkDialog");
                dialog.modal("hide");
                self.refreshCriteriaDS();
                self.refreshNewDocListViewDS();
                self.refreshCommunicationListViewDS();
                self.refreshDocListView();
            },
            error: function (error) {
                self.cpiLoadingSpinner.hide();
                pageHelper.showErrors(error.responseText);
            }
        });
    }

    refreshSearchLink() {        
        const grid = $("#searchLinkGrid").data("kendoGrid");
        if (grid.dataSource.page() !== 1) {
            grid.dataSource.page(1);
        }
        grid.dataSource.read();
    }

    onRespDocketingComboBoxChange = (e) => {
        if (e.sender) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");
            const grid = $("#searchLinkGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            var selectedItems = [];
            if (e.sender.dataItem()) {
                var multiDataItems = e.sender.dataItems();                
                for (var i = 0; i < multiDataItems.length; i += 1) {
                    var item = multiDataItems[i];
                    selectedItems.push(item.Name);
                }
                dataItem.RespDocketing = selectedItems.join("; ");               
            } 
            else {
                dataItem.RespDocketing = "";
            }
        }
    }

    onRespReportingComboBoxChange = (e) => {
        if (e.sender) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");
            const grid = $("#searchLinkGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            var selectedItems = [];
            if (e.sender.dataItem()) {
                var multiDataItems = e.sender.dataItems();                
                for (var i = 0; i < multiDataItems.length; i += 1) {
                    var item = multiDataItems[i];
                    selectedItems.push(item.Name);
                }
                dataItem.RespReporting = selectedItems.join("; ");               
            }
            else {
                dataItem.RespReporting = "";
            }
        }
    }

    onSearchLinkGridCellClose = (e) => {
        if (e.sender) {
            const rows = $(`#${e.sender.element[0].id}`).find("tr.k-master-row");
            const grid = $("#searchLinkGrid").data("kendoGrid");
            const data = grid.dataSource.data();          
            if (rows) {
                for (var i = 0; i < rows.length; i++) {                    
                    const rowData = data[i];                    
                    if (rowData && rowData.RespDocketing) {
                        grid.dataSource.data()[i].RespDocketing = rowData.RespDocketing;
                    }

                    if (rowData && rowData.RespReporting) {
                        grid.dataSource.data()[i].RespReporting = rowData.RespReporting;
                    }
                }
                grid.refresh();                
            }
        }
    }    

    showReviewFilters() {
        const popupContainer = $(".site-content .popup").last();
        const dialogContainer = popupContainer.find("#docVerificationReviewFiltersDialog");
        dialogContainer.modal("show");
        let entryForm = dialogContainer.find("form")[0];     
        entryForm = $(entryForm);
        const self = this;
        const afterSubmit = function (result) {
            self.refreshNewDocListViewDS();
        };
        entryForm.cpiPopupEntryForm({ dialogContainer: dialogContainer, afterSubmit: afterSubmit });
    }
    //New Documents tab - END

    //Mass update responsible Docketing/Reporting - START
    //Only available from 2nd or 4th tab
    editResponsible(system, keyId, docName, respType) {
        const self = this;
        let url = $("body").data("base-url") + "/Shared/DocumentVerification/UpdateResponsible?";
        var ids = system + "|" + keyId;
        $.get(url, { ids: ids, docNames: docName, respType: respType })
            .done((result) => {
                //clear all existing hidden popups to avoid kendo id issue
                $(".site-content .popup").empty();
                const popupContainer = $(".site-content .popup").last();
                popupContainer.html(result);
                const dialog = $("#documentResponsibleDialog");
                dialog.unbind();
                dialog.modal("show");

                self.initializeUpdateResponsible(respType);
            })
            .fail(function (error) {
                pageHelper.showErrors(error);
            });
    }

    initializeUpdateResponsible = () => {
        const dialog = $("#documentResponsibleDialog");
        const self = this;        
        $(document).ready(() => {           

            dialog.find(".saveResponsible").on("click", function (e) {
                e.preventDefault();
                self.updateResponsible();
            });
        });
    }

    updateResponsible() {
        const self = this;
        const idSystemTypes = $("#idSystemTypes");
        const respType = $("#respType").val();
        
        const oldResponsible_MultiSelect = $("#oldResponsible").data("kendoMultiSelect");
        const newResponsible_MultiSelect = $("#newResponsible").data("kendoMultiSelect");
        const oldResponsible = oldResponsible_MultiSelect !== null && oldResponsible_MultiSelect !== undefined ? oldResponsible_MultiSelect.value() : {};
        const newResponsible = newResponsible_MultiSelect !== null && newResponsible_MultiSelect !== undefined ? newResponsible_MultiSelect.value() : {};        
        
        const responsibleDocketing_MultiSelect = $("#responsibleDocketing").data("kendoMultiSelect");
        const responsibleReporting_MultiSelect = $("#responsibleReporting").data("kendoMultiSelect");
        const responsibleDocketing = responsibleDocketing_MultiSelect !== null && responsibleDocketing_MultiSelect !== undefined ? responsibleDocketing_MultiSelect.value() : {};
        const responsibleReporting = responsibleReporting_MultiSelect !== null && responsibleReporting_MultiSelect !== undefined ? responsibleReporting_MultiSelect.value() : {};

        //Validation when updating specific responsible (Docketing OR Reporting)
        if (respType !== null && respType > "") {
            if (!newResponsible || newResponsible.length < 1) {
                this.cpiAlert.warning(this.getMessageFromData("#documentResponsibleDialog", "label-warning"));
                return;
            } 

            if (oldResponsible) {
                let hasDuplicates = oldResponsible.filter(value => newResponsible.includes(value)).length > 0;
                if (hasDuplicates) {
                    this.cpiAlert.warning(this.getMessageFromData("#documentResponsibleDialog", "label-duplicate-error"));
                    return;
                }
            }
        }
        
        //Validation when assign responsible (Docketing AND/OR Reporting)
        if (!respType || respType === "") {
            if ((!responsibleDocketing || responsibleDocketing.length < 1) && (!responsibleReporting || responsibleReporting.length < 1)) {
                this.cpiAlert.warning(this.getMessageFromData("#documentResponsibleDialog", "label-warning"));
                return;
            }
        }

        const baseUrl = $("body").data("base-url");
        
        const idList = [...new Set(idSystemTypes[0].value.split(';'))].join(";");        
        if (idList && idList > "") {
            self.cpiLoadingSpinner.show();

            let docUrl = baseUrl + "/Shared/DocumentVerification/";
            let data = {};

            if (!respType || respType === "") {
                docUrl += "AssignDocumentResponsible";
                data = { ids: idList, responsibleDocketing: responsibleDocketing, responsibleReporting: responsibleReporting };
            }
            else {
                docUrl += "UpdateResponsible";
                data = { respType: respType, ids: idList, oldResponsible: oldResponsible, newResponsible: newResponsible };
            }

            $.ajax({
                url: docUrl,
                type: 'POST',
                data: data,
                dataType: "json",
                success: (response) => {
                    self.cpiLoadingSpinner.hide();
                    pageHelper.handleEmailWorkflow(response);

                    const dialog = $("#documentResponsibleDialog");
                    dialog.modal("hide");
                    self.refreshCriteriaDS();
                    self.refreshNewDocListViewDS();
                    self.refreshCommunicationListViewDS();
                    self.refreshDocListView();
                },
                error: function (error) {
                    self.cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error.responseText);
                }
            });
        }
    }    
    //Mass update responsible - END

    getSystems(name) {
        let systems = "|";
        const systemTypes = $(name).find(".system-types input");

        $.each(systemTypes, function () {
            if (this.checked) {
                systems += this.value + "|";
            }
        });
        return { systemType: systems };
    }

    getDefaultSystems(name) {
        let systems = "|";
        const systemTypes = $(name).find(".def-system-types input");

        $.each(systemTypes, function () {
            if (this.checked) {
                systems += this.value + "|";
            }
        });
        return { systemType: systems };
    }

    redirectToAction(system, actId) {

        let url = $("body").data("base-url") + "/";

        if (system === "P")
            url = url + "Patent/ActionDue/Detail/" + actId;
        else if (system === "T")
            url = url + "Trademark/ActionDue/Detail/" + actId;
        else if (system === "G")
            url = url + "GeneralMatter/Actiondue/Detail/" + actId;

        window.open(url, '_blank');
    }

    redirectToRecordByCaseNumber(systemType, caseNumber, parentId) {

        let url = $("body").data("base-url") + "/";

        if (systemType === "P")
            url = url + "Patent/Invention/DetailLink?caseNumber=" + caseNumber;        
        else if (systemType === "G")
            url = url + "GeneralMatter/Matter/DetailLink?id=" + parentId;

        window.open(url, '_blank');
    }

    redirectToRecordByCountry(systemType, parentId) {

        let url = $("body").data("base-url") + "/";

        if (systemType === "P")
            url = url + "Patent/CountryApplication/DetailLink?id=" + parentId;
        else if (systemType === "T")
            url = url + "Trademark/TmkTrademark/DetailLink?id=" + parentId;        

        window.open(url, '_blank');
    }

    getCriteria(name) {
        const data = pageHelper.formDataToJson($(name)).payLoad;        
        if (data.Patent)
            data.Patent = "P";
        if (data.Trademark)
            data.Trademark = "T";
        if (data.GeneralMatter)
            data.GeneralMatter = "G";

        if (data.AttorneyFilter1)
            data.AttorneyFilter1 = "A1";
        if (data.AttorneyFilter2)
            data.AttorneyFilter2 = "A2";
        if (data.AttorneyFilter3)
            data.AttorneyFilter3 = "A3";
        if (data.AttorneyFilter4)
            data.AttorneyFilter4 = "A4";
        if (data.AttorneyFilter5)
            data.AttorneyFilter5 = "A5";
        if (data.AttorneyFilterR)
            data.AttorneyFilterR = "AR";
        if (data.AttorneyFilterD)
            data.AttorneyFilterD = "AD";
        if (data.AttorneyFilterRD)
            data.AttorneyFilterRD = "ARD";

        if (data.Attorneys)
            data.Attorneys = $(name).find("#Attorneys").val();

        if (data.ActionTypes)
            data.ActionTypes = $(name).find("#ActionTypes").val();

        if (data.DocNames)
            data.DocNames = $(name).find("#DocNames_documentVerificationSearch").val();

        if (data.DocUploadedBys)
            data.DocUploadedBys = $(name).find("#DocUploadedBys").val();

        if (data.Countries)
            data.Countries = $(name).find("#Countries").val();

        if (data.Sources)
            data.Sources = $(name).find("#Sources").val();

        if (data.ActCreatedBys)
            data.ActCreatedBys = $(name).find("#ActCreatedBys").val();

        if (data.Clients)
            data.Clients = $(name).find("#Clients").val();

        if (data.RespDocketings)
            data.RespDocketings = $(name).find("#RespDocketings").val();

        if (data.RespReportings)
            data.RespReportings = $(name).find("#RespReportings").val();
                    
        return data;
    }

    viewDocument(system, docFileName, parentId, keyId = '') {
        const baseUrl = $("body").data("base-url");
        let url = "";
        let systemName = "";
        let screenCode = "";
        if (system === "P") {
            systemName = "Patent";
            screenCode = "CA";
        }
        else if (system === "T") {
            systemName = "Trademark";
            screenCode = "Tmk";
        }
        else if (system === "G") {
            systemName = "GeneralMatter";
            screenCode = "GM";
        }

        if (keyId && keyId.startsWith('reqid') || keyId.startsWith('dedocketid')) {
            url += "/Shared/DocumentVerification/ViewDocketRequestDocument?";    
            url = url + "systemTypeCode=" + system + "&docFileName=" + docFileName + "&parentId=" + parentId + "&keyId=" + keyId;

        }
        else {
            url += "/Shared/DocViewer/ZoomImageLink?";
            url = url + "system=" + systemName + "&imageFile=" + docFileName + "&screenCode=" + screenCode + "&key=" + parentId + "&fileType=7";            
        }       

        documentPage.zoomDocument(url);
    }

    viewSPDocument(docLibrary, driveItemId) {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Shared/SharePointGraph/GetPreviewUrl`;

        let retry = 0;
        openPreviewFile();

        function openPreviewFile() {
            $.get(url, { docLibrary: docLibrary, id: driveItemId })
                .done(function (result) {
                    const a = document.createElement("a");
                    document.body.appendChild(a);
                    a.href = result.previewUrl;
                    a.target = "_blank"
                    a.click();
                    setTimeout(() => {
                        document.body.removeChild(a);
                    }, 0);
                })
                .fail(function (e) {
                    if (e.status == 401 && retry < 3) {
                        retry++;
                        const baseUrl = $("body").data("base-url");
                        const url = `${baseUrl}/Graph/SharePoint`;

                        sharePointGraphHelper.getGraphToken(url, () => {
                            openPreviewFile();
                        });
                    }
                    else {
                        pageHelper.showErrors(e.responseText);
                    }
                });
        }
    }
        
    editDocument(system, parentId, docId) {
        let url = $("body").data("base-url") + "/Shared/DocDocuments/GridUpdate?";
        var documentLink = "";

        if (system === "P") {
            documentLink = system + "|CA|AppId|" + parentId;
        }
        else if (system === "T") {
            documentLink = system + "|Tmk|TmkId|" + parentId;
        }
        else if (system === "G") {
            documentLink = system + "|GM|MatId|" + parentId;
        }

        var param = {
            documentLink: documentLink,
            id: docId,
            showDocumentViewer: true
        };
       
        $.get(url, param)
            .done((result) => {
                //clear all existing hidden popups to avoid kendo id issue
                $(".site-content .popup").empty();
                const popupContainer = $(".site-content .popup").last();
                popupContainer.html(result);
                this.initializeEditor();   
                this.loadEditorViewerContainer(popupContainer);
            })
            .fail((e) => {
                pageHelper.showErrors(e.responseText);
            });
    }

    editNewDocument(docId) {
        let url = $("body").data("base-url") + "/Shared/DocDocuments/NewDocUpdate?";        

        var param = {
            id: docId
        };

        $.get(url, param)
        .done((result) => {
            //clear all existing hidden popups to avoid kendo id issue
            $(".site-content .popup").empty();
            const popupContainer = $(".site-content .popup").last();
            popupContainer.html(result);
            this.initializeEditor();            
            this.loadEditorViewerContainer(popupContainer);
        })
        .fail((e) => {
            pageHelper.showErrors(e.responseText);
        });        
    }

    editSPDocument(driveItemId) {
        let url = $("body").data("base-url");

        if (driveItemId && driveItemId > '') {
            url += "/Shared/SharePointGraph/ModifySPFile?";
        }
        else {
            url += "/Shared/SharePointGraph/AddSPFile?";
        }

        var param = {
            driveItemId: driveItemId
        };

        $.get(url, param)
            .done((result) => {
                //clear all existing hidden popups to avoid kendo id issue
                $(".site-content .popup").empty();
                const popupContainer = $(".site-content .popup").last();
                popupContainer.html(result);
                this.initializeEditor(true);
                this.loadEditorViewerContainer(popupContainer);
            })
            .fail((e) => {
                if (e.status == 401) {
                    const baseUrl = $("body").data("base-url");
                    const url = `${baseUrl}/Graph/SharePoint`;
                    sharePointGraphHelper.getGraphToken(url, () => {
                        const retryMsg = "Please try it again after the SharePoint authentication.";
                        pageHelper.showErrors(retryMsg);
                    });
                }
                else {
                    pageHelper.showErrors(e.responseText);
                }                
            });
    }

    loadEditorViewerContainer(popupContainer) {
        const docEditorViewerContainer = popupContainer.find("#documentEditorViewerContainer");
        if (docEditorViewerContainer && docEditorViewerContainer.length > 0) {
            const docViewerUrl = docEditorViewerContainer.data("viewer-url");
            $.ajax({
                url: docViewerUrl,
                dataType: "html",
                cache: false,
                beforeSend: function () { },
                success: function (docResult) {
                    docEditorViewerContainer.empty();
                    docEditorViewerContainer.html(docResult);
                },
                error: function (e) {
                    pageHelper.showErrors(e);
                }
            });
        }
    }

    initializeEditor = (isSP = false, checkDocType = true) => {
        const container = $("#documentEditorDialog");
        container.modal("show");
        const form = $("#documentEditorForm");
        form.floatLabels();
        const self = this;        

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
             
            //self.cpiLoadingSpinner.show();            
            if (isSP === false && !this.validateUploadAction(checkDocType))
                return;                        

            pageHelper.hideErrors();
            
            var formData = new FormData(form[0]);

            var actGrid = $("#documentVerificationActGrid").data("kendoGrid");
            ////show error if IsActRequired = true and Action grid is empty
            //if (formData && formData.has('IsActRequired') && formData.get('IsActRequired') === 'true' && actGrid) {
            //    var gridData = actGrid.dataSource.data();
            //    if (gridData && gridData.length <= 0) {
            //        alert(container.data("missing-action"));
            //        return;
            //    }                
            //}

            //auto save changes on Action grid            
            if (actGrid && actGrid.dataSource.hasChanges()) {
                //var emptyBaseDateItems = actGrid.dataSource.data().filter((item) => (item.BaseDate === "" || !item.BaseDate));
                //if (emptyBaseDateItems && emptyBaseDateItems.length > 0) {
                //    alert(container.data("missing-basedate"));
                //    return;
                //}
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
                        return;
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
                        self.cpiLoadingSpinner.hide();
                        pageHelper.handleEmailWorkflow(response);
                        $("#documentEditorDialog").modal("hide");                        
                        self.refreshDocListView();
                        self.refreshCriteriaDS();
                    },
                    error: function (e) {
                        self.cpiLoadingSpinner.hide();
                        pageHelper.showErrors(e);
                    }
                });
            }
            
        });
    }
    
    validateUploadAction = (checkDocType = true) => {
        const container = $("#documentEditorDialog");

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

        if (checkDocType) {
            const action = container.find("input[name=DocTypeId]").data("kendoComboBox").text().toLowerCase();
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
        }        
        
        //else if (action === "copyImage") {
        //    if (isEmpty("ImageSelected")) {
        //        showError("ImageSelectedError");
        //        return false;
        //    }
        //}
        return true;
    }

    openAddAction(system, parentId, actionTypeId) {
        let url = $("body").data("base-url");
        let area = "";

        if (system === "P") {
            area = "Patent";
        }
        else if (system === "T") {
            area = "Trademark";
        }
        else if (system === "G") {
            area = "GeneralMatter";
        }

        url = url + "/" + area + "/ActionDue/DetailLink?id=0&parentId=" + parentId + "&actionTypeId=" + actionTypeId;
        window.open(url, "_blank");
    }

    refreshCriteriaDS() {
        $("select[data-role='multiselect']").each(function () {
            const multi = $(this).data("kendoMultiSelect");
            if (multi) {
                const selectedValues = multi.value();
                multi.dataSource.read();

                // Reset selected values, only if they exist in the new data source
                const validSelectedValues = [];
                const newDataSourceData = multi.dataSource.data(); // Get the data from the new dataSource

                for (let i = 0; i < selectedValues.length; i++) {
                    const value = selectedValues[i];
                    if (newDataSourceData.some(item => multi.options.dataValueField ? item[multi.options.dataValueField] === value : item === value)) {
                        validSelectedValues.push(value);
                    }
                }

                multi.value(validSelectedValues);
                multi.trigger("change");                
            }
        });              
    }

    refreshToDoCount = () => {
        const docVerificationCount = $(".nav-login .badge.doc-verification-count");
        if (docVerificationCount) {
            const url = docVerificationCount.data("url");
            if (url) {
                $.get(url)
                    .done((response) => {
                        docVerificationCount.text(response.ToDoItemCount > 0 ? response.ToDoItemCount : "");
                    });
            }
        }
    }

    allowDropMail = (event) => {
        event.preventDefault();
    }

    dropMail = (event) => {
        event.preventDefault();

        const el = $(event.target);
        const dropZone = el.closest("#DropZone");
        const targetId = event.dataTransfer.getData("targetId");
        const mailbox = event.dataTransfer.getData("mailbox");
        let messageIds = [];

        const updateStatus = (status) => {
            localStorage.setItem(targetId, status);
        }

        const cancelDrop = (wait) => {
            if (wait)
                setTimeout(() => {
                    updateStatus("cancel");
                }, wait);
            else
                updateStatus("cancel");
        }

        if (event.dataTransfer.getData("hasAttachments")) 
            messageIds = JSON.parse(event.dataTransfer.getData("hasAttachments"));

        const saveMessage = () => {
            const url = dropZone.data("save-email-url");

            if (url) {
                const verificationToken = $(this.refineSearchContainer).find("input[name=__RequestVerificationToken]").val()

                cpiLoadingSpinner.show();
                $.post(url, { ids: messageIds, mailbox: mailbox, __RequestVerificationToken: verificationToken })
                    .done((response) => {
                        cpiLoadingSpinner.hide();
                        updateStatus("ok");
                        this.refreshNewDocListViewDS();
                    })
                    .fail((error) => {
                        cpiLoadingSpinner.hide();
                        updateStatus("fail");
                        pageHelper.showErrors(error);
                    });
            }
            else {
                cancelDrop(1000);
            }
        }

        if (messageIds.length > 0 && mailbox != '') {
            let dropConfirm = messageIds.length == 1 && event.dataTransfer.getData("sender") ? dropZone.data("drop-confirm") : dropZone.data("drop-confirm-multiple");

            if (dropConfirm) {
                dropConfirm = dropConfirm.replace("{subject}", event.dataTransfer.getData("subject")).replace("{sender}", event.dataTransfer.getData("sender")).replace("{count}", messageIds.length);

                cpiConfirm.confirm(window.cpiBreadCrumbs.getTitle(), dropConfirm, saveMessage, null, null, () => {
                    cancelDrop();
                });
            }
            else
                saveMessage();
        }
        else if (mailbox != '') {
            const noAttachmentsWarning = dropZone.data("drop-no-attachments");
            if (noAttachmentsWarning)
                cpiAlert.warning(dropZone.data("drop-no-attachments"));

            cancelDrop(1000);
            return;
        }
    }   

    //Print/Exports -START
    //New Doc Tab 1
    printNewDoc = () => {
        let url = $("body").data("base-url") + "/Shared/DocumentVerification/PrintNewDoc";
        
        const warningMsg = this.getMessageFromData("#dvContentPage", "label-print-warning");
        var ids = "";

        $("input[name='chkNewDocLink']:checked").each(function () {
            ids += this.value + "|";
        });
        const selectedIds = $.unique(ids.split("|").filter(d => d));
        if (!selectedIds || selectedIds.length < 1 || ids === "") {
            this.cpiAlert.warning(warningMsg);
            return;
        }

        cpiLoadingSpinner.show();

        fetch(url, {
            method: "POST",
            headers: {
                Accept: "arraybuffer",
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                IDs: ids,
                ReportFormat: 0
            })
        })
            .then(response => {
                if (!response.ok)
                    throw response;

                return response.blob();
            })
            .then(data => {
                cpiLoadingSpinner.hide();

                const a = document.createElement("a");
                document.body.appendChild(a);
                const blobUrl = window.URL.createObjectURL(data);
                a.href = blobUrl;
                a.download = "Documents For Review";
                a.click();
                setTimeout(() => {
                    window.URL.revokeObjectURL(blobUrl);
                    document.body.removeChild(a);
                }, 0);
            })
            .catch(error => {
                cpiLoadingSpinner.hide();
                if (error.status >= 500)
                    pageHelper.showErrors(error);
                else
                    error.text().then(errorMessage => {
                        pageHelper.showErrors(errorMessage);
                    })
            });
    }

    excelExportNewDoc = () => {
        let url = $("body").data("base-url") + "/Shared/DocumentVerification/ExportToExcelNewDoc";
        
        const warningMsg = this.getMessageFromData("#dvContentPage", "label-print-warning");
        var ids = "";

        $("input[name='chkNewDocLink']:checked").each(function () {
            ids += this.value + "|";
        });
        const selectedIds = $.unique(ids.split("|").filter(d => d));
        if (!selectedIds || selectedIds.length < 1 || ids === "") {
            this.cpiAlert.warning(warningMsg);
            return;
        }
        cpiLoadingSpinner.show();

        fetch(url, {
            method: "POST",
            headers: {
                Accept: "arraybuffer",
                "Content-Type": "application/json",
            },
            body:JSON.stringify(ids)
        })
            .then(response => {
                if (!response.ok)
                    throw response;

                return response.blob();
            })
            .then(data => {
                cpiLoadingSpinner.hide();

                const a = document.createElement("a");
                document.body.appendChild(a);
                const blobUrl = window.URL.createObjectURL(data);
                a.href = blobUrl;
                a.download = "Documents For Review";
                a.click();
                setTimeout(() => {
                    window.URL.revokeObjectURL(blobUrl);
                    document.body.removeChild(a);
                }, 0);
            })
            .catch(error => {
                cpiLoadingSpinner.hide();
                if (error.status >= 500)
                    pageHelper.showErrors(error);
                else
                    error.text().then(errorMessage => {
                        pageHelper.showErrors(errorMessage);
                    })
            });
    }

    //Doc Tab 2
    printDoc = () => {
        let url = $("body").data("base-url") + "/Shared/DocumentVerification/PrintDoc";
        
        const warningMsg = this.getMessageFromData("#dvContentPage", "label-print-warning");
        var ids = "";

        $("input[name='chkDocLink']:checked").each(function () {
            if (($(this).data('system-type') !== '' && $(this).data('system-type') !== 'O' && $(this).data('system-type') !== null)) {
                //ids += this.value + "|";
                let tempId = $(this).data('doc-id');
                let tempSystemType = $(this).data("system-type");
                ids += tempSystemType + "|" + this.value + "|" + tempId + ";";
            }
        });
        const selectedIds = $.unique(ids.split(";").filter(d => d));
        if (!selectedIds || selectedIds.length < 1 || ids === "") {
            this.cpiAlert.warning(warningMsg);
            return;
        }

        cpiLoadingSpinner.show();

        fetch(url, {
            method: "POST",
            headers: {
                Accept: "arraybuffer",
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                IDs: ids,
                ReportFormat: 0
            })
        })
            .then(response => {
                if (!response.ok)
                    throw response;

                return response.blob();
            })
            .then(data => {
                cpiLoadingSpinner.hide();

                const a = document.createElement("a");
                document.body.appendChild(a);
                const blobUrl = window.URL.createObjectURL(data);
                a.href = blobUrl;
                a.download = "Docketing Requests";
                a.click();
                setTimeout(() => {
                    window.URL.revokeObjectURL(blobUrl);
                    document.body.removeChild(a);
                }, 0);
            })
            .catch(error => {
                cpiLoadingSpinner.hide();
                if (error.status >= 500)
                    pageHelper.showErrors(error);
                else
                    error.text().then(errorMessage => {
                        pageHelper.showErrors(errorMessage);
                    })
            });
    }

    excelExportDoc = () => {
        let url = $("body").data("base-url") + "/Shared/DocumentVerification/ExportToExcelDoc";
        
        const warningMsg = this.getMessageFromData("#dvContentPage", "label-print-warning");
        var ids = "";

        $("input[name='chkDocLink']:checked").each(function () {
            if (($(this).data('system-type') !== '' && $(this).data('system-type') !== 'O' && $(this).data('system-type') !== null)) {
                //ids += this.value + "|";
                let tempId = $(this).data('doc-id');
                let tempSystemType = $(this).data("system-type");
                ids += tempSystemType + "|" + this.value + "|" + tempId + ";";
            }
        });
        const selectedIds = $.unique(ids.split(";").filter(d => d));
        if (!selectedIds || selectedIds.length < 1 || ids === "") {
            this.cpiAlert.warning(warningMsg);
            return;
        }
        cpiLoadingSpinner.show();

        fetch(url, {
            method: "POST",
            headers: {
                Accept: "arraybuffer",
                "Content-Type": "application/json",
            },
            body: JSON.stringify(ids)
        })
            .then(response => {
                if (!response.ok)
                    throw response;

                return response.blob();
            })
            .then(data => {
                cpiLoadingSpinner.hide();

                const a = document.createElement("a");
                document.body.appendChild(a);
                const blobUrl = window.URL.createObjectURL(data);
                a.href = blobUrl;
                a.download = "Docketing Requests";
                a.click();
                setTimeout(() => {
                    window.URL.revokeObjectURL(blobUrl);
                    document.body.removeChild(a);
                }, 0);
            })
            .catch(error => {
                cpiLoadingSpinner.hide();
                if (error.status >= 500)
                    pageHelper.showErrors(error);
                else
                    error.text().then(errorMessage => {
                        pageHelper.showErrors(errorMessage);
                    })
            });
    }

    //Act Tab 3
    printActionDoc = () => {
        let url = $("body").data("base-url") + "/Shared/DocumentVerification/PrintActionDoc";
        
        const warningMsg = this.getMessageFromData("#dvContentPage", "label-print-warning");
        var ids = "";

        $("input[name='chkActVerify']:checked").each(function () {
            ids += this.value + "|";
        });
        const selectedIds = $.unique(ids.split("|").filter(d => d));
        if (!selectedIds || selectedIds.length < 1 || ids === "") {
            this.cpiAlert.warning(warningMsg);
            return;
        }
        cpiLoadingSpinner.show();

        fetch(url, {
            method: "POST",
            headers: {
                Accept: "arraybuffer",
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                IDs: ids,
                ReportFormat: 0
            })
        })
            .then(response => {
                if (!response.ok)
                    throw response;

                return response.blob();
            })
            .then(data => {
                cpiLoadingSpinner.hide();

                const a = document.createElement("a");
                document.body.appendChild(a);
                const blobUrl = window.URL.createObjectURL(data);
                a.href = blobUrl;
                a.download = "Dockets For Verification";
                a.click();
                setTimeout(() => {
                    window.URL.revokeObjectURL(blobUrl);
                    document.body.removeChild(a);
                }, 0);
            })
            .catch(error => {
                cpiLoadingSpinner.hide();
                if (error.status >= 500)
                    pageHelper.showErrors(error);
                else
                    error.text().then(errorMessage => {
                        pageHelper.showErrors(errorMessage);
                    })
            });
    }

    excelExportActionDoc = () => {
        let url = $("body").data("base-url") + "/Shared/DocumentVerification/ExportToExcelActionDoc";
        
        const warningMsg = this.getMessageFromData("#dvContentPage", "label-print-warning");
        var ids = "";
        $("input[name='chkActVerify']:checked").each(function () {
            ids += this.value + "|";
        });
        const selectedIds = $.unique(ids.split("|").filter(d => d));

        if (!selectedIds || selectedIds.length < 1 || ids === "") {
            this.cpiAlert.warning(warningMsg);
            return;
        }
        cpiLoadingSpinner.show();

        fetch(url, {
            method: "POST",
            headers: {
                Accept: "arraybuffer",
                "Content-Type": "application/json",
            },
            body: JSON.stringify(ids)
        })
            .then(response => {
                if (!response.ok)
                    throw response;

                return response.blob();
            })
            .then(data => {
                cpiLoadingSpinner.hide();

                const a = document.createElement("a");
                document.body.appendChild(a);
                const blobUrl = window.URL.createObjectURL(data);
                a.href = blobUrl;
                a.download = "Dockets For Verification";
                a.click();
                setTimeout(() => {
                    window.URL.revokeObjectURL(blobUrl);
                    document.body.removeChild(a);
                }, 0);
            })
            .catch(error => {
                cpiLoadingSpinner.hide();
                if (error.status >= 500)
                    pageHelper.showErrors(error);
                else
                    error.text().then(errorMessage => {
                        pageHelper.showErrors(errorMessage);
                    })
            });
    }
    //Com Tab 4
    printCommunicationDoc = () => {
        let url = $("body").data("base-url") + "/Shared/DocumentVerification/PrintCommDoc";
        
        const warningMsg = this.getMessageFromData("#dvContentPage", "label-print-warning");
        var ids = "";

        $("input[name='chkCommunicationLink']:checked").each(function () {
            ids += this.value + "|";
        });
        const selectedIds = $.unique(ids.split("|").filter(d => d));

        if (!selectedIds || selectedIds.length < 1 || ids === "") {
            this.cpiAlert.warning(warningMsg);
            return;
        }

        cpiLoadingSpinner.show();

        fetch(url, {
            method: "POST",
            headers: {
                Accept: "arraybuffer",
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                IDs: ids,
                ReportFormat: 0
            })
        })
            .then(response => {
                if (!response.ok)
                    throw response;

                return response.blob();
            })
            .then(data => {
                cpiLoadingSpinner.hide();

                const a = document.createElement("a");
                document.body.appendChild(a);
                const blobUrl = window.URL.createObjectURL(data);
                a.href = blobUrl;
                a.download = "Documents For Sending";
                a.click();
                setTimeout(() => {
                    window.URL.revokeObjectURL(blobUrl);
                    document.body.removeChild(a);
                }, 0);
            })
            .catch(error => {
                cpiLoadingSpinner.hide();
                if (error.status >= 500)
                    pageHelper.showErrors(error);
                else
                    error.text().then(errorMessage => {
                        pageHelper.showErrors(errorMessage);
                    })
            });
    }
    
    excelExportCommunicationDoc = () => {
        let url = $("body").data("base-url") + "/Shared/DocumentVerification/ExportToExcelCommDoc";
        
        const warningMsg = this.getMessageFromData("#dvContentPage", "label-print-warning");
        var ids = "";

        $("input[name='chkCommunicationLink']:checked").each(function () {
            ids += this.value + "|";
        });
        const selectedIds = $.unique(ids.split("|").filter(d => d));
        if (!selectedIds || selectedIds.length < 1 || ids === "") {
            this.cpiAlert.warning(warningMsg);
            return;
        }
        cpiLoadingSpinner.show();

        fetch(url, {
            method: "POST",
            headers: {
                Accept: "arraybuffer",
                "Content-Type": "application/json",
            },
            body: JSON.stringify(ids)
        })
            .then(response => {
                if (!response.ok)
                    throw response;

                return response.blob();
            })
            .then(data => {
                cpiLoadingSpinner.hide();

                const a = document.createElement("a");
                document.body.appendChild(a);
                const blobUrl = window.URL.createObjectURL(data);
                a.href = blobUrl;
                a.download = "Documents For Sending";
                a.click();
                setTimeout(() => {
                    window.URL.revokeObjectURL(blobUrl);
                    document.body.removeChild(a);
                }, 0);
            })
            .catch(error => {
                cpiLoadingSpinner.hide();
                if (error.status >= 500)
                    pageHelper.showErrors(error);
                else
                    error.text().then(errorMessage => {
                        pageHelper.showErrors(errorMessage);
                    })
            });
    }
    //Print -END

    //iManage -START
    editIMDocument(systemType, driveItemId, parentId, docName) {
        //const iManage = this.page.find(".imanage-work");
        const self = this; 

        let url = $("body").data("base-url");
        var param = {
            driveItemId: driveItemId,
            systemType: systemType,
            parentId: parentId
        };

        if (driveItemId && driveItemId > '' && systemType && systemType > '' && parentId && parentId > 0) {
            url += "/iManageWork/DVEditDocument";
        }
        else {
            url += "/iManageWork/DVUploadDocument";
            param = { driveItemId: driveItemId };
        }        
        
        cpiLoadingSpinner.show();
        $.get(url, param)
            .done((response) => {     
                cpiLoadingSpinner.hide();                
                documentVerificationPage.showIMEditor(docName, response, (e) => {                    
                    //this.dropOnSuccess({ response: e }, refreshDocuments);      
                    pageHelper.handleEmailWorkflow(e);
                    self.refreshDocListView();
                    self.refreshCriteriaDS();
                }, (error) => {
                    //refreshDocuments();
                });
            })
            .fail((e) => {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(e.responseText);              
            });
    }

    showIMEditor = (title, content, onSuccess, onError) => {
        cpiConfirm.save(title, content, (e) => {
            const docEditor = $(".document-editor");
            const form = docEditor.find("form");
            const remarks = docEditor.find("#Remarks").val();
            const formFile = docEditor.find("#FormFile");
            const files = formFile.data("kendoUpload").getFiles();
            const payLoad = pageHelper.formDataToJson(form).payLoad;
            const formData = new FormData();                    
            const verificationToken = $(this.refineSearchContainer).find("input[name=__RequestVerificationToken]").val();

            //validate required file
            if (files.length == 0 && formFile.data("required")) {
                formFile.closest(".file-upload").find(".field-validation-valid").addClass("field-validation-error").text(formFile.data("required"));
                throw formFile.data("required");
            }

            for (const key in payLoad) {
                if (payLoad.hasOwnProperty(key)) {
                    //split array values
                    if (Array.isArray(payLoad[key])) {
                        for (const item of payLoad[key]) {
                            formData.append(key, item);
                        }
                    }
                    else {
                        formData.append(key, payLoad[key]);
                    }
                }
            }

            if (files.length > 0)
                formData.append("FormFile", files[0].rawFile);

            if (remarks)
                formData.append("Remarks", remarks);

            var actGrid = $("#documentVerificationActGrid").data("kendoGrid");
            ////show error if IsActRequired = true and Action grid is empty
            //if (formData && formData.has('IsActRequired') && formData.get('IsActRequired') === 'true' && actGrid) {
            //    var gridData = actGrid.dataSource.data();
            //    if (gridData && gridData.length <= 0) {
            //        var actGridElement = $("#documentVerificationActGrid");
            //        actGridElement.closest(".document-verification").find(".field-validation-valid").addClass("field-validation-error").text(actGridElement.data("required"));
            //        throw actGridElement.data("required");                   
            //    }                
            //}

            cpiConfirm.keepOpen = true;

            const actGridDefer = $.Deferred();
            //auto save changes on DocVerification Action grid            
            if (actGrid && actGrid.dataSource.hasChanges()) {
                $.when(pageHelper.kendoGridSave({
                    name: 'documentVerificationActGrid'
                    //popup windows closes after saving, no need to refresh
                    //,afterSubmit: () => { $("#documentVerificationActGrid").data("kendoGrid").dataSource.read(); }
                }))
                .then(
                    function (e) {                  
                        actGridDefer.resolve();
                    },
                    function (e) {                        
                        actGridDefer.reject(e);                        
                    },
                    null
                );                
            }
            else {                
                actGridDefer.resolve();
            }

            $.when(actGridDefer).done(function() {
                pageHelper.hideErrors();
                cpiLoadingSpinner.show();                
                $.ajax({
                    type: form[0].method,
                    url: form[0].action,
                    headers: { "RequestVerificationToken": verificationToken },
                    data: formData,
                    processData: false,
                    contentType: false,
                    success: (response) => {
                        const modalDialog = $("#cpiConfirm").find(".modal-dialog");
                        if (modalDialog.length > 0) {
                            modalDialog.css({
                                "max-width": "",
                                "max-height": ""
                            });
                        }
                        cpiConfirm.close();
                        cpiLoadingSpinner.hide();
                        if (onSuccess)
                            onSuccess(response);                        
                    },
                    error: (error) => {                        
                        cpiLoadingSpinner.hide();
                        pageHelper.showErrors(error);                        
                        if (onError)
                            onError(error);                        
                    }
                });
            })
            .fail(function(error) {                
                pageHelper.showErrors(error);
            });

        }, true, (eCancel) => {
            const modalDialog = $("#cpiConfirm").find(".modal-dialog");
            if (modalDialog.length > 0) {
                modalDialog.css({
                    "max-width": "",
                    "max-height": ""
                });
            }
        }, true);
    };
    //iManage -END

    getMessageFromData(containerSelector, dataKey) {
        const $container = $(containerSelector);
        if ($container.length) {
            return $container.data(dataKey);
        }
        return "";
    }

    isElementInViewport(el) {
        const rect = el.getBoundingClientRect();

        return (
            rect.top >= 0 &&
            rect.left >= 0 &&
            rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
            rect.right <= (window.innerWidth || document.documentElement.clientWidth)
        );
    }

    syncStickyHeader(realId, fakeId) {
        const $real = $(realId);
        const $fake = $(fakeId);

        if ($real.length === 0 || $fake.length === 0) return;

        if (this.isElementInViewport($real[0])) {
            $fake.hide();
        } else {
            // Sync width and horizontal position (offset)
            const realWidth = $real.outerWidth();
            const realOffset = $real.offset().left;

            $fake.css({
                "width": realWidth,
                "left": realOffset,
                "display": "block"
            });
        }
    }
}