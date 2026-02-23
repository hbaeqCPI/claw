import ActivePage from "../activePage";

export default class OwnerPage extends ActivePage {

    constructor() {
        super();
        this.contactLettersFilter = {};
        this.currentLetterEntryContact = 0;
        this.ownerContactLetterEditMode = false;
        this.lastSelectedContactRow = null;
        this.ownerContactLettersContainer = "ownerContactLetters";
        this.lastSelectedContact = null;
        this.lastSelectedLetter = null;

    }
    
    initialize(ownerId, pageId, entityType) {
        this.editableGrids = [
            {
                name: 'ownerContactsGrid', isDirty: false, filter: { ownerId: ownerId }, afterSubmit: this.updateRecordStamps
            },
            {
                name: 'ownerContactLettersGrid', isDirty: false, filter: this.getContactLettersFilter,
                afterSubmit: this.contacts_resetEditedFlag,
                onDirty: () => { this.ownerContactLetterEditMode = true; },
                onCancel: this.contacts_resetEditedFlag
            }
        ];

        $(document).ready(() => {
            const ownerContactsGrid = $("#ownerContactsGrid");

            ownerContactsGrid.on("click", ".contactLink", (e) => {
                e.stopPropagation();

                let url = $(e.target).data("url");
                const row = $(e.target).closest("tr");
                const dataItem = ownerContactsGrid.data("kendoGrid").dataItem(row);
                const linkUrl = url.replace("actualValue", dataItem.ContactID);
                pageHelper.openLink(linkUrl, false);
            });

            ownerContactsGrid.on("click", ".letter-entry", (e) => {
                e.stopPropagation();
             
                const element = $(e.target);
                const ownerId = element.data("owner-id");
                const contactId = element.data("contact-id");

                if (!this.ownerContactLetterEditMode) {
                    const parentRow = element.parents("tr.k-state-selected");
                    if (parentRow) {
                        parentRow.addClass("contact-selected");
                        this.lastSelectedContactRow = parentRow;
                    }

                    this.currentLetterEntryContact = contactId;
                    this.contacts_ShowLetterTypeEntry(ownerId, contactId);
                }

            });

            $("#ownerAddToClient").on("click", (e) => {
                e.preventDefault();
                const form = $("#" + this.detailContentContainer).find("form");
                const title = form.data("confirm-title");
                const msg = form.data("copy-owner-msg");
                const id = form.find("#OwnerID").val();

                cpiConfirm.confirm(title, msg, function () {
                    const baseUrl = $("body").data("base-url");
                    const url = `${baseUrl}/Shared/Owner/AddOwnerToClient`;

                    $.post(url, { id: id })
                        .done(function () {
                            $("#ownerAddToClient").addClass("d-none");
                            pageHelper.showSuccess(form.data("copy-success-msg"));
                        })
                        .fail(function (e) {
                            pageHelper.showErrors(e.responseText);
                        });
                });
            });
        });
    }

   
    valueMapper = (options, url) => {
        if (!url)
            url = $("body").data("base-url") + "/Shared/Owner/ValueMapper";

        $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }

    ownerCodeNameValueMapper = (options, url) => {
        if (!url)
            url = $("body").data("base-url") + "/Shared/Owner/OwnerCodeNameValueMapper";

        $.ajax({
            url: url,
            data: { value: options.value },
            success: function (data) {
                options.success(data);
            }
        });
    }

    deleteContactRow = (e, row) => {
        if (!this.ownerContactLetterEditMode) {
            this.deleteGridRow(e, row);
        }
    }

    contacts_ShowLetterTypeEntry = (ownerId, contactId) => {
        $(`#${this.ownerContactLettersContainer}`).show();
        this.contactLettersFilter = { EntityType: "O", EntityId: ownerId, ContactId: contactId };
        const ownerContactLettersGrid = $("#ownerContactLettersGrid").data("kendoGrid");
        ownerContactLettersGrid.dataSource.read();
    }

    contacts_onRowSelect = (row) => {
        if (!this.ownerContactLetterEditMode) {
            $(`#${this.ownerContactLettersContainer}`).hide();
            //const data = row.sender.dataItem(row.sender.select());
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

        if (this.ownerContactLetterEditMode) {
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
    }

    //hide letter settings if contact is deleted
    contacts_onRemove() {
        $("#ownerContactLetters").hide();
    }

    contacts_resetEditedFlag = () => {
        this.ownerContactLetterEditMode = false;
        this.currentLetterEntryContact = 0;
    }

    getContactLettersFilter = () => {
        return { contactLettersFilter: this.contactLettersFilter };
    }


    copyToClient() {
        const copyForm = $("#copyToClient");
        copyForm.unbind("submit"); //clear previously attached submit handler

        copyForm.on("submit", function (e) {
            e.preventDefault();
            const params = copyForm.serialize();
            $.post(copyForm.attr("action"), params)
                .done(function () {
                    $("#ownerCopyToClient").hide();
                })
                .fail(function (error) {
                    page.showErrors(error.responseText);
                });
        });
        $(copyForm).submit();
    }

}





