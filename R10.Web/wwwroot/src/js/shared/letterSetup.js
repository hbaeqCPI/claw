import SearchEntryPage from "../searchEntryPage";
//import FileUtility from "./fileUtility";

// for new letter version; will replace letterPage.js
export default class LetterSetup extends SearchEntryPage {

    constructor() {
        super();
        this.detailActiveTabId = "";
        this.recordStampsUrl = "";
        this.previewContainer = null;
        this.previewGridId = "";
        this.previewSelection = "";

        this.initializeEditableGrids = this.initializeEditableGrids.bind(this);
        this.loadTabContent = this.loadTabContent.bind(this);
        this.updateRecordStamps = this.updateRecordStamps.bind(this);
        this.removeDataSourceNode = this.removeDataSourceNode.bind(this);
        this.generateLetter = this.generateLetter.bind(this);
    }


    initializeDetailPage(detailPage) {

        this.detailContainer = $(`#${detailPage.detailContainer}`);
        this.detailContentContainer = $(this.detailContainer).find(".cpiDataContainer");
        this.recordStampsUrl = detailPage.recordStampsUrl;

        const self = this;
        const mainControlButtons = $(self.detailContainer).find(".cpiButtonsDetail");
        const saveCancelButtons = $(self.detailContainer).find("#editActionButtons");                          // actionButtons
        const isReadOnly = detailPage.isReadOnly;

        let entryForm = self.detailContainer.find("form")[0];
        let addMode = detailPage.addMode;
        const id = detailPage.id;
        const systemType = detailPage.systemType;

        // move search sidebar
        self.moveSideBar(false);
        self.tabChangeSetListener();
        self.initializeEditableGrids(id, systemType);

        // reset dirty flags
        this.isParentDirty = false;
        cpiBreadCrumbs.markLastNode({ dirty: false });

        // attach jquery validator
        $.validator.unobtrusive.parse(entryForm);
        entryForm = $(entryForm);
        entryForm.data("validator").settings.ignore = "";               // include hidden fields (kendo controls)
        pageHelper.addMaxLength(entryForm);

        // mask/disable data source grid/tree if readonly
        if (isReadOnly) {
            $("#divDataSourceGrid").addClass("masked-area");
            $("#divDataSourceGrid").attr("readonly", true);
        }
        else {
            $("#divDataSourceGrid").removeClass("masked-area");
            $("#divDataSourceTree").attr("readonly", false);
        }

        // main buttons
        if (self.detailContainer.length > 0) {
            self.manageDetailPageButtons({ detailContainer: self.detailContainer, id: id });
        }

        // show save/cancel buttons, hide other buttons
        const setToSaveMode = function () {
            saveCancelButtons.removeClass("d-none");
            mainControlButtons.hide();
        };

        const setToViewMode = function () {
            saveCancelButtons.addClass("d-none");
            mainControlButtons.show();
            cpiBreadCrumbs.markLastNode({ dirty: false });
            self.refreshNodeDirtyFlag();                           // n/a? - refreshes grid
        };

        const markDirty = function () {
            if (self.isParentDirty)
                return;
            self.isParentDirty = true;
            cpiBreadCrumbs.markLastNode({ dirty: true });
            self.detailContentContainer.addClass("dirty");
            setToSaveMode();
        };

        // attach markDirty to input fields
        entryForm.on("input", ".cpiMainEntry input, .cpiMainEntry textarea", function () {
            markDirty();
        });

        entryForm.find(".cpiMainEntry .k-combobox > input").each(function () {
            const comboBox = $(this).data("kendoComboBox");
            if (comboBox) {
                comboBox.bind("change", function () {
                    markDirty();
                });
            }
        });

        entryForm.on("click", "#SignatureQESetupId_letterDetail_cpiButtonLink", function () {
            let url = $(this).data("url");
            const qeSetupId = $("#SignatureQESetupId_letterDetail").data("kendoComboBox").value();
            url = url.replace("actualValue", qeSetupId);
            window.open(url, "_blank");
        });

        const submitForm = function () {
            cpiLoadingSpinner.show();

            const json = pageHelper.formDataToJson(entryForm);
            pageHelper.postJson(entryForm.attr("action"), json)
                .done(function (result) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showSuccess(entryForm.data("save-message"));

                    self.isParentDirty = false;

                    setToViewMode();
                    self.detailContentContainer.removeClass("dirty");

                    // refresh search grid client side (w/o resetting everything)
                    self.selectSearchResultRow = false;
                    if (addMode) {
                        const data = { LetId: result.LetId, LetName: result.LetName };
                        self.insertSearchGridRow(self.searchResultGridId, data);
                        addMode = false;
                    } else {
                        const grid = $(self.searchResultGridId).data("kendoGrid");              ///////////////////// ------------- FACTOR OUT - similar logic in dataQuery.js
                        const select = grid.select();
                        const data = grid.dataItem(select);
                        data.set("LetName", result.LetName);
                        $(self.lastGridSelection).addClass("k-state-selected");
                    }

                    self.updateRecordStamps()
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

        //template manager button click
        const templateButton = $("#openTemplateManager");
        if (templateButton) {
            templateButton.on("click", (e) => {
                const url = templateButton.data("url");
                $.ajax({
                    url: url,
                    data: { sys: self.systemType },
                    success: function (result) {
                        //const popupContainer = $(".cpiContainerPopup").last();
                        const popupContainer = $(".site-content .popup");
                        popupContainer.empty();
                        popupContainer.html(result);
                    },
                    error: function (e) {
                        pageHelper.showErrors(e);
                    }
                });

            });
        }

        const templateComboBox = this.detailContainer.find("#TemplateFile_letterDetail").data("kendoComboBox");
        if (templateComboBox) {
            templateComboBox.dataSource.bind("error", (e) => {
                console.log("error", e);
                if (e.xhr.status == 401) {
                    const baseUrl = $("body").data("base-url");
                    const url = `${baseUrl}/Graph/SharePoint`;

                    sharePointGraphHelper.getGraphToken(url, () => {
                        const retryMsg = this.detailContainer.find(".edit-template").data("retry-msg");
                        pageHelper.showErrors(retryMsg);
                    });
                }
                else {
                    pageHelper.showErrors(e.responseText);
                }
            })
        }

        this.detailContainer.find(".edit-template").on("click", function () {
            const el = $(this);
            const templateFile = el.data("template-file");
            const systemType = el.data("system-type");

            if (templateFile && systemType) {
                const baseUrl = $("body").data("base-url");
                const url = `${baseUrl}/Shared/SharePointGraph/LetterTemplateGetEditUrl`;

                $.get(url, { sys: systemType, templateFile: templateFile })
                    .done(function (result) {
                        const a = document.createElement("a");
                        document.body.appendChild(a);
                        a.href = result.editUrl;
                        a.target = "_blank";
                        a.click();
                        setTimeout(() => {
                            document.body.removeChild(a);
                        }, 0);
                    })
                    .fail(function (e) {
                        if (e.status == 401) {
                            const baseUrl = $("body").data("base-url");
                            const url = `${baseUrl}/Graph/SharePoint`;

                            sharePointGraphHelper.getGraphToken(url, () => {
                                pageHelper.showErrors(el.data("retry-msg"));
                            });
                        }
                        else {
                            pageHelper.showErrors(e.responseText);
                        }

                    });
            }
        });
    }

    initializeEditableGrids(id, sys) {
        this.editableGrids = [
            { name: 'dataSourceGrid', isDirty: false, filter: { parentId: id, sys: sys }, afterSubmit: this.updateRecordStamps },
            { name: 'letterTagsGrid', isDirty: false, filter: { parentId: id }, afterSubmit: this.updateRecordStamps },
            { name: 'userDataGrid', isDirty: false, filter: { parentId: id, sys: sys }, afterSubmit: this.updateRecordStamps },
            { name: 'letterFilterGrid', isDirty: false, filter: { parentId: id, sys: sys }, afterSubmit: this.updateRecordStamps },
            { name: 'letterFilterUserGrid', isDirty: false, filter: { parentId: id, sys: sys }, afterSubmit: this.updateRecordStamps }
        ];

        this.configureEditableGrids();
    }

    tabChangeSetListener() {
        const self = this;
        self.tabsLoaded = [];

        // restore previous active tab
        if (self.detailActiveTabId === "") {
            self.loadTabContent("dataSourceTab");
        }
        else {
            $(`#${self.detailActiveTabId}`).tab("show");
            self.loadTabContent(self.detailActiveTabId);
        }

        $('#letterTab a').on('click', (e) => {
            e.preventDefault();
            self.detailActiveTabId = e.target.id;

            if (self.tabsLoaded.indexOf(self.detailActiveTabId) === -1) {
                self.tabsLoaded.push(self.detailActiveTabId);
                self.loadTabContent(self.detailActiveTabId);
            }
        });
    }

    loadTabContent(tab) {

        const refreshGrid = function (gridId) {
            const gridHandle = $(gridId);
            //console.log(gridHandle);
            const grid = gridHandle.data("kendoGrid");
            //console.log(grid);
            grid.dataSource.read();
        }

        switch (tab) {
            case "dataSourceTab":
                $(document).ready(() => {
                    refreshGrid("#dataSourceGrid");
                    refreshGrid("#letterFilterGrid");
                });
                break;
            case "letterTagTab":
                $(document).ready(() => {
                    refreshGrid("#letterTagsGrid");
                });
                break;
            case "userDataTab":
                $(document).ready(() => {
                    refreshGrid("#userDataGrid");
                });
                break;
            case "userFilterTab":
                $(document).ready(() => {
                    refreshGrid("#letterFilterUserGrid");
                });
                break;
            case "fieldListTab":
                $(document).ready(() => {
                    refreshGrid("#fieldListGrid");
                });
                break;
            case "previewTab":
                $(document).ready(() => {
                    this.previewLetter();
                });
                break;

            case "":
                break;
        }
    }

    //------------------------- Preview 

    initializePreview(previewPage) {
        this.previewContainer = $(`#${previewPage.previewContainerId}`);
        this.previewGridId = `#${previewPage.previewGridId}`;

        const self = this;

        const refreshButton = this.previewContainer.find("#refreshButton");
        refreshButton.on("click", function (e) {
            self.previewLetter();
        });

        const previewLetterButton = this.previewContainer.find("#previewLetterButton");
        previewLetterButton.on("click", function (e) {
            self.generateLetter(false);
        });

        const generateLetterButton = this.previewContainer.find("#generateLetterButton");
        generateLetterButton.on("click", function (e) {
            self.generateLetter(true);
        });

    }

    // mass letter generation on setup screen; pop-up/main screen generation is in letter.js
    generateLetter(isLog) {

        const letInfo = this.getSelectedLetInfo();
        const title = this.previewContainer.data("gen-title");
        let msg = "";
        if (isLog)
            msg = this.previewContainer.data("gen-log-message");
        else
            msg = this.previewContainer.data("gen-nolog-message");

        msg += "<br>" + "<span style='font-style:italic; font-weight: bold;' class='pt-2 pb-2'>" + letInfo.letName + "</span >";

        cpiConfirm.confirm(title, msg, function () {
            const data = {
                letId: letInfo.letId, isLog: isLog, systemType: letInfo.systemType, letterScreenCode: letInfo.letterScreenCode,
                selectedContacts: [], screenSource: "gensetup", previewSelection: letInfo.previewSelection
                //__RequestVerificationToken: $($(`input[name='${pageHelper.verificationTokenFormData}']`)[0]).val()
            };
            $("#letterFormParams").val(JSON.stringify(data));
            $("#letterForm").submit();

        });
    }

    getSelectedLetInfo() {
        const screenCode = $("#ScreenId_letterDetail").data("kendoComboBox").value();
        return {
            letId: $("#LetId").val(), letName: $("#LetName").val(), templateFile: $("#TemplateFile").val(), systemType: $("#SystemType").val(),
            letterScreenCode: screenCode, previewSelection: $("#previewSelection").val()
        };
    }

    previewLetter() {
        const self = this;
        const url = this.previewContainer.data("preview-url");
        const id = this.previewContainer.data("letter-id");
        const includeGenerated = this.previewContainer.find("#includeGenerated")[0].checked;

        const data = { letId: id, includeGenerated: includeGenerated };
        // 1) get the grid model (url + 'Init'); 2) pass it on to dynamicGrid.createGrid
        cpiLoadingSpinner.show();
        $.ajax({
            url: url + 'Init',
            data: data,
            success: (gridModel) => {
                const modelJson = JSON.parse(gridModel);
                // can create grid row template here to format
                dynamicGrid.createGrid(self.previewGridId, modelJson, url, data, 10, true);
                this.previewSelection = "";
                $("#previewSelection").val(this.previewSelection);

                const el = $(`${self.previewGridId}`);
                const grid = el.data("kendoGrid");
                grid.setOptions({
                    persistSelection: true,
                    change: function (arg) {
                        this.previewSelection = "(" + this.selectedKeyNames().join(", ") + ")";
                        $("#previewSelection").val(this.previewSelection);
                    },
                    dataBound: function (arg) {
                       
                    }
                });

                // disable generate buttons if no data source or has error message
                const noRecordSource = (modelJson[0].NoData !== undefined || modelJson[0].Err_Message !== undefined);
                $("#generateLetterButton").prop('disabled', noRecordSource);
                $("#previewLetterButton").prop('disabled', noRecordSource);

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


    //------------------------- DATA SOURCE grid-tree view drag & drop

    treeDropNode(e, letId) {
        const treeView = $("#dataSourceTree").data("kendoTreeView");
        const sourceItem = treeView.dataItem(e.sourceNode);
        const sourceId = sourceItem.id;

        let destItem = treeView.dataItem(e.destinationNode);
        if (e.dropPosition !== "over") {
            destItem = destItem.parentNode();
        }
        const destId = destItem.id;

        // send update to server
        const data = { letId: letId, dataSourceId: sourceId, parentRecSourceId: destId }
        const url = $("#letterTreeSection").data("tree-update-url");
        $.ajax({
            type: "POST",
            url: url,
            headers: { "RequestVerificationToken": $($(`input[name='${pageHelper.verificationTokenFormData}']`)[0]).val() },
            data: data
        })
            .done(function () {
                cpiLoadingSpinner.hide();
                treeView.append({ id: sourceId, text: sourceDesc }, selectedNode);
            })
            .fail(function (e) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            });

    }

    treeDropGrid(e, letId) {
        // get info about dragged grid row
        const draggableElement = e.draggable.currentTarget;
        const gridRow = $("#dataSourceGrid").getKendoGrid().dataSource.getByUid($(draggableElement).data("uid"));

        if (gridRow === undefined)          // exit if drop is not from grid
            return;

        cpiLoadingSpinner.show();
        const sourceId = gridRow.DataSourceId;
        const sourceDesc = gridRow.DataSourceDescMain;

        // get info about selected tree node on which dragged grid row will be drop
        const treeView = $("#dataSourceTree").data("kendoTreeView");
        const selectedNode = treeView.select();
        const parentSourceId = treeView.dataItem(selectedNode).id;                // used for ajax tree update

        // send update to server
        const data = { letId: letId, dataSourceId: sourceId, parentRecSourceId: parentSourceId }
        const url = $("#letterTreeSection").data("tree-drop-url");
        $.ajax({
            type: "POST",
            url: url,
            headers: { "RequestVerificationToken": $($(`input[name='${pageHelper.verificationTokenFormData}']`)[0]).val() },
            data: data
        })
            .done(function () {
                cpiLoadingSpinner.hide();
                treeView.append({ id: sourceId, text: sourceDesc }, selectedNode);
            })
            .fail(function (e) {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(e);
            });
    }

    removeDataSourceNode(e, letId) {
        const item = e.item.id;
        const node = $(e.target);
        const treeView = $("#dataSourceTree").data("kendoTreeView");
        const recSourceId = treeView.dataItem(node).id;
        if (item === "removeNode" && recSourceId !== "0") {
            const letterContainer = $("#letterTreeSection");
            const url = letterContainer.data("tree-remove-url");
            const data = { letId: letId, recSourceId: recSourceId };
            $.ajax({
                type: "POST",
                url: url,
                headers: { "RequestVerificationToken": $($(`input[name='${pageHelper.verificationTokenFormData}']`)[0]).val() },
                data: data,
                success: function () {
                    treeView.remove(node);
                }
            });
        }
    }

    //------------------------- DATA FILTERS

    // filter for screen combobox on detail container
    getScreenFilter(systemType) {
        return { featureType: 'Let', systemType: systemType };
    }

    getFilterFieldsFilter(gridId) {
        const grid = $(gridId).data("kendoGrid");
        const selectedItem = grid.dataItem(grid.select());
        let recSourceId = selectedItem.RecSourceId;
        if (!recSourceId)
            recSourceId = selectedItem.LetterRecordSource.RecSourceId;

        return { recSourceId: recSourceId };
    }

    getOperandDataFilter(gridId) {                       // actualDataFilter in old letter version
        const grid = $(gridId).data("kendoGrid");
        const selectedItem = grid.dataItem(grid.select());
        //console.log(grid, selectedItem);

        let recSourceId = selectedItem.LetterRecordSource.RecSourceId;
        if (recSourceId === 0)
            recSourceId = selectedItem.RecSourceId;

        return {
            recSourceId: recSourceId,
            fieldName: selectedItem.FieldName
        };
    }

    valueMapperFilter(options) {
        letterSetup.valueMapperFilterData(options, "#letterFilterGrid");

    }
    valueMapperFilterUser(options) {
        letterSetup.valueMapperFilterData(options, "#letterFilterUserGrid");
    }

    valueMapperFilterData(options, gridId) {                // virtualization of filter data picklist
        const grid = $(gridId).data("kendoGrid");
        const selectedItem = grid.dataItem(grid.select());
        let recSourceId = selectedItem.RecSourceId;
        if (!recSourceId)
            recSourceId = selectedItem.LetterRecordSource.RecSourceId;

        const params = {
            recSourceId: recSourceId,
            fieldName: selectedItem.FieldName,
            value: options.value
        };
        const url = $(`${gridId}Container`).data("filter-mapper-url");

        $.ajax({
            url: url,
            data: params,
            success: function (data) {
                options.success(data);
            }
        });
    }

    //------------------------- Letter Template
    refreshTemplateGrid(e) {
        const grid = $("#fileTemplateGrid").data("kendoGrid");
        grid.dataSource.read();
        pageHelper.showSuccess(e.response.success);
    }

    uploadError(e) {
        cpiAlert.warning(e.XMLHttpRequest.responseText);
    }

    getUploadFilter(sys) {
        const overwriteExisting = $("#overwriteExisting")[0].checked;
        return { sys: sys, overwriteExisting: overwriteExisting };
    }

    deleteTemplate(e, grid) {
        e.preventDefault();
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        pageHelper.deleteGridRow(e, dataItem);
    }

    downloadTemplate(e, grid) {
        e.preventDefault();
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        const container = $("#fileTemplateGridContainer");
        const url = container.data("download-url");
        //console.log(dataItem);
        const data = { sys: dataItem.SystemType, templateFile: dataItem.TemplateFile };
        window.fileUtility.getFileStream(url, data);
    }

    //------------------------- Field List Export
    initializeFieldList(fieldListPage) {
        const fieldListContainer = $(`#${fieldListPage.gridContainer}`);

        // Export to Excel
        fieldListContainer.find(".export-excel").on("click", function () {
            const grid = $(`#${fieldListPage.gridName}`);
            const url = fieldListContainer.data("export-excel-url");
            const letId = $("#LetId").val();
            let data = { letId: letId };
            if (grid !== undefined) {
                const sort = grid.data("kendoGrid").dataSource.sort();
                if (sort !== undefined)
                    $.extend(data, { sortField: sort[0].field, sortDir: sort[0].dir })
            }

            window.fileUtility.getFileStream(url, data);
        });
    }


    //------------------------- Miscellaneous 
    updateRecordStamps() {
        const options = { recordStampsUrl: this.recordStampsUrl, infoContainer: this.detailContainer };
        pageHelper.updateRecordStamps(options);        // needs: recordStampsUrl, infoContainer
    }

    getScreenId() {
        const screenId = $("#ScreenId_letterDetail").data("kendoComboBox").value();
        return { screenId: screenId };
    }
} 