

export default class DocumentPage {
    constructor() {

        this.searchFormId = "";
        this.mainContainerId = "";
        this.mainContainer = null;
        this.subSearchContainer = null;
        this.searchResultContainer = null;

        this.messageForm = null;

        this.searchGridId = "";
        this.currentGridRow = null;

        this.documentDetailContainer = null;
        this.treeContainer = null;
        this.treeView = null;
        this.currentTreeNode = null;
        this.currentNodeId = 0;

        this.treeNodeSelect = this.treeNodeSelect.bind(this);
        this.modalDialog = null;
    }

    //------------------------- SEARCH PAGE
    // search tab header - contains the system type, screen comboboxes
    initializeSearchTabHeader(searchPage) {
        this.mainContainerId = searchPage.containerId;
        this.mainContainer = $(searchPage.containerId);
        this.treeContainer = $(`${this.mainContainerId} .documentTreeMainContainer`);
        this.documentDetailContainer = $(`${this.mainContainerId} .documentDetailContainer`);

        this.subSearchContainer = $(searchPage.subSearchContainerId);
        this.searchResultContainer = $(searchPage.searchResultContainerId);

        // init context menu labels
        this.labelMenuNew = searchPage.labelMenuNew;
        this.labelMenuView = searchPage.labelMenuView;
        this.labelMenuRename = searchPage.labelMenuRename;
        this.labelMenuDelete = searchPage.labelMenuDelete;

        const self = this;


        $(searchPage.containerId).floatLabels();

        //const filterTabs = $(`${searchPage.containerId} .nav-tabs.refine-search`);
        const searchTab = $(`${searchPage.containerId} .nav-tabs:not('.nav-tabs-accordion')`);
        $(searchTab).accordionTabs();

        // open system search tab
        $(searchPage.tabName).addClass("show")
        $(searchPage.tabName).next(".accordion-content").slideDown("fast")


        //pageHelper.initializeSidebar(this);                                   // this needs: searchResultContainer, refineSearchContainer, searchResultGrid; also: searchUrl
        const showFilterButtonContainer = $(`${searchPage.containerId} .page-main .sidebar-link`);
        $(`${searchPage.containerId} .page-sidebar.refine-search`).collapsibleSidebar(showFilterButtonContainer);


        // bind system type radio button change event
        $("#DocSystemType input").on("change", function () {
            const screenCombo = $("#ScreenCode_documentSearch").data("kendoComboBox");
            //const screenCombo = self.getScreenCodeCombo();
            if (screenCombo.element.data("fetched") === 1) {
                screenCombo.element.data("fetched", 0);             // set screen combobox status for fetching so it loads new set of screens based on system type
                screenCombo.text("");                               // clear previous selection
            }
        });

        window.cpiBreadCrumbs.addNode({
            name: $(searchPage.containerId).attr("id"),
            label: searchPage.title,
            url: searchPage.url,
            refresh: true,
            updateHistory: true
        });

        pageHelper.moveBreadcrumbs(searchPage.containerId);

        // zoom to specific records, from main screens 
        const defaultScreenCode = searchPage.defaultScreenCode;
        if (defaultScreenCode > "") {
            var combo = self.getScreenCodeCombo();
            combo.dataSource.read();
            self.refreshSubSearch(defaultScreenCode, searchPage.defaultRecordId)
        }
    }

    // initialize all-containing search tab container
    initializeSearchTabMain(searchPage) {

        this.searchFormId = searchPage.searchFormId;                    // used in gridMainSearchFilters
        this.searchGridId = searchPage.searchGridId;
        this.subSearchContainer = $(searchPage.subSearchContainerId);
        this.searchResultContainer = $(searchPage.searchResultContainerId);

        const self = this;
        const searchContainer = self.mainContainer;
        const searchForm = $(searchPage.searchFormId);


        const searchButton = $(searchContainer).find(".search-submit");
        if (searchButton.length > 0) {
            searchButton.on("click", function (e) {
                e.preventDefault();
                let searchGrid = $(self.searchGridId).data("kendoGrid");
                if (searchGrid === undefined)
                    searchGrid = $(self.searchGridId).data("kendoListView");
                if (searchGrid !== undefined) {
                    searchGrid.dataSource.read()                       // refresh search results grid
                        .then(function () {
                            if (searchGrid.dataSource.data().length) {
                                searchGrid.select("tr:eq(1)");
                                //self.loadDocumentTree(this);          // duplicates result grid change event triggered by select
                            }
                            else {
                                self.resetDetailContainer();
                            }
                        });
                }
            });
        }

        const clearButton = $(searchContainer).find(".search-clear");
        if (clearButton.length > 0) {
            clearButton.on("click", function (e) {
                e.preventDefault();
                searchForm.clearSearch();
            });
        }

        $(searchPage.screenComboId).on("change", function (e) {
            if (this.value) {
                const combo = $(this).data("kendoComboBox");
                if (combo.selectedIndex === -1) {        // limit to list
                    //this._clear.click();
                    combo.value("");
                }
                else {
                    self.refreshSubSearch(this.value, "");
                    //refreshSubSearchResults(this.value);                      // move up as callback of refreshSubSearch to avoid concurrency issue with localizer loading
                }
            }
        });
    }

