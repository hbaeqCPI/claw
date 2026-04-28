import ActivePage from "../activePage";

export default class UserSetupPage extends ActivePage {

    constructor() {
        super();
    }

    init(pageId) {
        pageHelper.moveBreadcrumbs(pageId);
    }

    initializeSidebarPage(sidebarPage) {
        //filter toggle button
        //$(".kendo-Grid .k-grid-toolbar").addClass("sidebar-link show");
        $(".kendo-Grid .k-grid-toolbar").addClass("show");
        super.initializeSidebarPage(sidebarPage);
    }

    initializeDetailPage(detailPage) {
        super.initializeDetailPage(detailPage);
        const form = $(`#${this.mainDetailContainer}`).find("form");
        //custom buttons
        cpiConfirm.reactivateButtons = {
            "action": { "class": "btn-primary", "label": form.data("label-reactivate"), "icon": "fa fa-check" },
            "close": { "class": "btn-secondary", "label": form.data("label-cancel"), "icon": "fa fa-undo-alt" }
        };
        cpiConfirm.unlockButtons = {
            "action": { "class": "btn-primary", "label": form.data("label-unlock"), "icon": "fa fa-unlock" },
            "close": { "class": "btn-secondary", "label": form.data("label-cancel"), "icon": "fa fa-undo-alt" }
        };
    }

    initializeDetailContentPage(detailContentPage) {
        const detailContentContainer = $(`#${this.detailContentContainer}`);

        //override after save callback
        if (detailContentPage.addMode)
            detailContentPage.addModeOptions.afterSubmit = (result) => {
                this.afterInsert(result.id).then(() => {
                    if (result.openPopup)
                        detailContentContainer.find(`.cpi-popup.${result.openPopup}`).click();
                });
            };
        else
            detailContentPage.editModeOptions.afterSubmit = (result) => {
                this.showDetails(result.id).then(function () {
                    if (result.openPopup)
                        detailContentContainer.find(`.cpi-popup.${result.openPopup}`).click();
                });
            };

        super.initializeDetailContentPage(detailContentPage);
        const self = this;

        detailContentContainer.find(".btn-toggle-pwd").on("click", function (e, show) { self.togglePassword($(e.currentTarget), show); });
        detailContentContainer.find(".btn-gen-pwd").on("click", function (e) { self.generatePassword($(this)); });
        detailContentContainer.find(".valid-date-range input[data-role='datepicker']").on("blur", function (e) { self.checkValidPeriod($(this)); });
        detailContentContainer.find(".pwd-option").on("click", function (e) { self.checkPasswordOption($(this)); });

        detailContentContainer.find(".cpi-confirm").on("click", function (e) { self.openConfirm($(this)); });
        detailContentContainer.find(".cpi-popup").on("click", function (e) { self.openPopup($(this)); });

        const popUp = detailContentContainer.find(".cpi-popup.show");
        if (popUp) {
            popUp.click();
            popUp.removeClass("show");
        }
    }

    //-- detail content methods --//
    checkValidPeriod(el) {
        if (window.kendo.parseDate(el.val(), ['dd-MMM-yyyy']) === null) {
            el.val("");
            return;
        }

        const form = el.closest("form");
        const fromDate = window.kendo.parseDate(form.find("input[name='ValidDateFrom']").val(), ['dd-MMM-yyyy']);
        const toDate = window.kendo.parseDate(form.find("input[name='ValidDateTo']").val(), ['dd-MMM-yyyy']);

        if (fromDate !== null && toDate !== null && fromDate > toDate) {
            el.val("");
        }
    }

    checkPasswordOption(el) {
        const form = el.closest("form");
        const requireChange = form.find("#RequireChangePassword");
        const neverExpires = form.find("#PasswordNeverExpires");
        const cannotChange = form.find("#CannotChangePassword");
        const checked = el.prop("checked");

        switch (el.attr("id")) {
            case "RequireChangePassword":
                if (cannotChange && checked)
                    cannotChange.prop("checked", false);

                break;
            case "PasswordNeverExpires":
                if (cannotChange && checked == false)
                    cannotChange.prop("checked", false);

                break;
            case "CannotChangePassword":
                if (requireChange && checked)
                    requireChange.prop("checked", false);

                if (neverExpires && checked)
                    neverExpires.prop("checked", true);

                break;
        }
    }

