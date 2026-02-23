export default class RTSUpdatePage  {
    constructor() {
        this.updateType = "Biblio";
    }

    initializeUpdate = () => {
        rtsUpdatePage.loadUpdateScreen("Biblio");

        const container = $("#ptoUpdateHistory");
        if (container.find("#rtsUpdateHistoryInfo").length === 0) {
            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Patent/RTSUpdate/GetUndoMenu`;
            $.get(url).done((result) => {
                container.html(result);
            });
        }
    }

    initializeTabs = () => {
        $("#ptoUpdateMain-tab a").off("click");
        $("#ptoUpdateMain-tab a").on("click", function (e) {
            //sync horizontal tabs
            var href = $(e.currentTarget).attr('href');
            $('.nav-tabs#ptoUpdateMain-tab li a').removeClass('active');
            $('.nav-tabs li a[href="' + href + '"]').addClass('active');
            $('.tab-pane').removeClass('active');
            $('.tab-pane' + href).addClass('active');
        });
    }

    stripeTableRows = (tableId) => {
        let highlightRow = 3;

        $(`#${tableId} tr`).each(function (index) {
            const row = index + 1;
            if (row === highlightRow || row === highlightRow + 1) {
                $(this).find("td").addClass("altItem");

                if (row === highlightRow + 1)
                    highlightRow = highlightRow + 4;
            }
            else {
                $(this).find("td").removeClass("altItem");
            }
        });
    }

    loadUpdateScreen(updateType) {
        let updateScreen;

        const biblioScreen = $("#updateScreenBiblio");

        switch (updateType) {
            case "Biblio":
                updateScreen = biblioScreen;
                biblioScreen.show();
                //actionScreen.hide();
                break;
        }
        const contentTab = updateScreen.find(".accordion-content");
        contentTab.first().addClass("show");
        contentTab.first().show();
        updateScreen.find(".nav-link.active").first().addClass("show");

        if (updateScreen.html().length !== 0)
            return;

        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Patent/RTSUpdate/GetUpdateScreen`;
        $.get(url, { updateType: updateType }).done((result) => {
            updateScreen.html(result);
        });
    }

    initializeUpdateActions = (updateType) => {
        const self = this;
        const updateTypeOption = $(".rts-update-main").find(`.updateTypeOption.${updateType}`);

        //updateTypeOption.on("click", ".update-type", function () {
        //    self.updateType = $(this).data("value");
        //    self.loadUpdateScreen($(this).data("value"));
        //});
        self.loadUpdateScreen(updateTypeOption.find("a.active").data("value"));

        updateTypeOption.on("click", ".rtsUpdate", function () {
            const container = $("#updateScreen");
            const title = container.data("update-confirm-title");
            const updatePrompt = container.data("update-mass-msg");

            cpiConfirm.confirm(title, updatePrompt, () => {
                self.runUpdate();
            });
        });

        updateTypeOption.on("click", ".export-excel-compare", function () {
            const criteria = rtsPTOUpdateBiblioPage.gridMainSearchFilters().mainSearchFilters;
            const dataJSON = JSON.stringify(criteria);

            let sortField = "";
            let sortDirection = "";
            const sort = $("#rtsBiblioUpdate-Grid").data("kendoListView").dataSource.sort();
            if (sort) {
                sortField = sort[0].field;
                sortDirection = sort[0].dir;
            }
            var url = $(this).data("url");

            const html = '<form method="POST" id="excel-server-export-form" action="' + url + '">';
            let form = $(html);
            form.append($('<input type="hidden" name="mainSearchFiltersJSON"/>').val(dataJSON));
            form.append($('<input type="hidden" name="sortField"/>').val(sortField));
            form.append($('<input type="hidden" name="sortDirection"/>').val(sortDirection));
            $("body").append(form);
            form.submit();
            $("#excel-server-export-form").remove();

        });

        
        //updateTypeOption.on("click", ".tlPrint", this.print);
    }


    initializeBiblioUpdate = () => {
        $("#rtsBiblioContainer").append($("#rtsBiblioUpdate-Grid_pager"));
        const biblioUpdateGrid = $("#rtsBiblioUpdate-Grid");

        const baseUrl = $("body").data("base-url");

        $(document).ready(function () {
            biblioUpdateGrid.on("click", "input[type='checkbox']", function () {
                const url = `${baseUrl}/Patent/RTSUpdate/BiblioUpdateSetting`;
                const checkBox = $(this);
                $.post(url, {
                    appId: checkBox.data("id"),
                    fieldName: checkBox.attr("name"),
                    update: this.checked,
                    tStamp: $(checkBox.closest("tr")).data("stamp")
                });
            });

            biblioUpdateGrid.on("click", ".update-indiv", function (e) {
                e.preventDefault();
                const parent = $($(this).parents("tr")[0]);
                const id = parent.data("id");

                const container = $("#updateScreen");
                const title = container.data("update-confirm-title");
                const updatePrompt = container.data("update-indiv-msg");

                cpiConfirm.confirm(title, updatePrompt, () => {
                    const url = `${baseUrl}/Patent/RTSUpdate/UpdateBiblioRecord`;
                    $.post(url, { appId: id })
                        .done(function (result) {
                            const grid = $("#rtsBiblioUpdate-Grid").data("kendoListView");

                            if (parseInt(grid.options.dataSource.pageSize) > 0)
                                grid.dataSource.pageSize(grid.options.dataSource.pageSize);
                            else
                                grid.dataSource.read();

                            pageHelper.showSuccess(result.success);
                            pageHelper.handleEmailWorkflow(result);
                        })
                        .fail(function (e) {
                            pageHelper.showErrors(e);
                        });
                });
            });

            const listView = $("#rtsBiblioUpdate-Grid").data("kendoListView");
            $("#rtsBiblioTable").find(".k-link").sorter(listView, true);
            
    
        });

    }

    runUpdate() {
        const baseUrl = $("body").data("base-url");
        let criteria, url, grid;


        switch (this.updateType) {
            case "Biblio":
                url = "UpdateBiblioRecords";
                criteria = rtsPTOUpdateBiblioPage.gridMainSearchFilters;
                grid = $("#rtsBiblioUpdate-Grid").data("kendoListView");
                break;
        }

        if (url.length > 0) {
            url = `${baseUrl}/Patent/RTSUpdate/${url}`;

            $.post(url, { mainSearchFilters: criteria().mainSearchFilters })
                .done(function (result) {
                    grid.dataSource.read().then(function () {
                        pageHelper.showSuccess(result.success);
                        pageHelper.handleEmailWorkflow(result);
                    });
                })
                .fail(function (e) {
                    pageHelper.showErrors(e);
                });
        }

    }

    showUpdHistoryUndoIndiv = (dataItem) => {
        return dataItem.UndoDate === null;
    }

    updHistoryUndoIndiv(e, grid, updateType) {
        const container = $("#rtsUpdateHistoryInfo");
        const title = container.data("confirm-title");
        const updatePrompt = container.data("undo-indiv-msg");

        cpiConfirm.confirm(title, updatePrompt, () => {
            const row = $(e.currentTarget).closest("tr");
            const dataItem = grid.dataItem(row);

            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Patent/RTSUpdate/Undo`;

            $.post(url, { logId: dataItem.LogId, updateType: updateType })
                .done((result) => {
                    switch (updateType) {
                        case "Biblio":
                            this.biblioUpdHistoryGridRead();
                            break;
                    }
                    pageHelper.showSuccess(result.success);
                })
                .fail(function (error) { pageHelper.showErrors(error); });
        });
    }

    /* biblio */
    biblioUpdHistory_RevertTypeChange = () => {
        this.biblioUpdHistoryGridRead();
    }

    getBiblioUpdHistory_ChangeDate() {
        const jobId = $("#JobId_BiblioUpdHistory").val();
        return (jobId === null || jobId === "") ? 0 : jobId;
    }
    getBiblioUpdHistory_ChangeDateChange = () => {
        this.biblioUpdHistoryGridRead();
    }

    getBiblioUpdHistory_RevertType() {
        let revertType = $("#RevertType_BiblioUpdHistory").val();
        if (revertType == '') revertType = 2;
        return revertType;
    }

    biblioUpdHistoryGridRead() {
        const grid = $("#biblioUpdHistoryGrid").data("kendoGrid");
        grid.dataSource.read();

        const undoButton = $("#biblioUpdHistory_undo");
        if (undoButton) {

            if (this.getBiblioUpdHistory_RevertType() === "0" &&
                this.getBiblioUpdHistory_ChangeDate() > 0) {
                undoButton.removeClass("d-none");
            } else {
                undoButton.addClass("d-none");
            }
        }
    }

    biblioUpdHistorySetBtns() {
        $(document).ready(() => {
            this.biblioUpdHistoryGridRead();

            $("#biblioUpdHistory_undo").click((e) => {
                const container = $("#rtsUpdateHistoryInfo");
                const title = container.data("confirm-title");
                const updatePrompt = container.data("undo-batch-msg");

                cpiConfirm.confirm(title, updatePrompt, () => {
                    const data = {
                        tmkId: $("#rtsUpdateHistoryInfo").find("#AppId").val(),
                        revertType: this.getBiblioUpdHistory_RevertType,
                        jobId: this.getBiblioUpdHistory_ChangeDate
                    };
                    $.post($(e.currentTarget).data("url"), data)
                        .done((result) => {
                            this.biblioUpdHistoryGridRead();
                            pageHelper.showSuccess(result.success);
                        })
                        .fail(function (error) { pageHelper.showErrors(error); });
                });
            });
        });
    }

    biblioUpdHistoryFilter = () => {
        var criteria = {
            revertType: this.getBiblioUpdHistory_RevertType(),
            jobId: this.getBiblioUpdHistory_ChangeDate()
        }
        return criteria;
    }
}





