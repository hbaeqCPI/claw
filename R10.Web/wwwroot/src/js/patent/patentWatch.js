import CompactSearchPage from "../compactSearchPage";

export default class PatentWatch extends CompactSearchPage {

    constructor() {
        super();
        this.tabsLoaded = [];
    }

    onSearchInitialized = () => {
        const self = this;
        $("#patentWatchContainer").append($(`${this.searchResultGrid}_pager`));
        $("#patentWatchUpdatesContainer").append($("#patentWatchUpdates-Grid_pager"));

        $(document).ready(() => {
            const grid = $(this.searchResultGrid);
            const listView = grid.data("kendoListView");

            $("#patentWatchTable").find(".k-link").sorter(listView, true);

            grid.on("click", ".pw-ls-toggle", (e) => {
                this.getLegalStatusEvents(e);
            });
            $("#patentWatchUpdates-Grid").on("click", ".pw-ls-toggle", (e) => {
                this.getLegalStatusEvents(e);
            });
            $("#patentWatchUpdates-Grid").on("click", ".pw-viewed", (e) => {
                this.markUpdatesAsViewed(e);
            });

            $("#pw-Watch").on("change", (e) => {
                if ($(e.target).is(":checked")) {
                    $(".users-to-notify").removeClass("d-none");
                    $(".keywords").removeClass("d-none");
                    $(".remarks").removeClass("d-none");
                }
                else {
                    $(".users-to-notify").addClass("d-none");
                    $(".keywords").addClass("d-none");
                    $(".remarks").addClass("d-none");
                }
            });

            //watchlist search
            const wlContainer = $("#watchList");
            wlContainer.find(".wl-search .k-combobox > input").each(function () {
                const comboBox = $(this).data("kendoComboBox");
                if (comboBox) {
                    comboBox.bind("change", function () {
                        self.refreshWatchList();
                    });
                }
            });

            wlContainer.find(".wl-search .k-datepicker input").each(function () {
                const datePicker = $(this).data("kendoDatePicker");
                if (datePicker) {
                    datePicker.bind("change", function () {
                        self.refreshWatchList();
                    });
                }
            });

            wlContainer.find(".wl-search .search-text").on("change", function () {
                self.refreshWatchList();
            });

            wlContainer.find(".wl-search .search-text").keypress(function (event) {
                if (event.which == 13) {
                    self.refreshWatchList();
                }
            });

            //watch updates search
            const wuContainer = $("#wu-search-form");
            wuContainer.find(".k-combobox > input").each(function () {
                const comboBox = $(this).data("kendoComboBox");
                if (comboBox) {
                    comboBox.bind("change", function () {
                        self.refreshWatchUpdates();
                    });
                }
            });
            wuContainer.find(".search-text").on("change", function () {
                self.refreshWatchUpdates();
            });

            wuContainer.find(".search-text").keypress(function (event) {
                if (event.which == 13) {
                    self.refreshWatchUpdates();
                }
            });


            this.exportToExcelHandler();
            this.tabChangeSetListener();
        });
        this.editableGrids = [{ name: "usersToNotifyGrid"}];
    }

    getWatchListSearchCriteria = () => {
        const form = $("#wl-search-form");
        const criteria = pageHelper.formDataToCriteriaList(form);
        return { criteria: criteria.payLoad };
    }

    getWatchUpdatesSearchCriteria = () => {
        const form = $("#wu-search-form");
        const criteria = pageHelper.formDataToCriteriaList(form);
        return { criteria: criteria.payLoad };
    }

    refreshWatchList = () => {
        const grid = $("#patentWatchList-Grid").data("kendoGrid");
        grid.dataSource.read();
    }

    refreshWatchUpdates = () => {
        const grid = $("#patentWatchUpdates-Grid").data("kendoListView");
        grid.dataSource.read();
    }

    tabChangeSetListener = () => {
        $("#patent-watch-results a").on("click", (e) => {
            e.preventDefault();
            const tab = $(e.target).attr("aria-controls");
            if (this.tabsLoaded.indexOf(tab) === -1) {
                this.tabsLoaded.push(tab);

                if (tab === "watchList") {
                    const grid = $("#patentWatchList-Grid").data("kendoGrid");
                    grid.dataSource.read();
                }
                else if (tab === "watchResults") {
                    this.configureWatchUpdatesColumnSort();
                    this.loadWatchUpdates();
                }
            }
        });
    }