    togglePassword(el, show) { 
        const form = el.closest("form");
        const pwd = form.find(".gen-pwd");
        const toggleIcon = form.find(".toggle-pwd");

        if (show || pwd.attr('type') === "password") {
            pwd.attr('type', 'text');
            toggleIcon.removeClass("fa-eye-slash")
            toggleIcon.addClass("fa-eye")
        } else {
            pwd.attr('type', 'password');
            toggleIcon.addClass("fa-eye-slash")
            toggleIcon.removeClass("fa-eye")
        }
    }

    generatePassword(el) {
        const form = el.closest("form");
        const spinner = el.find(".gen-pwd-spinner");
        const togglePwd = form.find(".btn-toggle-pwd");

        spinner.show();
        
        $.post(el.data("url"))
            .done(function (response) {
                spinner.hide();

                const pwd = form.find(".gen-pwd");                                
                pwd.val(response.password);
                pwd.blur();

                togglePwd.trigger("click", [true]);
            })
            .fail((error) => {
                spinner.hide();
                pageHelper.showErrors(error);
            });
    }

    //-- account settings methods --//
    saveSetting(el) {
        const form = el.closest("form");
        const url = form.data("save-setting-url");
        const settingName = el.closest(".user-settings").data("group");
        const value = el.is(':checked');
        const setting = `{ ${el.data("setting")}: ${el.is(':checked')} }`;
        
        if (url) {
            const userId = form.find("#UserId").val();
            const status = form.find(".modal-status");

            cpiLoadingSpinner.show();
            status.slideUp();
            $.post(url, { userId: userId, settingName: settingName, setting: setting, __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val() })
                .done(function (response) {
                    cpiLoadingSpinner.hide();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    status.find(".message").html(pageHelper.getErrorMessage(error));
                    status.slideDown();
                    el.prop( "checked", !value );
                });
        }
    };

    saveDocumentStorageAccountType(e) {
        const el = $(e.sender.element);
        const form = el.closest("form");
        const url = form.data("save-setting-url");

        if (url) {
            const settingName = el.closest(".user-settings").data("group");
            const setting = `{ ${el.data("setting")}: ${e.sender.value() } }`;
            const userId = form.find("#UserId").val();
            const status = form.find(".modal-status");

            cpiLoadingSpinner.show();
            status.slideUp();

            $.post(url, { userId: userId, settingName: settingName, setting: setting, __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val() })
                .done(function (response) {
                    cpiLoadingSpinner.hide();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    status.find(".message").html(pageHelper.getErrorMessage(error));
                    status.slideDown();
                });
        }
    }

    linkEntity(e) {
        const el = $(e.sender.element);
        const form = el.closest("form");
        const group = el.closest(".link-entity");
        const url = form.data("link-entity-url");

        if (url) {            
            const entityId = e.sender.value();
            const entityType = group.data("entity-type");
            const userId = form.find("#UserId").val();
            const validator = group.find(".field-validation-error");

            cpiLoadingSpinner.show();
            validator.hide();

            $.post(url, { userId: userId, entityId: entityId, entityType: entityType,  __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val() })
                .done(function (response) {
                    cpiLoadingSpinner.hide();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    validator.html(pageHelper.getErrorMessage(error));
                    validator.show();
                });
        }
    }

    saveReviewerRole(el) {     
        const form = el.closest("form");
        const url = el.data("save-reviewer-url");
        const value = el.is(':checked');
        
        if (url) {
            const userId = form.find("#UserId").val();
            const status = form.find(".modal-status");

            cpiLoadingSpinner.show();
            status.slideUp();
            $.post(url, { userId: userId, isReviewer: value, __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val() })
                .done(function (response) {
                    cpiLoadingSpinner.hide();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    status.find(".message").html(pageHelper.getErrorMessage(error));
                    status.slideDown();
                    el.prop( "checked", !value );
                });
        }
    }

