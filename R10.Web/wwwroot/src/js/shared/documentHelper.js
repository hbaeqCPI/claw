
const treeNodeSelect = function (e) {

    const treeView = $(e.sender.element[0]).data("kendoTreeView");
    const dataItem = treeView.dataItem(e.node);
    const idArray = dataItem.id.split('|');
    const type = idArray[5];
    const screenCode = idArray[1];

    console.log(dataItem.id);
    $(e.sender.element[0]).siblings(".doc-tree-node").val($(e.node).data("uid"));

    if (type === "root" || type === "folder" || type === "doc") {
        const mainImageContainer = $(e.sender.element[0]).closest("#imageDataContainer");
        const searchContainer = mainImageContainer.find(".image-search");
        const documentLink = `${idArray[0]}|${idArray[1]}|${idArray[2]}|${idArray[3]}`;
        let treeSelection;

        mainImageContainer.find(".image-container").data("link", documentLink);
        console.log(mainImageContainer.find(".image-container").data("link"));

        searchContainer.find("#ScreenCode").val(screenCode);
        searchContainer.find("#ParentId").val(idArray[3]);

        if (type === "doc") {
            treeSelection = searchContainer.find("#DocId");
            treeSelection.val(idArray[7]);
        }
        else if (type === "folder") {
            treeSelection = searchContainer.find("#FolderId");
            searchContainer.find("#DocId").val("0");
            treeSelection.val(idArray[7]);
        }
        else{
            treeSelection = searchContainer.find("#ScreenCode");
            searchContainer.find("#DocId").val("0");
            searchContainer.find("#FolderId").val("0");
            searchContainer.find("#ParentId").val(idArray[3]);
            treeSelection.val(screenCode);
        }
        treeSelection.trigger("change");

        const gridContainer = $(mainImageContainer.find(".image-container"));
        if (type === "folder") 
            gridContainer.data("folder-id", idArray[7]);
        else if (type === "doc")
            gridContainer.data("folder-id", idArray[6]);
        else 
            gridContainer.data("folder-id", "0");
    }
    
}


const spTreeNodeSelect = function (e) {

    const treeView = $(e.sender.element[0]).data("kendoTreeView");
    const dataItem = treeView.dataItem(e.node);

    console.log("dataItem",dataItem);

    const prevNode = $(e.sender.element[0]).siblings(".sp-doc-tree-node").val();

    const mainImageContainer = $(e.sender.element[0]).closest("#imageDataContainer");    
    const imageContainer = $(mainImageContainer.find(".image-container"));

    if (prevNode || !dataItem.id.includes("sp-root")) {
        const searchContainer = mainImageContainer.find(".image-search");
        searchContainer.find("#TreeNodeId").val(dataItem.id);
        searchContainer.find("#FromTreeView").val("1");

        if (dataItem.id.includes("folder")) {
            const idArray = dataItem.id.split('|');
            imageContainer.data("folder-id", idArray[0]);
        }

        const viewOption = mainImageContainer.find('input[name="ImageViewOption"]').val();
        
        if (viewOption === "GridView") {
            const grid = $(".imageGridView").last().data("kendoGrid");
            grid.dataSource.read();
        }
        else {
            const listViewImages = $(".imageListView").last().data("kendoListView");
            listViewImages.dataSource.read();
        }
    }
    else if (dataItem.id.includes("sp-root")) {
        imageContainer.data("folder-id", "0");
    }
    $(e.sender.element[0]).siblings(".sp-doc-tree-node").val(dataItem.id);
}


const treeNodeExpand = function (e) {
    $(e.sender.element[0]).siblings(".doc-tree-node").val($(e.node).data("uid"));
}

const treeNodeDragStart = function(e) {
    // prevent root node drag
    if ($(e.sourceNode).parentsUntil(".k-treeview", ".k-item").length === 0) {
        e.preventDefault();
    }
}

const treeNodeDrop = function (e) {
    const treeView = $(e.sender.element[0]).data("kendoTreeView");
    const sourceItem = treeView.dataItem(e.sourceNode);
    const sourceId = sourceItem.id;

    let destItem = treeView.dataItem(e.destinationNode);
    if (e.dropPosition !== "over") {
        destItem = destItem.parentNode();
    }
    const destId = destItem.id;

    // exit if operation is not valid; dragging to a doc
    if (destId.includes("|doc|") || (sourceId.includes("|doc|") && destId.includes("|root|"))) {
        e.setValid(false);
        return;
    }

    const treeContainer = $(e.sender.element[0]).closest(".doc-tree-container");
    const url = treeContainer.data("drop-url");
    let data = { sourceId: sourceId, destId: destId };

    $.post(url, data)
        .fail(function (error) {
            pageHelper.showErrors(error);
        });
}

