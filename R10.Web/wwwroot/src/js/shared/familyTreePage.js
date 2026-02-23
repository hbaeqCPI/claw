// common family tree scripts

export default class FamilyTree {

    constructor() {
        this.screenName = "";
        this.isPatent = false;
        this.isTrademark = false;
    }

    initFamilyTree(container) {
        const parent = $(`#${container}`);
        parent.find(".cpiFamilyTreeLink").on('click', function (e) {            
            $.get(parent.data("url-family")).done(function (result) {
                //$(".cpiContainerPopup").empty();
                //var popupContainer = $(".cpiContainerPopup").last();
                const popupContainer = $(".site-content .popup");
                popupContainer.empty();
                popupContainer.html(result);
                var dialog = $("#familyTreeDialog");
                dialog.modal("show");
            }).fail(function (error) {
                page.showErrors(error.responseText);
            });
        });
    }

    // ---------- patent-specific 
    initFamilyTreeDialogPat = (familyScreen) => {
        const self = this;

        $('input[name="ftvOption"]').change(function () {
            const ftvOptionChoice = $(this).val();
            switch (ftvOptionChoice) {
                case "F":
                    $('#FamilyNumberFTV').show();
                    $('#InventionFTV').hide();
                    $('#CtryAppFTV').hide();
                    break;
                case "I":
                    $('#FamilyNumberFTV').hide();
                    $('#InventionFTV').show();
                    $('#CtryAppFTV').hide();
                    break;
                case "C":
                    $('#FamilyNumberFTV').hide();
                    $('#InventionFTV').hide();
                    $('#CtryAppFTV').show();
            }
            self.updateParamType(ftvOptionChoice); 
        });

        this.screenName = familyScreen;
        this.isPatent = true;
        this.refreshNodeDetail();
    }

    // ---------- trademark-specific
    initFamilyTreeDialogTmk = (familyScreen) => {
        $('input[name="ftvOption"]').change(function () {
            const ftvOptionChoice = $(this).val();
            switch (ftvOptionChoice) {
                case "F":
                    $('#TrademarkNameFTV').show();
                    $('#CaseNumberFTV').hide();
                    break;
                case "C":
                    $('#TrademarkNameFTV').hide();
                    $('#CaseNumberFTV').show();
                    break;
                
            }
            self.updateParamType(ftvOptionChoice); 
        });

        this.screenName = familyScreen;
        this.isTrademark = true;
        this.refreshNodeDetail();
    }

    // ---------- common logic
    onTreeComboChange = () => {
        const option = $('input[type=radio][name="ftvOption"]:checked');
        const comboName = option.attr("id").replace("option","");

        const combo = $("#" + comboName + "_" + this.screenName).data("kendoComboBox");
        const comboValue = combo.value();
        if (comboValue === null || comboValue === "") return;

        const comboType = option.val();

        this.updateParamValue(comboValue);         // update tree filter 'paramValue' - contains either family number/trademark name, casenumber (for patent), appid/tmkid
        this.updateParamType(comboType);

        this.loadNewTree();                        // load new tree 
        combo.value(null);                         // empty combo selection
    }


    loadNewTree() {
        var tree = $("#familyTree").data("kendoTreeView");              // refresh tree
        tree.dataSource.read();
        this.refreshNodeDetail();

        var treeView = $("#familyTreeDiagram").data("kendoDiagram");
        treeView.dataSource.read();
    }

            
    refreshNodeDetail() {
        const url = $("#familyTreeSection").data("family-tree-detail-url");
        $.ajax({
            url: url,
            type: "GET",
            dataType: "html",
            data: this.dataFilter(),
            success: function (data) {
                $("#familyTreeDetailSection").html(data);
                //const actionGrid = $("#familyActionsGrid").data("kendoGrid");             // no need for this, the action grid automatically refreshes
                //actionGrid.dataSource.read();
            }
        });
    }


    onTreeNodeSelect = (e) => {
        const tree = $("#familyTree").data("kendoTreeView").dataItem(e.node);
        const nodeId = parseInt(tree.id);
        const nodeType = nodeId === 0 ? "F" : nodeId < 0 ? "I" : "C";
        this.updateParamType(nodeType);
        this.updateParamValue(nodeType === "F" ? tree.text : tree.id);

        this.refreshNodeDetail();
    }


    onTreeNodeDrop = (e) => {
        const tree = $('#familyTree').data('kendoTreeView');
        const sourceItem = tree.dataItem(e.sourceNode);
        const sourceId = parseInt(sourceItem.id);
        let destItem = tree.dataItem(e.destinationNode);

        if (e.dropPosition !== "over") {
            destItem = destItem.parentNode();
        }
        const destId = parseInt(destItem.id);

        // check if operation is valid, exit if not
        if (this.isPatent && !(sourceId > 0 && destId > 0 || sourceId < 0 && destId === 0 || sourceId > 0 && destId < 0)) {
            e.setValid(false);
            return;
        }


        const url = $("#familyTreeSection").data("family-tree-dragdrop-url");
        $.ajax({
            url: url,
            type: "POST",
            data: {
                childId: sourceItem.id,
                newParentId: destItem ? destItem.id : 0,
                parentInfo: destItem.text
            }
        });
    }

    onTreeNodeDragStart = (e) => {
        if ($(e.sourceNode).parentsUntil(".k-treeview", ".k-item").length === 0) {
            //alert("You cannot drag the root node!");
            e.preventDefault();
        }
    }

