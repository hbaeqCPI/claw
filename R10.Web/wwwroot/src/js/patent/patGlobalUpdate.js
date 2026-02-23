export default class PatGlobalUpdate {

    controllerUrl = "";
    refreshLog = true;
    criteriaUpdated = false;

    caseNumberSearchValueMapper(options) {
        const url = $("#updateCriteriaForm").data("case-number-mapper-url");
        $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }

    getCriteria() {
        const form = $("#updateCriteriaForm");
        const data = pageHelper.formDataToJson(form, false);

        const updateField = $("#UpdateField_patGlobalUpdate").data('kendoComboBox').value();
        const dataFrom = $("#FromData_patGlobalUpdate").data('kendoComboBox').value();
        const dataTo = $("#ToData_patGlobalUpdate").data('kendoComboBox').value();
        data.payLoad["UpdateField"] = updateField;
        data.payLoad["DataFrom"] = dataFrom;
        data.payLoad["DataTo"] = dataTo;

        if (updateField.toLowerCase() == "paydate") {
            const dateDataFrom = pageHelper.cpiDateFormatToSave($("#FromDateData_patGlobalUpdate").data('kendoDatePicker').value());
            const dateDataTo = pageHelper.cpiDateFormatToSave($("#ToDateData_patGlobalUpdate").data('kendoDatePicker').value());
            data.payLoad["DateDataFrom"] = dateDataFrom;
            data.payLoad["DateDataTo"] = dateDataTo;
        }

        const multiselect = $("#ApplicationStatuses").data("kendoMultiSelect");
        const items = multiselect.value();
        let appStatuses = "";
        for (var i = 0; i < items.length; i++) {
            appStatuses += "|" + items[i] + "|";
        }
        data.payLoad["ApplicationStatuses"] = appStatuses;

        const caseTypeMultiSelect = $("#CaseTypes").data("kendoMultiSelect");
        const caseTypeItems = caseTypeMultiSelect.value();
        let caseTypes = "";
        for (var i = 0; i < caseTypeItems.length; i++) {
            caseTypes += "|" + caseTypeItems[i] + "|";
        }
        data.payLoad["CaseTypes"] = caseTypes;

        const kwMultiselect = $("#Keywords").data("kendoMultiSelect");
        const kwItems = kwMultiselect.value();
        let keywords = "";
        for (var i = 0; i < kwItems.length; i++) {
            keywords += "|" + kwItems[i] + "|";
        }
        data.payLoad["Keywords"] = keywords;


        const deDocketActions = $("#DeDocketActions").is(":checked") && updateField.toLowerCase() === "applicationstatus";
        const deDocketTakenDateFrom = $("input[name='DeDocketTakenDateFrom']:checked").val();
        const deDocketTakenDate = pageHelper.cpiDateFormatToSave($("input[name = 'DeDocketTakenDate']").data("kendoDatePicker").value());

        const updateStatusDate = $("#UpdateStatusDate").is(":checked") && updateField.toLowerCase() === "applicationstatus";
        const statusDate = pageHelper.cpiDateFormatToSave($("input[name = 'StatusDate']").data("kendoDatePicker").value());

        data.payLoad["DeDocketActions"] = deDocketActions;
        data.payLoad["DeDocketTakenDateFrom"] = deDocketTakenDateFrom;
        data.payLoad["DeDocketTakenDate"] = deDocketTakenDate;
        data.payLoad["UpdateStatusDate"] = updateStatusDate;
        data.payLoad["StatusDate"] = statusDate;

        const attorneyPosition = $("input[name='SelectAttorneyPosition']:checked").val();
        data.payLoad["AttorneyPosition"] = attorneyPosition;
        data.payLoad["AttorneyFilter1"] = $("#AttorneyFilter1").is(":checked") ? "1" : "0";
        data.payLoad["AttorneyFilter2"] = $("#AttorneyFilter2").is(":checked") ? "1" : "0";
        data.payLoad["AttorneyFilter3"] = $("#AttorneyFilter3").is(":checked") ? "1" : "0";
        data.payLoad["AttorneyFilter4"] = $("#AttorneyFilter4").is(":checked") ? "1" : "0";
        data.payLoad["AttorneyFilter5"] = $("#AttorneyFilter5").is(":checked") ? "1" : "0";
        data.payLoad["AttorneyFilterR"] = $("#AttorneyFilterR").is(":checked") ? "1" : "0";
        data.payLoad["AttorneyFilterD"] = $("#AttorneyFilterD").is(":checked") ? "1" : "0";
        data.payLoad["IncludeAttyInClient"] = $("#IncludeAttyInClient").is(":checked") ? "1" : "0";

        const inventorLevel = $("input[name='SelectInventorLevel']:checked").val();
        data.payLoad["InventorLevel"] = inventorLevel;
        data.payLoad["InventorInv"] = $("#InventorInv").is(":checked") ? "1" : "0";
        data.payLoad["InventorApp"] = $("#InventorApp").is(":checked") ? "1" : "0";

        const ownerLevel = $("input[name='SelectOwnerLevel']:checked").val();
        data.payLoad["OwnerLevel"] = ownerLevel;
        data.payLoad["OwnerInv"] = $("#OwnerInv").is(":checked") ? "1" : "0";
        data.payLoad["OwnerApp"] = $("#OwnerApp").is(":checked") ? "1" : "0";

        return data.payLoad;
    }

    getUpdateField() {
        return $("#UpdateField_patGlobalUpdate").data('kendoComboBox').value();
    }

    getUpdateFieldFromParam = () => {
        var fromDataVal = $("#FromData_patGlobalUpdate").data('kendoComboBox').text();
        return { updateField: this.getUpdateField(), fieldValue: fromDataVal };
    }

    getUpdateFieldToParam = () => {
        var toDataVal = $("#ToData_patGlobalUpdate").data('kendoComboBox').text();
        return { updateField: this.getUpdateField(), fieldValue: toDataVal };
    }

    configurePreviewRows = () => {
        const updateField = this.getUpdateField();
        let isCtryAppLevel = false;
        let isActionLevel = false;
        let isCostLevel = false;
        if (updateField === "RespAtty")
            isActionLevel = true;
        else if (updateField === "PayDate")
            isCostLevel = true;
        else if (updateField === "Attorney") {
            const attorneyPosition = $("input[name='SelectAttorneyPosition']:checked").val();
            const attorneyFilterR = $("#AttorneyFilterR").is(":checked") ? "1" : "0";
            const attorneyFilterD = $("#AttorneyFilterD").is(":checked") ? "1" : "0";

            if (attorneyPosition == "1" || attorneyFilterR == "1" || attorneyFilterD == "1")
                isActionLevel = true;
        }
        else if (updateField === "Inventor") {
            const inventorLevel = $("input[name='SelectInventorLevel']:checked").val();
            const inventorApp = $("#InventorApp").is(":checked") ? "1" : "0";            

            if (inventorLevel == "1" || inventorApp == "1")
                isCtryAppLevel = true;
        }
        else if (updateField === "Owner") {
            const ownerLevel = $("input[name='SelectOwnerLevel']:checked").val();
            const ownerApp = $("#OwnerApp").is(":checked") ? "1" : "0";

            if (ownerLevel == "1" || ownerApp == "1")
                isCtryAppLevel = true;
        }


        if ("Agent-AppOwner-ApplicationStatus-AppInventor-PayDate".search(updateField) > -1) isCtryAppLevel = true;

        if (isActionLevel) {
            $(".updateActionBlock").show();
        } else {
            $(".updateActionBlock").hide();
        }
        if (isCostLevel) {
            $(".updateCostBlock").show();
        } else {
            $(".updateCostBlock").hide();
        }

        if (isCtryAppLevel || isActionLevel) {
            $(".updateCtryAppBlock").show();
        } else {
            $(".updateCtryAppBlock").hide();
        }
    }

    refreshUpdatePreview = function () {

        if (this.criteriaUpdated) {
            const updateField = this.getUpdateField();
            const fromData = $("#FromData_patGlobalUpdate").data('kendoComboBox').value();
            let fromDateData = null;
            if (updateField.toLowerCase() === "paydate")
                fromDateData = $("#FromDateData_patGlobalUpdate").data('kendoDatePicker').value();

            // if no update field and data from, 
            if (updateField === "" || (fromData === "" && (fromDateData == null && updateField.toLowerCase() != "paydate"))) {
                $('#previewListView').data('kendoListView').dataSource.data([]);
                return;
            }
            const listView = $('#previewListView').data('kendoListView');
            listView.dataSource.read();
            listView.dataSource.page(1);

            this.criteriaUpdated = false;
        }
    }

    updateFieldChange = () => {

        let dateUpdate = false;
        const updateField = $("#UpdateField_patGlobalUpdate").data('kendoComboBox').value();
        if (updateField.toLowerCase() === "paydate") {
            $("#updateCriteriaForm").find(".payment-group").removeClass("d-none");
            dateUpdate = true;
        }
        else
            $("#updateCriteriaForm").find(".payment-group").addClass("d-none");

        if (dateUpdate) {
            $(".update-text-group").addClass("d-none");
            $(".update-date-group").removeClass("d-none");
        }
        else {
            $(".update-text-group").removeClass("d-none");
            $(".update-date-group").addClass("d-none");
        }

        if (updateField.toLowerCase() !== "applicationstatus") {
            $(".dedocket-group").addClass("d-none");
            $(".status-date-group").addClass("d-none");
        }

        if (updateField.toLowerCase() != "attorney") {
            $(".attorney-group").addClass("d-none");
        }

        if (updateField.toLowerCase() != "inventor") {
            $(".inventor-group").addClass("d-none");
        }

        if (updateField.toLowerCase() != "owner") {
            $(".owner-group").addClass("d-none");
        }
    }

    fromDataChange = (e) => {
        const tab = $('#update-tab .nav-link.active').attr('id');
        this.criteriaUpdated = true;

        if (tab === "Records-tab") {
            this.refreshUpdatePreview();
        }
        const updateField = $("#UpdateField_patGlobalUpdate").data('kendoComboBox').value();
        const fromData = $("#FromData_patGlobalUpdate").data('kendoComboBox').value();
        const toData = $("#ToData_patGlobalUpdate").data('kendoComboBox').value();

        if (updateField.toLowerCase() === "applicationstatus" && fromData && toData) {
            $(".status-date-group").removeClass("d-none");

            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/Patent/ApplicationStatus/GetPicklistdata`;
            $.get(url, { property: 'ApplicationStatus', selectProperty: false })
                .done((result) => {
                    const fromStatus = result.filter(s => s.StatusId === +fromData);
                    const toStatus = result.filter(s => s.StatusId === +toData);

                    if (((fromStatus.length > 0 && fromStatus[0].ActiveSwitch) || +fromData == -1) && toStatus.length > 0 && !toStatus[0].ActiveSwitch) {
                        $(".dedocket-group").removeClass("d-none")
                    }
                    else
                        $(".dedocket-group").addClass("d-none")
                })
                .fail(function (error) {
                    pageHelper.showErrors(error.responseText);
                });
        }
        else {
            $(".dedocket-group").addClass("d-none");
            $(".status-date-group").addClass("d-none");

            if (updateField.toLowerCase() === "attorney" && fromData && toData) {
                $(".attorney-group").removeClass("d-none");
            }

            if (updateField.toLowerCase() === "inventor" && fromData && toData) {
                $(".inventor-group").removeClass("d-none");
            }

            if (updateField.toLowerCase() === "owner" && fromData && toData) {
                $(".owner-group").removeClass("d-none");
            }
        }

    }

    attorneyPositionChange = () => {
        let self = this;
        $("input[name='SelectAttorneyPosition']").change(function () {
            if (this.value == "1") {
                $(".atty-group-position").addClass("d-none");
                refreshPreview();
            }
            else {
                $(".atty-group-position").removeClass("d-none");
                refreshPreview();
            }
        });
        $("#AttorneyFilter1,#AttorneyFilter2,#AttorneyFilter3,#AttorneyFilter4,#AttorneyFilter5,#AttorneyFilterR,#AttorneyFilterD,#IncludeAttyInClient").change(() => {
            refreshPreview();
        });

        function refreshPreview() {
            const tab = $('#update-tab .nav-link.active').attr('id');
            self.criteriaUpdated = true;

            if (tab === "Records-tab") {
                self.refreshUpdatePreview();
            }
        }
    }

    inventorLevelChange = () => {
        let self = this;
        $("input[name='SelectInventorLevel']").change(function () {
            if (this.value == "1") {
                $(".inventor-group-level").addClass("d-none");
                refreshPreview();
            }
            else {
                $(".inventor-group-level").removeClass("d-none");
                refreshPreview();
            }
        });
        $("#InventorInv,#InventorApp").change(() => {
            refreshPreview();
        });

        function refreshPreview() {
            const tab = $('#update-tab .nav-link.active').attr('id');
            self.criteriaUpdated = true;

            if (tab === "Records-tab") {
                self.refreshUpdatePreview();
            }
        }
    }

    ownerLevelChange = () => {
        let self = this;
        $("input[name='SelectOwnerLevel']").change(function () {
            if (this.value == "1") {
                $(".owner-group-level").addClass("d-none");
                refreshPreview();
            }
            else {
                $(".owner-group-level").removeClass("d-none");
                refreshPreview();
            }
        });
        $("#OwnerInv,#OwnerApp").change(() => {
            refreshPreview();
        });

        function refreshPreview() {
            const tab = $('#update-tab .nav-link.active').attr('id');
            self.criteriaUpdated = true;

            if (tab === "Records-tab") {
                self.refreshUpdatePreview();
            }
        }
    }

    bindEvents = () => {
        const self = this;

        // tab change
        $('#update-tab a').on('click', (e) => {
            const tab = e.target.id;
            switch (tab) {
                case "Records-tab":
                    this.refreshUpdatePreview();
                    break;
                case "Log-tab":
                    if (this.refreshLog) {
                        const logGrid = $('#updateLog').data('kendoGrid');
                        logGrid.dataSource.read();
                        this.refreshLog = false;
                    }
                    break;
            }
        });

        $("#UpdateField_patGlobalUpdate").change((e) => {
            const chkComboFields = "CaseNumber-FamilyNumber-Agent-Attorney1-Attorney2-Attorney3-Attorney4-Attorney5-Client-InvOwner-AppOwner";
            const updateField = $("#UpdateField_patGlobalUpdate").data('kendoComboBox').value();
            let prevDisabledElt = $("#prevDisabledElt").val();               // use for disabling criteria fields (disable if = data to update)
            const screen = "patGlobalUpdate";

            if (prevDisabledElt !== "") {
                // enable previously disable combo box
                if (chkComboFields.search(prevDisabledElt) > -1) {
                    enableComboBox(`#${prevDisabledElt}_${screen}`, true);
                    enableComboBox(`#${prevDisabledElt}Name_${screen}`, true);
                }
                else if (prevDisabledElt === "ApplicationStatus") {
                    enableAppStatus(true);
                }
                // clear
                prevDisabledElt = "";
            }

            // check if we need to disable criteria field = updatefield
            if (chkComboFields.search(updateField) > -1) {
                prevDisabledElt = updateField;
                enableComboBox(`#${prevDisabledElt}_${screen}`, false);
                enableComboBox(`#${prevDisabledElt}Name_${screen}`, false);
            }
            else if (updateField === "ApplicationStatus") {
                enableAppStatus(false);
                prevDisabledElt = updateField;
            }

            $("#prevDisabledElt").val(prevDisabledElt);

            function enableComboBox(comboName, enable) {
                const comboBox = $(comboName).data("kendoComboBox");
                if (comboBox) {
                    comboBox.enable(enable);
                }
            }

            function enableAppStatus(enable) {
                const multiSelect = $("#ApplicationStatuses").data("kendoMultiSelect");
                multiSelect.enable(enable);
                $('input[name=ActiveSwitch]').attr("disabled", !enable);
            }

            // clear selected criteria (don't clear as requested)
            //const updateCriteriaForm = $("#updateCriteriaForm");
            //updateCriteriaForm.clearSearch();
        });

        $("#runUpdate").on('click', (e) => {
            const updateField = this.getUpdateField();
            const fromData = $("#FromData_patGlobalUpdate").data('kendoComboBox').value();
            const toData = $("#ToData_patGlobalUpdate").data('kendoComboBox').value();
            let fromDateData = null
            let toDateData = null;
            if (updateField.toLowerCase() === "paydate") {
                fromDateData = $("#FromDateData_patGlobalUpdate").data('kendoDatePicker').value();
                toDateData = $("#ToDateData_patGlobalUpdate").data('kendoDatePicker').value();
            }

            const updateCriteriaForm = $("#updateCriteriaForm");

            // require from data
            if (fromData === "" && (fromDateData == null && updateField.toLowerCase() != "paydate")) {
                const msg = updateCriteriaForm.data("missing-from-msg");
                cpiAlert.warning(msg);
                return;
            }

            // require to data
            if (toData === "" && (toDateData == null && updateField.toLowerCase() != "paydate")) {
                const msg = updateCriteriaForm.data("missing-to-msg");
                cpiAlert.warning(msg);
                return;
            }

            // don't allow if update sets appstatus or inventors to null
            if ($("#ToData_patGlobalUpdate").data('kendoComboBox').value() === "" &&
                ("ApplicationStatus-InvInventor-AppInventor-InvOwner-AppOwner-Inventor-Owner".search(updateField) > -1)) {
                alert(`You cannot remove: ${updateField}`);
                return;
            }

            const updateMsg = updateCriteriaForm.data("update-msg");
            cpiConfirm.confirm("", updateMsg, () => {
                const param = this.getCriteria();
                const selectedIds = this.getSelection();
                const selectedDKIds = this.getDataKeySelection();

                if (selectedIds.length > 0 || $("#IncludeAttyInClient").is(":checked")) {
                    param.KeyIds = selectedIds;
                    param.DataKeyIds = selectedDKIds;

                    $.ajax({
                        url: this.controllerUrl + "PatGlobalUpdate/RunUpdate",
                        data: param,
                        type: "POST",
                        success: (data) => {
                            const updatedMsg = updateCriteriaForm.data("updated-msg");
                            pageHelper.showSuccess(updatedMsg + " " + data);
                            this.refreshLog = true;
                            this.criteriaUpdated = true; //force refresh on preview list
                            this.refreshUpdatePreview();
                        },
                        error: function (e) {
                            pageHelper.showErrors(e);
                        }
                    });
                }
            });
        });

        const previewContainer = $("#updatePreviewContainer");
        const previewContainerGrid = previewContainer.find("#previewListView");

        previewContainerGrid.on("change", "input[type='checkbox']", function () {
            self.pageSelection(this, this.checked);
        });

        previewContainer.on("change", "input[type='checkbox'].page-update", function () {
            const pageSelection = this.checked;
            previewContainerGrid.find("input[type='checkbox']").each(function () {
                self.pageSelection(this, pageSelection);
                $(this).prop("checked", pageSelection);
            });
        });

        self.checkUpdatedCriteria();
    }

    pageSelection = (checkbox, selection) => {
        const previewContainer = $("#updatePreviewContainer");
        const previewContainerGrid = previewContainer.find("#previewListView");
        const resultGrid = previewContainerGrid.data("kendoListView");
        const data = resultGrid.dataSource.data();
        const id = $(checkbox).data("id");
        if (id) {
            const item = data.find(e => e.KeyId == id); //diff data type
            if (item) {
                item.Selected = selection;
            }
        }
    }

    getSelection = () => {
        const previewContainer = $("#updatePreviewContainer");
        const previewContainerGrid = previewContainer.find("#previewListView");
        const resultGrid = previewContainerGrid.data("kendoListView");
        //const selected = resultGrid.dataSource.data().filter(r => r.Selected).map(s => s.KeyId);
        const selected = resultGrid.dataSource.data().filter(r => r.Selected).map(s => { return { Id: s.KeyId, DataKey: s.DataKey } });
        return selected;
    }

    getDataKeySelection = () => {
        const previewContainer = $("#updatePreviewContainer");
        const previewContainerGrid = previewContainer.find("#previewListView");
        const resultGrid = previewContainerGrid.data("kendoListView");
        const selectedDK = resultGrid.dataSource.data().filter(r => r.Selected).map(s => s.DataKey);
        return selectedDK;
    }

    updateLogDataBound = (e) => {
        const data = e.sender.dataSource.data();

        if (data.length > 0) {
            const listView = e.sender.element;

            //Weird issue with rendering page on different browsers/computers
            //Set delay to ensure the tag exists before run textOverflow()
            setTimeout(
                function () {
                    //collapsible container
                    $.each(listView.find(".globalUpdate-collapsible"), function () {
                        $(this).textOverflow();
                    });
                }, 150);
        }
    }

    checkUpdatedCriteria = () => {
        const self = this;
        const container = $("#updateCriteriaForm");

        container.on("input", "input, textarea", () => {
            self.criteriaUpdated = true;
        });

        //for checkboxes on ie/edge
        container.on("change", "input[type='checkbox']", () => {
            self.criteriaUpdated = true;
        });

        container.on("change", "input[type='radio']", () => {
            self.criteriaUpdated = true;
        });

        container.find(".k-combobox > input").each(function () {
            const comboBox = $(this).data("kendoComboBox");
            if (comboBox) {
                comboBox.bind("change", () => {
                    self.criteriaUpdated = true;
                });
            }
        });

        container.find(".k-dropdown > input").each(function () {
            const dropdownList = $(this).data("kendoDropDownList");
            if (dropdownList) {
                dropdownList.bind("change", () => {
                    self.criteriaUpdated = true;
                });
            }
        });

        container.find(".k-datepicker input").each(function () {
            const datePicker = $(this).data("kendoDatePicker");
            if (datePicker) {
                datePicker.bind("change", () => {
                    self.criteriaUpdated = true;
                });
            }
        });

        container.find(".k-datetimepicker input").each(function () {
            const dateTimePicker = $(this).data("kendoDateTimePicker");
            if (dateTimePicker) {
                dateTimePicker.bind("change", () => {
                    self.criteriaUpdated = true;
                });
            }
        });

        container.find(".k-numerictextbox input").each(function () {
            const numericTextBox = $(this).data("kendoNumericTextBox");
            if (numericTextBox) {
                numericTextBox.bind("spin", () => {
                    self.criteriaUpdated = true;
                });
            }
        });

        container.find(".k-editable-area > textarea").each(function () {
            var editor = $(this).data("kendoEditor");
            if (editor) {
                $(editor.body).bind("input", () => {
                    self.criteriaUpdated = true;
                });
                $(editor.body).bind("keyup", (e) => {
                    //delete key
                    if (e.keyCode === 46) {
                        self.criteriaUpdated = true;
                    }
                });
            }
        });
        container.find(".k-multiselect > input").each(function () {
            const multiselect = $(this).data("kendoMultiSelect");
            if (multiselect) {
                multiselect.bind("change", () => {
                    self.criteriaUpdated = true;
                });
            }
        });

        container.on("click", ".k-clear-value", () => {
            self.criteriaUpdated = true;
        });
    }
}