    savePreviewerRole(el) {
        const form = el.closest("form");
        const url = el.data("save-previewer-url");
        const value = el.is(':checked');

        if (url) {
            const userId = form.find("#UserId").val();
            const status = form.find(".modal-status");

            cpiLoadingSpinner.show();
            status.slideUp();
            $.post(url, { userId: userId, isPreviewer: value, __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val() })
                .done(function (response) {
                    cpiLoadingSpinner.hide();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    status.find(".message").html(pageHelper.getErrorMessage(error));
                    status.slideDown();
                    el.prop("checked", !value);
                });
        }
    }

    saveDecisionMakerRole(el) { 
        const form = el.closest("form");
        const url = form.data("save-decision-maker-url");
        const value = el.is(':checked');
        const systemId = el.data("system");
        
        if (url) {
            const userId = form.find("#UserId").val();
            const status = form.find(".modal-status");

            cpiLoadingSpinner.show();
            status.slideUp();
            $.post(url, { userId: userId, isDecisionMaker: value, systemId: systemId, __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val() })
                .done(function (response) {
                    cpiLoadingSpinner.hide();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    status.find(".message").html(pageHelper.getErrorMessage(error));
                    status.slideDown();
                    el.prop( "checked", !value );
                });
        }
    }

    saveAttorneyRole = (el) => {
        const form = el.closest("form");
        const url = form.data("save-attorney-url");
        const value = el.is(':checked');
        const systemId = el.data("system");
        
        if (url) {
            const userId = form.find("#UserId").val();
            const status = form.find(".modal-status");

            cpiLoadingSpinner.show();
            status.slideUp();
            $.post(url, { userId: userId, canAccess: value, systemId: systemId, __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val() })
                .done((response) => {
                    cpiLoadingSpinner.hide();
                    this.toggleUploadRole(value, systemId);
                    this.togglePatentScoreRole(value);
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    status.find(".message").html(pageHelper.getErrorMessage(error));
                    status.slideDown();
                    el.prop( "checked", !value );
                });
        }
    }

    saveSoftDocketRole = (el) => {
        const form = el.closest("form");
        const url = form.data("save-soft-docket-url");
        const value = el.is(':checked');
        const systemId = el.data("system");

        if (url) {
            const userId = form.find("#UserId").val();
            const status = form.find(".modal-status");

            cpiLoadingSpinner.show();
            status.slideUp();
            $.post(url, { userId: userId, canModify: value, systemId: systemId, __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val() })
                .done((response) => {
                    cpiLoadingSpinner.hide();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    status.find(".message").html(pageHelper.getErrorMessage(error));
                    status.slideDown();
                    el.prop("checked", !value);
                });
        }
    }

    saveRequestDocketRole = (el) => {
        const form = el.closest("form");
        const url = form.data("save-request-docket-url");
        const value = el.is(':checked');
        const systemId = el.data("system");

        if (url) {
            const userId = form.find("#UserId").val();
            const status = form.find(".modal-status");

            cpiLoadingSpinner.show();
            status.slideUp();
            $.post(url, { userId: userId, canModify: value, systemId: systemId, __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val() })
                .done((response) => {
                    cpiLoadingSpinner.hide();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    status.find(".message").html(pageHelper.getErrorMessage(error));
                    status.slideDown();
                    el.prop("checked", !value);
                });
        }
    }

    saveUploadRole(el) {
        const form = el.closest("form");
        const url = form.data("save-upload-url");
        const value = el.is(':checked');
        const systemId = el.data("system");
        
        if (url) {
            const userId = form.find("#UserId").val();
            const status = form.find(".modal-status");

            cpiLoadingSpinner.show();
            status.slideUp();
            $.post(url, { userId: userId, canUpload: value, systemId: systemId, __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val() })
                .done(function (response) {
                    cpiLoadingSpinner.hide();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    status.find(".message").html(pageHelper.getErrorMessage(error));
                    status.slideDown();
                    el.prop( "checked", !value );
                });
        }
    }

