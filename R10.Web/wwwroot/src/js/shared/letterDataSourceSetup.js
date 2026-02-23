import SearchEntryPage from "../searchEntryPage";

export default class LetterDataSourceSetup extends SearchEntryPage {

    constructor() {
        super();
        this.detailActiveTabId = "";
        this.recordStampsUrl = "";
        this.previewContainer = null;
        this.previewGridId = "";

        this.initializeEditableGrids = this.initializeEditableGrids.bind(this);
        this.loadTabContent = this.loadTabContent.bind(this);
        this.updateRecordStamps = this.updateRecordStamps.bind(this);
        //this.removeDataSourceNode = this.removeDataSourceNode.bind(this);
        //this.generateLetter = this.generateLetter.bind(this);
    }

    initializeDetailPage(detailPage) {

        this.detailContainer = $(`#${detailPage.detailContainer}`);
        this.detailContentContainer = $(this.detailContainer).find(".cpiDataContainer");
        this.recordStampsUrl = detailPage.recordStampsUrl;

        const self = this;
        //const mainControlButtons = $(self.detailContainer).find(".cpiButtonsDetail");
        //const saveCancelButtons = $(self.detailContainer).find("#editActionButtons");                          // actionButtons
        //const isReadOnly = detailPage.isReadOnly;

        let entryForm = self.detailContainer.find("form")[0];
        //let addMode = detailPage.addMode;
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
        //if (isReadOnly) {
        //    $("#divDataSourceGrid").addClass("masked-area");
        //    $("#divDataSourceGrid").attr("readonly", true);
        //}
        //else {
        //    $("#divDataSourceGrid").removeClass("masked-area");
        //    $("#divDataSourceTree").attr("readonly", false);
        //}

        //// main buttons
        if (self.detailContainer.length > 0) {
            self.manageDetailPageButtons({ detailContainer: self.detailContainer, id: id });
        }

        //// show save/cancel buttons, hide other buttons
        //const setToSaveMode = function () {
        //    saveCancelButtons.removeClass("d-none");
        //    mainControlButtons.hide();
        //};

        //const setToViewMode = function () {
        //    saveCancelButtons.addClass("d-none");
        //    mainControlButtons.show();
        //    cpiBreadCrumbs.markLastNode({ dirty: false });
        //    self.refreshNodeDirtyFlag();                           // n/a? - refreshes grid
        //};

        //const markDirty = function () {
        //    if (self.isParentDirty)
        //        return;
        //    self.isParentDirty = true;
        //    cpiBreadCrumbs.markLastNode({ dirty: true });
        //    self.detailContentContainer.addClass("dirty");
        //    setToSaveMode();
        //};

        //// attach markDirty to input fields
        //entryForm.on("input", ".cpiMainEntry input, .cpiMainEntry textarea", function () {
        //    markDirty();
        //});

        //entryForm.find(".cpiMainEntry .k-combobox > input").each(function () {
        //    const comboBox = $(this).data("kendoComboBox");
        //    if (comboBox) {
        //        comboBox.bind("change", function () {
        //            markDirty();
        //        });
        //    }
        //});

        //entryForm.on("click", "#SignatureQESetupId_letterDetail_cpiButtonLink", function () {
        //    let url = $(this).data("url");
        //    const qeSetupId = $("#SignatureQESetupId_letterDetail").data("kendoComboBox").value();
        //    url = url.replace("actualValue", qeSetupId);
        //    window.open(url, "_blank");
        //});

        //const submitForm = function () {
        //    cpiLoadingSpinner.show();

        //    const json = pageHelper.formDataToJson(entryForm);
        //    pageHelper.postJson(entryForm.attr("action"), json)
        //        .done(function (result) {
        //            cpiLoadingSpinner.hide();
        //            pageHelper.showSuccess(entryForm.data("save-message"));

        //            self.isParentDirty = false;

        //            setToViewMode();
        //            self.detailContentContainer.removeClass("dirty");

        //            // refresh search grid client side (w/o resetting everything)
        //            self.selectSearchResultRow = false;
        //            if (addMode) {
        //                const data = { LetId: result.LetId, LetName: result.LetName };
        //                self.insertSearchGridRow(self.searchResultGridId, data);
        //                addMode = false;
        //            } else {
        //                const grid = $(self.searchResultGridId).data("kendoGrid");              ///////////////////// ------------- FACTOR OUT - similar logic in dataQuery.js
        //                const select = grid.select();
        //                const data = grid.dataItem(select);
        //                data.set("LetName", result.LetName);
        //                $(self.lastGridSelection).addClass("k-state-selected");
        //            }

        //            self.updateRecordStamps()
        //            pageHelper.resetComboBoxes(self.searchForm);        // refresh search buttons to reflect saved data

        //        })
        //        .fail(function (e) {
        //            cpiLoadingSpinner.hide();
        //            pageHelper.showErrors(e);

        //            self.refreshSearchGrid();
        //        });
        //};

        //entryForm.on("submit", function (e) {
        //    e.preventDefault();
        //    //client side validation (using jquery validation)
        //    if (entryForm.valid()) {
        //        submitForm();
        //    }
        //    else {
        //        cpiLoadingSpinner.hide();
        //        entryForm.wasValidated();
        //    }
        //});

        //template manager button click
        //const templateButton = $("#openTemplateManager");
        //if (templateButton) {
        //    templateButton.on("click", (e) => {
        //        const url = templateButton.data("url");
        //        $.ajax({
        //            url: url,
        //            data: { sys: self.systemType },
        //            success: function (result) {
        //                //const popupContainer = $(".cpiContainerPopup").last();
        //                const popupContainer = $(".site-content .popup");
        //                popupContainer.empty();
        //                popupContainer.html(result);
        //            },
        //            error: function (e) {
        //                pageHelper.showErrors(e);
        //            }
        //        });

        //    });
        //}

        //const templateComboBox = this.detailContainer.find("#TemplateFile_letterDetail").data("kendoComboBox");
        //if (templateComboBox) {
        //    templateComboBox.dataSource.bind("error", (e) => {
        //        console.log("error", e);
        //        if (e.xhr.status == 401) {
        //            const baseUrl = $("body").data("base-url");
        //            const url = `${baseUrl}/Graph/SharePoint`;

        //            sharePointGraphHelper.getGraphToken(url, () => {
        //                const retryMsg = this.detailContainer.find(".edit-template").data("retry-msg");
        //                pageHelper.showErrors(retryMsg);
        //            });
        //        }
        //        else {
        //            pageHelper.showErrors(e.responseText);
        //        }
        //    })
        //}

        //this.detailContainer.find(".edit-template").on("click", function () {
        //    const el = $(this);
        //    const templateFile = el.data("template-file");
        //    const systemType = el.data("system-type");

        //    if (templateFile && systemType) {
        //        const baseUrl = $("body").data("base-url");
        //        const url = `${baseUrl}/Shared/SharePointGraph/LetterTemplateGetEditUrl`;

        //        $.get(url, { sys: systemType, templateFile: templateFile })
        //            .done(function (result) {
        //                const a = document.createElement("a");
        //                document.body.appendChild(a);
        //                a.href = result.editUrl;
        //                a.target = "_blank";
        //                a.click();
        //                setTimeout(() => {
        //                    document.body.removeChild(a);
        //                }, 0);
        //            })
        //            .fail(function (e) {
        //                if (e.status == 401) {
        //                    const baseUrl = $("body").data("base-url");
        //                    const url = `${baseUrl}/Graph/SharePoint`;

        //                    sharePointGraphHelper.getGraphToken(url, () => {
        //                        pageHelper.showErrors(el.data("retry-msg"));
        //                    });
        //                }
        //                else {
        //                    pageHelper.showErrors(e.responseText);
        //                }

        //            });
        //    }
        //});            
    }

