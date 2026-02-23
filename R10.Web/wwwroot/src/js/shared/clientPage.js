import ActivePage from "../activePage";
import * as pageHelper from "../pageHelper";

export default class ClientPage extends ActivePage {

    constructor() {
        super();
        this.contactLettersFilter = {};
        this.currentLetterEntryContact = 0;
        this.clientContactLetterEditMode = false;
        this.lastSelectedContactRow = null;

        this.desigCtryFilter = {};
        this.clientDesigCtryEditMode = false;
        this.lastSelectedParentDesigCtryRow = null;
        this.desigCtryParent = null;

        this.clientContactLettersGrid = "clientContactLettersGrid";
        this.clientContactLettersContainer = "clientContactLetters";
        this.clientChildDesCountryGrid = "clientChildDesCountryGrid";
        this.clientChildDesCountryContainer = "clientChildDesCountry";

        this.lastSelectedContact = null;
        this.lastSelectedLetter = null;
    }

    valueMapper = (options, url) => {
        if (!url)
            url = $("body").data("base-url") + "/Shared/Client/ValueMapper";

        $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }

    initialize(clientId, pageId, entityType) {
        this.editableGrids = [
            {
                name: 'clientContactsGrid', isDirty: false, filter: { clientId: clientId },
                afterSubmit: this.updateRecordStamps
            },
            {
                name: 'clientContactLettersGrid', isDirty: false, filter: this.getContactLettersFilter,
                afterSubmit: this.contacts_resetEditedFlag,
                onDirty: () => { this.clientContactLetterEditMode = true; },
                onCancel: this.contacts_resetEditedFlag
            },
            {
                name: 'clientDesCountryGrid', isDirty: false, filter: { clientId: clientId },
                afterSubmit: this.updateRecordStamps
            },
            {
                name: 'clientChildDesCountryGrid', isDirty: false, filter: this.childDesig_getParam,
                afterSubmit: this.updateRecordStamps
            },
            {
                name: `reviewerSettingsGrid_${pageId}`, isDirty: false, filter: { parentId: clientId },
                afterSubmit: this.updateRecordStamps
            }
        ];

        $(document).ready(() => {
            const clientContactsGrid = $("#clientContactsGrid");

            clientContactsGrid.on("click", ".contactLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = clientContactsGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.ContactID);
                pageHelper.openLink(linkUrl, false);
            });

            clientContactsGrid.on("click", ".letter-entry", (e) => {
                e.stopPropagation();

                const element = $(e.target);
                const clientId = element.data("client-id");
                const contactId = element.data("contact-id");

                if (!this.clientContactLetterEditMode) {
                    const parentRow = element.parents("tr.k-state-selected");
                    if (parentRow) {
                        parentRow.addClass("contact-selected");
                        this.lastSelectedContactRow = parentRow;
                    }

                    this.currentLetterEntryContact = contactId;
                    this.contacts_ShowLetterTypeEntry(clientId, contactId);
                }
            });

            $("#clientDesCountryGrid").on("click", ".desig-entry", (e) => {
                e.stopPropagation();

                const element = $(e.target);
                const grid = $("#clientDesCountryGrid").data("kendoGrid");
                const currentRow = grid.dataItem(element.parents("tr").select());
                this.desigCtryParent = currentRow;

                if (!this.clientDesigCtryEditMode) {
                    const parentRow = element.parents("tr.k-state-selected");
                    if (parentRow) {
                        parentRow.addClass("parent-desig-selected");
                        this.lastSelectedParentDesigCtryRow = parentRow;
                    }
                    this.parentDesig_ShowDesigCtryTypeEntry();
                }

            });
            $("#clientAddToOwner").on("click", (e) => {
                e.preventDefault();
                const form = $("#" + this.detailContentContainer).find("form");
                const title = form.data("confirm-title");
                const msg = form.data("copy-owner-msg");
                const id = form.find("#ClientID").val();

                cpiConfirm.confirm(title, msg, function () {
                    const baseUrl = $("body").data("base-url");
                    const url = `${baseUrl}/Shared/Client/AddClientToOwner`;

                    $.post(url, { id: id })
                        .done(function () {
                            $("#clientAddToOwner").addClass("d-none");
                            pageHelper.showSuccess(form.data("copy-success-msg"));
                        })
                        .fail(function (e) {
                            pageHelper.showErrors(e.responseText);
                        });
                });
            });

        });
    }

    //designated countries
    deleteParentDesigRow = (e, row) => {
        if (!this.clientDesigCtryEditMode) {
            this.deleteGridRow(e, row);
        }
    }

    parentDesig_ShowDesigCtryTypeEntry = () => {
        $(`#${this.clientChildDesCountryContainer}`).show();
        this.desigCtryFilter = { parentId: this.desigCtryParent.EntityDesCtryID };
        const clientDesigCtrysGrid = $(`#${this.clientChildDesCountryGrid}`).data("kendoGrid");
        clientDesigCtrysGrid.dataSource.read();
    }

    parentDesig_getDesigCtryFilter = () => {
        return this.desigCtryFilter;
    }

    parentDesig_onRowSelect = (row) => {
        const currentRow = row.sender.dataItem(row.sender.select());
        this.desigCtryParent = currentRow;

        if (this.clientDesigCtryEditMode) {
            $(this.lastSelectedParentDesigCtryRow).addClass("k-state-selected");
        }
        $(`#${this.clientChildDesCountryContainer}`).hide();
    }

    parentDesig_onEdit = (e) => {
        if (this.clientDesigCtryEditMode) {
            e.sender.closeCell();
            e.preventDefault();
        }
    }

    parentDesig_resetEditedFlag = () => {
        this.clientDesigCtryEditMode = false;
    }

    //hide child designated country container if parent is deleted
    parentDesig_onRemove = () => {
        $(`#${this.clientChildDesCountryContainer}`).hide();
    }

    parentDesig_getSystemType = () => {
        return { systemType: this.desigCtryParent.SystemTypeName.substring(0, 1) };
    }

    parentDesig_getCaseTypeParam = () => {
        return {
            systemType: this.desigCtryParent.SystemTypeName.substring(0, 1),
            parentCountry: this.desigCtryParent.DesCtry
        };
    }

    parentDesig_getCountryParam = () => {
        return {
            parentId: this.desigCtryParent.EntityDesCtryID,
            systemType: this.desigCtryParent.SystemTypeName.substring(0, 1),
            parentCountry: this.desigCtryParent.DesCtry,
            parentCaseType: this.desigCtryParent.DesCaseType
        };
    }

    childDesig_onRowSelect = (e) => {
        const currentRow = e.sender.dataItem(e.sender.select());
        this.desigCtryChild = currentRow.DesCtry;
    }

    childDesig_getCaseTypeParam = () => {
        return {
            parentId: this.desigCtryParent.EntityDesCtryID,
            systemType: this.desigCtryParent.SystemTypeName.substring(0, 1),
            parentCountry: this.desigCtryParent.DesCtry,
            parentCaseType: this.desigCtryParent.DesCaseType,
            desCountry: this.desigCtryChild
        };
    }

    childDesig_getParam = () => {
        return {
            parentId: this.desigCtryParent.EntityDesCtryID,
            clientId: this.desigCtryParent.ClientID,
            systemType: this.desigCtryParent.SystemTypeName.substring(0, 1)
        };
    }

    getDefaultCaseType = (e) => {
        const desCaseType = e.dataItem.DesCaseType;

        const row = $(`#${e.sender.element[0].id}`).closest("tr");
        const grid = $("#clientChildDesCountryGrid").data("kendoGrid");
        const dataItem = grid.dataItem(row);
        dataItem.DesCaseType = desCaseType;
        $(row).find(".des-case-type").html(desCaseType);
    }

    //contacts
    deleteContactRow = (e, row) => {
        if (!this.clientContactLetterEditMode) {
            this.deleteGridRow(e, row);
        }
    }

    contacts_ShowLetterTypeEntry = (clientId, contactId) => {
        $(`#${this.clientContactLettersContainer}`).show();
        this.contactLettersFilter = { EntityType: "C", EntityId: clientId, ContactId: contactId };
        const clientContactLettersGrid = $(`#${this.clientContactLettersGrid}`).data("kendoGrid");
        clientContactLettersGrid.dataSource.read();
    }

    contacts_onRowSelect = (row) => {
        if (!this.clientContactLetterEditMode) {
            //const data = row.sender.dataItem(row.sender.select());
            $(`#${this.clientContactLettersContainer}`).hide();
            //if (data.GenAllLetters !== 2) {
            //}
        }
        else {
            $(this.lastSelectedContactRow).addClass("k-state-selected");
        }
    }

    contacts_updateName = (property, value, id) => {
        this.lastSelectedContact[property] = value;
        this.lastSelectedContact.dirty = true;
        if (id > 0)
            this.lastSelectedContact.ContactID = id;
    }

    contacts_onEdit = (e) => {
        const selected = e.sender.select();
        if (selected && selected.length > 0)
            this.lastSelectedContact = e.sender.dataItem(selected);
        else
            this.lastSelectedContact = e.sender.dataSource._data[0];

        if (this.clientContactLetterEditMode) {
            e.sender.closeCell();
            e.preventDefault();
        }
    }

    letters_onEdit = (e) => {
        const selected = e.sender.select();
        if (selected && selected.length > 0)
            this.lastSelectedLetter = e.sender.dataItem(selected);
        else
            this.lastSelectedLetter = e.sender.dataSource._data[0];
    }

    letters_updateName = (property, value) => {
        this.lastSelectedLetter[property] = value;
        //this.lastSelectedContact.dirty = true;
    }

    //hide letter settings if contact is deleted
    contacts_onRemove = () => {
        $(`#${this.clientContactLettersContainer}`).hide();
    }

    contacts_resetEditedFlag = () => {
        this.clientContactLetterEditMode = false;
        this.currentLetterEntryContact = 0;
    }

    getContactLettersFilter = () => {
        return { contactLettersFilter: this.contactLettersFilter };
    }

    showEntityDataSync() {
        const dialog = $("#entityDataSyncDialog");
        dialog.modal("show");

        dialog.find("#sync").on("click", function () {
            const grid = $("#entitySyncGrid").data("kendoGrid");
            let selected = [];

            const affectsAll = dialog.find("#affectsAll").prop("checked");
            if (affectsAll) {
                selected = grid.dataSource.data().map(item => item.id);
            }
            else
                selected = grid.selectedKeyNames();

            const form = dialog.find("form")[0];
            const url = $(form).attr("action");

            $.post(url, { ids: selected })
                .done(function () {
                    dialog.modal("hide");
                })
                .fail(function (e) {
                    pageHelper.showErrors(e.responseText);
                });
        });


        dialog.find("input[name='Affects']").on("change", function () {
            const copyButton = dialog.find("#sync");
            const affects = $(this).val();

            if (affects === "A")
                copyButton.removeAttr("disabled");
            else {
                const grid = $("#entitySyncGrid").data("kendoGrid");
                if (grid.selectedKeyNames().length > 0)
                    copyButton.removeAttr("disabled");
                else
                    copyButton.attr("disabled", "disabled");
            }
        });

        dialog.find("#EntitySearch").keyup(pageHelper.setDelay(() => {
            const grid = $("#entitySyncGrid").data("kendoGrid");
            grid.dataSource.read();

        }, 500)); //trigger the search only when the user stops typing for no. of ms

    }

    entitySyncSearchGetParam() {
        const dialog = $("#entityDataSyncDialog");
        return {
            searchText: dialog.find("#EntitySearch").val()
        }
    }



    entitySyncSelectionChange(parent) {
        const copyButton = $(`#${parent}`).find("#sync");
        const grid = $("#entitySyncGrid").data("kendoGrid");
        if (grid.selectedKeyNames().length > 0)
            copyButton.removeAttr("disabled");
        else
            copyButton.attr("disabled", "disabled");
    }

    copyToOwner() {
        const copyForm = $("#copyToOwner");
        copyForm.unbind("submit"); //clear previously attached submit handler

        copyForm.on("submit", function (e) {
            e.preventDefault();
            const params = copyForm.serialize();
            $.post(copyForm.attr("action"), params)
                .done(function () {
                    $("#clientCopyToOwner").hide();
                })
                .fail(function (error) {
                    pageHelper.showErrors(error.responseText);
                });
        });
        $(copyForm).submit();
    }
    
}





