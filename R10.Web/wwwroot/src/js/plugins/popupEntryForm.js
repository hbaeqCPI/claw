//import cpiStatusMessage from "../statusMessage";
//import * as pageHelper from "../pageHelper";


(function ($) {
    if (typeof $.fn.cpiPopupEntryForm === "undefined") {
        $.fn.cpiPopupEntryForm = function(options) {
            var plugin = this;

            if (options.closeOnSubmit === undefined)
                options.closeOnSubmit = true;

            if (options.showSuccessMsg === undefined)
                options.showSuccessMsg = true;

            $.validator.unobtrusive.parse(plugin);
            if (plugin.data("validator") !== undefined) {
                plugin.data("validator").settings.ignore = ""; //include hidden fields (kendo controls)
            }
            pageHelper.addMaxLength(plugin);
            pageHelper.clearInvalidKendoDate(plugin);
            //pageHelper.focusLabelControl(plugin);

            plugin.on("submit",
                function(e) {
                    e.preventDefault();
                    if (options.beforeSubmit) {
                        options.beforeSubmit();
                    }
                    if (plugin.valid()) {
                        pageHelper.postData(plugin.attr("action"), plugin)
                            .done(function (response) {
                                let successMsg = "";
                                if (response.success)
                                    successMsg = response.success;
                                else {
                                    successMsg = plugin.data("save-message");
                                }
                                if (options.showSuccessMsg && successMsg)
                                    pageHelper.showSuccess(successMsg);

                                if (options.closeOnSubmit)
                                    options.dialogContainer.modal("hide");

                                if (options.afterSubmit)
                                    options.afterSubmit(response);
                            })
                            .fail(function (error) {
                                cpiLoadingSpinner.hide();
                                window.pageHelper.showErrors(error);
                            });

                    } else {
                        plugin.wasValidated();
                    }
                });
            return this;
        };
    }
})(jQuery);
