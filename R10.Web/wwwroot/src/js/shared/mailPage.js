import ActivePage from "../activePage";

export default class MailPage extends ActivePage {

    constructor() {
        super();
        this.initLoadDetails = false;
        this.collapsedFolders = [];
    }

    initializeSidebarPage(sidebarPage) {
        const page = $(sidebarPage.container);

        //replace sidebar opener button label and icon
        const foldersLabel = page.find(".mail .toolbar").data("folders-label");
        page.find(".page-sidebar").data("show-filters", foldersLabel);
        page.find(".page-sidebar").data("show-filters-icon", "fal fa-bars");

        //move refineSearch inside list pane
        const refineSearch = $(`${sidebarPage.form}`);
        page.find(".mail .filters").append(refineSearch.detach());

        //replace filter opener button container
        refineSearch.data("toggle-button-container", ".mail .toolbar .filter-link");

        //batch actions
        page.find(".mail .batch-action").on("click", this.applyAction)

        //call base class first to initialize activePage
        //before calling refreshMailSidebar
        super.initializeSidebarPage(sidebarPage);

        //sidebar navigation
        this.refreshMailSidebar();

        //refresh unread count every minute
        setInterval(() => {
            this.refreshUnreadItemCount(true);
        }, 60 * 1000);
    }

    selectFolder = (e) => {
        e.preventDefault();

        const el = $(e.currentTarget);
        const folder = el.data("folder");
        const page = $(this.searchResultContainer);
        const target = $(e.target);

        //folder name input
        if (target.hasClass("edit")) {
            return;
        }

        //current folder
        //do nothing
        //if (el.hasClass("active"))
        //    return;

        //toggle active folder
        el.closest(".folders").find(".folder").removeClass("active");
        el.addClass("active");

        //show list
        this.showList();

        //reset filters
        const refineSearch = $(this.refineSearchContainer);

        //set folder criteria
        refineSearch.find("#Folder").val(folder);

        //clear filters and refresh grid
        page.find(".search-clear").click();

        //hide filters
        if (refineSearch.is(":visible"))
            page.find(".toggle-filters").click();

        //clear detail
        this.clearDetails();

        //close floating sidebar
        if (this.sidebar.isFloating())
            this.sidebar.close();
    }

    getMailboxName = () => {
        return $(this.searchResultContainer).find(".mail-nav").data("mailbox");
    }

    gridMainSearchFilters = (e) => {
        const refineSearch = $(this.refineSearchContainer);

        //set mailbox criteria
        refineSearch.find("#Mailbox").val(this.getMailboxName());

        return pageHelper.gridMainSearchFilters(refineSearch);
    }

    searchResultGridDataBound = (e) => {
        const grid = e.sender;
        const data = e.sender.dataSource.data();
        const content = $(this.searchResultContainer).find(".mail .content");

        //clear
        grid._selectedIds = {};
        grid.clearSelection();
        this.clearDetails();

        if (data.length > 0) {

            if (!this.initLoadDetails && !content.is(":hidden"))
                setTimeout(function () {
                    grid.select(grid.tbody.find("tr:first"));
                }, 50);

            this.initLoadDetails = true;

            const rows = grid.tbody.find("tr");
            for (var i = 0; i < rows.length; i++) {
                const row = $(rows[i]);
                const rowData = data[i];

                if (!rowData.IsRead)
                    row.addClass("unread");

                row.attr("id", data[i].Id);

                //drag and drop parameters
                row.attr("draggable", "true");
                row.attr("ondragstart", "mailPage.dragMail(event)");
                row.attr("ondragend", "mailPage.dropMail(event)");
                row.attr("data-file-name", rowData.DownloadFileName);
                row.attr("data-sender", rowData.Name);
                row.attr("data-subject", rowData.Subject);
                row.attr("data-has-attachments", rowData.HasAttachments);
            }
        }

        //toggle from/to columns
        const activeFolder = this.getActiveFolder();

        if (activeFolder.closest(".parent-folder").find(".folder").hasClass("sent-items")) {
            grid.showColumn("ToRecipients");
            grid.hideColumn("Name");
        } else {
            grid.showColumn("Name");
            grid.hideColumn("ToRecipients");
        }

        this.refreshDetailsToolbar();
    }

    //fires when grid row is selected
    searchResultGridChange = (e) => {
        const grid = $(e.sender)[0];
        const selected = grid.selectedKeyNames();
        const batchActions = $(this.searchResultContainer).find(".mail .batch-actions");

        batchActions.hide();

        if (selected.length == 1) {
            this.getMessage(selected[0]).then(() => {
                const row = grid.select();
                //mark as read if auto mark as read is true and message is unread
                if ($(grid.element[0]).data("mark-as-read") && row.hasClass("unread"))
                    this.updateIsRead(selected[0], true);
            });
        }
        else if (selected.length > 1) {
            const activeFolder = this.getActiveFolder();

            this.clearDetails();
            batchActions.show();

            //only show restore if direct parent is Download Items folder
            //if (activeFolder.closest(".parent-folder").find(".folder").hasClass("deleted-items"))
            if (activeFolder.hasClass("deleted-items"))
                batchActions.find(".restore-message").show();
            else
                batchActions.find(".restore-message").hide();
        }
    }

