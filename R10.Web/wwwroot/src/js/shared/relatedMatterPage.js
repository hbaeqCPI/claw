
export default class RelatedMatterPage {

    constructor() {
        //super();
    }

    initialize = (gridName, id) => {
        $(document).ready(() => {

            const relatedMattersGrid = $(`#${gridName}`);

            if (relatedMattersGrid) {
                relatedMattersGrid.find(".k-grid-toolbar").on("click",
                    ".k-grid-Add",
                    () => {
                        const url = $(`#${gridName}`).parent().data("url-add-related-matter");
                        const data = {
                            parentId: id,
                        };

                        this.openMatterPopup("relatedMatterSelectionGrid", gridName, "relatedMatterAddDialog", id, 0, url, data, true);
                    })
            }
        });

    }

    // #region related matter page
    relatedMattersGridDataBound = (gridName, id) => {
        const relatedMattersGrid = $(`#${gridName}`);

        const grid = relatedMattersGrid.data("kendoGrid");
        const data = grid.dataSource.data();
        if (data.length > 0) {
            const listView = grid.element;
            $.each(listView.find(".tmkNameSearchResult-collapsible"), function () {
                $(this).textOverflow();
            });
        }
    }

    editRelatedMattersGridRow = (e, gridName) => {
        const grid = $(`#${gridName}`).data("kendoGrid");
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));

        const url = $(`#${gridName}`).parent().data("url-edit-related-matter");
        const data = {
            keyId: dataItem.KeyId,
            parentId: dataItem.ParentId
        };

        this.openMatterPopup("relatedMatterSelectionGrid", gridName, "relatedMatterAddDialog", dataItem.ParentId, dataItem.KeyId, url, data, true);
    }

    getRelatedMatterSources = () => {
        const grid = $("#relatedMatterSelectionGrid").data("kendoGrid");
        grid._selectedIds = {};
        grid.clearSelection();
        grid.dataSource.read();
    }

    showSharePointTmkDefaultImage = (tmkId) => {
        const element = $(`#tmk-sr-${tmkId}`);
        const displayUrl = element.data("display-url");

        if (displayUrl) {
            $("#defaultImage").prop("src", displayUrl);
            $("#imageWindow").show();
        }
    }

    hideDefaultImage = (widndow) => {
        $(`#${widndow}`).hide();
    }

    showDefaultImage = (tmkId, imgPath) => {
        if (imgPath != "null") {
            const baseUrl = $("body").data("base-url");
            const url = `${baseUrl}/FileViewer/GetImage?system=Matter&filename=${imgPath}&screenCode=tmk&key=${tmkId}`;
            $("#defaultImage").prop("src", url);
            $("#imageWindow").show();
        }
    }

    openMatterPopup = (gridName, parentGridName, dialogContainerName, parentId, keyId, url, data, closeOnSave) => {
        const self = this;

        $.ajax({
            url: url,
            data: data,
            success: function (result) {
                $(".cpiContainerPopup").empty();
                const popupContainer = $(".cpiContainerPopup").last();
                popupContainer.html(result);
                const dialogContainer = $(`#${dialogContainerName}`);
                dialogContainer.modal("show");

                dialogContainer.find(".search-submit").on("click", function () {
                    self.getRelatedMatterSources();
                });

                dialogContainer.find("#save").on("click", function () {
                    let form = $(`#${dialogContainerName}`).find("form")[0];
                    form = $(form);

                    const el = $(`#${gridName}`);
                    const grid = el.data("kendoGrid");
                    const selectedKeys = grid.selectedKeyNames();
                    const saveUrl = $(this).data("url");
                    const saveData = {
                        parentId: parentId,
                        keyId: keyId,
                        from: selectedKeys
                    };

                    $.post(saveUrl, saveData)
                        .done(function () {
                            const parentGrid = $(`#${parentGridName}`).data("kendoGrid");
                            const dialogContainer = $(`#${dialogContainerName}`);

                            var msg = el.parent().data("update-success");
                            if (saveData.from && saveData.from.length > 1) {
                                msg = el.parent().data("updates-success");
                            }
                            dialogContainer.modal("hide");
                            pageHelper.showSuccess(msg);
                            //pageHelper.updateRecordStamps(this);
                            parentGrid.dataSource.read();
                        })
                        .fail(function (error) { pageHelper.showErrors(error.responseText); });
                });

                dialogContainer.find("#showActiveOnly").change(function () {
                    //self.getRelatedMatterSources();
                });

                //let entryForm = dialogContainer.find("form")[0];
                //entryForm = $(entryForm);
                //entryForm.cpiPopupEntryForm(
                //    {
                //        dialogContainer: dialogContainer,
                //        closeOnSubmit: closeOnSave,
                //        beforeSubmit: function () {
                //        },
                //        afterSubmit: function (e) {
                //            dialogContainer.modal("hide");
                //            self.updateRecordStamps();
                //            grid.dataSource.read();
                //        }
                //    }
                //);
            },
        });
    }

    // related matter Add page
    getRelatedMatterSources = () => {
        const grid = $("#relatedMatterSelectionGrid").data("kendoGrid");
        grid._selectedIds = {};
        grid.clearSelection();
        grid.dataSource.read();
    }

    relatedMatterSelectionChange = (parent) => {
        const addButton = $(`#${parent}`).find("#save");

        const grid = $("#relatedMatterSelectionGrid").data("kendoGrid");
        if (grid.selectedKeyNames().length > 0)
            addButton.removeAttr("disabled");
        else
            addButton.attr("disabled", "disabled");
    }

    getRrelatedMatterParam = (dialogContainerName) => {
        let form = $(`#${dialogContainerName}`).find("form")[0];
        form = $(form);

        const param = {
            caseNumber: form.find("input[name = 'CaseNumber']").data("kendoComboBox").text(),
            //country: form.find("input[name = 'Country']").data("kendoComboBox").text(),
            subCase: form.find("input[name = 'SubCase']").data("kendoComboBox").text(),
            projectName: form.find("input[name = 'ProjectName']").data("kendoComboBox").text(),
            program: form.find("input[name = 'Program']").data("kendoComboBox").text(),
            //classIdList: form.find("select[name = 'ClassId']").data("kendoMultiSelect").value(),
            //goods: form.find("input[name = 'Goods']").data("kendoComboBox").text(),
            title: form.find("input[name = 'MatterTitle']").data("kendoComboBox").text(),
            activeOnly: form.find("#showActiveOnly").is(':checked'),
            parentId: form.find("#ParentId").val()
        };

        if (form.find("#SearchText").length > 0)
            param.searchText = form.find("#SearchText").val();

        return param;

    }

    onMattersGridDataBound = (e) => {
        var gridId = e.sender.element[0].id;
        var grid = $("#" + gridId).data("kendoGrid");
        var view = grid.dataSource.view();

        for (var i = 0; i < view.length; i++) {
            if (view[i].ReadOnly) {
                grid.tbody.find("tr[data-uid='" + view[i].uid + "']").addClass("read-only");
            }
        }

        const data = e.sender.dataSource.data();
        if (data.length > 0) {
            const listView = e.sender.element;
            $.each(listView.find(".tmkNameSearchResult-collapsible"), function () {
                $(this).textOverflow();
            });
        }
    }

    onMatterReplaceGridDataBound = (e) => {
        var rows = e.sender.tbody.children();
        for (var j = 0; j < rows.length; j++) {
            var row = $(rows[j]);
            row.addClass("k-alt");
        }

        const data = e.sender.dataSource.data();
        if (data.length > 0) {
            const listView = e.sender.element;
            $.each(listView.find(".tmkNameSearchResult-collapsible"), function () {
                $(this).textOverflow();

            });
        }
    }

}