const spTreeNodeDrop = function (e) {
    const treeView = $(e.sender.element[0]).data("kendoTreeView");
    const sourceItem = treeView.dataItem(e.sourceNode);
    const sourceId = sourceItem.id;

    let destItem = treeView.dataItem(e.destinationNode);
    if (e.dropPosition !== "over") {
        destItem = destItem.parentNode();
    }
    const destId = destItem.id;

    // exit if operation is not valid; dragging to a doc
    if (destId.includes("|doc")) {
        e.setValid(false);
        return;
    }

    const treeContainer = $(e.sender.element[0]).closest(".doc-tree-container");
    const url = treeContainer.data("drop-url");
    let data = { sourceId: sourceId, destId: destId };

    $.post(url, data)
        .fail(function (error) {
            pageHelper.showErrors(error);
        });
}

//context menu
const treeMenuOpen = function (e) {
    const currentTreeNode = $(e.target);

    const treeView = $(this.target).data("kendoTreeView");
    const currentNodeId = treeView.dataItem(currentTreeNode).id;
    const newlyAddedFolder = !currentNodeId.includes("|");

    let showFolderOption = !currentNodeId.includes("doc");
    let showRenameOption = currentNodeId.includes("user") || newlyAddedFolder;
    let showDeleteOption = currentNodeId.includes("user") || newlyAddedFolder;

    const treeContainer = $(this.target).closest(".doc-tree-container");
    const labelMenuNew = treeContainer.data("label-new");
    const labelMenuRename = treeContainer.data("label-rename");
    const labelMenuDelete = treeContainer.data("label-delete");

    if (currentNodeId.includes("sp-root")) {
        showFolderOption = true;
        showRenameOption = false;
        showDeleteOption = false;
    }

    const items = [];
    if (showFolderOption)
        items.push({
            text: "<span class='fal fa-file-plus fa-fixed-width'></span>" + labelMenuNew,
            attr: { "data-action": "add" },
            encoded: false
        });

    if (showRenameOption)
        items.push({
            text: "<span class='fal fa-file-edit fa-fixed-width'></span>" + labelMenuRename,
            attr: { "data-action": "rename" },
            encoded: false
        });

    if (showDeleteOption)
        items.push({
            text: "<span class='fal fa-file-times fa-fixed-width'></span>" + labelMenuDelete,
            attr: { "data-action": "delete" },
            encoded: false
        });

    this.setOptions({
        dataSource: items
    });
}

const treeMenuSelect = function (e) {
    const selected = $(e.item);
    const action = selected.data("action");

    const treeView = $(this.target).data("kendoTreeView");
    const currentTreeNode = $(e.target);
    const currentNodeId = treeView.dataItem(currentTreeNode).id;

    switch (action) {
        case "add":
            treeNodeAdd(this.target, currentNodeId, currentTreeNode, treeView);
            break;
    
        case "rename":
            treeNodeRename(this.target, currentNodeId, currentTreeNode,treeView);
            break;

        case "delete":
            e.preventDefault();
            treeNodeDelete(this.target, currentNodeId, currentTreeNode, treeView);
            break;
    }

}

const treeNodeAdd = function (target, nodeId, currentTreeNode, treeView) {
    const treeContainer = $(target).closest(".doc-tree-container");
    const url = treeContainer.data("add-folder-url");
    const screenTitle = treeContainer.data("add-folder-label")

    // open popup window for folder name input
    const folderName = $(treeContainer).find("#treeNodeAddName");
    const treeNodeAddTemplate = kendo.template($($("#treeNodeAddTemplate")[0]).html());

    const createNewFolder = function (e) {
        e.preventDefault();

        const dialog = $(e.currentTarget).closest("[data-role=window]").getKendoWindow();
        const textbox = dialog.element.find(".k-textbox");
        const folderName = textbox.val();

        $.ajax({
            url: url,
            data: { id: nodeId, folderName: folderName },
            success: (folderData) => {
                treeView.append({
                    id: folderData.id,
                    text: folderData.text,
                    iconClass: folderData.iconClass,
                    detailAction: folderData.detailAction,
                    template: kendo.template($("#documentTree-template").html())
                }, currentTreeNode);

            },
            error: function (e) {
                if (e.responseJSON !== undefined)
                    pageHelper.showErrors(e.responseJSON);
                else
                    pageHelper.showErrors(e.responseText);
            }
        });
        dialog.close();
    }

    $("<div />")
        .html(treeNodeAddTemplate({ folderName: folderName }))
        .appendTo("body")
        .kendoWindow({
            modal: true,
            visible: false,
            title: screenTitle,
            deactivate: function () {
                this.destroy();
            }
        })
        // bind the Save button's click handler
        .on("click", ".k-primary", function (e) {
            createNewFolder(e);
        })
        .on("keyup", ".tree-node-input", function (e) {
            if (e.key === "Enter" || e.keyCode === 13) {
                createNewFolder(e);
            }
        })
        .getKendoWindow().center().open();
}

