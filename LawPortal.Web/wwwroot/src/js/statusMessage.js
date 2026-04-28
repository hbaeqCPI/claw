//import $ from "jquery";

class CpiStatusMessage {

    constructor() {
        if (!CpiStatusMessage.instance) {
           // this.container = null;
            this.initialize();
            CpiStatusMessage.instance = this;
        }
        return CpiStatusMessage.instance;
    }

    initialize() {
        $(document).ready(function () {
           // this.container = $(".page-status.alert");

            $("[data-hide]").on("click", function () {
                $(this).closest("." + $(this).attr("data-hide")).slideUp();
            });
        });
    }

    success(message, delay) {
        $(".page-status.alert").removeClass("alert-danger");
        $(".page-status.alert").addClass("alert-success");
        $(".page-status.alert span.message").html(message);
        $(".page-status.alert").show(function () {
            if (delay)
                $(this).delay(delay).slideUp();
        });
    }
    error(message, delay) {
        $(".page-status.alert").removeClass("alert-success");
        $(".page-status.alert").addClass("alert-danger");
        $(".page-status.alert span.message").html(message);
        $(".page-status.alert").show(function () {
            if (delay)
                $(this).delay(delay).slideUp();
        });
    }
    hide() {
        $(".page-status.alert").clearQueue();
        $(".page-status.alert").slideUp();
    }
}

const instance = new CpiStatusMessage();
//Object.freeze(instance);

export default instance;