    saveInventorRole = (el) => {
        const form = el.closest("form");
        const url = form.data("save-inventor-url");
        const value = el.is(':checked');
        const systemId = el.data("system");

        if (url) {
            const userId = form.find("#UserId").val();
            const status = form.find(".modal-status");

            cpiLoadingSpinner.show();
            status.slideUp();
            $.post(url, { userId: userId, canAccess: value, systemId: systemId, __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val() })
                .done((response) => {
                    cpiLoadingSpinner.hide();
                    this.toggleUploadRole(value, systemId);
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    status.find(".message").html(pageHelper.getErrorMessage(error));
                    status.slideDown();
                    el.prop( "checked", !value );
                });
        }
    }

    savePatentScoreRole(el) {
        const form = el.closest("form");
        const url = form.data("save-patent-score-url");
        const value = el.is(':checked');
        const systemId = el.data("system");

        if (url) {
            const userId = form.find("#UserId").val();
            const status = form.find(".modal-status");

            cpiLoadingSpinner.show();
            status.slideUp();
            $.post(url, { userId: userId, canModify: value, systemId: systemId, __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val() })
                .done(function (response) {
                    cpiLoadingSpinner.hide();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    status.find(".message").html(pageHelper.getErrorMessage(error));
                    status.slideDown();
                    el.prop("checked", !value);
                });
        }
    }

    saveMailsRole = (el) => {
        const form = el.closest("form");
        const url = form.data("save-mails-url");
        const value = el.is(':checked');
        const systemId = el.data("system");

        if (url) {
            const userId = form.find("#UserId").val();
            const status = form.find(".modal-status");

            cpiLoadingSpinner.show();
            status.slideUp();
            $.post(url, { userId: userId, canAccess: value, systemId: systemId, __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val() })
                .done((response) => {
                    cpiLoadingSpinner.hide();
                    this.toggleUploadRole(value, systemId);
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    status.find(".message").html(pageHelper.getErrorMessage(error));
                    status.slideDown();
                    el.prop("checked", !value);
                });
        }
    }

    toggleUploadRole(canAccess, systemId) {
        const uploadLabel = $(`.upload.${systemId.toLowerCase()} .toggle-label`);
        const uploadSwitch = $(`.upload.${systemId.toLowerCase()} .toggle-option`);

        if (canAccess) {
            uploadLabel.removeClass("text-muted");
            uploadSwitch.prop("disabled", false);
        } else {
            uploadLabel.addClass("text-muted");
            uploadSwitch.prop("disabled", true);
            uploadSwitch.prop("checked", false);
        }
    }

    togglePatentScoreRole(canAccess) {
        const patScoreLabel = $(`.patent-score .toggle-label`);
        const patScoreSwitch = $(`.patent-score .toggle-option`);

        if (canAccess) {
            patScoreLabel.removeClass("text-muted");
            patScoreSwitch.prop("disabled", false);
        } else {
            patScoreLabel.addClass("text-muted");
            patScoreSwitch.prop("disabled", true);
            patScoreSwitch.prop("checked", false);
        }
    }

    
    saveRole(e, systemId, urlDataName) {
        const el = $(e.sender.element);
        const form = el.closest("form");
        const url = form.data(urlDataName);

        if (url) {            
            const roleId = e.sender.value();
            const userId = form.find("#UserId").val();
            const status = form.find(".modal-status");

            cpiLoadingSpinner.show();
            status.slideUp();

            $.post(url, { userId: userId, systemId: systemId, roleId: roleId,  __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val() })
                .done(function (response) {
                    cpiLoadingSpinner.hide();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    status.find(".message").html(pageHelper.getErrorMessage(error));
                    status.slideDown();
                });
        }
    }

    saveAuxiliaryRole(e, systemId) {
        this.saveRole(e, systemId, "save-auxiliary-url");
    }

    saveCountryLawRole(e, systemId) {
        this.saveRole(e, systemId, "save-country-law-url");
    }

    saveActionTypeRole(e, systemId) {
        this.saveRole(e, systemId, "save-action-type-url");
    }

    saveLetterRole(e, systemId) {
        this.saveRole(e, systemId, "save-letter-url");
    }

    saveCustomQueryRole(e, systemId) {
        this.saveRole(e, systemId, "save-custom-query-url");
    }

