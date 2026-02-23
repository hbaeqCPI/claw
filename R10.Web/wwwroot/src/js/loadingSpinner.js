//import $ from "jquery";

class CpiLoadingSpinner {
    constructor() {
        if (!CpiLoadingSpinner.instance) {
           // this.container = null;
            this.message = "";
            this.timer = null;

            CpiLoadingSpinner.instance = this;
        }
        return CpiLoadingSpinner.instance;
    }

    
    show(message, delay) {
        message = message || this.message;
        delay = delay || 500;
        $("#cpiLoadingSpinner .modal-body span").html(message);

        clearTimeout(this.timer);
        this.timer = setTimeout(function () {
            $("#cpiLoadingSpinner").modal({
                backdrop: 'static',
                keyboard: false
            });
            $(".modal-backdrop").addClass("loading-spinner");
        }, delay);
    }

    hide() {
        clearTimeout(this.timer);
        $("#cpiLoadingSpinner").modal("hide");
        $(".modal-backdrop").removeClass("loading-spinner");
    }

    setMessage(message) {
        this.message = message;
    }
}

const instance = new CpiLoadingSpinner();
//Object.freeze(instance);

export default instance;