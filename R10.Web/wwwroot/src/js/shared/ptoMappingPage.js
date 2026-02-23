import SearchPage from "../searchPage";

export default class PTOMappingPage extends SearchPage {

    constructor() {
        super();
        this.ptoMappingSearchAction = "#ptoMappingSearchAction";
        this.ptoMappingCountry = "#ptoMappingCountry";
        this.mapSourceId = "#mapSourceId";
        this.ptoMappingActionType = "#ptoMappingActionType";
        this.gridName = null;
        this.lastSelection = null;
    }

    initialize = (gridName) => {
        this.gridName = gridName;
        this.editableGrids = [
            { name: "mappedActionsGrid", filter: this.getMapParams },
            { name: "mappedActionToCloseGrid", filter: this.getMapSourceId }
        ];
    }

    onSourceRowDataBound = () => {
        const container = $(".pto-mapping");
        container.addClass("d-none");

        const url = container.data("entry-screen-url");
        $.get(url).done((result) => {
            $("#pto-mapping-grids").html(result);
            this.editableGrids.forEach((el) => {
                const grid = $(`#${el.name}`);
                pageHelper.kendoGridDirtyTracking(grid, el, this.afterGridSaveOrCancel, this.afterGridSaveOrCancel, this.onGridDirty);
            });
        });
    }

    afterGridSaveOrCancel = () => {
        const pos = this.editableGrids.findIndex(function (element) {
            return element.isDirty;
        });
        if (pos === -1)
            cpiBreadCrumbs.markLastNode({ dirty: false });
    }

    onGridDirty = () => {
        cpiBreadCrumbs.markLastNode({ dirty: true });
    }

    selectionValid = () => {
        return !cpiBreadCrumbs.hasDirtyNode();
    }

    onSourceRowSelect = (row) => {
        const grid = $(`#${this.gridName}`);

        //don't change record selector if form is dirty
        if (this.lastSelection === null) {
            this.lastSelection = grid.find(".k-state-selected");
        }
        else {

            if (this.selectionValid()) {
                this.lastSelection = grid.find(".k-state-selected");
            }
            else {
                // Clear current selection, and return to last saved selection
                $(row.select()).removeClass("k-state-selected");
                $(this.lastSelection).addClass("k-state-selected");

                const container = $(".pto-mapping");
                const error = container.data("dirty-message");
                pageHelper.showErrors(error);
                return;
            }
        }

        const data = row.dataItem(row.select());
        $(this.ptoMappingSearchAction).val(data.MapSearchAction);
        $(this.ptoMappingCountry).val(data.MapCountry);
        $(this.mapSourceId).val(data.MapSourceId);

        const gridDue = $("#mappedActionsGrid").data("kendoGrid");
        gridDue.dataSource.read();

        const gridClose = $("#mappedActionToCloseGrid").data("kendoGrid");
        gridClose.dataSource.read();

        $(".pto-mapping").removeClass("d-none");
    }

    deleteGridRow(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));

        if (dataItem.MapDueId > 0 || dataItem.MapCloseId > 0) {
            pageHelper.deleteGridRow(e, dataItem);
        }
        else {
            grid.removeRow($(e.currentTarget).closest("tr"));
        }
    }

    getMapParams = () => {
        return {
            mapSearchAction: $(this.ptoMappingSearchAction).val(),
            mapCountry: $(this.ptoMappingCountry).val()
        };
    }

    getMapCountry = () => {
        return {
            mapCountry: $(this.ptoMappingCountry).val()
        };
    }

    getActionDueParams = () => {

        return {
            mapCountry: $(this.ptoMappingCountry).val(),
            actionType: $(this.ptoMappingActionType).val()
        };
    }

    getMapSourceId = () => {
        return {
            mapSourceId: $(this.mapSourceId).val()
        };
    }

    onChange_ActionType = (e) => {
        const actionType = e.sender.value();
        $(ptoMappingActionType).val(actionType);

        const container = $(this.searchResultContainer).find(".mapping-header");
        const system = container.data("system");
        const actionParamUrl = container.data("action-param-url");
        $.get(actionParamUrl, this.getActionDueParams()).done((result) => {

            let addedCounter = 0;
            if (result.length > 0) {
                const grid = $(this.searchResultContainer).find("#mappedActionsGrid").data("kendoGrid");

                result.forEach((item, index) => {

                    const dataItem = grid.dataSource.data().find((el) => {
                        if (system === "P") 
                            return el.PMSActionType === actionType && el.PMSActionDue === item.ActionDue;
                        else 
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

                        if (system === "P") {
                            entry.PMSActionType = actionType;
                            entry.PMSActionDue = item.ActionDue;
                        }
                        else {
                            entry.TMSActionType = actionType;
                            entry.TMSActionDue = item.ActionDue;
                        }
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
    onChange_ActionDue = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");

            //get values from action parameter
            const yr = e.dataItem["Yr"];
            const mo = e.dataItem["Mo"];
            const dy = e.dataItem["Dy"];
            const indicator = e.dataItem["Indicator"];

            //and use it as defaults
            const grid = $(this.searchResultContainer).find("#mappedActionsGrid").data("kendoGrid");
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
    onMappedActionRowSelect = (row) => {
        const data = row.dataItem(row.select());
        $(this.ptoMappingActionType).val(data.PMSActionType);
    }
}