import SearchEntryPage from "../searchEntryPage";
//import FileUtility from "./fileUtility";

// for new docx version; will replace docxPage.js
export default class DOCXSetup extends SearchEntryPage {

    constructor() {
        super();
        this.detailActiveTabId = "";
        this.recordStampsUrl = "";
        this.previewContainer = null;
        this.previewGridId = "";

        this.initializeEditableGrids = this.initializeEditableGrids.bind(this);
        this.loadTabContent = this.loadTabContent.bind(this);
        this.updateRecordStamps = this.updateRecordStamps.bind(this);
        this.removeDataSourceNode = this.removeDataSourceNode.bind(this);
        this.generateDOCX = this.generateDOCX.bind(this);
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
                        const data = { DOCXId: result.DOCXId, DOCXName: result.DOCXName };
                        self.insertSearchGridRow(self.searchResultGridId, data);
                        addMode = false;
                    } else {
                        const grid = $(self.searchResultGridId).data("kendoGrid");              ///////////////////// ------------- FACTOR OUT - similar logic in dataQuery.js
                        const select = grid.select();
                        const data = grid.dataItem(select);
                        data.set("DOCXName", result.DOCXName);
                        $(self.lastGridSelection).addClass("k-state-selected");
                    }

                    self.updateRecordStamps()
                    pageHelper.resetComboBoxes(self.searchForm);        // refresh search buttons to reflect saved data

                })
                .fail(function (e) {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(e);

                    //self.refreshSearchGrid();
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
    }

    initializeEditableGrids(id, sys) {
        this.editableGrids = [
            { name: 'dataSourceGrid', isDirty: false, filter: { parentId: id, sys: sys }, afterSubmit: this.updateRecordStamps },
            { name: 'userDataGrid', isDirty: false, filter: { parentId: id, sys: sys  }, afterSubmit: this.updateRecordStamps },
            { name: 'docxFilterGrid', isDirty: false, filter: { parentId: id, sys: sys  }, afterSubmit: this.updateRecordStamps },
            { name: 'docxFilterUserGrid', isDirty: false, filter: { parentId: id, sys: sys  }, afterSubmit: this.updateRecordStamps }
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

        $('#docxTab a').on('click', (e) => {
            e.preventDefault();
            self.detailActiveTabId = e.target.id;

            if (self.tabsLoaded.indexOf(self.detailActiveTabId) === -1) {
                self.tabsLoaded.push(self.detailActiveTabId);
                self.loadTabContent(self.detailActiveTabId);
            }
        });
    }

    loadTabContent(tab) {

        const refreshGrid = function(gridId) {
            const gridHandle = $(gridId);
            const grid = gridHandle.data("kendoGrid");
            grid.dataSource.read();
        }

        switch (tab) {
            case "dataSourceTab":
                $(document).ready(() => {
                    refreshGrid("#dataSourceGrid");
                    refreshGrid("#docxFilterGrid");
                });
                break;
            case "userDataTab":
                $(document).ready(() => {
                    refreshGrid("#userDataGrid");
                });
                break;
            case "userFilterTab":
                $(document).ready(() => {
                    refreshGrid("#docxFilterUserGrid");
                });
                break;
            case "fieldListTab":
                $(document).ready(() => {
                    refreshGrid("#fieldListGrid");
                });
                break;
            case "usptoHeaderKeywordListTab":
                $(document).ready(() => {
                    refreshGrid("#usptoHeaderKeywordGrid");
                });
                break;
            case "previewTab":
                $(document).ready(() => {
                    this.previewDOCX();
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
            self.previewDOCX();
        });

        const previewDOCXButton = this.previewContainer.find("#previewDOCXButton");
        previewDOCXButton.on("click", function (e) {
            self.generateDOCX(false);
        });

        const generateDOCXButton = this.previewContainer.find("#generateDOCXButton");
        generateDOCXButton.on("click", function (e) {
            self.generateDOCX(true);
        });

    }

    // mass docx generation on setup screen; pop-up/main screen generation is in docx.js
    generateDOCX(isLog) {
        
        const docxInfo = this.getSelectedDOCXInfo();
        const title = this.previewContainer.data("gen-title");
        let msg = "";
        if (isLog)
            msg = this.previewContainer.data("gen-log-message");
        else
            msg = this.previewContainer.data("gen-nolog-message");

        msg += "<br>" + "<span style='font-style:italic; font-weight: bold;' class='pt-2 pb-2'>" + docxInfo.docxName + "</span >";
        cpiConfirm.confirm(title, msg, function () {
            const data = {
                docxId: docxInfo.docxId, isLog: isLog, systemType: docxInfo.systemType, docxScreenCode: docxInfo.docxScreenCode, screenSource: "gensetup", docDesc: docxInfo.docxName
                //selectedContacts: [], 
                //__RequestVerificationToken: $($(`input[name='${pageHelper.verificationTokenFormData}']`)[0]).val()
            };
            $("#docxFormParams").val(JSON.stringify(data));
            $("#docxForm").submit();

        });
    }

    getSelectedDOCXInfo() {
        const screenCode = $("#ScreenId_docxDetail").data("kendoComboBox").value();
        return {
            docxId: $("#DOCXId").val(), docxName: $("#DOCXName").val(), templateFile: $("#TemplateFile").val(), systemType: $("#SystemType").val(),
            docxScreenCode: screenCode
        };
    }

    previewDOCX() {
        const self = this;
        const url = this.previewContainer.data("preview-url");
        const id = this.previewContainer.data("docx-id");
        const includeGenerated = this.previewContainer.find("#includeGenerated")[0].checked;

        const data = { docxId: id, includeGenerated: includeGenerated };
        // 1) get the grid model (url + 'Init'); 2) pass it on to dynamicGrid.createGrid
        cpiLoadingSpinner.show();
        $.ajax({
            url: url + 'Init',
            data: data,
            success: (gridModel) => {
                const modelJson = JSON.parse(gridModel);
                // can create grid row template here to format
                dynamicGrid.createGrid(self.previewGridId, modelJson, url, data, 10);

                // disable generate buttons if no data source or has error message
                const noRecordSource = (modelJson[0].NoData !== undefined || modelJson[0].Err_Message !== undefined);
                $("#generateDOCXButton").prop('disabled', noRecordSource);
                $("#previewDOCXButton").prop('disabled', noRecordSource);

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
   
    treeDropNode(e, docxId) {
        const treeView = $("#dataSourceTree").data("kendoTreeView");
        const sourceItem = treeView.dataItem(e.sourceNode);
        const sourceId = sourceItem.id;

        let destItem = treeView.dataItem(e.destinationNode);
        if (e.dropPosition !== "over") {
            destItem = destItem.parentNode();
        }
        const destId = destItem.id;

        // send update to server
        const data = { docxId: docxId, dataSourceId: sourceId, parentRecSourceId: destId }
        const url = $("#docxTreeSection").data("tree-update-url");
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

    treeDropGrid(e, docxId) {
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
        const data = { docxId: docxId, dataSourceId: sourceId, parentRecSourceId: parentSourceId }
        const url = $("#docxTreeSection").data("tree-drop-url");
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

    removeDataSourceNode(e, docxId) {
        const item = e.item.id;
        const node = $(e.target);
        const treeView = $("#dataSourceTree").data("kendoTreeView");
        const recSourceId = treeView.dataItem(node).id;
        if (item === "removeNode" && recSourceId !== "0") {
            const docxContainer = $("#docxTreeSection");
            const url = docxContainer.data("tree-remove-url");
            const data = { docxId: docxId, recSourceId: recSourceId };
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
        return { featureType: 'DOCX', systemType: systemType };
    }

    onChange_ScreenName = (e) => {
        const DOCXCatId = this.detailContentContainer.find("input[name='DOCXCatId']");
        DOCXCatId.data("fetched", 0);
    }

    getScreenName = () => {
        const screenId = this.detailContentContainer.find("input[name='ScreenId']");
        return { screenId: screenId.val() };
    }

    getFilterFieldsFilter(gridId) {
        const grid = $(gridId).data("kendoGrid");
        const selectedItem = grid.dataItem(grid.select());
        let recSourceId = selectedItem.RecSourceId;
        if (!recSourceId)
            recSourceId = selectedItem.DOCXRecordSource.RecSourceId;

        return { recSourceId: recSourceId };
    }

    getOperandDataFilter(gridId) {                       // actualDataFilter in old docx version
        const grid = $(gridId).data("kendoGrid");
        const selectedItem = grid.dataItem(grid.select());
        //console.log(grid, selectedItem);

        let recSourceId = selectedItem.DOCXRecordSource.RecSourceId;
        if (recSourceId === 0)
            recSourceId = selectedItem.RecSourceId;

        return {
            recSourceId: recSourceId,
            fieldName: selectedItem.FieldName
        };
    }
        
    valueMapperFilter(options) {
        docxSetup.valueMapperFilterData(options,"#docxFilterGrid");

    }
    valueMapperFilterUser(options) {
        docxSetup.valueMapperFilterData(options, "#docxFilterUserGrid");
    }

    valueMapperFilterData(options, gridId) {                // virtualization of filter data picklist
        const grid = $(gridId).data("kendoGrid");
        const selectedItem = grid.dataItem(grid.select());
        let recSourceId = selectedItem.RecSourceId;
        if (!recSourceId)
            recSourceId = selectedItem.DOCXRecordSource.RecSourceId;
      
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

    //------------------------- DOCX Template
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
            const docxId = $("#DOCXId").val();
            let data = { docxId: docxId };
            if (grid !== undefined) {
                const sort = grid.data("kendoGrid").dataSource.sort();
                if (sort !== undefined)
                    $.extend(data, { sortField: sort[0].field, sortDir: sort[0].dir })
            }

            window.fileUtility.getFileStream(url, data);
        });
    }

    initializeDOCXHeaderKeyword(headerKeywordPage) {
        const headerKeywordContainer = $(`#${headerKeywordPage.gridContainer}`);

        // Export to Excel
        headerKeywordContainer.find(".export-excel").on("click", function () {
            const grid = $(`#${headerKeywordPage.gridName}`);
            const url = headerKeywordContainer.data("export-excel-url");
            const docxId = $("#DOCXId").val();
            let data = { docxId: docxId };
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


} 