    saveProductsRole(e, systemId) {
        this.saveRole(e, systemId, "save-products-url");
    }

    saveCostEstimatorRole(e, systemId) {
        this.saveRole(e, systemId, "save-cost-estimator-url");
    }

    saveGermanRemunerationRole(e, systemId) {
        this.saveRole(e, systemId, "save-german-remuneration-url");
    }

    saveFrenchRemunerationRole(e, systemId) {
        this.saveRole(e, systemId, "save-french-remuneration-url");
    }

    saveDocumentVerificationRole(e, systemId) {
        this.saveRole(e, systemId, "save-document-verification-url");
    }

    saveWorkflowRole(e, systemId) {
        this.saveRole(e, systemId, "save-workflow-url");
    }

    saveDashboardAccess = (el) => {
        const form = el.closest("form");
        const url = form.data("save-dashboard-access-url");
        const value = el.is(':checked');
        const systemId = el.data("system");

        if (url) {
            const userId = form.find("#UserId").val();
            const status = form.find(".modal-status");

            cpiLoadingSpinner.show();
            status.slideUp();
            $.post(url, { userId: userId, canAccess: value, systemId: systemId, __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val() })
                .done((response) => {
                    cpiLoadingSpinner.hide();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    status.find(".message").html(pageHelper.getErrorMessage(error));
                    status.slideDown();
                    el.prop("checked", !value);
                });
        }
    }

    saveMailboxAccess = (el) => {
        const form = el.closest("form");
        const url = form.data("save-mailbox-access-url");
        const value = el.is(':checked');
        const mailbox = el.data("mailbox");

        if (url) {
            const userId = form.find("#UserId").val();
            const status = form.find(".modal-status");

            cpiLoadingSpinner.show();
            status.slideUp();
            $.post(url, { userId: userId, canAccess: value, mailbox: mailbox, __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val() })
                .done((response) => {
                    cpiLoadingSpinner.hide();
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    status.find(".message").html(pageHelper.getErrorMessage(error));
                    status.slideDown();
                    el.prop("checked", !value);
                });
        }
    }

    //-- assign roles methods --//
    gridUserSystemRolesOnError(e) {
        if (e.errors) {
            const grid = $("#gridUserSystemRoles").data("kendoGrid");

            grid.one("dataBinding", function (args) {
                args.preventDefault();
                
                const errors = [];
                for (const error in e.errors) {
                    let name = "";
                    let message = "";

                    switch (error) {
                        case "System.Id":
                            name = "System";
                            message = "The System field is required.";
                            break;
                        case "SystemId":
                            name = "System";
                            message = e.errors[error].errors[0];
                            break;
                        case "Role.Id":
                            name = "Role";
                            message = "The Role field is required.";
                            break;
                        case "RoleId":
                            name = "Role";
                            message = e.errors[error].errors[0];
                            break;
                        case "RespOffice":
                            name = "RespOffice";
                            message = e.errors[error].errors[0];
                            break;
                    }

                    if (name)
                        //showMessage(grid.editable.element, name, message);
                        grid.editable.element.find("[data-valmsg-for=" + name + "],[data-val-msg-for=" + name + "]")
                            .replaceWith(validationMessageTmpl({ field: name, message: message }))
                    else
                        errors.push(e.errors[error].errors[0]);
                }

                if (errors.length > 0) {   
                    const form = $(args.sender.element).closest("form");
                    const status = form.find(".modal-status");
                    if (status.length > 0) {                            
                        status.find(".message").html(errors.join("<br>"));
                        status.slideDown();
                    }
                }
            });
        }
    }

    //-- entity filter methods --//
    getEntityListFilter(e) {
        const form = $("#SetEntityFilter");
        const entityFilterType = form.find("#entityFilterType").data("kendoDropDownList").value();
        const userId = form.find("#Id").val();
        const search = form.find("#entitySearch").val();

        return { entityFilterType: entityFilterType, entity: search, userId: userId };
    }

    getUserEntityListFilter() {
        const form = $("#SetEntityFilter");
        const entityFilterType = form.find("#entityFilterType").data("kendoDropDownList").value();
        const userId = form.find("#Id").val();

        return { entityFilterType: entityFilterType, userId: userId };
    }

