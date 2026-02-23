
import Image from "../image";
import SearchPage from "../searchPage";

export default class QuickDocket extends SearchPage {

    constructor() {
        super();
        this.image = new Image();
        this.allEntitiesState = "";
        this.allRemarksState = "";
        this.allDdkState = "";
        this.isDirty = false;
        this.dedocketSystems = "";
        this.dedocketIndicators = "";
        this.qdSelected = [];
    }

    initializeQuickDocket() {
        const self = this;
        $(document).ready(() => {
            $(".container-crumbs").addClass("d-none");
            $("#quickDocket-tab").parent().removeClass("mt-2");

            this.configureQuickDocketTab();

            $('#quickDocket-tab a').on('click', (e) => {
                e.preventDefault();

                const tab = e.target.id;
                const qdSearchNavBanner = $(this.refineSearchContainer).find(".qdSearchNavBanner");
                qdSearchNavBanner.hide();

                if (tab !== "quickDocketCalendar-tab") {
                    $("#calendar").removeClass("active show");
                }
                if (tab !== "quickDocketDefaultSettings-tab") {
                    $("#defaultSettings").removeClass("active show");
                }

                if (tab === "quickDocketCalendar-tab") {
                    const container = $("#calendar");

                    if (container.find('#schedulerContainer').length === 0) {
                        const url = container.data("url");
                        $.get(url, (result) => {
                            container.html(result);
                            this.configureCalendarTab();
                        });
                    }

                    qdSearchNavBanner.show();
                }

                else if (tab === "quickDocketDefaultSettings-tab") {
                    const container = $("#defaultSettings");

                    if (container.find('#defaultSettingsContainer').length === 0) {
                        const url = container.data("url");
                        const systemType = container.data("system");
                        $.get(`${url}?systemType=${systemType}`, (result) => {
                            container.html(result);
                            container.find("#qdDefaultSettingsForm").floatLabels();
                            this.configureDefaultSettingsTab();
                        });
                    }
                }
            });
            $('#qdContentPage').on("click", ".print-record", () => {
                this.print();
            });
            $('#qdContentPage').on("click", ".excel-export", () => {
                this.exportToExcel();
            });
            $('#qdContentPage').on("click", ".qd-save-all", () => {
                const baseUrl = $("body").data("base-url");
                const url = `${baseUrl}/Shared/QuickDocket/Save`;
                const qdDataSource = $('#quickDocketSearchResults-Grid').data('kendoListView').dataSource;
                const data = qdDataSource.data().filter(item => item.DateTakenDirty || item.RemarksDirty || item.DeDocketDirty).map((item) => {
                    return {
                        DDId: item.DDId,
                        ActId: item.ActId,
                        DateTaken: pageHelper.cpiDateFormatToSave(item.DateTaken),
                        /*ActionDueRemarks: item.ActionDueRemarks,*/
                        QDRemarks: item.QDRemarks,
                        DateTakenDirty: item.DateTakenDirty,
                        RemarksDirty: item.RemarksDirty,
                        DeDocketDirty: item.DeDocketDirty,
                        System: item.System,
                        RespOffice: item.RespOffice,
                        tStamp: item.tStamp,
                        ddTStamp: item.ddTStamp,
                        country: item.Country
                    }
                });

                $.post(url, { data })
                    .done(function () {
                        self.isDirty = false;
                        //$('#qdContentPage').find(".qd-mu-dtaken").removeClass("d-none");
                        qdDataSource.read();
                    })
                    .fail(function (e) {
                        pageHelper.showErrors(e.responseText);
                    });
            });
            $('#qdContentPage').on("click", ".qd-cancel-all", () => {
                const confirmTitle = $("#quickDocketContainer").data("confirm-title");
                const confirmMsg = $("#quickDocketContainer").data("cancel-confirm-message");

                cpiConfirm.confirm(confirmTitle, confirmMsg, function () {
                    self.isDirty = false;
                    //$('#qdContentPage').find(".qd-mu-dtaken").removeClass("d-none");
                    $('#quickDocketSearchResults-Grid').data('kendoListView').dataSource.read();
                });
            });

            $('#qdContentPage').on("click", ".qd-mu-dtaken", () => {
                const baseUrl = $("body").data("base-url");
                const url = `${baseUrl}/Shared/QuickDocket/UpdateDateTakenScreen`;

                $.get(url)
                    .done((result) => {
                        const popupContainer = $(".cpiContainerPopup").last();
                        popupContainer.html(result);
                        const dialogContainer = $("#qdUpdateDateTakenDialog");
                        let entryForm = dialogContainer.find("form")[0];
                        dialogContainer.modal("show");
                        entryForm = $(entryForm);

                        entryForm.on("change", "input[name='DateType']", function () {
                            if ($(this).val() == "3") {
                                entryForm.find(".custom-date").removeClass("d-none");
                            }
                            else {
                                entryForm.find(".custom-date").addClass("d-none");
                            }
                        });

                        entryForm.on("submit", (e) => {
                            e.preventDefault();
                            e.stopPropagation();

                            $.post($(e.target).attr("action"), {
                                dateParam: pageHelper.formDataToJson(entryForm).payLoad,
                                searchCriteria: JSON.stringify(self.getCriteria(self.refineSearchContainer)),
                                recIds: self.qdSelected
                            }).done(() => {
                                $('#quickDocketSearchResults-Grid').data('kendoListView').dataSource.read();
                                $('#qdContentPage').find(".qd-mu-dtaken").addClass("d-none");
                                self.qdSelected = [];
                                dialogContainer.modal("hide");

                            }).fail((e) => {
                                pageHelper.showErrors(e);
                            });
                        });
                    })
                    .fail((e => {
                        pageHelper.showErrors(e);
                    }));
            });

            $('#qdContentPage').on("click", ".qd-mu-ddinstrx", () => {
                const baseUrl = $("body").data("base-url");
                const url = `${baseUrl}/Shared/QuickDocket/UpdateDeDocketInstrxScreen`;

                $.get(url, { searchCriteria: JSON.stringify(self.getCriteria(self.refineSearchContainer)) })
                    .done((result) => {
                        const popupContainer = $(".cpiContainerPopup").last();
                        popupContainer.html(result);
                        const dialogContainer = $("#qdUpdateDDInstructionDialog");
                        let entryForm = dialogContainer.find("form")[0];
                        dialogContainer.modal("show");
                        entryForm = $(entryForm);

                        entryForm.on("submit", (e) => {
                            e.preventDefault();
                            e.stopPropagation();

                            $.post($(e.target).attr("action"), {
                                ddParam: pageHelper.formDataToJson(entryForm).payLoad,
                                searchCriteria: JSON.stringify(self.getCriteria(self.refineSearchContainer)),
                                recIds: self.qdSelected
                            }).done((result) => {
                                $('#quickDocketSearchResults-Grid').data('kendoListView').dataSource.read();
                                $('#qdContentPage').find(".qd-mu-ddinstrx").addClass("d-none");
                                self.qdSelected = [];

                                if (result.emailWorkflows.length > 0) {
                                    const promise = pageHelper.handleEmailWorkflow(result);
                                    promise.then(() => {
                                        dialogContainer.modal("hide");
                                    });
                                }
                                else {
                                    dialogContainer.modal("hide");
                                }
                                

                            }).fail((e) => {
                                pageHelper.showErrors(e);
                            });
                        });
                    })
                    .fail((e => {
                        pageHelper.showErrors(e);
                    }));
            });

            $("#quickDocketContainer").append($("#quickDocketSearchResults-Grid_pager"));

            //file upload
            $("#quickDocketContainer").on("dragover", ".ddd-form", function () {
                $(this).addClass("ddd-form-drop");
            });
            $("#quickDocketContainer").on("dragleave", ".ddd-form", function () {
                $(this).removeClass("ddd-form-drop");
            });
            self.initializeFileUpload();

        });
    }

