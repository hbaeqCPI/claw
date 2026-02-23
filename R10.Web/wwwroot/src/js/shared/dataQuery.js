import SearchEntryPage from "../searchEntryPage";

export default class DataQuery extends SearchEntryPage {

    constructor() {
        super();
        this.loadTabContent = this.loadTabContent.bind(this);
        this.initializeEditableGrids = this.initializeEditableGrids.bind(this);
        this.detailActiveTabId = "";
    }

    initializeDetailContentHeader(detailContentHeader) {
        const havingCustomWidget = detailContentHeader.havingCustomWidget;
        if (havingCustomWidget) {
            document.getElementById("createModifySpan").innerHTML = ModifyWidgetLabel;
        } else {
            document.getElementById("createModifySpan").innerHTML = CreateWidgetLabel;
        }

        this.detailContainer = $(`#${detailContentHeader.detailHeaderContainer}`);
        this.detailContentContainer = $(this.detailContainer).find(".cpiDataContainer");

        const detailHeaderContainer = this.detailContainer;                                                     // dataQueryDetail

        const self = this;
        const mainControlButtons = $(detailHeaderContainer).find(".cpiButtonsDetail");
        const saveCancelButtons = $(detailHeaderContainer).find("#editActionButtons");                          // actionsButtons
        const aqbContainer = $(this.aqbContainerId);
        const isAQBReadOnly = detailContentHeader.isAQBReadOnly;

        let addMode = detailContentHeader.addMode;
        let id = detailContentHeader.id;

        let entryForm = detailHeaderContainer.find("form")[0];

        // reset dirty flags
        this.isParentDirty = false;
        let isAQBDirty = false;                             // flag for AQB SQL changes
        cpiBreadCrumbs.markLastNode({ dirty: false });

        // set up AQB area
        const clearQueryResultGrid = function () {
            const gridId = aqbContainer.data("result-grid");
            var grid = $(`#${gridId}`); //.data("kendoGrid");
            if (grid) {
                grid.empty();
            }
        }

        aqbContainer.find(".dq-query-tab").tab('show');             // set focus on tab with AQB, else it will not refresh AQB
        const sqlExpr = $('#SQLExpr').val();                    // set AQB expr; this will trigger AQB SqlChanged, which will trigger SQLExpr change event below
        AQB.Web.Core.updateSQL(sqlExpr);
        AQB.Web.Core.sendDataToServer();

        if (isAQBReadOnly) {
            $("#divAQB").addClass("masked-area");
            $("#aqbRemarks").attr("readonly", true);
        }
        else {
            $("#divAQB").removeClass("masked-area");
            $("#aqbRemarks").attr("readonly", false);
        }
        clearQueryResultGrid();


        // note: breadcrumbs moved up to avoid detail content refresh issues
        //pageHelper.moveBreadcrumbs(`#${detailContentHeader.detailHeaderContainer}`);
        self.moveSideBar(false);

        // attach jquery validator
        $.validator.unobtrusive.parse(entryForm);
        entryForm = $(entryForm);
        entryForm.data("validator").settings.ignore = "";               // include hidden fields (kendo controls)
        pageHelper.addMaxLength(entryForm);                             // auto add maxlength to entry fields

        // main buttons
        if (detailHeaderContainer.length > 0) {
            self.manageDetailPageButtons({ detailContainer: detailHeaderContainer, id: id });
        }

        //download power bi connector button
        detailHeaderContainer.find("#downloadPowerBIConnectorButton").on("click", (e) => {
            window.fileUtility.getFileStream($(e.currentTarget).data("url"));
        });

        // record stamps - in separate section of page
        const recordStampsUrl = entryForm.data("recordstamp-url");
        self.refreshRecordStamps(recordStampsUrl, id, addMode);

        // show save/cancel buttons, hide other buttons
        const setToSaveMode = function () {
            saveCancelButtons.removeClass("d-none");
            mainControlButtons.hide();
        };

        const setToViewMode = function () {
            saveCancelButtons.addClass("d-none");
            mainControlButtons.show();
            cpiBreadCrumbs.markLastNode({ dirty: false });
            //refreshNodeDirtyFlag();                           // n/a - refreshes grid
        };

        const markDirty = function () {
            self.isParentDirty = true;
            cpiBreadCrumbs.markLastNode({ dirty: true });
            detailHeaderContainer.addClass("dirty");
            setToSaveMode();
        };

        // attach markDirty to SQLExpr change
        $('#SQLExpr').change(function () {
            console.log("change trigger");

            if (isAQBReadOnly)
                return;
            markDirty();
        });

        // set aqbRemarks with hidden remarks; attach markDirty 
        self.setRemarks("#Remarks", "#aqbRemarks");
        $("#aqbRemarks").on("input", function () {
            markDirty();
        });

        // attach markDirty to input fields
        $(document).on("input", ".cpiMainEntry input, .cpiMainEntry textarea", function () {
            markDirty();
        });

        $(document).on("change", ".cpiMainEntry input", function () {
            markDirty();
        });

        const submitForm = function () {
            cpiLoadingSpinner.show();

            // update hidden remarks inside the form; 
            self.setRemarks("#aqbRemarks", "#Remarks");             // this is the remarks that will post to the server
            if (!addMode)
                self.fillRecordStamp(entryForm);                    // update hidden stamps field inside the form

            const json = pageHelper.formDataToJson(entryForm);

            pageHelper.postJson(entryForm.attr("action"), json)
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showSuccess(entryForm.data("save-message"));

                    self.isParentDirty = false;
                    self.searchResultSelectedChanged = false;

                    setToViewMode();
                    detailHeaderContainer.removeClass("dirty");

                    // refresh search grid client side (w/o resetting everything)
                    self.selectSearchResultRow = false;
                    if (addMode) {
                        const data = { QueryId: result.QueryId, QueryName: result.QueryName };
                        self.insertSearchGridRow(self.searchResultGridId, data);
                        addMode = false;
                    } else {
                        const grid = $(self.searchResultGridId).data("kendoGrid");
                        const select = grid.select();
                        const data = grid.dataItem(select);
                        data.set("QueryName", result.QueryName);
                        $(self.searchResultGridId).find("span.k-dirty").remove();
                        $(self.lastGridSelection).addClass("k-state-selected");
                    }

                    // refresh timestamps
                    id = result.QueryId;
                    const recordStampsUrl = entryForm.data("recordstamp-url");
                    self.refreshRecordStamps(recordStampsUrl, id, addMode);

                    pageHelper.resetComboBoxes(self.searchForm);        // refresh search buttons to reflect saved data

                })
                .fail(function (e) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(e);

                    self.refreshSearchGrid();
                });
        };

        entryForm.on("submit", function (e) {
            e.preventDefault();
            //client side validation (using jquery validation)
            if (entryForm.valid()) {
                submitForm();
            }
            else {
                cpiLoadingSpinner.hide();
                entryForm.wasValidated();
            }
        });

        //workaround for buttons to be visible when search screen is hidden
        //$("#dataQuerySearch").find(".close").on("click", () => {
        //$("#dataQuerySearch").find(".cpiButtonsDetail").prepend("&nbsp;");
        //$("#dataQuerySearch").find("#editActionButtons").prepend("&nbsp;");
        //});
    }

    initializeDetailContentMain(detailContentMain) {
        this.dqMainContainerId = detailContentMain.dqMainContainerId;
        this.aqbContainerId = detailContentMain.aqbContainerId;

        const aqbContainer = $(`${this.aqbContainerId}`);                      //dataQueryDetail
        const aqbResultGridId = detailContentMain.aqbResultGridId;
        const self = this;

        self.tabChangeSetListener();
        self.initializeEditableGrids(0);

        const runQuery = function (url) {
            // 1) get the grid model (url + 'Init'); 2) pass it on to dynamicGrid.createGrid
            cpiLoadingSpinner.show();
            $.ajax({
                url: url + 'Init',
                success: (gridModel) => {
                    const modelJson = JSON.parse(gridModel);
                    dynamicGrid.createGrid(aqbResultGridId, modelJson, url, {});
                    cpiLoadingSpinner.hide();
                },
                error: function (e) {
                    if (e.responseJSON !== undefined)
                        pageHelper.showGridErrors(e.responseJSON);
                    else
                        pageHelper.showErrors(e.responseText);
                    cpiLoadingSpinner.hide();
                }
            });

        }

        const isEmptyQuery = function () {
            var qb = AQB.Web.QueryBuilder;
            if (qb.EditorComponent.getSql() === "") {
                const msg = aqbContainer.data("empty-sql");
                cpiAlert.warning(msg, function () {
                    aqbContainer.find(".dq-query-tab").tab('show');
                });
                return true;
            }
            return false;
        }


        const openExportWidgetEntry = function (url, data, closeOnSave) {
            const self = this;

            $.ajax({
                url: url,
                data: data,
                success: function (result) {
                    const popupContainer = $(".cpiContainerPopup").last();
                    popupContainer.html(result);
                    const dialogContainer = $("#dqExportToWidgetDialog");

                    let entryForm = dialogContainer.find("form")[0];
                    dialogContainer.modal("show");
                    entryForm = $(entryForm);
                    entryForm.cpiPopupEntryForm(
                        {
                            dialogContainer: dialogContainer,
                            closeOnSubmit: closeOnSave,
                            beforeSubmit: function () {
                                //const parentStamp = self.getParentTStamp();
                                //dialogContainer.find("#ParentTStamp").val(parentStamp);
                            },
                            afterSubmit: function () {
                                dialogContainer.modal("hide");
                            }
                        }
                    );
                },
                //error: function (error) {
                //    pageHelper.showErrors(error.responseText);
                //}
            });
        }

        const exportData = function (urlData, check) {
            if (isEmptyQuery()) {
                return;
            }

            const url = aqbContainer.data(urlData);
            const queryName = $('input[name$="QueryName"]')[1].value;
            const aqbResultGrid = $(aqbResultGridId).data("kendoGrid");


            let data = { queryName: queryName };
            if (aqbResultGrid !== undefined) {
                const sort = aqbResultGrid.dataSource.sort();
                if (sort !== undefined)
                    $.extend(data, { sortField: sort[0].field, sortDir: sort[0].dir })
            }

            if (check == false) {
                window.fileUtility.getFileStream(url, data);
            }
            else {
                //check query results large file
                $.ajax({
                    url: aqbContainer.data("export-check-url"),
                    type: "Post",
                    data: data,
                    success: function (response) {
                        if (response) {
                            // query is over 2000 and contains image
                            const title = "Email Export";
                            const msg = "The export will be emailed to you.";
                            cpiConfirm.confirm(title, msg,
                                function () {
                                    //email this export
                                    $.ajax({
                                        url: url.replace("Export", "Email"),
                                        method: "POST",
                                        data: data,
                                        success: function () {
                                            console.log("Request completed successfully.");
                                        },
                                        error: function (xhr, status, error) {
                                            pageHelper.showErrors(error);
                                        }
                                    });
                                },
                                {
                                    "action": { "class": "btn-primary", "label": "Confirm", "icon": "fa fa-check" },
                                    "close": { "class": "btn-secondary", "label": "Cancel", "icon": "fa fa-undo-alt" }
                                }, undefined,
                                function () {
                                    console.log("test3");
                                }
                            );
                        }
                        else {
                            window.fileUtility.getFileStream(url, data);
                        }
                    },
                    error: function (xhr, status, error) {
                        pageHelper.showErrors(error);
                    }
                });
            }
        }

        // Run query button
        aqbContainer.find(".run-query").on("click", function () {
            if (!isEmptyQuery()) {
                const url = aqbContainer.data("run-url");
                runQuery(url);
            }
        });

        // Export to Excel
        aqbContainer.find(".export-excel").on("click", function () {
            exportData("export-excel-url", false);
        });

        // Export to Excel
        aqbContainer.find(".export-word").on("click", function () {
            exportData("export-word-url", false);
        });

        // Export to XML
        aqbContainer.find(".export-xml").on("click", function () {
            exportData("export-xml-url", false);
        });

        // Export to XML
        aqbContainer.find(".export-json").on("click", function () {
            exportData("export-json-url", false);
        });

        aqbContainer.find(".export-rdl").on("click", function () {
            exportData("export-rdl-url", false);
        });

        aqbContainer.find(".export-widget").on("click", function () {
            const queryId = document.getElementById("QueryId").value;
            const url = aqbContainer.data("export-widget-url");
            const data = {
                queryId: queryId,
            };
            openExportWidgetEntry(url, data, true);
        });

        // open query help form
        aqbContainer.find(".query-help").on("click", function () {
            const url = aqbContainer.data("help-url");
            self.showHelp(url);
        });

    }

    initializeEditableGrids(id) {
        this.editableGrids = [
            { name: 'dataQueryTagsGrid', isDirty: false, filter: { parentId: id }, afterSubmit: this.updateRecordStamps },
        ];

        this.configureEditableGrids();
    }

    tabChangeSetListener() {
        const self = this;
        self.tabsLoaded = [];


        $('#dataQueryTab a').on('click', (e) => {
            e.preventDefault();
            self.detailActiveTabId = e.target.id;
            self.loadTabContent(self.detailActiveTabId);
        });
    }

    loadTabContent(tab) {

        const refreshGrid = function (gridId) {
            const gridHandle = $(gridId);
            const grid = gridHandle.data("kendoGrid");
            grid.dataSource.read();
        }

        switch (tab) {
            case "dataQueryTagTab":
                $(document).ready(() => {
                    refreshGrid("#dataQueryTagsGrid");
                });
                const readOnly = document.getElementById("isReadOnlyHeader").value;
                const queryId = document.getElementById("QueryId").value;
                var addMode = queryId == 0;
                if (readOnly == "True" || addMode) {
                    //var grid = $("#dataQueryTagsGrid").data("kendoGrid");
                    //grid.setOptions({
                    //    editable: false
                    //});
                    const add = $('#dataQueryTagsGrid .k-grid-toolbar .k-grid-add');
                    add.hide();
                }
                else {
                    const add = $('#dataQueryTagsGrid .k-grid-toolbar .k-grid-add');
                    add.show();
                }
                break;

            case "":
                break;
        }
    }

    showHelp = (url) => {
        $.get(url)
            .done((result) => {
                //clear all existing hidden popups to avoid kendo id issue
                $(".cpiContainerPopup").empty();

                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
            })
            .fail((e) => {
                pageHelper.showErrors(e.responseText);
            });
    }
    setRemarks(fromRemarksId, toRemarksId) {
        const fromRemarks = $(fromRemarksId).val();
        $(toRemarksId).val(fromRemarks);

        const tab = $("#dataQueryRemarksTab");
        if (fromRemarks === "")
            tab.removeClass("has-remarks");
        else
            tab.addClass("has-remarks");

    };

    refreshRecordStamps(url, id, addMode) {
        const stampContainer = $(this.dqMainContainerId);
        if (addMode) {
            stampContainer.find(".content-stamp").hide();
        } else if (id > 0) {
            const recordStampsUrl = url.replace("recid", id);
            const activePage = { recordStampsUrl: recordStampsUrl, infoContainer: stampContainer };
            pageHelper.updateRecordStamps(activePage);
            stampContainer.find(".content-stamp").show();
        }
    }

    fillRecordStamp(entryForm) {
        // record stamp is shown at the footer separate from the form body; 
        // update the empty tStamp inside the form before posting form to server
        const tStampForm = entryForm.find("#tStamp");
        tStampForm.val(dataQuery.getRecordStamp());
    }

    getQueryId() {
        const queryId = document.getElementById("QueryId").value;
        return {
            id: queryId
        };
    }

    dataQueryTagsGridEdit(e) {
        if (e.model.isNew()) {
            const queryId = document.getElementById("QueryId").value;
            e.model.set("QueryId", queryId);
        }
    }

    dataQueryTagsGridDataBound(e) {
        const readOnly = document.getElementById("isReadOnlyHeader").value;

        var grid = $("#dataQueryTagsGrid").data("kendoGrid");
        var gridRows = grid.tbody.find("tr");

        gridRows.each(function (e) {
            var dataItem = grid.dataItem($(this));
            if (readOnly == "True") {
                $(this).find("td.data-Tag").removeClass("editable-cell");
            }
        })
    }

    isReadOnly(e) {
        const readOnly = document.getElementById("isReadOnlyHeader").value;
        const queryId = document.getElementById("QueryId").value;
        var addMode = queryId == 0;
        if (readOnly)
            return readOnly == "True" || addMode;
        else
            return true;
    }

}


