export default class EPOMappingPage {
    constructor() {
        this.mappingType = "EPODocuments";
    }

    initializeUpdate = () => {
        this.loadUpdateScreen("EPODocuments");
        this.loadUpdateScreen("EPOActions");
    }

    initializeTabs = () => {
        $("#epoMappingMain-tab a").off("click");
        $("#epoMappingMain-tab a").on("click", (e) => {
            //expand search criteria tab
            const tab = $(".epo-mapping-main .refine-search .nav-link");
            tab.addClass("show");
            tab.siblings(".accordion-content").addClass("show").css("display", "block");

            //sync horizontal tabs
            var href = $(e.currentTarget).attr('href');
            $('.nav-tabs#epoMappingMain-tab li a').removeClass('active');
            $('.nav-tabs li a[href="' + href + '"]').addClass('active');
            $('.tab-pane').removeClass('active');
            $('.tab-pane' + href).addClass('active');
        });
    }

    loadUpdateScreen(mappingType) {
        let updateScreen;

        switch (mappingType) {
            case "EPODocuments":
                updateScreen = $("#mappingScreenEPODocuments");
                updateScreen.show();
                break;

            case "EPOActions":
                updateScreen = $("#mappingScreenEPOActions");
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
        const url = `${baseUrl}/Patent/EPOMapping/GetMappingScreen`;
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

    //epo documents region
    initializeEPODocumentsMappingGrids = () => {
        const topGrid = $('#epoDocumentsMapping-Grid');
        const el = {
            name: 'epoDocumentsMapping-Grid', isDirty: false
        };
        this.epoDocumentsEditableGrids = [{el}];
        pageHelper.kendoGridDirtyTracking(topGrid, el, this.epoDocumentsAfterGridSaveOrCancel, this.epoDocumentsAfterGridSaveOrCancel, this.onGridDirty);
        this.epoDocumentsEditableGrids.push({ name: 'epoDocumentsMappedActionsGrid' });
        this.epoDocumentsEditableGrids.push({ name: 'epoDocumentsMappedTagsGrid' });
    }

    onEPODocumentsSourceRowDataBound = () => {
        const container = $(".epo-documents-mapping");
        container.addClass("d-none");

        const url = container.data("entry-screen-url");
        $.get(url).done((result) => {
            container.find("#epo-doc-mapping-grids").html(result);
            this.epoDocumentsEditableGrids.forEach((el) => {
                const grid = $(`#${el.name}`);
                pageHelper.kendoGridDirtyTracking(grid, el, this.epoDocumentsAfterGridSaveOrCancel, this.epoDocumentsAfterGridSaveOrCancel, this.onGridDirty);
            });
        });
    }

    epoDocumentsAfterGridSaveOrCancel = () => {
        const pos = this.epoDocumentsEditableGrids.findIndex(function (element) {
            return element.isDirty;
        });
        if (pos === -1)
            cpiBreadCrumbs.markLastNode({ dirty: false });
    }

    onEPODocumentsSourceRowSelect = (row) => {
        const grid = $('#epoDocumentsMapping-Grid');

        //don't change record selector if form is dirty
        if (this.epoDocumentsLastSelection === null) {
            this.epoDocumentsLastSelection = grid.find(".k-state-selected");
        }
        else {

            if (this.selectionValid()) {
                this.epoDocumentsLastSelection = grid.find(".k-state-selected");
            }
            else {
                // Clear current selection, and return to last saved selection
                $(row.select()).removeClass("k-state-selected");
                $(this.epoDocumentsLastSelection).addClass("k-state-selected");

                const container = $(".epo-documents-mapping");
                const error = container.data("dirty-message");
                pageHelper.showErrors(error);
                return;
            }
        }

        const data = row.dataItem(row.select());
        $("#epoDocumentsDocumentCode").val(data.DocumentCode);

        const gridDue = $("#epoDocumentsMappedActionsGrid").data("kendoGrid");
        gridDue.dataSource.read();        

        const gridTag = $("#epoDocumentsMappedTagsGrid").data("kendoGrid");
        gridTag.dataSource.read();

        $(".epo-documents-mapping").removeClass("d-none");
    }

    epoDocumentsGetDocumentCode  = () => {
        return {
            documentCode: $("#epoDocumentsDocumentCode").val()
        };
    }

    epoDocumentsGetActionDueParams = () => {
        return { actionType: $("#epoDocumentsMappingActionType").val() };
    }    

    epoDocumentsOnChange_ActionType = (e) => {
        const actionType = e.sender.value();
        $("#epoDocumentsMappingActionType").val(actionType);

        const container = $(".epo-documents-mapping").find(".mapping-header");
        const actionParamUrl = container.data("action-param-url");
        $.get(actionParamUrl, this.epoDocumentsGetActionDueParams()).done((result) => {

            let addedCounter = 0;
            if (result.length > 0) {
                const grid = $(".epo-documents-mapping").find("#epoDocumentsMappedActionsGrid").data("kendoGrid");
                result.forEach((item, index) => {
                    const dataItem = grid.dataSource.data().find((el) => {
                        return el.ActionType === actionType && el.ActionDue === item.ActionDue;
                    });

                    if (!dataItem) {
                        addedCounter++;
                        const entry = {
                            MapDueId: 0,
                            DocumentCode: $("#epoDocumentsDocumentCode").val(),
                            Yr: item.Yr,
                            Mo: item.Mo,
                            Dy: item.Dy,
                            Indicator: item.Indicator
                        };

                        entry.ActionType = actionType;
                        entry.ActionDue = item.ActionDue;
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

    epoDocumentsOnChange_ActionDue = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");

            //get values from action parameter
            const yr = e.dataItem["Yr"];
            const mo = e.dataItem["Mo"];
            const dy = e.dataItem["Dy"];
            const indicator = e.dataItem["Indicator"];

            //and use it as defaults
            const grid = $(".epo-documents-mapping").find("#epoDocumentsMappedActionsGrid").data("kendoGrid");
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

    onEPODocumentsMappedActionRowSelect = (row) => {
        const data = row.dataItem(row.select());
        $("#epoDocumentsMappingActionType").val(data.ActionType);
    }  

    onEPODocumentsMappedActTagRowEdit = (e) => {        
        if (e && e.model && (!e.model.DocumentCode || e.model.DocumentCode < '')) {
            e.model.DocumentCode = $("#epoDocumentsDocumentCode").val();
        }
    } 
    //epo documents region end

    //epo actions region
    initializeEPOActionsMappingGrids = () => {
        const topGrid = $('#epoActionsMapping-Grid');
        const el = {
            name: 'epoActionsMapping-Grid', isDirty: false
        };
        this.epoActionsEditableGrids = [{el}];
        pageHelper.kendoGridDirtyTracking(topGrid, el, this.epoActionsAfterGridSaveOrCancel, this.epoActionsAfterGridSaveOrCancel, this.onGridDirty);
        this.epoActionsEditableGrids.push({ name: 'epoActionsMappedActionsGrid' });        
    }

    onEPOActionsSourceRowDataBound = () => {
        const container = $(".epo-actions-mapping");
        container.addClass("d-none");

        const url = container.data("entry-screen-url");
        $.get(url).done((result) => {
            container.find("#epo-act-mapping-grids").html(result);
            this.epoActionsEditableGrids.forEach((el) => {
                const grid = $(`#${el.name}`);
                pageHelper.kendoGridDirtyTracking(grid, el, this.epoActionsAfterGridSaveOrCancel, this.epoActionsAfterGridSaveOrCancel, this.onGridDirty);
            });
        });
    }

    epoActionsAfterGridSaveOrCancel = () => {
        const pos = this.epoActionsEditableGrids.findIndex(function (element) {
            return element.isDirty;
        });
        if (pos === -1)
            cpiBreadCrumbs.markLastNode({ dirty: false });
    }

    onEPOActionsSourceRowSelect = (row) => {
        const grid = $('#epoActionsMapping-Grid');

        //don't change record selector if form is dirty
        if (this.epoActionsLastSelection === null) {
            this.epoActionsLastSelection = grid.find(".k-state-selected");
        }
        else {

            if (this.selectionValid()) {
                this.epoActionsLastSelection = grid.find(".k-state-selected");
            }
            else {
                // Clear current selection, and return to last saved selection
                $(row.select()).removeClass("k-state-selected");
                $(this.epoActionsLastSelection).addClass("k-state-selected");

                const container = $(".epo-actions-mapping");
                const error = container.data("dirty-message");
                pageHelper.showErrors(error);
                return;
            }
        }

        const data = row.dataItem(row.select());
        $("#epoActionsTermId").val(data.TermId);

        const gridDue = $("#epoActionsMappedActionsGrid").data("kendoGrid");
        gridDue.dataSource.read();
        
        $(".epo-actions-mapping").removeClass("d-none");
    }

    epoActionsGetTermId = () => {
        return {
            termId: $("#epoActionsTermId").val()
        };
    }

    epoActionsGetActionDueParams = () => {
        return { actionType: $("#epoActionsMappingActionType").val() };
    }    

    epoActionsOnChange_ActionType = (e) => {
        const actionType = e.sender.value();
        $("#epoActionsMappingActionType").val(actionType);

        const container = $(".epo-actions-mapping").find(".mapping-header");
        const actionParamUrl = container.data("action-param-url");
        $.get(actionParamUrl, this.epoActionsGetActionDueParams()).done((result) => {

            let addedCounter = 0;
            if (result.length > 0) {
                const grid = $(".epo-actions-mapping").find("#epoActionsMappedActionsGrid").data("kendoGrid");
                result.forEach((item, index) => {
                    const dataItem = grid.dataSource.data().find((el) => {
                        return el.ActionType === actionType && el.ActionDue === item.ActionDue;
                    });

                    if (!dataItem) {
                        addedCounter++;
                        const entry = {
                            MapDueId: 0,
                            TermId: $("#epoActionsTermId").val(),                            
                            Indicator: item.Indicator
                        };

                        entry.ActionType = actionType;
                        entry.ActionDue = item.ActionDue;
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

    epoActionsOnChange_ActionDue = (e) => {
        if (e.item) {
            const row = $(`#${e.sender.element[0].id}`).closest("tr");

            //get values from action parameter            
            const indicator = e.dataItem["Indicator"];

            //and use it as defaults
            const grid = $(".epo-actions-mapping").find("#epoActionsMappedActionsGrid").data("kendoGrid");
            const dataItem = grid.dataItem(row);            
            dataItem.Indicator = indicator; 
            
            $(row).find(".indicator-field").html(indicator);
        }
    }    

    onEPOActionsMappedActionRowSelect = (row) => {
        const data = row.dataItem(row.select());
        $("#epoActionsMappingActionType").val(data.ActionType);
    }  

    onEPOActionsMappedActRowEdit = (e) => {        
        if (e && e.model && (!e.model.TermId || e.model.TermId < 0)) {
            e.model.TermId = $("#epoActionsTermId").val();
        }
    } 
    //epo actions region end

    epoDeleteGridRow(e, grid) {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));

        if (dataItem.MapDueId > 0 || dataItem.MapTagId > 0) {
            pageHelper.deleteGridRow(e, dataItem);
        }
        else {
            grid.removeRow($(e.currentTarget).closest("tr"));
        }
    }
}