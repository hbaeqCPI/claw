export default class TLMappingPage {
    constructor() {
        this.mappingType = "IFWDocuments";
    }

    initializeUpdate = () => {
        $(document).ready(() => {
            tlMappingPage.loadUpdateScreen("IFWDocuments");
            $("#trnxHistory-tab").on("click", function () {
                tlMappingPage.loadUpdateScreen("TransactionHistory");
            });
            $("#ifwDocuments-tab").on("click", function () {
                const tab = $("#ifwDocumentsSearchMainTab");
                tab.addClass("show");
                tab.siblings(".accordion-content").addClass("show").css("display", "block");
            });
        });
    }


    loadUpdateScreen(mappingType) {
        let updateScreen;

        switch (mappingType) {
            case "IFWDocuments":
                updateScreen = $("#mappingScreenIFWDoc");
                updateScreen.show();
                break;

            case "TransactionHistory":
                updateScreen = $("#mappingScreenTrnxHistory");
                updateScreen.show();
                break;
            
        }
        const contentTab = updateScreen.find(".accordion-content");
        contentTab.first().addClass("show");
        contentTab.first().show();
        updateScreen.find(".nav-link.active").first().addClass("show");

        if (updateScreen.html().length !== 0)
            return;

        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Trademark/TLMapping/GetMappingScreen`;
        $.get(url, { mappingType: mappingType }).done((result) => {
            updateScreen.html(result);
        });
    }

    onGridDirty = () => {
        cpiBreadCrumbs.markLastNode({ dirty: true });
    }

    selectionValid = () => {
        return !cpiBreadCrumbs.hasDirtyNode();
    }

    //ifw documents region
    initializeIFWDocumentsMappingGrids = () => {
        const topGrid = $('#tlIFWDocumentsMapping-Grid');
        const el = {
            name: 'tlIFWDocumentsMapping-Grid', isDirty: false
        };
        this.ifwDocumentsEditableGrids = [{el}];
        //pageHelper.kendoGridDirtyTracking(topGrid, el, this.trnxHistoryAfterGridSaveOrCancel, this.trnxHistoryAfterGridSaveOrCancel, this.onGridDirty);
        pageHelper.kendoGridDirtyTracking(topGrid, el, this.ifwDocumentsAfterGridSaveOrCancel, this.ifwDocumentsAfterGridSaveOrCancel, this.onGridDirty);
        this.ifwDocumentsEditableGrids.push({ name: 'ifwDocumentsMappedActionsGrid' });

        topGrid.on("click", ".client-entry", (e) => {
            e.stopPropagation();

            const element = $(e.target);
            const grid = topGrid.data("kendoGrid");
            const currentRow = grid.dataItem(element.parents("tr").select());

            const url = $("#detailFormIFWDoc").data("client-entry-screen-url");
            $.get(url, { mapId: currentRow.MapId }).done(function (result) {
                const popupContainer = $(".site-content .popup");
                popupContainer.empty();
                popupContainer.html(result);
                var dialog = $("#ifwMappingClientEntryDialog");
                dialog.modal("show");

                const clientEntryGrid = $('#tlMapActionDocumentClientGrid');
                const el = {
                    name: 'tlMapActionDocumentClientGrid', isDirty: false
                };
                pageHelper.kendoGridDirtyTracking(clientEntryGrid, el);

            }).fail(function (error) {
                pageHelper.showErrors(error.responseText);
            });
        });
    }

    deleteGridRow(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        pageHelper.deleteGridRow(e, dataItem, function () { });
    }

    onIFWDocumentsSourceRowDataBound = (e) => {
        const container = $(".ifw-documents-pto-mapping");
        container.addClass("d-none");

        const url = container.data("entry-screen-url");
        $.get(url).done((result) => {
            container.find("#pto-mapping-grids").html(result);
            this.ifwDocumentsEditableGrids.forEach((el) => {
                const grid = $(`#${el.name}`);
                pageHelper.kendoGridDirtyTracking(grid, el, this.ifwDocumentsAfterGridSaveOrCancel, this.ifwDocumentsAfterGridSaveOrCancel, this.onGridDirty);
            });
        });

        const genActionIndex = e.sender.wrapper.find(".k-grid-header [data-field=" + "IsGenActionDE" + "]").index();
        const dataItems = e.sender.dataSource.view();
        for (var j = 0; j < dataItems.length; j++) {
            const row = e.sender.tbody.find("[data-uid='" + dataItems[j].uid + "']");
            const genActionCell = row.children().eq(genActionIndex);
            const useAI = dataItems[j].get("UseAI");

            if (!useAI) {
                genActionCell.removeClass("editable-cell");
                row.find(".isGenActionDE-chkbx").remove();
            }
        }
    }

    ifwDocumentsAfterGridSaveOrCancel = () => {
        const pos = this.ifwDocumentsEditableGrids.findIndex(function (element) {
            return element.isDirty;
        });
        if (pos === -1)
            cpiBreadCrumbs.markLastNode({ dirty: false });
    }

    onIFWDocumentsSourceRowSelect = (row) => {
        const grid = $('#tlIFWDocumentsMapping-Grid');

        //don't change record selector if form is dirty
        if (this.ifwDocumentsLastSelection === null) {
            this.ifwDocumentsLastSelection = grid.find(".k-state-selected");
        }
        else {

            if (this.selectionValid()) {
                this.ifwDocumentsLastSelection = grid.find(".k-state-selected");
            }
            else {
                // Clear current selection, and return to last saved selection
                $(row.select()).removeClass("k-state-selected");
                $(this.ifwDocumentsLastSelection).addClass("k-state-selected");

                const container = $(".pto-mapping");
                const error = container.data("dirty-message");
                pageHelper.showErrors(error);
                return;
            }
        }

        const data = row.dataItem(row.select());
        $("#ifwDocumentsMapHdrId").val(data.MapHdrId);

        const gridDue = $("#ifwDocumentsMappedActionsGrid").data("kendoGrid");
        gridDue.dataSource.read();
        $(".ifw-documents-pto-mapping").removeClass("d-none");
    }

    ifwDocumentsGetMapHdrId = () => {
        return {
            mapHdrId: $("#ifwDocumentsMapHdrId").val()
        };
    }
    //ifw documents region end

    //transaction history region
    initializeTrnxHistoryMappingGrids = () => {
        this.trnxHistoryEditableGrids = [
            { name: "trnxHistoryMappedActionsGrid", filter: this.trnxHistoryGetMapParams },
            { name: "trnxHistoryMappedActionToCloseGrid", filter: this.trnxHistoryGetMapSourceId }
        ];
    }

    onTrnxHistorySourceRowDataBound = () => {
        const container = $(".trnx-history-pto-mapping");
        container.addClass("d-none");

        const url = container.data("entry-screen-url");
        $.get(url).done((result) => {
            container.find("#pto-mapping-grids").html(result);
            this.trnxHistoryEditableGrids.forEach((el) => {
                const grid = $(`#${el.name}`);
                pageHelper.kendoGridDirtyTracking(grid, el, this.trnxHistoryAfterGridSaveOrCancel, this.trnxHistoryAfterGridSaveOrCancel, this.onGridDirty);
            });
        });
    }

    trnxHistoryAfterGridSaveOrCancel = () => {
        const pos = this.trnxHistoryEditableGrids.findIndex(function (element) {
            return element.isDirty;
        });
        if (pos === -1)
            cpiBreadCrumbs.markLastNode({ dirty: false });
    }

    onTrnxHistorySourceRowSelect = (row) => {
        const grid = $('#tlTransactionHistoryMapping-Grid');

        //don't change record selector if form is dirty
        if (this.trnxHistoryLastSelection === null) {
            this.trnxHistoryLastSelection = grid.find(".k-state-selected");
        }
        else {

            if (this.selectionValid()) {
                this.trnxHistoryLastSelection = grid.find(".k-state-selected");
            }
            else {
                // Clear current selection, and return to last saved selection
                $(row.select()).removeClass("k-state-selected");
                $(this.trnxHistoryLastSelection).addClass("k-state-selected");

                const container = $(".pto-mapping");
                const error = container.data("dirty-message");
                pageHelper.showErrors(error);
                return;
            }
        }

        const data = row.dataItem(row.select());
        $("#trnxHistoryPTOMappingSearchAction").val(data.MapSearchAction);
        $("#trnxHistoryPTOMappingCountry").val(data.MapCountry);
        $("#trnxHistoryPTOMapSourceId").val(data.MapSourceId);

        const gridDue = $("#trnxHistoryMappedActionsGrid").data("kendoGrid");
        gridDue.dataSource.read();

        const gridClose = $("#trnxHistoryMappedActionToCloseGrid").data("kendoGrid");
        gridClose.dataSource.read();
        $(".trnx-history-pto-mapping").removeClass("d-none");
    }

    onTrnxHistoryMappedActionRowSelect = (row) => {
        const data = row.dataItem(row.select());
        $("#trnxHistoryPTOMappingActionType").val(data.TMSActionType);
    }
    
    trnxHistoryGetMapParams = () => {
        return {
            mapSearchAction: $("#trnxHistoryPTOMappingSearchAction").val(),
            mapCountry: $("#trnxHistoryPTOMappingCountry").val()
        };
    }
    trnxHistoryGetMapSourceId = () => {
        return {
            mapSourceId: $("#trnxHistoryPTOMapSourceId").val()
        };
    }
    trnxHistoryGetMapCountry = () => {
        return {
            mapCountry: $("#trnxHistoryPTOMappingCountry").val()
        };
    }
    trnxHistoryGetActionDueParams = () => {
        return {
            mapCountry: $("#trnxHistoryPTOMappingCountry").val(),
            actionType: $("#trnxHistoryPTOMappingActionType").val()
        };
    }
    

    trnxHistoryOnChange_ActionType = (e) => {
        const actionType = e.sender.value();
        $("#trnxHistoryPTOMappingActionType").val(actionType);

        const container = $(".trnx-history-pto-mapping").find(".mapping-header");
        const actionParamUrl = container.data("action-param-url");
        $.get(actionParamUrl, this.trnxHistoryGetActionDueParams()).done((result) => {

            let addedCounter = 0;
            if (result.length > 0) {
                const grid = $(".trnx-history-pto-mapping").find("#trnxHistoryMappedActionsGrid").data("kendoGrid");

                result.forEach((item, index) => {

                    const dataItem = grid.dataSource.data().find((el) => {
                        return el.TMSActionType === actionType && el.TMSActionDue === item.ActionDue;
                    });

                    if (!dataItem) {
                        addedCounter++;

                        const entry = {
                            IncludeDisplay: true,
                            IncludeCompare: true,
                            IncludeUpdate: false,
                            Yr: item.Yr,
                            Mo: item.Mo,
                            Dy: item.Dy,
                            Indicator: item.Indicator
                        };

                        entry.TMSActionType = actionType;
                        entry.TMSActionDue = item.ActionDue;
                        grid.dataSource.insert(index, entry);
                    }
                });

                if (addedCounter > 0) {
                    const dataItem = grid.dataSource.at(addedCounter);
                    grid.dataSource.remove(dataItem);

                    grid.dataSource.data().forEach((item) => {
                        item.dirty = true;
                    });
                }

            }
        });
    }
    trnxHistoryOnChange_ActionDue = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");

            //get values from action parameter
            const yr = e.dataItem["Yr"];
            const mo = e.dataItem["Mo"];
            const dy = e.dataItem["Dy"];
            const indicator = e.dataItem["Indicator"];

            //and use it as defaults
            const grid = $(".trnx-history-pto-mapping").find("#trnxHistoryMappedActionsGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);
            dataItem.Yr = yr;
            dataItem.Mo = mo;
            dataItem.Dy = dy;
            dataItem.Indicator = indicator;

            $(row).find(".yr-field").html(yr);
            $(row).find(".mo-field").html(mo);
            $(row).find(".dy-field").html(dy);
            $(row).find(".indicator-field").html(indicator);

        }
    }

    trnxHistoryDeleteGridRow(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));

        if (dataItem.MapDueId > 0 || dataItem.MapCloseId > 0) {
            pageHelper.deleteGridRow(e, dataItem);
        }
        else {
            grid.removeRow($(e.currentTarget).closest("tr"));
        }
    }
    //transaction history region end


}