    updateParamType(paramType) {
        $("#familyTreeParamType").val(paramType);                       // change treeview filters
    }


    updateParamValue(paramValue) {
        $("#familyTreeParamValue").val(paramValue);                       // change treeview filters
    }

    updateParamForPrint() {
        const tree = $("#familyTree").data("kendoTreeView");

        const treeData = tree.dataItem(tree.select());
        const treeRootData = tree.dataItem($("#familyTree").find('.k-first'));
        const nodeId = parseInt(treeData.id);
        const nodeType = nodeId === 0 ? "F" : nodeId < 0 ? "I" : "C";
        $("#familyTreeParamRecordType").val(nodeType);

        $("#familyTreeParamFamilyNumber").val(treeRootData.text);
        $("#familyTreeParamID").val(treeData.id);
    }

    //print
    submitFamilyForm(form) {
    cpiLoadingSpinner.show();
    const url = form.data("url")
    const json = pageHelper.formDataToJson(form);

    fetch(url, {
        method: "POST",
        headers: {
            Accept: "arraybuffer",
            "Content-Type": "application/json",
        },
        body: JSON.stringify(json.payLoad)
    })
        .then(response => {
            if (!response.ok)
                throw response;

            return response.blob();
        })
        .then(data => {
            cpiLoadingSpinner.hide();
            if (document.getElementById("ReportFormat").value == 4) {
                const a = document.createElement("a");
                document.body.appendChild(a);
                const blobUrl = window.URL.createObjectURL(data);
                a.href = blobUrl;
                a.target = "_blank"
                a.click();
                setTimeout(() => {
                    window.URL.revokeObjectURL(blobUrl);
                    document.body.removeChild(a);
                }, 0);
            }
            else {
                const a = document.createElement("a");
                document.body.appendChild(a);
                const blobUrl = window.URL.createObjectURL(data);
                a.href = blobUrl;
                a.download = "Family Tree Print Screen";
                a.click();
                setTimeout(() => {
                    window.URL.revokeObjectURL(blobUrl);
                    document.body.removeChild(a);
                }, 0);
            }
        })
        .catch(error => {
            cpiLoadingSpinner.hide();
            if (error.status >= 500)
                pageHelper.showErrors("Unhandled error.");
            else
                error.text().then(errorMessage => {
                    pageHelper.showErrors(errorMessage);
                })
        });
}

    dataFilter() {
        return {
            paramType: $("#familyTreeParamType").val(),
            paramValue: $("#familyTreeParamValue").val()
        };
    }

    /* Family diagram */
    familyVisualTemplate = (options) => {
        var dataviz = kendo.dataviz;
        var g = new dataviz.diagram.Group();
        var dataItem = options.dataItem;

        //Add rectangle layout/container
        g.append(new dataviz.diagram.Rectangle({
            width: 300,
            height: 120,
            stroke: {
                width: 1
            }
            //fill: "#81aaac"
        }));

        //Prepare hyperlink for records
        const baseUrl = $("body").data("base-url");
        var recordUrl = "";
        if (dataItem.Type == "I")
            recordUrl = `${baseUrl}/Patent/Invention/DetailLink/actualValue`;
        if (dataItem.Type == "C")
            recordUrl = `${baseUrl}/Patent/CountryApplication/DetailLink/actualValue`;
        if (dataItem.Type == "T")
            recordUrl = `${baseUrl}/Trademark/TmkTrademark/DetailLink/actualValue`;

        if (dataItem.Type != "F")
            recordUrl = recordUrl.replace('actualValue', dataItem.Id);

        //Add CaseNumberCtrySub label with hyperlink
        var caseText = new dataviz.diagram.TextBlock({
            text: dataItem.Text,
            x: 5,
            y: 10,
            fontWeight: "bold",
            //color: "#fff"
        });

        if(dataItem.Type != "F")
            caseText.drawingElement.url = recordUrl;
        g.append(caseText);

        //Add Client label with hyperlink
        var layoutClient = new dataviz.diagram.Layout(new dataviz.diagram.Rect(5, 30, 300, 30), {
            alignContent: "center",
            spacing: 1
        });
        g.append(layoutClient);
        var textsClient = dataItem.Client.split(" ");
        for (var i = 0; i < textsClient.length; i++) {
            var detailTextClient = new dataviz.diagram.TextBlock({
                text: textsClient[i],
                x: 5,
                y: 20,
                //color: "#fff"
            });
            detailTextClient.drawingElement.url = recordUrl;

            layoutClient.append(detailTextClient);
        }
        layoutClient.reflow();

        //Add Title label with hyperlink
        var layoutTitle = new dataviz.diagram.Layout(new dataviz.diagram.Rect(5, 20, 300, 115), {
            alignContent: "center",
            spacing: 4
        });
        g.append(layoutTitle);
        var textsTitle = dataItem.Title.split(" ");
        for (var i = 0; i < textsTitle.length; i++) {
            var detailTextTitle = new dataviz.diagram.TextBlock({
                text: textsTitle[i],
                x: 5,
                y: 20,
                //color: "#fff"
            });
            detailTextTitle.drawingElement.url = recordUrl;

            layoutTitle.append(detailTextTitle);
        }
        layoutTitle.reflow();

        return g;
    }

    //expandNodes(treeId) {
    //    const tree = $("#" + treeId).data("kendoTreeView");
    //    //const expandPath = $("#expandPath").val().split("|");
    //    //tree.expandPath(expandPath);                                  // this does not seem to expand the path properly
    //    tree.expand(".k-item")
    //}
    
}