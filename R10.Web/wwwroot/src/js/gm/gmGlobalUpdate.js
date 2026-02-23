export default class GMGlobalUpdate {

    controllerUrl = "";
    refreshLog = true;

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

        const updateField = $("#UpdateField_gmGlobalUpdate").data('kendoComboBox').value();
        const dataFrom = $("#FromData_gmGlobalUpdate").data('kendoComboBox').value();
        const dataTo = $("#ToData_gmGlobalUpdate").data('kendoComboBox').value();
        data.payLoad["UpdateField"] = updateField;
        data.payLoad["DataFrom"] = dataFrom;
        data.payLoad["DataTo"] = dataTo;

        if (updateField.toLowerCase() == "paydate") {
            const dateDataFrom = pageHelper.cpiDateFormatToSave($("#FromDateData_gmGlobalUpdate").data('kendoDatePicker').value());
            const dateDataTo = pageHelper.cpiDateFormatToSave($("#ToDateData_gmGlobalUpdate").data('kendoDatePicker').value());
            data.payLoad["DateDataFrom"] = dateDataFrom;
            data.payLoad["DateDataTo"] = dateDataTo;
        }

        const multiselect = $("#MatterStatuses").data("kendoMultiSelect");
        const items = multiselect.value();
        let gmStatuses = "";        
        for (var i = 0; i < items.length; i++) {
            gmStatuses += "|" + items[i] + "|";
        }
        data.payLoad["MatterStatuses"] = gmStatuses;

        const kwMultiselect = $("#Keywords").data("kendoMultiSelect");
        const kwItems = kwMultiselect.value();
        let keywords = "";
        for (var i = 0; i < kwItems.length; i++) {
            keywords += "|" + kwItems[i] + "|";
        }
        data.payLoad["Keywords"] = keywords;

        const deDocketActions = $("#DeDocketActions").is(":checked") && updateField.toLowerCase() === "matterstatus";
        const deDocketTakenDateFrom = $("input[name='DeDocketTakenDateFrom']:checked").val();
        const deDocketTakenDate = pageHelper.cpiDateFormatToSave($("input[name = 'DeDocketTakenDate']").data("kendoDatePicker").value());

        const updateStatusDate = $("#UpdateStatusDate").is(":checked") && updateField.toLowerCase() === "matterstatus";
        const statusDate = pageHelper.cpiDateFormatToSave($("input[name = 'StatusDate']").data("kendoDatePicker").value());

        data.payLoad["DeDocketActions"] = deDocketActions;
        data.payLoad["DeDocketTakenDateFrom"] = deDocketTakenDateFrom;
        data.payLoad["DeDocketTakenDate"] = deDocketTakenDate;
        data.payLoad["UpdateStatusDate"] = updateStatusDate;
        data.payLoad["StatusDate"] = statusDate;

        return data.payLoad;
    }

    getUpdateField() {
        return $("#UpdateField_gmGlobalUpdate").data('kendoComboBox').value();
    }

    getUpdateFieldFromParam = () => {
        var fromDataVal = $("#FromData_gmGlobalUpdate").data('kendoComboBox').text();
        return { updateField: this.getUpdateField(), fieldValue: fromDataVal };
    }

    getUpdateFieldToParam = () => {
        var toDataVal = $("#ToData_gmGlobalUpdate").data('kendoComboBox').text();
        return { updateField: this.getUpdateField(), fieldValue: toDataVal };
    }

    configurePreviewRows=()=> {
        const updateField = this.getUpdateField();        
        let isActionLevel = false;
        let isCostLevel = false;
        if (updateField === "RespAtty")
            isActionLevel = true;
        else if (updateField === "PayDate")
            isCostLevel = true;

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
    }

    refreshUpdatePreview = function () {
        const updateField = this.getUpdateField();
        const fromData = $("#FromData_gmGlobalUpdate").data('kendoComboBox').value();
        let fromDateData = null;
        if (updateField.toLowerCase() === "paydate")
            fromDateData = $("#FromDateData_gmGlobalUpdate").data('kendoDatePicker').value();

        // if no update field and data from, 
        if (updateField === "" || (fromData === "" && (fromDateData == null && updateField.toLowerCase() != "paydate"))) {
            $('#previewListView').data('kendoListView').dataSource.data([]);
            return;
        }
        const listView = $('#previewListView').data('kendoListView');
        listView.dataSource.read();
        listView.dataSource.page(1);
    }

    updateFieldChange = () => {
        let dateUpdate = false;
        const updateField = $("#UpdateField_gmGlobalUpdate").data('kendoComboBox').value();
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
        if (updateField.toLowerCase() !== "matterstatus") {
            $(".dedocket-group").addClass("d-none");
            $(".status-date-group").addClass("d-none");
        }
    }

    // no need to trap this event, the fromDataChange kicks off before this (and takes care of the refresh)
    //this.updateFieldChange = function (e) {
    //    alert("updateFieldChange");
    //    $('#previewListView').data('kendoListView').dataSource.data([]);
    //}

    fromDataChange = (e)=> {
        const tab = $('.tab-header .active').attr('id');
        if (tab === "Records-tab") {
            this.refreshUpdatePreview();
        }
        const updateField = $("#UpdateField_gmGlobalUpdate").data('kendoComboBox').value();
        const fromData = $("#FromData_gmGlobalUpdate").data('kendoComboBox').value();
        const toData = $("#ToData_gmGlobalUpdate").data('kendoComboBox').value();

        if (updateField.toLowerCase() === "matterstatus" && fromData && toData) {
            $(".status-date-group").removeClass("d-none");

            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/GeneralMatter/MatterStatus/GetPicklistdata`;
            $.get(url, { property: 'MatterStatus', selectProperty: false })
                .done((result) => {
                    const fromStatus = result.filter(s => s.MatterStatusID === +fromData);
                    const toStatus = result.filter(s => s.MatterStatusID === +toData);

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
        }
    }

    bindEvents = () => {
        const self = this;

        // tab change
        $('#update-tab a').on('click', (e)=> {
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

        $("#UpdateField_gmGlobalUpdate").change((e)=> {
            const chkComboFields = "CaseNumber-Agent-Client";
            const updateField = $("#UpdateField_gmGlobalUpdate").data('kendoComboBox').value();
            let prevDisabledElt = $("#prevDisabledElt").val();               // use for disabling criteria fields (disable if = data to update)
            const screen = "gmGlobalUpdate";

            if (prevDisabledElt !== "") {
                // enable previously disable combo box
                if (chkComboFields.search(prevDisabledElt) > -1) {
                    enableComboBox(`#${prevDisabledElt}_${screen}`, true);
                    enableComboBox(`#${prevDisabledElt}Name_${screen}`, true);
                }
                else if (prevDisabledElt === "MatterStatus") {
                    enableGMStatus(true);
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
            else if (updateField === "MatterStatus") {
                enableGMStatus(false);
                prevDisabledElt = updateField;
            }

            $("#prevDisabledElt").val(prevDisabledElt);

            function enableComboBox(comboName, enable) {                
                const comboBox = $(comboName).data("kendoComboBox");
                if (comboBox) {
                    comboBox.enable(enable);
                }
            }

            function enableGMStatus(enable) {
                const multiSelect = $("#MatterStatuses").data("kendoMultiSelect");
                multiSelect.enable(enable);
                $('input[name=ActiveSwitch]').attr("disabled", !enable);
            }

            // clear selected criteria (don't clear as requested)
            //const updateCriteriaForm = $("#updateCriteriaForm");
            //updateCriteriaForm.clearSearch();
        });

        $("#runUpdate").on('click', (e) => {
            const updateField = this.getUpdateField();
            const fromData = $("#FromData_gmGlobalUpdate").data('kendoComboBox').value();
            const toData = $("#ToData_gmGlobalUpdate").data('kendoComboBox').value();
            let fromDateData = null
            let toDateData = null;
            if (updateField.toLowerCase() === "paydate") {
                fromDateData = $("#FromDateData_gmGlobalUpdate").data('kendoDatePicker').value();
                toDateData = $("#ToDateData_gmGlobalUpdate").data('kendoDatePicker').value();
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

            // don't allow if update sets  to null            
            if ($("#ToData_gmGlobalUpdate").data('kendoComboBox').value() === "" &&
                ("MatterStatus-Attorney".search(updateField) > -1)) {
                alert(`You cannot remove: ${updateField}`);
                return;
            }

            const updateMsg = updateCriteriaForm.data("update-msg");
            cpiConfirm.confirm("", updateMsg, () => {
                const param = this.getCriteria();
                const selectedIds = this.getSelection();

                if (selectedIds.length > 0) {
                    param.KeyIds = selectedIds;

                    $.ajax({
                        url: this.controllerUrl + "GMGlobalUpdate/RunUpdate",
                        data: param,
                        type: "POST",
                        success: (data) => {
                            const updatedMsg = updateCriteriaForm.data("updated-msg");
                            pageHelper.showSuccess(updatedMsg + " " + data);

                            this.refreshLog = true;
                            this.refreshUpdatePreview();
                        },
                        error: function (e) {
                            if (e.responseJSON !== undefined)
                                pageHelper.showGridErrors(e.responseJSON);
                            else
                                pageHelper.showErrors(e.responseText);
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
        const selected = resultGrid.dataSource.data().filter(r => r.Selected).map(s => s.KeyId);
        return selected;
    }

    updateLogDataBound(e) {        
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
}


