//import cpiStatusMessage from "./statusMessage";
//import cpiLoadingSpinner from "./loadingSpinner";
//import cpiConfirm from "./confirm";
//import cpiAlert from "./alert";

export default class BasePage {

    constructor() {
        this.initializeBreadCrumbs();
        this.cpiStatusMessage = window.cpiStatusMessage;
        this.cpiLoadingSpinner = window.cpiLoadingSpinner;
        this.cpiConfirm = window.cpiConfirm;
        this.cpiAlert = window.cpiAlert;
        this.pageHelper = window.pageHelper;

    }

    initializeBreadCrumbs() {
        $(document).ready(function() {
            if (typeof (window.cpiBreadCrumbs) === "undefined") {
                const container = ".container-crumbs";
                window.cpiBreadCrumbs = $(container).cpiBreadCrumbs();

                $(window).on('beforeunload', function () {
                    let activeCopy = false;

                    $(document).find("input[name=CopyOptions]").each(function () {
                        if (this.value)
                            activeCopy = true;
                    });
                    if (activeCopy)
                       return "dirty";
                    else {
                        if (window.cpiBreadCrumbs.hasDirtyNode()) {
                            const dirtyPagePrompt = window.cpiBreadCrumbs.data("dirty-page-msg");
                            return dirtyPagePrompt;
                        }
                    }
                    
                });   
            }
        });
    }
}