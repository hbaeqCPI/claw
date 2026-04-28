//import $ from "jquery";

class CpiAlert {

    constructor() {
        if (!CpiAlert.instance) {
            this.container = null;
            this.oldButton = null;
            this.defaultButton = null;
            this.dialog = null;

            this.initialize();
            CpiAlert.instance = this;
            window.cpiAlert = this;
        }
        return CpiAlert.instance;
    }

    initialize() {
        $(document).ready(() => {
            this.container = $("#cpiAlert");

            this.defaultButton = { "class": "btn-primary", "label": this.container.data("btn-close"), "icon": "fa fa-times" };

            this.dialog = {
                title: this.container.data("title"),
                button: this.defaultButton,
                open: function (message, onClose) {
                    this.alert(this.title, message, onClose, this.defaultButton);
                }
            };
        });
    }

    open(options) {
        let title = options.title;
        let message = options.message;
        let onClose = options.onClose;
        let button = options.button;
        let allowCancel = options.allowCancel;
        let largeModal = options.largeModal;
        let extraLargeModal = options.extraLargeModal;
        let noPadding = options.noPadding;

        title = title || this.dialog.title;
        message = message || "";
        button = button || this.defaultButton;
        allowCancel = allowCancel === undefined ? true : allowCancel;
        largeModal = largeModal === undefined ? false : largeModal;
        extraLargeModal = extraLargeModal === undefined ? false : extraLargeModal;

        if (noPadding)
            this.container.find(".modal-dialog").addClass("no-padding");
        else
            this.container.find(".modal-dialog").removeClass("no-padding");

        if (largeModal)
            this.container.find(".modal-dialog").addClass("modal-lg");
        else
            this.container.find(".modal-dialog").removeClass("modal-lg");

        if (extraLargeModal)
            this.container.find(".modal-dialog").addClass("modal-xl");
        else
            this.container.find(".modal-dialog").removeClass("modal-xl");

        if (this.oldButton) {
            this.container.find(".modal-close").removeClass(this.oldButton.class);
            this.container.find(".modal-close i").removeClass(this.oldButton.icon);
        }
        this.oldButton = button;

        this.container.find(".modal-title").html(title);
        this.container.find(".modal-message").html(message);

        this.container.find(".modal-close").addClass(button.class);
        this.container.find(".modal-close i").addClass(button.icon);
        this.container.find(".modal-close span").html(button.label);

        this.container.off("hide.bs.modal");
        if (onClose) {
            this.container.on("hide.bs.modal", function (e) {
                try {
                    onClose(e);
                }
                catch (err) {
                    console.error(err);

                    e.preventDefault();
                    e.stopImmediatePropagation();
                    return false;
                }
            });
        }

        this.container.off("keypress");
        if (allowCancel) {
            this.container.on("keypress", (e) => {
                if (e.which === 13) {
                    this.container.find(".modal-close").click();
                    e.preventDefault();
                }
                if (e.which === 27) {
                    e.preventDefault();
                }
            });

            this.container.data("keyboard", true);
            this.container.find(".close").show();
        }
        else {
            this.container.data("keyboard", false);
            this.container.find(".close").hide();
        }

        this.container.modal("show");
    }

    //DEPRECATED USE .open(options)
    //alert(title, message, onClose, button, allowCancel, largeModal) {
    //    button = button || { "class": "btn-primary", "label": this.container.data("btn-okay"), "icon": "fa fa-check" };
    //    this.open({ title: title, message: message, onClose: onClose, button: button, allowCancel: allowCancel, largeModal: largeModal });
    //}

    popUp(title, message, onClose, largeModal) {
        this.open({ title: title, message: message, onClose: onClose, largeModal: largeModal });
    }

    warning(message, onClose, title) {
        if (!title)
            title = window.cpiBreadCrumbs.getTitle() || document.getElementsByTagName("title")[0].innerHTML;

        let content = message;
        if (!content.includes("message-wrap"))
            content = `<div class="row message-wrap"><div class="col-2 text-center pl-md-4 pt-1"><i class="text-danger far fa-exclamation-triangle fa-2x"></i></div><div class="col-10"><p>${message}</p></div></div>`;

        this.open({ title: title, message: content, onClose: onClose });
    }

    success(title, message, onClose) {
        let content = message;
        if (!content.includes("message-wrap"))
            content = `<div class="row message-wrap"><div class="col-2 text-center pl-md-4 pt-1"><i class="text-success far fa-check-circle fa-2x"></i></div><div class="col-10"><p>${message}</p></div></div>`;

        this.open({ title: title, message: content, onClose: onClose });
    }

    close() {
        this.container.modal("hide");
    }
}

const instance = new CpiAlert();
//Object.freeze(instance);

export default instance;