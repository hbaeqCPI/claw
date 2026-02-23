import ActivePage from "../activePage";

export default class PatInventionRemunerationPage extends ActivePage {
    
    constructor() {
        super();
        this.currentInventorInvID = 0;
        this.inventorSelected = [];
    }

    Init(screen) {
        const grid = $(`#inventionInventorRemunerationInventorsInfoList`);
        grid.data("kendoListView").dataSource.read();

        const grid2 = $(`#inventionInventorRemunerationProductSalesInfoGrid`).data("kendoGrid");
        grid2.dataSource.read();
        grid.on("click", ".inventorLink", (e) => {
            e.stopPropagation();

            let url = $(e.target).data("url");
            const row = $(e.target).closest("tr");
            const dataItem = grid.data("kendoListView").dataItem(row);
            pageHelper.openLink(url, false);
        });

        this.InitInventorList();

        this.tabChangeSetListener();

        $(document).ready(() => {
            const productsGrid = $(`#productsGrid_${screen}`);
            productsGrid.on("click", ".productLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = productsGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.ProductId);
                pageHelper.openLink(linkUrl, false);
            });
        });
    }

    tabChangeSetListener = () => {
        $('#remunerationDetailTab a').on('click',
            (e) => {
                e.preventDefault();
                const tab = e.target.id;
                this.loadTabContent(tab);
            });
    }

    loadTabContent(tab) {
        const pageId = this.mainDetailContainer;

        switch (tab) {
            case "remunerationProductsTab":
                $(document).ready(() => {
                    const productsGrid = $(`#productsGrid_${pageId}`).data("kendoGrid");
                    if (productsGrid)
                        productsGrid.dataSource.read();
                });
                break;

            case "inventorAccountSummaryTab":
                $(document).ready(() => {
                    const grid = $(`#inventionRemunerationDistributionInfoGrid`).data("kendoGrid");
                    if (grid)
                    {
                        const remunerationId = document.getElementById("RemunerationId");
                        const invId = document.getElementById("InvId");
                        $(`#inventionRemunerationDistributionInfoGrid`).find(".k-grid-toolbar").on("click",
                            ".ShowPaymentDateUpdateScreen",
                            () => {
                                const url = $(`#inventionRemunerationDistributionInfoGrid`).parent().data("url-mass-update");
                                const data = {
                                    remunerationId: remunerationId.value,
                                    invId: invId.value,
                                };
                                this.openAwardMassUpdateEntry(grid, url, data, true);
                            });
                        grid.dataSource.read();
                    }
                });
                break;
        }
    }

    openAwardMassUpdateEntry(grid, url, data, closeOnSave) {
        const self = this;

        $.ajax({
            url: url,
            data: data,
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $("#inventorAwardMassUpdateEntryDialog");

                let entryForm = dialogContainer.find("form")[0];
                dialogContainer.modal("show");
                entryForm = $(entryForm);
                entryForm.cpiPopupEntryForm(
                    {
                        dialogContainer: dialogContainer,
                        closeOnSubmit: closeOnSave,
                        beforeSubmit: function () {
                        },
                        afterSubmit: function (e) {
                            grid.dataSource.read();
                            self.updateRecordStamps();
                            dialogContainer.modal("hide");

                            if (e.emailWorkflows) {
                                const promise = pageHelper.handleEmailWorkflow(e);
                                promise.then(() => {
                                });
                            }
                        }
                    }
                );
            },
        });
    }

    InitInventorList() {
        const name = "inventionInventorRemunerationInventorsInfoList";
        const el = $(`#${name}`);
        const grid = el.data("kendoListView");

        this.handleInventorListViewEntry(el, grid);
    }

    inventorsInfoGridChange = () => {
        const grid = $("#inventionInventorRemunerationInventorsInfoGrid").data("kendoGrid");
        const selectedItem = grid.dataItem(grid.select());
        if (selectedItem !== null) {
            var data = {};
            data["inventorInvID"] = selectedItem.InventorInvID;
            this.currentInventorInvID = selectedItem.InventorInvID;
            $('#inventionInventorRemunerationDistributionInfoGrid').data('kendoGrid').dataSource.read(data);
        }
    }

    inventorsInfoListChange = (e) => {
        var id = e.dataset.id;
        if (id != this.currentInventorInvID) {
            var data = {};
            data["inventorInvID"] = id;
            this.currentInventorInvID = id;
            $('#inventionInventorRemunerationDistributionInfoGrid').data('kendoGrid').dataSource.read(data);
        }
    }

    //SonarQube: Duplicate name 'GetCurrentInventorInvID'
    //GetCurrentInventorInvID = (e) => {
    //    var data = {};
    //    data["inventorInvID"] = this.currentInventorInvID;
    //    return data;
    //}

    LumpSumInfoEditable = (e) => {
        return e.PaidByLumpSum;
    };

    BuyingRightsAmountEditable = (e) => {
        return e.BuyingRightsDate == null;
    };

    //RevenueEditable = (e) => {
    //    return e.UseOverrideRevenue == true;
    //};
    //UseOverrideAmountEditable = (e) => {
    //    return e.PaidDate == null;
    //};
    //AmountEditable = (e) => {
    //    return e.PaidDate == null && e.UseOverrideAmount == true;
    //};


    EditableCellHandler = () => {

        document.querySelectorAll('.data-LumpSumAmount').forEach(element => {
            var currentDataItem = $("#inventionInventorRemunerationInventorsInfoGrid").data("kendoGrid").dataItem($(element).closest("tr"));
            if (!currentDataItem.PaidByLumpSum) {
                element.classList.remove("editable-cell");
            }
        });

        document.querySelectorAll('.data-LumpSumPaidDate').forEach(element => {
            var currentDataItem = $("#inventionInventorRemunerationInventorsInfoGrid").data("kendoGrid").dataItem($(element).closest("tr"));
            if (!currentDataItem.PaidByLumpSum) {
                element.classList.remove("editable-cell");
            }
        });

        document.querySelectorAll('.data-BuyingRightsAmount').forEach(element => {
            var currentDataItem = $("#inventionInventorRemunerationInventorsInfoGrid").data("kendoGrid").dataItem($(element).closest("tr"));
            if (currentDataItem.BuyingRightsDate != null) {
                element.classList.remove("editable-cell");
            }
        });

        $(".revenueRefreshLink").on("click", function () {
            var grid = $("#inventionInventorRemunerationProductSalesInfoGrid").data("kendoGrid");
            var currentDataItem = grid.dataItem($(this).closest("tr"));
            currentDataItem.UseOverrideRevenue = false;
            currentDataItem.dirty = true;
            const gridInfo = {
                name: "inventionInventorRemunerationProductSalesInfoGrid",
                isDirty: true,
                filter: { parentId: currentDataItem.RemunerationId },
            };
            pageHelper.kendoGridDirtyTracking(grid, gridInfo);
            $("#inventionInventorRemunerationProductSalesInfoGrid").find(".k-grid-Save").click();
        });
        $(".amountRefreshLink").on("click", function () {
            var grid = $("#inventionInventorRemunerationDistributionInfoGrid").data("kendoGrid");
            var currentDataItem = grid.dataItem($(this).closest("tr"));
            currentDataItem.UseOverrideAmount = false;
            currentDataItem.dirty = true;
            const gridInfo = {
                name: "inventionInventorRemunerationDistributionInfoGrid",
                isDirty: true,
                filter: { parentId: currentDataItem.InventorInvID },
            };
            pageHelper.kendoGridDirtyTracking(grid, gridInfo);
            $("#inventionInventorRemunerationDistributionInfoGrid").find(".k-grid-Save").click();
        })
    };

    InitMatrix = () => {
        this.MatrixHandleMenu();
        const container = $("#MatrixDataContainer");
        container.on("change",
            ".matrix-data",
            (e) => {
                this.UpdateMatrixData(e);
            });
    }

    MatrixHandleMenu = () => {
        document.querySelectorAll(".matrix-menu").forEach(menu => {
            if (menu.className.includes("active")) {
                const activeContent = document.getElementById(menu.id + "-content");
                activeContent.removeAttribute("hidden");
            }
        })   
    }

    MenuOnChange = (e) => {
        document.querySelectorAll(".matrix-menu").forEach(menu => {
            if (menu.className.includes("active")) {
                const content = document.getElementById(menu.id + "-content");
                content.setAttribute("hidden", "hidden");
                $(menu).removeClass("active");
            }
            if (menu.id == e.id) {
                $(menu).addClass("active");
                const activeContent = document.getElementById(menu.id + "-content");
                activeContent.removeAttribute("hidden");
            }
        })   
    };

    UpdateMatrixData = (e) => {
        this.UpdateMatrixValue(e);

        var matrixData = document.getElementById("MatrixData");
        var matrixDataText = "";
        document.querySelectorAll(".matrix-data").forEach(dataElement => {
            if (dataElement.type == "number" && dataElement.value != null) {
                matrixDataText += "|" + dataElement.id + "~" + dataElement.value;//Math.round(dataElement.value * 100) / 100;
            } else if ((dataElement.type == "checkbox" || dataElement.type == "radio")) {
                matrixDataText += "|" + dataElement.id + "~";
                matrixDataText += dataElement.checked ? "1" : "0";
            }   
        })  
        matrixData.value = matrixDataText;
    }

    UpdateMatrixValue = (e) => {
        if (e.target.name.includes("MatrixManualId~"))
            return;
        const matrix = document.getElementById(e.target.name)
        const matrixType = matrix.getAttribute("MatrixType");
        const maxValue = matrix.getAttribute("MaxValue");
        const minValue = matrix.getAttribute("MinValue");
        const useManualEntry = matrix.getAttribute("UseManualEntry");
        if (e.target.name == e.target.id)
            return;
        if (useManualEntry == "1")
            return;

        let actualValue = 0.0; 
        const searchName = "[name = '" + e.target.name + "']";
        if (matrixType =="Sum") {
            document.querySelectorAll(searchName).forEach(dataElement => {
                if (dataElement.checked)
                    actualValue += parseFloat(dataElement.getAttribute("CriteriaValue"));
            })  
        }
        else if (matrixType == "Count") {
            document.querySelectorAll(searchName).forEach(dataElement => {
                if (dataElement.checked)
                    actualValue += 1;
            }) 
        }
        else if (matrixType == "Range") {
            document.querySelectorAll(searchName).forEach(dataElement => {
                if (dataElement.checked)
                    actualValue += parseFloat(dataElement.getAttribute("CriteriaValue"));
            }) 
        }
        else if (matrixType == "Selection") {
            document.querySelectorAll(searchName).forEach(dataElement => {
                if (dataElement.checked)
                    actualValue += parseFloat(dataElement.getAttribute("CriteriaValue"));
            }) 
        }
        else if (matrixType == "Max") {
            let result = Number.MIN_VALUE;
            document.querySelectorAll(searchName).forEach(dataElement => {
                if (dataElement.value > result) {
                    actualValue = parseFloat(dataElement.value);
                    result = parseFloat(dataElement.value);
                }
            }) 
        }
        else if (matrixType == "Min") {
            let result = Number.MAX_VALUE;
            document.querySelectorAll(searchName).forEach(dataElement => {
                if (dataElement.value < result) {
                    actualValue = parseFloat(dataElement.value);
                    result = parseFloat(dataElement.value);
                }
            }) 
        }

        if (maxValue != null && actualValue > parseFloat(maxValue))
            actualValue = maxValue;
        if (minValue != null && actualValue < parseFloat(minValue))
            actualValue = minValue;
        matrix.value = actualValue;
    }

    emailInventorAwardGridRow = (e, grid, afterEmail) => {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        e.preventDefault();
        const parent = $("#" + e.delegateTarget.id).parent();
        const url = parent.data("url-email") + "?id=" + dataItem.InventorInvID;

        $.ajax({
            url: url,
            type: "Get",
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);

                if (afterEmail)
                    afterEmail(result);
            },
            error: function (e) {
                pageHelper.showErrors(e.responseText);
            }
        });
    }

    showLetterInventorAwardGridRow = (e, grid) => {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        e.preventDefault();
        const parent = $("#" + e.delegateTarget.id).parent();
        const url = parent.data("url-letter") + "?id=" + dataItem.InventorInvID;

        $.ajax({
            url: url,
            type: "Get",
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
            },
            error: function (e) {
                pageHelper.showErrors(e.responseText);
            }
        });
    }

    emailInventorAwardListRow = (id) => {
        const containner = $("#inventorListContainer");
        const url = containner.data("url-email") + "?id=" + id;

        $.ajax({
            url: url,
            type: "Get",
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
            },
            error: function (e) {
                pageHelper.showErrors(e.responseText);
            }
        });
    }

    showLetterInventorAwardListRow = (id) => {
        const containner = $("#inventorListContainer");
        const url = containner.data("url-letter") + "?id=" + id;

        $.ajax({
            url: url,
            type: "Get",
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
            },
            error: function (e) {
                pageHelper.showErrors(e.responseText);
            }
        });
    }

    emailDistributionAwardGridRow = (e, grid, afterEmail) => {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        e.preventDefault();
        const parent = $("#" + e.delegateTarget.id).parent();
        const url = parent.data("url-email") + "?id=" + dataItem.DistributionId;

        $.ajax({
            url: url,
            type: "Get",
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);

                if (afterEmail)
                    afterEmail(result);
            },
            error: function (e) {
                pageHelper.showErrors(e.responseText);
            }
        });
    }

    showLetterDistributionAwardGridRow = (e, grid) => {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        e.preventDefault();
        const parent = $("#" + e.delegateTarget.id).parent();
        const url = parent.data("url-letter") + "?id=" + dataItem.DistributionId;

        $.ajax({
            url: url,
            type: "Get",
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
            },
            error: function (e) {
                pageHelper.showErrors(e.responseText);
            }
        });
    }

    showStage = (e, grid) => {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        e.preventDefault();
        const parent = $("#" + e.delegateTarget.id).parent();
        const url = parent.data("url-stage") + "?id=" + dataItem.ProductSaleId;

        $.ajax({
            url: url,
            type: "Get",
            success: function (result) {
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                $("#stageDialog").modal('show');;
            },
            error: function (e) {
                pageHelper.showErrors(e.responseText);
            }
        });
    }

    exportStageToExcel = (productSaleId) => {
        const url = $("#yearlyAwardsContainer").data("url-export-stage-excel") + "?id=" + productSaleId;
        var req = new XMLHttpRequest();
        req.open("GET", url, true);
        req.responseType = "blob";
        req.onload = function (event) {
            var blob = req.response;
            //var fileName = req.getResponseHeader("fileName") //if you have the fileName header available
            var link = document.createElement('a');
            link.href = window.URL.createObjectURL(blob);
            link.download = "Stage Info";
            link.click();
        };

        req.send();
    }

    handleInventorListViewEntry(el, grid) {
        const form = $("#inventorListEntry");
        const pager = $("#inventionInventorRemunerationInventorsInfoList_pager");
        const self = this;

        if (pager) {
            pager.addClass("pt-3");
            form.append(pager);
        }

        el.on("click", "tr td.item-view", function (e) {
            self.inventorListGetEditTemplate(grid, $(this.parentElement));
        });
        

        //el.on("click", ".ids-view", function (e) {
        //    self.handleIDSListViewViewer(e, grid);
        //});

        $("#inventorListCancelAll").on("click", () => {
            this.inventorSelected = [];
            const container = $("#inventorListDialog");
            const title = container.data("confirm-title");
            const cancelPrompt = container.data("cancel-confirm-message");

            cpiConfirm.confirm(title, cancelPrompt, () => {
                $("#inventionInventorRemunerationInventorsInfoList").data("kendoListView").dataSource.read();
                this.setInventorListEntryNotDirty();
                //$("#remunerationIventorTable").find(".k-link").data("sorter").enableSort();
            });

        });

        $("#inventorListSaveAll").on("click", () => {
            self.handleInventorListViewSave(grid);
            //$("#remunerationIventorTable").find(".k-link").data("sorter").enableSort();
        });

        form.on("submit", (e) => {
            e.preventDefault();
            e.stopPropagation();

            this.inventorListGetFormData(form, grid);
            window.cpiStatusMessage.hide();
        });

        $("#inventorListExport").on("click", () => {
            const container = $("#inventorListDialog");
            const remunerationId = document.getElementById("RemunerationId");
            const url = container.data("export-url") + "?id=" + remunerationId.value;
            console.log(url);
            var req = new XMLHttpRequest();
            req.open("GET", url, true);
            req.responseType = "blob";
            req.onload = function (event) {
                var blob = req.response;
                var link = document.createElement('a');
                link.href = window.URL.createObjectURL(blob);
                link.download = "Inventor List Export";
                link.click();
            };

            req.send();
        });
    }

    inventorListGetFormData(form, grid) {
        const data = pageHelper.formDataToJson(form);

        const inventorInv = this.mapRawDataToInventorInv(data.payLoad);
        inventorInv.dirty = true;

        const allData = grid.dataSource.data();
        const existing = allData.find(r => r.InventorInvID === inventorInv.InventorInvID);

        if (existing) {
            let dataItem = grid.dataSource.getByUid(existing.uid);

            const entries = Object.entries(inventorInv);
            for (const [prop, val] of entries) {
                dataItem[prop] = val;
            }
        }
    }

    inventorListGetEditTemplate(grid, row) {

        //can modify
        if ($("#inventorListAddNew").length === 0)
            return;

        const form = $("#inventorListEntry");
        //submit the previous one first
        if (form.find("#InventorInvID").length > 0) {
            form.submit();

            const id = form.find("#InventorInvID").val();
            const data = grid.dataSource.data().find(r => r.InventorInvID === parseInt(id));
            if (data) {
                data.Percentage = form.find("#Percentage").val();
                data.PositionA = form.find("#PositionA").val();
                data.PositionB = form.find("#PositionB").val();
                data.PositionC = form.find("#PositionC").val();
                data.InventorPosition = form.find("#InventorPosition").val();
                data.BuyingRightsAmount = form.find("#BuyingRightsAmount").val();
                data.BuyingRightsDate = form.find("#BuyingRightsDate_").val();
                data.InitialPayment = form.find("#InitialPayment").val();
                data.InitialPaymentDate = form.find("#InitialPaymentDate_").val();
                data.PaidByLumpSum = document.getElementById("PaidByLumpSum").checked;
                data.LumpSumAmount = form.find("#LumpSumAmount").val();
                data.LumpSumPaidDate = form.find("#LumpSumPaidDate_").val();
                data.RemunerationRemarks = form.find("#RemunerationRemarks").val();
                data.ClaimedDate = form.find("#ClaimedDate_").val();

                this.inventorSelected = this.inventorSelected.filter(item => item.InventorInvID !== parseInt(id));
                this.inventorSelected.push(data);

                const template = kendo.template($("#remunerationInventorTemplate").html());
                const result = template(data);

                const oldRow = $(`#inventionInventorRemunerationInventorsInfoList tr[data-id=${id}]`);
                $(result).insertBefore(oldRow[0]);
                oldRow.remove();
            }
        }

        let id = row.data("id");

        const container = $("#inventorListDialog");
        let templateUrl = container.data("url-edit-template");

        $.get(templateUrl, { InventorInvID: id }, (response) => {
            const listId = "inventionInventorRemunerationInventorsInfoList";
            const idSelector = id;

            const oldRow = $(`#${listId} tr[data-id=${idSelector}]`);
            $(response).insertBefore(oldRow[0]);
            oldRow.remove();

            //use current value
            var currentValue = this.inventorSelected.filter(item => item.InventorInvID == parseInt(id));
            if (currentValue.length != 0)
            {
                form.find("#Percentage").val(currentValue[0].Percentage);
                form.find("#PositionA").val(currentValue[0].PositionA);
                form.find("#PositionB").val(currentValue[0].PositionB);
                form.find("#PositionC").val(currentValue[0].PositionC);
                form.find("#InventorPosition").val(currentValue[0].InventorPosition);
                form.find("#BuyingRightsAmount").val(currentValue[0].BuyingRightsAmount);
                form.find("#BuyingRightsDate_").val(currentValue[0].BuyingRightsDate);
                form.find("#InitialPayment").val(currentValue[0].InitialPayment);
                form.find("#InitialPaymentDate_").val(currentValue[0].InitialPaymentDate);
                document.getElementById("PaidByLumpSum").checked = currentValue[0].PaidByLumpSum;
                form.find("#LumpSumAmount").val(currentValue[0].LumpSumAmount);
                form.find("#LumpSumPaidDate_").val(currentValue[0].LumpSumPaidDate);
                form.find("#RemunerationRemarks").val(currentValue[0].RemunerationRemarks);
                form.find("#ClaimedDate_").val(currentValue[0].ClaimedDate);
            }

            this.stripeTableRows(listId);

            this.handleFormChanges(form);

            $.validator.unobtrusive.parse(form);
            if (form.data("validator") !== undefined) {
                form.data("validator").settings.ignore = ""; //include hidden fields (kendo controls)
            }
            pageHelper.addMaxLength(form);
            pageHelper.clearInvalidKendoDate(form);

        });
    }

    handleInventorListViewSave(grid) {
        const form = $("#inventorListEntry");

        if (form.find("#InventorInvID").length > 0) {
            form.submit();

            const id = form.find("#InventorInvID").val();
            const data = grid.dataSource.data().find(r => r.InventorInvID === parseInt(id));
            if (data) {
                data.Percentage = form.find("#Percentage").val();
                data.PositionA = form.find("#PositionA").val();
                data.PositionB = form.find("#PositionB").val();
                data.PositionC = form.find("#PositionC").val();
                data.InventorPosition = form.find("#InventorPosition").val();
                data.BuyingRightsAmount = form.find("#BuyingRightsAmount").val();
                data.BuyingRightsDate = form.find("#BuyingRightsDate_").val();
                data.InitialPayment = form.find("#InitialPayment").val();
                data.InitialPaymentDate = form.find("#InitialPaymentDate_").val();
                data.PaidByLumpSum = document.getElementById("PaidByLumpSum").checked;
                data.LumpSumAmount = form.find("#LumpSumAmount").val();
                data.LumpSumPaidDate = form.find("#LumpSumPaidDate_").val();
                data.RemunerationRemarks = form.find("#RemunerationRemarks").val();
                data.ClaimedDate = form.find("#ClaimedDate_").val();

                this.inventorSelected = this.inventorSelected.filter(item => item.InventorInvID !== parseInt(id))
                this.inventorSelected.push(data);

                //const template = kendo.template($("#remunerationInventorTemplate").html());
                //const result = template(data);

                //const oldRow = $(`#inventionInventorRemunerationInventorsInfoList tr[data-id=${id}]`);
                //$(result).insertBefore(oldRow[0]);
                //oldRow.remove();
            }
        }

        const inventorInvsToSave = this.inventorSelected;//grid.dataSource.data().filter(r => r.dirty);

        if (inventorInvsToSave.length > 0) {
            const container = $("#inventorListDialog");
            const saveError = container.data("save-error");
            const saveMsg = container.data("save-message");
            const saveMsgAll = container.data("save-message-all");

            window.pageHelper.hideErrors();
            const saveUrl = container.data("save-url");

            const list = [];
            this.inventorSelected.forEach(e => {
                const inventorInv = this.mapRawDataToInventorInv(e);
                this.inventorFormatDatesForServer(inventorInv);
                list.push(inventorInv);
            });

            $.post(saveUrl, { updated: list })
                .done(() => {
                    const msg = inventorInvsToSave.length === 1 ? saveMsg : saveMsgAll;
                    this.inventorSelected = [];
                    grid.dataSource.read();
                    this.setInventorListEntryNotDirty();
                    this.updateRecordStamps();
                    pageHelper.showSuccess(msg);
                })
                .fail(() => {
                    pageHelper.showErrors(saveError);
                });

        }
    }

    markinventorListEntryDirty(el) {
        $("#inventorListExport").addClass("d-none");
        $("#inventorListSaveAll").removeClass("d-none");
        $("#inventorListCancelAll").removeClass("d-none");
        $("#inventionInventorRemunerationInventorsInfoList_pager").hide();
        cpiBreadCrumbs.markLastNode({ dirty: true });
        this.recordNavigator.hide();

        if (el !== undefined) {
            const parent = $($(el).parents("tr")[0]);
            const id = parent.data("id");
            const record = $("#inventionInventorRemunerationInventorsInfoList").find(`tr[data-id=${id}]`);
        }
    }
    setInventorListEntryNotDirty() {
        $("#inventorListExport").removeClass("d-none");
        $("#inventorListSaveAll").addClass("d-none");
        $("#inventorListCancelAll").addClass("d-none");
        $("#inventionInventorRemunerationInventorsInfoList_pager").show();
        window.cpiStatusMessage.hide();
        cpiBreadCrumbs.markLastNode({ dirty: false });
        this.recordNavigator.show();
    }

    handleFormChanges(form) {
        const self = this;

        form.on("input", "input,textarea", (e) => {
            self.markinventorListEntryDirty(e.target);
        });
        form.on("change", "input[type='checkbox']", (e) => { self.markinventorListEntryDirty(e.target); });
        form.find(".k-combobox > input").each(function () {
            const comboBox = $(this).data("kendoComboBox");
            if (comboBox) {
                comboBox.bind("change", (e) => {
                    self.markinventorListEntryDirty(e.sender.element[0]);
                });
            }
        });

        form.find("input[data-role='multicolumncombobox']").each(function () {
            const comboBox = $(this).data("kendoMultiColumnComboBox");
            if (comboBox) {
                comboBox.bind("change", function () {
                    self.markinventorListEntryDirty(e.sender.element[0]);
                });
            }
        });

        form.find(".k-dropdownlist > input").each(function () {
            const dropdownList = $(this).data("kendoDropDownList");
            if (dropdownList) {
                dropdownList.bind("change", (e) => {
                    self.markinventorListEntryDirty(e.sender.element[0]);
                });
            }
        });
        form.find(".k-datepicker input").each(function () {
            const datePicker = $(this).data("kendoDatePicker");
            if (datePicker) {
                datePicker.bind("change", (e) => {
                    self.markinventorListEntryDirty(e.sender.element[0]);
                });
            }
        });
    }

    genTempRelatedCaseId() {
        const date = new Date();
        const hour = date.getHours();
        const mins = date.getMinutes();
        const secs = date.getSeconds();
        const ms = date.getMilliseconds();

        const id = parseInt([hour, mins, secs, ms].join("")) * -1;
        return id;
    }

    inventorFormatDatesForServer(e) {
        if (e.BuyingRightsDate && typeof e.BuyingRightsDate !== "string") {
            e.BuyingRightsDate = pageHelper.cpiDateFormatToSave(e.BuyingRightsDate);
        }
        if (e.LumpSumPaidDate && typeof e.LumpSumPaidDate !== "string") {
            e.LumpSumPaidDate = pageHelper.cpiDateFormatToSave(e.LumpSumPaidDate);
        }
        if (e.FRFirstPaymentDate && typeof e.FRFirstPaymentDate !== "string") {
            e.FRFirstPaymentDate = pageHelper.cpiDateFormatToSave(e.FRFirstPaymentDate);
        }
        if (e.FRSecondPaymentDate && typeof e.FRSecondPaymentDate !== "string") {
            e.FRSecondPaymentDate = pageHelper.cpiDateFormatToSave(e.FRSecondPaymentDate);
        }
        if (e.FRThirdPaymentDate && typeof e.FRThirdPaymentDate !== "string") {
            e.FRThirdPaymentDate = pageHelper.cpiDateFormatToSave(e.FRThirdPaymentDate);
        }
        if (e.DateCreated && typeof e.DateCreated !== "string") {
            e.DateCreated = pageHelper.cpiDateFormatToSave(e.DateCreated);
        }
        if (e.LastUpdate && typeof e.LastUpdate !== "string") {
            e.LastUpdate = pageHelper.cpiDateFormatToSave(e.LastUpdate);
        }
        if (e.ClaimedDate && typeof e.ClaimedDate !== "string") {
            e.ClaimedDate = pageHelper.cpiDateFormatToSave(e.ClaimedDate);
        }
        if (e.InitialPaymentDate && typeof e.InitialPaymentDate !== "string") {
            e.InitialPaymentDate = pageHelper.cpiDateFormatToSave(e.InitialPaymentDate);
        }
    }


    mapRawDataToInventorInv(data) {
        const inventorInv = {
            InventorInvID: data.InventorInvID ? parseInt(data.InventorInvID) : 0,
            Inventor: data.Inventor ? data.Inventor : "",
            Position: data.Position ? data.Position : null,
            PositionId: data.PositionId ? data.PositionId : 0,
            Percentage: data.Percentage ? data.Percentage : 0,
            PositionA: data.PositionA ? data.PositionA : 0,
            PositionB: data.PositionB ? data.PositionB : 0,
            PositionC: data.PositionC ? data.PositionC : 0,
            InventorPosition: data.InventorPosition ? data.InventorPosition : 0,

            BuyingRightsAmount: data.BuyingRightsAmount ? data.BuyingRightsAmount : 0,
            BuyingRightsDate: data.BuyingRightsDate ? new Date(data.BuyingRightsDate) : null,
            InitialPayment: data.InitialPayment ? data.InitialPayment : 0,
            InitialPaymentDate: data.InitialPaymentDate ? new Date(data.InitialPaymentDate) : null,
            PaidByLumpSum: data.PaidByLumpSum !== undefined ? data.PaidByLumpSum : false,
            LumpSumAmount: data.LumpSumAmount ? data.LumpSumAmount : 0,
            LumpSumPaidDate: data.LumpSumPaidDate ? new Date(data.LumpSumPaidDate) : null,
            RemunerationRemarks: data.RemunerationRemarks ? data.RemunerationRemarks : "",
            ClaimedDate: data.ClaimedDate ? new Date(data.ClaimedDate) : null,

            SumABC: data.SumABC ? data.SumABC : 0,
            
            OrderOfEntry: data.OrderOfEntry ? parseInt(data.OrderOfEntry) : 0,
            Remarks: data.Remarks ? data.Remarks : "",
            IsApplicant: data.IsApplicant !== undefined ? data.IsApplicant : 0,
            FRRemunerationId: data.FRRemunerationId ? parseInt(data.FRRemunerationId) : 0,
            FRFirstPayment: data.FRFirstPayment ? parseInt(data.FRFirstPayment) : 0,
            FRSecondPayment: data.FRSecondPayment ? parseInt(data.FRSecondPayment) : 0,
            FRThirdPayment: data.FRThirdPayment ? parseInt(data.FRThirdPayment) : 0,
            FRFirstPaymentDate: data.FRFirstPaymentDate ? new Date(data.FRFirstPaymentDate) : null,
            FRSecondPaymentDate: data.FRSecondPaymentDate ? new Date(data.FRSecondPaymentDate) : null,
            FRThirdPaymentDate: data.FRThirdPaymentDate ? new Date(data.FRThirdPaymentDate) : null,
            IsDirty: data.dirty,
            CreatedBy: data.CreatedBy ? data.CreatedBy : "",
            DateCreated: data.DateCreated ? new Date(data.DateCreated) : null,
            UpdatedBy: data.UpdatedBy ? data.UpdatedBy : "",
            LastUpdate: data.LastUpdate ? new Date(data.LastUpdate) : null,
            tStamp: data.tStamp ? data.tStamp : "",
        };

        return inventorInv;
    }

    inventorsInfoListDataBound = () => {
        this.stripeTableRows("inventionInventorRemunerationInventorsInfoList");
    }

    stripeTableRows(tableId) {
        let highlightRow = 3;
        $(`#${tableId} tr`).each(function (index) {
            const row = index + 1;
            if (row === highlightRow || row === highlightRow + 1) {
                $(this).find("td").addClass("listView-alt-row");

                if (row === highlightRow + 1)
                    highlightRow = highlightRow + 4;
            }
            else {
                $(this).find("td").removeClass("listView-alt-row");
            }
        });
    }

    showInventionLink = (screen, title, isReadOnly) => {
        const container = $(`#${screen}`).find(".cpiButtonsDetail");
        const pageNav = container.find(".nav");
        pageNav.prepend(`<a class="nav-link invention-link" href="#" target="_self" title="${title}" role="button"><i class="fal fa-external-link pr-2"></i>${title}</a>`);
        container.find(".invention-link").on("click", function () {
            $(`#${screen}`).find(".case-number-link").trigger("click");
        });
    }

    formatNumberToDisplay = (num) => {
        return num.toLocaleString('en-US', { minimumFractionDigits: 2 });
    }

    GetCurrentInventorInvID = (e) => {
        var data = {};
        data["inventorInvID"] = this.currentInventorInvID;
        data["remunerationId"] = document.getElementById("RemunerationId").value;;
        return data;
    }
}





