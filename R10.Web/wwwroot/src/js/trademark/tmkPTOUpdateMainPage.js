export default class PTOUpdateMainPage {

    constructor() {
        this.updateType = "Biblio";
    }

    initializeUpdate = () => {
        $(document).ready(() => {
            tmkPTOUpdateMainPage.loadUpdateScreen("Biblio");

            $("#ptoUpdateHistory-tab").on("click", function () {
                const container = $("#ptoUpdateHistory");
                if (container.find("#tlUpdateHistoryInfo").length === 0) {
                    const baseUrl = $("body").data("base-url");
                    const url = `${baseUrl}/Trademark/TLUpdate/GetUndoMenu`;
                    $.get(url).done((result) => {
                        container.html(result);
                    });
                }
            });
        });

    }

    initializeUpdateActions = (updateType) => {
        const self = this;
        const updateTypeOption = $(".tl-update-main").find(`.updateTypeOption.${updateType}`);

        updateTypeOption.on("click", ".update-type", function () {
            self.updateType = $(this).data("value");
            self.loadUpdateScreen($(this).data("value"));
        });
        self.loadUpdateScreen(updateTypeOption.find("a.active").data("value"));

        updateTypeOption.on("click", ".tlUpdate", function () {
            const container = $("#updateScreen");
            const title = container.data("update-confirm-title");
            const updatePrompt = container.data("update-mass-msg");

            cpiConfirm.confirm(title, updatePrompt, () => {
                self.runUpdate();
            });
        });

        updateTypeOption.on("click", ".tlPrint", this.print);
    }

    loadUpdateScreen(updateType) {
        let updateScreen;

        const biblioScreen = $("#updateScreenBiblio");
        const tmkNameScreen = $("#updateScreenTmkName");
        const actionScreen = $("#updateScreenAction");

        switch (updateType) {
            case "Biblio":
                updateScreen = biblioScreen;
                biblioScreen.show();
                tmkNameScreen.hide();
                actionScreen.hide();
                break;

            case "TmkName":
                updateScreen = tmkNameScreen;
                tmkNameScreen.show();
                biblioScreen.hide();
                actionScreen.hide();
                break;

            case "Action":
                updateScreen = actionScreen;
                actionScreen.show();
                tmkNameScreen.hide();
                biblioScreen.hide();
                break;
        }
        const contentTab = updateScreen.find(".accordion-content");
        contentTab.first().addClass("show");
        contentTab.first().show();
        updateScreen.find(".nav-link.active").first().addClass("show");

        if (updateScreen.html().length !== 0)
            return;

        cpiLoadingSpinner.show();
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Trademark/TLUpdate/GetUpdateScreen`;
        $.get(url, { updateType: updateType }).done((result) => {
            cpiLoadingSpinner.hide();
            updateScreen.html(result);
        });
    }

    runUpdate() {
        const baseUrl = $("body").data("base-url");
        let criteria, url, grid;


        switch (this.updateType) {
            case "Biblio":
                url = "UpdateBiblioRecords";
                criteria = tmkPTOUpdateBiblioPage.gridMainSearchFilters;
                grid = $("#tlBiblioUpdate-Grid").data("kendoListView");
                break;

            case "TmkName":
                url = "UpdateTrademarkNameRecords";
                criteria = tmkPTOUpdateTrademarkNamePage.gridMainSearchFilters;
                grid = $("#tlTmkNameUpdate-Grid").data("kendoGrid");
                break;

            case "Action":
                url = "UpdateActionRecords";
                criteria = tmkPTOUpdateActionPage.gridMainSearchFilters;
                grid = $("#tlActionUpdate-Grid").data("kendoGrid");
                break;
        }

        if (url.length > 0) {
            url = `${baseUrl}/Trademark/TLUpdate/${url}`;

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


    biblioClientValueMapper = (options) => {
        const url = "BiblioClientValueMapper";
        this.searchValueMapper(url, options);
    }

    biblioCaseNumberValueMapper = (options) => {
        const url = "BiblioCaseNumberValueMapper";
        this.searchValueMapper(url, options);
    }

    tmkNameClientValueMapper = (options) => {
        const url = "TrademarkNameClientValueMapper";
        this.searchValueMapper(url, options);
    }

    tmkNameCaseNumberValueMapper = (options) => {
        const url = "TrademarkNameCaseNumberValueMapper";
        this.searchValueMapper(url, options);
    }

    actionClientValueMapper = (options) => {
        const url = "ActionClientValueMapper";
        this.searchValueMapper(url, options);
    }

    actionCaseNumberValueMapper = (options) => {
        const url = "ActionCaseNumberValueMapper";
        this.searchValueMapper(url, options);
    }

    searchValueMapper = (url, options) => {
        const baseUrl = $("body").data("base-url");
        url = `${baseUrl}/Trademark/TLUpdateLookup/${url}`;
        $.ajax({
            url: url,
            data: options.value,
            success: function (data) {
                options.success(data);
            }
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

            $.each($(this).find(".tmkNameSearchResult-collapsible"), function () {
                $(this).textOverflow();
            });
        });
    }

    initializeBiblioUpdate = () => {
        const container = $("#tlBiblioContainer");
        container.append($("#tlBiblioUpdate-Grid_pager"));

        const biblioUpdateGrid = $("#tlBiblioUpdate");

        //biblioUpdateGrid.find(".page-sidebar").addClass("mt-2");
        const baseUrl = $("body").data("base-url");

        $(document).ready(function () {
            biblioUpdateGrid.on("click", "input[type='checkbox']", function () {
                const url = `${baseUrl}/Trademark/TLUpdate/BiblioUpdateSetting`;
                const checkBox = $(this);
                $.post(url, {
                    tlTmkId: checkBox.data("id"),
                    fieldName: checkBox.attr("name"),
                    update: this.checked,
                    tStamp: $(checkBox.closest("tr")).data("stamp")
                });
            });

            biblioUpdateGrid.on("click", ".goods-compare", function (e) {
                e.preventDefault();
                const parent = $($(this).parents("tr")[0]);
                const id = parent.data("id");

                const url = `${baseUrl}/Trademark/TLUpdate/GoodsCompare`
                $.get(url, { tlTmkId: id })
                    .done(function (html) {
                        const popupContainer = $(".cpiContainerPopup").last();
                        popupContainer.html(html);
                        const dialog = $("#tlCompareGoodsDialog");
                        dialog.modal("show");
                    })
                    .fail(function (e) {
                        pageHelper.showErrors(e.responseText);
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

                    const url = `${baseUrl}/Trademark/TLUpdate/UpdateBiblioRecord`;
                    $.post(url, { tlTmkId: id })
                        .done(function (result) {
                            const grid = $("#tlBiblioUpdate-Grid").data("kendoListView");

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

            const listView = $("#tlBiblioUpdate-Grid").data("kendoListView");
            $("#tlBiblioTable").find(".k-link").sorter(listView, true);
        });

    }

    initializeTrademarkNameUpdate = (grid) => {
        const trademarkNameGrid = $(`#${grid}`);
        const baseUrl = $("body").data("base-url");

        trademarkNameGrid.on("click", "input[type='checkbox']", function () {
            const url = `${baseUrl}/Trademark/TLUpdate/TrademarkNameUpdateSetting`;
            const checkBox = $(this);

            $.post(url, {
                tlTmkId: checkBox.data("id"),
                fieldName: checkBox.attr("name"),
                update: this.checked,
                tStamp: $(checkBox.closest("tr")).find(".stamp").html()
            });
        });
    }

    initializeActionUpdate = (grid) => {
        const actionGrid = $(`#${grid}`);
        const baseUrl = $("body").data("base-url");

        actionGrid.on("click", "input[type='checkbox']", function (e) {
            const url = `${baseUrl}/Trademark/TLUpdate/ActionUpdateSetting`;
            const row = $(e.target).closest("tr");
            const dataItem = actionGrid.data("kendoGrid").dataItem(row);

            $.post(url, {
                tlTmkId: dataItem.TLTmkId,
                actionType: dataItem.ActionType,
                actionDue: dataItem.ActionDue,
                baseDate: pageHelper.cpiDateFormatToSave(dataItem.BaseDate),
                exclude: this.checked
            });
        });
    }

    print = () => {
        var printForm = "#tlBiblioUpdate-RefineSearch";
        var printFormId = 1;
        var printFormName = "Bibliographic Update";
        switch (this.updateType) {
            case "Biblio":
                printForm = "#tlBiblioUpdate-RefineSearch";
                printFormId = 1;
                printFormName = "Bibliographic Update";
                break;

            case "TmkName":
                printForm = "#tlTmkNameUpdate-RefineSearch";
                printFormId = 2;
                printFormName = "Trademark Name Update";
                break;

            case "Action":
                printForm = "#tlActionUpdate-RefineSearch";
                printFormId = 3;
                printFormName = "Actions Update";
                break;
        }
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Trademark/TLUpdate/Print?update=` + printFormId;
        const criteria = JSON.stringify(pageHelper.formDataToJson($(printForm)).payLoad);
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
                a.download = printFormName;
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

    updateTrademarkNameIndiv = (e, grid) => {
        const row = $(e.currentTarget).closest("tr");
        const dataItem = grid.dataItem(row);

        const container = $("#updateScreen");
        const title = container.data("update-confirm-title");
        const updatePrompt = container.data("update-indiv-msg");

        cpiConfirm.confirm(title, updatePrompt, () => {
            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Trademark/TLUpdate/UpdateTrademarkNameRecord`;
            $.post(url, { tlTmkId: dataItem.TLTmkId })
                .done(function (result) {
                    const grid = $("#tlTmkNameUpdate-Grid").data("kendoGrid");

                    if (parseInt(grid.options.dataSource.pageSize) > 0)
                        grid.dataSource.pageSize(grid.options.dataSource.pageSize);
                    else
                        grid.dataSource.read();

                    pageHelper.showSuccess(result.success);
                })
                .fail(function (e) {
                    pageHelper.showErrors(e);
                });
        });
    }

    /* Update History */
    updateHistoryHandleMenu() {
        const form = $("#tlUpdateHistoryInfo");
        form.on("submit", function (e) {
            e.preventDefault();
            e.stopPropagation();

            let container;
            switch (form.find("#menuChoice").val()) {
                case "Action":
                    container = $("#tlActionUpdateHistoryContainer");
                    break;

                case "TmkName":
                    container = $("#tlTmkNameUpdateHistoryContainer");
                    break;

                case "Goods":
                    container = $("#tlGoodsUpdateHistoryContainer");
                    break;
                default:
                    container = $("#tlBiblioUpdateHistoryContainer");
            }

            if (container.find("input[name='RevertType']").length === 0) {
                const params = form.serialize();
                $.post(form.attr("action"), params)
                    .done(function (result) {
                        container.html(result);
                    })
                    .fail(function (e) {
                        pageHelper.showErrors(e.responseText);
                    });
            }
            $("#tlUpdateHistoryContainer").children().hide();
            container.show();

        });
        form.find("a").click(function () {
            const choices = form.find("a");
            $.each(choices, function () {
                $(this).removeClass("active");
            });

            const selected = $(this).data("value");
            $(this).addClass("active");

            $(form.find("#menuChoice")[0]).val(selected);
            form.submit();
        });
        form.submit();
    }

    showUpdHistoryUndoIndiv = (dataItem) => {
        return dataItem.UndoDate === null;
    }

    updHistoryUndoIndiv(e, grid, updateType) {
        const container = $("#tlUpdateHistoryInfo");
        const title = container.data("confirm-title");
        const updatePrompt = container.data("undo-indiv-msg");

        cpiConfirm.confirm(title, updatePrompt, () => {
            const row = $(e.currentTarget).closest("tr");
            const dataItem = grid.dataItem(row);

            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Trademark/TLUpdate/Undo`;

            $.post(url, { logId: dataItem.LogId, updateType: updateType })
                .done((result) => {
                    switch (updateType) {
                        case "Biblio":
                            this.biblioUpdHistoryGridRead();
                            break;

                        case "TmkName":
                            this.tmkNameUpdHistoryGridRead();
                            break;

                        case "Action":
                            this.actionUpdHistoryGridRead();
                            break;

                        case "Goods":
                            this.goodsUpdHistoryGridRead();
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
                const container = $("#tlUpdateHistoryInfo");
                const title = container.data("confirm-title");
                const updatePrompt = container.data("undo-batch-msg");

                cpiConfirm.confirm(title, updatePrompt, () => {
                    const data = {
                        tmkId: $("#tlUpdateHistoryInfo").find("#TmkId").val(),
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

    /* actions */
    actionUpdHistory_RevertTypeChange = () => {
        this.actionUpdHistoryGridRead();
    }

    getActionUpdHistory_ChangeDate() {
        const jobId = $("#JobId_ActionUpdHistory").val();
        return (jobId === null || jobId === "") ? 0 : jobId;
    }
    getActionUpdHistory_ChangeDateChange = () => {
        this.actionUpdHistoryGridRead();
    }

    getActionUpdHistory_RevertType() {
        let revertType = $("#RevertType_ActionUpdHistory").val();
        if (revertType == '') revertType = 2;
        return revertType;
    }

    actionUpdHistoryGridRead() {
        const grid = $("#actionUpdHistoryGrid").data("kendoGrid");
        grid.dataSource.read();

        const undoButton = $("#actionUpdHistory_undo");
        if (undoButton) {

            if (this.getActionUpdHistory_RevertType() === "0" &&
                this.getActionUpdHistory_ChangeDate() > 0) {
                undoButton.removeClass("d-none");
            } else {
                undoButton.addClass("d-none");
            }
        }
    }

    actionUpdHistorySetBtns() {
        $(document).ready(() => {
            this.actionUpdHistoryGridRead();

            $("#actionUpdHistory_undo").click((e) => {
                const container = $("#tlUpdateHistoryInfo");
                const title = container.data("confirm-title");
                const updatePrompt = container.data("undo-batch-msg");

                cpiConfirm.confirm(title, updatePrompt, () => {
                    const data = {
                        tmkId: $("#tlUpdateHistoryInfo").find("#TmkId").val(),
                        revertType: this.getActionUpdHistory_RevertType,
                        jobId: this.getActionUpdHistory_ChangeDate
                    };
                    $.post($(e.currentTarget).data("url"), data)
                        .done((result) => {
                            this.actionUpdHistoryGridRead();
                            pageHelper.showSuccess(result.success);
                        })
                        .fail(function (error) { pageHelper.showErrors(error); });
                });
            });
        });
    }

    actionUpdHistoryFilter = () => {
        return {
            revertType: this.getActionUpdHistory_RevertType(),
            jobId: this.getActionUpdHistory_ChangeDate()
        };
    }

    /* tmk name */
    tmkNameUpdHistory_RevertTypeChange = () => {
        this.tmkNameUpdHistoryGridRead();
    }

    getTmkNameUpdHistory_ChangeDate() {
        const jobId = $("#JobId_TmkNameUpdHistory").val();
        return (jobId === null || jobId === "") ? 0 : jobId;
    }
    getTmkNameUpdHistory_ChangeDateChange = () => {
        this.tmkNameUpdHistoryGridRead();
    }

    getTmkNameUpdHistory_RevertType() {
        let revertType = $("#RevertType_TmkNameUpdHistory").val();
        if (revertType == '') revertType = 2;
        return revertType;
    }

    tmkNameUpdHistoryGridRead() {
        const grid = $("#tmkNameUpdHistoryGrid").data("kendoGrid");
        grid.dataSource.read();

        const undoButton = $("#tmkNameUpdHistory_undo");
        if (undoButton) {

            if (this.getTmkNameUpdHistory_RevertType() === "0" &&
                this.getTmkNameUpdHistory_ChangeDate() > 0) {
                undoButton.removeClass("d-none");
            } else {
                undoButton.addClass("d-none");
            }
        }
    }

    tmkNameUpdHistorySetBtns() {
        $(document).ready(() => {
            this.tmkNameUpdHistoryGridRead();

            $("#tmkNameUpdHistory_undo").click((e) => {
                const container = $("#tlUpdateHistoryInfo");
                const title = container.data("confirm-title");
                const updatePrompt = container.data("undo-batch-msg");

                cpiConfirm.confirm(title, updatePrompt, () => {
                    const data = {
                        tmkId: $("#tlUpdateHistoryInfo").find("#TmkId").val(),
                        revertType: this.getTmkNameUpdHistory_RevertType,
                        jobId: this.getTmkNameUpdHistory_ChangeDate
                    };
                    $.post($(e.currentTarget).data("url"), data)
                        .done((result) => {
                            this.tmkNameUpdHistoryGridRead();
                            pageHelper.showSuccess(result.success);
                        })
                        .fail(function (error) { pageHelper.showErrors(error); });
                });

            });
        });

    }

    tmkNameUpdHistoryFilter = () => {
        return {
            revertType: this.getTmkNameUpdHistory_RevertType(),
            jobId: this.getTmkNameUpdHistory_ChangeDate()
        };
    }

    /* goods */
    goodsUpdHistory_RevertTypeChange = () => {
        this.goodsUpdHistoryGridRead();
    }

    getGoodsUpdHistory_ChangeDate() {
        const jobId = $("#JobId_GoodsUpdHistory").val();
        return (jobId === null || jobId === "") ? 0 : jobId;
    }
    getGoodsUpdHistory_ChangeDateChange = () => {
        this.goodsUpdHistoryGridRead();
    }

    getGoodsUpdHistory_RevertType() {
        let revertType = $("#RevertType_GoodsUpdHistory").val();
        if (revertType == '') revertType = 2;
        return revertType;
    }

    goodsUpdHistoryGridRead() {
        const grid = $("#goodsUpdHistoryGrid").data("kendoGrid");
        grid.dataSource.read();

        const undoButton = $("#goodsUpdHistory_undo");
        if (undoButton) {

            if (this.getGoodsUpdHistory_RevertType() === "0" &&
                this.getGoodsUpdHistory_ChangeDate() > 0) {
                undoButton.removeClass("d-none");
            } else {
                undoButton.addClass("d-none");
            }
        }
    }

    goodsUpdHistorySetBtns() {
        $(document).ready(() => {
            this.goodsUpdHistoryGridRead();

            $("#goodsUpdHistory_undo").click((e) => {
                const container = $("#tlUpdateHistoryInfo");
                const title = container.data("confirm-title");
                const updatePrompt = container.data("undo-batch-msg");

                cpiConfirm.confirm(title, updatePrompt, () => {
                    const data = {
                        tmkId: $("#tlUpdateHistoryInfo").find("#TmkId").val(),
                        revertType: this.getGoodsUpdHistory_RevertType,
                        jobId: this.getGoodsUpdHistory_ChangeDate
                    };
                    $.post($(e.currentTarget).data("url"), data)
                        .done((result) => {
                            this.goodsUpdHistoryGridRead();
                            pageHelper.showSuccess(result.success);
                        })
                        .fail(function (error) { pageHelper.showErrors(error); });
                });
            });
        });
    }

    goodsUpdHistoryFilter = () => {
        return {
            revertType: this.getGoodsUpdHistory_RevertType(),
            jobId: this.getGoodsUpdHistory_ChangeDate()
        };
    }
}




