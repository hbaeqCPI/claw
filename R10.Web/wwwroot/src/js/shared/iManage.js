export default class iManage {
    constructor() {
        this.page = undefined;
        this.initLoadDetails = false;
        this.collapsedFoldersStorageName = "iManage-collapsedFolders";
        this.collapsedFolders = [];
        this.authenticating = 0;
        this.dragged = undefined;
        this.activePage = undefined;
        this.refreshDelay = 3000;
        this.selected = [];
    }

    initializeViewer = (activePage) => {
        this.activePage = activePage;
        this.page = $(`#${activePage.mainDetailContainer}`);

        const iManage = this.page.find(".imanage-work");
        if (iManage.length > 0) {
            const displayOption = iManage.find('.display-option input[name="DocumentDisplayOption"]');
            const searchForm = iManage.find(".documents .search .form");

            displayOption.on("change", (e) => {
                this.getViewer($(e.target).val());
            });
            searchForm.liveSearch(this.refineSearch);

            this.initializeToolbar();
            this.refreshViewer(displayOption.val(), searchForm.find('input[name="ContainerId"]').val());
        }
    }

    initializeToolbar = () => {
        const filters = this.page.find(".imanage-work .documents .search");
        const toolbar = this.page.find(".imanage-work .documents .toolbar .k-toolbar");
        const toggleFilters = toolbar.find(".toggle-filters");
        const toggleFiltersLabel = toggleFilters.find(".label");

        toggleFiltersLabel.text(filters.is(":hidden") ? toggleFilters.data("label-show") : toggleFilters.data("label-hide"));
        toggleFilters.off("click");
        toggleFilters.on("click", (e) => {
            e.preventDefault();

            if (filters.is(":hidden")) {
                toggleFiltersLabel.text(toggleFilters.data("label-hide"));
                filters.show();
            }
            else {
                toggleFiltersLabel.text(toggleFilters.data("label-show"));
                filters.hide();
            }
        });

        const clearFilters = toolbar.find(".clear-filters");
        clearFilters.off("click");
        clearFilters.on("click", (e) => {
            e.preventDefault();
            filters.clearSearch();
            this.updateFilterCount(filters);
            this.refreshDocuments();
        });

        this.updateFilterCount(filters);

        const saveDocument = toolbar.find(".save-document");
        saveDocument.off("click");
        saveDocument.on("click", (e) => {
            e.preventDefault();
            const el = $(e.currentTarget);
            let id = "";
            let title = window.cpiBreadCrumbs.getTitle();
            let folderId = "";

            if (el.hasClass("select-single") && this.selected.length == 1) {
                const kendoEl = this.getKendoDocumentViewer();
                const dataItem = kendoEl.dataSource.get(this.selected[0].documentId);
                id = dataItem.Id;
                title = dataItem.Title;
                folderId = dataItem.ContainerId;
            }

            this.saveDocument(id, title, el.data("url"), folderId);
        });

        const deleteDocuments = toolbar.find(".delete-documents");
        deleteDocuments.off("click");
        deleteDocuments.on("click", (e) => {
            e.preventDefault();
            if (this.selected.length == 1) {
                const kendoEl = this.getKendoDocumentViewer();
                const dataItem = kendoEl.dataSource.get(this.selected[0].documentId);
                
                this.deleteDocument(dataItem);
            }
            else
                this.deleteDocuments();
        });

        const downloadDocuments = toolbar.find(".download-document");
        downloadDocuments.off("click");
        downloadDocuments.on("click", (e) => {
            e.preventDefault();
            if (this.selected.length == 1) {
                const kendoEl = this.getKendoDocumentViewer();
                const dataItem = kendoEl.dataSource.get(this.selected[0].documentId);

                this.downloadDocument(dataItem);
            }
        });

        const mergeDocuments = toolbar.find(".merge-documents");
        mergeDocuments.off("click");
        mergeDocuments.on("click", (e) => {
            e.preventDefault();
            this.mergeDocuments();
        });

        const openDocument = toolbar.find(".open-document");
        openDocument.off("click");
        openDocument.on("click", (e) => {
            e.preventDefault();
            if (this.selected.length == 1) {
                const kendoEl = this.getKendoDocumentViewer();
                const dataItem = kendoEl.dataSource.get(this.selected[0].documentId);

                window.open(dataItem.WorkUrl);
            }
        });
    }

    refreshViewer = (displayOption, containerId) => {
        this.getFolderTree(containerId, () => {
            this.getViewer(displayOption);
            this.getDefaultImage();
        });
    }

    //DefaultImage view component
    getDefaultImage = () => {
        const defaultImage = this.page.find(".imanage-default-image");
        if (defaultImage && defaultImage.data("url")) {
            cpiLoadingSpinner.show();
            $.get(defaultImage.data("url"),)
                .done((response) => {
                    cpiLoadingSpinner.hide();
                    defaultImage.html(response);
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    this.checkError(error, () => {
                        this.getDefaultImage();
                    });
                });
        }
    }

    //Search results grid image
    getDefaultGridImage = (activePage) => {
        this.activePage = activePage;
        this.page = $(activePage.searchResultContainer);
        
        this.page.find(".imanage-default-image:not(.loaded)").each((i, e) => {
            const el = $(e);
            
            if (el.data("filename") && el.data("url")) {
                cpiLoadingSpinner.show();
                $.get(el.data("url"), { filename: el.data("filename") })
                    .done((response) => {
                        cpiLoadingSpinner.hide();
                        el.addClass("loaded");
                        el.closest(".image-default").find("i").hide();
                        el.html(response);

                        if (el.find("img").length > 0)
                            el.closest(".image").addClass("found");

                        el.show();

                        //fetch next
                        this.getDefaultGridImage(activePage);
                    })
                    .fail((error) => {
                        cpiLoadingSpinner.hide();
                        this.checkError(error, () => {
                            this.getDefaultGridImage(activePage);
                        });
                    });

                //fetch next image after $.get.done()
                return false;
            }
        })
    }

    refreshDefaultImage = () => {
        this.activePage.image.refreshDefaultImage(this.activePage).then(() => {
            this.getDefaultImage();
        });
    }

    getFolderTree = (containerId, callback) => {
        const deferred = $.Deferred();
        const iManage = this.page.find(".imanage-work");
        const sidebar = iManage.find(".side-bar");
        const documents = iManage.find(".target");
        
        cpiLoadingSpinner.show();
        $.get(sidebar.data("url"), { pageId: this.page.attr("id"), containerId: containerId, documentLink: iManage.data("document-link"), token: iManage.data("token") })
            .done((response) => {
                cpiLoadingSpinner.hide();
                if (response) {
                    sidebar.html(response);
                    
                    const searchForm = iManage.find(".documents .search .form");
                    const folderTree = sidebar.find(".folder-tree");
                    searchForm.find('input[name="ContainerId"]').val(folderTree.data("default"));

                    const setup = sidebar.find(".setup");
                    if (setup.length == 0) {
                        documents.find(".toolbar .select-active").toggle(sidebar.find(".folder.active").length > 0);
                        documents.show();
                        if (callback)
                            callback();
                    }
                }
                else
                    documents.hide();

                deferred.resolve();
            })
            .fail((error) => {
                cpiLoadingSpinner.hide();
                this.checkError(error, () => {
                    this.getFolderTree(containerId, callback);
                });
                documents.hide();
                deferred.reject();
            });

        return deferred.promise();
    };


    getViewer = (displayOption) => {
        const iManage = this.page.find(".imanage-work");
        const viewer = iManage.find(".documents .viewer");

        cpiLoadingSpinner.show();
        $.get(viewer.data("url"), { pageId: this.page.attr("id"), displayOption: displayOption, token: iManage.data("token") })
            .done((response) => {
                cpiLoadingSpinner.hide();
                viewer.html(response);
            })
            .fail((error) => {
                cpiLoadingSpinner.hide();
                this.checkError(error, () => {
                    getViewer();
                });
            });
    }

    getSetup = () => {
        const iManage = this.page.find(".imanage-work");
        const folderTree = iManage.find(".folder-tree");

        cpiLoadingSpinner.show();
        $.get(folderTree.data("setup-url"), { documentLink: iManage.data("document-link"), token: iManage.data("token") })
            .done((response) => {
                cpiLoadingSpinner.hide();
                folderTree.html(response);

                const setup = folderTree.find(".setup");
                const rootFolderId = setup.find("#DocRootContainerId");
                const rootFolderIdVal = rootFolderId.val();
                rootFolderId.trigger("focus").val("").val(rootFolderIdVal).trigger("input");
            })
            .fail((error) => {
                cpiLoadingSpinner.hide();
                this.checkError(error, () => {
                    this.getSetup();
                });
            });
    };

    initializeSetup = () => {
        const iManage = this.page.find(".imanage-work");
        const folderTree = iManage.find(".folder-tree");
        const documents = iManage.find(".target");
        const setup = folderTree.find(".setup");
        const displayOption = iManage.find('.display-option input[name="DocumentDisplayOption"]');
        const searchForm = iManage.find(".documents .search .form");

        const showViewer = () => {
            setup.removeClass("dirty");
            this.activePage.entryFormInstance.refreshGridDirtyStatus(setup, false);

            searchForm.find('input[name="ContainerId"]').val(defaultFolderId || rootContainerId);
            this.refreshViewer(displayOption.val(), defaultFolderId || rootContainerId);
        }

        let rootContainerId = setup.find("#DocRootContainerId").val();
        let defaultFolderId = setup.find("#DocDefaultFolderId").val();

        documents.hide();

        setup.floatLabels();        
        setup.find("#DocRootContainerId").on("change", this.rootContainerOnChange).trigger("focus");
        setup.find(".cancel").on("click", (e) => {
            e.preventDefault();
            showViewer();
        });
        setup.find(".save").on("click", (e) => {
            e.preventDefault();
            const url = $(e.currentTarget).data("url");
            if (url) {
                cpiLoadingSpinner.show();

                rootContainerId = setup.find("#DocRootContainerId").val();
                defaultFolderId = setup.find("#DocDefaultFolderId").val();

                $.post(url, { documentLink: iManage.data("document-link"), rootContainerId: rootContainerId, defaultFolderId: defaultFolderId, token: iManage.data("token"), __RequestVerificationToken: this.getVerificationToken() })
                    .done(() => {
                        cpiLoadingSpinner.hide();
                        folderTree.data("root", rootContainerId);
                        folderTree.data("default", defaultFolderId || rootContainerId);
                        showViewer();
                        this.refreshDefaultImage();
                    })
                    .fail((error) => {
                        cpiLoadingSpinner.hide();
                        pageHelper.showErrors(error);
                    });
            }
        });
        setup.find(".btn-new-workspace").on("click", (e) => {
            const url = $(e.currentTarget).data("url");
            const title = $(e.currentTarget).attr("title");
            if (url) {
                cpiLoadingSpinner.show();

                $.get(url, { documentLink: iManage.data("document-link"), token: iManage.data("token") })
                    .done((response) => {
                        cpiLoadingSpinner.hide();
                        cpiConfirm.save(title, response, () => {
                            const input = $(".new-workspace").find("#WorkspaceName");
                            const validation = $(".new-workspace").find("#WorkspaceName-error");
                            const workspaceName = input.val();

                            if (!workspaceName) {
                                validation.closest(".field-validation-error").show();
                                input.addClass("input-validation-error");
                                input.trigger("focus");
                                throw validation.text();
                            }

                            cpiLoadingSpinner.show();
                            $.post(url, { documentLink: iManage.data("document-link"), token: iManage.data("token"), workspaceName: workspaceName })
                                .done((response) => {
                                    cpiLoadingSpinner.hide();
                                    setup.find("#DocRootContainerId").val("").val(response.DocRootContainerId).trigger("input");
                                    setup.find("#DocRootContainerName").val("").val(response.DocRootContainerName).trigger("input");

                                    const defaultFolder = setup.find("#DocDefaultFolderId").data("kendoDropDownList");
                                    defaultFolder.dataSource.read().then(() => {
                                        defaultFolder.value(response.DocDefaultFolderId);
                                        defaultFolder.focus();
                                        defaultFolder.open();
                                        setup.find("#DocDefaultFolderId").resetFloatLabel();
                                    });

                                })
                                .fail((error) => {
                                    cpiLoadingSpinner.hide();
                                    pageHelper.showErrors(error);
                                });
                            
                        }, false, () => {
                            setup.find("#DocRootContainerId").trigger("focus");
                        });
                    })
                    .fail((error) => {
                        cpiLoadingSpinner.hide();
                        pageHelper.showErrors(error);
                    });
            }
        });

        //mark tab dirty
        setup.addClass("dirty");
        this.activePage.entryFormInstance.refreshGridDirtyStatus(setup, true);
    }

    initializeFolders = () => {
        const iManage = this.page.find(".imanage-work");
        const folderTree = iManage.find(".folder-tree");
        const folders = folderTree.find(".folders");
        const folder = folders.find(".folder");
        const storedCollapsedFolders = localStorage.getItem(this.collapsedFoldersStorageName);

        //call toggleFolder only when documents tab is visible
        const toggleFolder = (el) => {
            const parentFolder = el.closest("li");
            const childFolders = parentFolder.find("> ul");
            const folderId = parentFolder.find("a.folder").data("folder");

            if (childFolders.is(":hidden")) { //returns true if documents tab is not visible
                childFolders.show();
                el.removeClass("fa-angle-right");
                el.addClass("fa-angle-down");

                const index = this.collapsedFolders.indexOf(folderId);
                if (index > -1)
                    this.collapsedFolders.splice(index, 1);
            }
            else {
                childFolders.hide();
                el.removeClass("fa-angle-down");
                el.addClass("fa-angle-right");

                if (this.collapsedFolders.indexOf(folderId) < 0)
                    this.collapsedFolders.push(folderId);
            }

            localStorage.setItem(this.collapsedFoldersStorageName, JSON.stringify(this.collapsedFolders));
        };

        if (storedCollapsedFolders)
            this.collapsedFolders = JSON.parse(storedCollapsedFolders);

        folder.on("click", this.selectFolder);

        folders.find(".folder-toggle .toggle").on("click", (e) => {
            e.preventDefault();
            toggleFolder($(e.currentTarget));
        });
        
        folderTree.find(".folders-toolbar .settings").on("click", (e) => {
            e.preventDefault();
            this.getSetup();
        });

        folderTree.find(".folders-toolbar .refresh").on("click", (e) => {
            e.preventDefault();
            const searchForm = iManage.find(".documents .search .form");
            this.getFolderTree(searchForm.find('input[name="ContainerId"]').val());
        });

        //restore collapsed folders
        this.collapsedFolders.forEach(function (item, index) {
            if (item) {
                const collapsedFolder = folders.find(`.folder[data-folder="${item}"]`);
                const toggle = collapsedFolder.find(".folder-toggle .toggle");
                const parentFolder = toggle.closest("li");
                const childFolders = parentFolder.find("> ul");

                if (collapsedFolder.length > 0) {
                    childFolders.hide();
                    toggle.removeClass("fa-angle-down");
                    toggle.addClass("fa-angle-right");
                }
            }
        });
    }

    //Opens iManage login screen in new tab
    //  Auth endpoint closes new tab after authentication if returnUrl is empty
    //  Auth endpoint redirects to returnUrl if not empty
    //Executes callback function after authentication
    getAuthToken = (callback) => {
        const iManage = this.page.find(".imanage-work");
        if (iManage.length > 0 && iManage.data("auth-url")) {
            const statusKey = "imanage-signin";
            const checkAuth = () => {
                const status = localStorage.getItem(statusKey);
                if (status) {
                    clearInterval(intervalId);

                    if (status == "ok" && callback)
                        callback();

                    this.authenticating -= 1;
                    if (this.authenticating == 0)
                        localStorage.removeItem(statusKey);
                }
            };
            const intervalId = setInterval(function () { checkAuth(); }, 1000);

            localStorage.setItem(statusKey, "");

            this.authenticating += 1;
            if (this.authenticating <= 1)
                window.open(iManage.data("auth-url"));
        }
        else {
            console.error("iManage authentication URL is undefined.")
        }
    }

    //iManage authentication error handler
    checkError(error, callback) {
        //Authorization Service Filter returns 401 error if user has no access token for iManage API
        if (error.status == 401 && callback)
            //Open iManage sign-in endpoint in new tab to acquire access token
            this.getAuthToken(callback);
        else
            pageHelper.showErrors(error);
    }

    selectFolder = (e) => {
        e.preventDefault();

        const el = $(e.currentTarget);
        const folderId = el.data("folder");
        const target = $(e.target);

        //folder name input
        if (target.hasClass("edit")) {
            return;
        }

        //current folder, do nothing
        if (el.hasClass("active"))
            return;

        //toggle active folder
        el.closest(".folders").find(".folder").removeClass("active");
        el.addClass("active");

        //set containerId criteria
        const searchForm = this.page.find(".imanage-work .documents .search .form");
        searchForm.find('input[name="ContainerId"]').val(folderId);

        //refresh viewer data
        this.refreshDocuments();
    }

    getKendoDocumentViewer = () => {
        const searchResults = this.page.find(".imanage-work .documents .viewer .results");
        const viewer = searchResults.find(`#${searchResults.data("name")}`)

        let kendoEl = viewer.data("kendoGrid");

        if (kendoEl === undefined)
            kendoEl = viewer.data("kendoListView");

        return kendoEl;
    }

    refreshDocuments = () => { 
        const kendoEl = this.getKendoDocumentViewer();
        if (kendoEl) {
            const dataSource = kendoEl.dataSource;
            dataSource.page(1);
            dataSource.read(); //dataSource.query does not work when ServerOperation(false)
        }
    }

    refineSearch = (el) => { 
        this.updateFilterCount(el.closest(".form"));
        this.refreshDocuments();
    }

    updateFilterCount = (searchForm) => {
        const toolbar = this.page.find(".imanage-work .documents .toolbar .k-toolbar");
        const toggleFilters = toolbar.find(".toggle-filters");
        const clearFilters = toolbar.find(".clear-filters");
        const filtersCount = $(searchForm).countFilters();
        const badge = toggleFilters.find(".total-filter-count");
        if (filtersCount > 0) {
            badge.text(filtersCount);
            badge.show();
            clearFilters.show();
        }
        else {
            badge.hide();
            clearFilters.hide();
        }
    }

    nameReadData = () => {
        const folderTree = this.page.find(".imanage-work .folder-tree");
        return { containerId: folderTree.data("root") };
    }

    defaultFolderReadData = () => {
        const setup = this.page.find(".imanage-work .folder-tree .setup");
        return { containerId: setup.find("#DocRootContainerId").val() };
    }

    rootContainerOnChange = (e) => {
        const rootContainer = $(e.currentTarget);

        cpiLoadingSpinner.show();
        $.get(rootContainer.data("url"), { containerId: rootContainer.val() })
            .done((response) => {
                cpiLoadingSpinner.hide();
                rootContainer.val(response.id);
                const setup = this.page.find(".imanage-work .folder-tree .setup");
                setup.find("#DocDefaultFolderId").data("kendoDropDownList").dataSource.read();
                setup.find("#DocRootContainerName").val(response.name);
            })
            .fail((error) => {
                cpiLoadingSpinner.hide();
            });
    }

    updateSelectedActions = () => {
        const toolbar = this.page.find(".imanage-work .documents .toolbar .k-toolbar");

        toolbar.find(".select-single").toggle(this.selected.length == 1);
        toolbar.find(".select-multi").toggle(this.selected.length > 0);
    }
    
    viewerReadData = (e) => {
        const searchForm = this.page.find(".imanage-work .documents .search .form :input");
        const criteria = pageHelper.formDataToCriteriaList(searchForm);
        return { criteria: criteria.payLoad };
    }
    
    viewerOnDataBound = (e) => {
        this.selected = [];
        this.updateSelectedActions();

        const kendoEl = e.sender;
        const items = kendoEl.items();
        
        items.each((i, row) => {
            $(row).addClass("document-item");
        });
    }
    
    viewerOnChange = (e) => {
        const kendoEl = e.sender; 
        const selected = kendoEl.select();

        this.selected = [];
        for (const row of selected) {
            const dataItem = kendoEl.dataItem(row);
            this.selected.push({ documentId: dataItem.Id, folderId: dataItem.ContainerId });
        }

        this.updateSelectedActions();
    }

    viewerOnRequestEnd = (e) => {        
    }

    viewerOnError = (e) => {
        this.checkError(e.xhr, () => {
            this.refreshDocuments();
        });
    }

    documentsOnDragOver = (event) => {
        event.preventDefault();
        
        //ignore dragged folder-tree folder
        if (this.dragged && this.dragged.data("folder"))
            return;

        //ignore dragged items inside target
        if (this.dragged && this.dragged.closest(".target").length > 0)
            return;
            
        //no active folder
        if (this.page.find(".imanage-work .folder-tree").find(".folder.active").length == 0)
            return;

        const documents = $(event.target).closest(".target");
        documents.addClass("drag-over");
    }

    documentsOnDragLeave = (event) => {
        event.preventDefault();

        //ignore dragged folder-tree folder
        if (this.dragged && this.dragged.data("folder"))
            return;

        const documents = $(event.target).closest(".target");
        documents.removeClass("drag-over");
    }
    
    documentsOnDrop = (event) => {
        event.preventDefault();

        //ignore dragged folder-tree folder
        if (this.dragged && this.dragged.data("folder"))
            return;

        //ignore dragged items inside target
        if (this.dragged && this.dragged.closest(".target").length > 0)
            return;

        const documents = $(event.target).closest(".target");
        documents.removeClass("drag-over");

        const folderTree = this.page.find(".imanage-work .folder-tree");
        const activeFolder = folderTree.find(".folder.active");

        //no active folder
        if (activeFolder.length == 0)
            return;

        //cannot upload to workspace
        if (activeFolder.data("type") == "Workspace") {
            if (folderTree.data("ws-error"))
                cpiAlert.warning(pageHelper.formatString(folderTree.data("ws-error"), activeFolder.find(".name").text()));

            //cancel upload from mailbox
            if (event.dataTransfer.getData("messageIds"))
                localStorage.setItem(event.dataTransfer.getData("targetId"), "cancel");

            return;
        }

        const searchForm = this.page.find(".imanage-work .documents .search .form");
        this.dropOnUpload(event, searchForm.find('input[name="ContainerId"]').val());
    }

    //open gleamtech document viewer
    documentOnClick = (event) => {
        event.preventDefault();
        const el = $(event.target);
        const iManage = this.page.find(".imanage-work");
        const documents = iManage.find(".documents");
        const url = documents.data("viewer-url");

        cpiLoadingSpinner.show()
        $.get(url, { id: el.data("document-id"), fileName: el.text(), token: iManage.data("token") })
            .done((result) => {
                cpiLoadingSpinner.hide();
                cpiAlert.open({ title: el.text(), message: result, extraLargeModal: true });
            })
            .fail((error) => {
                cpiLoadingSpinner.hide();
                this.checkError(error, () => {
                    this.documentOnClick(event);
                });
            });
    }

    //move documents to folder
    documentOnDragStart = (event) => {
        this.dragged = $(event.target);
        
        const kendoEl = this.getKendoDocumentViewer();
        const dataItem = kendoEl.dataItem(this.dragged.closest(".document-item"));
        const dragImage = this.page.find(".imanage-work .drag-image");

        let ids = [];
        if (this.selected.length > 0 && this.selected.find(i => i.documentId == dataItem.Id))
            ids = this.selected;
        else
            ids.push({ documentId: dataItem.Id, folderId: dataItem.ContainerId });

        if (ids.length == 1)
            dragImage.html(`<i class="${dataItem.IconClass} pr-1"></i>${dataItem.Title}`);
        else
            dragImage.html(`<i class="fal fa-files pr-1"></i><span class="badge badge-pill badge-info">${ids.length}</span>`);

        event.dataTransfer.setDragImage(dragImage[0], -13, 5);
        event.dataTransfer.setData("documentIds", JSON.stringify(ids));
    }

    //move folder-tree folder
    folderOnDragStart = (event) => {
        const folder = $(event.target).closest(".folder");

        event.dataTransfer.setData("folderId", folder.data("folder"));
        event.dataTransfer.setData("folderName", folder.find(".name").text());
        event.dataTransfer.setData("folderParentId", folder.closest(".child-folders").closest(".child-folder").find(".folder").first().data("folder"));        

        this.dragged = folder;
    }

    //drag+drop file to folder-tree folder
    folderOnDragEnd = (event) => {
        this.dragged = undefined;
    }

    //drag+drop file to folder-tree folder
    folderOnDragOver = (event) => {
        event.preventDefault();
        const folder = $(event.target).closest(".folder");
        const activeFolder = folder.closest(".folders").find(".folder.active");

        event.dataTransfer.dropEffect = "none";

        //ignore move document to active folder
        if (folder.hasClass("active") && this.dragged.data("document-id"))
            return;

        //ignore move document to workspace
        if (folder.data("type") == "Workspace" && this.dragged.data("document-id"))
            return;

        if (this.dragged && this.dragged.data("folder"))
            event.dataTransfer.dropEffect = "move";

        else if (this.dragged && this.dragged.data("document-id")) {
            if (event.ctrlKey)
                event.dataTransfer.dropEffect = "copy";
            else if (event.shiftKey)
                event.dataTransfer.dropEffect = "move"; //create reference is not supported in API
            else
                event.dataTransfer.dropEffect = "move";
        }

        folder.addClass("drag-over");
    }

    //drag+drop file to folder-tree folder
    folderOnDragLeave = (event) => {
        event.preventDefault();
        const folder = $(event.target).closest(".folder");
        folder.removeClass("drag-over");
    }

    //drag+drop file to folder-tree folder
    folderOnDrop = (event) => {
        event.preventDefault();
        const folder = $(event.target).closest(".folder");

        //move document to folder
        if (event.dataTransfer.getData("documentIds")) {
            //ignore move document to active folder
            if (folder.hasClass("active"))
                return;

            //ignore move document to workspace
            if (folder.data("type") == "Workspace")
                return;
        }

        folder.removeClass("drag-over");

        //cannot upload to workspace
        if (!event.dataTransfer.getData("folderId") && folder.data("type") == "Workspace") {
            const folderTree = $(event.target).closest(".folder-tree");
            if (folderTree.data("ws-error"))
                cpiAlert.warning(pageHelper.formatString(folderTree.data("ws-error"), folder.find(".name").text()));

            //cancel upload from mailbox
            if (event.dataTransfer.getData("messageIds"))
                localStorage.setItem(event.dataTransfer.getData("targetId"), "cancel");

            return;
        }

        this.dropOnUpload(event, folder.data("folder"));
    }

    dropOnUpload = (event, destinationFolderId) => {
        const folders = this.page.find(".imanage-work .folder-tree .folders");
        const destinationFolder = folders.find(`.folder[data-folder='${destinationFolderId}']`);

        const refreshDocuments = () => {
            setTimeout(() => {
                const activeFolder = folders.find(".folder.active");

                if (activeFolder.data("folder").toLowerCase() == destinationFolderId.toLowerCase()) {
                    this.refreshDocuments();
                } else {
                    destinationFolder.trigger("click");
                }
            }, this.getDelay());
        }

        //move documents to folder
        if (event.dataTransfer.getData("documentIds")) {
            this.moveDocuments(event);
            return;
        }

        //move folder within folder-tree
        if (event.dataTransfer.getData("folderId")) {
            this.dropFolder(event);
            return;
        }

        //upload email from mailbox
        if (event.dataTransfer.getData("mailbox")) {
            this.dropMail(event, destinationFolderId, refreshDocuments);
            return;
        }

        //upload files
        this.dropFiles(event, destinationFolderId, refreshDocuments);            
    }

    dropOnSuccess = (event, refreshDocuments) => {
        pageHelper.handleSignatureWorkflow(event.response, () => {
            if (refreshDocuments)
                refreshDocuments();
        });
        pageHelper.handleEmailWorkflow(event.response);
    }

    dropMail = (event, destinationFolderId, refreshDocuments) => {
        pageHelper.hideErrors();

        event.data = { folderId: destinationFolderId };
        this.activePage.image.dropMail(event, (e) => {
            this.dropOnSuccess(e, refreshDocuments);
        });
    }

    moveDocuments = (event) => {
        const destinationFolder = $(event.target).closest(".folder");
        const ids = JSON.parse(event.dataTransfer.getData("documentIds"));
        const iManage = this.page.find(".imanage-work");
        const searchForm = iManage.find(".documents .search .form");
        const documents = iManage.find(".documents");        
        let url = documents.data("move-url");

        if (event.ctrlKey)
            url = documents.data("copy-url");
        else if (event.shiftKey)
            url = documents.data("move-url"); //create reference is not supported in API

        //ignore move document to workspace
        if (destinationFolder.data("type") == "Workspace")
            return;            

        pageHelper.hideErrors();   

        if (url) {
            cpiLoadingSpinner.show();
            $.post(url, { ids: ids, destinationFolderId: destinationFolder.data("folder"), token: iManage.data("token"), __RequestVerificationToken: this.getVerificationToken() })
                .done(() => {
                    cpiLoadingSpinner.hide();
                    searchForm.find('input[name="ContainerId"]').val(destinationFolder.data("folder"));
                    const displayOption = iManage.find('.display-option input[name="DocumentDisplayOption"]');
                    this.refreshViewer(displayOption.val(), destinationFolder.data("folder"));
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error);
                });
        }
    }

    dropFolder = (event) => {
        const folderTree = $(event.target).closest(".folder-tree");
        const folder = folderTree.find(`.folder[data-folder='${event.dataTransfer.getData("folderId")}']`);
        const destinationFolder = $(event.target).closest(".folder");      

        pageHelper.hideErrors();

        //cannot move parent folder to child folder
        if (folder.closest("li").find(`.folder[data-folder='${destinationFolder.data("folder")}']`).length > 0) {
            const error = folderTree.data("move-error");
            if (error)
                cpiAlert.warning(pageHelper.formatString(error, folder.find(".name").text(), destinationFolder.find(".name").text()));

            return;
        }

        const iManage = this.page.find(".imanage-work");
        const searchForm = iManage.find(".documents .search .form");

        cpiLoadingSpinner.show();
        $.post(folderTree.data("move-url"), { folderId: folder.data("folder"), destinationFolderId: destinationFolder.data("folder"), token: iManage.data("token"), __RequestVerificationToken: this.getVerificationToken() })
            .done(() => {
                cpiLoadingSpinner.hide();
                searchForm.find('input[name="ContainerId"]').val(folder.data("folder"));
                const displayOption = iManage.find('.display-option input[name="DocumentDisplayOption"]');
                this.refreshViewer(displayOption.val(), folder.data("folder"));
            })
            .fail((error) => {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(error);
            });
    }

    dropFiles = (event, destinationFolderId, refreshDocuments) => {
        const dropZone = $(event.target).closest(".drop-zone");
        const formData = new FormData();
        formData.append("folderId", destinationFolderId);

        if (event.dataTransfer.items) {
            // Use DataTransferItemList interface to access the file(s)
            [...event.dataTransfer.items].forEach((item, i) => {
                // If dropped items aren't files, reject them
                if (item.kind === "file") {
                    const file = item.getAsFile();
                    formData.append("droppedFiles", file, file.name);
                }
            });

        }
        else {
            // Use DataTransfer interface to access the file(s)
            [...event.dataTransfer.files].forEach((file, i) => {
                formData.append("droppedFiles", file, file.name);
            });
        }

        pageHelper.hideErrors();
        cpiLoadingSpinner.show();
        $.ajax({
            type: "POST",
            url: dropZone.data("upload-url"),
            headers: { "RequestVerificationToken": this.getVerificationToken() },
            data: formData,
            processData: false,
            contentType: false,
            success: (response) => {
                cpiLoadingSpinner.hide();
                this.dropOnSuccess({ response: response }, refreshDocuments);
            },
            error: (error) => {
                cpiLoadingSpinner.hide();
                pageHelper.showErrors(error);
                refreshDocuments();
            }
        });
    }

    folderMenuOnOpen = (e) => {
        const menu = e.sender;
        const folder = $(e.target);
        const isFolder = folder.data("type") == "Folder";
        const hasDocuments = folder.data("has-documents");
        const hasSubfolders = folder.data("has-subfolders");
        const isDefault = folder.data("folder") == this.page.find(".imanage-work .folder-tree").data("default");
        
        menu.enable("#delete", isFolder && !hasDocuments && !hasSubfolders);
        menu.enable("#rename", isFolder);
        menu.enable("#default", isFolder && !isDefault);
    }

    folderMenuOnSelect = (e) => {
        e.preventDefault();
        const iManage = this.page.find(".imanage-work");
        const folderTree = iManage.find(".folder-tree");
        const searchForm = iManage.find(".documents .search .form");
        const displayOption = iManage.find('.display-option input[name="DocumentDisplayOption"]');
        
        const folder = $(e.target);
        const selected = $(e.item);
        const data = { id: folder.data("folder"), documentLink: iManage.data("document-link"), token: iManage.data("token"), __RequestVerificationToken: this.getVerificationToken() };
        let url = selected.find(".k-menu-link").attr("href");

        const submit = (callback) => {
            cpiLoadingSpinner.show();
            $.post(url, data)
                .done(() => {
                    cpiLoadingSpinner.hide();

                    if (typeof callback === "function")
                        callback();
                    else
                        this.getFolderTree(searchForm.find('input[name="ContainerId"]').val());
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    cpiAlert.warning(pageHelper.getErrorMessage(error));
                    
                    this.getFolderTree(searchForm.find('input[name="ContainerId"]').val());
                });
        }

        const saveFolderName = (targetFolder) => {
            const folderName = targetFolder.find(".name");
            const folderIcon = targetFolder.find(".icon");
            const displayName = folderName.text();

            targetFolder.removeClass("folder"); //disable context menu
            folderIcon.hide();
            folderName.html(`<input class="form-control new-name edit" type='text' value='${folderName.text()}'><button type="button" class="save-button edit">Save</button>`);

            setTimeout(function () {
                const newName = folderName.find(".new-name");

                newName.focus().select();
                newName.on("focusout", function (e) {
                    if ($(e.relatedTarget).hasClass("save-button") && $(this).val().trim() && displayName != $(this).val()) {
                        data.name = $(this).val();
                        newName.focus().select();
                        submit();
                    }
                    else {
                        folderName.data("new-name", $(this).val());
                        folderName.text(displayName);

                        targetFolder.addClass("folder");
                        folderIcon.show();

                        folder.closest("li").find(".new-child-folders").remove();
                        folder.closest("li").find(".child-folders .new-child-folder").remove();
                    }
                });
            }, 100);
        }        

        switch (selected.attr("Id")) {
            case "create":
                let childFolders = folder.closest("li").find(".child-folders").first();
                if (childFolders.length == 0) {
                    childFolders = $(`<ul class="new-child-folders nav nav-pills cpi-nav-vertical flex-column child-folders"></ul>`);
                    folder.closest("li").append(childFolders);
                }
                childFolders.prepend(`<li class="new-child-folder nav-item child-folder"><a href="#" class="new-folder nav-link"><span class="name"></span></a></li>`);
                
                data.parentContainerType = folder.data("type");

                saveFolderName(childFolders.find(".child-folder .new-folder"));
                break;

            case "rename":
                saveFolderName(folder);
                break;

            case "default":
                url = folderTree.data("default-folder-url")
                submit(() => {
                    folderTree.data("default", folder.data("folder"));
                    searchForm.find('input[name="ContainerId"]').val(folder.data("folder"));
                    this.refreshViewer(displayOption.val(), folder.data("folder"));
                });
                break;

            case "delete":
                let callback = submit;
                
                //refresh viewer if deleting active folder
                if (folder.hasClass("active")) {                    
                    callback = () => {
                        submit(() => {
                            searchForm.find('input[name="ContainerId"]').val(folderTree.data("root"));
                            this.refreshViewer(displayOption.val(), folderTree.data("root"));
                        })
                    };
                }
                
                cpiConfirm.warning(window.cpiBreadCrumbs.getTitle(), pageHelper.formatString(selected.data("delete-confirm"), folder.find(".name").text()), callback)
                break;
        }
    }

    getVerificationToken = () => {
        const form = this.page.find(".imanage-work").closest("form");
        return form.find("input[name=__RequestVerificationToken]").val();
    }

    gridOnCommand = (e) => {
        e.preventDefault();
        const el = $(e.target);
        const row = el.closest("tr");
        const grid = row.closest(".k-grid").data("kendoGrid");
        const dataItem = grid.dataItem(row);

        switch (e.data.commandName) {
            case "Edit":
                this.saveDocument(dataItem.Id, dataItem.Title, this.page.find(".imanage-work .documents .toolbar .k-toolbar .save-document.edit").data("url"), dataItem.ContainerId);
                break;

            case "Delete":
                this.deleteDocument(dataItem);
                break;

            case "Download":
                this.downloadDocument(dataItem);
                break;

            case "Open":
                window.open(dataItem.WorkUrl);
                break;
        }
    }

    initializeEditor = () => {
        const docEditor = $(".document-editor");
        docEditor.floatLabels();
    }

    formFileOnSelect = (e) => {
        //clear validation error
        e.sender.element.closest(".file-upload").find(".field-validation-valid").removeClass("field-validation-error").text("");

        //hide name/type
        e.sender.element.closest("form").find(".doc-filename").hide();
    }

    formFileOnRemove = (e) => {
        //show name/type
        e.sender.element.closest("form").find(".doc-filename").show();
    }

    showEditor = (title, content, onSuccess, onError) => {
        cpiConfirm.save(title, content, (e) => {
            const docEditor = $(".document-editor");
            const form = docEditor.find("form");
            const remarks = docEditor.find("#Remarks").val();
            const formFile = docEditor.find("#FormFile");
            const files = formFile.data("kendoUpload").getFiles();
            const payLoad = pageHelper.formDataToJson(form).payLoad;
            const formData = new FormData();
            const self = this;

            //validate required file
            if (files.length == 0 && formFile.data("required")) {
                formFile.closest(".file-upload").find(".field-validation-valid").addClass("field-validation-error").text(formFile.data("required"));
                throw formFile.data("required");
            }

            for (const key in payLoad) {
                if (payLoad.hasOwnProperty(key)) {
                    //split array values
                    if (Array.isArray(payLoad[key])) {
                        for (const item of payLoad[key]) {
                            formData.append(key, item);
                        }
                    }
                    else {
                        formData.append(key, payLoad[key]);
                    }
                }
            }

            if (files.length > 0)
                formData.append("FormFile", files[0].rawFile);

            if (remarks)
                formData.append("Remarks", remarks);

            var actGrid = $("#documentVerificationActGrid").data("kendoGrid");
            //show error if IsActRequired = true and Action grid is empty
            if (formData && formData.has('IsActRequired') && formData.get('IsActRequired') === 'true' && actGrid) {
                var gridData = actGrid.dataSource.data();
                if (gridData && gridData.length <= 0) {
                    var actGridElement = $("#documentVerificationActGrid");
                    actGridElement.closest(".document-verification").find(".field-validation-valid").addClass("field-validation-error").text(actGridElement.data("required"));
                    throw actGridElement.data("required");                   
                }                
            }

            cpiConfirm.keepOpen = true;

            const actGridDefer = $.Deferred();
            //auto save changes on DocVerification Action grid            
            if (actGrid && actGrid.dataSource.hasChanges()) {
                $.when(pageHelper.kendoGridSave({
                    name: 'documentVerificationActGrid'
                    //popup windows closes after saving, no need to refresh
                    //,afterSubmit: () => { $("#documentVerificationActGrid").data("kendoGrid").dataSource.read(); }
                }))
                .then(
                    function (e) {                  
                        actGridDefer.resolve();
                    },
                    function (e) {                        
                        actGridDefer.reject(e);                        
                    },
                    null
                );                
            }
            else {                
                actGridDefer.resolve();
            }

            $.when(actGridDefer).done(function() {
                pageHelper.hideErrors();
                cpiLoadingSpinner.show();                
                $.ajax({
                    type: form[0].method,
                    url: form[0].action,
                    headers: { "RequestVerificationToken": self.getVerificationToken() },
                    data: formData,
                    processData: false,
                    contentType: false,
                    success: (response) => {
                        cpiConfirm.close();
                        cpiLoadingSpinner.hide();
                        if (onSuccess)
                            onSuccess(response);                        
                    },
                    error: (error) => {                        
                        cpiLoadingSpinner.hide();
                        pageHelper.showErrors(error);                        
                        if (onError)
                            onError(error);                        
                    }
                });
            })
            .fail(function(error) {                
                pageHelper.showErrors(error);
            });

        }, true, null, true);
    };

    //Upload document from RMS
    saveDocumentRMS = (activePage, url, item, docId) => {
        this.activePage = activePage;
        this.page = $(activePage.searchResultContainer);

        if (url) {
            cpiLoadingSpinner.show();
            $.get(url, { tmkId: item.TmkId, actId: item.ActId, dueId: item.DueId, rmsDocId: docId })
                .done((content) => {
                    cpiLoadingSpinner.hide();
                    this.showEditor(window.cpiBreadCrumbs.getTitle(), content, (response) => {
                        this.activePage.refreshPage().then(() => { //refresh rms tickler
                            this.dropOnSuccess({ response: response });
                            if (response.success)
                                pageHelper.showSuccess(response.success);
                        });
                    }, (error) => {});
                })
                .fail((error) => {
                    this.authenticating = 0; //force login screen
                    cpiLoadingSpinner.hide();
                    this.checkError(error, () => {
                        this.saveDocumentRMS(activePage, url, item, docId);
                        this.getDefaultGridImage(activePage); //refresh rms tickler images
                    });
                });
        }
    }

    //Upload document from Foreign Filing
    saveDocumentFF = (activePage, url, item, docId) => {
        this.activePage = activePage;
        this.page = $(activePage.searchResultContainer);

        if (url) {
            cpiLoadingSpinner.show();
            $.get(url, { invId: item.InvId, appId: item.AppId, actId: item.ActId, dueId: item.DueId, ffDocId: docId })
                .done((content) => {
                    cpiLoadingSpinner.hide();
                    this.showEditor(window.cpiBreadCrumbs.getTitle(), content, (response) => {
                        this.activePage.refreshPage().then(() => { //refresh ff tickler
                            this.dropOnSuccess({ response: response });
                            if (response.success)
                                pageHelper.showSuccess(response.success);
                        });
                    }, (error) => { });
                })
                .fail((error) => {
                    this.authenticating = 0; //force login screen
                    cpiLoadingSpinner.hide();
                    this.checkError(error, () => {
                        this.saveDocumentFF(activePage, url, item, docId);
                    });
                });
        }
    }

    saveDocument = (id, title, url, folderId) => {
        const iManage = this.page.find(".imanage-work");
        const folderTree = iManage.find(".folder-tree");
        const activeFolder = folderTree.find(".folder.active"); 

        const refreshDocuments = () => {
            setTimeout(() => {
                this.refreshDocuments();
            }, this.getDelay());
        }

        //cannot upload to workspace
        if (activeFolder.data("type") == "Workspace" && !folderId) {
            if (folderTree.data("ws-error"))
                cpiAlert.warning(pageHelper.formatString(folderTree.data("ws-error"), activeFolder.find(".name").text()));

            return;
        }

        if (url) {
            cpiLoadingSpinner.show();
            $.get(url, { iManageDocId: id, iManageFolderId: folderId || activeFolder.data("folder"), token: iManage.data("token") })
                .done((response) => {
                    cpiLoadingSpinner.hide();

                    this.showEditor(title, response, (e) => {
                        this.dropOnSuccess({ response: e }, refreshDocuments);
                        this.refreshDefaultImage();
                    }, (error) => {
                        refreshDocuments();
                    });
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    this.checkError(error, () => {
                        getViewer();
                    });
                });
        }
    }

    deleteDocument = (dataItem) => {
        const iManage = this.page.find(".imanage-work");
        const documents = iManage.find(".documents");
        const url = documents.data("delete-url");

        if (url) {
            this.confirmDelete(url,
                pageHelper.formatString(documents.data("delete-confirm"), dataItem.Title),
                { id: dataItem.Id, token: iManage.data("token"), __RequestVerificationToken: this.getVerificationToken() }
            );
        } 
    }

    deleteDocuments = () => {
        const iManage = this.page.find(".imanage-work");
        const documents = iManage.find(".documents");
        const url = documents.data("delete-multi-url");
        const ids = this.selected.map((item) => { return item.documentId });

        if (url) {
            this.confirmDelete(url, pageHelper.formatString(documents.data("delete-multi-confirm"), this.selected.length),
                { ids: ids, token: iManage.data("token"), __RequestVerificationToken: this.getVerificationToken() }
            );
        }
    }

    confirmDelete = (url, message, data) => {
        const deferred = $.Deferred();
        cpiConfirm.warning(window.cpiBreadCrumbs.getTitle(), message, () => {
            cpiLoadingSpinner.show();
            $.post(url, data)
                .done(() => {
                    setTimeout(() => {
                        cpiLoadingSpinner.hide();
                        const kendoEl = this.getKendoDocumentViewer();
                        kendoEl.dataSource.read().then(() => {
                            deferred.resolve();
                        });
                    }, this.getDelay());
                    this.refreshDefaultImage();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error);
                    deferred.reject();
                });
        });

        return deferred.promise();
    }

    downloadDocument = (dataItem) => {
        const iManage = this.page.find(".imanage-work");
        const documents = iManage.find(".documents");
        const url = documents.data("download-url");

        pageHelper.fetchReport(url, dataItem.Id, this.getVerificationToken(), dataItem.Title);
    }

    mergeDocuments = () => {
        const iManage = this.page.find(".imanage-work");
        const documents = iManage.find(".documents");
        const viewUrl = documents.data("merge-view-url");
        const mergeUrl = documents.data("merge-url");

        const folderTree = iManage.find(".folder-tree");
        const activeFolder = folderTree.find(".folder.active");

        if (this.selected.length <= 1) {
            pageHelper.showErrors(documents.data("merge-file-count-error"));
            return;
        }

        const kendoEl = this.getKendoDocumentViewer();
        const selectedDocumentIds = this.selected.map(item => item.documentId); 
        const selectedDocs = kendoEl.dataSource.data().filter(item => selectedDocumentIds.includes(item.Id)); 
                
        const nonPdfFiles = selectedDocs.filter(f => !f.Title.endsWith(".pdf"));
        if (nonPdfFiles && nonPdfFiles.length > 0) {
            pageHelper.showErrors(documents.data("merge-file-type-error"));
            return;
        }

        //cannot upload to workspace
        if (activeFolder.data("type") == "Workspace") {
            if (folderTree.data("ws-error"))
                cpiAlert.warning(pageHelper.formatString(folderTree.data("ws-error"), activeFolder.find(".name").text()));

            return;
        }

        const iManageFolderId = activeFolder.data("folder");

        cpiLoadingSpinner.show();
        $.get(viewUrl)
            .done((result) => {
                cpiLoadingSpinner.hide();
                //clear all existing hidden popups to avoid kendo id issue
                $(".site-content .popup").empty();
                const popupContainer = $(".site-content .popup").last();
                popupContainer.html(result);
                const container = $("#documentMergeDialog");
                container.modal("show");

                var imageMergeGrid = container.find("#imageMergeGrid");
                if (imageMergeGrid) {
                    $(imageMergeGrid).data("kendoGrid").setDataSource(selectedDocs);
                    $(imageMergeGrid).data("kendoGrid").dataSource.pageSize(selectedDocs.length);
                }

                container.on("click", "#mergeDocument", (e) => {
                    e.preventDefault();
                    let mergedDocName = "";
                    const mergedDocNameInput = container.find("#MergedDocName");

                    if (mergedDocNameInput && mergedDocNameInput.length > 0)
                        mergedDocName = mergedDocNameInput[0].value;

                    const error = container.find("#MergedDocName-error");
                    const errorSpan = $(error).closest(".field-validation-error");

                    if (mergedDocName > "") {
                        const selectedMergeDocs = $(imageMergeGrid).data("kendoGrid").dataSource.data();
                        const orderedDocs = selectedMergeDocs.map(m => ({ Id: m.Id, Title: m.Title }));

                        cpiLoadingSpinner.show();
                        $.post(mergeUrl, { iManageFolderId: iManageFolderId, mergedDocName: mergedDocName, docList: orderedDocs })
                            .done(function (result) {
                                cpiLoadingSpinner.hide();
                                container.modal("hide");
                                kendoEl.dataSource.read();
                                pageHelper.showSuccess(result);
                            })
                            .fail((error) => {
                                cpiLoadingSpinner.hide();
                                pageHelper.showErrors(error)
                            });

                    }
                    else {
                        errorSpan.show();
                        $(mergedDocNameInput).addClass("input-validation-error");
                        return;                             
                    }
                });

            })
            .fail((e) => {
                cpiLoadingSpinner.hide();
                this.showErrors(e.responseText);
            });
    }

    getDelay = () => {
        //include_subtree=true returns STALE data. use this.refreshDelay as workaround
        const searchForm = this.page.find(".imanage-work .documents .search .form");
        const includeSubfolders = searchForm.find("#IncludeSubFolders");
        return includeSubfolders.is(":checked") ? this.refreshDelay : 0;
    }
}