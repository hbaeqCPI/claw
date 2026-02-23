
export default class RelatedPatentPage {

    constructor() {
        //super();
    }

    initialize = (gridName, id) => {
        $(document).ready(() => {

            const relatedPatentsGrid = $(`#${gridName}`);

            if (relatedPatentsGrid) {
                relatedPatentsGrid.find(".k-grid-toolbar").on("click",
                    ".k-grid-Add",
                    () => {
                        const url = $(`#${gridName}`).parent().data("url-add-related-patent");
                        const data = {
                            parentId: id,
                        };

                        this.openPatentPopup("relatedPatentSelectionGrid", gridName, "relatedPatentAddDialog", id, 0, url, data, true);
                    })
            }
        });

    }

    // #region related patent page
    relatedPatentsGridDataBound = (gridName, id) => {
        const relatedPatentsGrid = $(`#${gridName}`);

        const grid = relatedPatentsGrid.data("kendoGrid");
        const data = grid.dataSource.data();
        if (data.length > 0) {
            const listView = grid.element;
            $.each(listView.find(".tmkNameSearchResult-collapsible"), function () {
                $(this).textOverflow();
            });
        }
    }

    editRelatedPatentsGridRow = (e, gridName) => {
        const grid = $(`#${gridName}`).data("kendoGrid");
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));

        const url = $(`#${gridName}`).parent().data("url-edit-related-patent");
        const data = {
            keyId: dataItem.KeyId,
            parentId: dataItem.ParentId
        };

        this.openPatentPopup("relatedPatentSelectionGrid", gridName, "relatedPatentAddDialog", dataItem.ParentId, dataItem.KeyId, url, data, true);
    }

    getRelatedPatentSources = () => {
        const grid = $("#relatedPatentSelectionGrid").data("kendoGrid");
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
            const url = `${baseUrl}/FileViewer/GetImage?system=Patent&filename=${imgPath}&screenCode=tmk&key=${tmkId}`;
            $("#defaultImage").prop("src", url);
            $("#imageWindow").show();
        }
    }

    openPatentPopup = (gridName, parentGridName, dialogContainerName, parentId, keyId, url, data, closeOnSave) => {
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
                    self.getRelatedPatentSources();
                });

                dialogContainer.find("#save").on("click", function () {
                    let form = $(`#${dialogContainerName}`).find("form")[0];
                    form = $(form);
                    const searchInvention = form.find("#searchInvention").is(':checked');
                    console.log(searchInvention);

                    const el = $(`#${gridName}`);
                    const grid = el.data("kendoGrid");
                    const selectedKeys = grid.selectedKeyNames();
                    const saveUrl = $(this).data("url");
                    const saveData = {
                        parentId: parentId,
                        keyId: keyId,
                        from: selectedKeys,
                        invention: searchInvention
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
                    //self.getRelatedPatentSources();
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

    // related patent Add page
    getRelatedPatentSources = () => {
        const grid = $("#relatedPatentSelectionGrid").data("kendoGrid");
        grid._selectedIds = {};
        grid.clearSelection();
        grid.dataSource.read();
    }

    relatedPatentSelectionChange = (parent) => {
        const addButton = $(`#${parent}`).find("#save");

        const grid = $("#relatedPatentSelectionGrid").data("kendoGrid");
        if (grid.selectedKeyNames().length > 0)
            addButton.removeAttr("disabled");
        else
            addButton.attr("disabled", "disabled");
    }

    getRrelatedPatentParam = (dialogContainerName) => {
        let form = $(`#${dialogContainerName}`).find("form")[0];
        form = $(form);

        const param = {
            caseNumber: form.find("input[name = 'CaseNumber']").data("kendoComboBox").text(),
            country: form.find("input[name = 'Country']").data("kendoComboBox").text(),
            subCase: form.find("input[name = 'SubCase']").data("kendoComboBox").text(),
            appNumber: form.find("input[name = 'AppNumber']").data("kendoComboBox").text(),
            patNumber: form.find("input[name = 'PatNumber']").data("kendoComboBox").text(),
            //classIdList: form.find("select[name = 'ClassId']").data("kendoMultiSelect").value(),
            //goods: form.find("input[name = 'Goods']").val(),
            title: form.find("input[name = 'AppTitle']").data("kendoComboBox").text(),
            activeOnly: form.find("#showActiveOnly").is(':checked'),
            searchInvention: form.find("#searchInvention").is(':checked'),
            parentId: form.find("#ParentId").val()
        };
            
        if (form.find("#SearchText").length > 0)
            param.searchText = form.find("#SearchText").val();

        return param;

    }

    onPatentsGridDataBound = (e) => {
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

    onPatentReplaceGridDataBound = (e) => {
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