    initializeEditableGrids(id, sys) {
        this.editableGrids = [
            { name: 'customFieldGrid', isDirty: false, filter: { parentId: id, sys: sys }, afterSubmit: this.updateRecordStamps }
        ];

        this.configureEditableGrids();
    }

    tabChangeSetListener() {
        const self = this;
        self.tabsLoaded = [];

        // restore previous active tab
        if (self.detailActiveTabId === "") {
            self.loadTabContent("fieldListTab");
        }
        else {
            $(`#${self.detailActiveTabId}`).tab("show");
            self.loadTabContent(self.detailActiveTabId);
        }

        $('#dataSourceTab a').on('click', (e) => {
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
            const grid = gridHandle.data("kendoGrid");
            grid.dataSource.read();
        }

        switch (tab) {
            case "fieldListTab":
                $(document).ready(() => {
                    refreshGrid("#fieldListGrid");
                });
                break;
            case "customFieldsTab":
                $(document).ready(() => {
                    refreshGrid("#customFieldGrid");
                });
                break;

            case "":
                break;
        }
    }

    //------------------------- Field List Export
    initializeFieldList(fieldListPage) {
        const fieldListContainer = $(`#${fieldListPage.gridContainer}`);

        // Export to Excel
        fieldListContainer.find(".export-excel").on("click", function () {
            const grid = $(`#${fieldListPage.gridName}`);
            const url = fieldListContainer.data("export-excel-url");
            const dataSourceId = $("#DataSourceId").val();
            let data = { dataSourceId: dataSourceId };
            if (grid !== undefined) {
                const sort = grid.data("kendoGrid").dataSource.sort();
                if (sort !== undefined)
                    $.extend(data, { sortField: sort[0].field, sortDir: sort[0].dir })
            }

            window.fileUtility.getFileStream(url, data);
        });
    }

    updateRecordStamps() {
        const options = { recordStampsUrl: this.recordStampsUrl, infoContainer: this.detailContainer };
        pageHelper.updateRecordStamps(options);        // needs: recordStampsUrl, infoContainer
    }

    getScreenId() {
        const screenId = $("#ScreenId_dataSourceDetail").data("kendoComboBox").value();
        return { screenId: screenId };
    }

    onFieldNameComboBoxSelect(e) {
        if (e.dataItem) {
            const grid = $("#customFieldGrid").data("kendoGrid");
            const row = $(`#${e.sender.element[0].id}`).closest("tr");
            const dataItem = grid.dataItem(row);
            //console.log(e.dataItem);
            //console.log(1);
            //dataItem.set('FieldName', e.dataItem["ColumnName"]);
            //dataItem.set('ColumnLabel', e.dataItem["ColumnLabel"]);
            dataItem.set('FieldSource', e.dataItem["FieldSource"]);
            dataItem.set('CustomFieldSettingId', e.dataItem["CustomFieldSettingId"]);
        }
    }
}