    configureWatchUpdatesColumnSort() {
        const container = $("#patentWatchUpdatesContainer #pwHeaderContainer");

        const self = this;
        container.find("a").each(function () {
            const el = $(this);

            if (el) {
                const dataField = el.data("field");

                if (dataField) {
                    $(el).on('click', function () {
                        const els = container.find(`[data-field='${dataField}']`);
                        self.addSortIcon(els);
                    });
                }
            }
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
        const dataSource = $('#patentWatchUpdates-Grid').data('kendoListView').dataSource;
        const sortDescriptor = [{ field: sortColumn, dir: sortOrder.toLowerCase() }];
        dataSource.sort(sortDescriptor);
    }

    hideSortIcons(columnClicked) {
        const headerContainer = $("#patentWatchUpdatesContainer #pwHeaderContainer");
        
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

    loadWatchUpdates = () => {
        $(document).ready(() => {
            const grid = $("#patentWatchUpdates-Grid").data("kendoListView");
            grid.dataSource.read();
        });
    }

    getLegalStatusEvents = (e) => {
        const element = e.target;
        const lsContainer = $(element.parentNode).siblings(".pw-ls-container");

        if (element.className.includes("plus")) {
            element.className = element.className.replace("plus", "minus");

            if (!lsContainer.hasClass("mounted")) {
                const header = $(element).data("ifd-header");
                this.getLSDEvents(header, lsContainer);
            }
            else
                lsContainer.show();
        }
        else {
            element.className = element.className.replace("minus", "plus");
            lsContainer.hide();
        }
    }


    exportToExcelHandler() {
        $("#pwExportExcel").on("click", ()=> {

            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Patent/PatentWatch/ExportToExcel`;

            const source = pageHelper.formDataToJson($(`${this.searchForm}`));
            const form = $("#pwExportExcelForm");
            form.attr("action", url);
            form.find("input[name='NumberType']").val(source.payLoad.NumberType);
            form.find("input[name='Numbers']").val(source.payLoad.Numbers);
            form.submit();
        });

        const pwList = $("#patentWatchList-Grid");
        pwList.find('.k-grid-excel-server').on('click', (e) => {
            e.preventDefault();
            const url = pwList.data("export-excel-list-url");

            if (url) {
                const data = pageHelper.gridMainSearchFilters($("#wl-search-form"));
                const dataJSON = JSON.stringify(data.mainSearchFilters);
                const html = '<form method="POST" id="excel-server-export-form" action="' + url + '">';
                let form = $(html);
                form.append($('<input type="hidden" name="mainSearchFiltersJSON"/>').val(dataJSON));
                $("body").append(form);
                form.submit();
                $("#excel-server-export-form").remove();
            }
        });

        $("#ExportUpdatesToExcel").on('click', (e) => {
            e.preventDefault();

            const url = e.currentTarget.href;
            if (url) {
                const data = pageHelper.gridMainSearchFilters($("#wu-search-form"));
                const dataJSON = JSON.stringify(data.mainSearchFilters);
                const html = '<form method="POST" id="excel-server-export-form" action="' + url + '">';
                let form = $(html);
                form.append($('<input type="hidden" name="mainSearchFiltersJSON"/>').val(dataJSON));
                $("body").append(form);
                form.submit();
                $("#excel-server-export-form").remove();
            }
        });

        
    }


    getLSDEvents(header, lsContainer) {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Patent/PatentWatch/GetLSDEvents`;

        $.get(url, { header: header })
            .done((result) => {
                const recId = header.replaceAll(" ", "");
                result = result.replaceAll("patentWatchLSDEvents-Grid", `patentWatchLSDEvents-Grid_${recId}`);
                lsContainer.html(result);
                lsContainer.addClass("mounted");
            })
            .fail((e => {
                pageHelper.showErrors(e);
            }));
    }

    dataBound=()=> {
        let highlightRow = 4;
        $(this.searchResultGrid).find("tr").each(function (index) {
            const row = index + 1;
          
            if (row >= highlightRow && row <= highlightRow + 2) {
                $(this).addClass("altItem");

                if (row === highlightRow + 2)
                    highlightRow = highlightRow + 6;
            }
        });
        const watchList = $("#patentWatchList-Grid").data("kendoGrid");
        watchList.dataSource.read();

        //for the notfound list
        const grid = $(this.searchResultGrid).data("kendoListView");
        const foundData = grid.dataSource.data();
        const searchForm = $(`${this.searchForm}`);
        const searchNos = searchForm.find("textarea[name='Numbers']").val().split(";");
        const notFound = searchNos.filter(no => !foundData.find(f => f.SearchNo === no));
        $("#patentWatchSearch").find("#recordsNotFound textarea").text(notFound.join(";"));
       
    }
    deleteGridRow(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        pageHelper.kendoGridDeleteRecord(e, dataItem);
        
    }

    usersToNotify(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Patent/PatentWatch/GetUsersToNotify`;

        $.get(url, { watchId: dataItem.WatchId, createdBy: dataItem.CreatedBy}).done((result) => {
            $(".cpiContainerPopup").empty();
            const popupContainer = $(".cpiContainerPopup").last();
            popupContainer.html(result);
            const dialog = $("#saveUsersToNotifyDialog");
            dialog.modal("show");

            this.editableGrids.forEach((el) => {
                const grid = $(`#${el.name}`);
                pageHelper.kendoGridDirtyTracking(grid, el);
            });

        }).fail(function (error) {
            pageHelper.showErrors(error.responseText);
        });
    }

    updateRemarks(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Patent/PatentWatch/GetUpdateRemarks`;

        $.get(url, { watchId: dataItem.WatchId, remarks: dataItem.Remarks, createdBy: dataItem.CreatedBy}).done((result) => {
            $(".cpiContainerPopup").empty();
            const popupContainer = $(".cpiContainerPopup").last();
            popupContainer.html(result);
            const dialog = $("#updateRemarksDialog");
            dialog.modal("show");

            let entryForm = dialog.find("form")[0];
            entryForm = $(entryForm);
            entryForm.cpiPopupEntryForm(
                {
                    dialogContainer: dialog,
                    afterSubmit: (result) => {
                        dialog.modal("hide");
                        dataItem.Remarks = result.remarks;
                    }
                }
            );

        }).fail(function (error) {
            pageHelper.showErrors(error.responseText);
        });
    }

    updateKeywords(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Patent/PatentWatch/GetUpdateKeywords`;

        $.get(url, { watchId: dataItem.WatchId, keywords: dataItem.Keywords, createdBy: dataItem.CreatedBy }).done((result) => {
            $(".cpiContainerPopup").empty();
            const popupContainer = $(".cpiContainerPopup").last();
            popupContainer.html(result);
            const dialog = $("#updateKeywordsDialog");
            dialog.modal("show");

            let entryForm = dialog.find("form")[0];
            entryForm = $(entryForm);
            entryForm.cpiPopupEntryForm(
                {
                    dialogContainer: dialog,
                    afterSubmit: (result) => {
                        dialog.modal("hide");
                        dataItem.Keywords = result.keywords;
                    }
                }
            );

        }).fail(function (error) {
            pageHelper.showErrors(error.responseText);
        });
    }

    markUpdatesAsViewed(e) {
        const container = $("#patentWatchUpdatesContainer");
        const confirmTitle = container.data("viewed-title");
        const confirmMsg = container.data("viewed-message");
        const element = e.target;
        self.cpiConfirm.confirm(confirmTitle, confirmMsg, function () {
            const watchId = $(element).data("watch-id");            

            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Patent/PatentWatch/MarkUpdateAsViewed`;

            $.get(url, { watchId: watchId})
                .done(() => {
                    const grid = $("#patentWatchUpdates-Grid").data("kendoListView");
                    grid.dataSource.read();
                })
                .fail((e => {
                    pageHelper.showErrors(e);
                }));
                    
        });
    }    

}