    getMessage = (id) => {
        const grid = this.searchResultGrid.data("kendoGrid");
        const url = $(grid.element).data("message-url");
        const deferred = new $.Deferred();
        const dataItem = grid.dataItem(grid.select());

        if (url) {
            const mail = $(this.searchResultContainer).find(".mail");
            const verificationToken = this.getVerificationToken();
            const content = mail.find(".content");

            //clear container
            content.empty();

            //get message
            cpiLoadingSpinner.show();
            $.post(url, { id: id, mailbox: this.getMailboxName(), __RequestVerificationToken: verificationToken })
                .done((html) => {
                    cpiLoadingSpinner.hide();
                    pageHelper.hideErrors();
                    content.html(html);

                    //back
                    content.find(".show-list").on("click", (e) => {
                        e.preventDefault();

                        this.showList();
                    });

                    //expand
                    content.find(".expand-detail").on("click", (e) => {
                        e.preventDefault();

                        this.sidebar.close();
                        this.expandDetail();
                    });

                    //mark as read
                    content.find(".mark-as-read").on("click", (e) => {
                        e.preventDefault();
                        this.updateIsRead(id, true);
                    });

                    //mark as unread
                    content.find(".mark-as-unread").on("click", (e) => {
                        e.preventDefault();
                        this.updateIsRead(id, false);
                    });

                    //delete
                    content.find(".delete-message").on("click", (e) => {
                        e.preventDefault();
                        const confirm = $(e.currentTarget).data("delete-confirm");

                        if (confirm) {
                            cpiConfirm.delete(window.cpiBreadCrumbs.getTitle(), confirm, () => {
                                this.deleteMessage(id);
                            });
                        }
                        else
                            this.deleteMessage(id);
                    });

                    //restore
                    content.find(".restore-message").on("click", (e) => {
                        e.preventDefault();
                        this.restoreMessage(id);
                    });

                    //reply, reply all, forward
                    content.find(".action.editor").on("click", (e) => {
                        e.preventDefault();
                        this.showEditor(id, $(e.currentTarget).data("editor"));
                    });

                    //view document link
                    content.find(".open-document-link").on("click", (e) => {
                        e.preventDefault();
                        window.open($(e.currentTarget).data("url"), '_blank');
                    });

                    //attachments
                    content.find(".attachment").on("click", this.fetchAttachment);

                    //view downloaded document
                    content.find(".view-document").on("click", (e) => {
                        e.preventDefault();
                        this.viewDocument(e);
                    });

                    content.show();

                    //show message if hidden (small viewport)
                    if (content.is(":hidden")) {
                        this.expandDetail();
                    }

                    //show toolbar
                    this.refreshDetailsToolbar();

                    deferred.resolve();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error);

                    deferred.reject();
                });
        }
        else {
            deferred.reject();
        }