    refreshSubSearch = function (screenCode, recordId) {
        if (this.subSearchContainer.length > 0) {
            const self = this;
            const systemType = documentPage.getSystemTypeValue();
            const url = self.subSearchContainer.data("subsearch-url") + "?systemType=" + systemType + "&screenCode=" + screenCode;
            $.get(url)
                .done(function (response) {
                    self.subSearchContainer.empty();
                    self.subSearchContainer.html(response);
                    self.refreshSubSearchResults(screenCode, recordId);            // move here from below to avoid concurrency issue with localizer loading
                })
                .fail(function (error) {
                    pageHelper.showErrors(error);
                });
        }
    }

    refreshSubSearchResults = function (screenCode, recordId) {
        if (this.searchResultContainer.length > 0) {
            const self = this;
            const systemType = documentPage.getSystemTypeValue();
            let url = self.searchResultContainer.data("searchresults-url") + "?systemType=" + systemType + "&screenCode=" + screenCode;
            if (recordId.length) url = url + "&recordId=" + recordId;      // append specific record id

            $.get(url)
                .done(function (response) {
                    self.searchResultContainer.empty();
                    self.searchResultContainer.html(response);
                    if (recordId.length === 0)
                        self.resetDetailContainer();
                })
                .fail(function (error) {
                    pageHelper.showErrors(error);
                });
        }
    }

    resetDetailContainer = function () {
        const self = this;
        let treeTitle = self.treeContainer.data("container-label");
        self.treeContainer.empty();
        self.treeContainer.html(treeTitle);

        treeTitle = self.documentDetailContainer.data("container-label");
        self.documentDetailContainer.empty();
        self.documentDetailContainer.html(treeTitle);
    } 

    // initialize sub-search page (invention, ctryapp, tmk, etc.)
    initializeSubSearchPage(searchPage) {
        const searchContainer = this.mainContainer;
        searchContainer.floatLabels();

        // close system tab
        const systemTab = $(searchPage.systemTabId);
        systemTab.removeClass("show");
        systemTab.next(".accordion-content").slideUp("fast")

        const searchTab = $(`${this.mainContainerId} .nav-tabs:not('.nav-tabs-accordion')`);
        $(searchTab).accordionTabs();


        // open sub search tab
        const subSearchTab = $(searchPage.tabNameId);
        subSearchTab.addClass("show");
        subSearchTab.next(".accordion-content").slideDown("fast");

    }

    //------------------------- SEARCH RESULT GRID
    initializeSearchResultPage(searchPage) {
        const self = this;
        const combo = this.getScreenCodeCombo();
        const len = combo.dataSource._data.length;
        if (len > 0) {
            let i;
            for (i = 0; i < len; i++) {
                if (combo.dataSource._data[i]["Value"] === searchPage.screenCode) {
                    combo.select(i);
                    combo.value(combo.dataSource._data[i]["Value"]);
                    combo.trigger("change");
                    break;
                }
            }
        }
        const resultGrid = $(searchPage.resultGridId).data("kendoGrid");

        resultGrid.dataSource.read({ keyId: searchPage.defaultRecordId })
            .then(function () {
                if (resultGrid.dataSource.data().length) {
                    resultGrid.select("tr:eq(1)");
                    //self.loadDocumentTree(this);                      // duplicates result grid change event triggered by select
                }
                else {
                    self.resetDetailContainer();
                }
            });
    }

    //copied from searchPage.js (this page class no longer inherits from searchPage.js)
    gridMainSearchFilters = (e) => {
        //kendo will pass an object if called from datasource.Data()
        const filterContainer = typeof e === "string" ? e : this.searchFormId;
        return pageHelper.gridMainSearchFilters($(filterContainer));
    }

