import SearchPage from "../searchPage";

export default class IDSManage extends SearchPage {

    constructor() {
        super();
        this.idsLib = window.idsLib;
    }

    caseNumberSearchValueMapper(options) {
        const url = $("#idsManageSearchMainTabContent").data("case-number-mapper-url");

        $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }

    onRowSelect(row) {
        const data = row.dataItem(row.select());
        const existing = this.getInstance(data.AppId);

        if (!existing) {
            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Patent/PatIDSManage/GetIDSInfo`;
            const self = this;
            $.get(url, { appId: data.AppId }).done(function (result) {
                const detailContainer = $("#idsManageDetail");
                self.detailActiveTab = detailContainer.find(".nav-link.active").attr("id");

                const instances = detailContainer.find(".ids-manage-app");
                if (instances.length > 0) {
                    const instance = $(instances[0]);
                    const keep = instance.data("keep") === "true";

                    if (!keep)
                        instance.remove();

                    detailContainer.prepend(result);

                }
                else
                    detailContainer.html(result);

                //restore default tab
                if (self.detailActiveTab && self.detailActiveTab.substr(0, 3) !== "ref") {
                    const newInstance = $(detailContainer.find(".ids-manage-app")[0]);
                    newInstance.find(".nav-tabs .nav-link").removeClass("active");
                    $(newInstance.find(".nav-tabs .nav-link")[1]).addClass("active");

                    newInstance.find(".tab-content .tab-pane").removeClass("active show");
                    $(newInstance.find(".tab-content .tab-pane")[1]).addClass("active show");
                }

                const gridIds = self.getIDSGridsId(data.AppId);
                self.enableDragDropIDSGrid(gridIds);

                //sorter
                const refGridSelector = $(`#relatedCasesGrid_${data.AppId}`);
                const grid = refGridSelector.data("kendoGrid");
                refGridSelector.find(".k-grid-header span.k-link").sorter(grid, true);

            }).fail(function (error) {
                pageHelper.showErrors(error.responseText);
            });
        }

    }

    initializeSearchResultPage = (searchResultPage) => {
        this.searchResultGrid = $(searchResultPage.grid);
        this.searchResultContainer = searchResultPage.container;
        this.refineSearchContainer = searchResultPage.refineSearchContainer;

        const filterCount = pageHelper.initializeSidebar(this);

        if (this.searchResultGrid.length > 0) {
            const resultsGrid = this.searchResultGrid.data("kendoGrid");
            resultsGrid.dataSource.read();
        }

        const refreshSearchResultPage = function () {
            pageHelper.moveBreadcrumbs(searchResultPage.container);

            filterCount.refreshAll();
            filterCount.openDefault();

            const grid = $(searchResultPage.grid).data("kendoGrid");
            grid.dataSource.read();
        };

        window.cpiBreadCrumbs.addNode({
            name: $(searchResultPage.container).attr("id"),
            label: searchResultPage.title,
            url: searchResultPage.url,
            refresh: true,
            updateHistory: true,
            refreshHandler: refreshSearchResultPage
        });
        this.cpiStatusMessage.hide();
        pageHelper.moveBreadcrumbs(searchResultPage.container);
    }

    getInstance = (id) => {
        const detailContainer = $("#idsManageDetail");
        const instances = detailContainer.find(".ids-manage-app");
        let existing = false;

        if (instances.length > 0) {
            $.each(instances, function () {
                const instance = $(this);

                if (parseInt(instance.data("id")) === parseInt(id)) {
                    existing = true;
                    return false;
                }
            });
        }
        return existing;
    }

    enableDragDropIDSGrid = (el) => {
        const grids = $(`${el}`);
        const self = this;

        grids.kendoDraggable({
            filter: "tr",
            hint: function (e) {
                const grid = $(e.parents(".kendo-Grid")).data("kendoGrid");

                //grid.selectedKeyNames() sorts the list, so we use _selectedIds
                const ids = [];
                for (var property in grid._selectedIds) {
                    if (property) {
                        ids.push(property);
                    }
                }
                const selected = ids.map(k => {
                    if (e.parents(".kendo-Grid").attr("id").startsWith("relatedCases")) {
                        const relateCase = grid.dataSource.view().filter(r => r.RelatedCasesId.toString() === k)[0];
                        return self.buildRelatedCasesDragRow(relateCase);
                    } else {
                        const nonPat = grid.dataSource.view().filter(n => n.NonPatLiteratureId.toString() === k)[0];
                        return self.buildNonPatDragRow(nonPat);
                    }
                }
                );
                const html = '<div class="k-grid k-widget border border-primary"><table><tbody>' + selected.join("") + '</tbody></table></div>';
                return $(html);
            }
        });

        grids.each(function (index) {
            const kendoGrid = $(this).data("kendoGrid");
            kendoGrid.wrapper.kendoDropTarget({
                drop: function (e) {
                    const sourceEl = e.draggable.element[0].id;
                    const dtnEl = $(this)[0].element[0].id;

                    //check if type is the same
                    if (sourceEl.split("_")[0] !== dtnEl.split("_")[0]) {
                        return;
                    }

                    if (sourceEl !== dtnEl) {
                        const sourceGrid = $(`#${sourceEl}`).data("kendoGrid");
                        const sourceIds = [];
                        const draggedRows = e.draggable.hint.find("tr").each(function () {
                            if (index === 0) {
                                sourceIds.push({
                                    RelatedCasesId: $(this.cells[0]).attr("id")
                                    //BaseAppId: 0
                                });
                            }
                            else {
                                sourceIds.push($(this.cells[0]).attr("id"));
                            }
                        });

                        const sourceData = sourceGrid.dataSource.data();
                        sourceIds.forEach(e => {
                            const rec = sourceData.find(r => r.RelatedCasesId === +e.RelatedCasesId);
                            if (rec) {
                                e.BaseAppId = rec.RelatedAppId
                            }
                        });

                        const dataItem = sourceGrid.dataSource.getByUid(e.draggable.currentTarget.data("uid"));
                        const dtnAppId = (dtnEl).split("_")[1];

                        const url = $("#idsManageDetail").data(index === 0 ? "url-copy-ref" : "url-copy-lit");
                        const data = { appId: dtnAppId, from: sourceIds };

                        $.post(url, data)
                            .done(() => {
                                kendoGrid.dataSource.read();
                                kendoGrid.refresh();
                            })
                            .fail(function (error) { pageHelper.showErrors(error.responseText); });
                    }
                }
            });
        });
    }

    getKey(id) {
        return `ids_${id}`;
    }

    getIDSGridsId(id) {
        return `#relatedCasesGrid_${id},#nonPatLiteratureGrid_${id}`;
    }

    buildRelatedCasesDragRow(rec) {
        const item =
            `<tr><td id="${rec.RelatedCasesId}">${rec.Relationship}</td>
        <td>${rec.RelatedCaseNumber}<br/>${rec.RelatedCountry}&nbsp${rec.RelatedSubCase}</td> 
        <td>${this.formatToGridDisplay(rec.RelatedPubNumber)}<br/>${this.formatDateToGridDisplay(rec.RelatedPubDate)}</td>
        <td>${this.formatToGridDisplay(rec.RelatedPatNumber)}<br/>${this.formatDateToGridDisplay(rec.RelatedIssDate)}</td>
        <td>${this.formatToGridDisplay(rec.RelatedFirstNamedInventor)}<br/>${this.formatToGridDisplay(rec.ReferenceSrc)}</td>
        </tr>`;
        return item;
    }

    buildNonPatDragRow(rec) {
        const item =
            `<tr><td id="${rec.NonPatLiteratureId}">${this.formatToGridDisplay(rec.NonPatLiteratureInfo)}</td></tr>`;
        return item;
    }

    formatToGridDisplay(data) {
        return data === null || data === "" ? "&nbsp;" : data;
    }
    formatDateToGridDisplay(data) {
        return data === null ? "&nbsp;" : pageHelper.cpiDateFormatToDisplay(data);
    }

    initialize() {
        const self = this;
        const mainContainer = $("#idsManageDetail");

        $(mainContainer).on("click",
            ".copy-link",
            function () {
                const openLink = $(this);
                const instance = openLink.parents(".ids-manage-app");
                const appId = instance.data("id");

                const activeTab = $(instance).find(".nav-link.active").attr("id");
                const recordType = activeTab.substr(0, 3) === "ref" ? "R" : "N";
                self.openCopyToFamilyScreen(appId, recordType);
            });


        $(mainContainer).on("click",
            ".update-date-link",
            function () {
                const openLink = $(this);
                const instance = openLink.parents(".ids-manage-app");
                const appId = instance.data("id");

                const activeTab = $(instance).find(".nav-link.active").attr("id");
                const recordType = activeTab.substr(0, 3) === "ref" ? "R" : "N";
                self.openMassUpdateScreen(appId, recordType);
            });

        $(mainContainer).on("click",
            ".keep-open-link",
            function () {
                const openLink = $(this);
                const instance = openLink.parents(".ids-manage-app");
                instance.data("keep", "true");
                openLink.addClass("d-none");
                $(mainContainer).find(".close-link").removeClass("d-none");
            });

        $(mainContainer).on("click",
            ".close-link",
            function () {
                const closeLink = $(this);
                const instance = closeLink.parents(".ids-manage-app");
                instance.remove();
            });

        $(mainContainer).on("click", ".details-link", function (e) {
            e.preventDefault();
            const detailsLink = $(this);
            const instance = detailsLink.parents(".ids-manage-app");
            const id = instance.data("id");

            cpiStatusMessage.hide();
            pageHelper.openDetailsLink(detailsLink);

            const breadCrumbs = cpiBreadCrumbs.getNodes();
            const lastNode = breadCrumbs[breadCrumbs.length - 1];
            lastNode.refreshHandler = function () {
                const relatedCasesGrid = $(`#relatedCasesGrid_${id}`).data("kendoGrid");
                relatedCasesGrid.dataSource.read();
                const nonPatLiteratureGrid = $(`#nonPatLiteratureGrid_${id}`).data("kendoGrid");
                nonPatLiteratureGrid.dataSource.read();
            }
        });
    }

    openMassUpdateScreen(appId, recordType) {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Patent/PatIDSManage/UpdateFilDateScreen`;

        $.get(url, { appId: appId, recordType: recordType })
            .done((result) => {
                const popupContainer = $(".cpiContainerPopup").last();
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
                                grid = $(`#relatedCasesGrid_${appId}`).data("kendoGrid");
                            else
                                grid = $(`#nonPatLiteratureGrid_${appId}`).data("kendoGrid");

                            grid.dataSource.read();
                            dialogContainer.modal("hide");
                        }
                    }
                );
            })
            .fail((e => {
                pageHelper.showErrors(e);
            }));
    }

    openCopyToFamilyScreen(appId, recordType) {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Patent/PatIDSManage/CopyToFamilyScreen`;

        $.get(url, { appId: appId, recordType: recordType })
            .done((result) => {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $("#patIDSCopyToFamilyDialog");
                dialogContainer.modal("show");

                const copyToGrid = $("#copyToFamilyCopyToCasesGrid").data("kendoGrid");
                let sourceGrid;
                if (recordType === "R") {
                    //sorter
                    const refGridSelector = $(`#copyToFamilyRelatedCasesGrid`);
                    sourceGrid = refGridSelector.data("kendoGrid");
                    refGridSelector.find(".k-grid-header span.k-link").sorter(sourceGrid, true);
                }
                else
                    sourceGrid = $(`#copyToFamilyNonPatLiteratureGrid`).data("kendoGrid");

                dialogContainer.on("change", "input[name='GroupType']", (e) => {
                    copyToGrid.dataSource.read();
                });

                dialogContainer.find("#copy").on("click", ()=> {
                    const copyFrom = sourceGrid.selectedKeyNames();
                    const copyTo = copyToGrid.selectedKeyNames();

                    const allData = copyToGrid.dataSource.data();
                    const copyToIds = copyTo.map((item) => {
                        let genAction = false;
                        const existing = allData.find(r => r.AppId === parseInt(item));
                        if (existing) {
                            let dataItem = copyToGrid.dataSource.getByUid(existing.uid);
                            genAction = dataItem.GenerateAction;
                        }
                        return { appId: item, genAction: genAction }
                    });

                    if (copyFrom.length > 0 && copyTo.length > 0) {
                        const data = {
                            appId: appId,
                            relatedCasesIds: copyFrom,
                            actionToGenerate: dialogContainer.find("input[name='ActionToGenerate']").val(),
                            baseDate: pageHelper.cpiDateFormatToSave(dialogContainer.find("#BaseDate").data("kendoDatePicker").value()),
                            dueDate: dialogContainer.find("#DueDate").val(),
                            indicator: dialogContainer.find("#Indicator").val(),
                            recordType: recordType,
                            selection: copyToIds
                        };
                        this.idsCopyToFamily(dialogContainer, data);
                    }
                    else {
                        const error = dialogContainer.data("required-error");
                        dialogContainer.find(".message").html(error);
                        dialogContainer.find(".page-status").css("display", "block");
                    }
                    
                });
            })
            .fail((e => {
                pageHelper.showErrors(e);
            }));
    }

    idsCopyToFamily(screen, data) {
        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Patent/PatIDSManage/CopyToFamily`;

        $.post(url, { selection: data })
            .done(function (result) {
                pageHelper.showSuccess(result.success);
                screen.modal("hide");
            })
            .fail(function (e) {
                const error = pageHelper.getErrorMessage(e);
                screen.find(".message").html(error);
                screen.find(".page-status").css("display", "block");
            });
    }

    getCopyToFamilyCasesParam() {
        const container = $("#patIDSCopyToFamilyDialog");
        return {
            appId: container.find("input[name='AppId']").val(),
            relatedBy: container.find("input[name='GroupType']:checked").val(),
            activeOnly: container.find("#idsCopyToFamilyShowActiveOnly").is(':checked')
        }
    }

    idsCopySelectionChange(grid) {
        const parent = $(grid.element).closest("div.modal");
        
        const copyButton = $(parent).find("#copy");
        if (grid.selectedKeyNames().length > 0)
            copyButton.removeAttr("disabled");
        else
            copyButton.attr("disabled", "disabled");
    }

    onChange_ActionType = (e) => {
        const actionType = e.sender.value();
        const dialogContainer = $("#patIDSCopyToFamilyDialog");
        const country = dialogContainer.find("#Country").val();
        const baseDate = pageHelper.cpiDateFormatToSave(dialogContainer.find("#BaseDate").data("kendoDatePicker").value());

        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Patent/PatIDSManage/GetFileIDSActionDetails`;
        $.get(url, {country: country, actionType: actionType, baseDate: baseDate })
            .done((result) => {
                dialogContainer.find("#Indicator").val(result.Indicator);
                dialogContainer.find("#DueDate").val(result.DueDate);
                dialogContainer.find("#DueDateFormatted").val(result.DueDateFormatted);
                dialogContainer.find("#ActParamId").val(result.ActParamId);
            })
            .fail((e => {
                pageHelper.showErrors(e);
            }));

    }

    idsCopyFamilyBaseDateChange = (e) => {
        if (e.sender.value() === null)
            e.sender.value(new Date());

        const baseDate = pageHelper.cpiDateFormatToSave(e.sender.value());
        const container = $("#patIDSCopyToFamilyDialog");

        const baseUrl = $("body").data("base-url");
        const url = `${baseUrl}/Patent/PatIDS/ComputeIDSFileDueDate`;
        $.get(url, { baseDate: baseDate, actParamId: container.find("#ActParamId").val() })
            .done(function (result) {
                console.log(result);
                container.find("#DueDate").val(result.dueDate);
                container.find("#DueDateFormatted").val(result.dueDateFormatted);
            })
            .fail(function (e) {
                pageHelper.showErrors(e.responseText);
            });

    }
}