    initializeFileUpload() {
        //browser's default behavior for files dropped on the document itself is to open it,
        //to avoid that, we need to handle the drop events on the document
        $(document).on("dragenter", function (e) {
            e.stopPropagation();
            e.preventDefault();
        });
        //while you are holding the mouse
        $(document).on("dragover", function (e) {
            e.stopPropagation();
            e.preventDefault();
        });
        $(document).on("drop", function (e) {
            e.stopPropagation();
            e.preventDefault();
        });
    }

    initializeFormFileUpload(form) {
        const entryForm = $(form);

        entryForm.on("drop", function (e) {
            e.preventDefault();


            const files = e.originalEvent.dataTransfer.files;
            const droppedFiles = { files: files };

            const existingFile = entryForm.find(".upload-dd-doc-filename");
            if (existingFile.html().length > 0) {
                const title = entryForm.data("upload-title");
                const msg = entryForm.data("upload-msg");
                cpiConfirm.confirm(title, msg, function () {
                    entryForm.trigger("filesDropped", droppedFiles);
                });
            }
            else {
                entryForm.trigger("filesDropped", droppedFiles);
            }

        });

        entryForm.on("filesDropped", function (e, droppedFiles) {
            entryForm.find(".upload-dd-doc-filename").text(droppedFiles.files[0].name);
            entryForm.find("#DocFile")[0].files = droppedFiles.files; //only the 1st one is loaded
            entryForm.removeClass("ddd-form-drop");
        });
    }