    refreshEntityLists(form) {      
        form.find("#listEntityFilter").data("kendoListBox").dataSource.read(userSetupPage.getEntityListFilter());
        form.find("#listUserEntityFilter").data("kendoListBox").dataSource.read(userSetupPage.getUserEntityListFilter());
    }

    entityFilterTypeOnSelect(e) {
        const list = this;
        const selectedValue = list.dataItem(e.item.index()).Value;
        const el = $(e.sender.element);
        const form = el.closest("form");
        const userEntityFilterList = form.find("#listUserEntityFilter").data("kendoListBox");
        const title = form.closest(".modal").find(".modal-title").text() || window.cpiBreadCrumbs.getTitle();
        
        if (this.value() != selectedValue && userEntityFilterList.items().length > 0) {
            cpiConfirm.warning(title, form.data("confim-change-message"), function () {
                list.value(selectedValue);
                list.trigger("change");
            });

            e.preventDefault();
        }
    }

    entityFilterTypeOnChange = (e) => {
        const entityFilterType = e.sender.value();
        const el = $(e.sender.element);
        const form = el.closest("form");
        const url = form.data("update-url");

        if (url) {
            cpiLoadingSpinner.show();
            $.post(url, { userId: form.find("#Id").val(), entityFilterType: entityFilterType, __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val() })
                .done((response) => {
                    cpiLoadingSpinner.hide();
                    this.showStatus(form, response.message);
                    this.refreshEntityLists(form);
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    this.showStatus(form, pageHelper.getErrorMessage(error), true);
                    this.refreshEntityLists(form);
                });
        }
    }

    userEntityListOnRemove = (e) => {
        const el = $(e.sender.element);
        const form = el.closest("form");
        const url = form.data("remove-url");
        const selectedValues = e.dataItems.map(function (_, x) {
            return e.dataItems[x].Id;
        });
        
        if (url) {
            cpiLoadingSpinner.show();
            $.post(url, { userId: form.find("#Id").val(), selectedItems: selectedValues, __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val() })
                .done((response) => {
                    cpiLoadingSpinner.hide();
                    this.showStatus(form, response.message);
                    //this.refreshEntityLists(form);
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    this.showStatus(form, pageHelper.getErrorMessage(error), true);
                    this.refreshEntityLists(form);
                });
        }
    }

    userEntityListOnAdd = (e) => {
        const el = $(e.sender.element);
        const form = el.closest("form");
        const url = form.data("add-url");
        const selectedValues = e.dataItems.map(function (_, x) {
            return e.dataItems[x].Id;
        });
        
        if (url) {
            cpiLoadingSpinner.show();
            $.post(url, { userId: form.find("#Id").val(), selectedItems: selectedValues, __RequestVerificationToken: form.find("input[name=__RequestVerificationToken]").val() })
                .done((response) => {
                    cpiLoadingSpinner.hide();
                    this.showStatus(form, response.message);
                    //this.refreshEntityLists(form);
                })
                .fail((error) => {
                    cpiLoadingSpinner.hide();
                    this.showStatus(form, pageHelper.getErrorMessage(error), true);
                    this.refreshEntityLists(form);
                });
        }
    }

    showStatus(form, message, isError) {
        const status = form.find(".modal-status");

        if (status.length > 0) {
            if (isError) {
                status.addClass("alert-danger");
                status.removeClass("alert-success");
            } else  {
                status.removeClass("alert-danger");
                status.addClass("alert-success");
            }

            status.find(".message").html(message);
            status.slideDown();
        }
    }
    
    onEntityFilterClose = (el) => { 
        const form = $("#SetEntityFilter");
        const userEntityFilterList = form.find("#listUserEntityFilter").data("kendoListBox");
        const entityFilterType = form.find("#entityFilterType").data("kendoDropDownList").value();

        if (userEntityFilterList.items().length == 0 && entityFilterType != 0) {
            const message = form.data("warning-close-message");
            this.showStatus(form, message, true);

            throw message;
        }
        
        pageHelper.showDetails(userSetupPage, el.data("get-id"));
    }
}