const treeNodeRename = function (target, nodeId, currentTreeNode, treeView) {
    const treeContainer = $(target).closest(".doc-tree-container");
    const url = treeContainer.data("rename-url");
    const screenTitle = treeContainer.data("rename-label");

    // open popup window for new name input
    const renameTemplate = kendo.template($($("#treeNodeRenameTemplate")[0]).html());
    const node = treeView.dataItem(currentTreeNode.closest(".k-item"));

    const renameFolderDoc = function (e) {
        e.preventDefault();

        const dialog = $(e.currentTarget).closest("[data-role=window]").getKendoWindow();
        const textbox = dialog.element.find(".k-textbox");
        const newName = textbox.val();
        node.set("text", newName);

        const data = {};
        data.id = nodeId;
        data.newName = newName;

        $.post(url, data)
            .done(function () {
                const mainImageContainer = $(target).closest("#imageDataContainer");
                const searchContainer = mainImageContainer.find(".image-search");
                const treeSelection = searchContainer.find("#FolderId");
                treeSelection.trigger("change");
            })
            .fail(function (error) {
                pageHelper.showErrors(error);
            });
        dialog.close();
    }
    
    $("<div />")
        .html(renameTemplate({ node: node }))
        .appendTo("body")
        .kendoWindow({
            modal: true,
            visible: false,
            title: screenTitle,
            deactivate: function () {
                this.destroy();
            }
        })
        // bind the Save button's click handler
        .on("click", ".k-primary", function (e) {
            renameFolderDoc(e);
        })
        .on("keyup", ".tree-node-input", function (e) {
            if (e.key === "Enter" || e.keyCode === 13) {
                renameFolderDoc(e);
            }
        })
        .getKendoWindow().center().open();
 }

// delete tree node (leaf node only)
const treeNodeDelete = function (target, nodeId, currentTreeNode, treeView) {
    const treeContainer = $(target).closest(".doc-tree-container");
    const title = treeContainer.data("delete-title");
    let content = treeContainer.data("delete-message");
    const url = treeContainer.data("delete-url");
    const confirmationUrl = treeContainer.data("delete-confirm-url");

    const deleteConfirmation = function (title, content, url) {
        cpiConfirm.delete(title, content, function () {

            const form = $(".modal-message .delete-confirm");
            if (form.length > 0) {
                $.validator.unobtrusive.parse(form);
                if (!form.valid()) {
                    form.wasValidated();
                    throw "Delete confirmation failed.";
                }
            }

            const data = {};
            data.id = nodeId;

            $.post(url, data)
                .done(function () {
                    treeView.remove(currentTreeNode);
                    const mainImageContainer = $(target).closest("#imageDataContainer");
                    const searchContainer = mainImageContainer.find(".image-search");
                    const treeSelection = searchContainer.find("#FolderId");
                    treeSelection.trigger("change");
                })
                .fail(function (error) {
                    pageHelper.showErrors(error);
                });

        });
    };

    $.get(confirmationUrl)
        .done(function (result) {
            content = `<div class="row message-wrap"><div class="col-2 text-center pl-md-4 pt-1"><i class="text-danger far fa-exclamation-triangle fa-2x"></i></div><div class="col-10"><p>${content}</p></div></div>${result}`;
            deleteConfirmation(title, content, url);
        })
        .fail(function (e) {
            pageHelper.showErrors(treeContainer.data("error-message") || "An error occurred. No updates were made.");
        });

}

export {
    treeNodeSelect, treeMenuOpen, treeMenuSelect, treeNodeDragStart, treeNodeDrop, treeNodeExpand, spTreeNodeSelect, spTreeNodeDrop
};