    configureQuickDocketTab() {
        const self = this;

        this.configureColumnSort();

        const headerContainer = $(".qdHeaderContainer");

        headerContainer.find("#ddkHeader").click((e) => {
            this.toggleAllDeDocket(e.currentTarget);
        });
        headerContainer.find("#remarksHeader").click((e) => {
            this.toggleAllRemarks(e.currentTarget);
        });

        $("#quickDocketContainer").on("click", "#entitiesHeader", (e) => {
            this.toggleAllEntities(e.currentTarget);
        });
        $("#quickDocketContainer").on("click", ".entities-container", (e) => {
            this.toggleEntities(e.currentTarget);
        });

        $("#quickDocketContainer").on("click", ".remarks-icon-container", (e) => {
            this.toggleRemarks(e.currentTarget);
        });

        $("#quickDocketContainer").on("click", ".remarks-edit-icon", (e) => {
            e.preventDefault();
            const el = $(e.currentTarget);
            const id = el.data("id");
            const remarksRow = el.closest("tr").next();

            if (remarksRow.find(`#remarks_${id}`).hasClass("d-none")) {
                remarksRow.find(`#remarks_${id}`).removeClass("d-none");
                remarksRow.find(`#remarks_edit_${id}`).addClass("d-none");
                remarksRow.find(".remarks-icon-container").removeClass("d-none");
                el.siblings(".ddk-edit-icon").show();
            }
            else {
                remarksRow.find(`#remarks_${id}`).addClass("d-none");
                remarksRow.find(`#remarks_edit_${id}`).removeClass("d-none");
                remarksRow.find(".remarks-icon-container").addClass("d-none");
                el.siblings(".ddk-edit-icon").hide();
            }

        });

        $("#quickDocketContainer").on("click", ".ddk-container", (e) => {
            this.toggleDeDocket(e.currentTarget);
        });

        $("#quickDocketContainer").on("click", ".ddk-edit-icon", (e) => {
            e.preventDefault();
            const el = $(e.currentTarget);
            const id = el.data("id");
            el.hide();

            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Shared/QuickDocket/GetDeDocketEntry`;
            const form = $(`#deDocketForm_${id}`);
            const ddId = form.find("[name='DDId']").val();
            const deDocketId = form.find("[name='DeDocketId']").val();
            const system = form.find("[name='System']").val();
            const respOffice = form.find("[name='RespOffice']").val();

            $.get(url, { ddId: ddId, deDocketId: deDocketId, system: system, respOffice: respOffice })
                .done((result) => {
                    $("#quickDocketContainer").find(`#deDocket_${id}`).addClass("d-none");
                    $("#quickDocketContainer").find(`#deDocket_edit_${id}`).removeClass("d-none");
                    $("#quickDocketContainer").find(`#deDocket_edit_${id}`).html(result);
                })
                .fail((e => {
                    pageHelper.showErrors(e);
                }));
        });

        $("#quickDocketContainer").on("click", ".save-icon", (e) => {
            this.saveRemarks(e);
            //this.hideEditRemarks(e);
        });
        $("#quickDocketContainer").on("click", ".cancel-icon", (e) => {
            this.hideEditRemarks(e);
        });
        $("#quickDocketContainer").on("click", ".dd-save-icon", (e) => {
            this.saveDeDocket(e);
            //this.hideEditDeDocket(e);
        });
        $("#quickDocketContainer").on("click", ".dd-cancel-icon", (e) => {
            this.hideEditDeDocket(e);

        });

        $("#quickDocketContainer").on("click", ".upload-dd-doc", (e) => {
            e.preventDefault();
            e.stopPropagation();

            const el = $(e.currentTarget);

            const parent = el.closest(".ddd-form");

            const existingFile = parent.find(".upload-dd-doc-filename");
            if (existingFile.html().length > 0) {
                const title = parent.data("upload-title");
                const msg = parent.data("upload-msg");
                cpiConfirm.confirm(title, msg, function () {
                    triggerFileUpload();
                });
            }
            else {
                triggerFileUpload();
            }

            function triggerFileUpload() {
                const id = el.data("id");
                const form = $(`#deDocketForm_${id}`);
                const input = form.find("#DocFile");
                input.on("change", (e) => {
                    let fileName = input.val();
                    if (fileName) {
                        const fileSplit = fileName.split("\\");
                        fileName = fileSplit[fileSplit.length - 1];
                        form.find(".upload-dd-doc-filename").html(fileName);
                    }
                });
                input.trigger("click");
            }
        });

        $("#quickDocketContainer").on("click", ".delete-file-link", (e) => {
            e.preventDefault();
            const el = $(e.currentTarget);
            const ddId = el.data("dd-id");

            const parent = el.closest(".ddd-form");
            const url = parent.data("delete-file-url");
            const deDocketId = el.data("ddd-id");
            const actId = el.data("act-id");
            const fileName = el.data("file-name");

            const title = parent.data("upload-title");
            const msg = parent.data("delete-file-msg");
            cpiConfirm.confirm(title, msg, function () {
                $.post(url, { ddId, deDocketId, actId, fileName })
                    .done(function () {
                        el.closest(".upload-dd-doc-filename").empty();
                    })
                    .fail(function (e) {
                        pageHelper.showErrors(e.responseText);
                    });
            });
        });


        $("#quickDocketContainer").on("click", ".attach-doc", (e) => {
            e.preventDefault();
            e.stopPropagation();
            const el = $(e.currentTarget);
            const id = el.data("id");
            const system = el.data("system");
            const respOffice = el.data("resp-office");
            const hasSharePoint = el.data("sharepoint-on");

            if (id && system) {
                const baseUrl = $("body").data("base-url");
                let url = "";
                let documentLink = "";
                let systemName = "";
                if (system === "P" || system === "L") {
                    documentLink = `P|Act|ActId|${id}`;
                    systemName = "Patent";
                }

                else if (system === "T" || system === "M") {
                    documentLink = `T|Act|ActId|${id}`;
                    systemName = "Trademark";
                }
                else {
                    documentLink = `G|Act|ActId|${id}`;
                    systemName = "GeneralMatter";
                }
                if (hasSharePoint) {
                    let retry = 0;
                    sharePointDedocketImageEntry();

                    function sharePointDedocketImageEntry() {
                        url = `${baseUrl}/Shared/SharePointGraph/ImageAddDedocket?documentLink=${documentLink}&system=${systemName}&respOffice=${respOffice}`;
                        $.get(url)
                            .done(function (result) {
                                const popupContainer = $(".site-content .popup");
                                popupContainer.html(result);
                                const dialogContainer = $("#documentEditorDialog");

                                let entryForm = dialogContainer.find("form")[0];
                                dialogContainer.modal("show");
                                entryForm = $(entryForm);
                                entryForm.cpiPopupEntryForm(
                                    {
                                        dialogContainer: dialogContainer,
                                        closeOnSubmit: false,
                                        beforeSubmit: function () {
                                            cpiLoadingSpinner.show();
                                        },
                                        afterSubmit: function () {
                                            cpiLoadingSpinner.hide();
                                            dialogContainer.modal("hide");
                                        }
                                    }
                                );
                            })
                            .fail(function (e) {
                                cpiLoadingSpinner.hide();

                                if (e.status == 401) {
                                    const baseUrl = $("body").data("base-url");
                                    const url = `${baseUrl}/Graph/SharePoint`;

                                    sharePointGraphHelper.getGraphToken(url, () => {
                                        retry++;
                                        if (retry < 3) {
                                            sharePointDedocketImageEntry();
                                        }
                                    });
                                }
                                else {
                                    pageHelper.showErrors(e.responseText);
                                }

                            });
                    }
                }
                else {
                    url = `${baseUrl}/Shared/DocDocuments/ImageAddDedocket?documentLink=${documentLink}&system=${systemName}&respOffice=${respOffice}`;
                    this.image.container = $("#quickDocketContainer")
                    this.image.hasViewScreen = false;
                    this.image.openPopEditor(url);
                }

            }

        });

        $("#quickDocketContainer").on("click", ".open-history", (e) => {
            e.preventDefault();
            e.stopPropagation();
            const el = $(e.currentTarget);
            const id = el.data("id");
            const system = el.data("system");
            if (id && system) {
                const baseUrl = $("body").data("base-url");
                let url = "";
                if (system === "P" || system === "L")
                    url = `${baseUrl}/Patent/ActionDueDate/DeDocketHistory?ddId=${id}`;
                else if (system === "T" || system === "M")
                    url = `${baseUrl}/Trademark/ActionDueDate/DeDocketHistory?ddId=${id}`;
                else
                    url = `${baseUrl}/GeneralMatter/ActionDueDate/DeDocketHistory?ddId=${id}`;

                $.get(url).done(function (result) {
                    const popupContainer = $(".cpiContainerPopup").last();
                    popupContainer.html(result);
                    const dialogContainer = popupContainer.find("#deDocketHistoryDialog");
                    dialogContainer.modal("show");
                });
            }
        });

        $("#quickDocketSearchDeDocketTabContent").on("click", "input[name='DeDocketInstructionOnly']", function () {
            const completedOptions = $("#quickDocketSearchDeDocketTabContent").find(".dedocket-completed-switch");
            if ($(this).is(":checked")) {
                completedOptions.removeClass("d-none");
                $("#quickDocketSearchDeDocketTabContent").find("input[name='DeDocketUninstructedOnly']").prop("checked", false);
            }
            else {
                completedOptions.addClass("d-none");
            }
        });
        $("#quickDocketSearchDeDocketTabContent").on("click", "input[name='DeDocketUninstructedOnly']", function () {
            //if ($(this).is(":checked")) {
            //    $("#quickDocketSearchDeDocketTabContent").find("input[name='DeDocketInstructionOnly']").prop("checked", false);
            //}
            const unInstructedOptions = $("#quickDocketSearchDeDocketTabContent").find(".dedocket-uninstructed-switch");
            if ($(this).is(":checked")) {
                unInstructedOptions.removeClass("d-none");
                $("#quickDocketSearchDeDocketTabContent").find("input[name='DeDocketInstructionOnly']").prop("checked", false);
            }
            else {
                unInstructedOptions.addClass("d-none");
            }
        });

        $("#quickDocketSearchResults-Grid").on("click", ".date-taken-default", function () {
            const dateTaken = $(this).siblings("span.date-taken-entry").find("input.date-taken-entry").data("kendoDatePicker");
            const currentDate = new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate());
            dateTaken.value(currentDate);
            dateTaken.trigger("change");
        });

