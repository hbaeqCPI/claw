//import $ from "jquery";

class CpiPrintConfirm {

    constructor() {
        if (!CpiPrintConfirm.instance) {
            this.container = null;
            this.oldButtons = null;
            this.defaultButtons = null;
            this.dialog = null;

            this.initialize();
            CpiPrintConfirm.instance = this;
            window.cpiPrintConfirm = this;
        }
        return CpiPrintConfirm.instance;
    }

    initialize() {
        $(document).ready(() => {
            this.container = $("#cpiPrintConfirm");
            this.defaultButtons = {
                "action": { "class": "btn-primary", "label": this.container.data("btn-yes"), "icon": "fa fa-check" },
                "close": { "class": "btn-secondary", "label": this.container.data("btn-no"), "icon": "fa fa-times" }
            };

            this.dialog = {
                title: this.container.data("title"),
                buttons: this.defaultButtons,
                onOpen: null,
                open: function (message, onConfirm) {
                    confirm(this.title, message, onConfirm, this.defaultButtons);
                }
            };
        });
    }

    confirm(title, message, onConfirm, buttons, largeModal, onCancel) {
        let cancel = true;

        title = title || this.dialog.title;
        message = message || "";
        buttons = buttons || this.defaultButtons;
        largeModal = largeModal === undefined ? false : largeModal;

        if (largeModal)
            this.container.find(".modal-dialog").addClass("modal-lg");
        else
            this.container.find(".modal-dialog").removeClass("modal-lg");

        if (this.oldButtons) {
            this.container.find(".modal-action").removeClass(this.oldButtons.action.class);
            this.container.find(".modal-action i").removeClass(this.oldButtons.action.icon);
            this.container.find(".modal-close").removeClass(this.oldButtons.close.class);
            this.container.find(".modal-close i").removeClass(this.oldButtons.close.icon);
        }
        this.oldButtons = buttons;

        this.container.find(".modal-title").html(title);
        this.container.find(".modal-message").html(message);

        this.container.find(".modal-action").addClass(buttons.action.class);
        this.container.find(".modal-action i").addClass(buttons.action.icon);
        this.container.find(".modal-action span").html(buttons.action.label);

        this.container.find(".modal-close").addClass(buttons.close.class);
        this.container.find(".modal-close i").addClass(buttons.close.icon);
        this.container.find(".modal-close span").html(buttons.close.label);

        this.container.find(".modal-close").off("keypress");
        this.container.find(".modal-close").on("keypress", function (e) {
            if (e.which === 13) {
                this.container.find(".modal-action").click();
                e.preventDefault();
            }
        });

        this.container.find(".modal-action").off("click");
        this.container.find(".modal-action").on("click", () => {
            cancel = false;

            if (onConfirm) {
                try {
                    onConfirm();
                    this.container.modal("hide");
                }
                catch (err) {
                    cancel = true;
                    console.error(err);
                }
            }
        });

        this.container.off("hidden.bs.modal");
        this.container.on("hidden.bs.modal", () => {
            this.dialog.onOpen = null;

            if (cancel && onCancel) {
                try {
                    onCancel();
                }
                catch (err) {
                    console.error(err);
                }
            }
        });

        this.container.off("shown.bs.modal");
        this.container.on("shown.bs.modal", () => {
            if (this.dialog.onOpen === null) {
                let input = $(".modal-message").find("*").filter(":input:visible:first");
                if (input.length > 0) {
                    input.focus();
                }
                else {
                    const kendoWidget = $(".modal-message").find(".k-widget")[0];
                    if ($(kendoWidget).hasClass("k-dropdown")) {
                        input = $(kendoWidget).find("input")[0];
                        const dropdown = $(input).data("kendoDropDownList");
                        dropdown.focus();
                    }
                }
            }
            else {
                this.dialog.onOpen();
            }
        });

        this.container.modal("show");
    }