    searchResultGridRequestEnd = (e) => {
        cpiStatusMessage.hide();

        if (e.response) {
            $(this.refineSearchContainer).find(".total-results-count").html(e.response.Total);

            if (e.response.Data.length > 0) {
                this.mainSearchRecordIds = e.response.Ids;
            }
            else if (this.showNoRecordError) {
                const form = $(`${this.searchContainer}-MainSearch`);
                pageHelper.showErrors($(form).data("no-results") || $("body").data("no-results"));
            }
        }
    }

    searchResultGridError = (e) => {
        cpiLoadingSpinner.hide();
        pageHelper.showErrors(e.xhr.responseText || "Error retrieving search results.");
    }


    //------------------------- TREE VIEW
    // display document tree on result grid select
    loadDocumentTree = (e) => {
        if (e === null)
            return;

        const self = this;

        const confirmGridSelect = function () {
            self.currentGridRow = $(".doc-search-container .k-state-selected");

            // empty container
            self.documentDetailContainer.empty();

            //// destroy previous context menu (context menu persists even if container has been emptied)
            //const treeMenu = $("#treeMenu").data("kendoContextMenu");
            //if (treeMenu) {
            //    treeMenu.destroy();
            //}

            const url = self.treeContainer.data("tree-url");

            $.get(url)
                .done(function (response) {
                    self.treeContainer.empty();
                    self.treeContainer.html(response);
                    self.treeView = $("#documentTree").data("kendoTreeView");

                    // put root node selection inside setTimeout to ensure tree has loaded; else it will cause error when tree load has not completed
                    setTimeout(function () {
                        self.treeView.select(".k-item:first")
                        const selNode = self.treeView.select();
                        self.treeView.trigger("select", { node: selNode });
                    }, 1000)

                })
                .fail(function (error) {
                    pageHelper.showErrors(error);
                });

            cpiBreadCrumbs.markLastNode({ dirty: false });
        }

        const cancelGridSelect = function () {
            e.preventDefault();
            if (self.currentGridRow !== null) {
                $(".doc-search-container .k-state-selected").removeClass("k-state-selected");       // remove focus on newly selected row
                $(self.currentGridRow).addClass("k-state-selected");                                // move focus to previous selection
            }
        }

        if (cpiBreadCrumbs.hasDirtyNode()) {
            //const container = $(self.documentDetailContainer.find(".doc-detail-form"));
            console.log("dirty node 1");
            cpiConfirm.confirm(self.messageForm.data("cancel-title"), self.messageForm.data("cancel-message"), confirmGridSelect, null, null, cancelGridSelect);
        }
        else {
            confirmGridSelect();
        }
    }

    //------------------------- TREE CONTEXT MENU
    treeMenuOpen(e) {
        const currentTreeNode = $(e.target);
        const currentNodeId = documentPage.treeView.dataItem(currentTreeNode).id;
        //const hasChildren = documentPage.treeView.dataItem(currentTreeNode).hasChildren;

        // refresh main fields
        documentPage.currentTreeNode = currentTreeNode;
        documentPage.currentNodeId = currentNodeId;

        //const showFolderOption = !this.currentNodeId.includes("doc");
        //const showViewOption = this.currentNodeId.includes("doc");
        //const showRenameOption = !this.currentNodeId.includes("root"); 
        //const showDeleteOption = !(this.currentNodeId.includes("root") || hasChildren) || this.currentNodeId.includes("doc");

        const showFolderOption = !currentNodeId.includes("doc");
        const showViewOption = currentNodeId.includes("doc");
        const showRenameOption = currentNodeId.includes("user");
        const showDeleteOption = currentNodeId.includes("user");

        const items = [];

        if (showFolderOption)
            items.push({
                text: "<span class='fal fa-file-plus fa-fixed-width'></span>" + documentPage.labelMenuNew,
                attr: { "data-action": "add" },
                encoded: false
            });

        if (showViewOption)
            items.push({
                text: "<span class='fal fa-search fa-fixed-width'></span>" + documentPage.labelMenuView,
                attr: { "data-action": "view" },
                encoded: false
            });

        if (showRenameOption)
            items.push({
                text: "<span class='fal fa-file-edit fa-fixed-width'></span>" + documentPage.labelMenuRename,
                attr: { "data-action": "rename" },
                encoded: false
            });

        if (showDeleteOption)
            items.push({
                text: "<span class='fal fa-file-times fa-fixed-width'></span>" + documentPage.labelMenuDelete,
                attr: { "data-action": "delete" },
                encoded: false
            });

        this.setOptions({
            dataSource: items
        });
    }