        //selection checkbox
        self.qdSelected = [];
        $("#quickDocketContainer").on("input", ".k-selector-all", (e) => {
            const checkbox = $(e.target);
            const selected = checkbox.prop("checked");

            $("#quickDocketSearchResults-Grid").find("input.k-selector").each((i, cb) => {
                const cbRow = $(cb).closest("tr");
                const id = cbRow.data("system") + '-' + cbRow.data("rec-id");

                const index = self.qdSelected.indexOf(id);
                if (selected) {
                    $(cb).prop("checked", true);
                    if (index === -1) {
                        self.qdSelected.push(id);
                    }
                }
                else {
                    $(cb).prop("checked", false);
                    if (index >= 0) {
                        self.qdSelected.splice(index, 1);
                    }
                }
            });
            self.toggleMassUpdateIcon();
        });
        $("#quickDocketContainer").on("input", "input.k-selector", (e) => {
            const checkbox = $(e.target);

            const cbRow = checkbox.closest("tr");
            const id = cbRow.data("system") + '-' + cbRow.data("rec-id");
            const index = self.qdSelected.indexOf(id);

            if (checkbox.prop("checked")) {
                if (index === -1) {
                    self.qdSelected.push(id);
                }
            }
            else {
                if (index >= 0) {
                    self.qdSelected.splice(index, 1);
                }
            }
            self.toggleMassUpdateIcon();
        });
        //