        return deferred.promise();
    }

    fetchAttachment = (e) => {
        const grid = this.searchResultGrid.data("kendoGrid");
        const verificationToken = this.getVerificationToken();
        const attachment = $(e.currentTarget);
        const payload = {};

        payload.MessageId = attachment.data("message-id");
        payload.Id = attachment.data("attachment-id");
        payload.MailboxName = this.getMailboxName();

        pageHelper.fetchReport($(grid.element).data("attachment-url"), payload, verificationToken, attachment.data("name"));
    }

    getActiveFolder = () => {
        const folders = $(this.searchResultContainer).find(".mail-nav .folders");
        return folders.find("li .folder.active");
    }

    getVerificationToken = () => {
        const form = $(this.refineSearchContainer);
        return form.find("input[name=__RequestVerificationToken]").val();
    }

    clearDetails = () => {
        const mail = $(this.searchResultContainer).find(".mail");
        mail.find(".content").empty();
    }

    refreshDetailsToolbar = () => {
        const grid = this.searchResultGrid.data("kendoGrid");
        const detailsWrap = $(this.searchResultContainer).find(".mail .details-wrap");
        const message = detailsWrap.find(".message");
        const toolbar = detailsWrap.find(".toolbar");

        if (message.length > 0) {
            toolbar.show();

            const id = message.attr('id');
            const row = grid.tbody.find(`tr[id="${id}"]`);

            if (row.hasClass("unread")) {
                toolbar.find(".mark-as-read").show();
                toolbar.find(".mark-as-unread").hide();
            }
            else {
                toolbar.find(".mark-as-read").hide();
                toolbar.find(".mark-as-unread").show();
            }
        }
        else {
            toolbar.hide();
        }
    }

    refreshUnreadItemCount = (refreshGrid) => {
        const folders = $(this.searchResultContainer).find(".mail-nav .folders");
        const url = folders.data("unread-count-url");

        if (url) {
            //cpiLoadingSpinner.show();
            $.get(url, { mailbox: this.getMailboxName() })
                .done((response) => {
                    //cpiLoadingSpinner.hide();
                    folders.find("li .folder").each((index, el) => {
                        const folder = $(el);
                        const unreadCount = folder.find(".unread-count");
                        const oldCount = unreadCount.text() || 0;
                        const data = response.find(d => d.Folder == folder.data("folder"));
                        const newCount = data ? data.UnreadItemCount : 0;
                        folder.find(".unread-count").text(newCount > 0 ? newCount : "");

                        if (refreshGrid && folder.hasClass("active") && oldCount != newCount) {
                            this.refreshGrid();
                        }
                    })
                })
                .fail((error) => {
                    //cpiLoadingSpinner.hide();
                });
        }
    }

    //refresh mail count on top of page (_LoginPartial)
    refreshMailCount = () => {
        const mailCount = $(".nav-login .badge.unread-message-count");
        const urls = [];
        if (mailCount) {
            mailCount.each((i, el) => {
                const mailbox = $(el);
                const url = mailbox.data("url");
                if (url && !urls.includes(url)) {
                    $.get(url)
                        .done((response) => {
                            $(`.nav-login .badge.unread-message-count.mailbox-${mailbox.data("mailbox")}`).text(response.UnreadItemCount > 0 ? response.UnreadItemCount : "");
                        });
                    urls.push(url);
                }
            });
        }
    }

    runMailDownloader = () => {
        const mailCount = $(".nav-login .badge.unread-message-count");
        const urls = [];
        const mailDownloader = (url) => {
            $.get(url)
                //ignore 401 to avoid auth popup page when not using sharepoint
                //.fail(function (e) {
                //    if (e.status == 401) {
                //        const baseUrl = $("body").data("base-url");
                //        const url = `${baseUrl}/Graph/SharePoint`;

                //        sharePointGraphHelper.getGraphToken(url, () => {
                //            mailDownloader(url);
                //        });
                //    }
                //});
        }

        if (mailCount) {
            mailCount.each((i, el) => {
                const mailbox = $(el);
                const url = mailbox.data("download-url");
                if (url && !urls.includes(url)) {
                    mailDownloader(url);
                }
                urls.push(url);
            });
        }
    }

    refreshGrid = () => {
        const grid = this.searchResultGrid.data("kendoGrid");
        grid.dataSource.read();
    }

    expandDetail = () => {
        const mail = $(this.searchResultContainer).find(".mail");
        const details = mail.find(".details");

        details.closest(".col").removeClass("col-md-7 d-none");
        details.addClass("full");

        mail.find(".list").closest(".col").hide();
        mail.find(".expand-detail").hide();
        mail.find(".show-list").show();
    }

    showList = () => {
        const mail = $(this.searchResultContainer).find(".mail");
        const details = mail.find(".details");

        details.closest(".col").addClass("col-md-7 d-none");
        details.removeClass("full");

        mail.find(".list").closest(".col").show();
        mail.find(".expand-detail").show();
        mail.find(".show-list").hide();
    }

    updateIsRead = (id, isRead) => {
        if (id) {
            const message = $(this.searchResultContainer).find(".mail .message");
            const verificationToken = this.getVerificationToken();
            const url = message.data("update-isread-url");

            if (url) {
                cpiLoadingSpinner.show();
                $.post(url, { id: id, mailbox: this.getMailboxName(), isRead: isRead, __RequestVerificationToken: verificationToken })
                    .done(() => {
                        cpiLoadingSpinner.hide();
                        const row = this.searchResultGrid.find(`tr[id="${id}"]`);

                        if (isRead)
                            row.removeClass("unread");
                        else
                            row.addClass("unread");

                        this.refreshUnreadItemCount();
                        this.refreshMailCount();
                        this.refreshDetailsToolbar();
                    })
                    .fail((error) => {
                        cpiLoadingSpinner.hide();
                    });
            }
        }
    }

    deleteMessage = (id) => {
        if (id) {
            const message = $(this.searchResultContainer).find(".mail .message");
            const verificationToken = this.getVerificationToken();
            const url = message.data("delete-message-url");

            if (url) {
                cpiLoadingSpinner.show();
                $.post(url, { id: id, mailbox: this.getMailboxName(), __RequestVerificationToken: verificationToken })
                    .done(() => {
                        cpiLoadingSpinner.hide();
                        this.clearDetails();
                        this.refreshUnreadItemCount();
                        this.refreshGrid();
                    })
                    .fail((error) => {
                        cpiLoadingSpinner.hide();
                        pageHelper.showErrors(error);
                    });
            }
        }
    }

    restoreMessage = (id) => {
        if (id) {
            const message = $(this.searchResultContainer).find(".mail .message");
            const verificationToken = this.getVerificationToken();
            const url = message.data("restore-message-url");

            if (url) {
                cpiLoadingSpinner.show();
                $.post(url, { id: id, mailbox: this.getMailboxName(), __RequestVerificationToken: verificationToken })
                    .done(() => {
                        cpiLoadingSpinner.hide();
                        this.clearDetails();
                        this.refreshUnreadItemCount();
                        this.refreshGrid();
                    })
                    .fail((error) => {
                        cpiLoadingSpinner.hide();
                        pageHelper.showErrors(error);
                    });
            }
        }
    }

    getEditor = (id, editor) => {
        const deferred = new $.Deferred();

        if (id) {
            const message = $(this.searchResultContainer).find(".mail .message");
            const url = message.data("reply-url");

            if (url) {
                const mail = $(this.searchResultContainer).find(".mail");
                const content = mail.find(".content");

                cpiLoadingSpinner.show();
                $.get(url, { id: id, mailbox: this.getMailboxName(), editor: editor })
                    .done((html) => {
                        cpiLoadingSpinner.hide();

                        //load editor
                        content.html(html);
                        content.show();

                        const form = content.find("form.editor");
                        //floating labels
                        form.floatLabels();

                        //show cc field
                        form.find(".more-recipients .more-cc").on("click", (e) => {
                            e.preventDefault();
                            $(e.target).hide();
                            form.find(".cc-recipients").show();
                        });

                        //show bcc field
                        form.find(".more-recipients .more-bcc").on("click", (e) => {
                            e.preventDefault();
                            $(e.target).hide();
                            form.find(".bcc-recipients").show();
                        });

                        //cancel
                        content.find(".cancel").on("click", (e) => {
                            e.preventDefault();
                            this.cancelEdit(id);
                        });

                        //send
                        content.find(".send").on("click", (e) => {
                            e.preventDefault();
                            this.sendMail(id);
                        });

                        //auto adjust html editor height
                        const htmlEditor = content.find(".k-iframe");
                        htmlEditor.removeClass("k-content");
                        htmlEditor.addClass("auto-fit");

                        //mark page as dirty
                        cpiBreadCrumbs.markLastNode({ dirty: true });

                        deferred.resolve();
                    })
                    .fail((error) => {
                        cpiLoadingSpinner.hide();
                        deferred.reject();
                    });
            }
        }
        else {
            deferred.reject();
        }

        return deferred.promise();
    }

    showEditor = (id, editor) => {
        this.getEditor(id, editor).then(() => {
            const mail = $(this.searchResultContainer).find(".mail");
            const content = mail.find(".content");

            //attachments
            content.find(".attachment").on("click", this.fetchAttachment);

            //expand
            this.sidebar.close();
            this.expandDetail();
        })
    }


    showRules = (e) => {
        const deferred = new $.Deferred();
        const el = $(e.target);
        const url = el.data("url");

        if (url) {
            cpiLoadingSpinner.show();
            $.get(url, { mailbox: this.getMailboxName() })
                .done((html) => {
                    cpiLoadingSpinner.hide();

                    const modal = $(this.searchResultContainer).find("#mailModal");
                    const content = modal.find(".modal-content");

                    //load
                    content.html(html);
                    modal.modal({ backdrop: 'static', keyboard: false });

                    //add
                    content.find(".rules .add").on("click", (e) => {
                        e.preventDefault();
                        this.editRule(e);
                    });

                    //move up
                    content.find(".rules .move-up").on("click", (e) => {
                        e.preventDefault();
                        this.moveRuleUp(e);
                    });

                    //move down
                    content.find(".rules .move-down").on("click", (e) => {
                        e.preventDefault();
                        this.moveRuleDown(e);
                    });

                    //edit
                    content.find(".rules .edit").on("click", (e) => {
                        e.preventDefault();
                        this.editRule(e);
                    });

                    //delete
                    content.find(".rules .delete").on("click", (e) => {
                        e.preventDefault();
                        this.deleteRule(e);
                    });

                    //run
                    content.find(".rules .run").on("click", (e) => {
                        e.preventDefault();
                        this.runRule(e);
                    });

                    //status
                    content.find(".rules .update-status").on("click", (e) => {
                        this.updateRuleStatus(e);
                    });

                    deferred.resolve();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    deferred.reject();
                });
        }

        return deferred.promise();
    }

    refresh = () => {
        $(this.searchResultContainer).find(".mail-nav .settings.rules").click();
    }

    updateRuleStatus = (e) => {
        const el = $(e.currentTarget);
        const rules = el.closest(".rules");
        const url = rules.data("status-url");

        if (url) {
            const rule = el.closest(".rule");
            cpiLoadingSpinner.show();
            $.post(url, {
                id: rule.data("id"),
                tStamp: rule.data("tstamp"),
                enabled: el.is(':checked'),
                __RequestVerificationToken: rules.find("input[name=__RequestVerificationToken]").val()
            })
                .done((result) => {
                    cpiLoadingSpinner.hide();
                    this.refresh();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    this.refresh();
                });
        }
    }

    runRule = (e) => {
        const el = $(e.currentTarget);
        const rules = el.closest(".rules");
        const url = rules.data("run-url");
        const icon = el.find("i");

        //rule is running
        //todo: cancel
        if (icon.hasClass("fa-spin"))
            return;

        const showSpinner = () => {
            icon.removeClass("fa-play");
            icon.addClass("fa-sync fa-spin");
        }
        const hideSpinner = () => {
            icon.addClass("fa-play");
            icon.removeClass("fa-sync fa-spin");
        }

        //check status
        const statusUrl = rules.data("download-status-url");
        cpiLoadingSpinner.show();
        $.get(statusUrl, { mailbox: this.getMailboxName() })
            .done((status) => {
                cpiLoadingSpinner.hide();

                if (url) {
                    cpiConfirm.warning(rules.data("run-title"), rules.data("run-message"), () => {
                        const rule = el.closest(".rule");
                        showSpinner();
                        $.post(url, {
                            mailbox: this.getMailboxName(),
                            id: rule.data("id"),
                            __RequestVerificationToken: rules.find("input[name=__RequestVerificationToken]").val()
                        })
                            .done((result) => {
                                hideSpinner();
                                //this.refresh();
                            })
                            .fail((error) => {
                                hideSpinner();
                                //todo: show error
                                //this.refresh();
                            });
                    });
                }
            })
            .fail((status) => {
                console.error(status);
                cpiLoadingSpinner.hide();
                cpiAlert.warning(rules.data("download-status-message"));
            });
    }

    moveRuleUp = (e) => {
        const el = $(e.currentTarget);
        const rules = el.closest(".rules");
        const url = rules.data("move-up-url");

        if (url) {
            const rule = el.closest(".rule");
            rule.after(rule.prev(".rule"));
            cpiLoadingSpinner.show();
            $.post(url, {
                id: rule.data("id"),
                tStamp: rule.data("tstamp"),
                __RequestVerificationToken: rules.find("input[name=__RequestVerificationToken]").val()
            })
                .done((result) => {
                    cpiLoadingSpinner.hide();
                    this.refresh();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    this.refresh();
                });
        }
    }

    moveRuleDown = (e) => {
        const el = $(e.currentTarget);
        const rules = el.closest(".rules");
        const url = rules.data("move-down-url");

        if (url) {
            const rule = el.closest(".rule");
            rule.before(rule.next(".rule"));
            cpiLoadingSpinner.show();
            $.post(url, {
                id: rule.data("id"),
                tStamp: rule.data("tstamp"),
                __RequestVerificationToken: rules.find("input[name=__RequestVerificationToken]").val()
            })
                .done((result) => {
                    cpiLoadingSpinner.hide();
                    this.refresh();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    this.refresh();
                });
        }
    }

    deleteRule = (e) => {
        const el = $(e.currentTarget);
        const rules = el.closest(".rules");
        const url = rules.data("delete-url");

        if (url) {
            cpiConfirm.warning(rules.data("delete-title"), rules.data("delete-message"), () => {
                const rule = el.closest(".rule");
                cpiLoadingSpinner.show();
                $.post(url, {
                    id: rule.data("id"),
                    tStamp: rule.data("tstamp"),
                    __RequestVerificationToken: rules.find("input[name=__RequestVerificationToken]").val()
                })
                    .done((result) => {
                        cpiLoadingSpinner.hide();
                        this.refresh();
                    })
                    .fail((error) => {
                        cpiLoadingSpinner.hide();
                        this.refresh();
                    });
            });
        }
    }

    editRule = (e) => {
        const el = $(e.currentTarget);
        const rules = el.closest(".rules")
        const url = rules.data("editor-url");

        if (url) {
            //cpiLoadingSpinner.show();
            $.get(url, { id: el.closest(".rule").data("id") })
                .done((html) => {
                    //cpiLoadingSpinner.hide();

                    const page = $(this.searchResultContainer);
                    const modal = page.find("#mailModal");
                    const content = modal.find(".modal-content");

                    const initForm = () => {
                        const form = content.find("form.rules-editor");

                        //floating labels
                        form.floatLabels();

                        //show delete if there multiple conditions
                        toggleDeleteButton();

                        //enable/disbable download folder based on do not move checkbox
                        const doNotMove = form.find("input[name=DoNotMove]")
                        doNotMove.on("click", (e) => {
                            form.find("#DownloadFolderId").data("kendoDropDownList").enable(!doNotMove.is(':checked'));
                        });
                    }

                    const deleteCondition = (el) => {
                        el.closest(".condition").remove();
                        toggleDeleteButton();
                    };

                    const toggleDeleteButton = () => {
                        const conditions = content.find(".conditions .condition");

                        if (conditions.length == 1)
                            conditions.find(".delete-condition").hide();
                        else
                            conditions.find(".delete-condition").show();
                    }

                    //load
                    content.html(html);
                    modal.modal({ backdrop: 'static', keyboard: false });

                    //initialize form
                    initForm();

                    //delete condition
                    content.find(".delete-condition").on("click", (e) => {
                        deleteCondition(e.currentTarget);
                    });

                    //add condition
                    content.find(".add-condition").on("click", (e) => {
                        e.preventDefault();
                        const conditions = content.find(".conditions");
                        const template = conditions.find(".condition").last();
                        const options = template.find("input.condition-list").data("kendoDropDownList").options;

                        const clone = template.clone();
                        const value = clone.find("input.condition-value");
                        const list = clone.find("input.condition-list");
                        const index = parseInt(value.attr("name").split("-")[1]) + 1;

                        clone.data("id", "0");
                        clone.data("tstamp", "");
                        clone.data("index", index);

                        const valueName = `Value-${index}`;
                        const listName = `Condition-${index}`;

                        value.val("");
                        value.attr("name", valueName);

                        list.attr("id", listName).attr("name", listName);
                        list.kendoDropDownList(options);
                        list.data("kendoDropDownList").select(0);

                        const valueValidator = value.closest(".form-group").find("[data-valmsg-for]");
                        const listValidator = list.closest(".form-group").find("[data-valmsg-for]");

                        valueValidator.data("valmsg-for", valueName);
                        listValidator.data("valmsg-for", listName);

                        valueValidator.attr("data-valmsg-for", valueName);
                        listValidator.attr("data-valmsg-for", listName);

                        //if (valueValidator.hasClass("field-validation-error")) {
                        //    valueValidator.data("valmsg-replace", false);
                        //    valueValidator.attr("data-valmsg-replace", false);
                        //}
                        //if (listValidator.hasClass("field-validation-error")) {
                        //    listValidator.data("valmsg-replace", false);
                        //    listValidator.attr("data-valmsg-replace", false);
                        //}

                        //delete condition
                        clone.find(".delete-condition").on("click", (e) => {
                            deleteCondition(e.currentTarget);
                        });

                        clone.show();

                        conditions.append(clone);

                        //const form = $(e.currentTarget).closest("form.rules-editor");
                        //$.validator.unobtrusive.parse(form);
                        initForm();
                    });

                    //cancel
                    content.find(".btn.cancel").on("click", (e) => {
                        e.preventDefault();
                        cpiBreadCrumbs.markLastNode({ dirty: false });
                        this.refresh();
                    });

                    //save
                    content.find(".btn.save").on("click", (e) => {
                        e.preventDefault();
                        this.saveRule(e);
                    });

                    //mark page as dirty
                    cpiBreadCrumbs.markLastNode({ dirty: true });
                })
                .fail((error) => {
                    //cpiLoadingSpinner.hide();
                    this.refresh();
                });
        }
    }

    saveRule = (e) => {
        const el = $(e.currentTarget);
        const form = el.closest("form.rules-editor");
        const url = form.data("save-url");

        //$.validator.setDefaults({ ignore: '' }); //trigger dropdownlist validation
        $.validator.setDefaults({ ignore: ":hidden:not(span.k-dropdown:visible > input)" });
        $.validator.unobtrusive.parse(form);

        if (form.valid()) {
            //todo: submit
            //pageHelper.postData(url, form);

            let ruleConditions = [];
            form.find(".conditions .condition").each(function (index) {
                const condition = $(this);
                const i = condition.data("index");

                ruleConditions.push({
                    Id: condition.data("id"),
                    RuleId: form.data("id"),
                    Condition: condition.find(`input[name=Condition-${i}]`).val(),
                    Value: condition.find(`input[name=Value-${i}]`).val(),
                    tStamp: condition.data("tstamp")
                })
            })

            const stopProcessing = form.find("input[name=StopProcessing]");
            const doNotMove = form.find("input[name=DoNotMove]");
            const downloadAttachments = form.find("input[name=DownloadAttachments]");

            cpiLoadingSpinner.show();
            $.post(url, {
                mailbox: this.getMailboxName(),
                downloadRule: {
                    Id: form.data("id"),
                    Name: form.find("input[name=Name]").val(),
                    ActionId: form.find("input[name=ActionId]").val(),
                    StopProcessing: stopProcessing.length == 0 ? true : stopProcessing.is(':checked'),
                    DoNotMove: doNotMove.length == 0 ? true : doNotMove.is(':checked'),
                    DownloadFolderId: form.find("input[name=DownloadFolderId]").val(),
                    DownloadAttachments: downloadAttachments.length == 0 ? true : downloadAttachments.is(':checked'),
                    Responsibles: form.find("select[name=Responsibles]").data("kendoMultiSelect").value(),
                    tStamp: form.data("tstamp"),
                    RuleConditions: ruleConditions
                },
                __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val()
            })
                .done((result) => {
                    cpiLoadingSpinner.hide();

                    cpiBreadCrumbs.markLastNode({ dirty: false });
                    this.refresh();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    //this.refresh();
                });
        }
        else {
            form.wasValidated();
        }
    }

    cancelEdit = (id) => {
        //open fixed sidebar
        if (!this.sidebar.isFloating())
            this.sidebar.open();

        //show list
        this.showList();

        //show message
        if (id)
            this.getMessage(id);

        //remove dirty
        cpiBreadCrumbs.markLastNode({ dirty: false });

        //hide error message
        pageHelper.hideErrors();

    }

    sendMail = (id) => {
        const message = $(this.searchResultContainer).find(".mail .message");
        const url = message.data("send-url");

        if (url) {
            pageHelper.hideErrors();
            cpiLoadingSpinner.show();
            pageHelper.postData(url, message.find("form.editor"))
                .done(() => {
                    cpiLoadingSpinner.hide();
                    this.cancelEdit(id);
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(error);
                });
        }
    }

    resizeEditor = (e) => {
        let iFrame = $(".k-iframe.auto-fit")[0];

        //iFrame.width = iFrame.contentWindow.document.body.scrollWidth;
        iFrame.height = iFrame.contentWindow.document.body.scrollHeight;

        const form = $(this.searchResultContainer).find(".mail .content form.editor");
        const cc = form.find("#CcRecipients");
        const bcc = form.find("#BccRecipients");

        if (!cc.val()) {
            cc.closest(".col").hide();
            form.find(".more-recipients .more-cc").show();
        }

        if (!bcc.val()) {
            bcc.closest(".col").hide();
            form.find(".more-recipients .more-bcc").show();
        }

        let editorText = e.sender.value();
        editorText = editorText.replace(/href="(javascript.*?)"/, 'href=#');
        e.sender.value(editorText)
    }

    //drag and drop
    dragMail = (event) => {
        const el = $(event.target);
        const mail = $(this.searchResultContainer).find(".mail");
        const targetId = event.target.id;
        const dragImage = mail.find(".drag-image");
        const grid = this.searchResultGrid.data("kendoGrid");
        let messageIds = grid.selectedKeyNames();
        let fileNames = [];
        let hasAttachments = [];
        dragImage.empty();

        if (messageIds.includes(targetId)) {
            messageIds.forEach(id => {
                const row = this.searchResultGrid.find(`tr[id='${id}']`);
                fileNames.push(row.data("file-name"));

                if (row.data("has-attachments"))
                    hasAttachments.push(id);
            });
        } else {
            messageIds = [];
            messageIds.push(targetId);
            fileNames.push(el.data("file-name"));
            if (el.data("has-attachments"))
                hasAttachments.push(targetId);
        }

        if (messageIds.length == 1) {
            event.dataTransfer.setData("subject", el.data("subject"));
            event.dataTransfer.setData("sender", el.data("sender"));
        }

        //dragImage.html(`<div class="sender">${el.data("sender")}</div><div class="subject">${el.data("subject")}</div>`);
        dragImage.text(`${messageIds.length} Email${messageIds.length > 1 ? "s" : ""}`);
        event.dataTransfer.setData("messageIds", JSON.stringify(messageIds));
        event.dataTransfer.setData("fileNames", JSON.stringify(fileNames));
        event.dataTransfer.setData("targetId", targetId);
        event.dataTransfer.setData("mailbox", this.getMailboxName());
        event.dataTransfer.setData("hasAttachments", JSON.stringify(hasAttachments));

        event.dataTransfer.setDragImage(dragImage[0], -13, 5);
    }
    dropMail = (event) => {
        event.preventDefault();

        if (event.dataTransfer.dropEffect == "copy") {
            this.cpiLoadingSpinner.show();

            const targetId = event.target.id;
            const intervalId = setInterval(function () { checkDrop(); }, 1000);
            const checkDrop = () => {
                const status = localStorage.getItem(targetId);
                if (status) {
                    clearInterval(intervalId);
                    localStorage.removeItem(targetId);
                    this.cpiLoadingSpinner.hide();

                    if (status == "ok") {
                        this.clearDetails();
                        this.refreshUnreadItemCount();
                        this.refreshGrid();
                    }
                }
            };

            localStorage.setItem(targetId, "");
        }
    }

    viewDocument = (e) => {
        const document = $(e.currentTarget);
        const url = document.data("doc-url");

        if (url) {
            cpiLoadingSpinner.show();
            $.get(url)
                .done((result) => {
                    cpiLoadingSpinner.hide();
                    cpiAlert.open({ title: document.data("doc-title"), message: result, extraLargeModal: true });
                })
                .fail((e) => {
                    cpiLoadingSpinner.hide();
                    pageHelper.showErrors(e);
                });
        }
    }

    applyAction = (e) => {
        const el = $(e.currentTarget);
        const activeFolder = this.getActiveFolder();

        switch (el.data("action")) {
            case "delete-message":
                let confirm = "";
                if (activeFolder.closest(".parent-folder").find(".folder").hasClass("deleted-items"))
                    confirm = el.data("delete-confirm");

                if (confirm) {
                    cpiConfirm.delete(window.cpiBreadCrumbs.getTitle(), confirm, () => {
                        this.batchDeleteMessages();
                    });
                }
                else
                    this.batchDeleteMessages();

                break;
            case "mark-as-read":
                this.batchUpdateIsRead(true);
                break;
            case "mark-as-unread":
                this.batchUpdateIsRead(false);
                break;
            case "restore-message":
                this.batchRestoreMessages(false);
                break;
        }
    }

    batchUpdateIsRead = (isRead) => {
        const grid = this.searchResultGrid.data("kendoGrid");
        const ids = grid.selectedKeyNames();

        if (ids.length > 0) {
            const action = $(this.searchResultContainer).find(".mail .batch-actions");
            const verificationToken = this.getVerificationToken();
            const url = action.data("batch-update-isread-url");

            if (url) {
                cpiLoadingSpinner.show();
                $.post(url, { ids: ids, mailbox: this.getMailboxName(), isRead: isRead, __RequestVerificationToken: verificationToken })
                    .done(() => {
                        cpiLoadingSpinner.hide();

                        for (var i = 0; i < ids.length; i++) {
                            const row = this.searchResultGrid.find(`tr[id="${ids[i]}"]`);

                            if (isRead)
                                row.removeClass("unread");
                            else
                                row.addClass("unread");
                        }

                        //refresh grid if unread filter is checked
                        const refreshGrid = $(this.refineSearchContainer).find("#Unread").is(':checked');
                        
                        this.refreshUnreadItemCount(refreshGrid);
                        this.refreshMailCount();
                        this.refreshDetailsToolbar();
                    })
                    .fail((error) => {
                        cpiLoadingSpinner.hide();
                        pageHelper.showErrors(error);
                    });
            }
        }
    }

    batchDeleteMessages = () => {
        const grid = this.searchResultGrid.data("kendoGrid");
        const ids = grid.selectedKeyNames();

        if (ids.length > 0) {
            const action = $(this.searchResultContainer).find(".mail .batch-actions");
            const verificationToken = this.getVerificationToken();
            const url = action.data("batch-delete-messages-url");

            if (url) {
                cpiLoadingSpinner.show();
                $.post(url, { ids: ids, mailbox: this.getMailboxName(), __RequestVerificationToken: verificationToken })
                    .done(() => {
                        cpiLoadingSpinner.hide();
                        this.refreshUnreadItemCount();
                        this.refreshGrid();
                    })
                    .fail((error) => {
                        cpiLoadingSpinner.hide();
                        pageHelper.showErrors(error);
                    });
            }
        }
    }

    batchRestoreMessages = () => {
        const grid = this.searchResultGrid.data("kendoGrid");
        const ids = grid.selectedKeyNames();

        if (ids.length > 0) {
            const action = $(this.searchResultContainer).find(".mail .batch-actions");
            const verificationToken = this.getVerificationToken();
            const url = action.data("batch-restore-messages-url");

            if (url) {
                cpiLoadingSpinner.show();
                $.post(url, { ids: ids, mailbox: this.getMailboxName(), __RequestVerificationToken: verificationToken })
                    .done(() => {
                        cpiLoadingSpinner.hide();
                        this.refreshUnreadItemCount();
                        this.refreshGrid();
                    })
                    .fail((error) => {
                        cpiLoadingSpinner.hide();
                        pageHelper.showErrors(error);
                    });
            }
        }
    }

    refreshMailSidebar = () => {
        const mailNav = $(this.searchResultContainer).find(".mail-nav");
        const url = mailNav.data("get-mail-folders-url");

        if (url) {
            cpiLoadingSpinner.show();
            const data = { mailbox: this.getMailboxName() };
            const activeFolder = this.getActiveFolder();

            if (activeFolder)
                data.activeFolderId = activeFolder.data("folder");

            $.post(url, data)
                .done((html) => {
                    cpiLoadingSpinner.hide();
                    mailNav.html(html);

                    //folders
                    mailNav.find(".folders .folder").on("click", this.selectFolder);

                    //rules
                    mailNav.find(".settings.rules").on("click", this.showRules);
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                });
        }
    }

    folderMenuOnOpen = (e) => {
        const menu = e.sender;
        const folder = $(e.target);
        const isChildFolder = folder.closest("ul").hasClass("child-folders");

        menu.enable("#delete", isChildFolder);
        menu.enable("#rename", isChildFolder);
    }

    folderMenuOnSelect = (e) => {
        e.preventDefault();

        const menu = e.sender;
        const folder = $(e.target);
        const selected = $(e.item);
        const url = selected.find(".k-menu-link").attr("href");
        const data = { mailbox: this.getMailboxName(), __RequestVerificationToken: this.getVerificationToken() };

        const submit = (callback) => {
            cpiLoadingSpinner.show();

            $.post(url, data)
                .done(() => {
                    cpiLoadingSpinner.hide();
                    this.refreshMailSidebar();

                    if (typeof callback === "function")
                        callback();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    cpiAlert.warning(pageHelper.getErrorMessage(error));
                    this.refreshMailSidebar();

                    if (typeof callback === "function")
                        callback();
                });
        }

        const saveFolderName = (targetFolder) => {
            const folderName = targetFolder.find(".name");
            const folderIcon = targetFolder.find(".icon");
            const folderUnreadCount = targetFolder.find(".unread-count");
            const displayName = folderName.text();

            targetFolder.removeClass("folder"); //disable context menu
            folderIcon.hide();
            folderUnreadCount.hide();
            //use previously entered name
            //folderName.html(`<input class="form-control new-name edit" type='text' value='${folderName.data("new-name") == null ? folderName.text() : folderName.data("new-name")}'><button class="save-button edit">Save</button>`);
            folderName.html(`<input class="form-control new-name edit" type='text' value='${folderName.text()}'><button class="save-button edit">Save</button>`);

            setTimeout(function () {
                const newName = folderName.find(".new-name");

                newName.focus().select();
                newName.on("focusout", function (e) {
                    if ($(e.relatedTarget).hasClass("save-button") && $(this).val().trim() && displayName != $(this).val()) {
                        data.name = $(this).val();
                        submit();
                    }
                    else {
                        folderName.data("new-name", $(this).val());
                        folderName.text(displayName);

                        targetFolder.addClass("folder");
                        folderIcon.show();
                        folderUnreadCount.show();

                        folder.closest("li").find(".new-child-folders").remove();
                        folder.closest("li").find(".child-folders .new-child-folder").remove();
                    }
                });
            }, 100);
        }

        data.id = folder.data("folder");

        switch (selected.attr("Id")) {
            case "create":
                let childFolders = folder.closest("li").find(".child-folders").first();
                if (childFolders.length == 0) {
                    childFolders = $(`<ul class="new-child-folders nav nav-pills cpi-nav-vertical flex-column child-folders"></ul>`);
                    folder.closest("li").append(childFolders);
                }
                childFolders.prepend(`<li class="new-child-folder nav-item child-folder"><a href="#" class="new-folder nav-link"><span class="name"></span></a></li>`);

                saveFolderName(childFolders.find(".child-folder .new-folder"));
                break;

            case "rename":
                saveFolderName(folder);
                break;

            case "delete":
                const activeFolder = this.getActiveFolder();
                const parentFolder = folder.closest(".parent-folder").find(".folder");
                const confirm = parentFolder.hasClass("deleted-items") ? selected.data("confirm-delete") : selected.data("confirm-move");
                let callback = submit;

                //refresh grid if deleting current folder
                if (folder.data("folder") == activeFolder.data("folder"))
                    callback = () => { submit(this.refreshGrid); }

                data.parentId = parentFolder.data("folder");
                cpiConfirm.warning(window.cpiBreadCrumbs.getTitle(), confirm.replace("{0}", folder.find(".name").text()), callback)
                break;
        }
    }

    //enables drag over icon
    folderOnDragOver = (e) => {
        e.preventDefault();
        e.dataTransfer.dropEffect = "move";
    }

    folderOnDragStart = (e, el) => {
        const folder = $(el);

        e.dataTransfer.setData("folderId", folder.data("folder"));
        e.dataTransfer.setData("folderName", folder.find(".name").text());
        e.dataTransfer.setData("folderParentId", folder.closest(".child-folders").closest(".child-folder").find(".folder").first().data("folder"));
    }

    folderOnDrop = (e, el) => {
        const folder = $(el);
        const folderId = folder.data("folder");
        const folderName = folder.find(".name").text();
        const activeFolder = this.getActiveFolder();
        const folders = folder.closest(".folders");

        const dataMessageIds = e.dataTransfer.getData("messageIds");
        const dataFolderId = e.dataTransfer.getData("folderId");
        const dataFolderName = e.dataTransfer.getData("folderName");
        const dataFolderParentId = e.dataTransfer.getData("folderParentId");

        const data = { destinationId: folderId, mailbox: this.getMailboxName(), __RequestVerificationToken: this.getVerificationToken() };
        const submit = (callback) => {
            cpiLoadingSpinner.show();

            $.post(url, data)
                .done(() => {
                    cpiLoadingSpinner.hide();
                    this.refreshMailSidebar();

                    if (typeof callback === "function")
                        callback();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    cpiAlert.warning(pageHelper.getErrorMessage(error));
                    this.refreshMailSidebar();

                    if (typeof callback === "function")
                        callback();
                });
        }

        let url = "";
        let confirm = "";

        //move messages to folder
        if (dataMessageIds) {
            //ignore moving to same folder
            if (folderId == activeFolder.data("folder"))
                return;

            const messageIds = jQuery.parseJSON(dataMessageIds);
            url = folders.data("move-messages-url");
            confirm = folders.data("move-messages-confirm");
            data.ids = messageIds;

            cpiConfirm.warning(window.cpiBreadCrumbs.getTitle(), confirm.replace("{0}", messageIds.length).replace("{1}", activeFolder.find(".name").text()).replace("{2}", folderName), () => { submit(this.refreshGrid); });

        }
        //move child folder to folder
        else if (dataFolderId && dataFolderName) {
            //ignore moving to same folder or same parent
            if (folderId == dataFolderId || folderId == dataFolderParentId)
                return;

            url = folders.data("move-folder-url");
            confirm = folders.data("move-folder-confirm");
            data.id = dataFolderId;

            cpiConfirm.warning(window.cpiBreadCrumbs.getTitle(), confirm.replace("{0}", dataFolderName).replace("{1}", folderName), submit);
        }
    }

    initializeFolders = () => {
        const storedCollapsedFolders = localStorage.getItem("collapsedFolders");
        const toggleFolder = (el) => {
            const parentFolder = el.closest("li");
            const childFolders = parentFolder.find("> ul");
            const folderId = parentFolder.find("a.folder").data("folder");

            if (childFolders.is(":hidden")) {
                childFolders.show();
                el.removeClass("fa-angle-right");
                el.addClass("fa-angle-down");

                var index = this.collapsedFolders.indexOf(folderId);
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

            localStorage.setItem("collapsedFolders", JSON.stringify(this.collapsedFolders));
        };

        if (storedCollapsedFolders)
            this.collapsedFolders = JSON.parse(storedCollapsedFolders);

        $(".mail-nav .folder-toggle .toggle").on("click", function (e) {
            e.stopPropagation();
            toggleFolder($(e.currentTarget));
        })

        this.collapsedFolders.forEach(function (item, index) {
            if (item) {
                const collapsedFolder = $(`.mail-nav .folders .folder[data-folder="${item}"]`);

                if (collapsedFolder.length > 0)
                    toggleFolder(collapsedFolder.find(".folder-toggle .toggle"));
            }
        });
    }
}
