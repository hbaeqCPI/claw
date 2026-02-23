export default class PatSearchPage {

    constructor() {
        this.resultGrid = null;
        this.basicCriteria = null;
        this.advancedCriteria = null;
        this.dateFields = [];
        this.searchMode = "b";
    }

    initialize = () => {
        const self = this;
        const screenName = "patentSearch";

        $(document).ready(() => {
            const resultGrid = $("#patentSearchResults-Grid");
            resultGrid.addClass("d-none");

            this.resultGrid = resultGrid.data("kendoListView");
            $("#patentSearchResults-Grid_pager").addClass("d-none");

            resultGrid.on("change", "input[type='checkbox']", function () {
                self.pageSelection(this, this.checked);
            });
            $("#patentSearchSelectAll").on("change", "input[type='checkbox']", function () {
                const pageSelection = this.checked;
                resultGrid.find("input[type='checkbox']").each(function () {
                    self.pageSelection(this, pageSelection);
                    $(this).prop("checked", pageSelection);
                });
            });
            $('#patentSearchContainer').on("click", ".excel-export", () => {
                this.exportToExcel();
            });

            $('#patentSearchContainer').on("click", ".add-pw", () => {
                this.addToPatentWatch();
            });

            $("#patent-search-mode input").on("change", function () {
                self.searchMode = $(this).val();
                self.updateSearchModeDisplay();
            });

            const criteriaGrid = $("#gridPatentSearchFilter");
            criteriaGrid.on("click", ".k-grid-LoadAll", function (e) {
                e.preventDefault();
                const container = $(this).closest("div.patent-search-advanced");
                const loadPrompt = container.data("load-prompt");
                const title = container.data("load-title");
                cpiConfirm.warning(title, loadPrompt, function () {
                    const criteriaGridData = criteriaGrid.data("kendoGrid");
                    criteriaGridData.dataSource.read();
                })

            });

            criteriaGrid.on("click", ".k-grid-RemoveFilter", function (e) {
                e.preventDefault();
                const container = $(this).closest("div.patent-search-advanced");
                const deletePrompt = container.data("removefilters-prompt");
                const title = container.data("delete-title");
                cpiConfirm.delete(title, deletePrompt, function () {
                    const criteriaGridData = criteriaGrid.data("kendoGrid");
                    criteriaGridData.dataSource.data([]);
                })
            });

            criteriaGrid.on("click", ".criteria-field", (e) => {
                e.stopPropagation();
                const row = $(e.target).closest("tr");
                const dataItem = criteriaGrid.data("kendoGrid").dataItem(row);
                if (dataItem.Field.FieldName.endsWith("Date"))
                    this.showDateEntryScreen(dataItem);
            });

            const basicContainer = $(".patent-search-basic");
            basicContainer.find("#pat-search-str").on("keydown", (e) => {
                const keyCode = e.keyCode || e.which;
                if (keyCode === 13) {
                    this.runQuery();
                }
            });
            basicContainer.find(".pat-search-submit").on("click", () => {
                $("#pat-search-adv-type").val("1");
                this.runQuery();
            });
            basicContainer.find(".pat-search-setting").on("click", () => {
                this.showSettingScreen();
            });

            const advancedContainer = $(".patent-search-advanced");
            advancedContainer.find(".pat-search-submit").on("click", () => {
                this.runQuery();
            });
            advancedContainer.find(".pat-search-build-sql").on("click", () => {
                this.showSQL();
            });
            advancedContainer.find(".pat-search-qry-str-refresh").on("click", () => {
                this.refreshSQL();
            });
            //advancedContainer.find(".pat-search-qry-str-submit").on("click", () => {
            //   // $("#pat-search-adv-type").val("2");
            //    this.runQuery();
            //});

            $("#patentSearchScheduledResults").on("click", ".prev-result-view", function () {
                self.currentSearchId = $(this).data("id");
                self.runQuery();
            });

            const baseUrl = $("body").data("base-url");
            const criteriaSaveUrl = `${baseUrl}/Patent/PatentSearch/SaveCriteriaScreen`;

            //save/load/clear criteria
            $(".pat-search-save-filters").on("click", function (e) {
                pageHelper.getSearchCriteriaScreen("", screenName, true, null, function () { return self.formDataToCriteriaList(); }, criteriaSaveUrl);
            });
            $(".pat-search-load-filters").on("click", function (e) {
                pageHelper.getSearchCriteriaScreen("", screenName, false, function (response) { return self.loadCriteriaToScreen(response); }, null, criteriaSaveUrl);
            });
            $(".pat-search-search-clear").on("click", function (e) {
                $("#gridPatentSearchFilter").data("kendoGrid").dataSource.data([]);
                $("#patent-search-mode input[value='b']").click();
                $("#pat-search-str").val("");
                $("input[name='basicSearchMode'][value='exact']").click();
            });
            pageHelper.loadDefaultSearchCriteria("", screenName, null, function (response) { self.loadCriteriaToScreen(response); });
        });
    }

    updateSearchModeDisplay = () => {

        if (this.searchMode === "a") {
            $(".patent-search-advanced").removeClass("d-none");
            $(".patent-search-basic").addClass("d-none");
            $(".patent-search-prev-results").addClass("d-none");
            $("#patent-search-save-criteria").removeClass("d-none");

        }
        else if (this.searchMode === "b") {
            $(".patent-search-basic").removeClass("d-none");
            $(".patent-search-advanced").addClass("d-none");
            $(".patent-search-prev-results").addClass("d-none");
            $("#patent-search-save-criteria").removeClass("d-none");
        }
        //result 
        else {
            $(".patent-search-basic").addClass("d-none");
            $(".patent-search-advanced").addClass("d-none");
            $(".patent-search-prev-results").removeClass("d-none");
            $("#patent-search-save-criteria").addClass("d-none");

            const prevResultsGrid = $("#patentSearchScheduledResults").data("kendoGrid");
            prevResultsGrid.dataSource.read();
        }
    }

    // for saving criteria
    formDataToCriteriaList = () => {
        const filters = [];

        // basic search criteria
        filters.push({ property: "SearchModeOption", operator: "", value: this.searchMode });

        if (this.searchMode === "b") {
            filters.push({ property: "BasicSearchTerm", operator: "", value: $("#pat-search-str").val() });
            filters.push({ property: "BasicSearchMode", operator: "", value: $("input[name='basicSearchMode']:checked").val() });
        }
        else {
            const advancedCriteria = this.getAdvancedCriteria();
            filters.push({ property: "gridDataFilter", operator: "", value: advancedCriteria });
            filters.push({ property: "advancedSQLStr", operator: "", value: $("#pat-search-qry-str").val() });
        }
        return filters;
    }

    loadCriteriaToScreen = (response) => {
        const criteria = JSON.parse(response);

        if (criteria.length > 0) {

            const keyValues = {};
            for (const item of criteria) {
                keyValues[item.property] = item.value;
            }

            this.searchMode = keyValues.SearchModeOption;

            const searchModeInputs = $("#patent-search-mode input");
            const searchModeLabels = $("label.patent-search-mode");
            searchModeInputs.prop("checked", false);
            searchModeLabels.removeClass("active");
            if (this.searchMode === "b") {
                $(searchModeLabels[0]).addClass("active");
                $(searchModeInputs[0]).prop("checked", true);
            }
            else {
                $(searchModeLabels[1]).addClass("active");
                $(searchModeInputs[1]).prop("checked", true);
            }
            this.updateSearchModeDisplay();

            if (keyValues.SearchModeOption === "b") {
                $("#pat-search-str").val(keyValues.BasicSearchTerm);
                $(`#basic-${keyValues.BasicSearchMode}`).prop("checked", true);
            }
            else {
                const advancedCriteriaGrid = $("#gridPatentSearchFilter").data("kendoGrid");
                advancedCriteriaGrid.dataSource.data(keyValues.gridDataFilter);

                $("#pat-search-qry-str").val(keyValues.advancedSQLStr);
                if (keyValues.advancedSQLStr > "") {
                    $(".qry-str").removeClass("d-none");
                }
            }
        }
    }

    pageSelection = (checkbox, selection) => {
        const data = this.resultGrid.dataSource.data();
        const id = $(checkbox).data("id");
        if (id) {
            const item = data.find(e => e.AppId == id); //intentional, different data type
            if (item) {
                item.Selected = selection
            }
        }
    }

    runQuery = () => {
        const active = $("#patent-search-mode").find(".btn.active input")[0].value;
        let proceed = false;

        //basic
        if (active === "b") {
            const searchStr = $("#pat-search-str").val();
            if (searchStr)
                proceed = true;
        }
        //advanced
        else if (active === "a") {
            const criteria = this.getAdvancedCriteria(true);
            if (criteria.length > 0)
                proceed = true;
        }
        //previous results
        else {
            proceed = true;
        }

        if (proceed) {
            const dataSource = $('#patentSearchResults-Grid').data('kendoListView').dataSource;
            cpiLoadingSpinner.show();
            $("#patentSearchResults-Grid").addClass("d-none");
            $("#patentSearchResults-Grid_pager").addClass("d-none");

            const exportToExcelContainer = $("#patentSearchSelectAll");
            exportToExcelContainer.addClass("d-none");
            exportToExcelContainer.find("input[type='checkbox']").prop("checked", false);

            dataSource.read().then(() => {
                cpiLoadingSpinner.hide();
                const grid = $('#patentSearchResults-Grid').data('kendoListView');
                if (grid.dataSource.page() !== 1)
                    grid.dataSource.page(1);

                $("#patentSearchResults-Grid").removeClass("d-none");
                $("#patentSearchResults-Grid_pager").removeClass("d-none");
                $("#patentSearchSelectAll").removeClass("d-none");
            });
        }
    }

    searchSubmit = () => {
        const active = $("#patent-search-mode").find(".btn.active input")[0].value;

        //basic
        if (active === "b") {
            const searchStr = $("#pat-search-str").val();
            const basicSearchMode = $(".patent-search-basic").find("input[name='basicSearchMode']:checked").val();
            this.basicCriteria = { searchString: searchStr, searchMode: basicSearchMode };
            return {
                mode: "basic",
                basicCriteria: this.basicCriteria
            };
        }

        //advanced
        else if (active === "a") {
            const mode = "advanced";
            if ($("#pat-search-qry-str").val() > "") {
                return {
                    mode: mode,
                    advancedSQLCriteria: $("#pat-search-qry-str").val()
                };
            }
            else {
                this.advancedCriteria = this.getAdvancedCriteria(true);
                return {
                    mode: mode,
                    advancedCriteria: this.advancedCriteria
                };
            }
        }
        else {
            return {
                mode: "previous",
                searchId: this.currentSearchId
            };
        }
    }

    getAdvancedCriteria = (forSearch) => {
        const records = [];
        const grid = $("#gridPatentSearchFilter").data("kendoGrid");
        if (grid && grid.dataSource) {
            const data = grid.dataSource.data();
            if (data.length) {
                for (let i = 0; i < data.length; i++) {
                    if (data[i].Criteria.length) {
                        if (!data[i].SearchModeType)
                            data[i].SearchModeType = "any";
                        if (!data[i].QueryType)
                            data[i].QueryType = "simple";

                        if (data[i].Field.FieldId && data[i].Criteria) {
                            if (forSearch)
                                records.push({ FieldId: data[i].Field.FieldId, Criteria: data[i].Criteria, SearchModeType: data[i].SearchModeType.toLowerCase(), QueryType: data[i].QueryType.toLowerCase() });
                            else
                                records.push({ Field: data[i].Field, Criteria: data[i].Criteria, SearchModeType: data[i].SearchModeType.toLowerCase(), QueryType: data[i].QueryType.toLowerCase() });
                        }
                    }
                }
            }
        }
        return records;
    }

    showSQL = () => {
        const container = $(".qry-str");
        if (container.hasClass("d-none")) {
            container.removeClass("d-none");
            this.refreshSQL();
        }
    }

    refreshSQL = () => {
        const baseUrl = $("body").data("base-url");
        let url = `${baseUrl}/Patent/PatentSearch/BuildAdvancedSearchCriteria`;

        const criteria = this.getAdvancedCriteria(true);
        if (criteria.length > 0) {
            $.post(url, { advancedCriteria: criteria }).done((result) => {
                if (result) {
                    $("#pat-search-qry-str").val(result);
                }
            }).fail(function (error) {
                pageHelper.showErrors(error.responseText);
            });
        }
    }

    exportToExcel = () => {
        const selected = this.resultGrid.dataSource.data().filter(r => r.Selected).map(s => s.AppId);
        let criteria = null;
        let mode = "";

        if (selected.length > 0) {
            const active = $("#patent-search-mode").find(".btn.active input")[0].value;

            //basic
            if (active === "b") {
                criteria = this.basicCriteria;
                mode = "basic";
            }

            //advanced
            else if (active === "a") {
                const advancedSQLStr = $("#pat-search-qry-str").val();
                if (advancedSQLStr > '') {
                    criteria = {
                        advancedSQLStr: advancedSQLStr
                    };
                }
                else {
                    criteria = {
                        criteria: this.advancedCriteria,
                    };
                }
                mode = "advanced";
            }
            else {
                criteria = { searchId: this.currentSearchId };
                mode = "previous";
            }

            criteria.appIds = selected;
            const form = $("#patSearchExportToExcel");
            form.find("#mode").val(mode);
            form.find("#exportCriteria").val(JSON.stringify(criteria));
            form.submit();
        }
    }

    showSettingScreen() {
        const baseUrl = $("body").data("base-url");
        let url = `${baseUrl}/Patent/PatentSearch/Settings`;

        $.get(url).done((result) => {
            if (result) {
                const popupContainer = $(".site-content .popup").last();
                popupContainer.html(result);
                const dialog = $("#patentSearchSettingsDialog");
                dialog.modal("show");
                dialog.floatLabels();

                let entryForm = dialog.find("form")[0];
                entryForm = $(entryForm);
                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialog
                    }
                );
            }

        }).fail(function (error) {
            pageHelper.showErrors(error.responseText);
        });
    }

    showDateEntryScreen(dataItem) {
        const baseUrl = $("body").data("base-url");
        let url = `${baseUrl}/Patent/PatentSearch/DateEntry`;

        const param = { FieldLabel: dataItem.Field.FieldLabel };
        const i = this.dateFields.findIndex(e => e.fieldName === dataItem.Field.FieldName);
        if (i > -1) {
            param.Operator = this.dateFields[i].operator;
            param.FromDate = pageHelper.cpiDateFormatToSave(this.dateFields[i].fromDate);
            param.ToDate = pageHelper.cpiDateFormatToSave(this.dateFields[i].toDate);
        }

        $.get(url, param).done((result) => {
            if (result) {
                const popupContainer = $(".site-content .popup").last();
                popupContainer.html(result);
                const dialog = $("#patentSearchDateEntryDialog");
                dialog.modal("show");
                dialog.floatLabels();

                let entryForm = dialog.find("form")[0];
                entryForm = $(entryForm);
                entryForm.cpiPopupEntryForm({ dialogContainer: dialog });

                entryForm.on("click", ".save", () => {
                    const operator = entryForm.find("input[name='Operator']").val();
                    const fromDate = entryForm.find("input[name='FromDate']").data("kendoDatePicker").value();
                    const toDate = entryForm.find("input[name='ToDate']").data("kendoDatePicker").value();

                    const rec = {
                        fieldName: dataItem.Field.FieldName,
                        operator: operator,
                        fromDate: fromDate,
                        toDate: toDate
                    };

                    let valid = true;
                    if (operator === "range" || operator === "=" || operator === ">=") {
                        if (fromDate === null) {
                            entryForm.find("input[name='FromDate']").closest("span").addClass("k-is-invalid");
                            valid = false;
                        }
                        else
                            entryForm.find("input[name='FromDate']").closest("span").removeClass("k-is-invalid");
                    }

                    if (operator === "range" || operator === "<=") {
                        if (toDate === null) {
                            entryForm.find("input[name='ToDate']").closest("span").addClass("k-is-invalid");
                            valid = false;
                        }
                        else
                            entryForm.find("input[name='ToDate']").closest("span").removeClass("k-is-invalid");
                    }

                    if (valid && (operator === "range" && fromDate > toDate)) {
                        entryForm.find("input[name='ToDate']").closest("span").addClass("k-is-invalid");
                        valid = false;
                    }

                    if (valid) {
                        const i = this.dateFields.findIndex(e => e.fieldName === rec.fieldName);
                        if (i > -1)
                            this.dateFields[i] = rec;
                        else
                            this.dateFields.push(rec);

                        const criteria = this.computeDateCriteria(rec);
                        dataItem.Criteria = criteria;
                        const grid = $("#gridPatentSearchFilter").data("kendoGrid");
                        grid.refresh();
                        dialog.modal("hide");
                    }
                });
            }

        }).fail(function (error) {
            pageHelper.showErrors(error.responseText);
        });
    }

    computeDateCriteria(rec) {
        let criteria = "";
        if (rec.operator === "range") {
            criteria = `${rec.fieldName} ge ~${window.kendo.toString(rec.fromDate, "yyyyMMdd")}~ and ${rec.fieldName} le ~${window.kendo.toString(rec.toDate, "yyyyMMdd")}~`;
        }
        else if (rec.operator === "=") {
            criteria = `${rec.fieldName} eq ~${window.kendo.toString(rec.fromDate, "yyyyMMdd")}~`;
            if (rec.toDate)
                criteria = `${criteria} or ${rec.fieldName} eq ~${window.kendo.toString(rec.toDate, "yyyyMMdd")}~`;
        }
        else if (rec.operator === ">=") {
            criteria = `${rec.fieldName} ge ~${window.kendo.toString(rec.fromDate, "yyyyMMdd")}~`;
        }
        else if (rec.operator === "<=") {
            criteria = `${rec.fieldName} le ~${window.kendo.toString(rec.toDate, "yyyyMMdd")}~`;
        }
        return criteria;
    }

    operatorChange(e) {
        const value = e.sender.value();
        const entryForm = $("#patentSearchDateEntryDialog").find("form")[0];

        if (value === ">=")
            $(entryForm).find("input[name='ToDate']").data("kendoDatePicker").value(null);
        else if (value === "<=")
            $(entryForm).find("input[name='FromDate']").data("kendoDatePicker").value(null);
    }

    deleteFilterRow(e) {
        e.preventDefault();

        const grid = $("#" + e.delegateTarget.id).data("kendoGrid");
        const form = $(e.currentTarget).closest("form");
        const deletePrompt = grid.options.editable.confirmDelete;
        const title = form.data("delete-title");
        cpiConfirm.delete(title, deletePrompt, function () {
            grid.removeRow($(e.currentTarget).closest("tr"));
            grid.dataSource._destroyed = [];
        })
    }

    addToPatentWatch = () => {
        const selected = this.resultGrid.dataSource.data().filter(r => r.Selected).map(s => {
            return {
                AppId: s.AppId,
                Term: s.Country + s.AppnoOrig
            }
        });

        if (selected.length > 0) {
            const container = $('#patentSearchContainer');
            const title = container.find(".add-pw").data("confirm-title");
            const msg = container.find(".add-pw").data("confirm-msg");

            cpiConfirm.confirm(title, msg, function () {
                const baseUrl = $("body").data("base-url");
                let url = `${baseUrl}/Patent/PatentSearch/AddToPatentWatch`;

                cpiLoadingSpinner.show();
                $.post(url, { numbers: selected }).done((response) => {
                    pageHelper.showSuccess(response.success);
                    cpiLoadingSpinner.hide();
                }).fail(function (error) {
                    pageHelper.showErrors(error.responseText);
                    cpiLoadingSpinner.hide();
                });
            })
        }

    }

    onSearchCriteriaNameChange = (e) => {
        const comboBox = e.item;

        if (comboBox) {
            const name = comboBox.text();
            const dialog = $("#saveSearchCriteriaDialog").last();
            dialog.find("#OldCriteriaName").val(name);
            dialog.find("#IsDefault").prop("checked", e.dataItem.IsDefault);
            dialog.find("#HasMonitoring").prop("checked", e.dataItem.HasMonitoring);

            const email = dialog.find("#Email");
            const templateName = dialog.find("#QESetupId_save-criteria").data("kendoComboBox");
            if (e.dataItem.HasMonitoring) {
                dialog.find(".email-entry").removeClass("d-none");
                // email.prop("required", true);
                dialog.find(".email-template-entry").removeClass("d-none");

                if (e.dataItem.PatSearchNotify) {
                    email.val(e.dataItem.PatSearchNotify.EmailsToNotify);
                    templateName.text(e.dataItem.PatSearchNotify.QEMain.TemplateName);
                    templateName.value(e.dataItem.PatSearchNotify.QESetupId);
                }
            }
            else {
                dialog.find(".email-entry").addClass("d-none");
                // email.removeAttr("required");
                email.val("");
                if (templateName) {
                    templateName.text("");
                    templateName.value(0);
                }
                dialog.find(".email-template-entry").addClass("d-none");
            }

        }
    }

}