    confirm(title, message, onConfirm, buttons, largeModal, onCancel) {
        let cancel = true;

        title = title || this.dialog.title;
        message = message || "";
        buttons = buttons || this.defaultButtons;
        largeModal = largeModal === undefined ? false : largeModal;

        if (largeModal)
            this.container.find(".modal-dialog").addClass("modal-lg");
        else
            this.container.find(".modal-dialog").removeClass("modal-lg");

        if (this.oldButtons) {
            this.container.find(".modal-action").removeClass(this.oldButtons.action.class);
            this.container.find(".modal-action i").removeClass(this.oldButtons.action.icon);
            this.container.find(".modal-close").removeClass(this.oldButtons.close.class);
            this.container.find(".modal-close i").removeClass(this.oldButtons.close.icon);
        }
        this.oldButtons = buttons;

        this.container.find(".modal-title").html(title);
        this.container.find(".modal-message").html(message);

        this.container.find(".modal-action").addClass(buttons.action.class);
        this.container.find(".modal-action i").addClass(buttons.action.icon);
        this.container.find(".modal-action span").html(buttons.action.label);

        this.container.find(".modal-close").addClass(buttons.close.class);
        this.container.find(".modal-close i").addClass(buttons.close.icon);
        this.container.find(".modal-close span").html(buttons.close.label);

        this.container.find(".modal-close").off("keypress");
        this.container.find(".modal-close").on("keypress", function (e) {
            if (e.which === 13) {
                this.container.find(".modal-action").click();
                e.preventDefault();
            }
        });

        this.container.find(".modal-action").off("click");
        this.container.find(".modal-action").on("click", () => {
            cancel = false;

            if (onConfirm) {
                try {
                    onConfirm();
                    this.container.modal("hide");
                }
                catch (err) {
                    cancel = true;
                    console.error(err);
                }
            }
        });

        this.container.off("hidden.bs.modal");
        this.container.on("hidden.bs.modal", () => {
            this.dialog.onOpen = null;

            if (cancel && onCancel) {
                try {
                    onCancel();
                }
                catch (err) {
                    console.error(err);
                }
            }
        });

        this.container.off("shown.bs.modal");
        this.container.on("shown.bs.modal", () => {
            if (this.dialog.onOpen === null) {
                let input = $(".modal-message").find("*").filter(":input:visible:first");
                if (input.length > 0) {
                    input.focus();
                }
                else {
                    const kendoWidget = $(".modal-message").find(".k-widget")[0];
                    if ($(kendoWidget).hasClass("k-dropdown")) {
                        input = $(kendoWidget).find("input")[0];
                        const dropdown = $(input).data("kendoDropDownList");
                        dropdown.focus();
                    }
                }
            }
            else {
                this.dialog.onOpen();
            }
        });

        this.container.modal("show");
    }
    delete(title, message, onConfirm) {
        let content = message;
        if (!content.includes("message-wrap"))
            content = `<div class="row message-wrap"><div class="col-2 text-center pl-md-4 pt-1"><i class="text-danger far fa-exclamation-triangle fa-2x"></i></div><div class="col-10"><p>${message}</p></div></div>`;

        this.confirm(title, content, onConfirm,
            {
                "action": { "class": "btn-danger", "label": this.container.data("btn-delete"), "icon": "fa fa-trash-alt" },
                "close": { "class": "btn-secondary", "label": this.container.data("btn-cancel"), "icon": "fa fa-undo-alt" }
            });
    }
    print(title, message, onConfirm) {
        let content = message;
        if (!content.includes("message-wrap"))
            content = `<div class="message-wrap"><div class="col-12 text-center pl-md-4 pt-1"><p>${message}</p></div></div>`;

        this.confirm(title, content, onConfirm,
            {
                "action": { "class": "btn-primary", "label": this.container.data("btn-print"), "icon": "fa fa-file-arrow-down" },
                "close": { "class": "btn-secondary", "label": this.container.data("btn-cancel"), "icon": "fa fa-undo-alt" }
            });
    }
    save(title, message, onConfirm, largeModal, onCancel) {
        this.confirm(title, message, onConfirm,
            {
                "action": { "class": "btn-primary", "label": this.container.data("btn-save"), "icon": "fa fa-save" },
                "close": { "class": "btn-secondary", "label": this.container.data("btn-cancel"), "icon": "fa fa-undo-alt" }
            }, largeModal, onCancel);
    }
}

const instance = new CpiPrintConfirm();
//Object.freeze(instance);

export default instance;