    treeMenuSelect = (e) => {
        const selected = $(e.item);
        const action = selected.data("action");

        this.currentTreeNode = $(e.target);
        this.currentNodeId = this.treeView.dataItem(this.currentTreeNode).id;

        switch (action) {
            case "add":
                this.treeNodeAdd();
                break;
            case "view":
                this.treeNodeView();
                break;
            case "rename":
                this.treeNodeRename();
                break;

            case "delete":
                e.preventDefault();
                this.treeNodeDelete();
                break;
        }
    }

    treeNodeView = () => {
        const treeContainer = $("#documentTreeContainer");
        const nodeId = this.currentNodeId;
        const docUrl = treeContainer.data("doc-url") + "/" + nodeId;

        $.get(docUrl)
            .done(function (response) {
                const linkUrl = response.DocUrl;
                const docFileName = response.DocFileName;
                let fileNoPath = "";
                if (docFileName) {
                    const aDocFileName = docFileName.split("\\");
                    fileNoPath = aDocFileName[aDocFileName.length - 1];
                    if (fileNoPath) {
                        let viewerUrl = treeContainer.data("viewer-url");
                        if (viewerUrl !== undefined) {
                            viewerUrl = viewerUrl.replace("param", docFileName);
                            viewerUrl = viewerUrl.replace("nodeId", nodeId);
                            documentPage.zoomDocument(viewerUrl);
                        }
                    }
                }

                if (linkUrl) {
                    if (linkUrl.startsWith("www") || linkUrl.startsWith("http") || linkUrl.startsWith("ftp") || linkUrl.startsWith("file")) {
                        window.open(linkUrl, '_blank');
                    }
                }

                if (!linkUrl && !fileNoPath) {
                    const msg = treeContainer.data("doc-msg");
                    cpiAlert.warning(msg);
                }

            })
            .fail(function (error) {
                pageHelper.showErrors(error);
            });

    }