        $(window).on('scroll', () => {
            const targetElement = $("#qdHeaderContainer_real")[0];

            if (this.isElementInViewport(targetElement)) {
                $("#qdHeaderContainer_fake").hide();
            } else {
                $("#qdHeaderContainer_fake").show();
            }
        })
        $("#qdHeaderContainer_fake").hide();
    }

    toggleMassUpdateIcon() {
        if (this.qdSelected.length > 0) {
            $('#qdContentPage').find(".qd-mu-dtaken").removeClass("d-none");
            $('#qdContentPage').find(".qd-mu-ddinstrx").removeClass("d-none"); //totally hidden if user is not a dedocketer
        }
        else {
            $('#qdContentPage').find(".qd-mu-dtaken").addClass("d-none");
            $('#qdContentPage').find(".qd-mu-ddinstrx").addClass("d-none");
        }
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

    configureColumnSort() {
        const container = $(".qdHeaderContainer");

        //const dueDate = container.find("#DueDate");
        const dueDate = container.find("[data-field='DueDate']"); 
        dueDate.append('<span data-sort="DueDateSortIcon" class="k-icon k-i-sort-asc-sm"></span>');

        const self = this;
        container.find("a").each(function () {
            const el = $(this);
            const dataField = el.data("field"); 
            el.on('click', function () {
                const els = container.find(`[data-field='${dataField}']`);
                self.addSortIcon(els);
            });
            
        });
    }

    addSortIcon(els) {
        const self = this;
        const sortAscIcon = "k-icon k-i-sort-asc-sm";
        const sortDescIcon = "k-icon k-i-sort-desc-sm";

        const sortColumn = $(els[0]).data("field");
        self.hideSortIcons(sortColumn);

        let sortOrder = "ASC";

        els.each(function () {
            const el = $(this);

            const sortIconId = sortColumn + "SortIcon";
            const sortIcon = el.find(`[data-sort='${sortIconId}']`);
            if (sortIcon.length === 0) {
                el.append("<span data-sort=" + sortIconId + " class='" + sortAscIcon + "'></span>");
            }
            else {
                const spanClass = sortIcon.attr("class");
                if (spanClass === undefined) {
                    sortIcon.addClass(sortAscIcon);
                }
                else if (spanClass === sortAscIcon) {
                    sortIcon.removeClass().addClass(sortDescIcon);
                    sortOrder = "DESC";
                }
                else {
                    sortIcon.removeClass().addClass(sortAscIcon);
                }
            }

        }) 
        self.refreshListView(sortColumn, sortOrder);
    }

    hideSortIcons(columnClicked) {
        const headerContainer = $(".qdHeaderContainer");
        headerContainer.find("a").each(function () {
            const column = $(this).data("field");

            // hide Sort icons except column clicked 
            if (columnClicked !== column) {
                const sortIcon = headerContainer.find(`[data-sort='${column}SortIcon']`);
                if (sortIcon.length > 0) {
                    sortIcon.removeClass();
                }
            }
        });
    }

    refreshListView(sortColumn, sortOrder) {
        const dataSource = $('#quickDocketSearchResults-Grid').data('kendoListView').dataSource;
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

    hideEditRemarks(e, action, remarks) {
        e.preventDefault();
        const parent = $(e.currentTarget).parents(".edit-remarks");
        parent.addClass("d-none");

        const id = parent.attr("id").split("_")[2];

        $(`#remarks_icon_${id}`).show();
        $(`#deDocket_icon_${id}`).show();

        const remarksContainer = $(`#remarks_${id}`);

        if (action === "save") {
            remarksContainer.find(".ro-remarks-value").html(remarks);
            $(`#form_${id}`).find("[name='QDRemarks']").val(remarks);
        }
        else {
            var oldValue = remarksContainer.find(".ro-remarks-value").html();
            $(`#form_${id}`).find("[name='QDRemarks']").val(oldValue.trim());
        }

        if (remarksContainer.find(".ro-remarks-value").html().trim().length > 0) {
            remarksContainer.removeClass("d-none");
            //remarksContainer.find(".ro-remarks").removeClass("d-none");
            remarksContainer.closest("tr").find(".remarks-icon-container").removeClass("d-none");
        }
    }

    saveRemarks(e) {
        const parent = $(e.currentTarget).parents(".edit-remarks");
        const id = parent.attr("id").split("_")[2];
        const form = $(`#form_${id}`);

        const remarks = form.find("[name='QDRemarks']").val();
        $.ajax({
            type: "POST",
            url: form.attr("action"),
            data: form.serialize(),
            success: (tStamp) => {
                form.find("[name='tStamp']").val(tStamp);
                this.hideEditRemarks(e, "save", remarks);

                const remarksIcon = $(`#remarks_icon_${id}`);
                if (remarks) {
                    remarksIcon.addClass("fa-comment-alt-lines");
                    remarksIcon.removeClass("fa-comment-alt")
                }
                else {
                    remarksIcon.removeClass("fa-comment-alt-lines");
                    remarksIcon.addClass("fa-comment-alt")
                }
            },
            error: function (e) {
                pageHelper.showErrors(e);
            }
        });
    }

    hideEditDeDocket(e) {
        e.preventDefault();
        const parent = $(e.currentTarget).parents(".edit-deDocket");
        parent.addClass("d-none");

        const id = parent.attr("id").split("_")[2];

        $(`#deDocket_icon_${id}`).show();
        $(`#remarks_icon_${id}`).show();

        const deDocketContainer = $(`#deDocket_${id}`);

        if (deDocketContainer.find(".deDocket .ddk-instruction").html() !== "null") {
            deDocketContainer.removeClass("d-none");
        }

    }

    saveDeDocket(e) {
        const parent = $(e.currentTarget).parents(".edit-deDocket");
        const id = parent.attr("id").split("_")[2];
        const form = $(`#deDocketForm_${id}`);

        form.off("submit");
        form.kendoValidator();
        form.on("submit", (se) => {
            se.preventDefault();
            se.stopPropagation();

            if (form.valid()) {
                const formData = new FormData(se.target);
                $.ajax({
                    type: "POST",
                    url: form.attr("action"),
                    //data: form.serialize(),
                    data: formData,
                    contentType: false, // needed for file upload
                    processData: false, // needed for file upload
                    success: (result) => {

                        form.find("[name='DeDocketId']").val(result.DeDocketId);
                        form.find("[name='tStamp']").val(result.tStamp);

                        const displayContainer = $(`#deDocket_${id}`);
                        if (displayContainer) {
                            displayContainer.find(".ddk-instruction").html(result.Instruction);
                            displayContainer.find(".ddk-remarks").html(result.Remarks);
                            displayContainer.find(".ddk-instructed-by").html(result.InstructedBy);
                            displayContainer.find(".ddk-instruction-date").html(pageHelper.cpiDateFormatToDisplay(new Date(result.InstructionDate)));
                            displayContainer.find(".ddk-instruction-completed").html(result.CompletedDesc);
                            displayContainer.find(".deDocket-icon").removeClass("fa-plus-square").addClass("fa-minus-square");
                            //displayContainer.find(".deDocket").removeClass("d-none");
                        }
                        this.hideEditDeDocket(e);
                        pageHelper.handleEmailWorkflow(result);
                    },
                    error: function (error) {
                        pageHelper.showErrors(error);
                    }
                });
            }
        });
        form.submit();
    }

    toggleAllRemarks(e) {
        const state = this.toggleIcon(e);

        const self = this;
        $("#quickDocketContainer .ro-remarks").each(function () {
            self.toggleIcon($(this).closest("tr"), state);
            self.allRemarksState = state;

            if (state === "-") {
                $(this).removeClass("d-none");
            }
            else
                $(this).addClass("d-none");
        });
    }

    toggleRemarks(e) {
        this.toggleIcon(e);
        const remarks = $(e).closest("tr").find(".ro-remarks");
        remarks.toggleClass("d-none");
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

    toggleEntities(e) {
        this.toggleIcon(e);
        const entities = $(e).siblings(".entities");
        entities.toggleClass("d-none");
    }

    toggleAllEntities(e) {
        const state = this.toggleIcon(e);

        const self = this;
        $("#quickDocketContainer .entities").each(function () {
            self.toggleIcon($(this).parent(), state);
            self.allEntitiesState = state;

            if (state === "-") {
                $(this).removeClass("d-none");
            }
            else
                $(this).addClass("d-none");
        });
    }

    toggleAllDeDocket(e) {
        const state = this.toggleIcon(e);

        const self = this;
        $("#quickDocketContainer .ddk").each(function () {
            self.toggleIcon($(this).parent(), state);
            self.allDdkState = state;

            if (state === "-") {
                $(this).removeClass("d-none");
            }
            else
                $(this).addClass("d-none");
        });
    }

    toggleDeDocket(e) {
        this.toggleIcon(e);
        const ddks = $(e).siblings(".ddk");
        ddks.toggleClass("d-none");
    }

    configureDefaultSettingsTab() {
        this.configureDueDateRange();
        this.configureBaseDateRange();
        this.configureDeDocketInstrxDateRange();
    }

    configureDueDateRange(currentId) {
        if (currentId === undefined) {
            const dueDateRangeType = $("input[name=DueDateRange]:checked");
            if (dueDateRangeType.length > 0) {
                currentId = dueDateRangeType[0].id;
            }
            else
                currentId = "dueDateTimeFrame";
        }

        const defaultSettingsContainer = $("#defaultSettingsContainer");
        defaultSettingsContainer.find("input[name=DueDateRange]:radio").each(function () {
            const id = $(this)[0].id;
            const selector = "#" + id + "Row";
            if (id === currentId) {
                $(selector).show();
            }
            else {
                $(selector).hide();
            }
        });

        defaultSettingsContainer.find("#saveSettings").click((e) => {
            e.preventDefault();
            this.saveSettings(e);
        });

        defaultSettingsContainer.find("#isDefaultPageIcon").click((e) => {
            e.preventDefault();
            this.toggleShowAsDefaultPage(e);
        });
        defaultSettingsContainer.find("input[name=DueDateRange]:radio").on("click", (e) => {
            this.configureDueDateRange(e.currentTarget.id);
        });
    }

    configureBaseDateRange(currentId) {
        if (currentId === undefined) {
            const baseDateRangeType = $("input[name=BaseDateRange]:checked");
            if (baseDateRangeType.length > 0) {
                currentId = baseDateRangeType[0].id;
            }
            else
                currentId = "baseDateNone";
        }

        const defaultSettingsContainer = $("#defaultSettingsContainer");
        defaultSettingsContainer.find("input[name=BaseDateRange]:radio").each(function () {
            const id = $(this)[0].id;
            const selector = "#" + id + "Row";

            if (id === currentId) {
                $(selector).show();
            }
            else {
                $(selector).hide();
            }
        });
        defaultSettingsContainer.find("input[name=BaseDateRange]:radio").on("click", (e) => {
            this.configureBaseDateRange(e.currentTarget.id);
        });
    }

    configureDeDocketInstrxDateRange(currentId) {
        if (currentId === undefined) {
            const deDocketInstrxDateRangeType = $("input[name=DeDocketInstrxDateRange]:checked");
            if (deDocketInstrxDateRangeType.length > 0) {
                currentId = deDocketInstrxDateRangeType[0].id;
            }
            else
                currentId = "deDocketInstrxDateNone";
        }

        const defaultSettingsContainer = $("#defaultSettingsContainer");
        defaultSettingsContainer.find("input[name=DeDocketInstrxDateRange]:radio").each(function () {
            const id = $(this)[0].id;
            const selector = "#" + id + "Row";

            if (id === currentId) {
                $(selector).show();
            }
            else {
                $(selector).hide();
            }
        });
        defaultSettingsContainer.find("input[name=DeDocketInstrxDateRange]:radio").on("click", (e) => {
            this.configureDeDocketInstrxDateRange(e.currentTarget.id);
        });
    }

    saveSettings(e) {
        const form = $("#" + e.target.closest("form").id);
        const isDefaultPage = form.find("#IsDefaultPage");
        isDefaultPage.val(isDefaultPage.is(":checked"));

        $.ajax({
            type: "POST",
            url: form.attr("action"),
            data: form.serialize(),
            success: function () {
                //const successMsg = form.data("save-message");
                //pageHelper.showSuccess(successMsg);
                location.reload();
            },
            error: function (error) {
                pageHelper.showErrors(error);
            }
        });
    }

    toggleShowAsDefaultPage(e) {
        const toggleOnClass = "text-primary far fa-toggle-on fa-2x";
        const toggleOffClass = "text-secondary far fa-toggle-off fa-2x";

        const icon = $(e.target);
        const className = icon.attr("class");
        const isDefaultPage = icon.next();

        if (className.includes("toggle-on")) {
            icon.removeClass().addClass(toggleOffClass);
            isDefaultPage.attr("value", false);
        }
        else {
            icon.removeClass().addClass(toggleOnClass);
            isDefaultPage.attr("value", true);
        }
    }

    requestStart(e) {
        const self = this;

        if (self.isDirty) {
            const message = $("#quickDocketContainer").data("cancel-databind");
            pageHelper.showErrors(message);
            e.preventDefault();
        }
    }

    dataBound(recRows) {
        const self = this;
        const gridData = $('#quickDocketSearchResults-Grid').data('kendoListView').dataSource.data();

        if (recRows === 3) {
            let highlightRow = 4;
            $("#quickDocketSearchResults-Grid tr").each(function (index) {
                const row = index + 1;

                if (row >= highlightRow && row <= highlightRow + 2) {
                    $(this).addClass("altItem");

                    if (row === highlightRow + 2)
                        highlightRow = highlightRow + 6;
                }
            });
        }
        else {
            let highlightRow = 3;
            $("#quickDocketSearchResults-Grid tr").each(function (index) {
                const row = index + 1;

                if (row >= highlightRow && row <= highlightRow + 1) {
                    $(this).addClass("altItem");

                    if (row === highlightRow + 1)
                        highlightRow = highlightRow + 4;
                }
            });

        }

        //expand all entities
        if (self.allEntitiesState === "-") {
            $("#quickDocketContainer .entities").each(function () {
                self.toggleIcon($(this).parent(), self.allEntitiesState);
                $(this).removeClass("d-none");
            });
        }

        if (self.allRemarksState === "-") {
            $("#quickDocketContainer .ro-remarks").each(function () {
                self.toggleIcon($(this).closest("tr"), self.allRemarksState);
                $(this).removeClass("d-none");
            });
        }

        $("#quickDocketSearchResults-Grid").find(".date-taken-entry").kendoDatePicker({
            "format": "dd-MMM-yyyy", "max": new Date(9999, 11, 31, 0, 0, 0, 0), "min": new Date(1800, 0, 1, 0, 0, 0, 0), "parseFormats": ["{0:dd-MMM-yyyy}", "d", "M d yyyy", "M/d/yyyy", "M-d-yyyy", "M.d.yyyy", "MMMM d, yyyy", "MMMM d yyyy", "d MMMM, yyyy", "d MMMM yyyy", "MMM d, yyyy", "MMM. d, yyyy", "M d yy", "M/d/yy", "M-d-yy", "M.d.yy", "M d", "M/d", "M-d", "M.d", "yyyy-M-d"]
        });

        $("#quickDocketSearchResults-Grid").find(".k-datepicker input").each(function () {
            const element = $(this);
            const datePicker = element.data("kendoDatePicker");
            if (datePicker) {
                datePicker.bind("change", () => {
                    const recId = element.closest("tr").data("rec-id");
                    if (recId) {
                        const rec = gridData.find(r => r.DDId === +recId);
                        if (rec) {
                            rec.DateTaken = datePicker.value();
                            rec.DateTakenDirty = true;
                            self.isDirty = true;
                            const contentPage = $('#qdContentPage');
                            contentPage.find(".qd-save-buttons").removeClass("d-none");
                            //contentPage.find(".qd-mu-dtaken").addClass("d-none");
                            contentPage.find(".print-record").addClass("d-none");
                            contentPage.find(".excel-export").addClass("d-none");
                        }
                    }

                });
            }
        });

        $("#quickDocketSearchResults-Grid").on("input", "textarea.action-remarks", function () {
            const element = $(this);
            const recId = element.closest("tr").data("rec-id");
            if (recId) {
                const rec = gridData.find(r => r.DDId === +recId);
                if (rec && rec.QDRemarks != element.val()) {
                    rec.QDRemarks = element.val();
                    rec.RemarksDirty = true;
                    self.isDirty = true;
                    const contentPage = $('#qdContentPage');
                    contentPage.find(".qd-save-buttons").removeClass("d-none");
                    //contentPage.find(".qd-mu-dtaken").addClass("d-none");
                    contentPage.find(".print-record").addClass("d-none");
                    contentPage.find(".excel-export").addClass("d-none");
                }
            }
        });

        //refresh the calendar also
        const calendarGrid = $('#qdScheduler');
        if (calendarGrid.length > 0) {
            cpiLoadingSpinner.show("", 1);
            calendarGrid.data("kendoScheduler").dataSource.read().then(cpiLoadingSpinner.hide());
        }

        const contentPage = $('#qdContentPage');
        contentPage.find(".qd-save-buttons").addClass("d-none");
        contentPage.find(".print-record").removeClass("d-none");
        contentPage.find(".excel-export").removeClass("d-none");

        //const canEditAction = gridData.find(r => r.CanEditAction);
        //if (!canEditAction) {
        //    contentPage.find(".qd-mu-dtaken").remove();
        //}
        //if (canEditAction) {
        //    contentPage.find(".qd-mu-dtaken").removeClass("d-none");
        //}
        //else {
        //    contentPage.find(".qd-mu-dtaken").addClass("d-none");
        //}
        
        self.qdSelected = []; //reset on databound
        const canEditInstruction = gridData.filter(r => r.CanEditInstruction);
        if (canEditInstruction.length <= 0) {
            contentPage.find(".qd-mu-ddinstrx").addClass("d-none");
        }
        else {
            contentPage.find(".qd-mu-dtaken").remove(); //dedodocketer has no access to this
        }

        $("#quickDocketContainer").find("input.k-selector").each((i, cb) => {

            const cbRow = $(cb).closest("tr");
            const id = cbRow.data("system")  + '-' + cbRow.data("rec-id");
            const index = this.qdSelected.indexOf(id);
            if (index >= 0) {
                $(cb).attr("checked", "checked");
            }
        });
    }

    configureCalendarTab() {

        const defaultSettingsContainer = $("#defaultSettingsContainer");
        const schedulerContainer = $("#schedulerContainer");

        defaultSettingsContainer.find("input[name=System]:checkbox").each(function () {
            const selector = "#" + $(this)[0].id;
            if (this.checked) {
                schedulerContainer.find(selector).prop('checked', true);
            }
            else {
                schedulerContainer.find(selector).prop('checked', false);
            }

        });

        const calendarGrid = $('#qdScheduler');
        if (calendarGrid.length > 0) {
            const tools = calendarGrid.find(".k-scheduler-toolbar");

            tools.find(".k-pdf").remove();
            tools.append($("#qdSchedulerToolbar").html());

            const form = $("#exportToCalendar");
            $(".k-ics, .k-ics-public").on("click", (e) => {
                form.find("#exportCriteria").val(JSON.stringify(this.getCriteria(this.refineSearchContainer)));
                if ($(e.target).closest("a").hasClass("k-ics-public")) {
                    form.find("#exportLocation").val("public");
                    const fileName = $("#qdScheduler").find("#fileName");

                    if ($(fileName).val().length > 0) {
                        form.find("#exportFileName").val($(fileName).val() + ".ics");
                    }
                    else {
                        $(fileName).css("border", "2px solid #CF4315");
                        return;
                    }
                }
                else {
                    form.find("#exportLocation").val("");
                }
                form.submit();
            });

            form.on("submit", (e) => {
                if (form.find("#exportLocation").val().length > 0) {
                    e.preventDefault();
                    e.stopPropagation();
                    const params = {
                        exportCriteria: form.find("#exportCriteria").val(),
                        exportLocation: "public",
                        exportFileName: form.find("#exportFileName").val()
                    };
                    $.post(form.attr("action"), params)
                        .done(function (result) {
                            const path = $("#exportPublicFullPath");
                            path.val(result.location);
                            path.removeClass("d-none");
                            $("#exportPublicFullPathCopy").removeClass("d-none");
                            pageHelper.showSuccess(result.message);
                        })
                        .fail(function (e) {
                            pageHelper.showErrors(e);
                        });
                }


            });
            $("#exportPublicFullPathCopy").on("click", () => {
                const path = $("#exportPublicFullPath");
                path.select();
                document.execCommand("copy");
            });
        }
    }

    getCriteria(name) {
        const data = pageHelper.formDataToJson($(name)).payLoad;
        if (data.Patent)
            data.Patent = "P";
        if (data.PTOActions)
            data.PTOActions = "L";
        if (data.Trademark)
            data.Trademark = "T";
        if (data.TrademarkLinks)
            data.TrademarkLinks = "M";
        if (data.GeneralMatter)
            data.GeneralMatter = "G";
        if (data.DMS)
            data.DMS = "D";
        if (data.AMS)
            data.AMS = "A";

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

        if (data.Indicators)
            data.Indicators = $("#Indicators_qdCriteria").val();
        if (data.Attorneys)
            data.Attorneys = $("#Attorneys_qdCriteria").val();
        if (data.Clients)
            data.Clients = $("#Clients_qdCriteria").val();
        if (data.ActionTypes)
            data.ActionTypes = $("#ActionTypes_qdCriteria").val();
        if (data.Delegated)
            data.Delegated = data.Delegated == "true";
        if (data.TrackOne)
            data.TrackOne = data.TrackOne == "true";
        if (data.PODocketed)
            data.PODocketed = data.PODocketed == "true";

        data.ToDueDate = this.formatToDateWithTime(data.ToDueDate);
        data.ToBaseDate = this.formatToDateWithTime(data.ToBaseDate);
        data.ToInstrxDate = this.formatToDateWithTime(data.ToInstrxDate);

        return data;
    }

    formatToDateWithTime(toDateString) {
        if (!toDateString)
            return toDateString;

        let toDate = new Date(toDateString);

        toDate = new Date(toDate.setDate(toDate.getDate() + 1));
        toDate = new Date(toDate.setMilliseconds(toDate.getMilliseconds() - 1));

        return pageHelper.cpiDateTimeFormatToSave(toDate)
    }

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

    redirectToAction(system, actId, country) {
        let url = $("body").data("base-url") + "/";

        if (system === "P" && country)
            url = url + "Patent/ActionDue/Detail/" + actId;
        else if (system === "P")
            url = url + "Patent/ActionDueInv/Detail/" + actId;
        else if (system === "L")
            url = url + "Patent/RTSAction/Detail/" + actId;
        else if (system === "T")
            url = url + "Trademark/ActionDue/Detail/" + actId;
        else if (system === "M")
            url = url + "Trademark/TLAction/Detail/" + actId;
        else if (system === "G")
            url = url + "GeneralMatter/Actiondue/Detail/" + actId;
        else if (system === "D")
            url = url + "DMS/Actiondue/Detail/" + actId;
        else
            url = url + "AMS/AnnuitiesDue/Detail/" + actId;

        window.open(url, '_blank');
    }

    redirectToParent(system, id,country) {
        let url = $("body").data("base-url") + "/";
        $.get(url + "Shared/QuickDocket/GetParentId", { systemType: system, actId: id,country:country }).done((result) => {
            if (system === "P")
                url = url + "Patent/CountryApplication/Detail/" + result;
            else if (system === "L")
                url = url + "Patent/CountryApplication/Detail/" + result;
            else if (system === "T")
                url = url + "Trademark/TmkTrademark/Detail/" + result;
            else if (system === "M")
                url = url + "Trademark/TmkTrademark/Detail/" + result;
            else if (system === "G")
                url = url + "GeneralMatter/Matter/Detail/" + result;
            else if (system === "D")
                url = url + "DMS/Disclosure/Detail/" + result;
            window.open(url, '_blank');
        })
    }

    redirectToInvention(id, country) {
        let url = $("body").data("base-url") + "/";
        $.get(url + "Shared/QuickDocket/GetInventionId", { actId: id, country: country }).done((result) => {
            url = url + "Patent/Invention/Detail/" + result;
            window.open(url, '_blank');
        })
    }

    addSoftDocket(system, id, country) {
        let url = $("body").data("base-url") + "/";

        console.log("country", country);
        $.get(url + "Shared/QuickDocket/GetParentId", { systemType: system, actId: id,country:country }).done((result) => {
            if (system === "P" && country > '')
                url = url + "Patent/ActionDue/SoftDocketEntry?parentId=" + result;
            else if (system === "T")
                url = url + "Trademark/ActionDue/SoftDocketEntry?parentId=" + result;
            else if (system === "G")
                url = url + "GeneralMatter/ActionDue/SoftDocketEntry?parentId=" + result;
            else if (system === "P")
                url = url + "Patent/ActionDueInv/SoftDocketEntry?parentId=" + result;

            $.get(url).done((result) => {
                if (result) {
                    $(".cpiContainerPopup").empty();
                    const popupContainer = $(".cpiContainerPopup").last();
                    popupContainer.html(result);
                    const dialog = $("#softDocketDialog");
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
                                    if (result.message) {
                                        pageHelper.showSuccess(result.message);
                                    }

                                    if (result.emailWorkflows) {
                                        pageHelper.handleEmailWorkflow(result);
                                    }
                                }
                            }
                        }
                    );
                }

            }).fail(function (error) {
                pageHelper.showErrors(error.responseText);
            });

        })
    }

    addDocketRequest(system, id, country) {
        let url = $("body").data("base-url") + "/";
        $.get(url + "Shared/QuickDocket/GetParentId", { systemType: system, actId: id, country: country }).done((result) => {
            if (system === "P")
                url = url + "Patent/ActionDue/DocketRequestEntry?parentId=" + result;
            else if (system === "T")
                url = url + "Trademark/ActionDue/DocketRequestEntry?parentId=" + result;
            else if (system === "G")
                url = url + "GeneralMatter/ActionDue/DocketRequestEntry?parentId=" + result;
            else if (system === "PI")
                url = url + "Patent/ActionDueInv/DocketRequestEntry?parentId=" + result;

            $.get(url).done((result) => {
                if (result) {
                    $(".cpiContainerPopup").empty();
                    const popupContainer = $(".cpiContainerPopup").last();
                    popupContainer.html(result);
                    const dialog = $("#docketRequestDialog");
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
                                    if (result.message) {
                                        pageHelper.showSuccess(result.message);
                                    }

                                    if (result.emailWorkflows) {
                                        pageHelper.handleEmailWorkflow(result);
                                    }
                                }
                            }
                        }
                    );
                }

            }).fail(function (error) {
                pageHelper.showErrors(error.responseText);
            });

        })
    }

    miniCalendarOnChange = (e) => {
        e.preventDefault();
        const scheduler = $('#qdScheduler').data("kendoScheduler");

        if (scheduler) {
            const view = scheduler.view();

            scheduler.view("day");
            scheduler.date(new Date(e.sender.value()));
            scheduler.view(view.title.toLowerCase());
        }
    }

    schedulerOnNavigate = (e) => {
        const dayAgendaContainer = $(".calendar .month-day");
        if (e.view == "month")
            dayAgendaContainer.show();
        else
            dayAgendaContainer.hide();
    }

    schedulerOnChange = (e) => {
        e.preventDefault();

        if (e.events.length > 0) {
            const actId = e.events[0].ActId;
            const system = e.events[0].System;
            const country = e.events[0].Country;

            this.redirectToAction(system, actId, country);
        }

        if (e.sender.viewName() == "month") {
            this.showDayAgenda(e.start);
        }
    }

    showDayAgenda = (day) => {
        const dayAgenda = $(".calendar .month-day .agenda");
        const agendaList = dayAgenda.find(".list");
        const agendaSpinner = dayAgenda.find(".spinner");
        const agendaHeader = dayAgenda.find(".header");
        const url = dayAgenda.data("url");
        const criteria = this.getCriteria(this.refineSearchContainer);
        const dueDate = pageHelper.cpiDateFormatToSave(day);

        agendaHeader.text(kendo.toString(new Date(dueDate), "ddd, MMM d"));
        agendaList.html("");

        if ((!criteria.FromDueDate || Date.parse(dueDate) >= Date.parse(criteria.FromDueDate)) &&
            (!criteria.ToDueDate || Date.parse(dueDate) <= Date.parse(criteria.ToDueDate))) {

            criteria.FromDueDate = dueDate;
            criteria.ToDueDate = this.formatToDateWithTime(day);

            agendaSpinner.show();

            $.post(url, criteria)
                .done((result) => {
                    agendaList.html(result);
                    agendaSpinner.hide();
                })
                .fail((e => {
                    pageHelper.showErrors(e);
                    agendaSpinner.hide();
                }));
        }
    }

    schedulerOnDataBound = (e) => {
        e.preventDefault();

        const selected = e.sender.select();
        if (e.sender.viewName() == "month") {
            if (selected.length == 0)
                e.sender.select({ start: new Date(), end: new Date() }); //select current date by default in month view
            else if (quickDocket.refreshCalendar)
                this.showDayAgenda(selected.start); //refresh agenda if search criteria is updated
        }
        quickDocket.refreshCalendar = false;
    }

    schedulerOnError(e) {
        this.cancelChanges();

        let errorMessage = "";
        if (e.errors) {
            for (const error in e.errors) {
                errorMessage += e.errors[error].errors[0] + " ";
            }
        }
        pageHelper.showErrors(errorMessage);
    }

    defaultTitleValueMapper = (options) => {
        const container = "#qdDefaultSettingsForm";
        const url = $(container).data("default-title-mapper-url");
        this.searchValueMapper(url, options);
    }

    defaultCaseNumberValueMapper = (options) => {
        const container = "#qdDefaultSettingsForm";
        const url = $(container).data("default-casenumber-mapper-url");
        this.searchValueMapper(url, options);
    }

    titleValueMapper = (options) => {
        const container = "#quickDocketSearchCaseInfoTabContent";
        const url = $(container).data("title-mapper-url");
        this.searchValueMapper(url, options);
    }

    caseNumberValueMapper = (options) => {
        const container = "#quickDocketSearchCaseInfoTabContent";
        const url = $(container).data("casenumber-mapper-url");
        this.searchValueMapper(url, options);
    }

    actionTypeValueMapper = (options) => {
        const container = "#quickDocketSearchCaseInfoTabContent";
        const url = $(container).data("actiontype-mapper-url");
        this.searchValueMapper(url, options);
    }

    actionDueValueMapper = (options) => {
        const container = "#quickDocketSearchCaseInfoTabContent";
        const url = $(container).data("actiondue-mapper-url");
        this.searchValueMapper(url, options);
    }

    clientValueMapper = (options) => {
        const container = "#quickDocketSearchCaseInfoTabContent";
        const url = $(container).data("client-mapper-url");
        this.searchValueMapper(url, options);
    }

    agentValueMapper = (options) => {
        const container = "#quickDocketSearchCaseInfoTabContent";
        const url = $(container).data("agent-mapper-url");
        this.searchValueMapper(url, options);
    }

    ownerValueMapper = (options) => {
        const container = "#quickDocketSearchCaseInfoTabContent";
        const url = $(container).data("owner-mapper-url");
        this.searchValueMapper(url, options);
    }

    searchValueMapper = (url, options) => {
        const data = this.getSystems(this.refineSearchContainer);
        data.value = options.value;
        $.ajax({
            url: url,
            data: data,
            success: function (data) {
                options.success(data);
            }
        });
    }

    print = () => {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Shared/QuickDocket/Print`;
        const criteria = JSON.stringify(this.getCriteria(this.refineSearchContainer));
        cpiLoadingSpinner.show();

        fetch(url, {
            method: "POST",
            headers: {
                Accept: "arraybuffer",
                "Content-Type": "application/json",
            },
            body: criteria
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
                a.download = "Quick Docket";
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

    exportToExcel = () => {
        const criteria = JSON.stringify(this.getCriteria(this.refineSearchContainer));
        const form = $("#qdExportToExcel");
        form.find("#exportCriteria").val(criteria);
        form.submit();
    }

    showDueDateDelegateScreen(system, actId, ddId) {
        const qdContainer = $("#quickDocketContainer");
        const url = qdContainer.data("duedate-delegate-url");
        cpiLoadingSpinner.show();

        $.ajax({
            url: url,
            data: { system: system, actId: actId, ddId: ddId },
            success: function (result) {
                cpiLoadingSpinner.hide();
                var popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
            },
            error: function (e) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            }
        });
    }
}





