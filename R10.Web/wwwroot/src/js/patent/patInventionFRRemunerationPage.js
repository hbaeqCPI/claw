import ActivePage from "../activePage";

export default class PatInventionRemunerationPage extends ActivePage {

    constructor() {
        super();
        this.currentInventorInvID = 0;
    }

    inventorsInfoGridChange = () => {
        const grid = $("#inventionInventorFRRemunerationInventorsInfoGrid").data("kendoGrid");
        const selectedItem = grid.dataItem(grid.select());
        if (selectedItem !== null) {
            var data = {};
            data["inventorInvID"] = selectedItem.InventorInvID;
            this.currentInventorInvID = selectedItem.InventorInvID;
            $('#inventionInventorFRRemunerationDistributionInfoGrid').data('kendoGrid').dataSource.read(data);
        }
    }

    GetCurrentInventorInvID = (e) => {
        var data = {};
        data["inventorInvID"] = this.currentInventorInvID;
        return data;
    }

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
            var currentDataItem = $("#inventionInventorFRRemunerationInventorsInfoGrid").data("kendoGrid").dataItem($(element).closest("tr"));
            if (!currentDataItem.PaidByLumpSum) {
                element.classList.remove("editable-cell");
            }
        });

        document.querySelectorAll('.data-LumpSumPaidDate').forEach(element => {
            var currentDataItem = $("#inventionInventorFRRemunerationInventorsInfoGrid").data("kendoGrid").dataItem($(element).closest("tr"));
            if (!currentDataItem.PaidByLumpSum) {
                element.classList.remove("editable-cell");
            }
        });

        document.querySelectorAll('.data-BuyingRightsAmount').forEach(element => {
            var currentDataItem = $("#inventionInventorFRRemunerationInventorsInfoGrid").data("kendoGrid").dataItem($(element).closest("tr"));
            if (currentDataItem.BuyingRightsDate != null) {
                element.classList.remove("editable-cell");
            }
        });

        $(".revenueRefreshLink").on("click", function () {
            var grid = $("#inventionInventorFRRemunerationProductSalesInfoGrid").data("kendoGrid");
            var currentDataItem = grid.dataItem($(this).closest("tr"));
            currentDataItem.UseOverrideRevenue = false;
            currentDataItem.dirty = true;
            const gridInfo = {
                name: "inventionInventorFRRemunerationProductSalesInfoGrid",
                isDirty: true,
                filter: { parentId: currentDataItem.FRRemunerationId },
            };
            pageHelper.kendoGridDirtyTracking(grid, gridInfo);
            $("#inventionInventorFRRemunerationProductSalesInfoGrid").find(".k-grid-Save").click();
        });
        $(".amountRefreshLink").on("click", function () {
            var grid = $("#inventionInventorFRRemunerationDistributionInfoGrid").data("kendoGrid");
            var currentDataItem = grid.dataItem($(this).closest("tr"));
            currentDataItem.UseOverrideAmount = false;
            currentDataItem.dirty = true;
            const gridInfo = {
                name: "inventionInventorFRRemunerationDistributionInfoGrid",
                isDirty: true,
                filter: { parentId: currentDataItem.InventorInvID },
            };
            pageHelper.kendoGridDirtyTracking(grid, gridInfo);
            $("#inventionInventorFRRemunerationDistributionInfoGrid").find(".k-grid-Save").click();
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
        if (matrixType == "Sum") {
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

    refreshInventorAwardGridRow = (e, grid) => {
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        e.preventDefault();
        const parent = $("#" + e.delegateTarget.id).parent();
        const url = parent.data("url-refresh") + "?id=" + dataItem.FRRemunerationId;
        const inventorInvID = dataItem.InventorInvID;

        $.post(url)
            .done(function (result) {
                console.log("I AM HERE");
                var grid = $("#inventionInventorFRRemunerationInventorsInfoGrid").data("kendoGrid");
                grid.dataSource.read();
                grid.refresh();
                pageHelper.showSuccess(result.success);                
                //screen.modal("hide");
            })
            .fail(function (e) {
                pageHelper.showErrors(e.responseText);
            });
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
}