    // delete tree node (leaf node only)
    treeNodeDelete = () => {
        const self = this;
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
                data.id = self.currentNodeId;
                data.__RequestVerificationToken = $(self.mainContainer.find("input[name=__RequestVerificationToken]")[0]).val();

                $.post(url, data)
                    .done(function () {
                        self.treeView.remove(self.currentTreeNode);
                    })
                    .fail(function (error) {
                        pageHelper.showErrors(error);
                    });

            });
        };

        const title = this.treeContainer.data("delete-title");
        let content = this.treeContainer.data("delete-message");
        const url = this.treeContainer.data("delete-url");
        const confirmationUrl = this.treeContainer.data("delete-confirm-url");

        $.get(confirmationUrl)
            .done(function (result) {
                content = `<div class="row message-wrap"><div class="col-2 text-center pl-md-4 pt-1"><i class="text-danger far fa-exclamation-triangle fa-2x"></i></div><div class="col-10"><p>${content}</p></div></div>${result}`;
                deleteConfirmation(title, content, url);
            })
            .fail(function (e) {
                pageHelper.showErrors(this.treeContainer.data("error-message") || "An error occurred. No updates were made.");
            });

    }

    treeNodeRename = () => {
        const url = this.treeContainer.data("rename-url");
        const screenTitle = this.treeContainer.data("rename-label");
        const self = this;



        // open popup window for new name input
        const renameTemplate = kendo.template($("#treeNodeRenameTemplate").html());
        const node = this.treeView.dataItem(this.currentTreeNode.closest(".k-item"));

        const renameFolderDoc = function (e) {
            e.preventDefault();

            const dialog = $(e.currentTarget).closest("[data-role=window]").getKendoWindow();
            const textbox = dialog.element.find(".k-textbox");
            const newName = textbox.val();
            node.set("text", newName);

            const data = {};
            data.id = self.currentNodeId;
            data.newName = newName;
            data.__RequestVerificationToken = $(self.mainContainer.find("input[name=__RequestVerificationToken]")[0]).val();

            $.post(url, data)
                //.done(function () {                 
                //})
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

    treeNodeAdd = () => {
        const url = this.treeContainer.data("add-folder-url");
        const screenTitle = this.treeContainer.data("add-folder-label")
        const self = this;

        // open popup window for folder name input
        const folderName = $(`${this.mainContainerId} #treeNodeAddName`);
        const treeNodeAddTemplate = kendo.template($("#treeNodeAddTemplate").html());

        const createNewFolder = function (e) {
            e.preventDefault();

            const dialog = $(e.currentTarget).closest("[data-role=window]").getKendoWindow();
            const textbox = dialog.element.find(".k-textbox");
            const folderName = textbox.val();

            $.ajax({
                url: url,
                data: { id: self.currentNodeId, folderName: folderName },
                success: (folderData) => {
                    self.treeView.append({
                        id: folderData.id,
                        text: folderData.text,
                        iconClass: folderData.iconClass,
                        detailAction: folderData.detailAction,
                        template: kendo.template($("#documentTree-template").html())
                    }, self.currentTreeNode);

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

    // tree view node select -> refresh node detail
    treeNodeSelect = (e) => {
        const dataItem = this.treeView.dataItem(e.node);
        const detailAction = dataItem.detailAction;

        if (detailAction === null || detailAction === "")
            return;

        const self = this;

        const confirmTreeSelect = function () {

            self.currentTreeNode = e.node;
            self.currentNodeId = dataItem.id;
            //$(self.currentTreeNode).find(".k-in").addClass("k-state-selected");             // set focus on node; causes all nodes to be selected sometimes

            self.documentDetailContainer.empty();
            let url = self.documentDetailContainer.data("detail-url");
            if (url !== undefined)
                url = url.replace("Dummy", detailAction);

            $.ajax({
                url: url,
                data: { id: self.currentNodeId },
                success: (response) => {
                    self.documentDetailContainer.html(response);
                },
                error: function (e) {
                    if (e.responseJSON !== undefined)
                        pageHelper.showErrors(e.responseJSON);
                    else
                        pageHelper.showErrors(e.responseText);
                }
            });
            cpiBreadCrumbs.markLastNode({ dirty: false });
            //self.isParentDirty = false;
        }

        const cancelTreeSelect = function () {
            e.preventDefault();
            setTimeout(function () {
                $(e.node).find(".k-state-selected").removeClass("k-state-selected");        // remove focus from new node selection
            });
            $(self.currentTreeNode).find(".k-in").addClass("k-state-selected");             // return focus to previous node
        }

        // check if detail form is dirty (have changes); if yes, prompt...; else proceed to select event function
        if (cpiBreadCrumbs.hasDirtyNode()) {
            //const container = $(self.documentDetailContainer.find(".doc-detail-form"));
            console.log("dirty node 2");
            cpiConfirm.confirm(self.messageForm.data("cancel-title"), self.messageForm.data("cancel-message"), confirmTreeSelect, null, null, cancelTreeSelect);
        }
        else {
            confirmTreeSelect();
        }


    }

    treeNodeDragStart = (e) => {
        // prevent root node drag
        if ($(e.sourceNode).parentsUntil(".k-treeview", ".k-item").length === 0) {
            //alert("You cannot drag the root node!");
            e.preventDefault();
        }
    }

    treeNodeDrop = (e) => {
        const sourceItem = this.treeView.dataItem(e.sourceNode);
        const sourceId = sourceItem.id;

        let destItem = this.treeView.dataItem(e.destinationNode);
        if (e.dropPosition !== "over") {
            destItem = destItem.parentNode();
        }
        const destId = destItem.id;

        // exit if operation is not valid; invalid: fixed folder/docs; dragging to a doc
        if (sourceId.includes("|fix|") || destId.includes("|fix|") || destId.includes("|doc|") || (sourceId.includes("|doc|") && destId.includes("|root|"))) {
            e.setValid(false);
            return;
        }

        const url = this.treeContainer.data("drop-url");
        let data = { sourceId: sourceId, destId: destId };
        data.__RequestVerificationToken = $(this.mainContainer.find("input[name=__RequestVerificationToken]")[0]).val();

        $.post(url, data)
            //.done(function () {})
            .fail(function (error) {
                pageHelper.showErrors(error);
            });

    }

    //------------------------- DATA FILTERS
    getSystemTypeValue() {
        return $("#DocSystemType").find(".btn.active input")[0].value;
    }

    getSystemTypeFilter() {
        const systemType = $("#DocSystemType").find(".btn.active input")[0].value;
        return { systemType: systemType };
    }

    getScreenCodeCombo() {
        return $("#ScreenCode_documentSearch").data("kendoComboBox");
    }

    getTreeFilter = (e) => {
        const self = documentPage;

        const systemType = self.getSystemTypeValue();
        const screenCombo = self.getScreenCodeCombo();
        const screenCode = screenCombo.value();

        const searchGrid = $(documentPage.searchGridId).data("kendoGrid");
        const gridDataItem = searchGrid.dataItem(searchGrid.select());

        return { systemType: systemType, screenCode: screenCode, dataKey: gridDataItem.DataKey, dataKeyValue: gridDataItem.DataKeyValue }
    }

    getUploadFilter(folderId) {
        return { folderId: folderId }
    }

    //------------------------- FOLDER
    initializeUserFolder = (folderPage) => {

        let docTabLoaded = false;

        //set tabchange listener
        $(`#${folderPage.tabName} a`).on('click', (e) => {
            e.preventDefault();
            const tab = e.target.id;
            if (!docTabLoaded && tab === folderPage.docTab) {
                const docGrid = $("#folderDocGrid").data("kendoGrid");
                docGrid.dataSource.read();
                docTabLoaded = true;
            }

        });
    }

    refreshFolderAfterUpload = (e) => {
        this.refreshDocumentGrid(e);

        // refresh tree node too; it now has a new child
        const selectedNode = this.treeView.dataItem(this.currentTreeNode);
        selectedNode.hasChildren = true;
        this.refreshTreeNode(e);
    }

    refreshDocumentGrid = (e) => {
        if ($("#folderDocGrid").length > 0) {
            const grid = $("#folderDocGrid").data("kendoGrid");
            grid.dataSource.read();
        }
    }

    // folder detail - doc grid add
    addDocument = () => {
        const grid = $("#folderDocGrid").data("kendoGrid");
        const url = grid.dataSource.transport.options.create.url;

        this.openDocumentDialog(url);
    }

    // folder detail - doc grid edit
    editDocument(e) {
        const grid = $("#folderDocGrid").data("kendoGrid");
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));

        let url = grid.dataSource.transport.options.update.url;
        url = url + "?id=" + dataItem.DocId;

        this.openDocumentDialog(url);
    }

    // initialize from document editor popup
    initializeDocumentEditor = (modalPage) => {
        const modalDialogId = `#${modalPage.modalDialog}`;
        this.modalDialog = $(modalDialogId);
        const self = this;

        this.modalDialog.modal("show");
        //$(`${modalDialogId} input:text:visible:first`).focus();
        const input = $(".modal-body").find("*").filter(":input:visible:first");
        input.focus();

        let entryForm = this.modalDialog.find("form")[0];

        let isParentDirty = false;

        // attach jquery validator
        $.validator.unobtrusive.parse(entryForm);
        entryForm = $(entryForm);
        entryForm.data("validator").settings.ignore = "";               // include hidden fields (kendo controls)
        pageHelper.addMaxLength(entryForm);

        const submitButton = $(this.modalDialog.find("#save"));
        const setToSaveMode = function () {
            submitButton.removeClass("d-none");
        }

        const markDirty = function () {
            if (isParentDirty)
                return;
            isParentDirty = true;
            setToSaveMode();
        };

        // attach markDirty to input fields
        entryForm.on("input", ".cpiMainEntry input, .cpiMainEntry textarea", function () {
            markDirty();
        });

        entryForm.find(".cpiMainEntry .k-combobox > input").each(function () {
            const comboBox = $(this).data("kendoComboBox");
            if (comboBox) {
                comboBox.bind("change", function () {
                    markDirty();
                });
            }
        });

        $(modalDialogId).on('hide.bs.modal', function (e) {

            if (isParentDirty) {
                if (self.messageForm) {
                    cpiConfirm.confirm(self.messageForm.data("cancel-title"), self.messageForm.data("cancel-message"), function () { }, null, undefined,
                        function () { self.modalDialog.modal("show"); });    // works but hides this modal so we have to show again
                }
            }
        });

        // set tabchange listener for document preview
        let previewTabLoaded = false;
        const previewTab = $(`#${modalPage.tabName} a`);
        if (previewTab) {
            $(`#${modalPage.tabName} a`).on('click', (e) => {
                e.preventDefault();
                const tab = e.target.id;
                if (!previewTabLoaded && tab === modalPage.previewTab) {
                    const docViewerContainer = $(`#${modalPage.docViewerContainer}`);
                    const url = docViewerContainer.data("viewer-url") + "&screenCode=" + modalPage.screenCode + "&docFileName=" + modalPage.docFileName + "&key=" + modalPage.key;
                    $.ajax({
                        url: url,
                        dataType: "html",
                        cache: false,
                        beforeSend: function () { },
                        success: function (result) {
                            docViewerContainer.empty();
                            docViewerContainer.html(result);
                        },
                        error: function (e) {
                            pageHelper.showErrors(e);
                        }
                    });

                    previewTabLoaded = true;
                }

            });
        }

        const submitForm = function () {
            cpiLoadingSpinner.show();

            var form = self.modalDialog.find("form");
            var formData = new FormData(form[0]);
            $.ajax({
                type: "POST",
                url: form.attr("action"),
                data: formData,
                contentType: false, // needed for file upload
                processData: false, // needed for file upload
                success: (result) => {
                    isParentDirty = false;
                    self.modalDialog.modal("hide");
                    cpiLoadingSpinner.hide();

                    if (self.messageForm) {
                        self.refreshDocumentGrid();         // refresh the grid
                        pageHelper.showSuccess(self.messageForm.data("save-message"));
                        self.refreshTreeNode();
                    }
                    else if (modalPage.docViewer) {
                        updateDocViewer(modalPage.docViewer);
                    }
                },
                error: function (e) {
                    pageHelper.showErrors(e);
                    isParentDirty = false;
                    self.modalDialog.modal("hide");
                    cpiLoadingSpinner.hide();
                }
            });

        };

        entryForm.on("submit", function (e) {
            e.preventDefault();

            //client side validation (using jquery validation)
            if (entryForm.valid()) {
                submitForm();
            }
            else {
                cpiLoadingSpinner.hide();
                entryForm.wasValidated();
            }
        });

        const updateDocViewer = function (docViewer) {
            const screen = docViewer.toLowerCase().split("-");
            let page = null;
            switch (screen[0]) {
                case "inv":
                    page = inventionPage;
                    break;
                case "ca":
                    page = patCountryAppPage;
                    break;
                case "pcost":
                    page = costTrackingPage;
                    break;
                case "pact":
                    page = patActionDuePage;
                    break;
                case "pacti":
                    page = patActionInvDuePage;
                    break;

                case "tmk":
                    page = tmkTrademarkPage;
                    break;
                case "tcost":
                    page = tmkCostTrackingPage;
                    break;
                case "tact":
                    page = tmkActionDuePage;
                    break;

                case "gm":
                    page = gmMatterPage;
                    break;
                case "gcost":
                    page = gmCostTrackingPage;
                    break;
                case "gact":
                    page = gmActionDuePage;
                    break;
            }
            
            if (page) {

                if (screen[1] === "g")
                    page.image.refreshGrid();
                else
                    page.image.refreshListView();
            }
        };

    }

    // folder detail - doc grid delete
    deleteDocument = (e, grid) => {
        const self = this;
        const dataItem = grid.dataItem($(e.currentTarget).closest("tr"));
        pageHelper.deleteGridRow(e, dataItem, function () { self.refreshTreeNode(); });
    }

    refreshTreeNode = () => {
        const selectedNode = this.treeView.dataItem(this.currentTreeNode);
        if (selectedNode.hasChildren) {
            selectedNode.loaded(false);
            selectedNode.load();
        }
    }

    //------------------------- SHARED
    // folder/document detail edit
    initializeUserDocumentEdit = (docPage) => {
        const pageContainer = $(docPage.pageContainer);
        const saveCancelButtons = $(pageContainer.find(docPage.editActionButtonsId));
        const uploadFile = docPage.uploadFile;
        const node = this.treeView.dataItem(this.currentTreeNode.closest(".k-item"));
        const self = this;

        let entryForm = pageContainer.find("form")[0];
        this.messageForm = $(entryForm);

        // reset dirty flag
        let isParentDirty = false;

        // attach jquery validator
        $.validator.unobtrusive.parse(entryForm);
        entryForm = $(entryForm);
        entryForm.data("validator").settings.ignore = "";               // include hidden fields (kendo controls)
        pageHelper.addMaxLength(entryForm);

        const setToSaveMode = function () {
            saveCancelButtons.removeClass("d-none");
        }

        const setToViewMode = function () {
            isParentDirty = false;
            saveCancelButtons.addClass("d-none");
            cpiBreadCrumbs.markLastNode({ dirty: false });
        };

        const markDirty = function () {
            if (isParentDirty)
                return;
            isParentDirty = true;
            cpiBreadCrumbs.markLastNode({ dirty: true });
            setToSaveMode();
        };

        const refreshForm = function () {
            pageContainer.empty();
            const url = entryForm.data("refresh-url");
            $.ajax({
                url: url,
                data: { id: self.currentNodeId },
                success: (response) => {
                    pageContainer.html(response);
                },
                error: function (e) {
                    pageHelper.showErrors(e);
                }
            });
        }

        const submitAfter = function (result) {
            cpiLoadingSpinner.hide();
            pageHelper.showSuccess(self.messageForm.data("save-message"));
            setToViewMode();

            // change node text, in case updated
            node.set("text", result.newName);
            $(docPage.pageHeaderId).text(result.newName);
        }

        const submitError = function (e) {
            cpiLoadingSpinner.hide();
            pageHelper.showErrors(e);
        }

        const submitForm = function () {
            cpiLoadingSpinner.show();
            const url = entryForm.data("save-url");
            const json = pageHelper.formDataToJson(entryForm);
            pageHelper.postJson(url, json)
                .done(function (result) {
                    submitAfter(result);
                })
                .fail(function (e) {
                    submitError(e);
                });
        };


        const submitFormUpload = function () {
            cpiLoadingSpinner.show();
            const url = entryForm.data("save-url");

            var form = pageContainer.find("form")[0];
            var formData = new FormData(form);
            $.ajax({
                type: "POST",
                url: url,
                data: formData,
                contentType: false, // needed for file upload
                processData: false, // needed for file upload
                success: (result) => {
                    submitAfter(result);
                    refreshForm();
                },
                error: function (e) {
                    submitError(e);
                }
            });
        }

        // attach markDirty to input fields
        entryForm.on("input", ".cpiMainEntry input, .cpiMainEntry textarea", function () {
            markDirty();
        });

        entryForm.find(".cpiMainEntry .k-combobox > input").each(function () {
            const comboBox = $(this).data("kendoComboBox");
            if (comboBox) {
                comboBox.bind("change", function () {
                    markDirty();
                });
            }
        });

        entryForm.on("submit", function (e) {
            e.preventDefault();

            //client side validation (using jquery validation)
            if (entryForm.valid()) {
                if (uploadFile)
                    submitFormUpload();
                else
                    submitForm();
            }
            else {
                cpiLoadingSpinner.hide();
                entryForm.wasValidated();
            }
        });

        const saveButton = $(pageContainer.find(".save-changes"));

        saveButton.on("click", function (e) {
            entryForm.submit();
        })

        const cancelButton = $(pageContainer.find(".cancel-changes"));
        cancelButton.on("click", function (e) {
            cpiConfirm.confirm(self.messageForm.data("cancel-title"), self.messageForm.data("cancel-message"), function () {
                isParentDirty = false;
                cpiBreadCrumbs.markLastNode({ dirty: false });
                pageContainer.removeClass("dirty");

                refreshForm();
                cpiStatusMessage.hide();
            });
        });
    }

    // folder detail - doc grid - popup to add/edit document
    openDocumentDialog = (url, callback) => {
        $.get(url)
            .done((result) => {
                //clear all existing hidden popups to avoid kendo id issue
                const popupContainer = $(".site-content .popup");
                popupContainer.empty();
                popupContainer.html(result);
                if (callback)
                    callback();
            })
            .fail((e) => {
                pageHelper.showErrors(e);
            });
    }
    //------------------------- DOCUMENT ZOOM
    // open document zoom dialog
    zoomDocument = (url, callback) => {
        this.openDocumentDialog(url, callback);
    }

    // initialize document zoom dialog
    initializeDocumentZoom = (modalPage) => {
        const modalDialogId = `#${modalPage.modalDialog}`;
        this.modalDialog = $(modalDialogId);

        const docViewerContainer = $(`#${modalPage.docViewerContainer}`);
        const url = docViewerContainer.data("viewer-url");

        $.ajax({
            url: url,
            dataType: "html",
            cache: false,
            beforeSend: function () { },
            success: function (result) {
                docViewerContainer.empty();
                docViewerContainer.html(result);
            },
            error: function (e) {
                pageHelper.showErrors(e);
            }
        });

        this.modalDialog.modal("show");
    }
}